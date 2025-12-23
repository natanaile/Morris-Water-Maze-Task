using UnityEngine;
using System.Collections;

/// <summary>
/// allow a GameObject to only be visible if VR is enabled. 
/// (e.g. apply to a virtual nose that should only appear when using an HMD, and not be visible on a conventional display)
/// </summary>
public class SelectiveEnable : MonoBehaviour {

	/// <summary>
	/// check in Unity Editor whether this GameObject should be enabled if VR is enabled.
	/// </summary>
	public bool EnabledWithVR;

	// Use this for initialization
	void Start () 
	{
		bool vrEnabled = UnityEngine.XR.XRSettings.enabled;

		this.gameObject.SetActive(!(EnabledWithVR ^ vrEnabled));
	}
}
