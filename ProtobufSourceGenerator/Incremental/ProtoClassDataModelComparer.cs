using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtobufSourceGenerator.Incremental;

internal sealed class ProtoClassDataModelComparer : IEqualityComparer<ProtoClassDataModel>
{
    internal static ProtoClassDataModelComparer Instance { get; } = new ProtoClassDataModelComparer();

    public bool Equals(ProtoClassDataModel x, ProtoClassDataModel y)
    {
        return x.Name == y.Name
            && x.Namespace == y.Namespace
            && x.IsRecord && y.IsRecord
            && x.IsReferenceType == y.IsReferenceType
            && x.UsedTags.Count == y.UsedTags.Count
            && !(x.Parent != null ^ y.Parent != null)
            && x.PropertyDataModels.SequenceEqual(y.PropertyDataModels)
            && x.UsedTags.SequenceEqual(y.UsedTags)
            && (x.Parent?.Equals(y.Parent) ?? true);
    }

    public int GetHashCode(ProtoClassDataModel obj)
    {
        return (obj.Name, obj.Namespace, obj.IsRecord, obj.IsReferenceType, obj.Parent != null, obj.PropertyDataModels.Count, obj.UsedTags.Count).GetHashCode();
    }
}