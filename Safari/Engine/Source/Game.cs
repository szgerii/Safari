using Engine.Graphics;
using Engine.Graphics.Stubs;
using Engine.Graphics.Stubs.Texture;
using Engine.Input;
using Engine.Interfaces;
using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace Engine;

public class Game : Microsoft.Xna.Framework.Game {
	public static Game? Instance { get; protected set; }
	public static GraphicsDeviceManager? Graphics { get; protected set; }
	public static SpriteBatch? SpriteBatch { get; protected set; }
	public static IRenderTarget2D? RenderTarget { get; protected set; }
	public static ContentManager ContentManager => Instance!.Content;
	/// <summary>
	/// The seed used for initalizing <see cref="Random"/> <br/>
	/// NOTE: this does not reseed the random, it only takes effect if it is set before <see cref="Initialize"/> is called
	/// </summary>
	public static int? Seed { get; set; }
	public static Random? Random { get; set; }

	public Point BaseResolution { get; protected set; }

	private VertexBuffer? fullScreenVbo;
	private IndexBuffer? fullScreenIbo;

	/// <summary>
	/// Whether the game is running in headless mode (without interacting with any graphics API)
	/// </summary>
	public bool IsHeadless { get; private init; }
	public static bool CanDraw => Instance != null && !Instance.IsHeadless;

	public static float RenderTargetScale {
		get {
			if (Graphics == null || RenderTarget == null) {
				throw new InvalidOperationException("Cannot calculate render target scale before the game has been completely initialized");
			}

			float scaleX = Graphics.PreferredBackBufferWidth / (float)RenderTarget.Width;
			float scaleY = Graphics.PreferredBackBufferHeight / (float)RenderTarget.Height;

			return Math.Min(scaleX, scaleY);
		}
	}

	#region SHORTHANDS
	public static void AddObject(GameObject obj) => SceneManager.Active!.AddObject(obj);
	public static void RemoveObject(GameObject obj) => SceneManager.Active!.RemoveObject(obj);
	#endregion

	public static ITexture2D LoadTexture(string path) {
		return CanDraw ? (Texture2DAdapter)ContentManager.Load<Texture2D>(path) : NoopTexture2D.Empty;
	}

	protected GraphicsDeviceManager? _graphics;
	protected SpriteBatch? _spriteBatch;

	public Game(bool headless = false) {
		IsHeadless = headless;
		Instance = this;

		if (IsHeadless) {
			if (Services.GetService(typeof(IGraphicsDeviceService)) != null) {
				Services.RemoveService(typeof(IGraphicsDeviceService));
			}

			Services.AddService(typeof(IGraphicsDeviceService), new DummyGraphicsDeviceService());
		} else {
			_graphics = new GraphicsDeviceManager(this);
			Graphics = _graphics;
		}
	}

	private void InitFullscreenBuffers() {
		fullScreenVbo = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
		fullScreenIbo = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
		int[] indices = new int[6] { 0, 1, 2, 2, 3, 0 };
		Vector3 bottomLeft = new Vector3(-1, -1, 0);
		Vector2 texBottomLeft = new Vector2(0, 1);
		Vector3 bottomRight = new Vector3(1, -1, 0);
		Vector2 texBottomRight = new Vector2(1, 1);
		Vector3 topLeft = new Vector3(-1, 1, 0);
		Vector2 texTopLeft = new Vector2(0, 0);
		Vector3 topRight = new Vector3(1, 1, 0);
		Vector2 texTopRight = new Vector2(1, 0);
		VertexPositionTexture[] verts = new VertexPositionTexture[4] {
			new VertexPositionTexture(topLeft, texTopLeft),
			new VertexPositionTexture(topRight, texTopRight),
			new VertexPositionTexture(bottomRight, texBottomRight),
			new VertexPositionTexture(bottomLeft, texBottomLeft)
		};
		fullScreenIbo.SetData(indices);
		fullScreenVbo.SetData(verts);
	}

	protected override void Initialize() {
		InputManager.Initialize();
		Content.RootDirectory = "Content";

		if (!IsHeadless) {
			DisplayManager.Init();
			DisplayManager.SetResolution(1280, 720);
			RenderTarget = new RenderTarget2DAdapter(new(GraphicsDevice, BaseResolution.X, BaseResolution.Y));
			InitFullscreenBuffers();
		} else {
			RenderTarget = new NoopRenderTarget2D(GraphicsDevice, BaseResolution.X, BaseResolution.Y);
		}

		Random ??= Seed != null ? new(Seed.Value) : new();

		if (IsHeadless) {
			// make sure the Game class doesn't try to initialize its graphics components later on
			FieldInfo fi = typeof(Microsoft.Xna.Framework.Game).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance)!;
			fi.SetValue(this, true);
		} else {
			base.Initialize();
		}
	}

	protected override void LoadContent() {
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		SpriteBatch = _spriteBatch;
	}

	protected override void Dispose(bool disposing) {
		SceneManager.UnloadActive();
		Random = null;
		
		base.Dispose(disposing);
	}

	protected override void Update(GameTime gameTime) {
		InputManager.Update(gameTime);

		if (SceneManager.HasLoadingScheduled) {
			SceneManager.PerformScheduledLoad();
		}

		SceneManager.Active?.PerformObjectRemovals();
		SceneManager.Active?.PerformObjectAdditions();

		SceneManager.Active?.Update(gameTime);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime) {
		if (Graphics == null || RenderTarget == null || SpriteBatch == null) {
			throw new InvalidOperationException("Cannot Draw before the game has been properly initialized");
		}

		float scale = RenderTargetScale;

		float diffX = Graphics.PreferredBackBufferWidth - scale * RenderTarget.Width;
		float diffY = Graphics.PreferredBackBufferHeight - scale * RenderTarget.Height;

		Vector2 offset = Vector2.Zero;
		if (diffX != 0) {
			offset.X = diffX / 2;
		}
		if (diffY != 0) {
			offset.Y = diffY / 2;
		}

		GraphicsDevice.SetRenderTarget(RenderTarget.ToRenderTarget2D());
		GraphicsDevice.Clear(Color.Black);

		Matrix trMatrix = (Camera.Active != null) ? Camera.Active.TransformMatrix : Matrix.Identity;
		SpriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			samplerState: SamplerState.PointClamp,
			transformMatrix: trMatrix
		);
		SceneManager.Active?.Draw(gameTime);
		SpriteBatch.End();

		// Perform post processing
		Texture2D result = RenderTarget.ToRenderTarget2D();
		if (SceneManager.Active != null) {
			foreach (IPostProcessPass pass in SceneManager.Active.PostProcessPasses) {
				// Let the pass prepare its uniforms and output texture
				pass.PreDraw(gameTime);
				// Set RT
				GraphicsDevice.SetRenderTarget(pass.Output!.ToRenderTarget2D());
				// bind ibo, vbo for fullscreen drawing
				GraphicsDevice.Indices = fullScreenIbo;
				GraphicsDevice.SetVertexBuffer(fullScreenVbo);
				// upload prev pass to the shader and bind it
				pass.Shader!.Parameters["PrevStep"].SetValue(result);
				pass.Shader.CurrentTechnique.Passes[0].Apply();
				// Draw call
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
				result = pass.Output.ToRenderTarget2D();
			}
		}

		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Black);

		SpriteBatch.Begin(
			samplerState: SamplerState.PointClamp,
			transformMatrix: Matrix.CreateTranslation(offset.X, offset.Y, 0)
		);
		SpriteBatch.Draw(result, Vector2.Zero, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		SpriteBatch.End();

		base.Draw(gameTime);
	}
}
