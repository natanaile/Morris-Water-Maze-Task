using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Temp")]
public class VRNWaterTaskTemp : AbstractVRNSettings
{
	public const string WATERTASK_TEMP_FILENAME = "WaterTaskTemp.xml";

	/// <summary>
	/// current trial
	/// </summary>
	[XmlElement("TrialNumber")]
	public int trialNumber;

	/// <summary>
	/// The name of the file that is currently being logged
	/// </summary>
	[XmlElement("CurrentAssessmentName")]
	public string currentAssessmentName;

	private static VRNWaterTaskTemp theInstance = null;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNWaterTaskTemp object you have is based off whatever is stored in the filesystem. For convenience, assumes default file location/name.
	/// </summary>
	/// <returns>Return an instance of VRNWaterTaskTemp. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNWaterTaskTemp Load()
	{
		theInstance = (VRNWaterTaskTemp)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + WATERTASK_TEMP_FILENAME, typeof(VRNWaterTaskTemp), theInstance);
		return theInstance;
	}

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk.
	/// </summary>
	public override void Save()
	{
		base.Save(Application.persistentDataPath, WATERTASK_TEMP_FILENAME, typeof(VRNWaterTaskTemp));
	}


	/// <summary>
	/// Default constructor initializes default values, should ONLY be called by baseclass. if you want an instance of VRNWaterTaskTemp, call VRNWaterTaskTemp.Load()!!!!
	/// </summary>
	public VRNWaterTaskTemp()
	{
		this.trialNumber = 1;
		this.currentAssessmentName = "";
	}

	public void Delete()
	{
		string tempPath = Application.persistentDataPath + "/" + WATERTASK_TEMP_FILENAME;
		try
		{
			File.Delete(tempPath);
		}
		catch (IOException)
		{
			Debug.Log("Could not delete file " + tempPath);
		}
	}
}
