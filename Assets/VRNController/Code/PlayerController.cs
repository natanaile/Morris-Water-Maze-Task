using System;
using UnityEngine;

/// <summary>
/// Abstract representation of what PlayerControllers can do. This class interacts with UI devices and handles the 
/// motion of the player. It collects inputs from one of its concrete subclasses (in particular, <see cref="PCPlayerController"/>).
/// </summary>
public abstract class PlayerController : MonoBehaviour
{
	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------
	/// <summary>
	/// Draw GUI elements
	/// </summary>
	public Canvas guiCanvas;
	/// <summary>
	/// The hud
	/// </summary>
	public HUD hud;
	/// <summary>
	/// The m popup template
	/// </summary>
	public Transform mPopupTemplate;
	/// <summary>
	/// The pause menu
	/// </summary>
	public GameObject pauseMenu;
	/// <summary>
	/// The menu button
	/// </summary>
	public GameObject menuButton;

	/// <summary>
	/// The FPS counter
	/// </summary>
	public GameObject FPS_Counter;

	//--------------------------
	// Other instance variables
	//--------------------------
	private bool isPaused = false;

	/// <summary>
	/// number of degrees through which the PlayerController rotates to the right this frame, if not decoupled
	/// </summary>
	public float rotationAngle { get; protected set; }

	/// <summary>
	/// number of meters this PlayerController moves forward this frame, if not decoupled
	/// </summary>
	public float forwardMotion { get; protected set; }

	/// <summary>
	/// number of meters this PlayerController strafes to the right this frame, if not decoupled
	/// </summary>
	public float rightMotion { get; protected set; }

	private Vector3 originPosition;
	private Quaternion originRotation;

	//-----------------------
	// HUD methods
	//-----------------------

	/// <summary>
	/// Show a popup that will not automatically terminate, and pass a notification when a button is selected.
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="content">The content.</param>
	/// <param name="buttonHandler">The button handler.</param>
	/// <param name="buttonText">The button text.</param>
	public void ShowPopup(string title, string content, Popup.HandleButton buttonHandler, params string[] buttonText)
	{
		Popup mPopup = Popup.Init(guiCanvas, mPopupTemplate.gameObject);
		mPopup.Show(title, content, buttonHandler, buttonText);
	}

	/// <summary>
	/// Show a popup that will automatically terminate when the 'submit' key is pressed
	/// </summary>
	/// <param name="title"></param>
	/// <param name="content"></param>
	public void ShowPopup(string title, string content)
	{
		Popup mPopup = Popup.Init(guiCanvas, mPopupTemplate.gameObject);
		mPopup.Show(title, content);
	}

	/// <summary>
	/// Show a popup that will automatically terminate when a button is clicked, and pass a notification
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="content">The content.</param>
	/// <param name="buttonHandler">The button handler.</param>
	public void ShowPopup(string title, string content, Popup.HandleButton buttonHandler)
	{
		Popup mPopup = Popup.Init(guiCanvas, mPopupTemplate.gameObject);
		mPopup.Show(title, content, buttonHandler);
	}

	/// <summary>
	/// Show a popup that will automatically terminate after 'millis' milliseconds, and pass a notification
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="content">The content.</param>
	/// <param name="buttonHandler">The button handler.</param>
	/// <param name="millis">The time, in milliseconds, to display the popup.</param>
	public void ShowPopup(string title, string content, Popup.HandleButton buttonHandler, int millis)
	{
		Popup mPopup = Popup.Init(guiCanvas, mPopupTemplate.gameObject);
		mPopup.Show(title, content, millis, buttonHandler);
	}

	/// <summary>
	/// Show or hide the HUD
	/// </summary>
	/// <param name="isHudEnabled"></param>
	public void SetHudEnabled(bool isHudEnabled)
	{
		if (null != hud)
		{
			hud.gameObject.SetActive(isHudEnabled);
		}
		else
		{
			Debug.Log("HUD is null for " + gameObject.name + "!!!");
		}
	}

	/// <summary>
	/// Handler for updating HUD text for a particular player
	/// </summary>
	/// <param name="hudText"></param>
	public delegate void UpdateHudHandler(string hudText);

	/// <summary>
	/// put some text in the HUD
	/// </summary>
	/// <param name="hudText"></param>
	public void UpdateHud(string hudText)
	{
		hud.SetText(hudText);
	}

