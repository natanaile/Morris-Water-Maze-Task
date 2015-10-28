using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base implementation of CompletedTask interface
/// </summary>
class BasicCompletedTask : CompletedTask
{
	public AbstractVREPlayer player { get; private set; }
	public TaskType type { get; private set; }
	public string description { get; private set; }

	public BasicCompletedTask(AbstractVREPlayer player, TaskType type, string description)
	{
		this.player = player;
		this.type = type;
		this.description = description;
	}
}
