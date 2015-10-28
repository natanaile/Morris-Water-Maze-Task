using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Temp")]
public class VRNTemp : AbstractVRNSettings
{
	public enum AssessmentType
	{
		TRAINING,
		RANDOM_TASKS,
		ORDERED_TASKS,
		EXIT
	}

	/// <summary>
	/// current trial
	/// </summary>
	[XmlElement("TrialNumber")]
	public int trialNumber;

	/// <summary>
	/// current mode
	/// </summary>
	[XmlElement("AssessmentMode")]
	public AssessmentType assessmentMode;

	/// <summary>
	/// The name of the file that is currently being logged
	/// </summary>
	[XmlElement("CurrentAssessmentName")]
	public string currentAssessmentName;

	private static VRNTemp theInstance = null;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNTemp object you have is based off whatever is stored in the filesystem. For convenience, assumes default file location/name.
	/// </summary>
	/// <returns>Return an instance of VRNTemp. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNTemp Load()
	{
		theInstance = (VRNTemp)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + VRNStaticMembers.TEMP_FILE_NAME, typeof(VRNTemp), theInstance);
		return theInstance;
	}

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk.
	/// </summary>
	public override void Save()
	{
		base.Save(Application.persistentDataPath, VRNStaticMembers.TEMP_FILE_NAME, typeof(VRNTemp));
	}


	/// <summary>
	/// Default constructor initializes default values, should ONLY be called by baseclass. if you want an instance of VRNTemp, call VRNTemp.Load()!!!!
	/// </summary>
	public VRNTemp()
	{
		this.trialNumber = 1;
		this.assessmentMode = AssessmentType.TRAINING;
		this.currentAssessmentName = "";
		//this.subjectName = "no_name";
	}

	public void Delete()
	{
		string tempPath = Application.persistentDataPath + "/" + VRNStaticMembers.TEMP_FILE_NAME;
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
