using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("TaskOrder")]
public class VRNTaskOrder : AbstractVRNSettings
{
	/// <summary>
	/// Pseudorandom order? if yes, will randomly choose between two successive tasks for each task (only half of tasks in tasksOrder will be used)
	/// </summary>
	[XmlElement("IsPseudoRandom")]
	public bool isPseudoRandom;

	/// <summary>
	/// task order
	/// </summary>
	[XmlElement("TasksOrder")]
	public string[] tasksOrder;

	public override void Save()
	{
		base.Save(Application.persistentDataPath, VRNStaticMembers.TASK_ORDER_FILE_NAME, typeof(VRNTaskOrder));
	}

	private static VRNTaskOrder theInstance = null;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNTaskOrder
	/// <returns>Return an instance of VRNTemp. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNTaskOrder Load()
	{
		theInstance = (VRNTaskOrder)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + VRNStaticMembers.TASK_ORDER_FILE_NAME, typeof(VRNTaskOrder), theInstance);
		return theInstance;
	}

	public VRNTaskOrder()
	{
		this.isPseudoRandom = true;
		this.tasksOrder = new string[]
		{
			"M",
			"O",
			"D",
			"F",
			"J",
			"L",
			"S",
			"U",
			"V",
			"X",
			"G",
			"I",
			"P",
			"R",
			"A",
			"C"
		};
	}
}