using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

internal sealed class PropertyShadowInfo
{
    public PropertyShadowInfo(ClassShadowInfo classInfo, PropertyDeclarationSyntax property, INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol)
    {
        ClassInfo = classInfo;
        Property = property;
        TypeSymbol = typeSymbol;
        PropertySymbol = propertySymbol;
    }

    public ClassShadowInfo ClassInfo { get; }

    public PropertyDeclarationSyntax Property { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    public IPropertySymbol PropertySymbol { get; }
}
