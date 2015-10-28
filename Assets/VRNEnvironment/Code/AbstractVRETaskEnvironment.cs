using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Not thread safe
/// </summary>
public abstract class AbstractVRETaskEnvironment : MonoBehaviour
{
	//---------------------------------------
	//        MonoBehavior
	//---------------------------------------

	// Use this for initialization
	public virtual void Start()
	{
		AttachPlayer();
		AttachVRTasks();
	}

	// Update is called once per frame
	public virtual void Update()
	{

	}

	//---------------------------------------
	//       VRETaskEnvironment
	//---------------------------------------

	//***********
	// PROPERTIES
	//***********
	/// <summary>
	/// Tasks that this environment is managing. Each task has a unique identifier, that should be its taskDescription.
	/// </summary>
	private Dictionary<string, AbstractVRETask> _tasks;

	/// <summary>
	/// Names of all tasks that this environment is managing. Each task has a unique name.
	/// </summary>
	private HashSet<string> _taskNamesSet;

	/// <summary>
	/// Names of all tasks that this environment is managing. Each task has a unique name.
	/// Changes to this array do not affect the AbstractVRETaskEnvironment.
	/// </summary>
	public string[] taskNamesArray
	{
		get
		{
			string[] taskNamesArrayTEMP = new string[_taskNamesSet.Count];
			_taskNamesSet.CopyTo(taskNamesArrayTEMP);
			return taskNamesArrayTEMP;
		}
	}

	private HashSet<string> _activeTasksSet;
	/// <summary>
	/// Tasks that are active
	/// Changes to this array do not affect the AbstractVRETaskEnvironment.
	/// </summary>
	public string[] activeTasksArray
	{
		get
		{
			string[] activeTasksArrayTEMP = new string[_activeTasksSet.Count];
			_activeTasksSet.CopyTo(activeTasksArrayTEMP);
			return activeTasksArrayTEMP;
		}
	}

	private HashSet<string> _inactiveTasksSet;
	/// <summary>
	/// Tasks that are inactive
	/// Changes to this array do not affect the AbstractVRETaskEnvironment.
	/// </summary>
	public string[] inactiveTasksArray
	{
		get
		{
			string[] inactiveTasksArrayTEMP = new string[_inactiveTasksSet.Count];
			_inactiveTasksSet.CopyTo(inactiveTasksArrayTEMP);
			return inactiveTasksArrayTEMP;
		}
	}

	/// <summary>
	/// The player in the VRE.
	/// </summary>
	public AbstractVREPlayer player { get; private set; }

	private System.Random rng;  // random number generator for getting random tasks

	//***********
	// ACCESSORS
	//***********

	/// <summary>
	/// returns a task with a particular description, or throws a KeyNotFoundException if that task is not in the environment.
	/// </summary>
	/// <param name="taskDescription"></param>
	/// <returns>a task with a particular description, or throws a KeyNotFoundException if that task is not in the environment.</returns>
	public AbstractVRETask GetTask(string taskDescription)
	{
		return _tasks[taskDescription];
	}


	//**********
	// MUTATORS
	//**********

	/// <summary>
	/// Default constructor
	/// </summary>
	public AbstractVRETaskEnvironment()
	{
		_tasks = new Dictionary<string, AbstractVRETask>();
		_taskNamesSet = new HashSet<string>();
		_inactiveTasksSet = new HashSet<string>();
		_activeTasksSet = new HashSet<string>();

		rng = new System.Random();
	}

	/// <summary>
	/// Add a new task to this environment. Throw an ArgumentException if the task already exists.
	/// </summary>
	/// <param name="task">Task to add</param>
	public void AddTask(AbstractVRETask task)
	{
		task.mEnvironment = this;
		_tasks.Add(task.description, task);

		if (!_taskNamesSet.Add(task.description) && _inactiveTasksSet.Add(task.description))
		{
			throw new ArgumentException("Serious error! task did not exist in _tasks, but did exist in _taskNames or _inactiveTasksSet. Ensure that you only edit the task/taskname sets by means of AddTask() and RemoveTask()");
		}
	}

	/// <summary>
	/// Remove a task from this environment. Throw an ArgumentException if the task is not in the environment.
	/// </summary>
	/// <param name="task">the AbstractVRETask to remove</param>
	public void RemoveTask(AbstractVRETask task)
	{
		task.mEnvironment = null;
		if (!_tasks.Remove(task.description))
		{
			throw new ArgumentException("Task " + task.description + " not found in _tasks.");
		}
		_taskNamesSet.Remove(task.description);
		_inactiveTasksSet.Remove(task.description); // just in case
		_activeTasksSet.Remove(task.description); // just in case
	}

