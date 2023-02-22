using Microsoft.CodeAnalysis.Testing;
using AnalyzeCS = ProtobufSourceGenerator.Tests.AnalyzerFixure<ProtobufSourceGenerator.Analyzer, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<ProtobufSourceGenerator.Analyzer, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace ProtobufSourceGenerator.Tests;

public class AnalyzerTests
{
    [Fact]
    public async Task NotPartial_MissingProtoContract_ReturnsDiagnosticError()
    {
        string code = @"namespace Test;
public class SomeEntity
{
    public int Value { get; set; }
}

[ProtoBuf.ProtoContract]
public partial class Entity
{   
    public SomeEntity Id { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto01").WithLocation(10, 23).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task Partial_WithProtoContract_NoDiagnosticError()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class SomeEntity
{
    public int Value { get; set; }
}

[ProtoBuf.ProtoContract]
public partial class Entity
{   
    public SomeEntity Id { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task ProtoMember_NotPartial_MissingProtoContract_NoError()
    {
        string code = @"namespace Test;
public class SomeEntity
{
    public int Value { get; set; }
}

[ProtoBuf.ProtoContract]
public partial class Entity
{   
    [global::ProtoBuf.ProtoMember(1)]
    public SomeEntity Id { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task ProtoIgnore_NotPartial_MissingProtoContract_NoError()
    {
        string code = @"namespace Test;
public class SomeEntity
{
    public int Value { get; set; }
}

[ProtoBuf.ProtoContract]
public partial class Entity
{   
    [global::ProtoBuf.ProtoMember(1)]
    public SomeEntity Id { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task Info_ForNonGeneratingProperty()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto03").WithLocation(5, 16).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task InfoIgnore_ForNonGeneratingProperty()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{   
    [ProtoBuf.ProtoIgnore]
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task ParentPartialType_NoDiagnosticError()
    {
        string code = @"namespace Test;
public partial class ParentEntity
{
    public int Value { get; set; }

    [ProtoBuf.ProtoContract]
    public partial class Entity
    {   
        public string Id { get; set; }
    }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task ParentType_NotPartial_DiagnosticError()
    {
        string code = @"namespace Test;
public class ParentEntity
{
    public int Value { get; set; }

    [ProtoBuf.ProtoContract]
    public partial class Entity
    {   
        public string Id { get; set; }
    }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto02").WithLocation(7, 26).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task ParentStructType_NotPartial_DiagnosticError()
    {
        string code = @"namespace Test;
public struct ParentEntity
{
    public int Value { get; set; }

    [ProtoBuf.ProtoContract]
    public partial class Entity
    {   
        public string Id { get; set; }
    }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto02").WithLocation(7, 26).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task NoInfo_ForNonGeneratingContainingType()
    {
        string code = @"namespace Test;
public partial class Entity
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task NotAutoProperty_GeneratesProtoWarning()
    {
        string code = @"#nullable enable
namespace Test;
[ProtoBuf.ProtoContract]
public partial class Entity
{   
    private int _id;
    public int Id { get => _id; set { if(value > 0) throw new System.Exception(); _id = value; } }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto03").WithLocation(7, 16).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }
}