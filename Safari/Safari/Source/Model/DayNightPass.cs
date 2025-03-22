using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;

namespace Safari.Source.Model;

public class DayNightPass : IPostProcessPass {
	private RenderTarget2D _output = null;
	private Effect dayNightPass = Game.ContentManager.Load<Effect>("Fx/dayNightPass");

	RenderTarget2D IPostProcessPass.Output => _output;
	Effect IPostProcessPass.Shader => dayNightPass;

	public void PreDraw(GameTime gameTime) {
		EnsureCorrectRT();
		dayNightPass.Parameters["Time"].SetValue((float)GameScene.Active.Model.TimeOfDay);
	}

	private void EnsureCorrectRT() {
		if (_output == null || Game.RenderTarget.Width != _output.Width || Game.RenderTarget.Height != _output.Height) {
			_output = new RenderTarget2D(Game.Graphics.GraphicsDevice, Game.RenderTarget.Width, Game.RenderTarget.Height);
		}
	}
}
