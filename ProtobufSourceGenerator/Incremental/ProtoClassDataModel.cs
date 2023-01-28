using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;

public class ProtoClassDataModel
{
    public ProtoClassDataModel(INamedTypeSymbol typeSymbol, IEnumerable<ProtoPropertyDataModel> propertyDataModels)
    {
        UsedTags = new();
        Name = typeSymbol.Name;
        Namespace = typeSymbol.ContainingNamespace.ToString();
        IsRecord = typeSymbol.IsRecord;
        IsReferenceType = typeSymbol.IsReferenceType;
        if (typeSymbol.ContainingSymbol is INamedTypeSymbol parentClass)
        {
            Parent = new ProtoClassDataModel(parentClass, Enumerable.Empty<ProtoPropertyDataModel>());
        }
        PropertyDataModels = propertyDataModels;
    }

    public HashSet<int> UsedTags { get; }

    public string Name { get; }

    public string Namespace { get; }

    public bool IsRecord { get; }

    public bool IsReferenceType { get; }

    public ProtoClassDataModel? Parent { get; }

    public IEnumerable<ProtoPropertyDataModel> PropertyDataModels { get; }
}