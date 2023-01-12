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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule01); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IPropertySymbol propertySymbol)
                return;

            if (propertySymbol.Type is not INamedTypeSymbol namedType || namedType.SpecialType != SpecialType.None)
                return;

            bool hasProtoContract = namedType.GetAttributes().Any(x => x.AttributeConstructor.ToString() != "ProtoBuf.ProtoContractAttribute");

            bool isPartial = namedType.DeclaringSyntaxReferences.First().GetSyntax() is TypeDeclarationSyntax typeDeclaration && typeDeclaration.Modifiers.Any(x => x.IsKeyword() && x.IsKind(SyntaxKind.PartialKeyword));

            if (hasProtoContract && isPartial)
                return;

            var diagnostic = Diagnostic.Create(Rule01, context.Symbol.Locations.First(), string.Empty);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
