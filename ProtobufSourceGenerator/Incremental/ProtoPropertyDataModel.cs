using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;


public record struct ProtoPropertyDataModel
{
    public enum PropertyKind
    {
        None,
        CollectionHelper,
        EnumerationHelper,
    }

    public ProtoPropertyDataModel(IPropertySymbol propertySymbol, PropertyKind kind = PropertyKind.None)
    {
        Kind = kind;
        PropertyIdentifier = propertySymbol.Name;
        PropertyTypeName = propertySymbol.Type.ToString();
        GenertyTypeParameter = string.Empty;
        if (propertySymbol.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            if (kind == PropertyKind.EnumerationHelper)
                GenertyTypeParameter = namedType.TypeArguments.First().ToString();
        }
    }

    public string PropertyTypeName { get; }

    public string PropertyIdentifier { get; }

    public string GenertyTypeParameter { get; }

    public PropertyKind Kind { get; }
}
