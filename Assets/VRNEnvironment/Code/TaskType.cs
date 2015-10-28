using UnityEngine;
using System.Collections;

/// <summary>
/// Describe an AbstractVRETask in a meaningful way. 
/// Can be extended to provide additional information or allow more complicated tasks.
/// </summary>
public enum TaskType 
{

	/// <summary>
	/// Move onto a specific area
	/// </summary>
	TYPE_GOTO,

	/// <summary>
	/// press the 'interact' control
	/// </summary>
	TYPE_INTERACT,

	/// <summary>
	/// Look at a target
	/// </summary>
	TYPE_LOOKAT,

	/// <summary>
	/// Maintain a specific quantity for a period of time
	/// </summary>
	TYPE_STAY,

	/// <summary>
	/// Compound tasks may have prerequisite tasks
	/// (E.g. go to the window and interact with it)
	/// </summary>
	TYPE_COMPOUND,

	/// <summary>
	/// A default task, undeclared type
	/// </summary>
	TYPE_DEFAULT

};
