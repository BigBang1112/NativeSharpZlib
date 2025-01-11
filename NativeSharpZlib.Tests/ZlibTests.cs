using System.IO.Compression;
using System.Text;

namespace NativeSharpZlib.Tests;

public class ZlibTests
{
    [Fact]
    public void Test()
    {
        using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        using (var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            zlibStream.Write(data, 0, data.Length);
        }
        stream.Position = 0;
        using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress);
        var buffer = new byte[256];
        var read = zlibStream2.Read(buffer, 0, buffer.Length);
        Assert.Equal(data, buffer.Take(read));
    }

    [Fact]
    public async Task TestAsync()
    {
        using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        using (var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            await zlibStream.WriteAsync(data, 0, data.Length);
        }
        stream.Position = 0;
        using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress);
        var buffer = new byte[256];
        var read = await zlibStream2.ReadAsync(buffer, 0, buffer.Length);
        Assert.Equal(data, buffer.Take(read));
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task TestModernAsync()
    {
        await using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        await using (var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            await zlibStream.WriteAsync(data);
        }
        stream.Position = 0;
        await using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress);
        var buffer = new byte[256];
        var read = await zlibStream2.ReadAsync(buffer);
        Assert.Equal(data, buffer.Take(read));
    }
#endif

    [Fact]
    public void TestFlush()
    {
        using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        using var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true);
        zlibStream.Write(data, 0, data.Length);
        zlibStream.Flush();
        stream.Position = 0;
        using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress, leaveOpen: true);
        var buffer = new byte[256];
        var read = zlibStream2.Read(buffer, 0, buffer.Length);
        Assert.Equal(data, buffer.Take(read));
    }

    [Fact]
    public async Task TestFlushAsync()
    {
        using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        using var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true);
        await zlibStream.WriteAsync(data, 0, data.Length);
        await zlibStream.FlushAsync();
        stream.Position = 0;
        using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress, leaveOpen: true);
        var buffer = new byte[256];
        var read = await zlibStream2.ReadAsync(buffer, 0, buffer.Length);
        Assert.Equal(data, buffer.Take(read));
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task TestModernFlushAsync()
    {
        await using var stream = new MemoryStream();
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        await using var zlibStream = new NativeZlibStream(stream, CompressionMode.Compress, leaveOpen: true);
        await zlibStream.WriteAsync(data);
        await zlibStream.FlushAsync();
        stream.Position = 0;
        await using var zlibStream2 = new NativeZlibStream(stream, CompressionMode.Decompress, leaveOpen: true);
        var buffer = new byte[256];
        var read = await zlibStream2.ReadAsync(buffer);
        Assert.Equal(data, buffer.Take(read));
    }
#endif
}