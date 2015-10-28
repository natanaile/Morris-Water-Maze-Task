using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A GotoTask represents a geographical target for a player to get to. It is considered complete if a player is in it for a set amount of time.
/// </summary>

[RequireComponent(typeof(Collider))]
public class GotoTask : AbstractVRETask
{
	/// <summary>
	///  did something enter the trigger area?
	/// </summary>
	private bool didEnter = false;

	/// <summary>
	/// how long have all the players in this task been there?
	/// </summary>
	private Dictionary<AbstractVREPlayer, float> occupyingTimesByPlayer = new Dictionary<AbstractVREPlayer,float>();

	/// <summary>
	/// how long does a player need to stay in the task?
	/// </summary>
	protected float latency = 0f;

	//---------------------
	// MonoBehaviour Stuff
	//---------------------
	public override void Awake()
	{
		base.Awake();

		Collider mCollider = GetComponent<Collider>();
		if (!mCollider.isTrigger)
		{
			mCollider.isTrigger = true; // in case programmer forgot
			Debug.LogWarning(gameObject.name + ": Forgot to configure collider as trigger... automatically configuring it.");
		}
	}

	/// <summary>
	/// A collider entered this trigger box
	/// </summary>
	/// <param name="other"></param>
	public virtual void OnTriggerEnter(UnityEngine.Collider other)
	{
		if (this.isTaskActive)
		{
			if (completedByPlayer == null && !didEnter) // not yet completed
			{
				AbstractVREPlayer occupyingPlayer = other.GetComponent<AbstractVREPlayer>();
				if (occupyingPlayer != null)
				{
					occupyingTimesByPlayer[occupyingPlayer] = Time.time;
				}
			}
		}
	}

	// OnTriggerStay is called once per frame for every Collider other that is touching the trigger
	public virtual void OnTriggerStay(Collider other)
	{
		if (null == completedByPlayer) // don't bother trying if it's already completed
		{
			AbstractVREPlayer occupyingPlayer = other.GetComponent<AbstractVREPlayer>();
			if (occupyingPlayer != null)
			{
				if (occupyingTimesByPlayer.ContainsKey(occupyingPlayer)) // this player is already known
				{
					if (Time.time >= (occupyingTimesByPlayer[occupyingPlayer] + latency))
					{
						didEnter = true;
						completedByPlayer = occupyingPlayer;
					}
				}
			}
		}
	}



	public virtual void OnTriggerExit(UnityEngine.Collider other) // could cause a bug if two players enter simultaneously
	{
		AbstractVREPlayer occupyingPlayer = other.GetComponent<AbstractVREPlayer>();
		if (occupyingPlayer != null)
		{
			occupyingTimesByPlayer.Remove(occupyingPlayer);
		}
	}

	//-----------------------------
	// AbstractVRETask Overrides
	//-----------------------------


	public override bool DidComplete()
	{
		// base is abstract

		if (didEnter)
		{
			didEnter = false;
			return true;
		}
		return false;
	}

	public override TaskType mType
	{
		// base is abstract
		get { return TaskType.TYPE_GOTO; }
	}

	public override CompletedTask GetCompletedTask()
	{
		// ignore base
		return new CompletedGotoTask(completedByPlayer, mType, this.description, gameObject.transform.position);
	}

	public override void DidSetActive(bool active)
	{
		// do nothing, base is abstract
	}
}
