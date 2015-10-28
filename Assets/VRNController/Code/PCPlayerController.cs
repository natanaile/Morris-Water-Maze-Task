using UnityEngine;
using System.Collections;
using System.IO;

/// PCPlayerController interacts with the VRNchair input device, and keyboard/mouse for input.
/// Made by paul (modified version of Mouse Look)
/// Requires the following inputs to be configured in the input manager:
/// 1. Joy_Pitch
///		- deadzone 0.01, Sensitivity 1,
///		- Joystick Y axis
///		
/// 2. Joy_Roll
///		- deadzone 0.01, Sensitivity 1,
///		- Joystick X axis

[RequireComponent(typeof(CapsuleCollider))]
public class PCPlayerController : PlayerController
{
	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------

	public int sensorID = 0;

	//////////////////////
	// JOYSTICK SETTINGS
	//////////////////////

	public float stickSensitivityX = 1.8f; // determined experimentally;
	public float stickSensitivityY = -10f; //(comfortable) //-0.0067f; //(real-life)
	
	public float keySensitivityPivot = 1f;
	public float keySensitivityFwdBwd = 1f;
	public float keySensitivityStrafe = 1f;
	
	public float minimumX = -360F;
	public float maximumX = 360F;
	
	public float minimumY = -60F;
	public float maximumY = 60F;
 
	public float mouseSensitivityX = 75f;

	/// <summary>
	/// height of players eyes above the ground in metres
	/// </summary>
	protected float eyeHeight { get; private set; }

	/// <summary>
	/// height of forehead above eyes (in metres)
	/// </summary>
	public const float foreheadHeight = 0.13f;

	//---------------------------
	// Not exposed to Editor
	//---------------------------

	public IMUSensorReader reader;
	private CapsuleCollider mCollider;

	//------------------------
	// PlayerController implementation
	//------------------------

	//////////////////////
	// DECOUPLED MODE
	//////////////////////

	/// <summary>
	/// Inform the user that they are in Decoupled mode, and that's why the screen is blank.
	/// </summary>
	protected Popup decoupledModePopup;

	protected override void ChangeDecoupled(bool isDecoupled)
	{
		// base is abstract

		if (isDecoupled) // switching TO decoupled mode
		{
			// Show black screen
			GameObject[] playerCamera = GameObject.FindGameObjectsWithTag("PlayerCamera"); // need to tag Camera in editor
			if (playerCamera.Length <= 2)
			{
				//foreach (GameObject anchor in playerCamera)
				//{
				//	Camera camera = anchor.GetComponent<Camera>();
				//	camera.clearFlags = CameraClearFlags.Color;
				//	camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
				//}

				decoupledModePopup = Popup.Init(guiCanvas, mPopupTemplate.gameObject);
				decoupledModePopup.Show("Alert!", "You have entered decoupled mode. The input device is being re-centered.");
			}
		}
		else
		{
			// hide black screen
			GameObject[] playerCamera = GameObject.FindGameObjectsWithTag("PlayerCamera"); // need to tag Camera in editor
			if (playerCamera.Length <= 2)
			{
				//foreach (GameObject anchor in playerCamera)
				//{
				//	Camera camera = anchor.GetComponent<Camera>();
				//	camera.clearFlags = CameraClearFlags.Skybox;
				//	camera.cullingMask = (int)0x7fffffff;
				//	Debug.Log("cull mask everything: " + LayerMask.NameToLayer("Everything"));
				//}

				if (decoupledModePopup != null)
				{
					decoupledModePopup.Close();
				}
			}

			//HACK ensure that arrow points forward
			if (null != reader)
			{
				reader.ResetRotation(this.sensorID);
			}
		}
	}

	//-------------------------------
	// MonoBehaviour Implementation
	//-------------------------------
	public override void Awake()
	{
		base.Awake();
		this.mCollider = GetComponent<CapsuleCollider>();
	}

