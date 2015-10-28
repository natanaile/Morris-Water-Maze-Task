using System;
using UnityEngine;

/// <summary>
/// Data from an IMU sensor
/// </summary>
public class IMUSensorPacket : BasicSensorPacket
{
	
    public const double SCALE_FACTOR = 16384.0f;
	
	/// <summary>
	/// orientation data from the IMU
	/// </summary>
	private QuaternionDouble _orientationDouble;

	/// <summary>
	/// get the orientation as a Quaternion
	/// </summary>
	public QuaternionDouble orientationDouble
	{
		get { return _orientationDouble; }
		private set { _orientationDouble = value; }
	}

	public Quaternion orientation
	{
		get
		{
			return new Quaternion(
				(float)orientationDouble.x,
				(float)orientationDouble.y,
				(float)orientationDouble.z,
				(float)orientationDouble.w
				);
		}
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public IMUSensorPacket(string[] fields)
		: base(fields)
	{

		float w, x, y, z;
		w = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 0]);
		x = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 1]);
		y = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 2]);
		z = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 3]);

		// normalize
		Vector4 quaternionMag = new Vector4(x, y, z, w);
		quaternionMag = quaternionMag.normalized;

		//orientation = new Quaternion(sensorID, sensorVariant, z, w);
		orientationDouble = new QuaternionDouble(
			quaternionMag.x,
			quaternionMag.y,
			quaternionMag.z,
			quaternionMag.w);
	}

	public IMUSensorPacket(byte[] rawData)
		: base(rawData)
	{
		byte[] wBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 0 * 2, wBytes, 0, wBytes.Length);

		byte[] xBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 1 * 2, xBytes, 0, xBytes.Length);

		byte[] yBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 2 * 2, yBytes, 0, yBytes.Length);

		byte[] zBytes = new byte[2];
		System.Array.Copy(rawData, HEADER_SIZE + 3 * 2, zBytes, 0, zBytes.Length);

		double x = -1f, y = -1f, z = -1f, w = -1f;

		try
		{
			w = (double)ByteArrayHelpers.GetSignedIntFromBytes(wBytes, false) / SCALE_FACTOR;
			x = -(double)ByteArrayHelpers.GetSignedIntFromBytes(xBytes, false) / SCALE_FACTOR;
			y = (double)ByteArrayHelpers.GetSignedIntFromBytes(yBytes, false) / SCALE_FACTOR;
			z = -(double)ByteArrayHelpers.GetSignedIntFromBytes(zBytes, false) / SCALE_FACTOR; // unity is weird... need to flip this 
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}

		orientationDouble = new QuaternionDouble(x, y, z, w);
	}

	public static BasicSensorPacket Initialize(string[] fields)
	{
		return new IMUSensorPacket(fields);
	}

}
