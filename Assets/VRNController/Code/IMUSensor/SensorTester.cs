using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;

/// <summary>
/// Visualize an ArduIMU and its motion (rotation and acceleration)
/// </summary>
/// <seealso cref="UnityEngine.MonoBehaviour"/>
public class SensorTester : MonoBehaviour
{
	//-------------------
	// Assign in Editor
	//-------------------
	/// <summary>
	/// The serial port
	/// </summary>
	public string serialPort = "COM6";
	/// <summary>
	/// The baud rate
	/// </summary>
	public int baudRate = 115200;
	/// <summary>
	/// The sensor identifier for the monitored ArduIMU
	/// </summary>
	public int sensorID = 0;

	/// <summary>
	/// useful information
	/// </summary>
	public Text info;

	/// <summary>
	/// The serial port name
	/// </summary>
	public Text serialPortText;
	/// <summary>
	/// The baudrate
	/// </summary>
	public Text baudrateText;
	/// <summary>
	/// log data
	/// </summary>
	public PositionTracker tracker;
	/// <summary>
	/// The messages
	/// </summary>
	public Text messages;

	/// <summary>
	/// The orientation model
	/// </summary>
	public GameObject orientationModel;

	/// <summary>
	/// graphical representations of each axis
	/// </summary>
	public GameObject axisX, axisY, axisZ;
	
	/// <summary>
	/// graphical represenations of each unwound axis
	/// </summary>
	public GameObject unwoundAxisX, unwoundAxisY, unwoundAxisZ;


	/// <summary>
	/// The dummy
	/// </summary>
	public GameObject dummy;

	private long[] timeBuffer = new long[20];
	private int currentBufferLocation;

	private bool isRunning;
	SensorReader reader;
	// Use this for initialization
	void Start()
	{
		////string bytes = "1c7d3f";
		////Debug.Log("bytes: " + bytes);
		////Debug.Log("float: " + BasicSensorPacket.RawDataToFloat(bytes));
		////buf = new CircularFloatBuffer(10);
		//Vector3 test1 = new Vector3(0, 1, 0);
		//Quaternion rotation1 = new Quaternion(
		//	0,
		//	0,
		//	0.70710678f,
		//	0.70710678f);

		//Vector3 result1 = QuatRotate(test1, rotation1);

		//Debug.Log("QuatRotateTest: (s.b -1, 0, 0)" + result1.x + ", " + result1.y + ", " + result1.z);

		//Vector3 test2 = new Vector3(0, 1, 0);
		//Quaternion rotation2 = new Quaternion(
		//	0.70710678f, 
		//	0,
		//	0,
		//	0.70710678f);

		//Vector3 result2 = QuatRotate(test2, rotation2);

		//Debug.Log("QuatRotateTest: (s.b 0, 0, 1)" + result2.x + ", " + result2.y + ", " + result2.z);
	}

	//CircularFloatBuffer buf;

