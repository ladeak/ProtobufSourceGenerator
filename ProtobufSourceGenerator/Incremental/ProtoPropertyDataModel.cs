using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;


public record struct ProtoPropertyDataModel
{
    public enum PropertyKind
    {
        None,
        ConcreteHelper,
        AbstractionCollactionHelper,
        AbstractionDictionaryHelper,
        EnumerationHelper,
    }

    public ProtoPropertyDataModel(IPropertySymbol propertySymbol, PropertyKind kind = PropertyKind.None)
    {
        Kind = kind;
        PropertyIdentifier = propertySymbol.Name;
        PropertyTypeName = propertySymbol.Type.ToString();
        GenertyTypeParameter0 = string.Empty;
        IsInit = propertySymbol.SetMethod?.IsInitOnly ?? false;
        CustomAttribute = string.Empty;
        foreach (var attribute in propertySymbol.ContainingType.GetAttributes())
        {
            if (attribute.AttributeClass.Name == "GeneratorOptionsAttribute" && attribute.AttributeClass.ContainingNamespace.Name == "ProtobufSourceGenerator")
            {
                var argument = attribute.NamedArguments.FirstOrDefault(x => x.Key == nameof(GeneratorOptionsAttribute.PropertyAttributeType));
                if (argument.Value.Type.Name == "Type" && argument.Value.Type.ContainingNamespace.Name == "System" && argument.Value.Value is INamedTypeSymbol namedTypeSymbol)
                {
                    CustomAttribute = namedTypeSymbol.ToString();
                    break;
                }
            }
        }

        if (propertySymbol.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            GenertyTypeParameter0 = namedType.TypeArguments.First().ToString();
            GenertyTypeParameter1 = namedType.TypeArguments.Last().ToString();
        }
    }

    public string CustomAttribute { get; set; }

    public bool IsInit { get; }

    public string PropertyTypeName { get; }

    public string PropertyIdentifier { get; }

    public string GenertyTypeParameter0 { get; }

    public string GenertyTypeParameter1 { get; }

    public PropertyKind Kind { get; }
}
