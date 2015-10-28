using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;

public class Popup : MonoBehaviour
{

	public const string TIMEOUT_STRING = "TIMER_TIMEOUT";
	public const string ANY_KEY_STRING = "TIMER_ANY_KEY";

	public enum PopupDismiss
	{
		/// <summary>
		/// Dismiss by pressing any key
		/// </summary>
		ANY_KEY,

		/// <summary>
		/// Dismiss by pressing the 'ack' key
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
	//public GameObject popupTemplate;
	//public Canvas guiPlane;

	public Text titleText;
	public Text contentText;



	//-----------------------------
	// Private instance variables
	//-----------------------------



	/// <summary>
	/// call this to indicate that a popup button was pressed
	/// </summary>
	/// <param name="buttonTitle"></param>
	public delegate void HandleButton(Popup popup, string buttonTitle);

	/// <summary>
	/// This event occurs when the user interacts with this popup. the string argument contains the title of the button that was pressed
	/// </summary>
	private event HandleButton ButtonPress;

	public PopupDismiss dismissMode;
	private Button[] popupButtons = new Button[0];
	public Button[] GetButtons()
	{
		return popupButtons;
	}

	public bool IsActive()
	{
		return gameObject.activeInHierarchy;
	}

	//public static Popup Init()
	//{
	//	return Popup.Init(guiPlane, popupTemplate);
	//}

	public static Popup Init(Canvas parent, GameObject mPopupTemplate)
	{
		GameObject popupObject = Instantiate(mPopupTemplate, parent.transform.position, parent.transform.rotation) as GameObject;
		//popupObject.transform.parent = parent.transform;
		popupObject.transform.SetParent(parent.transform, true);
		popupObject.transform.localScale = Vector3.one; // ensure that popups are the correct size

		Popup mPopup = popupObject.GetComponent<Popup>();

		return mPopup;
	}

	public void SetTitle(string mTitle)
	{
		titleText.text = mTitle;
	}

	public void SetContent(string mContent)
	{
		contentText.text = mContent;
	}

	private void SetButtonText(params string[] buttonText)
	{
		GameObject buttonTemplate = transform.FindChild("ButtonTemplate").gameObject;
		buttonTemplate.SetActive(false);
		ArrayList popupButtonObjects = new ArrayList();

		foreach (string currentButtonLabel in buttonText)
		{
			GameObject currentButton = Instantiate(buttonTemplate) as GameObject;
			currentButton.name = "button_" + currentButtonLabel;
			currentButton.transform.SetParent(gameObject.transform, true);

			Text currentButtonText = currentButton.transform.FindChild("Text").gameObject.GetComponent<Text>();
			currentButtonText.text = currentButtonLabel;

			currentButton.SetActive(true);
			Button buttonObject = currentButton.GetComponent<Button>();
			popupButtonObjects.Add(buttonObject);

			buttonObject.onClick.AddListener(() => ButtonPressedCallback(currentButtonText.text));
		}

		popupButtons = (Button[])popupButtonObjects.ToArray(typeof(Button));
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void Close()
	{
		Destroy(this.gameObject);
	}

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
	/// <param name="title"></param>
	/// <param name="content"></param>
	/// <param name="millis">number of milliseconds to wait before dismissing</param>
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

	private IEnumerator WaitAndClose(float waitTimeSeconds)
	{
		yield return new WaitForSeconds(waitTimeSeconds);

		if (this.dismissMode == PopupDismiss.TIMEOUT)
		{
			Close();
		}
	}

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

	// Update is called every frame, if the MonoBehaviour is enabled (Since v1.0)
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
