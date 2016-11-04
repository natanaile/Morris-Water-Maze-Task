/// <summary>
/// represent a <see cref="UnityEngine.Vector3"/> using doubles instead of floats.
/// </summary>
public class Vector3Double
{
	/// <summary>
	/// The vector coordinates.
	/// </summary>
	public double x, y, z;

	/// <summary>
	/// constructor
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
    public Vector3Double (double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
