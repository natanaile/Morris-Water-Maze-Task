using UnityEngine;
using System.Collections;

public class TreeColourSelector : MonoBehaviour
{

	public Material redTreeMaterial;
	public Material standardTreeMaterial;

	// Use this for initialization
	void Start()
	{
		VRNWaterTaskSettings mWaterTaskSettings = VRNWaterTaskSettings.Load();
		if (mWaterTaskSettings.redTree)
		{
			Material[] replacementMaterials = new Material[] { redTreeMaterial };
			GetComponent<Renderer>().materials = replacementMaterials;
		}
		else
		{
			Material[] replacementMaterials = new Material[] { standardTreeMaterial };
			GetComponent<Renderer>().materials = replacementMaterials;
		}
	}
}
