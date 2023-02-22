using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator.Incremental;

[Generator]
public class IncrementalSourceGenerator : IIncrementalGenerator
{
    private IncrementalProtoClassGenerator ClassGenerator { get; } = new IncrementalProtoClassGenerator();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName("ProtoBuf.ProtoContractAttribute", FilterClassNodes,
            static (GeneratorAttributeSyntaxContext context, CancellationToken token) =>
            {
                var typeSymbol = context.TargetSymbol as INamedTypeSymbol;
                var memberProperties = typeSymbol.GetMembers().OfType<IPropertySymbol>();
                var currentClass = new ProtoClassDataModel(typeSymbol);
                foreach (var propertySymbol in memberProperties)
                {
                    token.ThrowIfCancellationRequested();
                    bool hasProtoAttribute = PropertyAttributeParser.HasProtoProperties(propertySymbol, out var tag);
                    if (hasProtoAttribute)
                        currentClass.UsedTags.Add(tag);

                    if (PropertyAttributeParser.CanGenerateAutoProperty(propertySymbol))
                        currentClass.PropertyDataModels.Add(new ProtoPropertyDataModel(propertySymbol));
                }
                return currentClass;
            }).WithComparer(ProtoClassDataModelComparer.Instance)
            .Where(x => x.PropertyDataModels.Any());

        context.RegisterImplementationSourceOutput(provider, (spc, classModel) =>
        {
            var source = ClassGenerator.CreateClass(classModel);
            spc.AddSource($"Proto{classModel.Name}.g.cs", source);
        });
    }

    public static bool FilterClassNodes(SyntaxNode syntaxNode, CancellationToken token)
    {
        do
        {
            if (syntaxNode is TypeDeclarationSyntax node && !node.Modifiers.Any(x => x.IsKeyword() && x.IsKind(SyntaxKind.PartialKeyword)))
                return false;
            syntaxNode = syntaxNode.Parent;
        } while (syntaxNode != null);
        return true;
    }
}