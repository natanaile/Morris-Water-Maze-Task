/// <summary>
/// Different types of packets can be received from the ArduIMU. This enum allows a packet to
/// be initialized once its variant is known.
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

/// <summary>
/// Define an extension method in a non-nested static class. 
/// </summary>
public static class PacketVariantMethods
{
	/// <summary>
	/// Initialize a <see cref="BasicSensorPacket"/> using the raw data stored in <paramref name="rawDataFields"/>. This
	/// function populates a structured packet, depending on the variant.
	/// </summary>
	/// <param name="variant">variant of packet data in <c>rawDataFields</c></param>
	/// <param name="rawDataFields">array of strings read from ArduIMU in String mode</param>
	/// <returns></returns>
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

	/// <summary>
	/// Initialize a <see cref="BasicSensorPacket"/> using the raw data stored in <paramref name="rawData"/>. This
	/// function populates a structured packet, depending on the variant.
	/// </summary>
	/// <param name="variant">variant of packet data in <c>rawDataFields</c></param>
	/// <param name="rawData">array of bytes read from ArduIMU in Binary mode</param>
	/// <returns></returns>
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
