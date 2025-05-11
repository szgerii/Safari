using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Interfaces;

/// <summary>
/// Interface for a pass/step in post processing
/// All post process draws are assumed to be fullscreen, one draw call, one shader, one technique draws
/// </summary>
public interface IPostProcessPass {
	
	/// <summary>
	/// The output texture that gets sent to the next step in post-processing, 
	/// or to the final draw to the screen (if this is the last step)
	/// </summary>
	public IRenderTarget2D? Output { get; }

	/// <summary>
	/// The shader that the pass uses (this is where the output of the previous step is uploaded to)
	/// </summary>
	public Effect? Shader { get; }

	/// <summary>
	/// Use this function to set uniforms, prepare the Output texture, etc
	/// After calling this function, the fullscreen buffers, rendertarget and the effect
	/// are bound and a draw call is performed.
	/// Beyond the uniforms set here, the output of the previous step is also bound
	/// with the name 'PrevStep'
	/// </summary>
	/// <param name="device">The device used for drawing (for easier access)</param>
	/// <param name="gameTime">XNA gametime</param>
	/// <param name="input">The result of the previous step in drawing</param>
	public void PreDraw(GameTime gameTime);
}