	public override void Start()
	{
		base.Start();

		// load control settings
		VRNControls controlSettings = VRNControls.Load();

		stickSensitivityX = controlSettings.stickSensitivityX;
		stickSensitivityY = controlSettings.stickSensitivityY;

		keySensitivityPivot = controlSettings.keySensitivityPivot;
		keySensitivityFwdBwd = controlSettings.keySensitivityFwdBwd;
		keySensitivityStrafe = controlSettings.keySensitivityStrafe;

		minimumX = controlSettings.minimumX;
		maximumX = controlSettings.maximumX;

		minimumY = controlSettings.minimumY;
		maximumY = controlSettings.maximumY;

		mouseSensitivityX = controlSettings.mouseSensitivity;

		// player height
		VRNAssessmentSettings assessmentSettings = VRNAssessmentSettings.Load();
		eyeHeight = assessmentSettings.eyeHeight;
		mCollider.height = eyeHeight + foreheadHeight;
		float eyeOffset = (foreheadHeight - eyeHeight) / 2;

		Vector3 colliderCenter = new Vector3(0f, eyeOffset, 0f);
		mCollider.center = colliderCenter;

		// connect to IMU
		try
		{
			VRNPlatformSettings mOptions = VRNPlatformSettings.Load();
			string serialPort = mOptions.serialPortID;
			int baudRate = mOptions.baudRate;

			reader = new IMUSensorReader(serialPort, baudRate);
			reader.OpenSerialPort();
			reader.ResetRotation(this.sensorID);
		}
		catch (IOException)
		{
			Debug.Log("Error connecting to Sensor reader");
		}
	}


	public void OnDestroy()
	{
		if (null != reader)
		{
			reader.Close();
		}
	}


	//-----------------------
	// Wheelchair input
	//-----------------------

	/// <summary>
	/// Check if a joystick is connected, ignore vJoy emulated joysticks.
	/// </summary>
	/// <returns></returns>
	bool IsJoystickConnected()
	{
		bool isConnected = false;
		string[] stickNames = Input.GetJoystickNames();
		isConnected = stickNames.Length > 0;

		return isConnected;
	}

	float lastReaderTry = -1f;
	protected override float GetRotation()
	{
		// base is abstract

		float motionX = 0f;

		if (reader.isConnected)//try reading from IMU
		{
			motionX = -reader.GetRotationChange(this.sensorID) / Time.deltaTime; // this function gets called every frame // flip sign to account for flipped coordinate system of IMU
			//Debug.Log("rotation: " + motionX);
		}
		else
		{
			// try reading from joystick if IMU is disconnected
			if (IsJoystickConnected())
			{
				motionX = Input.GetAxis("Joy_Roll") * stickSensitivityX;
				//Debug.Log("sensorID: " + motionX);
			}

			if (Time.time - lastReaderTry > 5)
			{
				lastReaderTry = Time.time;
				Debug.Log("Try to open serial port... (Time = " + Time.time + "s)");
				reader.OpenSerialPort();
				reader.ResetRotation(this.sensorID);
			}

		}
		// keyboard overrides wheelchair
		float horizontalInput = keySensitivityPivot * Input.GetAxis("Pivot");
		if (horizontalInput != 0f)
		{
			motionX = horizontalInput;
		}

		// mouse overrides keyboard
		horizontalInput = Input.GetAxis("Mouse X") * mouseSensitivityX;
		horizontalInput = Mathf.Clamp(horizontalInput, minimumX, maximumX);
		if (horizontalInput != 0f)
		{
			motionX = horizontalInput;
		}


		//if (motionX > 0)
		//{
		//	Debug.Log("Motion x:" + motionX);
		//}

		return motionX;
	}

	protected override float GetForwardMotion()
	{
		// base is abstract

		float motionY = 0f;

		if (IsJoystickConnected())
		{
			motionY = Input.GetAxis("Joy_Pitch") * stickSensitivityY;
			//Debug.Log("sensorVariant: " + motionY);
		}

		//keyboard input overrides joystick
		float verticalInput = keySensitivityFwdBwd * Input.GetAxis("Vertical");
		if (verticalInput != 0f)
		{
			motionY = verticalInput;
		}

		if (motionY > 0)
		{
			//Debug.Log("Motion y:" + motionY);
		}

		return motionY;
	}

	protected override float GetRightMotion()
	{
		// base is abstract

		float motionX = 0.0f;
		//keyboard input
		float strafeInput = keySensitivityStrafe * Input.GetAxis("Horizontal");
		if (strafeInput != 0f)
		{
			motionX = strafeInput;
		}

		return motionX;
	}
}

