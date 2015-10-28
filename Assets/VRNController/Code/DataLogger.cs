using System;
using System.IO;
using System.Threading;

/// <summary>
/// Log data to a CSV file
/// </summary>
public class DataLogger
{

	public interface DataLoggerDelegate
	{
		/// <summary>
		/// Return a line of data
		/// </summary>
		/// <returns></returns>
		string GetDataLine();

		/// <summary>
		/// Return a header line for the data file
		/// </summary>
		/// <returns></returns>
		string GetHeaderLine();
	}

	// separate thread for logging position, independent of game thread.
	Timer logPositionTimer;
	private string logFilePath;

	// instrumentation
	public bool isLogging { get; private set; }
	private bool wasLogging = false;

	/// <summary>
	/// has a log file been opened? (i.e. is it legit to call Resume)
	/// </summary>
	private bool primedLogFile = false;

	//long startTimeMillis = 0;

	private DataLoggerDelegate mDelegate;

	public DataLogger(DataLoggerDelegate dataLoggerDelegate)
	{
		this.mDelegate = dataLoggerDelegate;
		this.isLogging = false;
	}

	/// <summary>
	/// Begin logging at a specific frequency. Successive calls do nothing. Use CSV file extension.
	/// </summary>
	/// <param name="outputFilePath">full file path to save data to</param>
	/// <param name="outputFileName">name of file where data will be saved</param>
	/// <param name="period">sampling interval</param>
	/// <param name="overwriteExisting">if a log file of this name already exists, should it be overwritten, or should a new file be created?</param>
	/// <param name="mCollisionHandlerMode">How should filename collisions be resolved?</param>
	/// <returns>true if logging was started, false if already logging.</returns>
	public bool StartLogging(string outputFilePath, string outputFileName, int period, bool overwriteExisting, FileCollisionHandlerMode mCollisionHandlerMode)
	{
		return StartLogging(outputFilePath, outputFileName, "csv", period, overwriteExisting, mCollisionHandlerMode);
	}

	/// <summary>
	/// Begin logging at a specific frequency. Successive calls do nothing. allow user to specify data file extension
	/// </summary>
	/// <param name="outputFilePath">full file path to save data to</param>
	/// <param name="outputFileName">name of file where data will be saved</param>
	/// <param name="extension">file extension for data file</param>
	/// <param name="period">sampling interval</param>
	/// <param name="overwriteExisting">if a log file of this name already exists, should it be overwritten, or should a new file be created?</param>
	/// <param name="mCollisionHandlerMode">How should filename collisions be resolved?</param>
	/// <returns>true if logging was started, false if already logging.</returns>
	public bool StartLogging(string outputFilePath, string outputFileName, string extension, int period, bool overwriteExisting, FileCollisionHandlerMode mCollisionHandlerMode)
	{
		if (!isLogging)
		{
			isLogging = true;
			wasLogging = false;


			// set up logging			
			FileStream logFileStream = OpenFileStream(outputFilePath, outputFileName, extension, 0, mCollisionHandlerMode);
			StreamWriter logFileOutput = new StreamWriter(logFileStream);
			outputFilePath = logFileStream.Name;
			logFilePath = outputFilePath;
			// write header
			//logFileOutput.WriteLine("time (0.1ns),Position x,Position y,Position z,RotationQ x,RotationQ y, RotationQ z, RotationQ w,RotationEuler x,RotationEuler y,RotationEuler z");
			logFileOutput.WriteLine(mDelegate.GetHeaderLine());
			logFileOutput.Close();

			primedLogFile = true;
			ResumeLogging();
		}
		else
		{
			wasLogging = true;
		}

		return !wasLogging;
	}

	/// <summary>
	/// Resume logging. Requires that a log file has already been setup.
	/// </summary>
	/// <returns>'true' if currently logging. 'false' if 'StartLogging' has not been called</returns>
	public bool ResumeLogging()
	{
		if (primedLogFile)
		{
			isLogging = true;
			logPositionTimer = new Timer(LogPositionCallback, null, 50, 16); // start the timer with a 16ms interval, delay the start to ignore any blips at the beginning
		}

		return primedLogFile && isLogging;
	}

	/// <summary>
	/// enumerate the different possible ways to handle filename collisions
	/// </summary>
	public enum FileCollisionHandlerMode
	{
		/// <summary>
		/// newer files overwrite older files
		/// </summary>
		NONE,

		/// <summary>
		/// duplicated files will have a version_number suffix, i.e. &lt;Folder&gt;/&lt;Filename&gt;_vers_1.&lt;extension&gt;
		/// </summary>
		FILENAME_VERSION_NUMBER,

