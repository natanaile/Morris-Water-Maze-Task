using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Settings for defining the controls used in the VRN application, most importantly forward/backward/rotational sensitivities.
/// These settings may be modified by the launcher (e.g. to set 2:1 sensitivity). Note that the <see cref="stickSensitivityY"/> and <see cref="stickSensitivityX"/>
/// used here are different from <see cref="F:VRNChairSettings.baseSensitivityY"/> and <see cref="F:VRNChairSettings.baseSensitivityX"/>.
/// </summary>
[XmlRoot("Controls")]
public class VRNControls : AbstractVRNSettings
{
	/// <summary>
	/// filename of this setting
	/// </summary>
	public const string CONTROLS_FILENAME = "Controls.xml";

	/// <summary>
	/// X axis sensitivity multiplier
	/// </summary>
	[XmlElement("StickSensitivityX")]
	public float stickSensitivityX;

	/// <summary>
	/// Y axis sensitivity multiplier.
	/// </summary>
	[XmlElement("StickSensitivityY")]
	public float stickSensitivityY;

	/// <summary>
	/// Mouse sensitivity
	/// </summary>
	[XmlElement("MouseSensitivity")]
	public float mouseSensitivity;

	/// <summary>
	/// key sensitivity to pivot
	/// </summary>
	[XmlElement("KeySensitivityPivot")]
	public float keySensitivityPivot;

	/// <summary>
	/// Forward/Backward keyboard sensitivity
	/// </summary>
	[XmlElement("KeySensitivityFwdBwd")]
	public float keySensitivityFwdBwd;

	/// <summary>
	/// Strafe keyboard sensitivity
	/// </summary>
	[XmlElement("KeySensitivityStrafe")]
	public float keySensitivityStrafe;

	/// <summary>
	/// maximum look angle (X)
	/// </summary>
	[XmlElement("MaximumX")]
	public float maximumX;

	/// <summary>
	/// minimum look angle (X)
	/// </summary>
	[XmlElement("MinimumX")]
	public float minimumX;


	/// <summary>
	/// maximum look angle (Y)
	/// </summary>
	[XmlElement("MaximumY")]
	public float maximumY;

	/// <summary>
	/// minimum look angle (Y)
	/// </summary>
	[XmlElement("MinimumY")]
	public float minimumY;


	private static VRNControls theInstance = null;

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk. call Save(string, string)
	/// </summary>
	public override void Save()
	{
		base.Save(Application.persistentDataPath, CONTROLS_FILENAME, typeof(VRNControls));
	}

	/// <summary>
	/// Loads the controls settings.
	/// </summary>
	/// <returns></returns>
	public static VRNControls Load()
	{
		theInstance = (VRNControls)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + CONTROLS_FILENAME, typeof(VRNControls), theInstance);
		return theInstance;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VRNControls"/> class. Call <see cref="Load"/> instead!
	/// </summary>
	public VRNControls()
	{
		this.stickSensitivityX = 135f;
		this.stickSensitivityY = -1.2f;
		
		this.minimumX = -360;
		this.maximumX = 360;
		this.maximumY = 60;
		this.minimumY = -60;

		this.keySensitivityFwdBwd = 5;
		this.keySensitivityPivot = 70;
		this.keySensitivityStrafe = 5;

		this.mouseSensitivity = 0f;
	}
}

