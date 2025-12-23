using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;

/// <summary>
/// A transient popup that is shown to the user and may be dismissed either after a certain amount of time, or by the
/// user pressing a key/button.
/// </summary>
public class Popup : MonoBehaviour
{
	/// <summary>
	/// button macro to indicate that timer has expired. (See <see cref="ButtonPress"/>)
	/// </summary>
	public const string TIMEOUT_STRING = "TIMER_TIMEOUT";
	
	/// <summary>
	/// button macro to indicate that any key has been pressed. (See <see cref="ButtonPress"/>)
	/// </summary>
	public const string ANY_KEY_STRING = "TIMER_ANY_KEY";

	/// <summary>
	/// There are different ways that a particular popup may be dismissed.
	/// </summary>
	public enum PopupDismiss
	{
		/// <summary>
		/// Dismiss by pressing any key
		/// </summary>
		ANY_KEY,

		/// <summary>
		/// Dismiss by pressing the 'ack' key (whatever it may be)
		/// </summary>
		ACK_KEY,

		/// <summary>
		/// Only show for a certain amount of time
		/// </summary>
		TIMEOUT,

		/// <summary>
		/// Do not auto-dismiss
		/// </summary>
		NONE
	}

	//----------------------
	// ASSIGNED IN EDITOR
	//----------------------

	/// <summary>
	/// The title text for the popup
	/// </summary>
	public Text titleText;
	/// <summary>
	/// The content text for the popup
	/// </summary>
	public Text contentText;



	//-----------------------------
	// Private instance variables
	//-----------------------------

	/// <summary>
	/// call this to indicate that a popup button was pressed
	/// </summary>
	/// <param name="popup"></param>
	/// <param name="buttonTitle"></param>
	public delegate void HandleButton(Popup popup, string buttonTitle);

	/// <summary>
	/// This event occurs when the user interacts with this popup. the string argument contains the title of the button that was pressed
	/// </summary>
	private event HandleButton ButtonPress;

	/// <summary>
	/// The means to dismiss this particular popup.
	/// </summary>
	public PopupDismiss dismissMode;

	private Button[] popupButtons = new Button[0];

	/// <summary>
	/// Gets the popup's buttons.
	/// </summary>
	/// <returns></returns>
	public Button[] GetButtons()
	{
		return popupButtons;
	}

	/// <summary>
	/// Determines whether this popup is active.
	/// </summary>
	/// <returns>
	///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
	/// </returns>
	public bool IsActive()
	{
		return gameObject.activeInHierarchy;
	}

	/// <summary>
	/// configure this particular popup according to a template
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="mPopupTemplate"></param>
	/// <returns></returns>
	public static Popup Init(Canvas parent, GameObject mPopupTemplate)
	{
		GameObject popupObject = Instantiate(mPopupTemplate, parent.transform.position, parent.transform.rotation) as GameObject;
		//popupObject.transform.parent = parent.transform;
		popupObject.transform.SetParent(parent.transform, true);
		popupObject.transform.localScale = Vector3.one; // ensure that popups are the correct size

		Popup mPopup = popupObject.GetComponent<Popup>();

		return mPopup;
	}

	/// <summary>
	/// Sets the title.
	/// </summary>
	/// <param name="mTitle">The m title.</param>
	public void SetTitle(string mTitle)
	{
		titleText.text = mTitle;
	}

	/// <summary>
	/// Sets the content.
	/// </summary>
	/// <param name="mContent">Content of the m.</param>
	public void SetContent(string mContent)
	{
		contentText.text = mContent;
	}

	/// <summary>
	/// Sets the button text.
	/// </summary>
	/// <param name="buttonText">The button text.</param>
	private void SetButtonText(params string[] buttonText)
	{
		GameObject buttonTemplate = transform.Find("ButtonTemplate").gameObject;
		buttonTemplate.SetActive(false);
		ArrayList popupButtonObjects = new ArrayList();

		foreach (string currentButtonLabel in buttonText)
		{
			GameObject currentButton = Instantiate(buttonTemplate) as GameObject;
			currentButton.name = "button_" + currentButtonLabel;
			currentButton.transform.SetParent(gameObject.transform, true);

			Text currentButtonText = currentButton.transform.Find("Text").gameObject.GetComponent<Text>();
			currentButtonText.text = currentButtonLabel;

			currentButton.SetActive(true);
			Button buttonObject = currentButton.GetComponent<Button>();
			popupButtonObjects.Add(buttonObject);

			buttonObject.onClick.AddListener(() => ButtonPressedCallback(currentButtonText.text));
		}

		popupButtons = (Button[])popupButtonObjects.ToArray(typeof(Button));
	}

