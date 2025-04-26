using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Interface isolation of <see cref="Texture2D"/>'s API
/// </summary>
public interface ITexture2D : IDisposable {
	/// <summary>
	/// Retrieves the underlying <see cref="Texture2D"/> instance if possible, or throws a <see cref="NoopOverreachException"/> <br/>
	/// Only call this if you're sure you're not in headless mode/not dealing with a noop instance
	/// </summary>
	/// <returns>The underlying texture</returns>
	/// <exception cref="NoopOverreachException"></exception>
	public Texture2D ToTexture2D();

	/// <summary>
	/// The bounds of the texture
	/// </summary>
	public Rectangle Bounds { get; }
	/// <summary>
	/// The width of the texture in pixels
	/// </summary>
	public int Width { get; }
	/// <summary>
	/// The height of the texture in pixels
	/// </summary>
	public int Height { get; }

	public void SetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct;
	public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct;
	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct;
	public void SetData<T>(T[] data) where T : struct;

	public void GetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct;
	public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		=> GetData(level, 0, rect, data, startIndex, elementCount);
	public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		=> GetData(0, null, data, startIndex, elementCount);
	public void GetData<T>(T[] data) where T : struct
		=> GetData(0, null, data, 0, data.Length);

	public abstract static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path, Action<byte[]> colorProcessor);
	public abstract static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path);

	public abstract static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor);
	public abstract static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream);

	public void SaveAsJpeg(Stream stream, int width, int height);
	public void SaveAsPng(Stream stream, int width, int height);

	public void Reload(Stream textureStream);
}
