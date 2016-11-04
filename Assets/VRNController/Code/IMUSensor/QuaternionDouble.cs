/// <summary>
/// Represent a <see cref="UnityEngine.Quaternion"/> with doubles instead of floats.
/// </summary>
public class QuaternionDouble
{

	/// <summary>
	/// The quaternion coordinates.
	/// </summary>
	public double x, y, z, w;

	/// <summary>
	/// constructor
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="w"></param>
	public QuaternionDouble(double x, double y, double z, double w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
}
