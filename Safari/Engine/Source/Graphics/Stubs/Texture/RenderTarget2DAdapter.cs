using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Wrapper class for <see cref="RenderTarget2D"/>
/// </summary>
public class RenderTarget2DAdapter : Texture2DAdapter, IRenderTarget2D {
	protected readonly RenderTarget2D baseRT;

	public RenderTargetUsage RenderTargetUsage => baseRT.RenderTargetUsage;
	public DepthFormat DepthStencilFormat => baseRT.DepthStencilFormat;
	public int MultiSampleCount => baseRT.MultiSampleCount;

	public event EventHandler<EventArgs> ContentLost {
		add { baseRT.ContentLost += value; }
		remove { baseRT.ContentLost -= value; }
	}

	public RenderTarget2DAdapter(RenderTarget2D baseRenderTarget) : base(baseRenderTarget) {
		ArgumentNullException.ThrowIfNull(baseRenderTarget);
		baseRT = baseRenderTarget;
	}

	public override void Dispose() {
		baseRT.Dispose();
	}

	public RenderTarget2D ToRenderTarget2D() {
		return baseRT;
	}

	public static explicit operator RenderTarget2DAdapter(RenderTarget2D baseRenderTarget) => new(baseRenderTarget);
	public static explicit operator RenderTarget2D(RenderTarget2DAdapter adapter) => adapter.baseRT;
}