		/// <summary>
		/// duplicated files will be placed in a sibling folder with a version_number suffix, i.e. &lt;Folder&gt;_vers_1/&lt;Filename&gt;.&lt;extension&gt;
		/// </summary>
		FOLDERNAME_VERSION_NUMBER,

		/// <summary>
		/// duplicated files will be placed in a sibling folder with a number suffix, i.e. &lt;Folder&gt;_1/&lt;Filename&gt;.&lt;extension&gt;
		/// </summary>
		FILENAME_NUMBER,

		/// <summary>
		/// duplicated files will have a number suffix, i.e. &lt;Folder&gt;/&lt;Filename&gt;_1.&lt;extension&gt;
		/// </summary>
		FOLDERNAME_NUMBER
	}

	/// <summary>
	/// Open a file stream in a new file
	/// </summary>
	/// <param name="filePath">the path to try</param>
	/// <param name="filePath">name of the output file</param>
	/// <param name="extension">file extension (no '.', e.g. for a .csv file this would be 'csv')</param>
	/// <param name="existing">the number of times that this had already failed</param>
	/// <param name="mCollisionHandlerMode">How should filename collisions be resolved?</param>
	/// <returns></returns>
	public static FileStream OpenFileStream(string filePath, string fileName, string extension, int existing, FileCollisionHandlerMode mCollisionHandlerMode)
	{
		FileStream fileStream;
		string decoratedPath = filePath;
		string decoratedName = fileName;


		switch (mCollisionHandlerMode)
		{
			case FileCollisionHandlerMode.FILENAME_VERSION_NUMBER:
				if (existing != 0)
				{
					decoratedName += "_vers_" + existing;
				}
				break;

			case FileCollisionHandlerMode.FILENAME_NUMBER:
				if (existing != 0)
				{
					decoratedName += "_" + existing;
				}
				break;

			case FileCollisionHandlerMode.FOLDERNAME_VERSION_NUMBER:
				if (existing != 0)
				{
					decoratedPath += "_vers_" + existing;
				}
				break;

			case FileCollisionHandlerMode.FOLDERNAME_NUMBER:
				if (existing != 0)
				{
					decoratedPath += "_" + existing;
				}
				break;

			case FileCollisionHandlerMode.NONE:
				break;

			default:
				throw new ArgumentException(mCollisionHandlerMode + " is not a recognized CollisionHandler");
		}

		string directory = decoratedPath;

		decoratedPath += "/" + decoratedName + "." + extension;

		try
		{
			fileStream = new FileStream(decoratedPath, FileMode.CreateNew);
		}
		catch (DirectoryNotFoundException)
		{
			System.IO.Directory.CreateDirectory(directory);
			//File.Create(decoratedPath);
			fileStream = new FileStream(decoratedPath, FileMode.Create);
		}

		catch (IOException) // file already exists
		{
			if (FileCollisionHandlerMode.NONE != mCollisionHandlerMode)
			{
				fileStream = OpenFileStream(filePath, fileName, extension, existing + 1, mCollisionHandlerMode);
			}
			else // no collision handling
			{
				fileStream = new FileStream(decoratedPath, FileMode.Create); // overwrite old file
			}
		}

		return fileStream;
	}

	/// <summary>
	/// Stop logging position. block until position logging thread is done. Successive calls do nothing.
	/// </summary>
	/// <returns>true if logging was stopped, false if logging has already stopped.</returns>
	public bool StopLogging()
	{
		bool didStop = HaltLogging();
		primedLogFile = false;
		return didStop;
	}

	/// <summary>
	/// Pause logging (may resume)
	/// </summary>
	/// <returns>'true' if logging was stopped, false if it was previously stopped.</returns>
	public bool HaltLogging()
	{
		if (isLogging)
		{
			isLogging = false;
			wasLogging = true;
			logPositionTimer.Dispose();
		}
		else
		{
			wasLogging = false;
		}

		//Debug.Log("Stopped logging position to file " + logFilePath);

		return wasLogging;
	}

	/// <summary>
	/// Get the current position and store it in a text file, along with the time
	/// </summary>
	private void LogPositionCallback(object state)
	{
		// log data
		//long currentTimeMillis = DateTime.Now.Ticks; // 0.1ns resolution
		//currentTimeMillis -= startTimeMillis;

		//string logData = currentTimeMillis + "," + currentPosition.x + "," + currentPosition.y + "," + currentPosition.z + "," + currentRotation.x + "," + currentRotation.y + "," + currentRotation.z + "," + currentRotation.w + "," + currentRotation.eulerAngles.x + "," + currentRotation.eulerAngles.y + "," + currentRotation.eulerAngles.z;


		string logData = mDelegate.GetDataLine();

		StreamWriter positionWriter = new StreamWriter(logFilePath, true); // append to a file that exists
		positionWriter.WriteLine(logData);
		positionWriter.Close();
	}
}