#define UNITY_VR_INTEGRATION
//#define OVR_INTEGRATION

using UnityEngine;
using System.Collections;
using System.IO;

/// PCPlayerControllerVR interacts with the Oculus Rift HMD and the VRNchair input device.
/// Made by paul (modified version of Mouse Look)
/// Requires the following inputs to be configured in the input manager:
/// 1. Joy_Pitch
///		- deadzone 0.01, Sensitivity 1,
///		- Joystick Y axis
///		
/// 2. Joy_Roll
///		- deadzone 0.01, Sensitivity 1,
///		- Joystick X axis
///		
/// Requires OVRCameraRig to have the two Eye anchors tagged as "OVRCamera"

#if OVR_INTEGRATION
[RequireComponent(typeof(OVRCameraRig))]
#endif

public class PCPlayerControllerVR : PCPlayerController
{
	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------

	/// <summary>
	/// correct any offsets of the OVRCameraRig
	/// </summary>
	public HmdLock HMDCameraRigCorrection;

#if OVR_INTEGRATION
	///// <summary>
	///// Oculus Rift baby
	///// </summary>
	public OVRCameraRig ovrCameraRig;
#endif

	public Transform trackerGameObj;

	//public bool dontBlank = true;
	//public bool webcamEnabled = true;

	//////////////////////
	// DECOUPLED MODE
	//////////////////////

	/// <summary>
	/// Keep track of HMD rotation in global Y axis before decoupling, so that the direction that the person is facing will 
	/// be unchanged after we exit decoupled mode.
	/// </summary>
	private float hmdYRotationBeforeDecoupling;

	private WebcamRender webcamRenderPlane;

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
#if UNITY_VR_INTEGRATION
				Quaternion hmdOrientationBeforeDecoupling = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				hmdYRotationBeforeDecoupling = hmdOrientationBeforeDecoupling.eulerAngles.y;
#endif

#if OVR_INTEGRATION
				Quaternion hmdOrientationBeforeDecoupling = OVRManager.display.GetHeadPose(0.0).orientation;
				hmdYRotationBeforeDecoupling = hmdOrientationBeforeDecoupling.eulerAngles.y;
#endif
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
				//foreach (GameObject anchor in playerCamera)
				//{
				//	Camera camera = anchor.GetComponent<Camera>();
				//	int x = ~(1 << LayerMask.NameToLayer("webcam"));
				//	camera.cullingMask &= ~(1 << LayerMask.NameToLayer("webcam"));
				//	Debug.Log("cull webcam");
				//}
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
#if OVR_INTEGRATION
				Quaternion hmdOrientationAfterDecoupling = OVRManager.display.GetHeadPose(0.0).orientation;
				float hmdYRotationAfterDecoupling = hmdOrientationAfterDecoupling.eulerAngles.y;
				HMDCameraRigCorrection.transform.Rotate(Vector3.up, -(hmdYRotationAfterDecoupling - hmdYRotationBeforeDecoupling));

				HmdReset(); // reset body rotation to point forward
#else
#if UNITY_VR_INTEGRATION
				Quaternion hmdOrientationAfterDecoupling = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				float hmdYRotationAfterDecoupling = hmdOrientationAfterDecoupling.eulerAngles.y;
				HMDCameraRigCorrection.transform.Rotate(Vector3.up, -(hmdYRotationAfterDecoupling - hmdYRotationBeforeDecoupling));


#endif
#endif
			}

			HmdReset(); // reset body rotation to point forward
		}
	}

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

#if OVR_INTEGRATION

		if (OVRManager.display != null)
		{
			Vector3 oldHeadAngles = OVRManager.display.GetHeadPose(0.0).orientation.eulerAngles;
			OVRManager.display.RecenterPose();
			Vector3 headEulerAngles = OVRManager.display.GetHeadPose(0.0).orientation.eulerAngles;
			if (oldHeadAngles.Equals(headEulerAngles))
			{
				Debug.Log("no difference");
			}
			else
			{
				Debug.Log("different:\n  old: " + oldHeadAngles + ",  new: " + headEulerAngles);
			}


			Vector3 bodyEulerAngles = HMDCameraRigCorrection.transform.localRotation.eulerAngles;
			float rotationCorrection = oldHeadAngles.y + bodyEulerAngles.y; // accumulated errors get passed into body so that head and body are alligned
			Debug.Log("Rotating " + rotationCorrection + " degrees in Space.Self");
			transform.Rotate(0f, rotationCorrection, 0f, Space.Self);


			HMDCameraRigCorrection.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // person is looking straight ahead, so align their body with their gaze so as to not break immersion by snapping the camera around
		}
#else

#if UNITY_VR_INTEGRATION

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

#endif
#endif

	}

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

	public override void Update()
	{
#if OVR_INTEGRATION
		if (!OVRManager.isHSWDisplayed)
		{
#endif
		base.Update();
		// Reset rotations to kill drift
		if (Input.GetButtonDown("OVR Reset"))
		{
			HmdReset();
		}

		Vector3 inverseVector = new Vector3(0, -rotationAngle, 0);

#if UNITY_VR_INTEGRATION
		if (UnityEngine.VR.VRSettings.enabled && !this.isDecoupled)
		{
#endif
			// oculus rift detects rotation in world space, so since the OVRCameraRig is a child of the player controller, we need to undo the playercontroller's rotation
			HMDCameraRigCorrection.transform.Rotate(inverseVector);
#if UNITY_VR_INTEGRATION
		}
#endif

#if OVR_INTEGRATION
			//// position the 3d tracker
			//trackerGameObj.transform.position = ovrCameraRig.trackerAnchor.localPosition;
			//trackerGameObj.transform.rotation = ovrCameraRig.trackerAnchor.localRotation;

		}
#endif

	}

	public override void Start()
	{
		base.Start();

		UnityEngine.VR.InputTracking.Recenter();

		webcamRenderPlane = GameObject.Find("WebcamRenderPlane").GetComponent<WebcamRender>();
		webcamRenderPlane.gameObject.SetActive(false);
	}

	// This function is called when the MonoBehaviour will be destroyed
	public override void OnDestroy()
	{
		base.OnDestroy();

		webcamRenderPlane.StopWebcam();

	}
}

