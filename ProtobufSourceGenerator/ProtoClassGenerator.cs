using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class ProtoClassGenerator
{
    public IEnumerable<(string, string)> CreateClasses(IEnumerable<PropertyShadowInfo> propertyShadows) => propertyShadows.GroupBy(x => x.TypeSymbol.Name).Select(x => (x.Key, CreateClass(x)));

    private string CreateClass(IEnumerable<PropertyShadowInfo> propertyShadows)
    {
        var typeInfo = propertyShadows.First().TypeSymbol;
        TypeDeclarationSyntax typeSyntax = GenerateType(typeInfo);

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

                while (shadow.ClassInfo.UsedTags.Contains(counter))
                    counter++;

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

                typeSyntax = typeSyntax.WithMembers(typeSyntax.Members.Add(newProperty));
            }
        }

        // For all parent types, we wrap the inner type
        while (typeInfo.ContainingSymbol is INamedTypeSymbol parentClass)
        {
            var parentClassSyntax = GenerateType(parentClass);
            parentClassSyntax = parentClassSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(typeSyntax));
            typeSyntax = parentClassSyntax;
            typeInfo = parentClass;
        }

        // Adding namespace
        var namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName(typeInfo.ContainingNamespace.ToString()));

        // Adding nullability
        namespaceDeclaration = namespaceDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true))));

        // Adding type
        namespaceDeclaration = namespaceDeclaration.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(typeSyntax));
        return namespaceDeclaration.NormalizeWhitespace().ToFullString();
    }

    private static TypeDeclarationSyntax GenerateType(INamedTypeSymbol typeInfo)
    {
        TypeDeclarationSyntax type = typeInfo switch
        {
            { IsRecord: true, IsReferenceType: true } => SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), typeInfo.Name).WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword)).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)).WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)),
            { IsRecord: true, IsReferenceType: false } => SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), typeInfo.Name).WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.StructDeclaration)).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)).WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)),
            { IsReferenceType: false } => SyntaxFactory.StructDeclaration(typeInfo.Name),
            _ => SyntaxFactory.ClassDeclaration(typeInfo.Name),
        };

        return type.WithModifiers(
            SyntaxFactory.TokenList(
                new[] {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                }));
    }

    private static string GetTypeName(PropertyShadowInfo shadow)
    {
        if (shadow.PropertySymbol.Type is INamedTypeSymbol propertyType)
            return GetTypeName(propertyType);
        return shadow.PropertySymbol.Type.Name;
    }

    private static string GetTypeName(INamedTypeSymbol type) => type.ToString();
}