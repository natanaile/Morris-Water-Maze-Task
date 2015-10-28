using UnityEngine;
using System.Collections;

/// <summary>
/// Represent a task. Concrete subclass should implement specific behaviour, including a constructor
/// that sets mType. GetCompletedTask() may  be overridden to return a CompletedTask that includes
/// extra information as necessary.
/// 
/// Extends MonoBehaviour so that subclasses can take advantage of this Task's location in game space.
/// </summary>
public abstract class AbstractVRETask : MonoBehaviour
{
	//---------------------------------------
	//        MonoBehavior
	//---------------------------------------

	// Awake is called when the script instance is being loaded (Since v1.0)
	public virtual void Awake()
	{
		this.description = this.gameObject.name;
	}

	// Use this for initialization
	public virtual void Start()
	{
		didStart = true;
	}

	// Update is called once per frame
	public virtual void FixedUpdate()
	{
		if (this.isTaskActive && DidComplete())
		{
			isCompleted = true;
			mEnvironment.TaskCompleted(this, completedByPlayer); //TODO should this run on a separate thread?
		}
	}

	public virtual void OnDestroy()
	{
		// TODO remove from assigned players to prevent them from having null tasks
	}



	//---------------------------------------
	//        VRETask
	//---------------------------------------

	private bool isInitialized;
	private bool didStart = false;


	private bool _isTaskActive;
	public bool isTaskActive
	{
		get
		{
			return _isTaskActive;
		}

		set
		{
			_isTaskActive = value;
			if (didStart) // prevent UnityExceptions
			{
				//gameObject.SetActive(value);
				DidSetActive(value);
			}

		}
	}

	/// <summary>
	/// this will set to true when the task is completed, and cannot be reset to false.
	/// </summary>
	public bool isCompleted { get; private set; }

	/// <summary>
	/// short description of this specific task
	/// </summary>
	public string description { get; private set; } // private setter allows tasks to be accessed by description

	/// <summary>
	/// The player that completed this task (if applicable)
	/// </summary>
	protected AbstractVREPlayer completedByPlayer { get; set; }

	private AbstractVREPlayer[] _assignedPlayers;
	/// <summary>
	/// Player(s) that are assigned to this task
	/// </summary>
	public AbstractVREPlayer[] assignedPlayers
	{
		get
		{
			return _assignedPlayers;
		}
	}

	/// <summary>
	/// The environment in which this task is located
	/// </summary>
	public AbstractVRETaskEnvironment mEnvironment { get; set; }

	/// <summary>
	/// Default Constructor
	/// </summary>
	public AbstractVRETask()
	{
		ResetTask();
		
		mEnvironment = null;
		description = "";
		isInitialized = false; // set true once initialized
	}

	/// <summary>
	/// Mark this task as incomplete and clear the completedByPlayer
	/// </summary>
	public void ResetTask()
	{
		this.isTaskActive = false;
		this.isCompleted = false;
		this.completedByPlayer = null; // is null until completed

		// do not affect isInitialized or description
	}

	//TODO Reexamine if this is necessary
	/// <summary>
	/// Initialize the task, return false if the task is already initialized
	/// </summary>
	/// <param name="description">task description</param>
	/// <param name="isActive">should the description</param>
	public virtual bool Initialize(string description, bool isActive)
	{
		if (!isInitialized)
		{
			this.description = description;
			this.isTaskActive = isActive;

			isInitialized = true;
			return true; // isInitialized has been changed, so we need to use static returns
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Assign a new player to this task, and assign this task to that player. Successive calls do nothing.
	/// </summary>
	/// <param name="task"></param>
	public void AddAssignedPlayer(AbstractVREPlayer player)
	{
		ArrayList playersList;

		if (null != this.assignedPlayers)
		{
			playersList = new ArrayList(this.assignedPlayers);
		}
		else
		{
			playersList = new ArrayList();
		}

		if (!playersList.Contains(player))
		{
			playersList.Add(player);
			_assignedPlayers = (AbstractVREPlayer[])playersList.ToArray(typeof(AbstractVREPlayer));

			player.AddAssignedTask(this);

			DidAddAssignedPlayer(player);
		}

	}

	/// <summary>
	/// Remove a player from this task, and remove this task from that player.
	/// </summary>
	/// <param name="task"></param>
	public void RemoveAssignedPlayer(AbstractVREPlayer player)
	{
		ArrayList playersList;

		if (null != this.assignedPlayers)
		{
			playersList = new ArrayList(this.assignedPlayers);
		}
		else
		{
			playersList = new ArrayList();
		}

		if (playersList.Contains(player))
		{
			playersList.Remove(player);
			_assignedPlayers = (AbstractVREPlayer[])playersList.ToArray(typeof(AbstractVREPlayer));
			player.RemoveAssignedTask(this);

			DidRemoveAssignedPlayer(player);
		}
	}

	//-------------------------------------------
	// Virtual functions that may be overridden
	//-------------------------------------------

	/// <summary>
	/// This function may be overriden if a custom subclass of CompletedTask 
	/// is to be returned.
	/// </summary>
	/// <returns>A CompletedTask object that describes this Task, or NULL if this 
	/// task is not yet completed. If the task was not completed by a player</returns>
	public virtual CompletedTask GetCompletedTask()
	{
		BasicCompletedTask completedTask = null;
		if (isCompleted)
		{
			completedTask = new BasicCompletedTask(completedByPlayer, mType, description);
		}

		return completedTask;
	}

	/// <summary>
	/// Callback to indicate that a player has been assigned to this task.
	/// </summary>
	/// <param name="player"></param>
	public virtual void DidAddAssignedPlayer(AbstractVREPlayer player)
	{
		// do nothing
	}

	/// <summary>
	/// Callback to indicate that a player has been removed from this task.
	/// </summary>
	/// <param name="player"></param>
	public virtual void DidRemoveAssignedPlayer(AbstractVREPlayer player)
	{
		// do nothing
	}

	//--------------------------------------
	// Abstract functions to be overridden
	//--------------------------------------

	/// <summary>
	/// Declare a type for this task
	/// </summary>
	public abstract TaskType mType { get; } // need to set in concrete subclass

	/// <summary>
	/// Check to see if this task is completed. The means of doing this will vary
	/// depending on the task type (e.g. for a simple GOTO task, you have to check 
	/// the target to see if a player entered it, but for a compound task you will 
	/// have to check multiple sub-tasks.
	/// If the task was completed by a player, make sure to populate completedByPlayer.
	/// This function is only called if the task is active.
	/// </summary>
	/// <returns>false if task was not completed this frame.</returns>
	public abstract bool DidComplete();

	/// <summary>
	/// The 'active' state of this task has changed
	/// </summary>
	/// <param name="active">What has the 'active' state changed to?</param>
	public abstract void DidSetActive(bool isTaskActive);
}
