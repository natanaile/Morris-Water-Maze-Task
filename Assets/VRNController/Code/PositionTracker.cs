using UnityEngine;
using System;

/// <summary>
/// The PositionTracker logs the position and orientation of a GameObject. It may be extended to add additional data fields, 
/// and child classes should override <see cref="GetDataLine"/> and <see cref="GetHeaderLine"/> (and of course be sure to call the base functions defined here).
/// </summary>
public class PositionTracker : MonoBehaviour, DataLogger.DataLoggerDelegate
{
	/// <summary>
	/// Should the position be logged relative to the world (true), or the local position relative to the parent (false)?
	/// </summary>
	public bool isAbsolutePosition = true;

	// separate thread for logging position, independent of game thread.
	private DataLogger mDataLogger;

	/// <summary>
	/// Gets the start time, in milliseconds.
	/// </summary>
	/// <value>
	/// The start time in milliseconds.
	/// </value>
	public long startTimeMillis { get; private set; }

	/// <summary>
	/// position of body, relative to parent (usually world)
	/// </summary>
	public Vector3 currentPosition { get; private set; }

	/// <summary>
	/// position in previous frame (used for detecting movement, measuring velocity, etc.)
	/// </summary>
	public Vector3 lastPosition { get; private set; }

	/// <summary>
	/// current angle of body relative to parent (usually world)
	/// </summary>
	public Quaternion currentRotation { get; private set; }

	/// <summary>
	/// Total distance covered since calling 'StartLogging()'
	/// </summary>
	public float distance { get; private set; }

	/// <summary>
	/// property that lets other objects know if the associated <see cref="DataLogger"/> is
	/// currently active.
	/// </summary>
	public bool isLogging
	{
		get
		{
			return (null != mDataLogger) && mDataLogger.isLogging;
		}
	}

	/// <summary>
	/// Default Constructor. Unity does not explicitly call constructors, instead using the <see cref="Start()"/> function.
	/// The default constructor is defined here to instantiate the DataLogger, which needs to spin up a thread.
	/// </summary>
	public PositionTracker()
	{
		mDataLogger = new DataLogger(this);
		//mPath = new VRNPath();
	}

	/// <summary>
	/// Use this for initialization
	/// </summary>
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

	/// <summary>
	/// this state variable is used to ensure that positions are captured the frame AFTER the <see cref="P:DataLogger.isLogging"/> 
	/// property of <see cref="mDataLogger"/> becomes true. 
	/// </summary>
	private bool updatePosnLastFrame = false;

	/// <summary>
	/// Called every frame. Override in subclasses to add extra functionality (but make sure to call base function!!!)
	/// </summary>
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

				// calculate total distance only when the position changes to avoid doubling up (so if the participant does not move, totalDistance only increments by 0)
				distance += Vector3.Distance(lastPosition, currentPosition);
			}
		} else
		{
			updatePosnLastFrame = false;
		}
	}

	/// <summary>
	/// Need to implement <c>OnDestroy()</c> since the logging thread will not terminate on its own, and will cause all sorts of mischief.
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

	/// <summary>
	/// Begin logging the position of this GameObject at a specific frequency. Successive calls do nothing.
	/// </summary>
	/// <param name="outputFilePath">full file path to save data to</param>
	/// <param name="outputFileName">name of file where data will be saved</param>
	/// <param name="period">sampling interval</param>
	/// <param name="overwriteExisting">if a log file of this name already exists, should it be overwritten, or should a new file be created?</param>
	/// <param name="mCollisionHandlerMode">What should be done in the event that a file with the specified name and directory already exists?</param>
	/// <returns></returns>
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
	/// <param name="mCollisionHandlerMode">What should be done in the event that a file with the specified name and directory already exists?</param>
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

	/// <summary>
	/// Temporarily stop logging, with the intention of resuming later on.
	/// </summary>
	/// <returns>true if successful</returns>
	public virtual bool PauseLogging()
	{
		return this.mDataLogger.HaltLogging();
	}

	/// <summary>
	/// Resume logging that was previously paused.
	/// </summary>
	/// <returns></returns>
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