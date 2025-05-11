using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Engine.Graphics.Stubs.Texture;

/// <summary>
/// Stub that can be used as a placeholder for <see cref="Texture2D"/>s
/// </summary>
public class NoopTexture2D : ITexture2D {
	/// <summary>
	/// An empty noop texture (width, height = -1)
	/// </summary>
	public static NoopTexture2D Empty => new();
	private Color[]? data;

	public int Width { get; private set; }
	public int Height { get; private set; }
	public Rectangle Bounds => new(0, 0, Width, Height);

	protected NoopTexture2D() {
		Width = -1;
		Height = -1;
		data = null;
	}

	private NoopTexture2D(int width, int height) {
		if (width <= 0 || height <= 0) {
			throw new ArgumentException("Width or height smaller than or equal to zero");
		}

		Width = width;
		Height = height;
		data = new Color[width * height];
	}

	public NoopTexture2D(GraphicsDevice? graphicsDevice, int width, int height) : this(width, height) { }
	public NoopTexture2D(GraphicsDevice? graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : this(width, height) { }
	public NoopTexture2D(GraphicsDevice? graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : this(width, height) { }

	public static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path, Action<byte[]> colorProcessor) => new NoopTexture2D();
	public static ITexture2D FromFile(GraphicsDevice graphicsDevice, string path) => new NoopTexture2D();

	public static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor) => new NoopTexture2D();
	public static ITexture2D FromStream(GraphicsDevice graphicsDevice, Stream stream) => new NoopTexture2D();

	public void GetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct { }
	public void GetData(out Color[] data) {
		data = this.data ?? [];
	}

	public void Reload(Stream textureStream) { }

	public void SaveAsJpeg(Stream stream, int width, int height) { }
	public void SaveAsPng(Stream stream, int width, int height) { }

	public void SetData<T>(int level, int arraySize, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct { }
	public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct { }
	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct { }
	public void SetData<T>(T[] data) where T : struct { }

	public void SetData(Color[] data) {
		this.data = data;
	}

	public void Dispose() {
		data = null;
	}

	public Texture2D ToTexture2D() {
		throw new NoopOverreachException();
	}
}
