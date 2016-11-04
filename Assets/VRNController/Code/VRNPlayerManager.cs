using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This class is attached to the VRNPlayerController, and sets up the quality/VR settings 
/// for the unity engine based upon the contents of <see cref="VRNChairSettings"/>.
/// </summary>
public class VRNPlayerManager : MonoBehaviour
{
	// Use this for initialization
	void Awake()
	{
		VRNChairSettings chairSettings = VRNChairSettings.Load();
		Cursor.visible = false; 


		UnityEngine.VR.VRSettings.enabled = chairSettings.hmdEnabled;

		if (!chairSettings.hmdEnabled)
		{
			if (chairSettings.vsyncEnabled)
			{
				QualitySettings.vSyncCount = 1;
			}
			else
			{
				QualitySettings.vSyncCount = 0;
			}
		}


		// set quality settings
		string[] qualityNames = QualitySettings.names;

		switch (chairSettings.qualityLevel)
		{
			case VRNChairSettings.QualityLevel.FAST:
				
				QualitySettings.SetQualityLevel(0);
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				Debug.Log("Quality level " + 0 + "/" + (qualityNames.Length - 1));
				break;

			case VRNChairSettings.QualityLevel.BEAUTIFUL:
				QualitySettings.SetQualityLevel(qualityNames.Length - 1);
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				Debug.Log("Quality level " + (qualityNames.Length - 1) + "/" + (qualityNames.Length - 1));
				break;

			default:
				Debug.LogError("Unknown quality level: " + chairSettings.qualityLevel + ". defaulting to most basic.");
				QualitySettings.SetQualityLevel(0);
				break;
		}
	}
}
