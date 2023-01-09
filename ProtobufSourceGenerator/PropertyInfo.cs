using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class PropertyInfo
{
    public PropertyInfo(ClassDeclarationSyntax classDeclaration, PropertyDeclarationSyntax property, INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol)
    {
        ClassDeclaration = classDeclaration;
        Property = property;
        TypeSymbol = typeSymbol;
        PropertySymbol = propertySymbol;
    }

    public ClassDeclarationSyntax ClassDeclaration { get; }
    public PropertyDeclarationSyntax Property { get; }
    public INamedTypeSymbol TypeSymbol { get; }
    public IPropertySymbol PropertySymbol { get; }
}
