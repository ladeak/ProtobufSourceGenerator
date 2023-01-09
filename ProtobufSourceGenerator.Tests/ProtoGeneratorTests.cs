using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using VerifyCS = ProtobufSourceGenerator.Tests.CSharpSourceGeneratorVerifier<ProtobufSourceGenerator.ProtoGenerator>;
using ProtoBuf;

namespace ProtobufSourceGenerator.Tests;

public class ProtoGeneratorTests
{
    [Fact]
    public async Task SinglePropertyTest()
    {
        var code = @"namespace Test;

[ProtoBuf.ProtoContract]
public partial class Entity
{
    public int Id { get; set; }
}";

        var generated = @"namespace Test;

public partial class Entity
{
    [ProtoBuf.ProtoMember(1)]
    private System.Int32 ProtoId { get => Id; set => Id = value; }
}";
        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(ProtoContractAttribute).Assembly.Location));
        test.TestState.ReferenceAssemblies = new ReferenceAssemblies("net7.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "7.0.0"), Path.Combine("ref", "net7.0"));


        await test.RunAsync();
    }
}
