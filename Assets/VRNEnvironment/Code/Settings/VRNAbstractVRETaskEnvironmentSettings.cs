using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AbstractVRETaskEnvironmentSettings")]
public class VRNAbstractVRETaskEnvironmentSettings : AbstractVRNSettings
{
	public const string ABSTRACT_VRE_TASK_ENV_SETTINGS_FILENAME = "AbstractTaskEnvSettings.xml";

	/// <summary>
	/// Reset the player's position for each trial
	/// </summary>
	[XmlElement("ResetPlayerPosition")]
	public bool resetPlayerPosition;

	/// <summary>
	/// The name of the current patient
	/// </summary>
	[XmlElement("CurrentPatientName")]
	public string currentPatientName;

	/// <summary>
	/// Where should the log data be stored? Player data folders will be created here.
	/// </summary>
	[XmlElement("DefaultDataPath")]
	public string defaultDataPath;

	/// <summary>
	/// How frequently should the subject's position be sampled?
	/// </summary>
	[XmlElement("LogRate")]
	public int logRate;

	//------------------
	// Boilerplate Code
	//------------------

	private static VRNAbstractVRETaskEnvironmentSettings theInstance = null;

	public static VRNAbstractVRETaskEnvironmentSettings Load()
	{
		string settingsPath = Application.persistentDataPath + "/" + ABSTRACT_VRE_TASK_ENV_SETTINGS_FILENAME;
		try
		{
			theInstance = (VRNAbstractVRETaskEnvironmentSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNAbstractVRETaskEnvironmentSettings), theInstance);
			return theInstance;
		}
		catch (InvalidOperationException ex)
		{
			Debug.LogError("could not parse the file at " + settingsPath + ". creating a new one." + ex);

			DateTime dateTime = DateTime.Now;
			string dateString = dateTime.ToString("yyyy_MMM_dd_HH_mm_ss");

			// back up corrupt file (for postmortem)
			File.Copy(settingsPath, settingsPath + "_corrupt_" + dateString);
			File.Delete(settingsPath);

			// try again
			theInstance = (VRNAbstractVRETaskEnvironmentSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNAbstractVRETaskEnvironmentSettings), theInstance);
			return theInstance;
		}
	}

	public override void Save()
	{
		base.Save(Application.persistentDataPath, ABSTRACT_VRE_TASK_ENV_SETTINGS_FILENAME, typeof(VRNAbstractVRETaskEnvironmentSettings));
	}

	public VRNAbstractVRETaskEnvironmentSettings()
	{
		this.resetPlayerPosition = true;
		this.currentPatientName = "TEST_NAME";

		// Create data directory depending on the system (thanks to http://stackoverflow.com/questions/1143706/getting-the-path-of-the-home-directory-in-c)
		string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) ?
			Environment.GetEnvironmentVariable("HOME") + "/" : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\";
		this.defaultDataPath = homePath + "VRNData";
		Directory.CreateDirectory(defaultDataPath);

		this.logRate = 16;
	}
}