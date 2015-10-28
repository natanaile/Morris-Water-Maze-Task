using UnityEngine;
using System.Collections;

/// <summary>
/// Provide information about a task that has been completed. Subclass to add additional fields.
/// </summary>
public interface CompletedTask
{
	/// <summary>
	/// The player that completed the task
	/// </summary>
	AbstractVREPlayer player { get; }

	/// <summary>
	/// the type of the task that this represented. 
	/// This field can be used to determine how to cast a 
	/// particular CompletedTask object.
	/// </summary>
	TaskType type { get; }

	/// <summary>
	/// describe the particular AbstractVRETask instance 
	/// that this CompletedTask represents
	/// </summary>
	string description { get; }
}
