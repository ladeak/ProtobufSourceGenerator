using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;

public class ProtoClassDataModel
{
    public ProtoClassDataModel(INamedTypeSymbol typeSymbol)
    {
        UsedTags = new();
        Name = typeSymbol.Name;
        Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : typeSymbol.ContainingNamespace.ToString();
        IsRecord = typeSymbol.IsRecord;
        IsReferenceType = typeSymbol.IsReferenceType;
        if (typeSymbol.ContainingSymbol is INamedTypeSymbol parentClass)
            Parent = new ProtoClassDataModel(parentClass);
        PropertyDataModels = new List<ProtoPropertyDataModel>();
    }

    public HashSet<int> UsedTags { get; }

    public string Name { get; }

    public string Namespace { get; }

    public bool IsRecord { get; }

    public bool IsReferenceType { get; }

    public ProtoClassDataModel? Parent { get; }

    public IList<ProtoPropertyDataModel> PropertyDataModels { get; }
}
