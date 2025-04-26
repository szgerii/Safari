using System;

namespace Engine.Graphics.Stubs;

/// <summary>
/// Exception thrown if somebody tries to use a method/property outside of the noop stub's intended functionality
/// </summary>
public class NoopOverreachException : Exception {
	public NoopOverreachException() : base() { }
	public NoopOverreachException(string message) : base(message) { }
	public NoopOverreachException(string message, Exception inner) : base(message, inner) { }
}
