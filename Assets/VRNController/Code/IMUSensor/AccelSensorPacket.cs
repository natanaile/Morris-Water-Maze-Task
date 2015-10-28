using System;
using UnityEngine;

/// <summary>
/// Data from an Acceleration sensor
/// </summary>
public class AccelSensorPacket : BasicSensorPacket
{
	public const double SCALE_FACTOR = 100.0f;
	
	/// <summary>
	/// acceleration data from the IMU
	/// </summary>
	private Vector3Double _accelerationDouble;

	/// <summary>
	/// get the acceleration as a Vector3Double
	/// </summary>
	public Vector3Double linearAccelerationDouble
	{
		get { return _accelerationDouble; }
		private set { _accelerationDouble = value; }
	}

	public Vector3 linearAcceleration
	{
		get
		{
			return new Vector3(
			(float)linearAccelerationDouble.x,
			(float)linearAccelerationDouble.y,
			(float)linearAccelerationDouble.z
			);
		}
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public AccelSensorPacket(string[] fields)
		: base(fields)
	{
		float x, y, z;
		x = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 0]);
		y = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 1]);
		z = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 2]);

		linearAccelerationDouble = new Vector3Double(x, y, z);
	}

	public AccelSensorPacket(byte[] rawData)
		: base(rawData)
	{
		byte[] xBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 0 * 2, xBytes, 0, xBytes.Length);

		byte[] yBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 1 * 2, yBytes, 0, yBytes.Length);

		byte[] zBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 2 * 2, zBytes, 0, zBytes.Length);

		double x = -1f, y = -1f, z = -1f;

		try
		{
			x = (double)ByteArrayHelpers.GetSignedIntFromBytes(xBytes, false) / SCALE_FACTOR;
			y = (double)ByteArrayHelpers.GetSignedIntFromBytes(yBytes, false) / SCALE_FACTOR;
			z = (double)ByteArrayHelpers.GetSignedIntFromBytes(zBytes, false) / SCALE_FACTOR;
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}

		linearAccelerationDouble = new Vector3Double(x, y, z);
	}

}
