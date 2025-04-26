using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Graphics.Stubs;

/// <summary>
/// Graphics device service to use with SDL's dummy video driver
/// </summary>
public class DummyGraphicsDeviceService : IGraphicsDeviceService {
	public GraphicsDevice GraphicsDevice { get; } = null;
	public event EventHandler<EventArgs> DeviceCreated;
	public event EventHandler<EventArgs> DeviceDisposing;
	public event EventHandler<EventArgs> DeviceReset;
	public event EventHandler<EventArgs> DeviceResetting;
}
