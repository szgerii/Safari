#pragma warning disable IDE0060
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Stub that can be used as a placeholder for <see cref="RenderTarget2D"/>s
/// </summary>
public class NoopRenderTarget2D : NoopTexture2D, IRenderTarget2D {
	public RenderTargetUsage RenderTargetUsage { get; set; }

	public DepthFormat DepthStencilFormat { get; set; }

	public int MultiSampleCount { get; set; }

// ignore obsolete warning (needed for MonoGame compatibility)
#pragma warning disable CS0067
	[Obsolete("This is provided for XNA compatibility only and is never called by MonoGame")]
	public event EventHandler<EventArgs>? ContentLost;
#pragma warning restore CS0067

	public NoopRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
		: base(graphicsDevice, width, height, mipMap, preferredFormat) {
		DepthStencilFormat = preferredDepthFormat;
		RenderTargetUsage = usage;
		MultiSampleCount = preferredMultiSampleCount;
	}

	public NoopRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1) { }

	public NoopRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, false) { }

	public NoopRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) { }

	public NoopRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
		: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) { }

	public RenderTarget2D ToRenderTarget2D() {
		throw new NoopOverreachException();
	}
}
