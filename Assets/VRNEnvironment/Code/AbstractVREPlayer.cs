using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerController))]
public abstract class AbstractVREPlayer : MonoBehaviour
{

	public PlayerController mPlayerController { get; private set; }
	
	//---------------------------------------
	//        MonoBehavior
	//---------------------------------------

	// Awake is called when the script instance is being loaded (Since v1.0)
	public virtual void Awake()
	{
		mPlayerController = GetComponent<PlayerController>();
		if (null == mPlayerController)
		{
			throw new ArgumentException("NO PLAYER CONTROLLER!!!");
		}
	}
	

	// Use this for initialization
	public virtual void Start()
	{
		// can be overridden in concrete subclass

		isReadyToBegin = false; // wait for player to ready up
	}

	// Update is called once per frame
	public virtual void Update()
	{
		// can be overridden in concrete subclass
	}

	public virtual void OnDestroy()
	{
		// TODO remove self from assigned task(s)
	}


	//---------------------------------------
	//       VREPlayer
	//---------------------------------------

	/// <summary>
	/// The name of this player
	/// </summary>
	public string playerName = "Private_Boolean";

	/// <summary>
	/// Player's score
	/// </summary>
	private double score;

	/// <summary>
	/// Tasks that this player has completed
	/// </summary>
	private CompletedTask[] mCompletedTasks;

	private AbstractVRETask[] _mAssignedTasks;
	/// <summary>
	/// Tasks assigned to this player
	/// </summary>
	public AbstractVRETask[] mAssignedTasks
	{
		get
		{
			return _mAssignedTasks;
		}
	}

	/// <summary>
	/// indicate whether player has 'readied up' from most recent call to ReadyPrompt
	/// </summary>
	public bool isReadyToBegin { get; private set; }


	protected void DidReady()
	{
		isReadyToBegin = true;
	}

	/// <summary>
	/// Prompt player to ready up to start
	/// </summary>
	public void ReadyPrompt(string title, string message)
	{
		isReadyToBegin = false;

		mPlayerController.ShowPopup(title, message, PopupResult);
	}

	private void PopupResult(Popup popup, string buttonTitle)
	{
		DidReady();
	}

	/// <summary>
	/// Assign a new task to this player, and assign this player to that task. Successive calls do nothing.
	/// </summary>
	/// <param name="task"></param>
	public void AddAssignedTask(AbstractVRETask task)
	{
		ArrayList tasksList;
		if (null != mAssignedTasks)
		{
			tasksList = new ArrayList(mAssignedTasks);
		} else
		{
			tasksList = new ArrayList();
		}

		if (!tasksList.Contains(task))
		{
			tasksList.Add(task);
			_mAssignedTasks = (AbstractVRETask[])tasksList.ToArray(typeof(AbstractVRETask));
			task.AddAssignedPlayer(this);

			DidAddAssignedTask(task);
		}
		
	}

	/// <summary>
	/// Remove a task from this player, and remove this player from that task.
	/// </summary>
	/// <param name="task"></param>
	public void RemoveAssignedTask(AbstractVRETask task)
	{
		ArrayList tasksList;
		if (null != mAssignedTasks)
		{
			tasksList = new ArrayList(mAssignedTasks);
		}
		else
		{
			tasksList = new ArrayList();
		}
		tasksList.Remove(task);
		_mAssignedTasks = (AbstractVRETask[])tasksList.ToArray(typeof(AbstractVRETask));

		task.RemoveAssignedPlayer(this);

		DidRemoveAssignedTask(task);
	}

	/// <summary>
	/// Local representation of Environment
	/// </summary>
	private AbstractVRETaskEnvironment _environment;
	/// <summary>
	/// Environment in which this AbstractVREPlayer exists
	/// </summary>
	public AbstractVRETaskEnvironment environment
	{
		get { return this._environment; }
		set { this._environment = value; }
	}

	/// <summary>
	/// Default Constructor
	/// </summary>
	public AbstractVREPlayer()
	{
		mCompletedTasks = new CompletedTask[0];
		_mAssignedTasks = new AbstractVRETask[0];
		score = 0;
	}


	/// <summary>
	/// The assessment will begin (may be overridden)  If overridden, be sure to call base function.
	/// </summary>
	protected virtual void BeginPlayer()
	{

	}

	/// <summary>
	/// Begin the player, and choose whether to start at the spawn point.
	/// </summary>
	/// <param name="startAtOrigin"></param>
	public void BeginPlayer(bool startAtOrigin)
	{
		if (startAtOrigin)
		{
			mPlayerController.ResetPlayer();
		}

		BeginPlayer();
	}

	/// <summary>
	/// begin the player, and start at a particular position.
	/// </summary>
	/// <param name="startTransform"></param>
	public void BeginPlayer(Transform startTransform)
	{
		mPlayerController.transform.localPosition = startTransform.localPosition;
		mPlayerController.transform.localRotation = startTransform.localRotation;
		mPlayerController.transform.parent = startTransform.parent;

		BeginPlayer();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>this player's current score</returns>
	public double GetScore()
	{
		return score;
	}

	/// <summary>
	/// Called by the AbstractVRETaskEnvironment in which the task exists to notify this player
	/// that they completed a task.
	/// </summary>
	public void TaskCompleted(AbstractVRETaskEnvironment environment, CompletedTask completedTask)
	{
		if (this._environment.Equals(environment)) //sanity check
		{
			// log
			ArrayList completedTasks = new ArrayList(mCompletedTasks);
			completedTasks.Add(completedTask);
			mCompletedTasks = (CompletedTask[])completedTasks.ToArray(typeof(CompletedTask));

			// allow behaviour for different tasks
			DidCompleteTask(completedTask);
		}
		else
		{
			throw new System.FormatException("player environment does not match task environment");
		}
	}

	//-------------------------------------------
	// Virtual functions that may be overridden
	//-------------------------------------------

	/// <summary>
	/// Callback to indicate that a task has been assigned to this player.
	/// </summary>
	/// <param name="player"></param>
	public virtual void DidAddAssignedTask(AbstractVRETask task)
	{
		// do nothing
	}

	/// <summary>
	/// Callback to indicate that a task has been removed from this player.
	/// </summary>
	/// <param name="player"></param>
	public virtual void DidRemoveAssignedTask(AbstractVRETask task)
	{
		// do nothing
	}

	//--------------------------------------
	// Abstract functions to be overridden
	//--------------------------------------

	/// <summary>
	/// Called when a task of a specific type has been completed. concrete implementations
	/// of this class will behave differently depending on their application. This function
	/// can be used to change the score, trigger an animation, or some other response.
	/// </summary>
	/// <param name="completedTask"></param>
	protected abstract void DidCompleteTask(CompletedTask completedTask);
}
