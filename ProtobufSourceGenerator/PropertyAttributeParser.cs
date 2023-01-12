using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator;

internal sealed class PropertyAttributeParser
{
    public static bool CanGenerateProperty(IPropertySymbol propertySymbol)
    {
        return propertySymbol.GetMethod != null
                && propertySymbol.SetMethod != null && !propertySymbol.SetMethod.IsReadOnly && !propertySymbol.SetMethod.IsInitOnly
                && !HasProtoProperties(propertySymbol, out _);
    }

    public static bool HasProtoProperties(IPropertySymbol propertySymbol, out int tag)
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
