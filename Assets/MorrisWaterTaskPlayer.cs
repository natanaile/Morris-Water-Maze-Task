using System;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(AdvancedPositionLogger))]
public class MorrisWaterTaskPlayer : AbstractVREPlayer
{
	private bool _blankScreen;
	private bool blankScreen
	{
		get
		{
			return _blankScreen;
		}

		set
		{
			_blankScreen = value;

			// enable/disable current task hint
			GameObject[] playerCameras = GameObject.FindGameObjectsWithTag("PlayerCamera");
			foreach (GameObject cameraObject in playerCameras)
			{
				Camera camera = cameraObject.GetComponent<Camera>();
				if (null != camera)
				{
					if (blankScreen)
					{
						camera.cullingMask = camera.cullingMask & ~LayerMask.GetMask("World", "Targets"); // turn off
						camera.clearFlags = CameraClearFlags.Color;
					}
					else
					{
						camera.cullingMask = camera.cullingMask | LayerMask.GetMask("World"); // turn on
						camera.clearFlags = CameraClearFlags.Skybox;
					}
				}
			}
		}
	}

	private AdvancedPositionLogger mPositionLogger;

	public override void Start()
	{
		base.Start();

		blankScreen = true;

		mPositionLogger = GetComponent<AdvancedPositionLogger>();
		mPositionLogger.isAbsolutePosition = false; // log relative to parent
	}

	protected override void BeginPlayer()
	{
		base.BeginPlayer();

		blankScreen = false;

		// start loggging
		VRNWaterTaskTemp temp = VRNWaterTaskTemp.Load();
		VRNAbstractVRETaskEnvironmentSettings taskEnvSettings = VRNAbstractVRETaskEnvironmentSettings.Load();
		VRNWaterTaskSettings settings = VRNWaterTaskSettings.Load();
		VRNWaterTaskOrder taskOrder = VRNWaterTaskOrder.Load(settings.presetPath);
		string logFilePath = taskEnvSettings.defaultDataPath + "/" + temp.currentAssessmentName + "/trial_" + temp.trialNumber;
		string fileName = "position";
		mPositionLogger.StartLogging(logFilePath, fileName, 16, false, DataLogger.FileCollisionHandlerMode.FILENAME_VERSION_NUMBER);

		FileStream metaFileStream = null;
		try
		{
			metaFileStream = new FileStream(logFilePath + "/meta.txt", FileMode.OpenOrCreate);
		}
		catch (DirectoryNotFoundException)
		{
			System.IO.Directory.CreateDirectory(logFilePath);
			metaFileStream = new FileStream(logFilePath + "/meta.txt", FileMode.OpenOrCreate);
		}
		catch (IOException) // file already exists
		{
			Debug.Log("Meta file already exists.");
		}

		if (null != metaFileStream)
		{
			StreamWriter writer = new StreamWriter(metaFileStream);
			writer.WriteLine("Participant Name: " + taskEnvSettings.currentPatientName);

			string dateFormat = DateTime.Now.ToString("yyyy-MM-dd"); // don't log hours/minutes
			writer.WriteLine("Date: " + dateFormat);
			writer.WriteLine("Player Start: " + taskOrder.tasksOrder[temp.trialNumber - 1].playerDirection);
			writer.WriteLine("Target Location: " + taskOrder.tasksOrder[temp.trialNumber - 1].taskDirection);
			writer.WriteLine("Task type: " + taskOrder.tasksOrder[temp.trialNumber - 1].taskType);
			VRNWaterTaskSettings waterTaskSettings = VRNWaterTaskSettings.Load();
			writer.WriteLine("Timeout: " + waterTaskSettings.hintTimeout + "s");
			writer.Close();
			Debug.Log("Wrote meta file to " + logFilePath + "/meta.txt");
		} else
		{
			Debug.LogError("COULD NOT WRITE META FILE TO " + logFilePath + "/meta.txt");
		}

	}

	protected override void DidCompleteTask(CompletedTask completedTask)
	{
		Debug.Log("Completed task.");
	}
}

