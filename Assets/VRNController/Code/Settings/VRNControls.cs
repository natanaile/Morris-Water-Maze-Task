using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Controls")]
public class VRNControls : AbstractVRNSettings
{
	public const string CONTROLS_FILENAME = "Controls.xml";

	/// <summary>
	/// X axis sensitivity
	/// </summary>
	[XmlElement("StickSensitivityX")]
	public float stickSensitivityX;

	/// <summary>
	/// Y axis sensitivity
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

	public override void Save()
	{
		base.Save(Application.persistentDataPath, CONTROLS_FILENAME, typeof(VRNControls));
	}

	public static VRNControls Load()
	{
		theInstance = (VRNControls)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + CONTROLS_FILENAME, typeof(VRNControls), theInstance);
		return theInstance;
	}

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

