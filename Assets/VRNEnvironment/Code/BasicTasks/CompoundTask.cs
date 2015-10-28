using System.Collections;

/// <summary>
/// A compound task can have child tasks, e.g. starting an engine may consist of a GotoTask and an InteractionTask.
/// </summary>
public class CompoundTask: AbstractVRETask
{
	/// <summary>
	/// prerequisite tasks that must be completed in order for this task to be considered complete
	/// </summary>
	private AbstractVRETask[] children;

	public CompoundTask()
	{
		children = new AbstractVRETask[0];
	}

	public void AddChildTask(AbstractVRETask childTask)
	{
		ArrayList childrenList = new ArrayList(children);
		childrenList.Add(childTask);
		children = (AbstractVRETask[])childrenList.ToArray(typeof(AbstractVRETask));
	}

	/// <summary>
	/// Check children to see if they completed this round
	/// </summary>
	/// <returns>true if all child tasks were completed, and false otherwise. Returns false if there are no children.</returns>
	public override bool DidComplete()
	{
		// base is abstract

		bool isComplete = children.Length > 0; // return false if there are no children

		foreach(AbstractVRETask child in children)
		{
			isComplete &= child.DidComplete();
			if (!isComplete)
			{
				break;
			}
		}

		return isComplete;
	}


	public override TaskType mType
	{
		// base is abstract
		get { return TaskType.TYPE_COMPOUND; }
	}

	public override void DidSetActive(bool isTaskActive)
	{
		// base is abstract
		throw new System.NotImplementedException();
	}
}
