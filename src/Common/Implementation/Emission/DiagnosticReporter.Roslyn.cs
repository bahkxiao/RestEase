using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal partial class DiagnosticReporter
    {
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        private static readonly DiagnosticDescriptor headerMustNotHaveColonInName = CreateDescriptor(
            DiagnosticCode.HeaderMustNotHaveColonInName,
            "Header attributes must not have colons in their names",
            "Header attribute name '{0}' must not contain a colon");
        public void ReportHeaderOnInterfaceMustNotHaveColonInName(TypeModel typeModel, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(header, typeModel.NamedTypeSymbol),
                header.Attribute!.Name);
        }

        public void ReportPropertyHeaderMustNotHaveColonInName(PropertyModel property)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(property.HeaderAttribute, property.PropertySymbol),
                property.HeaderAttribute!.Attribute.Name);
        }

        public void ReportHeaderOnMethodMustNotHaveColonInName(MethodModel method, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(header, method.MethodSymbol),
                header.Attribute!.Name);
        }

        public void ReportHeaderParameterMustNotHaveColonInName(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(parameter.HeaderAttribute, parameter.ParameterSymbol),
                parameter.HeaderAttribute!.Attribute.Name);
        }

        private static readonly DiagnosticDescriptor headerOnInterfaceMustHaveValue = CreateDescriptor(
            DiagnosticCode.HeaderOnInterfaceMustHaveValue,
            "Header attributes on the interface must have a value",
            "Header on interface must have a value (i.e. be of the form [Header(\"{0}\", \"Value Here\")])");
        public void ReportHeaderOnInterfaceMustHaveValue(TypeModel typeModel, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(headerOnInterfaceMustHaveValue, AttributeLocations(header, typeModel.NamedTypeSymbol), header.Attribute.Name);
        }

        private static readonly DiagnosticDescriptor allowAnyStatusCodeAttributeNotAllowedOnParentInterface = CreateDescriptor(
            DiagnosticCode.AllowAnyStatusCodeAttributeNotAllowedOnParentInterface,
            "Parent interfaces may not have any [AllowAnyStatusCode] attributes",
            "Parent interface (of type '{0}') may not have an [AllowAnyStatusCode] attribute");
        public void ReportAllowAnyStatusCodeAttributeNotAllowedOnParentInterface(TypeModel typeModel, AllowAnyStatusCodeAttributeModel attribute)
        {
            this.AddDiagnostic(
                allowAnyStatusCodeAttributeNotAllowedOnParentInterface,
                AttributeLocations(attribute, attribute.DefinedOn),
                typeModel.NamedTypeSymbol.Name);
        }

        private static readonly DiagnosticDescriptor eventsNotAllowed = CreateDescriptor(
            DiagnosticCode.EventsNotAllowed,
            "Interfaces must not have any events",
            "Interface must not have any events");
        public void ReportEventNotAllowed(EventModel eventModel)
        {
            this.AddDiagnostic(eventsNotAllowed, eventModel.EventSymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustHaveOneAttribute = CreateDescriptor(
            DiagnosticCode.PropertyMustHaveOneAttribute,
            "Properties must have exactly one attribute",
            "Property must have exactly one attribute");
        public void ReportPropertyMustHaveOneAttribute(PropertyModel property)
        {
            this.AddDiagnostic(propertyMustHaveOneAttribute, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustBeReadOnly = CreateDescriptor(
            DiagnosticCode.PropertyMustBeReadOnly,
            "Property must be read-only",
            "Property must have a getter but not a setter");
        public void ReportPropertyMustBeReadOnly(PropertyModel property)
        {
            // We don't want to include the headers in the squiggle
            this.AddDiagnostic(propertyMustBeReadOnly, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustBeReadWrite = CreateDescriptor(
            DiagnosticCode.PropertyMustBeReadWrite,
            "Property must be read/write",
            "Property must have a getter and a setter");
        public void ReportPropertyMustBeReadWrite(PropertyModel property)
        {
            // We don't want to include the headers in the squiggle
            this.AddDiagnostic(propertyMustBeReadWrite, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor requesterPropertyMustHaveZeroAttributes = CreateDescriptor(
            DiagnosticCode.RequesterPropertyMustHaveZeroAttributes,
            "IRequester properties must not have any attributes",
            "IRequester property must not have any attribtues");
        public void ReportRequesterPropertyMustHaveZeroAttributes(PropertyModel propertyModel, List<AttributeModel> attributes)
        {
            this.AddDiagnostic(
                requesterPropertyMustHaveZeroAttributes,
                AttributeLocations(attributes, propertyModel.PropertySymbol));
        }

        private static readonly DiagnosticDescriptor multipleRequesterProperties = CreateDescriptor(
            DiagnosticCode.MultipleRequesterProperties,
            "Only a single IRequester property is allowed",
            "There must not be more than one property of type IRequester");
        public void ReportMultipleRequesterProperties(PropertyModel property)
        {
            this.AddDiagnostic(multipleRequesterProperties, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor missingPathPropertyForBasePathPlaceholder = CreateDescriptor(
            DiagnosticCode.MissingPathPropertyForBasePathPlaceholder,
            "BasePath placeholders must have corresponding path properties",
            "Unable to find a [Path(\"{0}\")] property for the path placeholder '{{{0}}}' in base path '{1}'");
        public void ReportMissingPathPropertyForBasePathPlaceholder(TypeModel typeModel, AttributeModel<BasePathAttribute> attributeModel, string basePath, string missingParam)
        {
            this.AddDiagnostic(missingPathPropertyForBasePathPlaceholder, AttributeLocations(attributeModel, typeModel.NamedTypeSymbol), missingParam, basePath);
        }

        private static readonly DiagnosticDescriptor methodMustHaveRequestAttribute = CreateDescriptor(
            DiagnosticCode.MethodMustHaveRequestAttribute,
            "All methods must have a suitable [Get] / [Post] / etc attribute",
            "Method does not have a suitable [Get] / [Post] / etc attribute");
        public void ReportMethodMustHaveRequestAttribute(MethodModel method)
        {
            this.AddDiagnostic(methodMustHaveRequestAttribute, method.MethodSymbol.Locations);
        }

        private static readonly DiagnosticDescriptor multiplePathPropertiesForKey = CreateDescriptor(
            DiagnosticCode.MultiplePathPropertiesForKey,
            "There must not be multiple path properties for the same key",
            "Multiple path properties for the key '{0}' are not allowed");
        public void ReportMultiplePathPropertiesForKey(string key, IEnumerable<PropertyModel> properties)
        {
            this.AddDiagnostic(multiplePathPropertiesForKey, properties.SelectMany(x => SymbolLocations(x.PropertySymbol)), key);
        }

        private static readonly DiagnosticDescriptor multiplePathParametersForKey = CreateDescriptor(
            DiagnosticCode.MultiplePathParametersForKey,
            "There must not be multiple path parameters for the same key",
            "Multiple path parameters for the key '{0}' are not allowed");
        public void ReportMultiplePathParametersForKey(MethodModel _, string key, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multiplePathParametersForKey, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)), key);
        }

        private static readonly DiagnosticDescriptor headerPropertyWithValueMustBeNullable = CreateDescriptor(
            DiagnosticCode.HeaderPropertyWithValueMustBeNullable,
            "Property header which has a default value must be of a type which is nullable",
            "[Header(\"{0}\", \"{1}\")] on property (i.e. containing a default value) can only be used if the property type is nullable");
        public void ReportHeaderPropertyWithValueMustBeNullable(PropertyModel property)
        {
            var attribute = property.HeaderAttribute!.Attribute;
            this.AddDiagnostic(
                headerPropertyWithValueMustBeNullable,
                AttributeLocations(property.HeaderAttribute, property.PropertySymbol),
                attribute.Name, attribute.Value);
        }

        private static readonly DiagnosticDescriptor missingPathPropertyOrParameterForPlaceholder = CreateDescriptor(
            DiagnosticCode.MissingPathPropertyOrParameterForPlaceholder,
            "All placeholders in a path must have a corresponding path property or path parameter",
            "No placeholder {{{0}}} for path parameter '{0}'");
        public void ReportMissingPathPropertyOrParameterForPlaceholder(MethodModel method, string placeholder)
        {
            // We'll put the squiggle on the attribute itself
            this.AddDiagnostic(missingPathPropertyOrParameterForPlaceholder, AttributeLocations(method.RequestAttribute, method.MethodSymbol), placeholder);
        }

        private static readonly DiagnosticDescriptor missingPlaceholderForPathParameter = CreateDescriptor(
            DiagnosticCode.MissingPlaceholderForPathParameter,
            "All path parameters on a method must have a corresponding placeholder",
            "No placeholder {{{0}}} for path parameter '{0}'");
        public void ReportMissingPlaceholderForPathParameter(MethodModel _, string placeholder, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(missingPlaceholderForPathParameter, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)), placeholder);
        }

        private static readonly DiagnosticDescriptor multipleHttpRequestMessagePropertiesForKey = CreateDescriptor(
            DiagnosticCode.MultipleHttpRequestMessagePropertiesForKey,
            "Multiple properties must not have the same HttpRequestMessageProperty key",
            "Multiple properties found for HttpRequestMessageProperty key '{0}'");
        public void ReportMultipleHttpRequestMessagePropertiesForKey(string key, IEnumerable<PropertyModel> properties)
        {
            this.AddDiagnostic(
                multipleHttpRequestMessagePropertiesForKey,
                properties.SelectMany(x => SymbolLocations(x.PropertySymbol)),
                key);
        }

        private static readonly DiagnosticDescriptor httpRequestMessageParamDuplicatesPropertyForKey = CreateDescriptor(
            DiagnosticCode.HttpRequestMessageParamDuplicatesPropertyForKey,
            "Method parameters must not have the same HttpRequsetMessageProperty key as a property",
            "Method parameter has the same HttpRequestMessageProperty key '{0}' as property '{1}'");
        public void ReportHttpRequestMessageParamDuplicatesPropertyForKey(MethodModel _, string key, PropertyModel property, ParameterModel parameter)
        {
            this.AddDiagnostic(
                httpRequestMessageParamDuplicatesPropertyForKey,
                SymbolLocations(parameter.ParameterSymbol),
                key, property.Name);
        }

        private static readonly DiagnosticDescriptor multipleHttpRequestMessageParametersForKey = CreateDescriptor(
            DiagnosticCode.MultipleHttpRequestMessageParametersForKey,
            "Method parameters must not have the same HttpRequestMessageProperty key",
            "Multiple parameters found for HttpRequestMessageProperty key '{0}'");
        public void ReportMultipleHttpRequestMessageParametersForKey(MethodModel _, string key, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(
                multipleHttpRequestMessageParametersForKey,
                parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)),
                key);
        }

        public void ReportParameterMustHaveZeroOrOneAttributes(MethodModel method, ParameterModel parameter, List<AttributeModel> _)
        {
            throw new NotImplementedException();
        }

        private static readonly DiagnosticDescriptor multipleCancellationTokenParameters = CreateDescriptor(
            DiagnosticCode.MultipleCancellationTokenParameters,
            "Methods must not have multiple CancellationToken parameters",
            "Multiple CancellationTokens are not allowed");

        public void ReportMultipleCancellationTokenParameters(MethodModel _, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multipleCancellationTokenParameters, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)));
        }

        public void ReportCancellationTokenMustHaveZeroAttributes(MethodModel method, ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        private static readonly DiagnosticDescriptor multipleBodyParameters = CreateDescriptor(
            DiagnosticCode.MultipleBodyParameters,
            "There must not be multiple body parameters",
            "Found more than one parameter with a [Body] attribute");
        public void ReportMultipleBodyParameters(MethodModel _, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multipleBodyParameters, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)));
        }

        private static readonly DiagnosticDescriptor queryMapParameterIsNotADictionary = CreateDescriptor(
            DiagnosticCode.QueryMapParameterIsNotADictionary,
            "QueryMap parameters must be dictionaries",
            "[QueryMap] parameter is not of the type IDictionary or IDictionary<TKey, TValue> (or their descendents)");
        public void ReportQueryMapParameterIsNotADictionary(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(queryMapParameterIsNotADictionary, SymbolLocations(parameter.ParameterSymbol));
        }

        private static readonly DiagnosticDescriptor headerParameterMustNotHaveValue = CreateDescriptor(
            DiagnosticCode.HeaderParameterMustNotHaveValue,
            "Header attributes on parameters must not have values",
            "Header attribute must have the form [Header(\"{0}\")], not [Header(\"{0}\", \"{1}\")]");
        public void ReportHeaderParameterMustNotHaveValue(MethodModel method, ParameterModel parameter)
        {
            var attribute = parameter.HeaderAttribute!.Attribute;
            this.AddDiagnostic(
                headerParameterMustNotHaveValue,
                AttributeLocations(parameter.HeaderAttribute, method.MethodSymbol),
                attribute.Name, attribute.Value);
        }

        private static readonly DiagnosticDescriptor methodMustHaveValidReturnType = CreateDescriptor(
            DiagnosticCode.MethodMustHaveValidReturnType,
            "Methods must have a return type of Task or Task<T>",
            "Method must have a return type of Task or Task<T>");
        public void ReportMethodMustHaveValidReturnType(MethodModel method)
        {
            this.AddDiagnostic(methodMustHaveValidReturnType, method.MethodSymbol.Locations);
        }

        // -----------------------------------------------------------
        // Not shared with the Emit DiagnosticReporter

        public void ReportCouldNotFindRestEaseType(string metadataName)
        {
            throw new NotImplementedException();
        }

        public void ReportCouldNotFindSystemType(string metadataName)
        {
            throw new NotImplementedException();
        }

        public void ReportExpressionsNotAvailable()
        {
            // This is only a warning, saying MethodInfo will be null
            throw new NotImplementedException();
        }

        private static DiagnosticDescriptor CreateDescriptor(DiagnosticCode code, string title, string messageFormat) =>
            new DiagnosticDescriptor(code.Format(), title, messageFormat, "RestEaseGeneration", DiagnosticSeverity.Error, isEnabledByDefault: true);

        private void AddDiagnostic(DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object?[] args)
        {
            var locationsList = (locations as IReadOnlyList<Location>) ?? locations.ToList();
            this.Diagnostics.Add(Diagnostic.Create(descriptor, locationsList.Count == 0 ? Location.None : locationsList[0], locationsList.Skip(1), args));
        }

        private void AddDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[] args)
        {
            this.Diagnostics.Add(Diagnostic.Create(descriptor, location ?? Location.None, args));
        }

        // Try and get the location of the whole 'Foo foo', and not just 'foo'
        private static IEnumerable<Location> SymbolLocations(ISymbol symbol)
        {
            var declaringReferences = symbol.DeclaringSyntaxReferences;
            return declaringReferences.Length > 0
                ? declaringReferences.Select(x => x.GetSyntax().GetLocation())
                : symbol.Locations;
        }

        private static IEnumerable<Location> AttributeLocations(AttributeModel? attributeModel, ISymbol fallback)
        {
            // TODO: This squiggles the 'BasePath(...)' bit. Ideally we'd want '[BasePath(...)]' or perhaps just '...'.
            var attributeLocation = attributeModel?.AttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
            return attributeLocation != null ? new[] { attributeLocation } : SymbolLocations(fallback);
        }

        private static IEnumerable<Location> AttributeLocations(IEnumerable<AttributeModel> attributeModels, ISymbol fallback)
        {
            var results = new List<Location>();
            bool anyFailed = false;
            foreach (var attributeModel in attributeModels)
            {
                var location = attributeModel?.AttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                if (location != null)
                {
                    results.Add(location);
                }
                else
                {
                    anyFailed = true;
                    break;
                }
            }

            return anyFailed || results.Count == 0 ? SymbolLocations(fallback) : results;
        }

    }
}