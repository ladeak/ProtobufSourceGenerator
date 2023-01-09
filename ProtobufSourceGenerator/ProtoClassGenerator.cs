﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ProtobufSourceGenerator;

public class ProtoClassGenerator
{
    public IEnumerable<(string, string)> CreateClasses(IEnumerable<PropertyInfo> propertyShadows) => propertyShadows.GroupBy(x => x.TypeSymbol.Name).Select(x => (x.Key, CreateClass(x)));

    private string CreateClass(IEnumerable<PropertyInfo> propertyShadows)
    {
        var classInfo = propertyShadows.First().TypeSymbol;

        StringBuilder sb = new();
        sb.AppendLine($"namespace {classInfo.ContainingNamespace};");
        sb.AppendLine();

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

                var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                  .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(shadow.Property.Identifier.Text),
                    SyntaxFactory.IdentifierName("value"))))
                  .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                var protoMemberAttribute = SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("Protobuf"), SyntaxFactory.IdentifierName("ProtoMember")))
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
        sb.Append(classSyntax.NormalizeWhitespace().ToFullString());
        return sb.ToString();
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
        sb.Append($"{type.ContainingNamespace}{type.Name}<");

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