﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using ProtoBuf;

namespace ProtobufSourceGenerator.Tests.Incremental;

public static class CSharpIncrementalSourceGeneratorVerifier<TIncrementalGenerator> where TIncrementalGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, DefaultVerifier>
    {
        public Test()
        {
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(ProtoContractAttribute).Assembly.Location));
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(GeneratorOptionsAttribute).Assembly.Location));
            TestState.ReferenceAssemblies = new ReferenceAssemblies("net7.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "7.0.0"), Path.Combine("ref", "net7.0"));
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                 compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        protected override IEnumerable<Type> GetSourceGenerators() => [typeof(TIncrementalGenerator)];

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
