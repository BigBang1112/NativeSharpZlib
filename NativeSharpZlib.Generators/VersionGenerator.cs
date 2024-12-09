using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace NativeSharpZlib.Generators;

[Generator]
public class FieldsGenerator : IIncrementalGenerator
{
    private const bool Debug = false;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (Debug && !Debugger.IsAttached)
        {
            Debugger.Launch();
        }

        var assemblies = context.CompilationProvider
            .Select((compilation, token) =>
            {
                return compilation.Assembly;
            });

        context.RegisterSourceOutput(assemblies, GenerateVersion);
    }

    private void GenerateVersion(SourceProductionContext context, IAssemblySymbol symbol)
    {
        var version = symbol.Identity.Version.ToString().TrimEnd('.', '0');
        context.AddSource("ZlibNative.Version.cs", $@"
namespace NativeSharpZlib;

internal sealed partial class ZlibNative
{{
    static ZlibNative()
    {{
        Version = ""{version}"";
    }}
}}
");
    }
}