	// Update is called once per frame
	void Update()
	{
		if (reader != null)
		{
			IMUSensorPacket imuPacket = reader.GetLastPacket(sensorID, PacketVariant.IMU_DATA) as IMUSensorPacket;
			if (null != imuPacket)
			{
				long lastLatency = reader.GetLastLatency(sensorID, PacketVariant.IMU_DATA);
				if (lastLatency > 0)
				{
					timeBuffer[currentBufferLocation] = lastLatency;
					currentBufferLocation += 1;
					if (currentBufferLocation >= timeBuffer.Length)
					{
						currentBufferLocation = 0;
					}
					long avgTimeMillis = 0;
					for (int index = 0; index < timeBuffer.Length; index++)
					{
						long x = timeBuffer[index];
						avgTimeMillis += x;
					}
					avgTimeMillis /= timeBuffer.Length;

					orientationModel.transform.localRotation = imuPacket.orientation;

					if (info != null)
					{
						string outputData = "X: " + imuPacket.orientation.x;
						outputData += "\nY: " + imuPacket.orientation.y;
						outputData += "\nZ: " + imuPacket.orientation.z;
						outputData += "\nW: " + imuPacket.orientation.w;
						outputData += "\nT(avg) " + avgTimeMillis + " ms";

						info.text = outputData;

						//Vector3 eulers = imuPacket.orientation.eulerAngles;
						//float angle;
						//Vector3 axis;
						//imuPacket.orientation.ToAngleAxis(out angle, out axis);
						//string outputData = "X: " + eulers.x;
						//outputData += "\nY: " + eulers.y;
						//outputData += "\nZ: " + eulers.z;

						//outputData += "\n\nAngle: " + angle;
						//outputData += "\nAxis: " + axis;

						//info.text = outputData;
					}
				}

			}

			AccelSensorPacket accelPacket = reader.GetLastPacket(sensorID, PacketVariant.ACCEL_DATA) as AccelSensorPacket;
			if (null != accelPacket)
			{
				//float mag = 1f + accelPacket.linearAcceleration.magnitude;

				Vector3 xVector = new Vector3(
					1f,
					1f,
					accelPacket.linearAcceleration.x
					);

				Vector3 yVector = new Vector3(
					1f,
					1f,
					accelPacket.linearAcceleration.y
					);

				Vector3 zVector = new Vector3(
					1f,
					1f,
					accelPacket.linearAcceleration.z
					);

				axisX.transform.localScale = xVector;
				axisY.transform.localScale = yVector;
				axisZ.transform.localScale = zVector;
			}


			if (null != accelPacket && null != imuPacket)
			{
				Vector3 accelVector = accelPacket.linearAcceleration;

				Quaternion orientationConj = new Quaternion(
					-imuPacket.orientation.x,
					imuPacket.orientation.y,
					-imuPacket.orientation.z,
					imuPacket.orientation.w);

				Vector3 corrections = QuatRotate(accelVector, orientationConj);


				Vector3 xVector = new Vector3(
					1f,
					1f,
					corrections.x
					);

				Vector3 yVector = new Vector3(
					1f,
					1f,
					corrections.y
					);

				Vector3 zVector = new Vector3(
					1f,
					1f,
					corrections.z
					);

				unwoundAxisX.transform.localScale = xVector;
				unwoundAxisY.transform.localScale = yVector;
				unwoundAxisZ.transform.localScale = zVector;
			}


		}
	}

	/// <summary>
	/// Engages this sensor tester.
	/// </summary>
	public void Engage()
	{
		if (!isRunning)
		{
			if (null != serialPortText && null != baudrateText)
			{
				try
				{
					this.baudRate = Convert.ToInt32(baudrateText.text);
					this.serialPort = serialPortText.text;

				}
				catch (Exception)
				{
					// invalid number
				}
			}
			StartReader();
			if (tracker != null)
			{
				string filePath = Application.persistentDataPath + "\\cubes";
				string fileName = "orientationData";
				if (tracker.StartLogging(filePath, fileName, 20, true, DataLogger.FileCollisionHandlerMode.FILENAME_VERSION_NUMBER))
				{
					messages.text = "start logging to " + filePath + "\\" + fileName;
				}
				else
				{
					messages.text = "can't start logging to " + filePath + "\\" + fileName;
				}
			}
		}
	}

	/// <summary>
	/// Terminates this sensor test.
	/// </summary>
	public void Terminate()
	{
		if (isRunning)
		{
			isRunning = false;
			if (null != reader)
			{
				reader.Close();
			}

			if (null != tracker)
			{
				if (tracker.StopLogging())
				{
					messages.text = "finished logging";
				}
				else
				{
					messages.text = "failed to finish logging";
				}
			}

		}
	}

	/// <summary>
	/// Starts the <see cref="SensorReader" />.
	/// </summary>
	public void StartReader()
	{
		isRunning = true;
		try
		{
			reader = new SensorReader(serialPort, baudRate);
			reader.OpenSerialPort();
		}
		catch (IOException)
		{
			Debug.Log("Could not connect to serial port at " + serialPort + ". Ensure that it is less than COM9.");
			reader = null;
		}

	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed
	/// </summary>
	public void OnDestroy()
	{
		if (tracker != null)
		{
			tracker.StopLogging();
		}

		if (reader != null)
		{
			reader.Close();
		}
	}

	/// <summary>
	/// rotate a vector according to a quaternion rotation
	/// </summary>
	/// <param name="vector">vector to rotate</param>
	/// <param name="quaternion">amount by which to rotate</param>
	/// <returns>rotated vector</returns>
	public static Vector3 QuatRotate(Vector3 vector, Quaternion quaternion)
	{
		Vector3 v = vector;
		Vector3 r = new Vector3(quaternion.x, quaternion.y, quaternion.z);
		float w = quaternion.w;

		Vector3 rXv = Vector3.Cross(r, v);
		Vector3 wv = v * w;
		Vector3 rvwv = rXv + wv;

		Vector3 result = v + 2 * Vector3.Cross(r, rvwv);


		return result;
	}
}
