using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AssessmentSettings")]
public class VRNWaterTaskSettings : AbstractVRNSettings
{
	public const string WATER_TASK_SETTINGS_FILENAME = "WaterTaskSettings.xml";

	[XmlElement("HintTimeout")]
	public int hintTimeout;

	[XmlElement("TimeToComplete")]
	public int timeToComplete;

	[XmlElement("PresetPath")]
	public string presetPath;

	//------------------
	// Boilerplate Code
	//------------------

	private static VRNWaterTaskSettings theInstance = null;

	public static VRNWaterTaskSettings Load()
	{
		string settingsPath = Application.persistentDataPath + "/" + WATER_TASK_SETTINGS_FILENAME;
		theInstance = (VRNWaterTaskSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNWaterTaskSettings), theInstance);
		return theInstance;
	}

	public override void Save()
	{
		base.Save(Application.persistentDataPath, WATER_TASK_SETTINGS_FILENAME, typeof(VRNWaterTaskSettings));
	}

	public VRNWaterTaskSettings()
	{
		this.hintTimeout = 30; // seconds
		this.timeToComplete = 5; // seconds
	}
}