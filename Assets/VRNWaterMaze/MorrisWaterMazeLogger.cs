using UnityEngine;
using System.Collections;

public class MorrisWaterMazeLogger : AdvancedPositionLogger {

	private MorrisWaterTaskEnvironment mEnvironment;


	public override void Start()
	{
		base.Start();

		mEnvironment = GameObject.FindObjectOfType<MorrisWaterTaskEnvironment>();
	}

	public override string GetHeaderLine()
	{
		return base.GetHeaderLine() + ",Showing Target";
	}

	public override string GetDataLine()
	{
		return base.GetDataLine() + "," + mEnvironment.showTargets;
	}
}