	/// <summary>
	/// Hides this popup.
	/// </summary>
	public void Hide()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Closes this popup.
	/// </summary>
	public void Close()
	{
		Destroy(this.gameObject);
	}

	/// <summary>
	/// Shows this popup.
	/// </summary>
	public void Show()
	{
		gameObject.SetActive(true);
	}

	/// <summary>
	/// Convenience method to set the title, content, buttons and callback all at once
	/// </summary>
	/// <param name="title"></param>
	/// <param name="content"></param>
	/// <param name="buttonCallback"></param>
	/// <param name="buttonText"></param>
	public void Show(string title, string content, HandleButton buttonCallback, params string[] buttonText)
	{
		SetTitle(title);
		SetContent(content);
		SetButtonText(buttonText);
		this.ButtonPress = buttonCallback;
		Show();
		if (buttonText.Length == 0)
		{
			//this.dismissMode = PopupDismiss.ANY_KEY;
			this.dismissMode = PopupDismiss.ACK_KEY;
		}
		else
		{
			this.dismissMode = PopupDismiss.NONE;
		}
	}

	/// <summary>
	/// Show a popup with just a title and content, that is dismissed by pressing any key
	/// </summary>
	/// <param name="title"></param>
	/// <param name="content"></param>
	public void Show(string title, string content)
	{
		SetTitle(title);
		SetContent(content);
		SetButtonText(new string[0]);
		Show();
		//this.dismissMode = PopupDismiss.ANY_KEY;
		this.dismissMode = PopupDismiss.ACK_KEY;
	}

	/// <summary>
	/// Show a popup with just at title and content that is dismissed after a certain amount of time
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="content">The content.</param>
	/// <param name="millis">number of milliseconds to wait before dismissing</param>
	/// <param name="buttonCallback">The button callback.</param>
	/// <param name="buttonText">The button text.</param>
	public void Show(string title, string content, int millis, HandleButton buttonCallback, params string[] buttonText)
	{
		SetTitle(title);
		SetContent(content);
		SetButtonText(buttonText);
		this.ButtonPress = buttonCallback;
		Show();
		this.dismissMode = PopupDismiss.TIMEOUT;

		StartCoroutine(WaitAndClose((float)millis / 1000.0f));
	}

	/// <summary>
	/// A co-routine that dismisses the popup after a certain amount of time, provided
	/// the popup dismissal is <see cref="PopupDismiss.TIMEOUT"/>.
	/// </summary>
	/// <param name="waitTimeSeconds"></param>
	/// <returns></returns>
	private IEnumerator WaitAndClose(float waitTimeSeconds)
	{
		yield return new WaitForSeconds(waitTimeSeconds);

		if (this.dismissMode == PopupDismiss.TIMEOUT)
		{
			Close();
		}
	}

	/// <summary>
	/// this gets attached to the OnClick listener of each button in the GUI.
	/// </summary>
	/// <param name="buttonTitle"></param>
	public void ButtonPressedCallback(string buttonTitle)
	{
		if (ButtonPress != null)
		{
			ButtonPress(this, buttonTitle);
		}
	}

	//-------------------------------
	// Monobehaviour Implementations
	//-------------------------------

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled (Since v1.0)
	/// </summary>
	public void Update()
	{
		// check to see if 'any' key has been hit and notify the client.
		if (gameObject.activeInHierarchy)
		{
			if (dismissMode == PopupDismiss.ANY_KEY)
			{
				if (Input.anyKeyDown)
				{
					if (ButtonPress != null)
					{
						ButtonPress(this, ANY_KEY_STRING);
						Close();
					}
				}
			}

			else if (dismissMode == PopupDismiss.ACK_KEY)
			{
				if (Input.GetButtonDown("Submit"))
				{
					if (ButtonPress != null)
					{
						ButtonPress(this, "Submit");
						Close();
					}
				}
			}
		}
	}
}
