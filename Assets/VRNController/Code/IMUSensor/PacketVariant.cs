/// <summary>
/// Different types of packets are possible
/// </summary>
public enum PacketVariant
{
	/// <summary>
	/// Raw orientation data from an IMU sensor in the form of a quaternion
	/// </summary>
	IMU_DATA = 'Q',

	/// <summary>
	/// Raw linear acceleration data from an accelerometer in the form of a vector3.
	/// </summary>
	ACCEL_DATA = 'A',

	/// <summary>
	/// Control data
	/// </summary>
	CONTROL = 'C',

	/// <summary>
	/// Unrecognized format
	/// </summary>
	UNKNOWN = 'X'
};

// Define an extension method in a non-nested static class. 
public static class PacketVariantMethods
{
	public static BasicSensorPacket InitPacket(this PacketVariant variant, string[] rawDataFields)
	{
		BasicSensorPacket packet = null;
		switch (variant)
		{
			case PacketVariant.IMU_DATA:
				packet = new IMUSensorPacket(rawDataFields);
				break;

			case PacketVariant.ACCEL_DATA:
				packet = new AccelSensorPacket(rawDataFields);
				break;

			default:
				packet = new BasicSensorPacket(rawDataFields);
				break;
		}

		return packet;
	}

	public static BasicSensorPacket InitPacket(this PacketVariant variant, byte[] rawData)
	{
		BasicSensorPacket packet = null;
		switch (variant)
		{
			case PacketVariant.IMU_DATA:
				packet = new IMUSensorPacket(rawData);
				break;

			case PacketVariant.ACCEL_DATA:
				packet = new AccelSensorPacket(rawData);
				break;

			default:
				packet = new BasicSensorPacket(rawData);
				break;
		}

		return packet;
	}
}
