using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CompletedGotoTask: CompletedTask
{
	public AbstractVREPlayer player { get; private set; }
	public TaskType type { get; private set; }
	public string description { get; private set; }
	public Vector3 position;

	public CompletedGotoTask(AbstractVREPlayer player, TaskType type, string description, Vector3 position)
	{
		this.player = player;
		this.type = type;
		this.description = description;
		this.position = position;
	}
}
