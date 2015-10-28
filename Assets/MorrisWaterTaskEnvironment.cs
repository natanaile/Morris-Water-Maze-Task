using UnityEngine;
using System.Collections;
using System;

public class MorrisWaterTaskEnvironment : AbstractVRETaskEnvironment
{

	public float radius;
	public float oasisInset;
	public float playerStartInset;
	public Transform OasisPrefab;


	public bool useStartSettingsFromFile;
	public float timeoutSeconds;
	public Direction oasisStartDirection;
	public Direction playerStartDirection;

	/// <summary>
	/// Describe current trial
	/// </summary>
	VRNWaterTaskOrder.VRNWaterTask task;

	/// <summary>
	/// current target oasis
	/// </summary>
	private Oasis mOasis;

	/// <summary>
	/// when the trial actually started
	/// </summary>
	private float startTime = float.PositiveInfinity;

	private bool isFinishedTask = false;

	private bool _showTargets;
	public bool showTargets
	{
		get
		{
			return _showTargets;
		}

		set
		{
			_showTargets = value;
			// enable/disable current task hint
			GameObject[] playerCameras = GameObject.FindGameObjectsWithTag("PlayerCamera");
			foreach (GameObject cameraObject in playerCameras)
			{
				Camera camera = cameraObject.GetComponent<Camera>();
				if (null != camera)
				{
					if (!showTargets)
					{
						camera.cullingMask = camera.cullingMask & ~LayerMask.GetMask("Targets"); // turn off
					}
					else
					{
						camera.cullingMask = camera.cullingMask | LayerMask.GetMask("Targets"); // turn on
					}

				}
			}
		}
	}


	/// <summary>
	/// Get a position in the Arena, as defined by its cardinal direction.
	/// </summary>
	/// <param name="direction">which direction to use</param>
	/// <param name="radius">radius of arena</param>
	/// <param name="inset">how far in from edge defined by arena</param>
	/// <returns></returns>
	private static Vector3 GetPositionInArena(Direction direction, float radius)
	{
		Vector3 position = new Vector3();

		float angle = direction.AngleDegrees();
		position.z = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
		position.x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);


		return position;
	}

	// Use this for initialization
	public override void Start()
	{
		VRNWaterTaskTemp temp = VRNWaterTaskTemp.Load();
		// load player position, oasis position
		if (useStartSettingsFromFile)
		{
			VRNWaterTaskOrder taskOrder = VRNWaterTaskOrder.Load();			
			task = taskOrder.tasksOrder[(temp.trialNumber - 1) % taskOrder.tasksOrder.Length];

			playerStartDirection = task.playerDirection;
			oasisStartDirection = task.taskDirection;

			VRNWaterTaskSettings settings = VRNWaterTaskSettings.Load();
			timeoutSeconds = settings.hintTimeout;

			// setup file for logging
			string dateFormat = DateTime.Now.ToString("yyyy-MM-dd_HH_mm"); // log hours/minutes
			//string dateFormat = DateTime.Now.ToString("yyyy-MM-dd"); // don't log hours/minutes
			VRNAssessmentSettings assessmentSettings = VRNAssessmentSettings.Load();
			temp.currentAssessmentName = assessmentSettings.currentPatientName + "_" + dateFormat;
			temp.Save();
		}
		else
		{
			// initialize with a standard value
			task = new VRNWaterTaskOrder.VRNWaterTask(oasisStartDirection, playerStartDirection, VRNWaterTaskOrder.TaskType.INVISIBLE_TARGET);
		}

		if (null != OasisPrefab && task.taskType != VRNWaterTaskOrder.TaskType.PROBE)
		{
			mOasis = (Oasis)((Transform)Instantiate(OasisPrefab)).gameObject.GetComponent<Oasis>();
			mOasis.transform.parent = this.transform;
			mOasis.transform.localPosition = GetPositionInArena(this.oasisStartDirection, radius - oasisInset);
		}
		else
		{
			Debug.LogError("NO OASIS PREFAB SET!!!");
		}

		base.Start();

		// set player position
		player.gameObject.transform.parent = this.gameObject.transform;
		player.gameObject.transform.localPosition = GetPositionInArena(playerStartDirection, (radius - playerStartInset));
		player.gameObject.transform.LookAt(this.transform); // look towards centre of arena

		StartCoroutine(WaitForPlayer("Morris Water Task", "Press the spacebar to begin Trial " + temp.trialNumber +".", BeginEnvironment));
	}

	public override void BeginEnvironment()
	{
		base.BeginEnvironment();

		startTime = Time.time;

		showTargets = task.taskType == VRNWaterTaskOrder.TaskType.VISIBLE_TARGET;
	}

	// Update is called once per frame
	public override void Update()
	{
		base.Update();

		if (task.taskType == VRNWaterTaskOrder.TaskType.INVISIBLE_TARGET)
		{
			if (!showTargets && (Time.time - startTime > timeoutSeconds) && !isFinishedTask)
			{
				showTargets = true;
				player.mPlayerController.ShowPopup("Uh-oh!", "Looks like you're having trouble finding the target! Go to the pulsating blue blob.", null, 2000);
			}
		}

		else if (task.taskType == VRNWaterTaskOrder.TaskType.PROBE) // just finish it off since there is not task.
		{
			if ((Time.time - startTime) > timeoutSeconds && !isFinishedTask)
			{
				base.TaskCompleted(null, player);
			}
		}
	}

	protected override void DidCompleteTask(AbstractVRETask objective, AbstractVREPlayer player)
	{
		isFinishedTask = true;

		if (null != objective)
		{
			StartCoroutine(WaitForPlayer("Trial Complete", "You found the target! Press the spacebar to continue.", NextLevel));
		} else
		{
			StartCoroutine(WaitForPlayer("Trial Complete", "There was no target this round! Press the spacebar to continue.", NextLevel));
		}
	}

	public void NextLevel()
	{
		VRNWaterTaskTemp temp = VRNWaterTaskTemp.Load();
		if (useStartSettingsFromFile)
		{
			temp.trialNumber++;
			temp.Save();
		}

		// check if finished
		VRNWaterTaskOrder taskOrder = VRNWaterTaskOrder.Load();
		if (temp.trialNumber > taskOrder.tasksOrder.Length)
		{
			// end it
			StartCoroutine(WaitForPlayer("Assessment Complete", "Press the spacebar to finish.", Application.Quit));
		}
		else
		{
			// continue
			Application.LoadLevel(0); // re-load the level to advance to next trial
		}
	}
}
