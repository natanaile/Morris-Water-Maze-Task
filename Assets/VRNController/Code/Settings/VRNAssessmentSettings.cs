using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AssessmentSettings")]
public class VRNAssessmentSettings : AbstractVRNSettings
{
	public enum DecoupledMode
	{
		/// <summary>
		/// screen is blank while user is decoupled
		/// </summary>
		BLANK_SCREEN,

		/// <summary>
		/// Screen shows VRN while user is decoupled, with YAW rotation locked.
		/// A live webcam feed may be displayed, if the necessary hardware is present.
		/// </summary>
		LOCKED_VR
	}


	public enum AssessmentMode
	{
		/// <summary>
		/// The VRE test, where the user navigates between target windows
		/// </summary>
		VRE,

		/// <summary>
		/// No data is logged and no prompts are provided. just walk around
		/// </summary>
		EXPLORE
	}


	[XmlElement("FPSCounterEnabled")]
	public bool fpsCounterEnabled;

	[XmlElement("OVREnabled")]
	public bool hmdEnabled;

	/// <summary>
	/// Show the subject the ideal path that they should have followed
	/// </summary>
	[XmlElement("ShowIdealpath")]
	public bool showIdealPath;

	/// <summary>
	/// Display a panorama shot automatically before doing the assessment
	/// </summary>
	[XmlElement("AutoPan")]
	public bool autopan;

	/// <summary>
	/// Prompt the player to leave the house after the final task
	/// </summary>
	[XmlElement("ExitTaskAtEnd")]
	public bool exitTaskAtEnd;

	/// <summary>
	/// Reset the player's position for each trial
	/// </summary>
	[XmlElement("ResetPlayerPosition")]
	public bool resetPlayerPosition;

	/// <summary>
	/// how to handle decoupling
	/// </summary>
	[XmlElement("DecoupledMode")]
	public DecoupledMode decoupledMode;

	/// <summary>
	/// How many times should the assessment be run to train the subject?
	/// </summary>
	[XmlElement("NumTrainingSessions")]
	public int numTrainingSessions;

	/// <summary>
	/// What is the training task?
	/// </summary>
	[XmlElement("TrainingTaskName")]
	public string trainingTaskName;

	/// <summary>
	/// How many times should the assessment be run?
	/// </summary>
	[XmlElement("NumSessions")]
	public int numSessions;

	/// <summary>
	/// What Mode?
	/// </summary>
	[XmlElement("AssessmentMode")]
	public AssessmentMode assessmentMode;

	/// <summary>
	/// How frequently should the subject's position be sampled?
	/// </summary>
	[XmlElement("LogRate")]
	public int logRate;
	/// <summary>
	/// The name of the current patient
	/// </summary>
	[XmlElement("CurrentPatientName")]
	public string currentPatientName;

	/// <summary>
	/// Should the targets be hidden while the subject is in the house? (prevents targets from being visible when not in the destination room)
	/// </summary>
	[XmlElement("AutoHideTargets")]
	public bool autoHideTargets;

	/// <summary>
	/// Speed of rotation of house in 'Rotation' mode
	/// </summary>
	[XmlElement("RotationSpeed")]
	public float rotationSpeed;

	/// <summary>
	/// How quickly camera moves to view outside of house in 'Rotation' mode
	/// </summary>
	[XmlElement("MoveSpeed")]
	public float moveSpeed;

	/// <summary>
	/// height of players eyes above the ground (different for each person) (in metres)
	/// </summary>
	[XmlElement("EyeHeight")]
	public float eyeHeight;

	//------------------
	// Boilerplate Code
	//------------------

	private static VRNAssessmentSettings theInstance = null;

	public static VRNAssessmentSettings Load()
	{
		string settingsPath = Application.persistentDataPath + "/" + VRNStaticMembers.ASSESSMENT_SETTINGS_FILE_NAME;
		try
		{
			theInstance = (VRNAssessmentSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNAssessmentSettings), theInstance);
			return theInstance;
		} catch (InvalidOperationException ex)
		{
			Debug.LogError("could not parse the file at " + settingsPath + ". creating a new one." + ex);

			DateTime dateTime = DateTime.Now;
			string dateString = dateTime.ToString("yyyy_MMM_dd_HH_mm_ss");

			// back up corrupt file (for postmortem)
			File.Copy(settingsPath, settingsPath + "_corrupt_" + dateString);
			File.Delete(settingsPath);

			// try again
			theInstance = (VRNAssessmentSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNAssessmentSettings), theInstance);
			return theInstance;
		}
	}

	public override void Save()
	{
		base.Save(Application.persistentDataPath, VRNStaticMembers.ASSESSMENT_SETTINGS_FILE_NAME, typeof(VRNAssessmentSettings));
	}

	public VRNAssessmentSettings()
	{
		logRate = 16; // locked 60 bb
		showIdealPath = false; // not working yet
		this.autoHideTargets = true;
		this.autopan = false;
		//this.currentAssessmentName = "";
		this.currentPatientName = "TESTING";
		this.numSessions = 8;
		this.numTrainingSessions = 2;
		this.resetPlayerPosition = true;
		this.exitTaskAtEnd = false;

		this.decoupledMode = DecoupledMode.BLANK_SCREEN;

		this.moveSpeed = 500f;
		this.rotationSpeed = 18f;
		this.trainingTaskName = "A";

		this.fpsCounterEnabled = false;
		this.eyeHeight = 1.360f;

		this.assessmentMode = AssessmentMode.VRE;

	}
}