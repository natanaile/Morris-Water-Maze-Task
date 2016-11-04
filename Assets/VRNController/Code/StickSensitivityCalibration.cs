using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This class is used by the VRN Controller calibration utility. To use this utility, simply build a project with the Calibration scene (<c>VRNController/Scenes/Calibration</c>) in it. 
/// If you have other scenes, in your build, you can cause the calibration scene to load by setting '<c>LoadBaseCalibration</c>' in <see cref="VRNChairSettings"/>.xml to '<c>TRUE</c>'.
/// </summary>
public class StickSensitivityCalibration : MonoBehaviour {

	/// <summary>
	/// display tools for calibration, and the current values
	/// </summary>
	public GameObject calibrationPanel;

	/// <summary>
	/// show some status
	/// </summary>
	public Text statusTextBox;

	private PCPlayerController mPlayerController;

	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {
		this.calibrationPanel.SetActive(false);
		VRNControls controls = VRNControls.Load();
		controls.keySensitivityFwdBwd = 1.0f; // force unity control
		controls.Save();

		VRNChairSettings chairSettings = VRNChairSettings.Load();
		this.statusTextBox.text = "Sensitivity: " + chairSettings.baseSensitivityY;

		this.mPlayerController = GetComponentInParent<PCPlayerController>();

		this.mPlayerController.SetHudEnabled(true);
		this.mPlayerController.hud.SetText("Press the '1' key to begin calibration");
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () 
	{
		if (Input.GetKeyUp(KeyCode.Alpha1))
		{
			this.calibrationPanel.SetActive(!this.calibrationPanel.activeInHierarchy);

			if (!this.calibrationPanel.activeInHierarchy)
			{
				this.mPlayerController.SetHudEnabled(false);
			}
		}

		if (this.calibrationPanel.activeInHierarchy)
		{
			VRNChairSettings chairSettings = VRNChairSettings.Load();

			if (Input.GetKeyUp(KeyCode.O))
			{
				chairSettings.baseSensitivityY += 0.1f;
				this.statusTextBox.text = "Sensitivity: " + chairSettings.baseSensitivityY;
				//this.mPlayerController.stickSensitivityY = chairSettings.baseSensitivityY;
				chairSettings.Save();
			}

			if (Input.GetKeyUp(KeyCode.L))
			{
				chairSettings.baseSensitivityY -= 0.1f;
				this.statusTextBox.text = "Sensitivity: " + chairSettings.baseSensitivityY;
				//this.mPlayerController.stickSensitivityY = chairSettings.baseSensitivityY;
				chairSettings.Save();
			}
		}
	}
}
