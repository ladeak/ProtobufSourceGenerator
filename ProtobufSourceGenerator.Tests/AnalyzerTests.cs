using Microsoft.CodeAnalysis.Testing;
using AnalyzeCS = ProtobufSourceGenerator.Tests.AnalyzerFixure<ProtobufSourceGenerator.Analyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<ProtobufSourceGenerator.Analyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync();
    }
    
    [Fact]
    public async Task NoInfo_ForNonGeneratingNestedType_Issue59()
    {
        string code = @"namespace Test;
public class Foo
{   
    public class Bar {}
}";
        var test = new AnalyzeCS { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
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
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Enums_DoNotGenerateWarning()
    {
        string code = @"#nullable enable
namespace Test;
public enum MyEnum
{
    One,
    Two
}
[ProtoBuf.ProtoContract]
public partial class Entity
{   
    public MyEnum Value { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task BaseTypeNonGenerated_IssuesWarning()
    {
        string code = @"namespace Test;
public class Base
{
    public int Value { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto04").WithLocation(7, 22).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PartialBaseTypeNonGenerated_IssuesWarning()
    {
        string code = @"namespace Test;
public partial class Base
{
    public int Value { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto04").WithLocation(7, 22).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoProtoIncludeBaseTypeNonGenerated_IssuesWarning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class Base
{
    public int Value { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto04").WithLocation(8, 22).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProtoIncludeBaseTypeNonGenerated_NoWarning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
[ProtoBuf.ProtoInclude(10, typeof(Derived))]
public partial class Base
{
    public int Value { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync();
    }

    [Fact]
    public async Task NonGeneratingDerived_BaseType_NoWarning()
    {
        string code = @"namespace Test;
public class Base
{
    public int Value { get; set; }
}
public class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProtoIncludeNotPartial_BaseType_NoWarning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
[ProtoBuf.ProtoInclude(10, typeof(Derived))]
public class Base
{
    public int Value { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProtoInclude_NotMatchingDerivedType_Warning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
[ProtoBuf.ProtoInclude(10, typeof(SomeOther))]
public class Base
{
    public int Value { get; set; }
}
public class SomeOther : Base
{
    public int Data { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto04").WithLocation(13, 22).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProtoInclude_NotMatchingDerivedTypeString_Warning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
[ProtoBuf.ProtoInclude(10, ""Test.SomeOther"")]
public class Base
{
    public int Value { get; set; }
}
public class SomeOther : Base
{
    public int Data { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        DiagnosticResult expected = VerifyCS.Diagnostic("Proto04").WithLocation(13, 22).WithArguments(string.Empty);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProtoInclude_MatchingDerivedTypeString_NoWarning()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
[ProtoBuf.ProtoInclude(10, ""Test.Derived"")]
public class Base
{
    public int Value { get; set; }
}
public class SomeOther : Base
{
    public int Data { get; set; }
}
[ProtoBuf.ProtoContract]
public partial class Derived : Base
{   
    public int Id { get; init; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ListDictionary_ShouldNotThrow()
    {
        string code = @"namespace Test;
[ProtoBuf.ProtoContract]
public partial class SomeEntity
{
    public System.Collections.Generic.List<int>? Value { get; set; }
}";
        var test = new AnalyzeCS() { TestCode = code };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}