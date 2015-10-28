#define UNITY_VR_INTEGRATION
//#define OVR_INTEGRATION

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VRNPlayerManager : MonoBehaviour
{
# if OVR_INTEGRATION
	//------------------------------
	// GameObjects from editor
	//------------------------------
	public GameObject VRPlayer;
	public GameObject MonocularPlayer;
#endif

	// Use this for initialization
	void Awake()
	{
		VRNAssessmentSettings mOptions = VRNAssessmentSettings.Load();

		VRNPlatformSettings settings = VRNPlatformSettings.Load();
		//Cursor.visible = false; 

#if OVR_INTEGRATION
		if (mOptions.ovrEnabled)
		{
			MonocularPlayer.SetActive(false);
			VRPlayer.SetActive(true);
		}
		else
		{
			VRPlayer.SetActive(false);
			MonocularPlayer.SetActive(true);

			if (settings.vsncEnabled)
			{
				QualitySettings.vSyncCount = 1;
			}
			else
			{
				QualitySettings.vSyncCount = 0;
			}
		}
#endif
#if UNITY_VR_INTEGRATION
		UnityEngine.VR.VRSettings.enabled = mOptions.hmdEnabled;

		if (!mOptions.hmdEnabled)
		{
			if (settings.vsncEnabled)
			{
				QualitySettings.vSyncCount = 1;
			}
			else
			{
				QualitySettings.vSyncCount = 0;
			}
		}
#endif



		// set quality settings
		string[] qualityNames = QualitySettings.names;

		switch (settings.qualityLevel)
		{
			case VRNPlatformSettings.QualityLevel.FAST:
				
				QualitySettings.SetQualityLevel(0);
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				Debug.Log("Quality level " + 0 + "/" + (qualityNames.Length - 1));
				break;

			case VRNPlatformSettings.QualityLevel.BEAUTIFUL:
				QualitySettings.SetQualityLevel(qualityNames.Length - 1);
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				Debug.Log("Quality level " + (qualityNames.Length - 1) + "/" + (qualityNames.Length - 1));
				break;

			default:
				Debug.LogError("Unknown quality level: " + settings.qualityLevel + ". defaulting to most basic.");
				QualitySettings.SetQualityLevel(0);
				break;
		}
	}
}
