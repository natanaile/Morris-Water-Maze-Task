using System;
using UnityEngine;

/// <summary>
/// Represent a packet that is coming from an ArduIMU and perform common operations with it. This class can be 
/// extended for specific types of sensor data (e.g. <see cref="IMUSensorPacket"/> or <see cref="AccelSensorPacket"/>).
/// </summary>
public class BasicSensorPacket
{
	//------------------------
	// binary data constants
	//------------------------

	/// <summary>
	/// binary header size (in bytes)
	/// </summary>
	public const int HEADER_SIZE = 8;

	/// <summary>
	/// maximum size of a binary payload (in bytes)
	/// </summary>
	public const int MAX_PAYLOAD_SIZE = 255;

	/// <summary>
	/// maximum size of a binary packet
	/// </summary>
	public const int MAX_PACKET_SIZE = BasicSensorPacket.HEADER_SIZE + BasicSensorPacket.MAX_PAYLOAD_SIZE;


	//------------------------
	// text data constants
	//------------------------
	/// <summary>
	/// where does the payload start in the packet
	/// </summary>
	public const int PAYLOAD_OFFSET = 3; // header size

	/// <summary>
	/// what character splits packets in the raw stream?
	/// </summary>
	public const string PACKET_DELIMITER = "\n";

	/// <summary>
	/// what character splits fields in a packet?
	/// </summary>
	private const string FIELD_DELIMITER = ",";

	//-------------------------
	// Instance Variables
	//-------------------------
	/// <summary>
	/// A unique identifier for this sensor
	/// </summary>
	public int sensorID { get; protected set; }

	/// <summary>
	/// What is the variant of this packet? use this information to correctly cast it.
	/// </summary>
	public PacketVariant variant { get; protected set; }

	/// <summary>
	/// Identify protocol version
	/// </summary>
	public int version { get; protected set; }
	
	/// <summary>
	/// raw data received over the serial port (text)
	/// </summary>
	public string[] rawDataFields { get; protected set; }

	/// <summary>
	/// raw data received from serial port (binary)
	/// </summary>
	public byte[] rawDataBytes { get; protected set; }

	/// <summary>
	/// Timestamp from sensor (milliseconds, generally)
	/// </summary>
	public long timestamp { get; private set; }


	//-----------------
	// Constructors
	//-----------------
	
	/// <summary>
	/// parse binary data to generate a basic packet
	/// </summary>
	/// <param name="rawData">data from ArduIMU</param>
	public BasicSensorPacket(byte[] rawData)
	{
		if (rawData.Length >= HEADER_SIZE)
		{
			// byte 0 is the version
			int mVersion = rawData[0];

			// byte 1 is the variant 
			PacketVariant mVariant;
			try
			{
				mVariant = (PacketVariant)Convert.ToChar(rawData[1]);
			}
			catch (Exception)
			{
				mVariant = PacketVariant.UNKNOWN;
			}


			// byte 2 is the sensor ID
			int mSensorID = rawData[2];

			// bytes 3-6 are the timestamp, but they are big-endian so deal with that
			byte[] timestampBytes =
            {
                rawData[3],
                rawData[4],
                rawData[5],
                rawData[6]
            };

			long mTimestamp;
			try
			{
				mTimestamp = ByteArrayHelpers.GetUnsignedIntFromBytes(timestampBytes, false);
			}
			catch (Exception)
			{
				mTimestamp = -1;
			}
			this.sensorID = mSensorID;
			this.variant = mVariant;
			this.version = mVersion;
			this.timestamp = mTimestamp;
			this.rawDataBytes = rawData;
		}
		else
		{
			this.sensorID = -1;
			this.variant = PacketVariant.UNKNOWN;
			this.version = -1;
			this.timestamp = -1;
			this.rawDataBytes = rawData;
		}

		this.rawDataFields = new String[0];
	}

