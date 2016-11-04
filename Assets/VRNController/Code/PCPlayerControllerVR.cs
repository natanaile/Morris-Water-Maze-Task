using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
///  PCPlayerControllerVR interacts with the HMD, and implements the more advanced features of 'Decoupled Mode', including
///  initializing and managing a <see cref="WebcamRender"/>. It also handles UI elements, and interaction with input 
///  devices by means of its base class(es).
/// </summary>
public class PCPlayerControllerVR : PCPlayerController
{
	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------

	/// <summary>
	/// correct any offsets of the OVRCameraRig
	/// </summary>
	public HmdLock HMDCameraRigCorrection;

	/// <summary>
	/// display a virtual version of the tracker camera (if applicable)
	/// </summary>
	public Transform trackerGameObj;

	//////////////////////
	// DECOUPLED MODE
	//////////////////////

	/// <summary>
	/// Keep track of HMD rotation in global Y axis before decoupling, so that the direction that the person is facing will 
	/// be unchanged after we exit decoupled mode.
	/// </summary>
	private float hmdYRotationBeforeDecoupling;

	/// <summary>
	/// used for displaying real-time live feed.
	/// </summary>
	private WebcamRender webcamRenderPlane;

	/// <summary>
	/// different from base class, when decoupling we need to lock the user's body and allow them to re-locate.
	/// </summary>
	/// <param name="isDecoupled"></param>
	protected override void ChangeDecoupled(bool isDecoupled)
	{
		base.ChangeDecoupled(isDecoupled);

		GameObject[] playerCamera = GameObject.FindGameObjectsWithTag("PlayerCamera"); // need to tag Camera in editor


		VRNChairSettings chairSettings = VRNChairSettings.Load();
		if (isDecoupled) // switching TO decoupled mode
		{
			if (chairSettings.decoupledMode == VRNChairSettings.DecoupledMode.LOCKED_VR)
			{
				// lock the DK2
				if (UnityEngine.VR.VRSettings.enabled)
				{
					HMDCameraRigCorrection.yawLock = true;
				}

				// locked VR mode
				if (playerCamera.Length <= 2)
				{
					webcamRenderPlane.gameObject.SetActive(true);
					if (!webcamRenderPlane.ConnectWebcam(chairSettings.webcamDeviceName))
					{
						webcamRenderPlane.gameObject.SetActive(false); // can't connect to a webcam
					}
				}
			}
			else
			{
				// blank screen
				if (playerCamera.Length <= 2)
				{
					foreach (GameObject anchor in playerCamera)
					{
						Camera camera = anchor.GetComponent<Camera>();
						camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
						camera.clearFlags = CameraClearFlags.Color;
					}
				}

				// grab the rift orientation
				Quaternion hmdOrientationBeforeDecoupling = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				hmdYRotationBeforeDecoupling = hmdOrientationBeforeDecoupling.eulerAngles.y;
			}
		}
		else // switching FROM decoupled
		{

			if (chairSettings.decoupledMode == VRNChairSettings.DecoupledMode.LOCKED_VR)
			{
				if (UnityEngine.VR.VRSettings.enabled)
				{
					HMDCameraRigCorrection.yawLock = false;
				}

				// hide webcam
				webcamRenderPlane.gameObject.SetActive(false);
			}
			else // blank screen
			{
				// show stuff
				foreach (GameObject anchor in playerCamera)
				{
					Camera camera = anchor.GetComponent<Camera>();
					camera.cullingMask = (int)0x7fffffff;
					camera.clearFlags = CameraClearFlags.Skybox;
				}

				// apply the rift orientation
				Quaternion hmdOrientationAfterDecoupling = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				float hmdYRotationAfterDecoupling = hmdOrientationAfterDecoupling.eulerAngles.y;
				HMDCameraRigCorrection.transform.Rotate(Vector3.up, -(hmdYRotationAfterDecoupling - hmdYRotationBeforeDecoupling));
			}

			HmdReset(); // reset body rotation to point forward
		}
	}

	/// <summary>
	/// return the participant to their original position/rotation
	/// </summary>
	public override void ResetPlayer()
	{
		base.ResetPlayer();

		HmdReset();
	}

	//---------------------------
	// Class-specific functions
	//---------------------------

	/// <summary>
	/// Reset the Oculus Rift, and align the person's body with their gaze (assume that they are looking straight ahead)
	/// </summary>
	private void HmdReset()
	{

		// find head global rotation
		Quaternion headOldLocalRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
		Quaternion headOldRotation = HMDCameraRigCorrection.transform.rotation * headOldLocalRotation;

		// find global yaw for PlayerController
		float playerControllerYaw = this.transform.rotation.eulerAngles.y;

		// recenter, because that's the whole point of this
		UnityEngine.VR.InputTracking.Recenter();

		// rotate PlayerController so it lines up with head global rotation, then set HMDCameraRigCorrection to 0
		float rotationCorrection = headOldRotation.eulerAngles.y - playerControllerYaw; // rotation of head relative to body
		Debug.Log("Rotating " + rotationCorrection + " degrees in Space.Self");
		transform.Rotate(0f, rotationCorrection, 0f, Space.Self);


		HMDCameraRigCorrection.ResetCorrection(); // person is looking straight ahead, so align their body with their gaze so as to not break immersion by snapping the camera around
	}

	/// <summary>
	/// Determines whether the webcam feed is currently displaying.
	/// </summary>
	/// <returns>
	///   <c>true</c> if webcam is playing; otherwise, <c>false</c>.
	/// </returns>
	public bool IsWebcamPlaying()
	{
		bool isPlaying = false;

		if(null!= webcamRenderPlane)
		{
			isPlaying = webcamRenderPlane.isActiveAndEnabled;
		}

		return isPlaying;
	}

	//-------------------------------
	// MonoBehaviour Implementation
	//-------------------------------

	/// <summary>
	/// called every frame. Can be overridden in subclasses, but they should call base.Update().
	/// This function moves the PlayerController GameObject by polling the inputs.
	/// </summary>
	public override void Update()
	{
		base.Update();
		// Reset rotations to kill drift
		if (Input.GetButtonDown("OVR Reset"))
		{
			HmdReset();
		}

		Vector3 inverseVector = new Vector3(0, -rotationAngle, 0);
		if (UnityEngine.VR.VRSettings.enabled && !this.isDecoupled)
		{
			// HMD detects rotation in world space, so since the Camera is a child of the PlayerController, we need to undo the PlayerController's rotation
			HMDCameraRigCorrection.transform.Rotate(inverseVector);

		}
	}

	/// <summary>
	/// Start is called just before any of the Update methods is called the first time
	/// </summary>
	public override void Start()
	{
		base.Start();

		UnityEngine.VR.InputTracking.Recenter();

		webcamRenderPlane = GameObject.Find("WebcamRenderPlane").GetComponent<WebcamRender>();
		webcamRenderPlane.gameObject.SetActive(false);
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();

		webcamRenderPlane.StopWebcam();
	}
}