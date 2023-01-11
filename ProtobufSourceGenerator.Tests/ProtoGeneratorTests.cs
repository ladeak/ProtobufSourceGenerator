using System.Text;
using Microsoft.CodeAnalysis.Text;
using VerifyCS = ProtobufSourceGenerator.Tests.CSharpSourceGeneratorVerifier<ProtobufSourceGenerator.ProtoGenerator>;

namespace ProtobufSourceGenerator.Tests;

public class ProtoGeneratorTests
{
    private const string CRLF = "\r\n";

    [Fact]
    public async Task SinglePropertyTest()
    {
        var code = @"namespace Test;

[ProtoBuf.ProtoContract]
public partial class Entity
{
    public int Id { get; set; }
}";

        var generated = @"#nullable enable
namespace Test;
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
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };
        await test.RunAsync();
    }

    [Fact]
    public async Task WithRecordTypesClasses()
    {
        var code = @"namespace Test;

[ProtoBuf.ProtoContract]
public partial class Entity
{
    public int Id { get; set; }
}

public record SomeEntity
{
    public int Id { get; set; }
}";

        var generated = @"#nullable enable
namespace Test;
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
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };
        await test.RunAsync();
    }

    [Fact]
    public async Task WithNonGeneratingInnerClass()
    {
        var code = @"namespace Test;

[ProtoBuf.ProtoContract]
public partial class Entity
{
    public class SomeEntity
    {
        public int Id { get; set; }
    }

    public int Id { get; set; }
}";

        var generated = @"#nullable enable
namespace Test;
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
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };
        await test.RunAsync();
    }

    [Fact]
    public async Task WithGeneratingInnerClasses()
    {
        var code = @"namespace Test;

[ProtoBuf.ProtoContract]
public partial class Entity
{
    [ProtoBuf.ProtoContract]
    public partial class SomeEntity
    {
        public int Id { get; set; }
    }

    public int Id { get; set; }
}";

        var generatedEntity = @"#nullable enable
namespace Test;
public partial class Entity
{
    [ProtoBuf.ProtoMember(1)]
    private System.Int32 ProtoId { get => Id; set => Id = value; }
}";

        var generatedSomeEntity = @"#nullable enable
namespace Test;
public partial class Entity
{
    public partial class SomeEntity
    {
        [ProtoBuf.ProtoMember(1)]
        private System.Int32 ProtoId { get => Id; set => Id = value; }
    }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(ProtoGenerator), "ProtoSomeEntity.g.cs", SourceText.From(generatedSomeEntity.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generatedEntity.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task WithGeneratingInner_NonGeneratingParentClass()
    {
        var code = @"namespace Test;

public class Entity
{
    [ProtoBuf.ProtoContract]
    public partial class SomeEntity
    {
        public int Id { get; set; }
    }

    public int Id { get; set; }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                },
            },
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task InitOnlyProperties_DoNotGenerate()
    {
        var code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{
    public int Id { get; init; }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                },
            },
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task NullabeReferenceTypesProperties()
    {
        var code = @"#nullable enable
namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{
    public string? Id { get; set; }
}";

        var generated = @"#nullable enable
namespace Test;
public partial class Entity
{
    [ProtoBuf.ProtoMember(1)]
    private System.String? ProtoId { get => Id; set => Id = value; }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task NullabeGenericReferenceTypesProperties()
    {
        var code = @"#nullable enable
namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{
    public System.Collections.Generic.List<string?> Id { get; set; } = new();
}";

        var generated = @"#nullable enable
namespace Test;
public partial class Entity
{
    [ProtoBuf.ProtoMember(1)]
    private System.Collections.Generic.List<System.String?> ProtoId { get => Id; set => Id = value; }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task NullabeValueTypesProperties()
    {
        var code = @"#nullable enable
namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{
    public int? Id { get; set; }
}";

        var generated = @"#nullable enable
namespace Test;
public partial class Entity
{
    [ProtoBuf.ProtoMember(1)]
    private System.Nullable<System.Int32> ProtoId { get => Id; set => Id = value; }
}";

        var test = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(ProtoGenerator), "ProtoEntity.g.cs", SourceText.From(generated.ReplaceLineEndings(CRLF), Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        };

        await test.RunAsync();
    }
}
