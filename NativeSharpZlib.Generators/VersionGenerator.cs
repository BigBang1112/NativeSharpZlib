using Microsoft.CodeAnalysis;

namespace NativeSharpZlib.Generators;

[Generator]
public class FieldsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblies = context.AnalyzerConfigOptionsProvider
            .Select((compilation, token) =>
            {
                if (compilation.GlobalOptions.TryGetValue("build_property.ZlibVersion", out var version))
                {
                    return version;
                }

                return "1.2.5";
            });

        context.RegisterSourceOutput(assemblies, GenerateVersion);
    }

    private void GenerateVersion(SourceProductionContext context, string version)
    {
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
