using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represent a path through the VRN
/// </summary>
public class VRNPath
{

	//-----------------------
	// PROPERTIES
	//-----------------------

	private ArrayList pointsList;
	/// <summary>
	/// an array of points that represent this path
	/// </summary>
	public VRNPoint3[] points
	{
		get
		{
			return (VRNPoint3[])pointsList.ToArray(typeof(VRNPoint3));
		}

		private set { }
	}

	/// <summary>
	/// Has this path been spatially normalized?
	/// </summary>
	public bool isNormalized { get; private set; }

	/// <summary>
	/// The distance covered by this path
	/// </summary>
	public double distance { get; private set; }

	private readonly object synclock = new Object(); // for synchronization

	//-----------------
	// Constructors
	//-----------------

	/// <summary>
	/// Default constructor
	/// </summary>
	public VRNPath()
	{
		pointsList = new ArrayList();
		isNormalized = false;
		distance = 0;
	}

	public VRNPath(VRNPoint3[] points)
	{
		pointsList = new ArrayList(points);
		isNormalized = false;
		distance = ComputeDistance(points);
	}

	public VRNPath(string pathToCSV)
	{
		isNormalized = false;
		LinkedList<VRNPoint3> data = new LinkedList<VRNPoint3>();

		StreamReader pathDataReader = File.OpenText(pathToCSV);
		string line = pathDataReader.ReadLine();
		if (null != line)
		{
			string[] titles = line.Split(new char[] { ',' });

			int x_column = GetIndex(titles, "position x", true, true);
			int y_column = GetIndex(titles, "position y", true, true);
			int z_column = GetIndex(titles, "position z", true, true);

			line = pathDataReader.ReadLine();
			while (null != line)
			{
				string[] dataFields = line.Split(new char[] { ',' });
				VRNPoint3 point = new VRNPoint3(
					Convert.ToDouble(dataFields[x_column]),
					Convert.ToDouble(dataFields[y_column]),
					Convert.ToDouble(dataFields[z_column]));

				data.AddLast(point);
				line = pathDataReader.ReadLine();
			}

			pointsList = new ArrayList(data.Count);

			LinkedListNode<VRNPoint3> node = data.First;
			for(int index = 0; index < data.Count; index ++)
			{
				pointsList.Add(node.Value);
				node = node.Next;
			}


			distance = ComputeDistance(this.points);
		}


		else // title line is null
		{
			pointsList = new ArrayList();
			distance = 0;
		}

	}

	//-----------------
	// ACCESSORS
	//-----------------

	protected static double ComputeDistance(VRNPoint3[] points)
	{
		double dist = 0.0;
		// traverse the list of points and add all the distances
		if (points.Length > 0)
		{
			for (int index = 1; index < points.Length; index++)
			{
				dist += points[index - 1].ComputeDistance(points[index]);
			}
		}

		return dist;
	}

	//-----------------
	// MUTATORS
	//-----------------

	/// <summary>
	/// Normalize the path according to the path-normalization algorithm
	/// </summary>
	public void Normalize()
	{
		// TODO: Implement
	}

	/// <summary>
	/// Add a new point and recompute the distance. Thread-safe.
	/// </summary>
	/// <param name="point">Point to add</param>
	public void AddPoint(VRNPoint3 point)
	{
		lock (synclock)
		{
			isNormalized = false;
			int numPoints = pointsList.Count;
			if (numPoints > 0)
			{
				VRNPoint3 lastPoint = (VRNPoint3)pointsList[numPoints - 1];
				distance += lastPoint.ComputeDistance(point);
			}
			else
			{
				distance = 0.0;
			}

			pointsList.Add(point);
		}
	}

	/// <summary>
	/// Remove a point and recompute the distance. Thread-safe.
	/// </summary>
	/// <param name="pointIndex">index of the point to remove</param>
	public void RemovePoint(int pointIndex)
	{
		lock (synclock)
		{
			pointsList.RemoveAt(pointIndex);
			distance = ComputeDistance(points);
		}
	}

	//---------------------
	// Other Helpers
	//---------------------

	/// <summary>
	/// find the index of <code>searchString</code> in the array <code>strings</code>, of -1 if <code>searchString</code> could not be found
	/// </summary>
	/// <param name="strings"></param>
	/// <param name="searchString"></param>
	/// <param name="startsWith">just check to see if strings[index] starts with searchString?</param>
	/// <param name="ignoreCase">ignore case?</param>
	/// <returns></returns>
	public static int GetIndex(string[] strings, string searchString, bool startsWith = false, bool ignoreCase = false)
	{
		int index = -1;

		// linear search cuz it's easy
		for (int searchIndex = 0; searchIndex < strings.Length; searchIndex++)
		{
			if (startsWith)
			{
				if (ignoreCase)
				{
					if (strings[searchIndex].ToUpper().Contains(searchString.ToUpper()))
					{
						index = searchIndex;
						break;
					}
				}
				else // ignoreCase == false
				{
					if (strings[searchIndex].Contains(searchString))
					{
						index = searchIndex;
						break;
					}
				}
			}
			else // startsWith == false
			{
				if (ignoreCase)
				{
					if (strings[searchIndex].ToUpper().Equals(searchString.ToUpper()))
					{
						index = searchIndex;
						break;
					}
				}
				else // ignoreCase == false
				{
					if (strings[searchIndex].Equals(searchString))
					{
						index = searchIndex;
						break;
					}
				}
			}

		}

		return index;
	}
}
