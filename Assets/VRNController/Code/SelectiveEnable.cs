using UnityEngine;
using System.Collections;

public class SelectiveEnable : MonoBehaviour {

	public bool EnabledWithVR;

	// Use this for initialization
	void Start () 
	{
		bool vrEnabled = UnityEngine.VR.VRSettings.enabled;

		this.gameObject.SetActive(!(EnabledWithVR ^ vrEnabled));
	}
}
