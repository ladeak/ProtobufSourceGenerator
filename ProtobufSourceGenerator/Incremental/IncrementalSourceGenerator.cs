using System.Collections.Generic;
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
                        currentClass.PropertyDataModels.AddRange(CreatePropertyModels(propertySymbol));
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

    private static IEnumerable<ProtoPropertyDataModel> CreatePropertyModels(IPropertySymbol propertySymbol)
    {
        var result = new List<ProtoPropertyDataModel>(2)
        {
            new ProtoPropertyDataModel(propertySymbol)
        };
        if (propertySymbol.Type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
            return result;

        if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            result.Add(new ProtoPropertyDataModel(propertySymbol, ProtoPropertyDataModel.PropertyKind.EnumerationHelper));
        }
        else if (IsCollectionType(namedType.OriginalDefinition))
        {
            result.Add(new ProtoPropertyDataModel(propertySymbol, ProtoPropertyDataModel.PropertyKind.CollectionHelper));
        }
        return result;
    }

    private static bool IsCollectionType(INamedTypeSymbol originalNamedType)
    {
        if (IsSpecialCollectionType(originalNamedType))
            return true;

        foreach (var implementedInterface in originalNamedType.Interfaces)
        {
            if (IsSpecialCollectionType(implementedInterface.OriginalDefinition))
                return true;
        }
        return false;
    }

    private static bool IsSpecialCollectionType(INamedTypeSymbol namedType)
    {
        if (namedType.SpecialType == SpecialType.System_Collections_Generic_ICollection_T
                || namedType.SpecialType == SpecialType.System_Collections_Generic_IList_T
                || namedType.SpecialType == SpecialType.System_Collections_Generic_IReadOnlyCollection_T
                || namedType.SpecialType == SpecialType.System_Collections_Generic_IReadOnlyList_T)
            return true;
        return false;
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