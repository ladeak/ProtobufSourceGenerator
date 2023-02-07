﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator.Incremental;

internal sealed class IncrementalProtoClassGenerator
{
    public string CreateClass(ProtoClassDataModel classModel)
    {
        TypeDeclarationSyntax typeSyntax = GenerateType(classModel);

        int counter = 1;
        foreach (ProtoPropertyDataModel property in classModel.PropertyDataModels)
        {
            var newProperty = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(property.PropertyTypeName),
              SyntaxFactory.Identifier($"Proto{property.PropertyIdentifier}"))
              .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
              .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(property.PropertyIdentifier)))
              .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
              .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(property.PropertyIdentifier),
                SyntaxFactory.IdentifierName("value"))))
              .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            while (classModel.UsedTags.Contains(counter))
                counter++;

            var protoMemberAttribute = SyntaxFactory.SingletonList(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.QualifiedName(SyntaxFactory.AliasQualifiedName(
                                SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword)), SyntaxFactory.IdentifierName("ProtoBuf")), SyntaxFactory.IdentifierName("ProtoMember")))
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

        typeSyntax = typeSyntax.WithAttributeLists(SyntaxFactory.SingletonList(
            SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.AliasQualifiedName(
                                SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword)), SyntaxFactory.IdentifierName("System")), SyntaxFactory.IdentifierName("Runtime")), SyntaxFactory.IdentifierName("CompilerServices")), SyntaxFactory.IdentifierName("CompilerGeneratedAttribute")))))));

        // For all parent types, we wrap the inner type
        while (classModel.Parent is { } parentClass)
        {
            var parentClassSyntax = GenerateType(parentClass);
            parentClassSyntax = parentClassSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(typeSyntax));
            typeSyntax = parentClassSyntax;
            classModel = parentClass;
        }

        SyntaxNode rootDeclaration = typeSyntax;

        // Adding namespace to type when namespace is not global
        if (!string.IsNullOrWhiteSpace(classModel.Namespace))
        {
            var namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName(classModel.Namespace));
            rootDeclaration = namespaceDeclaration.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(typeSyntax));
        }

        // Adding nullability
        rootDeclaration = rootDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment("// <auto-generated/>"), SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true))));
        return rootDeclaration.NormalizeWhitespace().ToFullString();
    }

    private static TypeDeclarationSyntax GenerateType(ProtoClassDataModel classModel)
    {
        TypeDeclarationSyntax type = classModel switch
        {
            { IsRecord: true, IsReferenceType: true } => SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), classModel.Name).WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword)).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)).WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)),
            { IsRecord: true, IsReferenceType: false } => SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), classModel.Name).WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.StructDeclaration)).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)).WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)),
            { IsReferenceType: false } => SyntaxFactory.StructDeclaration(classModel.Name),
            _ => SyntaxFactory.ClassDeclaration(classModel.Name),
        };

        return type.WithModifiers(
            SyntaxFactory.TokenList(
                new[] {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                }));
    }
}