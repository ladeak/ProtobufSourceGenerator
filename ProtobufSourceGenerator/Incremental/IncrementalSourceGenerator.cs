using Microsoft.CodeAnalysis;

namespace ProtobufSourceGenerator.Incremental;

public class IncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //context.SyntaxProvider.ForAttributeWithMetadataName
    }
}
