using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// a simple box on the player's screen that shows information. (i.e. text)
/// </summary>
public class HUD : MonoBehaviour {

	/// <summary>
	/// the text to show
	/// </summary>
	public Text hudText;

	/// <summary>
	/// Sets the text of the HUD.
	/// </summary>
	/// <param name="hudMsg">The hud MSG.</param>
	public void SetText(string hudMsg)
	{
		hudText.text = hudMsg;
	}
}