	/// <summary>
	/// Parse incoming text data to generate a basic packet
	/// </summary>
	/// <param name="rawDataFields">Separate each token of the incoming packet depending on the delimiter (might be a ',' or a ';', who knows?)</param>
	public BasicSensorPacket(String[] rawDataFields)
	{
		if (rawDataFields.Length >= PAYLOAD_OFFSET)
		{
			// index 0 is the version
			int mVersion = -1;
			try
			{
				mVersion = Convert.ToInt32(rawDataFields[0]);
			}
			catch (Exception)
			{
				throw new FormatException(rawDataFields[0] + " could not be parsed as an integer. ");
			}

			// index 1 is the variant 
			PacketVariant mVariant;
			mVariant = (PacketVariant)Convert.ToChar(rawDataFields[1]);

			// index 2 is the sensor ID
			int mSensorID = -1;
			try
			{
				mSensorID = Convert.ToInt32(rawDataFields[2]);
			}
			catch (Exception)
			{
				throw new FormatException(rawDataFields[2] + " could not be parsed as an integer. ");
			}

			//// index 3 is the timestamp
			//long mTimestamp = -1;
			//try
			//{
			//	mTimestamp = Convert.ToUInt32(rawDataFields[3], 16);
			//}
			//catch (Exception)
			//{
			//	throw new FormatException(rawDataFields[3] + " could not be parsed as an integer. ");
			//}

			this.sensorID = mSensorID;
			this.variant = mVariant;
			this.version = mVersion;
			//this.timestamp = mTimestamp;
			this.rawDataFields = rawDataFields;
		}
		else
		{
			this.sensorID = -1;
			this.variant = PacketVariant.UNKNOWN;
			this.version = -1;
			this.timestamp = -1;
			this.rawDataFields = rawDataFields;
		}

		this.rawDataBytes = new byte[0];
	}

	/// <summary>
	/// for simpler abstraction. When a new sensor variant is added, choose which sensor to initialize
	/// </summary>
	/// <returns></returns>
	private static BasicSensorPacket ChoosePacket(string[] rawDataFields, PacketVariant variant)
	{
		return variant.InitPacket(rawDataFields);
	}

	/// <summary>
	/// Call this function to parse a raw text stream into a packet. If this function recognizes the packet's type, that packet will be parsed into the correct type of packet (e.g. IMUPacket)
	/// </summary>
	/// <param name="fieldDelimiters">may be null if default delimiters are to be used</param>
	/// <param name="rawData"></param>
	/// <returns></returns>
	public static BasicSensorPacket ParseData(char[] fieldDelimiters, string rawData)
	{
		// figure out which characters to use for delimiting
		char[] localFieldDelimiters = FIELD_DELIMITER.ToCharArray();
		if (null != fieldDelimiters)
		{
			localFieldDelimiters = fieldDelimiters;
		}

		// split into an array to make things easier
		string[] rawDataFields = rawData.Split(localFieldDelimiters);

		BasicSensorPacket packet = null;
		if (rawDataFields.Length > 3) // version number, type, sensor ID ( at least 3 fields)
		{
			// assume that index 1 is the variant 
			PacketVariant mVariant;
			try
			{
				mVariant = (PacketVariant)Convert.ToChar(rawDataFields[1]);
			}
			catch (Exception)
			{
				mVariant = PacketVariant.UNKNOWN;
			}

			try
			{
				packet = mVariant.InitPacket(rawDataFields);
			}
			catch (Exception ex)
			{
				Debug.Log("Could not initialize packet with data " + rawDataFields + " Exception details:\n" + ex);
				packet = PacketVariant.UNKNOWN.InitPacket(rawDataFields);
			}
		}

		return packet;
	}

	/// <summary>
	/// Call this function to parse a raw byte stream into a packet. If this function recognizes the packet's type, that packet will be parsed into the correct subclass of <see cref="BasicSensorPacket"/> (e.g. <see cref="IMUSensorPacket"/>)
	/// </summary>
	/// <param name="data">array of bytes that have been read from the ArduIMU.</param>
	/// <returns>A subclass of <see cref="BasicSensorPacket"/></returns>
	public static BasicSensorPacket ParseData(byte[] data)
	{
		BasicSensorPacket packet = null;
		if (data.Length >= HEADER_SIZE) // full header
		{

			// byte 1 is the variant 
			PacketVariant mVariant;
			try
			{
				mVariant = (PacketVariant)Convert.ToChar(data[1]);
			}
			catch (Exception)
			{
				mVariant = PacketVariant.UNKNOWN;
			}
			
			packet = mVariant.InitPacket(data);

		}

		return packet;
	}


}