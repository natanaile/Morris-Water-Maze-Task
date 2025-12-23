using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Apply settings that are specific to the VRN Chair and its use. These include quality settings, base sensitivities used for
/// calibration and the details for how Decoupled Mode should operate (e.g. whether or not to use a webcam, whether to show a blue screen or interactive view, etc.).
/// </summary>
[XmlRoot("ChairSettings")]
public class VRNChairSettings : AbstractVRNSettings
{
	/// <summary>
	/// The chair settings filename
	/// </summary>
	public const string CHAIR_SETTINGS_FILENAME = "ChairSettings.xml";

	/// <summary>
	/// There are two different ways for Decoupled Mode to behave.
	/// </summary>
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

	/// <summary>
	/// Visual settings for eye candy such as shadows, anti-aliasing, anisotropic filtering, etc.
	/// </summary>
	public enum QualityLevel
	{
		/// <summary>
		/// basic settings for older/slower hardware. Disable real-time shadows/anti-aliasing/texture filtering
		/// </summary>
		FAST,

		/// <summary>
		/// fancy settings for newer/faster hardware. Enable real-time shadows/anti-aliasing/texture filtering
		/// </summary>
		BEAUTIFUL
	}

	/// <summary>
	/// base y-sensitivity of wheelchair. Calibrate this to enable 1:1 forward/backward motion. 
	/// This value is specific to a particular wheelchair, and should be set for each chair.
	/// </summary>
	[XmlElement("BaseSensitivityY")]
	public float baseSensitivityY;

	/// <summary>
	/// base x-sensitivity of wheelchair, to enable 1:1 rotation of wheelchair
	/// This value is specific to a particular wheelchair, and should be set for each chair.
	/// </summary>
	[XmlElement("BaseSensitivityX")]
	public float baseSensitivityX;
	
	/// <summary>
	/// If true, the HMD will be used, if false, the conventional display.
	/// </summary>
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
	/// String name of the webcam to use
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

	/// <summary>
	/// show the current frames per second
	/// </summary>
	[XmlElement("FPSCounterEnabled")]
	public bool fpsCounterEnabled;

	/// <summary>
	/// If true, Load the calibration scene instead of whatever other scene(s) may be present, and run the <see cref="StickSensitivityCalibration"/>.
	/// </summary>
	[XmlElement("LoadBaseCalibration")]
	public bool loadBaseCalibration;

	//------------------
	// Boilerplate Code
	//------------------

	private static VRNChairSettings theInstance = null;

	/// <summary>
	/// Loads the chair settings.
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk.
	/// </summary>
	public override void Save()
	{
		base.Save(Application.persistentDataPath, CHAIR_SETTINGS_FILENAME, typeof(VRNChairSettings));
	}

	/// <summary>
	/// default constructor with default values. Call <see cref="Load"/> instead!
	/// </summary>
	public VRNChairSettings()
	{
		this.baseSensitivityX = 1.0f;
		this.baseSensitivityY = 1.0f; // Positive = push joystick up to go forward

		this.hmdEnabled = false; // Set to false for laptop/desktop mode, true for VR headset
		
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