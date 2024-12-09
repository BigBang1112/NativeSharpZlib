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
}