using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("WaterMazeTaskOrder")]
public class VRNWaterTaskOrder : AbstractVRNSettings
{
	public const string WATERTASK_ORDER_FILENAME = "presetpaths/WaterTaskOrder.xml";
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

	[XmlRoot("WaterMazeTask")]
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
	
	/// <summary>
	/// task order
	/// </summary>
	[XmlElement("TasksOrder")]
	public VRNWaterTask[] tasksOrder;

	/// <summary>
	/// Distinguish between different task orders (e.g. quick tasks, egocentric 1, egocentric 2, etc.)
	/// </summary>
	[XmlElement("PresetName")]
	public string presetName;

	public override void Save()
	{
		base.Save(Application.persistentDataPath, WATERTASK_ORDER_FILENAME, typeof(VRNWaterTaskOrder));
	}

	/// <summary>
	/// name of the file that contains this VRNWaterTaskOrder
	/// </summary>
	private string filename;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNTaskOrder
	/// <returns>Return an instance of VRNTemp. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	public static VRNWaterTaskOrder Load()
	{
		return Load(WATERTASK_ORDER_FILENAME);
	}

	/// <summary>
	/// Since we are allowing multiple VRNWaterTaskOrders to exist 
    /// simultaneously, we need to allow multiple instances to exist. By storing
    /// them in a \&gtString, VRNWaterTaskOrder\&lt map, we can ensure that there
    /// is only ever one instance for each file.
	/// </summary>
	/// <typeparam name="String"></typeparam>
	/// <typeparam name="VRNWaterTaskOrder"></typeparam>
	/// <param name="?"></param>
	/// <returns></returns>
    private static Dictionary<string, VRNWaterTaskOrder> theInstances = new Dictionary<string, VRNWaterTaskOrder>();
	
	/// <summary>
	/// Use this in lieu of a constructor. This variant allows a path to be
    /// specified, and allows multiple TaskOrder files to reside in the same
	/// folder.</summary>
	/// <param name="path">path to file, relative to the default settings folder. (E.g.
    /// <code>C:/Users/name/AppData/CompanyName/ProgramName/orders/myOrder.xml</code>)
    /// Call <aref>Load</aref> if you want the default.</param>
	/// <returns>an instance of VRNWaterTaskOrder. If a suitable file exists, the
    /// parameters will be loaded from it. otherwise, default settings will be
    /// used and a new file will be created.</returns>
    public static VRNWaterTaskOrder Load(string path)
    {
        string filepath = path;
        if (filepath.CompareTo("") == 0) // handle empty case
        {
            filepath = WATERTASK_ORDER_FILENAME;
        }
        
        VRNWaterTaskOrder theInstance;
		try
		{
			theInstance = theInstances[filepath];
		} catch (KeyNotFoundException)
		{
			theInstance = null;
		}
        theInstance = (VRNWaterTaskOrder) AbstractVRNSettings.Load(Application.persistentDataPath + "/" + filepath, typeof(VRNWaterTaskOrder), theInstance);
        theInstance.filename = filepath;
        theInstances[filepath] = theInstance;
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