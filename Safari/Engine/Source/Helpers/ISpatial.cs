namespace Engine.Helpers;

/// <summary>
/// Interface for objects that can be represented with a floating point precision AABB
/// </summary>
public interface ISpatial {
	/// <summary>
	/// The AABB of the object
	/// </summary>
	public Vectangle Bounds { get; }
}
