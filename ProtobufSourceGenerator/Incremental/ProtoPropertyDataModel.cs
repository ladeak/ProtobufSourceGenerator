using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;

public record struct ProtoPropertyDataModel
{
    public ProtoPropertyDataModel(IPropertySymbol propertySymbol)
    {
        PropertyIdentifier = propertySymbol.Name;
        PropertyTypeName = propertySymbol.Type.Name;
    }

    public string PropertyTypeName { get; }

    public string PropertyIdentifier { get; }
}
