﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class ProtoSyntaxTreeWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel _semantics;
    private List<PropertyShadowInfo> _properties;
    private Stack<bool> _collectingProperties;
    private Stack<ClassShadowInfo> _currentClass;

    public ProtoSyntaxTreeWalker(SemanticModel model)
    {
        _semantics = model;
    }

    public IEnumerable<PropertyShadowInfo> Analyze(SyntaxNode root)
    {
        _properties = new();
        _currentClass = new();
        _collectingProperties = new();
        Visit(root);
        var result = _properties;
        _properties = null;
        _currentClass = null;
        return result;
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node) => VisitTypeDeclaration(node, base.VisitStructDeclaration);

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node) => VisitTypeDeclaration(node, base.VisitRecordDeclaration);

    public override void VisitClassDeclaration(ClassDeclarationSyntax node) => VisitTypeDeclaration(node, base.VisitClassDeclaration);

    public void VisitTypeDeclaration<T>(T node, Action<T> baseAction) where T : TypeDeclarationSyntax
    {
        var collectingProperties = false;
        if (node != null && node.Modifiers.Any(x => x.IsKeyword() && x.IsKind(SyntaxKind.PartialKeyword)))
        {
            foreach (var attributeList in node.AttributeLists)
            {
                if (attributeList.Attributes.Any(x => x.Name.ToFullString().Contains("ProtoContract")
                && _semantics.GetSymbolInfo(x).Symbol is IMethodSymbol symbol
                && symbol.ContainingType.ToString() == "ProtoBuf.ProtoContractAttribute"))
                {
                    collectingProperties = true;
                }
            }
        }
        _collectingProperties.Push(collectingProperties);
        _currentClass.Push(new ClassShadowInfo(node));
        baseAction(node);
        _collectingProperties.Pop();
        _currentClass.Pop();
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (_collectingProperties.Count > 0 && _collectingProperties.All(x => x == true) && _currentClass.Count > 0)
        {
            var currentClass = _currentClass.Peek();
            var typeSymbol = _semantics.GetDeclaredSymbol(currentClass.TypeDeclaration);
            var propertySymbol = _semantics.GetDeclaredSymbol(node);

            bool hasProtoAttribute = HasProtoProperties(propertySymbol, out var tag);
            if (hasProtoAttribute)
                currentClass.UsedTags.Add(tag);

            if (propertySymbol.GetMethod != null
                && propertySymbol.SetMethod != null && !propertySymbol.SetMethod.IsReadOnly && !propertySymbol.SetMethod.IsInitOnly
                && !hasProtoAttribute)
                _properties.Add(new PropertyShadowInfo(currentClass, node, typeSymbol, propertySymbol));
        }
        base.VisitPropertyDeclaration(node);
    }

    private static bool HasProtoProperties(IPropertySymbol propertySymbol, out int tag)
    {
        tag = default;
        bool hasProtoAttribute = false;
        foreach (var attribute in propertySymbol.GetAttributes())
        {
            if (attribute.AttributeClass.ToString() == "ProtoBuf.ProtoMemberAttribute" || attribute.AttributeClass.ToString() == "ProtoBuf.ProtoIgnoreAttribute")
            {
                hasProtoAttribute = true;
                var member = attribute.ConstructorArguments.FirstOrDefault(x => x.Kind == TypedConstantKind.Primitive && x.Type.SpecialType == SpecialType.System_Int32);
                if (member is { Value: int parsedTag })
                    tag = parsedTag;
            }
        }

        return hasProtoAttribute;
    }
}
