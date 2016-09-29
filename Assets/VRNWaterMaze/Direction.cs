using System;
using System.Collections.Generic;
public enum Direction
{
	NORTH,
	SOUTH,
	EAST,
	WEST,
	NORTHEAST,
	NORTHWEST,
	SOUTHEAST,
	SOUTHWEST
};

public static class DirectionExtension
{
	public static float AngleDegrees(this Direction direction, int numSegments)
	{
		if (Direction.NORTH == direction)
		{
			return 0.0f;
		}
		else
		{
			float segmentSize = (float)360.0 / numSegments;

			Dictionary<Direction, int> segmentOffsetLookup = new Dictionary<Direction, int>();

			switch (numSegments)
			{
				case 2:
					segmentOffsetLookup[Direction.SOUTH] = 1;
					break;

				case 3:
					segmentOffsetLookup[Direction.SOUTHEAST] = 1;
					segmentOffsetLookup[Direction.SOUTHWEST] = 2;
					break;

				case 4:
					segmentOffsetLookup[Direction.EAST] = 1;
					segmentOffsetLookup[Direction.SOUTH] = 2;
					segmentOffsetLookup[Direction.WEST] = 3;
					break;

				case 5:
					segmentOffsetLookup[Direction.NORTHEAST] = 1;
					segmentOffsetLookup[Direction.SOUTHEAST] = 2;
					segmentOffsetLookup[Direction.SOUTHWEST] = 3;
					segmentOffsetLookup[Direction.NORTHWEST] = 4;
					break;
				case 6:
					segmentOffsetLookup[Direction.NORTHEAST] = 1;
					segmentOffsetLookup[Direction.SOUTHEAST] = 2;
					segmentOffsetLookup[Direction.SOUTH] = 3;
					segmentOffsetLookup[Direction.SOUTHWEST] = 4;
					segmentOffsetLookup[Direction.NORTHWEST] = 5;
					break;
				case 8:
					segmentOffsetLookup[Direction.NORTHEAST] = 1;
					segmentOffsetLookup[Direction.EAST] = 2;
					segmentOffsetLookup[Direction.SOUTHEAST] = 3;
					segmentOffsetLookup[Direction.SOUTH] = 4;
					segmentOffsetLookup[Direction.SOUTHWEST] = 5;
					segmentOffsetLookup[Direction.WEST] = 6;
					segmentOffsetLookup[Direction.NORTHWEST] = 7;
					break;

				default:
					throw new System.ArgumentException("Direction.cs||Invalid number of segments: " + numSegments);

			}

			float result = -1.0f;
			try
			{
				result = segmentOffsetLookup[direction] * segmentSize;
			} catch (KeyNotFoundException)
			{
				throw new ArgumentException("Direction.cs||Invalid direction " + direction + " for number of segments: " + numSegments);
			}
			return result;
		}
	}

	public static float AngleDegrees8(this Direction direction)
	{
		switch (direction)
		{
			case Direction.NORTH:
				return 0f;

			case Direction.EAST:
				return 90f;

			case Direction.SOUTH:
				return 180f;

			case Direction.WEST:
				return 270f;

			case Direction.NORTHEAST:
				return 45f;

			case Direction.SOUTHEAST:
				return 135f;

			case Direction.SOUTHWEST:
				return 225f;

			case Direction.NORTHWEST:
				return 315f;

			default:
				throw new System.ArgumentException("Invalid direction: " + direction);
		}
	}

	public static float AngleDegrees6(this Direction direction)
	{
		switch (direction)
		{
			case Direction.NORTH:
				return 0f;

			case Direction.SOUTH:
				return 180f;

			case Direction.NORTHEAST:
				return 60f;

			case Direction.SOUTHEAST:
				return 120f;

			case Direction.SOUTHWEST:
				return 240f;

			case Direction.NORTHWEST:
				return 300f;

			default:
				throw new System.ArgumentException("Invalid direction: " + direction);
		}
	}

	static int Main(string[] args)
	{
		System.Console.WriteLine("Direction main");
		try
		{
			System.Console.WriteLine("North, 2 segments -> {0:f} (should be 0)", AngleDegrees(Direction.NORTH, 2));
			System.Console.WriteLine("South, 2 segments -> {0:f} (should be 180)", AngleDegrees(Direction.SOUTH, 2));
			System.Console.WriteLine("Northwest, 5 segments -> {0:f} (should be 288)", AngleDegrees(Direction.NORTHWEST, 5));
			System.Console.WriteLine("East, 6 segments -> {0:f} (should be ??)", AngleDegrees(Direction.EAST, 6));
			System.Console.WriteLine("East, 8 segments -> {0:f} (should be 90)", AngleDegrees(Direction.EAST, 8));

		}
		catch (Exception ex)
		{
			System.Console.WriteLine(ex);
		}
		return 0;
	}
}