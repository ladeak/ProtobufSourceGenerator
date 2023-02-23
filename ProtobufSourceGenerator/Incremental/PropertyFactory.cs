using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator.Incremental;

public static class PropertyFactory
{
    public static string GetNormalPropertyName(ProtoPropertyDataModel property) => $"Proto{property.PropertyIdentifier}";

    public static string GetIsEmptyPropertyName(ProtoPropertyDataModel property) => $"ProtoIsEmpty{property.PropertyIdentifier}";

    public static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateGetterSetter(ProtoPropertyDataModel property) => property.Kind switch
    {
        ProtoPropertyDataModel.PropertyKind.None => CreateNormalGetterSetter(property),
        ProtoPropertyDataModel.PropertyKind.AbstractionCollactionHelper => CreateAbstractCollectionHelperGetterSetter(property),
        ProtoPropertyDataModel.PropertyKind.AbstractionDictionaryHelper => CreateAbstractDictionaryHelperGetterSetter(property),
        ProtoPropertyDataModel.PropertyKind.ConcreteHelper => CreateCollectionHelperGetterSetter(property),
        ProtoPropertyDataModel.PropertyKind.EnumerationHelper => CreateEnumerationHelperGetterSetter(property),
        _ => throw new InvalidOperationException("Proto property type kind supported"),
    };

    private static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateNormalGetterSetter(ProtoPropertyDataModel property)
    {
        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(property.PropertyIdentifier)))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        var setter = SyntaxFactory.AccessorDeclaration(CreateSetterAccessor(property))
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxFactory.IdentifierName(property.PropertyIdentifier),
            SyntaxFactory.IdentifierName("value"))))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        return (getter, setter);
    }

    private static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateCollectionHelperGetterSetter(ProtoPropertyDataModel property)
    {
        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
            SyntaxFactory.ConditionalAccessExpression(SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Count"))),
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var setter = SyntaxFactory.AccessorDeclaration(CreateSetterAccessor(property))
            .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.IfStatement(SyntaxFactory.IdentifierName("value"),
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), SyntaxFactory.ImplicitObjectCreationExpression()))))));

        return (getter, setter);
    }

    private static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateAbstractCollectionHelperGetterSetter(ProtoPropertyDataModel property)
    {
        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
            SyntaxFactory.ConditionalAccessExpression(SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Count"))),
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var newObject = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(
                                SyntaxFactory.AliasQualifiedName(SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                    SyntaxFactory.IdentifierName("System")), SyntaxFactory.IdentifierName("Collections")), SyntaxFactory.IdentifierName("Generic")), SyntaxFactory.GenericName(SyntaxFactory.Identifier("List"))
                        .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.ParseTypeName(property.GenertyTypeParameter0))))))
                        .WithArgumentList(SyntaxFactory.ArgumentList());


        var setter = SyntaxFactory.AccessorDeclaration(CreateSetterAccessor(property))
            .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.IfStatement(SyntaxFactory.IdentifierName("value"),
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), newObject))))));

        return (getter, setter);
    }

    private static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateAbstractDictionaryHelperGetterSetter(ProtoPropertyDataModel property)
    {
        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
            SyntaxFactory.ConditionalAccessExpression(SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Count"))),
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var newObject = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(
                                SyntaxFactory.AliasQualifiedName(SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                    SyntaxFactory.IdentifierName("System")), SyntaxFactory.IdentifierName("Collections")), SyntaxFactory.IdentifierName("Generic")), SyntaxFactory.GenericName(SyntaxFactory.Identifier("Dictionary"))
                        .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] {
                                            SyntaxFactory.IdentifierName(property.GenertyTypeParameter0),
                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                            SyntaxFactory.IdentifierName(property.GenertyTypeParameter1) })))))
                        .WithArgumentList(SyntaxFactory.ArgumentList());


        var setter = SyntaxFactory.AccessorDeclaration(CreateSetterAccessor(property))
            .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.IfStatement(SyntaxFactory.IdentifierName("value"),
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), newObject))))));

        return (getter, setter);
    }

    private static (AccessorDeclarationSyntax Getter, AccessorDeclarationSyntax Setter) CreateEnumerationHelperGetterSetter(ProtoPropertyDataModel property)
    {
        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
              SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression,
              SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, SyntaxFactory.IdentifierName(GetNormalPropertyName(property)), SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
              SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                  SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Linq")), SyntaxFactory.IdentifierName("Enumerable")), SyntaxFactory.IdentifierName("Any")))
                  .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(GetNormalPropertyName(property))))))))))
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var setter = SyntaxFactory.AccessorDeclaration(CreateSetterAccessor(property))
          .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.IfStatement(SyntaxFactory.IdentifierName("value"),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(GetNormalPropertyName(property)),
                                            SyntaxFactory.InvocationExpression(GetEnumerableEmpty(property))))))));

        return (getter, setter);
    }

    private static MemberAccessExpressionSyntax GetEnumerableEmpty(ProtoPropertyDataModel property) =>
        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Linq")), SyntaxFactory.IdentifierName("Enumerable")),
            SyntaxFactory.GenericName(SyntaxFactory.Identifier("Empty"))
            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.ParseTypeName(property.GenertyTypeParameter0)))));

    private static SyntaxKind CreateSetterAccessor(ProtoPropertyDataModel property) => property.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration;
}