using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class ProtoClassGenerator
{
    public IEnumerable<(string, string)> CreateClasses(IEnumerable<PropertyInfo> propertyShadows) => propertyShadows.GroupBy(x => x.TypeSymbol.Name).Select(x => (x.Key, CreateClass(x)));

    private string CreateClass(IEnumerable<PropertyInfo> propertyShadows)
    {
        var classInfo = propertyShadows.First().TypeSymbol;

        var classSyntax = SyntaxFactory.ClassDeclaration(classInfo.Name)
        .WithModifiers(
            SyntaxFactory.TokenList(
                new[] {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                }));

        int counter = 1;
        foreach (var shadow in propertyShadows)
        {
            if (shadow.Property.AccessorList.Accessors.All(x => x.Body == null && x.ExpressionBody == null))
            {
                string typeName = GetTypeName(shadow);
                var newProperty = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(typeName),
                  SyntaxFactory.Identifier($"Proto{shadow.Property.Identifier.Text}"))
                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                  .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(shadow.Property.Identifier.Text)))
                  .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                  .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(shadow.Property.Identifier.Text),
                    SyntaxFactory.IdentifierName("value"))))
                  .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                var protoMemberAttribute = SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("ProtoBuf"), SyntaxFactory.IdentifierName("ProtoMember")))
                            .WithArgumentList(
                                SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.AttributeArgument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(counter++)))))))));

                newProperty = newProperty.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { getter, setter })))
                .NormalizeWhitespace().WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(" ")))
                .WithAttributeLists(protoMemberAttribute);

                classSyntax = classSyntax.WithMembers(classSyntax.Members.Add(newProperty));
            }
        }

        // For all parent types, we wrap the inner type
        while (classInfo.ContainingSymbol is INamedTypeSymbol parentClass)
        {
            var parentClassSyntax = SyntaxFactory.ClassDeclaration(parentClass.Name)
                .WithModifiers(
                SyntaxFactory.TokenList(
                    new[] {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
           }));

            parentClassSyntax = parentClassSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));
            classSyntax = parentClassSyntax;
            classInfo = parentClass;
        }

        var namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(
            SyntaxFactory.IdentifierName(classInfo.ContainingNamespace.ToString()));

        namespaceDeclaration = namespaceDeclaration.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));
        return namespaceDeclaration.NormalizeWhitespace().ToFullString();
    }

    private static string GetTypeName(PropertyInfo shadow)
    {
        if (shadow.PropertySymbol.Type is INamedTypeSymbol propertyType)
            return GetTypeName(propertyType);
        return shadow.PropertySymbol.Type.Name;
    }

    private static string GetTypeName(INamedTypeSymbol type)
    {
        if (!type.IsGenericType)
            return $"{type.ContainingNamespace}.{type.Name}";

        StringBuilder sb = new();
        sb.Append($"{type.ContainingNamespace}.{type.Name}<");

        for (int i = 0; i < type.TypeArguments.Length; i++)
        {
            var typeArgument = type.TypeArguments[i];
            if (typeArgument is INamedTypeSymbol namedType)
                sb.Append(GetTypeName(namedType));
            else
                sb.Append(typeArgument.Name);

            if (i < type.TypeArguments.Length - 1)
                sb.Append(", ");
        }
        sb.Append(">");
        return sb.ToString();
    }
}