using UnityEngine;
using System;

/// <summary>
/// The PositionTracker logs the position and orientation of a GameObject. It may be extended to add additional data fields, and child classes should override GetDataLine() and GetHeaderLine() (and of course be sure to call the base functions defined here).
/// </summary>
public class PositionTracker : MonoBehaviour, DataLogger.DataLoggerDelegate
{
	/// <summary>
	/// Should the position be logged relative to the world (true), or the local position relative to the parent (false)?
	/// </summary>
	public bool isAbsolutePosition = true;

	// separate thread for logging position, independent of game thread.
	private DataLogger mDataLogger;
	//private string logFilePath;

	public long startTimeMillis { get; private set; }

	public Vector3 currentPosition { get; private set; }
	public Vector3 lastPosition { get; private set; }
	public Quaternion currentRotation { get; private set; }

	/// <summary>
	/// Total distance covered since calling 'StartLogging()'
	/// </summary>
	public float distance { get; private set; }

	public bool isLogging
	{
		get
		{
			return (null != mDataLogger) && mDataLogger.isLogging;
		}
	}

	public PositionTracker()
	{
		mDataLogger = new DataLogger(this);
		//mPath = new VRNPath();
	}

	// Use this for initialization
	public virtual void Start()
	{
		if (isAbsolutePosition)
		{
			currentPosition = transform.position;
		}
		else
		{
			currentPosition = transform.localPosition;
		}

		lastPosition = currentPosition;
	}

	private bool updatePosnLastFrame = false;
	public virtual void Update()
	{
		if (mDataLogger.isLogging)
		{
			if (!updatePosnLastFrame)
			{
				updatePosnLastFrame = true;
			}
			else
			{
				lastPosition = currentPosition;

				if (isAbsolutePosition)
				{
					currentPosition = transform.position;
					currentRotation = transform.rotation;
				}
				else
				{
					currentPosition = transform.localPosition;
					currentRotation = transform.localRotation;
				}

				// calculate total distance only when the position changes to avoid doubling up (so if the player does not move, totalDistance only increments by 0)
				distance += Vector3.Distance(lastPosition, currentPosition);
			}
		} else
		{
			updatePosnLastFrame = false;
		}
	}

	/// <summary>
	/// Need to implement OnDestroy since the logging thread will not terminate on its own, and will cause all sorts of mischief.
	/// </summary>
	public void OnDestroy()
	{
		Debug.Log("PositionTracker on " + gameObject.name + "OnDestroy: Try to stop logging.");
		if (isLogging)
		{
			StopLogging();
		} else
		{
			Debug.Log("Not logging.");
		}
	}

	public virtual bool StartLogging(string outputFilePath, string outputFileName, int period, bool overwriteExisting, DataLogger.FileCollisionHandlerMode mCollisionHandlerMode)
	{
		return StartLogging(outputFilePath, outputFileName, "csv", period, overwriteExisting, mCollisionHandlerMode);
	}

	/// <summary>
	/// Begin logging the position of this GameObject at a specific frequency. Successive calls do nothing.
	/// </summary>
	/// <param name="outputFilePath">full file path to save data to</param>
	/// <param name="outputFileName">name of file where data will be saved</param>
	/// <param name="extension">file extension for dat file</param>
	/// <param name="period">sampling interval</param>
	/// <param name="overwriteExisting">if a log file of this name already exists, should it be overwritten, or should a new file be created?</param>
	/// <returns>true if logging was started, false if already logging.</returns>
	public virtual bool StartLogging(string outputFilePath, string outputFileName, string extension, int period, bool overwriteExisting, DataLogger.FileCollisionHandlerMode mCollisionHandlerMode)
	{
		distance = 0f;
		startTimeMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

		//mPath = new VRNPath();
		bool didStart = mDataLogger.StartLogging(outputFilePath, outputFileName, extension, period, overwriteExisting, mCollisionHandlerMode);
		if (didStart)
		{
			Debug.Log("Started logging position to " + outputFilePath + "/" + outputFileName);
		}
		else
		{
			Debug.LogWarning("Could not start logging position to " + outputFilePath + "/" + outputFileName + ": already logging.");
		}

		return didStart;
	}

	public virtual bool PauseLogging()
	{
		return this.mDataLogger.HaltLogging();
	}

	public virtual bool ResumeLogging()
	{
		return this.mDataLogger.ResumeLogging();
	}

	/// <summary>
	/// Stop logging position. block until position logging thread is done. Successive calls do nothing.
	/// </summary>
	/// <returns>true if logging was stopped, false if logging has already stopped.</returns>
	public virtual bool StopLogging()
	{
		bool didStop = mDataLogger.StopLogging();
		if (didStop)
		{
			Debug.Log("stopped logging position");
		}
		else
		{
			Debug.LogWarning("Could not stop logging position: not logging position");
		}
		return didStop;
	}

	/// <summary>
	/// This function is called each time the DataLogger thread creates a new record. The data needs to be formatted in a comma-separated string with the data in the order specified in GetHeaderLine().
	/// this function may be overridden by child classes to add additional data fields to the log.
	/// </summary>
	/// <returns></returns>
	public virtual string GetDataLine()
	{
		long currentTimeMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; // 0.1ns resolution
		currentTimeMillis -= startTimeMillis;

		//mPath.AddPoint(new VRNPoint3(currentPosition.x, currentPosition.y, currentPosition.z));

		return currentTimeMillis + "," + currentPosition.x + "," + currentPosition.y + "," + currentPosition.z + "," + currentRotation.x + "," + currentRotation.y + "," + currentRotation.z + "," + currentRotation.w + "," + currentRotation.eulerAngles.x + "," + currentRotation.eulerAngles.y + "," + currentRotation.eulerAngles.z;
	}

	/// <summary>
	/// This function is called once when the DataLogger thread creates a new log file, and returns the names of each of the data fields. The names need to be formatted in a comma-separated string with the data in the order specified in GetDataLine().
	/// this function may be overridden by child classes to add additional data fields to the log.
	/// </summary>
	/// <returns></returns>
	public virtual string GetHeaderLine()
	{
		return "time (ms),Position x,Position y,Position z,RotationQ x,RotationQ y, RotationQ z, RotationQ w,RotationEuler x,RotationEuler y,RotationEuler z";

	}
}