	/// <summary>
	/// Pause the simulation and show the pause menu 
	/// (Code is commented out because this function never worked right and I see no reason to fix it since it is fundamentally useless with the existence of 'Decoupled Mode'.)
	/// </summary>
	/// <param name="shouldPause"></param>
	public void SetSimulationPaused(bool shouldPause)
	{
//		isPaused = shouldPause;
//		if (isPaused)
//		{
//			Time.timeScale = 0;
//			//ToggleGameObject(pauseMenu, true);

//			// show/hide menu button
//#if UNITY_EDITOR || UNITY_STANDALONE
//			menuButton.SetActive(false);
//#else
//		ToggleGameObject(menuButton, false);
//#endif
//		}
//		else
//		{
//			Time.timeScale = 1;
//			//ToggleGameObject(pauseMenu, false);

//			// show/hide menu button
//#if UNITY_EDITOR || UNITY_STANDALONE
//			menuButton.SetActive(false);
//#else
//			ToggleGameObject(menuButton, true);
//#endif
//		}

	}

	//--------------------------
	// Interact with controls
	//--------------------------

	private bool _isDecoupled = false;
	/// <summary>
	/// Decoupled mode for if you need to move the subject but not have the virtual character move.
	/// This is preferable to pausing as it allows the subject to look around freely and not
	/// become simulator-sick.
	/// </summary>
	public bool isDecoupled
	{
		set
		{
			if (_isDecoupled != value) // only do the next bit if we are transitioning
			{
				_isDecoupled = value;
				ChangeDecoupled(value);
			}
		}

		get
		{
			return _isDecoupled;
		}
	}

	/// <summary>
	/// Enable/disable translation for this person
	/// </summary>
	public bool isParalyzed { get; set; }

	/// <summary>
	/// return the participant to their original position/rotation
	/// </summary>
	public virtual void ResetPlayer()
	{
		transform.position = originPosition;
		transform.rotation = originRotation;
	}


	//-------------------------------
	// MonoBehaviour Implementation
	//-------------------------------

	/// <summary>
	/// called every frame. Can be overridden in subclasses, but they should call base.Update().
	/// This function moves the PlayerController GameObject by polling the inputs.
	/// </summary>
	public virtual void Update()
	{
		if (Input.GetButtonDown("Pause"))
		{
			SetSimulationPaused(!isPaused);
		}

		if (Input.GetButtonDown("Toggle_Decoupled_Mode"))
		{
			this.isDecoupled = !this.isDecoupled;
		}

		rotationAngle = 0.0f;
		forwardMotion = 0.0f;

		rotationAngle = Time.deltaTime * GetRotation();
		forwardMotion = Time.deltaTime * GetForwardMotion();
		rightMotion = Time.deltaTime * GetRightMotion();

		if (!this.isDecoupled && !this.isParalyzed)
		{
			Vector3 rotationVector = new Vector3(0, rotationAngle, 0);
			Vector3 translationVector = new Vector3(rightMotion, 0, forwardMotion);

			transform.Rotate(rotationVector);
			transform.Translate(translationVector);
		}
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded
	/// </summary>
	public virtual void Awake()
	{
		SetHudEnabled(false);

		VRNChairSettings chairSettings = VRNChairSettings.Load();
		FPS_Counter.SetActive(chairSettings.fpsCounterEnabled);

#if UNITY_EDITOR || UNITY_STANDALONE
		if (null != menuButton)
		{
			menuButton.SetActive(false);
		}
		else
		{
			Debug.Log("Menu Button is null for " + gameObject.name + "!!!");
		}
#else
		mPopup.Show("Welcome", "Press the 'OK' Button to begin.", "OK");
		Button[] buttons = mPopup.GetButtons();
		buttons[0].onClick.AddListener(BeginSimulation);
		ToggleGameObject(menuButton, true);
#endif

		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	/// <summary>
	/// Start is called just before any of the Update methods is called the first time
	/// </summary>
	public virtual void Start()
	{
		// override in subclass
		originPosition = transform.position;
		originRotation = transform.rotation;
	}

	//------------------------
	// Override in subclasses
	//------------------------

	/// <summary>
	/// Speed at which to pivot participant's body
	/// </summary>
	/// <returns></returns>
	protected abstract float GetRotation();

	/// <summary>
	/// Speed at which to move participant forward/backward
	/// </summary>
	/// <returns></returns>
	protected abstract float GetForwardMotion();

	/// <summary>
	/// Speed at which to strafe to left and right
	/// </summary>
	/// <returns></returns>
	protected abstract float GetRightMotion();

	/// <summary>
	/// Change the value of the Decoupled variable.
	/// </summary>
	/// <param name="isDecoupled">What did decoupled change to?</param>
	protected abstract void ChangeDecoupled(bool isDecoupled);

}