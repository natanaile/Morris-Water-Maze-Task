using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("ChairSettings")]
public class VRNChairSettings : AbstractVRNSettings
{
	public const string CHAIR_SETTINGS_FILENAME = "ChairSettings.xml";

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

	public enum QualityLevel
	{
		FAST,
		BEAUTIFUL
	}

	/// <summary>
	/// base y-sensitivity of wheelchair, to enable 1:1 forward/backward motion. 
	/// This value is specific to a particular wheelchair, and should be set for each chair.
	/// </summary>
	[XmlElement("BaseSensitivityY")]
	public float baseSensitivityY;

	/// <summary>
	/// base x-sensitivity of wheelchair, to enable 1:1 rotation
	/// This value is specific to a particular wheelchair, and should be set for each chair.
	/// </summary>
	[XmlElement("BaseSensitivityX")]
	public float baseSensitivityX;
	
	[XmlElement("HMDEnabled")]
	public bool hmdEnabled;

	/// <summary>
	/// The name of the serial port to get intertial input from
	/// </summary>
	[XmlElement("SerialPortID")]
	public string serialPortID;

	/// <summary>
	/// Serial port baud rate
	/// </summary>
	[XmlElement("BaudRate")]
	public int baudRate;

	/// <summary>
	/// how to handle decoupling
	/// </summary>
	[XmlElement("DecoupledMode")]
	public DecoupledMode decoupledMode;

	/// <summary>
	/// What webcam to use?
	/// </summary>
	[XmlElement("WebcamDeviceName")]
	public string webcamDeviceName;
	
	/// <summary>
	/// height of players eyes above the ground (different for each person) (in metres)
	/// </summary>
	[XmlElement("EyeHeight")]
	public float eyeHeight;
	
	/// <summary>
	/// How powerful is the PC?
	/// </summary>
	[XmlElement("QualityLevel")]
	public QualityLevel qualityLevel;
	
	/// <summary>
	/// The name of the serial port to get intertial input from
	/// </summary>
	[XmlElement("VsyncEnabled")]
	public bool vsyncEnabled;

	[XmlElement("FPSCounterEnabled")]
	public bool fpsCounterEnabled;

	[XmlElement("LoadBaseCalibration")]
	public bool loadBaseCalibration;

	//------------------
	// Boilerplate Code
	//------------------

	private static VRNChairSettings theInstance = null;

	public static VRNChairSettings Load()
	{
		string settingsPath = Application.persistentDataPath + "/" + CHAIR_SETTINGS_FILENAME;
		try
		{
			theInstance = (VRNChairSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNChairSettings), theInstance);
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
			theInstance = (VRNChairSettings)AbstractVRNSettings.Load(settingsPath, typeof(VRNChairSettings), theInstance);
			return theInstance;
		}
	}

	public override void Save()
	{
		base.Save(Application.persistentDataPath, CHAIR_SETTINGS_FILENAME, typeof(VRNChairSettings));
	}

	public VRNChairSettings()
	{
		this.baseSensitivityX = 1.0f;
		this.baseSensitivityY = -1.0f;

		this.hmdEnabled = true;
		
		this.serialPortID = "COM4";
		this.baudRate = 115200;

		this.decoupledMode = DecoupledMode.LOCKED_VR;
		this.webcamDeviceName = "NONE";
		
		this.eyeHeight = 1.25f;

		this.fpsCounterEnabled = false;
		this.qualityLevel = QualityLevel.BEAUTIFUL;
		this.vsyncEnabled = true;

		this.loadBaseCalibration = false;

	}
}