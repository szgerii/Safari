using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Interface isolation of <see cref="RenderTarget2D"/>'s API
/// </summary>
public interface IRenderTarget2D : ITexture2D {
	/// <summary>
	/// Retrieves the underlying <see cref="RenderTarget2D"/> instance if possible, or throws a <see cref="NoopOverreachException"/> <br/>
	/// Only call this if you're sure you're not in headless mode/not dealing with a noop instance
	/// </summary>
	/// <returns>The underlying render target</returns>
	/// <exception cref="NoopOverreachException"></exception>
	public RenderTarget2D ToRenderTarget2D();

	/// <summary>
	/// Determines how the render target handles clearing itself
	/// </summary>
	public RenderTargetUsage RenderTargetUsage { get; }

	/// <summary>
	/// The depth-stencil buffer's format
	/// </summary>
	public DepthFormat DepthStencilFormat { get; }
	public int MultiSampleCount { get; }

	[Obsolete("This is provided for XNA compatibility only and will always return false")]
	public bool IsContentLost => false;
	public event EventHandler<EventArgs> ContentLost;
}
