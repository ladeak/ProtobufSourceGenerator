using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProtobufSourceGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer : DiagnosticAnalyzer
    {
        private const string Category = "ProtoSourceGeneration";
        private static DiagnosticDescriptor Rule01 = new DiagnosticDescriptor("Proto01", "Type must be partial with ProtoContract attribute", "Type must be partial with ProtoContract attribute", Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Type must be partial with ProtoContract attribute.");
        private static DiagnosticDescriptor Rule02 = new DiagnosticDescriptor("Proto02", "Nested type's parent must be partial type", "Nested type's parent must be partial type", Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Nested type's parent must be partial type.");
        private static DiagnosticDescriptor Rule03 = new DiagnosticDescriptor("Proto03", "Consider attributing property with ProtoMember", "Consider attributing property with ProtoMember", Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: "This property is not considered for ProtoBuf source generation. Consider manually marking the type with ProtoIgnore or ProtoMember attributes.");
        private static DiagnosticDescriptor Rule04 = new DiagnosticDescriptor("Proto04", "Consider attributing base type with ProtoContract, ProtoInclude as a partial class", "Consider attributing base type with ProtoContract, ProtoInclude", Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: "Consider attributing base type with ProtoContract, ProtoInclude and amend to a partial class.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule01, Rule02, Rule03, Rule04); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            context.RegisterSymbolAction(AnalyzeTypeHierarchy, SymbolKind.NamedType);
        }

        private void AnalyzeTypeHierarchy(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedType)
                return;

            ValidateBaseTypes(context, namedType);
            ValidateNestedTypes(context, namedType);
        }

        private void ValidateBaseTypes(SymbolAnalysisContext context, INamedTypeSymbol namedType)
        {
            if (!IsPartial(namedType) || !HasProtoContractAttribute(namedType))
                return;

            var typeSymbol = namedType.BaseType;
            while (typeSymbol.SpecialType != SpecialType.System_Object
                && typeSymbol.SpecialType != SpecialType.System_Enum
                && typeSymbol.SpecialType != SpecialType.System_ValueType)
            {
                if (!HasProtoContractAttribute(typeSymbol) || !HasProtoIncludeAttribute(typeSymbol, namedType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule04, context.Symbol.Locations.First(), string.Empty));
                    return;
                }
                typeSymbol = typeSymbol.BaseType;
            }
        }

        private void ValidateNestedTypes(SymbolAnalysisContext context, INamedTypeSymbol namedType)
        {
            var typeSymbol = namedType.ContainingType;
            while (typeSymbol != null)
            {
                if (!IsPartial(typeSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule02, context.Symbol.Locations.First(), string.Empty));
                    return;
                }
                typeSymbol = typeSymbol.ContainingType;
            }
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IPropertySymbol propertySymbol)
                return;

            if (context.Symbol.ContainingSymbol is not INamedTypeSymbol containingType
                || !(HasProtoContractAttribute(containingType) && IsPartial(containingType)))
                return;

            if (!PropertyAttributeParser.CanGenerateAutoProperty(propertySymbol))
            {
                if (!PropertyAttributeParser.HasProtoProperties(propertySymbol, out _))
                    context.ReportDiagnostic(Diagnostic.Create(Rule03, context.Symbol.Locations.First(), string.Empty));
                return;
            }

            if (propertySymbol.Type.TypeKind == TypeKind.Enum)
                return;

            if (propertySymbol.Type is not INamedTypeSymbol namedType || namedType.SpecialType != SpecialType.None)
                return;
            bool hasProtoContract = HasProtoContractAttribute(namedType);
            bool isPartial = IsPartial(namedType);

            if (hasProtoContract && isPartial)
                return;

            var diagnostic = Diagnostic.Create(Rule01, context.Symbol.Locations.First(), string.Empty);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool HasProtoContractAttribute(INamedTypeSymbol namedType)
        {
            return namedType.GetAttributes().Any(x => x.AttributeClass.Name == "ProtoContractAttribute" && x.AttributeClass.ContainingNamespace.Name == "ProtoBuf");
        }

        private static bool HasProtoIncludeAttribute(INamedTypeSymbol namedType, INamedTypeSymbol matchingType)
        {
            var protoIncludeAttribute = namedType.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == "ProtoIncludeAttribute" && x.AttributeClass.ContainingNamespace.Name == "ProtoBuf");
            if (protoIncludeAttribute is null)
                return false;

            var matchingTypeFound = protoIncludeAttribute.ConstructorArguments.Any(x => x.Type.Name == "Type"
               && x.Type.ContainingNamespace.Name == "System"
               && x.Value is INamedTypeSymbol knownType
               && knownType.Equals(matchingType, SymbolEqualityComparer.Default));
            if (matchingTypeFound)
                return true;

            return protoIncludeAttribute.ConstructorArguments.Any(x => x.Type.Name == "String"
               && x.Type.ContainingNamespace.Name == "System"
               && x.Value is string knownType
               && knownType == matchingType.ToString());
        }

        private bool IsPartial(INamedTypeSymbol namedType)
        {
            return namedType.DeclaringSyntaxReferences.First().GetSyntax() is TypeDeclarationSyntax typeDeclaration && typeDeclaration.Modifiers.Any(x => x.IsKeyword() && x.IsKind(SyntaxKind.PartialKeyword));
        }
    }
}
