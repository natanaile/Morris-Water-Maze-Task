using System;
using UnityEngine;

/// <summary>
/// A set of accelerometer data from an ArduIMU
/// </summary>
public class AccelSensorPacket : BasicSensorPacket
{
	/// <summary>
	/// ArduIMU samples are multiplied by a scale factor so that they can be sent as fixed-point numbers. 
	/// This makes transmission cheaper because fewer bytes need to be sent.
	/// </summary>
	public const double SCALE_FACTOR = 100.0f;
	
	/// <summary>
	/// acceleration data from the IMU
	/// </summary>
	private Vector3Double _accelerationDouble;

	/// <summary>
	/// get the acceleration as a <see cref="Vector3Double"/>
	/// </summary>
	public Vector3Double linearAccelerationDouble
	{
		get { return _accelerationDouble; }
		private set { _accelerationDouble = value; }
	}

	/// <summary>
	/// get the linear acceleration (x, y, z) reported by the ArduIMU as a regular <see cref="Vector3"/>.
	/// </summary>
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
	/// Constructor (from an array of strings, if data was transmitted in a comma-separated string format.
	/// </summary>
	/// <param name="fields">string data that was sent from ArduIMU</param>
	public AccelSensorPacket(string[] fields)
		: base(fields)
	{
		float x, y, z;
		x = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 0]);
		y = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 1]);
		z = ByteArrayHelpers.RawDataToFloat(fields[PAYLOAD_OFFSET + 2]);

		linearAccelerationDouble = new Vector3Double(x, y, z);
	}

	/// <summary>
	/// Constructor (from a byte array, if data was transmitted as raw bytes)
	/// </summary>
	/// <param name="rawData">byte-representation of the packet.</param>
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
