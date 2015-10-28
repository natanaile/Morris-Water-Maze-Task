
using System;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class AdvancedPositionLogger: PositionTracker
{
	private PlayerController mPlayerController;

	public override void Start()
	{
		base.Start();

		mPlayerController = GetComponent<PlayerController>();
		if (null == mPlayerController)
		{
			throw new NullReferenceException("PositionLogger on player " + gameObject.name + " requires a component that extends PlayerController!");
		}
	}

	public override string GetDataLine()
	{
		string baseLine = base.GetDataLine();
		return baseLine + "," + mPlayerController.isDecoupled;
	}

	public override string GetHeaderLine()
	{
		return base.GetHeaderLine() + "," + "Is Decoupled";
	}
}