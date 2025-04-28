using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Wrapper class for <see cref="Texture2D"/>
/// </summary>
public class Texture2DAdapter : ITexture2D {
	protected readonly Texture2D baseTexture;

	public int Width => baseTexture.Width;
	public int Height => baseTexture.Height;
	public Rectangle Bounds => baseTexture.Bounds;

	public Texture2DAdapter(Texture2D baseTexture) {
		ArgumentNullException.ThrowIfNull(baseTexture);

		this.baseTexture = baseTexture;
	}

	public static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path, Action<byte[]> colorProcessor)
		=> new Texture2DAdapter(Texture2D.FromFile(graphicsDevice, path, colorProcessor));

	public static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path)
		=> new Texture2DAdapter(Texture2D.FromFile(graphicsDevice, path));

	public static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor)
		=> new Texture2DAdapter(Texture2D.FromStream(graphicsDevice, stream, colorProcessor));

	public static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		=> new Texture2DAdapter(Texture2D.FromStream(graphicsDevice, stream));

	public void GetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		=> baseTexture.GetData(level, arraySize, rect, data, startIndex, elementCount);

	public void Reload(Stream textureStream)
		=> baseTexture.Reload(textureStream);

	public void SaveAsJpeg(Stream stream, int width, int height)
		=> baseTexture.SaveAsJpeg(stream, width, height);

	public void SaveAsPng(Stream stream, int width, int height)
		=> baseTexture.SaveAsPng(stream, width, height);

	public void SetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		=> baseTexture.SetData(level, arraySize, rect, data, startIndex, elementCount);

	public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		=> baseTexture.SetData(level, rect, data, startIndex, elementCount);

	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		=> baseTexture.SetData(data, startIndex, elementCount);

	public void SetData<T>(T[] data) where T : struct
		=> baseTexture.SetData(data);

	public virtual void Dispose() => baseTexture.Dispose();

	public Texture2D ToTexture2D() => baseTexture;

	public static explicit operator Texture2DAdapter(Texture2D baseTexture) => new(baseTexture);
	public static explicit operator Texture2D(Texture2DAdapter adapter) => adapter.baseTexture;
}
