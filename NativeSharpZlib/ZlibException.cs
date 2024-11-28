namespace NativeSharpZlib;

[Serializable]
public class ZlibException : Exception
{
	public ZlibException() { }
	public ZlibException(string message) : base(message) { }
	public ZlibException(string message, Exception inner) : base(message, inner) { }
}
