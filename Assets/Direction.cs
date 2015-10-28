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
	public static float AngleDegrees(this Direction direction)
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
}