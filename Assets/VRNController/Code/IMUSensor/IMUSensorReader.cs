using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System;
using UnityEngine.UI;
using System.Collections.Generic; // need to enable .NET 2.0

/// <summary>
/// Read input from the ArduIMUs and parse it
/// </summary>
public class IMUSensorReader : SensorReader
{
	public static readonly Quaternion EMPTY_QUATERION = new Quaternion(0, 0, 0, 1.0f);

	//private Quaternion lastOrientation;
	private Dictionary<int, Quaternion> lastOrientations;

	public IMUSensorReader(string serialPortID, int baudrate)
		: base(serialPortID, baudrate)
	{
		lastOrientations = new Dictionary<int, Quaternion>();
	}

	/// <summary>
	/// Sets the 'last' rotation against which subsequent calls of GetRotationChange() will be matched. This allows you to ignore rotation that may have occured since you last called GetRotationChange()
	/// </summary>
	/// <param name="sensorID"></param>
	public void ResetRotation(int sensorID)
	{
		lastOrientations[sensorID] = EMPTY_QUATERION;
	}


	/// <summary>
	/// Get the change in rotation since the last time this function was called.
	/// </summary>
	/// <param name="sensorID"></param>
	/// <returns></returns>
	public float GetRotationChange(int sensorID)
	{
		float rotation = 0f;

		try
		{
			// check if we got 'zeroed'
			if (lastOrientations[sensorID].Equals(EMPTY_QUATERION))
			{
				this.lastOrientations[sensorID] = GetOrientation(sensorID);
			}

			Quaternion currentOrientation = GetOrientation(sensorID);
			Quaternion rotationChange = RelativeOrientation(this.lastOrientations[sensorID], currentOrientation);

			float angle;
			Vector3 axis;
			rotationChange.ToAngleAxis(out angle, out axis);

			// need to project into Vector3.up... for now just assume that axis roughly approximates Vector3.up
			float sign = Math.Sign(axis.z);

			//Debug.Log("x: " + Math.Sign(axis.x) + " y: " + Math.Sign(axis.y) + " z: " + Math.Sign(axis.z));
			rotation = angle * sign;


			// update for next time
			lastOrientations[sensorID] = currentOrientation;
		} catch (KeyNotFoundException ex)
		{
			Debug.Log("Could not find key " + sensorID + " in lastOrientations. Perhaps you have not called ResetRotation for Sensor " + sensorID + ".\n" + ex);
		}

		return rotation;
	}

	/// <summary>
	/// Compute the relative rotation between two quaternions. This is done by computing the conjugate of the reference quaternion and multiplying by the other quaternion.
	/// </summary>
	/// <param name="reference">angle relative to which the result should be calculated.</param>
	/// <param name="other"></param>
	/// <returns>The angle that 'reference' would need to be rotated through to get 'other'</returns>
	private static Quaternion RelativeOrientation(Quaternion reference, Quaternion other)
	{
		Quaternion resultant;

		Quaternion conjugate = new Quaternion
			(
				-reference.x,
				-reference.y,
				-reference.z,
				reference.w
			);

		resultant = other * conjugate;

		return resultant;
	}

	/// <summary>
	/// Get the most recent orientation from a particular sensor, or identity if that sensor has not returned anything yet.
	/// </summary>
	/// <param name="sensorID"></param>
	/// <returns></returns>
	public Quaternion GetOrientation(int sensorID)
	{
		Quaternion q = EMPTY_QUATERION;
		BasicSensorPacket p = GetLastPacket(sensorID, PacketVariant.IMU_DATA);
		if (null != p)
		{
			IMUSensorPacket packet = (IMUSensorPacket)p;
			q = packet.orientation;
		}

		return q;
	}
}
// fin