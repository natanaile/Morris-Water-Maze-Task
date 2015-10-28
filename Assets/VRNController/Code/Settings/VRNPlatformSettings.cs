using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Options")]
public class VRNPlatformSettings : AbstractVRNSettings
{
	public enum QualityLevel
	{
		FAST,
		BEAUTIFUL
	}

	/// <summary>
	/// What webcam to use?
	/// </summary>
	[XmlElement("WebcamDeviceName")]
	public string webcamDeviceName;

	/// <summary>
	/// How powerful is the PC?
	/// </summary>
	[XmlElement("QualityLevel")]
	public QualityLevel qualityLevel;

	/// <summary>
	/// Where should the log data be stored? Player data folders will be created here.
	/// </summary>
	[XmlElement("DefaultDataPath")]
	public string defaultDataPath;

	/// <summary>
	/// The name of the serial port to get intertial input from
	/// </summary>
	[XmlElement("SerialPortID")]
	public string serialPortID;

	/// <summary>
	/// The name of the serial port to get intertial input from
	/// </summary>
	[XmlElement("VsyncEnabled")]
	public bool vsncEnabled;

	/// <summary>
	/// Serial port baud rate
	/// </summary>
	[XmlElement("BaudRate")]
	public int baudRate;

	private static VRNPlatformSettings theInstance = null;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNPlatformSettings object you have is based off whatever is stored in the filesystem. For convenience, assumes default file location/name.
	/// </summary>
	/// <returns>Return an instance of VRNPlatformSettings. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNPlatformSettings Load()
	{
		theInstance = (VRNPlatformSettings) AbstractVRNSettings.Load(Application.persistentDataPath + "/" + VRNStaticMembers.PLATFORM_SETTINGS_FILE_NAME, typeof(VRNPlatformSettings), theInstance);
		return theInstance;
	}	

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk.
	/// </summary>
	public override void Save()
	{
		base.Save(Application.persistentDataPath, VRNStaticMembers.PLATFORM_SETTINGS_FILE_NAME, typeof(VRNPlatformSettings));
	}

	/// <summary>
	/// Default constructor initializes default values
	/// </summary>
	public VRNPlatformSettings()
	{
		this.serialPortID = "COM6";
		this.baudRate = 115200;
		this.vsncEnabled = false;
		this.qualityLevel = QualityLevel.FAST;
		this.webcamDeviceName = "NONE";

		// Create data directory depending on the system (thanks to http://stackoverflow.com/questions/1143706/getting-the-path-of-the-home-directory-in-c)
		string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) ?
			Environment.GetEnvironmentVariable("HOME") + "/": Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\";
		this.defaultDataPath = homePath + "VRNData";
		Directory.CreateDirectory(defaultDataPath);
	}
}
