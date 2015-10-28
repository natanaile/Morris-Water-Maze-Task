using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StickSensitivityCalibration : MonoBehaviour {

	public GameObject calibrationPanel;
	public Text statusTextBox;

	private PCPlayerController mPlayerController;

	// Use this for initialization
	void Start () {
		this.calibrationPanel.SetActive(false);
		VRNControls controls = VRNControls.Load();
		this.statusTextBox.text = "Sensitivity: " + controls.stickSensitivityY;

		this.mPlayerController = GetComponentInParent<PCPlayerController>();

		this.mPlayerController.SetHudEnabled(true);
		this.mPlayerController.hud.SetText("Press the '1' key to begin calibration");
	}
	
	// Update is called once per frame
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
			VRNControls controls = VRNControls.Load();

			if (Input.GetKeyUp(KeyCode.O))
			{
				controls.stickSensitivityY += 0.1f;
				this.statusTextBox.text = "Sensitivity: " + controls.stickSensitivityY;
				this.mPlayerController.stickSensitivityY = controls.stickSensitivityY;
				controls.Save();
			}

			if (Input.GetKeyUp(KeyCode.L))
			{
				controls.stickSensitivityY -= 0.1f;
				this.statusTextBox.text = "Sensitivity: " + controls.stickSensitivityY;
				this.mPlayerController.stickSensitivityY = controls.stickSensitivityY;
				controls.Save();
			}
		}
	}
}
