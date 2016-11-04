
using System;
using UnityEngine;

/// <summary>
/// Record additional data concerning the VRN Chair, specifically the direction the user is looking, and whether or not 'Decoupled Mode' is currently enabled.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class AdvancedPositionLogger: PositionTracker
{
	private PlayerController mPlayerController;

	/// added by Kazu to get Camera direction (orientation)
	private GameObject playerCamera;

	/// <summary>
	/// most recent rotation of camera, relative to its immediate parent (this is actually the <b>global</b> rotation relative to the ground, because the HMD reports its 
	/// rotation <b>globally</b>, and we perform transformations further up the kinematic chain to compensate
	/// for this)
	/// </summary>
	public Quaternion currentCameraRotation { get; private set; }

	/// <summary>
	/// Most recent rotation of the camera, relative to the user's body. This is found by calling <see cref="M:IMUSensorReader.RelativeOrientation()"/> to compute
	/// the relative orientations of the head and the body. (look at the source code for <see cref="Update()"/>).
	/// </summary>
	public Quaternion relativeCameraRotation { get; private set; }



	/// <summary>
	/// Use this for initialization
	/// </summary>
	/// <exception cref="System.NullReferenceException">PositionLogger on player " + gameObject.name + " requires a component that extends PlayerController!</exception>
	public override void Start()
	{
		base.Start();

		playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera"); //added by Kazu, it can be playerCamera = GameObject.Find("Camera");

		if (null == playerCamera)
		{
			Debug.LogError("COULD NOT FIND Player Camera: make sure that the main camera is tagged with 'PlayerCamera'");
		}

		mPlayerController = GetComponent<PlayerController>();
		if (null == mPlayerController)
		{
			throw new NullReferenceException("PositionLogger on player " + gameObject.name + " requires a component that extends PlayerController!");
		}
	}

	/// <summary>
	/// Called every frame. Override in subclasses to add extra functionality (but make sure to call base function!!!)
	/// </summary>
	public override void Update()
	{
		base.Update();

		// compute head rotation relative to the PlayerController GameObject. This is necessary because the HMD computes its orientation GLOBALLY, 
		// and there are several links in between it and the virtual body to account for this.
		currentCameraRotation = playerCamera.transform.localRotation;
		relativeCameraRotation = IMUSensorReader.RelativeOrientation(currentRotation, currentCameraRotation);
	}

	/// <summary>
	/// This function is called each time the DataLogger thread creates a new record. The data needs to be formatted in a comma-separated string with the data in the order specified in GetHeaderLine().
	/// this function may be overridden by child classes to add additional data fields to the log.
	/// </summary>
	/// <returns></returns>
	public override string GetDataLine()
	{
		string baseLine = base.GetDataLine();
		return baseLine + "," + 
			relativeCameraRotation.x + "," + relativeCameraRotation.y + "," + relativeCameraRotation.z + "," + relativeCameraRotation.w + "," + 
			relativeCameraRotation.eulerAngles.x + "," + relativeCameraRotation.eulerAngles.y + "," + relativeCameraRotation.eulerAngles.z + "," + 
			mPlayerController.isDecoupled;
	}

	/// <summary>
	/// This function is called once when the DataLogger thread creates a new log file, and returns the names of each of the data fields. The names need to be formatted in a comma-separated string with the data in the order specified in GetDataLine().
	/// this function may be overridden by child classes to add additional data fields to the log.
	/// </summary>
	/// <returns></returns>
	public override string GetHeaderLine()
	{
		return base.GetHeaderLine() + ",RotationCameraQ x,RotationCameraQ y,RotationCameraQ z,RotationCameraQ w,RotationCamera x,RotationCamera y,RotationCamera z,Is Decoupled";
	}
}