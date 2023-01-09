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
    private bool _collectingProperties;
    private ClassDeclarationSyntax _currentClass;

    public ProtoSyntaxTreeWalker(SemanticModel model)
    {
        _semantics = model;
    }

    public IEnumerable<PropertyInfo> Analyze(SyntaxNode root)
    {
        _properties = new();
        _collectingProperties = false;
        _currentClass = null;
        Visit(root);
        var result = _properties;
        _properties = null;
        _currentClass = null;
        return result;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _collectingProperties = false;
        _currentClass = null;
        if (node != null && node.Modifiers.Any(x => x.IsKeyword() && x.IsKind(SyntaxKind.PartialKeyword)))
        {
            foreach (var attributeList in node.AttributeLists)
            {
                if (attributeList.Attributes.Any(x => x.Name.ToFullString().Contains("ProtoContract")
                && _semantics.GetSymbolInfo(x).Symbol is IMethodSymbol symbol
                && symbol.ContainingType.ToString() == "Protobuf.ProtoContractAttribute"))
                {
                    _collectingProperties = true;
                    _currentClass = node;
                }
            }
        }
        base.VisitClassDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (_collectingProperties && _currentClass != null)
        {
            var typeSymbol = _semantics.GetDeclaredSymbol(_currentClass);
            var propertySymbol = _semantics.GetDeclaredSymbol(node);
            if (propertySymbol.GetMethod != null && !propertySymbol.GetMethod.IsReadOnly
            && propertySymbol.SetMethod != null && !propertySymbol.SetMethod.IsReadOnly)
                _properties.Add(new PropertyInfo(_currentClass, node, typeSymbol, propertySymbol));
        }
        base.VisitPropertyDeclaration(node);
    }
}
