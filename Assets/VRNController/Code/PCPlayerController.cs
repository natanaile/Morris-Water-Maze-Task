using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// PCPlayerController interacts with the VRNchair input device, and keyboard/mouse for input. It 
/// handles UI events using its base class. Inspiration for the mouse handling came from the <see cref="MouseLook"/> class.
/// It also is responsible for deciding whether to load the Calibration level or not. This logic appears in <see cref="Awake()"/>.
/// The Calibration level uses the <see cref="StickSensitivityCalibration"/> to set the base calibration of the VRN Chair, so 
/// it can be used in 1:1 tracking.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class PCPlayerController : PlayerController
{
	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------

	/// <summary>
	/// the sensor id of the ArduIMU to use for monitoring the user's body orientation.
	/// </summary>
	public int sensorID = 0;

	//////////////////////
	// JOYSTICK SETTINGS
	//////////////////////

	/// <summary>
	/// The stick sensitivity x
	/// </summary>
	public float stickSensitivityX = 1.8f; // determined experimentally;
	/// <summary>
	/// The stick sensitivity y
	/// </summary>
	public float stickSensitivityY = -10f; //(comfortable) //-0.0067f; //(real-life)

	/// <summary>
	/// The key sensitivity pivot
	/// </summary>
	public float keySensitivityPivot = 1f;
	/// <summary>
	/// The key sensitivity forward BWD
	/// </summary>
	public float keySensitivityFwdBwd = 1f;
	/// <summary>
	/// The key sensitivity strafe
	/// </summary>
	public float keySensitivityStrafe = 1f;

	/// <summary>
	/// The minimum x
	/// </summary>
	public float minimumX = -360F;
	/// <summary>
	/// The maximum x
	/// </summary>
	public float maximumX = 360F;

	/// <summary>
	/// The minimum y
	/// </summary>
	public float minimumY = -60F;
	/// <summary>
	/// The maximum y
	/// </summary>
	public float maximumY = 60F;

	/// <summary>
	/// The mouse sensitivity x
	/// </summary>
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

	/// <summary>
	/// This provides orientation values from an ArduIMU device.
	/// </summary>
	public IMUSensorReader reader;

	/// <summary>
	/// The collider
	/// </summary>
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

	/// <summary>
	/// Change the value of the Decoupled variable.
	/// </summary>
	/// <param name="isDecoupled">What did decoupled change to?</param>
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

	/// <summary>
	/// Awake is called when the script instance is being loaded
	/// </summary>
	public override void Awake()
	{
		base.Awake();

		// see if we need to load the calibration scene
		VRNChairSettings chairSettings = VRNChairSettings.Load();
		if (chairSettings.loadBaseCalibration && !SceneManager.GetActiveScene().name.Equals("Calibration")) // make sure that we are not already in the calibration level!
		{
			VRNControls controlSettings = VRNControls.Load();
			controlSettings.stickSensitivityY = 1.0f;
			controlSettings.Save();
			SceneManager.LoadScene("Calibration");
		}

		this.mCollider = GetComponent<CapsuleCollider>();
	}

	/// <summary>
	/// Start is called just before any of the Update methods is called the first time
	/// </summary>
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

		// participant height
		VRNChairSettings chairSettings = VRNChairSettings.Load();
		eyeHeight = chairSettings.eyeHeight;
		mCollider.height = eyeHeight + foreheadHeight;
		float eyeOffset = (foreheadHeight - eyeHeight) / 2;

		Vector3 colliderCenter = new Vector3(0f, eyeOffset, 0f);
		mCollider.center = colliderCenter;

		// connect to IMU
		try
		{
			string serialPort = chairSettings.serialPortID;
			int baudRate = chairSettings.baudRate;

			reader = new IMUSensorReader(serialPort, baudRate);
			reader.OpenSerialPort();
			reader.ResetRotation(this.sensorID);
		}
		catch (IOException)
		{
			Debug.Log("Error connecting to Sensor reader");
		}
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed
	/// </summary>
	public virtual void OnDestroy()
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
	/// <summary>
	/// Speed at which to pivot participant's body
	/// </summary>
	/// <returns></returns>
	protected override float GetRotation()
	{
		// base is abstract

		VRNChairSettings chairSettings = VRNChairSettings.Load();

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
				motionX = Input.GetAxis("Joy_Roll") * stickSensitivityX * chairSettings.baseSensitivityX;
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

	/// <summary>
	/// Speed at which to move participant forward/backward
	/// </summary>
	/// <returns></returns>
	protected override float GetForwardMotion()
	{
		// base is abstract

		VRNChairSettings chairSettings = VRNChairSettings.Load();

		float motionY = 0f;

		if (IsJoystickConnected())
		{
			motionY = Input.GetAxis("Joy_Pitch") * stickSensitivityY * chairSettings.baseSensitivityY;
			//Debug.Log("sensorVariant: " + motionY);
		}

		//keyboard input overrides joystick
		float verticalInput = keySensitivityFwdBwd * Input.GetAxis("Vertical");
		if (verticalInput != 0f)
		{
			motionY = verticalInput;
		}

		//if (motionY > 0)
		//{
		//	Debug.Log("Motion y:" + motionY);
		//}

		return motionY;
	}

	/// <summary>
	/// Speed at which to strafe to left and right
	/// </summary>
	/// <returns></returns>
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

		//if (motionX > 0)
		//{
		//	Debug.Log("Motion x:" + motionX);
		//}

		return motionX;
	}
}

