using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Material))]
public class WebcamRender : MonoBehaviour {
	
	private Material webcamMaterial;
	private WebCamTexture webcamTex;

	// Awake is called when the script instance is being loaded (Since v1.0)
	public void Awake()
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		webcamMaterial = renderer.materials[0];
	}

	// Start is called just before any of the Update methods is called the first time (Since v1.0)
	public void Start()
	{
		//if(!ConnectWebcam("Logitech QuickCam Pro 9000"))
		//{
		//	this.gameObject.SetActive(false);
		//}
	}

	/// <summary>
	/// Stop the webcam
	/// </summary>
	/// <returns>true if the webcam was stopped, false if not or if no webcam was running.</returns>
	public bool StopWebcam()
	{
		if (null != webcamTex)
		{
			webcamTex.Stop();
			return webcamTex.isPlaying;
		}
		return false;
	}

	/// <summary>
	/// Connect to a webcam at 320x240 @75fps
	/// </summary>
	/// <param name="webcamName"></param>
	/// <returns></returns>
	public bool ConnectWebcam(string webcamName)
	{
		foreach (WebCamDevice device in WebCamTexture.devices)
		{
			Debug.Log("Device: " + device.name + " Front facing: " + device.isFrontFacing);
		}
		if(null != webcamTex)
		{
			webcamTex.Stop();
		}

		webcamTex = new WebCamTexture(webcamName, 320, 240, 75);

		webcamMaterial.SetTexture(0, webcamTex);
		webcamTex.Play();

		return webcamTex.isPlaying;
	}
	

	// Update is called once per frame
	void Update () {
	
	}
}