	/// <summary>
	/// Remove a task from this environment. Throw an ArgumentException if the task is not in the environment.
	/// </summary>
	/// <param name="taskDescription">the description for a particular AbstractVRETask to remove</param>
	public void RemoveTask(string taskDescription)
	{
		AbstractVRETask task = GetTask(taskDescription);
		RemoveTask(task);
	}


	/// <summary>
	/// Activate a specific task from the task list (i.e. set its 'isTaskActive' property to true). 
	/// Successive calls do nothing. Throws a KeyNotFoundException if that task is not in the environment.
	/// </summary>
	/// <param name="taskDescription">the description of the task to activate</param>
	public void ActivateTask(string taskDescription)
	{
		SetTaskActive(taskDescription, true);
	}

	/// <summary>
	/// Deactivate a specific task from the task list (i.e. set its 'isTaskActive' property to false). 
	/// Successive calls do nothing. Throws a KeyNotFoundException if that task is not in the environment.
	/// </summary>
	/// <param name="taskIndex"></param>
	public void DeactivateTask(string taskDescription)
	{
		SetTaskActive(taskDescription, false);
	}

	/// <summary>
	/// Shared code for ActivateTask() and DeactivateTask()
	/// </summary>
	/// <param name="taskDescription"></param>
	/// <param name="isActive"></param>
	private void SetTaskActive(string taskDescription, bool isActive)
	{
		if (isActive != _tasks[taskDescription].isTaskActive)
		{
			_tasks[taskDescription].isTaskActive = isActive;

			if (isActive)
			{
				_activeTasksSet.Add(taskDescription);
				_inactiveTasksSet.Remove(taskDescription);
			}
			else
			{
				_activeTasksSet.Remove(taskDescription);
				_inactiveTasksSet.Add(taskDescription);
			}
		}
	}

	//*************
	// CONVENIENCE
	//*************

	/// <summary>
	/// Activates a random task from  the task list, provided that task is not already complete.
	/// Throws a KeyNotFoundException if that task is not in the environment.
	/// </summary>
	/// <returns>the description of the activated task, or null if no task was activated, because there are no inactive tasks.</returns>
	public string ActivateRandomTask()
	{
		string activatedTaskName = null;
		if (inactiveTasksArray.Length > 0)
		{
			int randomTask;
			randomTask = rng.Next(inactiveTasksArray.Length - 1); // don't overflow
			activatedTaskName = inactiveTasksArray[randomTask];
			ActivateTask(activatedTaskName);
		}
		return activatedTaskName;
	}

	/// <summary>
	/// Deactivate a task and remove it from the TaskEnvironment. Destroy its GameObject. 
	/// This function is dangerous because any references to the task will become broken.
	/// </summary>
	/// <param name="taskDescription">which task should be destroyed.</param>
	public void DestroyTask(string taskDescription)
	{
		DeactivateTask(taskDescription);

		Destroy(_tasks[taskDescription].gameObject);
	}

	/// <summary>
	/// Deactivate all tasks that are active
	/// </summary>
	public void DeactivateActiveTasks()
	{
		string[] activeTasks = this.activeTasksArray; // need to make a copy since activeTasks will be changing during the loop
		DeactivateTasks(activeTasks);
	}

	/// <summary>
	/// Deactivate multiple tasks
	/// </summary>
	/// <param name="tasksToDeactivate"></param>
	public void DeactivateTasks(string[] tasksToDeactivate)
	{
		foreach (string taskDescription in tasksToDeactivate)
		{
			DeactivateTask(taskDescription);
		}
	}

	/// <summary>
	/// Add player in the scene to this VREHouseTaskEnvironment
	/// </summary>
	private void AttachPlayer()
	{
		GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("Player");
		if (playersInScene.Length > 1)
		{
			Debug.LogError("Too many players!");
		}
		else if (playersInScene.Length < 1)
		{
			Debug.LogError("No players detected!");
		}
		else
		{
			this.player = playersInScene[0].GetComponent<AbstractVREPlayer>();

			if (null != this.player)
			{
				this.player.environment = this;
			}
			else
			{
				Debug.LogError("Player " + playersInScene[0].name + " needs to have an AbstractVREPlayer component attached!!");
			}
		}
	}


