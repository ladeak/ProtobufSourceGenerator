using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator.Incremental;

public class ProtoPropertyDataModel
{
    public ProtoPropertyDataModel(ProtoClassDataModel classInfo, PropertyDeclarationSyntax property, INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol)
    {
        ClassInfo = classInfo;
        PropertyIdentifier = property.Identifier.Text;
        TypeSymbol = typeSymbol;
        PropertyTypeName = propertySymbol.Type.Name;
    }

    public ProtoClassDataModel ClassInfo { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    public string PropertyTypeName { get; }

    public string PropertyIdentifier { get; }
}
