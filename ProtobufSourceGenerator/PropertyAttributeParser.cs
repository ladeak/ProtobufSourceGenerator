using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

internal sealed class PropertyAttributeParser
{
    public static bool CanGenerateAutoProperty(IPropertySymbol propertySymbol, CancellationToken token = default)
    {
        if (propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(token) is PropertyDeclarationSyntax propertySyntax
                       && propertySyntax.AccessorList != null
                       && propertySyntax.AccessorList.Accessors.All(x => x.Body == null && x.ExpressionBody == null))
        {
            return PropertyAttributeParser.CanGenerateProperty(propertySymbol);
        }
        return false;
    }

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