	/// <summary>
	/// Attach VRTasks to appropriate GameObjects
	/// </summary>
	private void AttachVRTasks()
	{
		GameObject[] taskObjs = GameObject.FindGameObjectsWithTag("Task");
		foreach (GameObject taskObj in taskObjs)
		{
			AbstractVRETask mTask = taskObj.GetComponent<AbstractVRETask>() as AbstractVRETask;
			AddTask(mTask);
		}
	}

	//***********
	// CALLBACKS
	//***********
	/// <summary>
	/// Called by a task when it has been completed by a player. Notify player that they have completed
	/// a task, then perform any application-specific processing (such as deleting the task or spawning another
	/// or even ending the game entirely)
	/// </summary>
	/// <param name="objective">the task that was completed</param>
	/// <param name="player">the player that completed the task (if applicable)</param>
	/// <returns></returns>
	public void TaskCompleted(AbstractVRETask objective, AbstractVREPlayer player)
	{
		CompletedTask mCompletedTask = null;
		if (null != objective)
		{
			 mCompletedTask = objective.GetCompletedTask();
		}

		if (null != player)
		{
			player.TaskCompleted(this, mCompletedTask);
		}

		DidCompleteTask(objective, player);
	}

	/// <summary>
	/// Begin the level. If overridden, be sure to call base function.
	/// </summary>
	public virtual void BeginEnvironment()
	{
		VRNAssessmentSettings settings = VRNAssessmentSettings.Load();
		player.BeginPlayer(settings.resetPlayerPosition);
	}

	/// <summary>
	/// Show player a popup that prompts them to do something. Wait for player to acknowledge the popup, 
	/// then do something interesting. (This is a coroutine.)
	/// </summary>
	/// <param name="title">title to show up in popup</param>
	/// <param name="message">message to show up in popup</param>
	/// <param name="playersReadyFunction">function to execute after the player has acknowledged the popup</param>
	/// <param name="functionarg">argument for 'playersReadyFunction'</param>
	/// <param name="functionArgs">more arguments for 'playersReadyFunction'</param>
	/// <returns></returns>
	protected IEnumerator WaitForPlayer(string title, string message, DelayedExecution.DelayedFunctionArgs playersReadyFunction, object functionarg, params object[] functionArgs) // thanks to http://stackoverflow.com/questions/15656925/c-sharp-params-with-a-required-minimum-of-one-value
	{
		bool allPlayersReady = false;
		// check to see if all players are ready

		player.ReadyPrompt(title, message);

		while (!allPlayersReady) // keep doing this until the player is ready
		{
			allPlayersReady = true;

			allPlayersReady &= player.isReadyToBegin;

			if (!allPlayersReady) { yield return 0; }
		}

		object[] args = new object[1 + functionArgs.Length];
		args[0] = functionarg;
		Array.ConstrainedCopy(functionArgs, 0, args, 1, functionArgs.Length);

		playersReadyFunction(args);
	}

	/// <summary>
	/// Show player a popup that prompts them to do something. Wait for player to acknowledge the popup, 
	/// then do something interesting. (This is a coroutine.)
	/// </summary>
	/// <param name="title">title to show up in popup</param>
	/// <param name="message">message to show up in popup</param>
	/// <param name="playersReadyFunction">function to execute after the player has acknowledged the popup</param>
	/// <returns></returns>
	protected IEnumerator WaitForPlayer(string title, string message, DelayedExecution.DelayedFunction playersReadyFunction)
	{
		bool allPlayersReady = false;
		// check to see if all players are ready

		player.ReadyPrompt(title, message);

		while (!allPlayersReady) // keep doing this until the player is ready
		{
			allPlayersReady = true;

			allPlayersReady &= player.isReadyToBegin;

			if (!allPlayersReady) { yield return 0; }
		}

		playersReadyFunction();
	}

	//--------------------------------------
	// Abstract functions to be overridden
	//--------------------------------------

	/// <summary>
	/// Called when a specific task has been completed. concrete implementations
	/// of this class will behave differently depending on their application. This function
	/// can be used to spawn new tasks, advance the game state or some other application-specific behaviour..
	/// </summary>
	/// <param name="objective">Task that was completed</param>
	/// <param name="player">Player that completed the task</param>
	protected abstract void DidCompleteTask(AbstractVRETask objective, AbstractVREPlayer player);
}
