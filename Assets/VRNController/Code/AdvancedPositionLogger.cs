
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
	private GameObject PlayerCamera;
	public Quaternion CurrentCameraRotation { get; private set; }
	public Quaternion RelativeCameraRotation { get; private set; }

	public override void Start()
	{
		base.Start();

		PlayerCamera = GameObject.FindGameObjectWithTag("PlayerCamera"); //added by Kazu, it can be PlayerCamera = GameObject.Find("Camera");

		mPlayerController = GetComponent<PlayerController>();
		if (null == mPlayerController)
		{
			throw new NullReferenceException("PositionLogger on player " + gameObject.name + " requires a component that extends PlayerController!");
		}
	}

	public override void Update()
	{
		base.Update();

		// compute head rotation relative to the PlayerController GameObject. This is necessary because the HMD computes its orientation GLOBALLY, 
		// and there are several links in between it and the virtual body to account for this.
		CurrentCameraRotation = PlayerCamera.transform.localRotation;
		RelativeCameraRotation = IMUSensorReader.RelativeOrientation(currentRotation, CurrentCameraRotation);
	}

	public override string GetDataLine()
	{
		string baseLine = base.GetDataLine();
		return baseLine + "," + 
			RelativeCameraRotation.x + "," + RelativeCameraRotation.y + "," + RelativeCameraRotation.z + "," + RelativeCameraRotation.w + "," + 
			RelativeCameraRotation.eulerAngles.x + "," + RelativeCameraRotation.eulerAngles.y + "," + RelativeCameraRotation.eulerAngles.z + "," + 
			mPlayerController.isDecoupled;
	}

	public override string GetHeaderLine()
	{
		return base.GetHeaderLine() + ",RotationCameraQ x,RotationCameraQ y,RotationCameraQ z,RotationCameraQ w,RotationCamera x,RotationCamera y,RotationCamera z,Is Decoupled";
	}
}