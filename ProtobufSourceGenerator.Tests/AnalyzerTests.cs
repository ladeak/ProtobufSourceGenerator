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
}