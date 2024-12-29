# NativeSharpZlib

[![Nuget](https://img.shields.io/nuget/v/NativeSharpZlib?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/NativeSharpZlib/)
[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112/NativeSharpZlib?include_prereleases&style=for-the-badge&logo=github)](https://github.com/BigBang1112/NativeSharpZlib/releases)
[![Code Coverage](https://img.shields.io/badge/Code%20Coverage-91%25-success?style=for-the-badge)](https://github.com/BigBang1112/NativeSharpZlib)

A simple zlib wrapper with NativeAOT support and transparent C lib build.

Another zlib wrapper for C#, as there wasn't one where you'd be able to set concrete de/compress block sizes, which is crucial for decompressing certain encrypted content.

Current zlib version: **1.2.5** (eventually to be updated)

Supported runtimes:
- `win-x64`
- `win-x86`
- `linux-x64`
- `linux-arm`
- `linux-arm64`
- `browser-wasm`

## Cross-platform caveats

When cross-compiling zlib, the `z_stream` struct becomes different size per platform, due to usage of `uLong` which is 4 bytes on Windows and WebAssembly. Therefore, the project had to be split to `netX.0-windows`, `netX.0-browser`, and `netX.0`. To use this package on Windows or WebAssembly, you must specify the platform-specific framework in your project (you can still multi-target though).

Be aware that this **does not currently work at all for Blazor Web App** project type, due to the `.Client` part not supporting multitargetting. You can use it only on standalone Blazor WebAssembly once specifying `netX.0-browser` target framework.

Also in case of Blazor, you may also need to add this NuGet package explicitly to the running project, even when it has been referenced by a different NuGet package you're using.

## Usage

The default `netX.0` is only compatible with UNIX-based OS. You have to multi-target if you want compatibility with Windows:

```xml
<PropertyGroup>
    <TargetFrameworks>net9.0-windows;net9.0</TargetFrameworks>
</PropertyGroup>
```

Example C# code:

```cs
var data = Encoding.UTF8.GetBytes("Hello, World!");

using var ms = new MemoryStream();
using (var compressStream = new NativeZlibStream(ms, CompressionMode.Compress, leaveOpen: true))
{
    compressStream.Write(data, 0, data.Length);
}

ms.Position = 0;

using var decompressStream = new NativeZlibStream(ms, CompressionMode.Decompress);
var buffer = new byte[256];
var read = decompressStream.Read(buffer, 0, buffer.Length);

Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, read));
```

You can also specify block sizes and compression level in `NativeZlibOptions`.

```cs
using var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, new NativeZlibOptions()
{
    CompressedBlockSize = 512,
    UncompressedBlockSize = 2048,
    CompressionLevel = 9,
});
```
