using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("TaskOrder")]
public class VRNWaterTaskOrder : AbstractVRNSettings
{


	public enum TaskType
	{
		/// <summary>
		/// Trial where target is hidden until a timer elapses
		/// </summary>
		INVISIBLE_TARGET,

		/// <summary>
		/// Trial where target is always visible
		/// </summary>
		VISIBLE_TARGET,

		/// <summary>
		/// Trial where there is no platform at all
		/// </summary>
		PROBE
	}

	[XmlRoot("Task")]
	public class VRNWaterTask
	{
		[XmlElement("TaskDirection")]
		public Direction taskDirection;

		[XmlElement("PlayerDirection")]
		public Direction playerDirection;

		[XmlElement("TaskType")]
		public TaskType taskType;

		public VRNWaterTask() : this(Direction.NORTH, Direction.SOUTH, TaskType.VISIBLE_TARGET) { }

		public VRNWaterTask(Direction taskDirection, Direction playerDirection, TaskType taskType)
		{
			this.taskDirection = taskDirection;
			this.playerDirection = playerDirection;
			this.taskType = taskType;
		}
	}

	public const string WATERTASK_ORDER_FILENAME = "WaterTaskOrder.xml";

	/// <summary>
	/// task order
	/// </summary>
	[XmlElement("TasksOrder")]
	public VRNWaterTask[] tasksOrder;

	public override void Save()
	{
		base.Save(Application.persistentDataPath, VRNStaticMembers.TASK_ORDER_FILE_NAME, typeof(VRNTaskOrder));
	}

	private static VRNWaterTaskOrder theInstance = null;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNTaskOrder
	/// <returns>Return an instance of VRNTemp. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNWaterTaskOrder Load()
	{
		theInstance = (VRNWaterTaskOrder)AbstractVRNSettings.Load(Application.persistentDataPath + "/" + WATERTASK_ORDER_FILENAME, typeof(VRNWaterTaskOrder), theInstance);
		return theInstance;
	}

	public VRNWaterTaskOrder()
	{
		this.tasksOrder = new VRNWaterTask[]
		{
			new VRNWaterTask(Direction.NORTHWEST, Direction.SOUTH, TaskType.INVISIBLE_TARGET),
			new VRNWaterTask(Direction.NORTHWEST, Direction.NORTH, TaskType.INVISIBLE_TARGET),
			new VRNWaterTask(Direction.NORTHWEST, Direction.EAST, TaskType.INVISIBLE_TARGET),
			new VRNWaterTask(Direction.NORTHWEST, Direction.WEST, TaskType.INVISIBLE_TARGET),
			new VRNWaterTask(Direction.NORTHWEST, Direction.WEST, TaskType.PROBE),
		};
	}
}