using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtobufSourceGenerator;

public class ClassShadowInfo
{
    public ClassShadowInfo(TypeDeclarationSyntax typeDeclaration)
    {
        TypeDeclaration = typeDeclaration;
        UsedTags = new();
    }

    public TypeDeclarationSyntax TypeDeclaration { get; }

    public HashSet<int> UsedTags { get; }
}
