using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator;

[Generator]
public class ProtoGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;
        List<PropertyInfo> propertyShadowInfos = new();
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var walker = new ProtoSyntaxTreeWalker(context.Compilation.GetSemanticModel(tree));
            propertyShadowInfos.AddRange(walker.Analyze(root));
        }

        var classGenerator = new ProtoClassGenerator();
        foreach (var (fileName, source) in classGenerator.CreateClasses(propertyShadowInfos))
            context.AddSource($"Proto{fileName}.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}
