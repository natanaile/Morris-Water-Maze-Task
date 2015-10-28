using System;
using System.Runtime.InteropServices;

/// <summary>
/// A 3D point with double components.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct VRNPoint3
{
	public double x, y, z;

	public VRNPoint3(double _x, double _y, double _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	/// <summary>
	/// Compute the Euclidean distance to another point
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public double ComputeDistance(VRNPoint3 other)
	{
		double dist = 0.0;
		if (!other.Equals(null))
		{
			double diffX = this.x - other.x;
			double diffY = this.y - other.y;
			double diffZ = this.z - other.z;

			double sqr = diffX * diffX + diffY * diffY + diffZ * diffZ;

			dist = Math.Sqrt(sqr);
		}

		return dist;
	}
};