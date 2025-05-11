using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Graphics.Stubs;

// even though they are unused, these events need to be declared for MonoGame API compatibility
#pragma warning disable CS0067
/// <summary>
/// Graphics device service to use with SDL's dummy video driver
/// </summary>
public class DummyGraphicsDeviceService : IGraphicsDeviceService {
	public GraphicsDevice? GraphicsDevice { get; } = null;
	public event EventHandler<EventArgs>? DeviceCreated;
	public event EventHandler<EventArgs>? DeviceDisposing;
	public event EventHandler<EventArgs>? DeviceReset;
	public event EventHandler<EventArgs>? DeviceResetting;
}
#pragma warning restore CS0067
