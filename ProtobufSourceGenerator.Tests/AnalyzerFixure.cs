using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using ProtoBuf;

namespace ProtobufSourceGenerator.Tests;

internal class AnalyzerFixure<TAnalyzer, TVerifier> : CSharpAnalyzerTest<TAnalyzer, TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TVerifier : IVerifier, new()
{
    public AnalyzerFixure()
    {
        TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(ProtoContractAttribute).Assembly.Location));
        TestState.ReferenceAssemblies = new ReferenceAssemblies("net9.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "9.0.0"), Path.Combine("ref", "net9.0"));
    }
}
