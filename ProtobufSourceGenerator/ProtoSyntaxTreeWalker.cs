using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class ProtoSyntaxTreeWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel _semantics;
    private List<PropertyInfo> _properties;
    private Stack<bool> _collectingProperties;
    private Stack<ClassDeclarationSyntax> _currentClass;

    public ProtoSyntaxTreeWalker(SemanticModel model)
    {
        _semantics = model;
    }

    public IEnumerable<PropertyInfo> Analyze(SyntaxNode root)
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

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        base.VisitRecordDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
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
        _currentClass.Push(node);
        base.VisitClassDeclaration(node);
        _collectingProperties.Pop();
        _currentClass.Pop();
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (_collectingProperties.Count > 0 && _collectingProperties.All(x => x == true) && _currentClass.Count > 0)
        {
            var currentClass = _currentClass.Peek();
            var typeSymbol = _semantics.GetDeclaredSymbol(currentClass);
            var propertySymbol = _semantics.GetDeclaredSymbol(node);
            if (propertySymbol.GetMethod != null && !propertySymbol.GetMethod.IsReadOnly
            && propertySymbol.SetMethod != null && !propertySymbol.SetMethod.IsReadOnly)
                _properties.Add(new PropertyInfo(currentClass, node, typeSymbol, propertySymbol));
        }
        base.VisitPropertyDeclaration(node);
    }
}
