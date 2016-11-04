using UnityEngine; // for Debug statements
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO; // need to enable .NET 2.0

/// <summary>
/// Read input from one or more ArduIMUs and parse it.
/// </summary>
public class SensorReader
{
	/// <summary>
	/// The default baud
	/// </summary>
	public const int DEFAULT_BAUD = 115200;
	/// <summary>
	/// The default port
	/// </summary>
	public const string DEFAULT_PORT = "COM4";

	/// <summary>
	/// Allow the received sensor packets to be indexed by type and by name
	/// </summary>
	struct SensorPacketDictionaryIndex
	{
		public int sensorID;
		public PacketVariant sensorVariant;
	}

	/// <summary>
	/// the serial port from which IMU data will come
	/// </summary>
	public string serialPortID;
	
	/// <summary>
	/// serial port baud
	/// </summary>
	public int baudRate;

	/// <summary>
	/// property that shows whether the serial port is connected.
	/// </summary>
	public bool isConnected { get; private set; }

	/// <summary>
	/// Store the last received sensor packet of each type from each sensor
	/// </summary>
	private Dictionary<SensorPacketDictionaryIndex, BasicSensorPacket> lastPacketsOfType;

	/// <summary>
	/// Store the latency for each type of sensor packet so we know how frequently we are getting measurements.
	/// Consult this rather than the times for lastPacketsOfType since this structure is immune to framerate fluctuations.
	/// </summary>
	private Dictionary<SensorPacketDictionaryIndex, long> latenciesByType;

	// Serial Communication
	SerialPort inputPort;
	char[] fieldDelimiters = { ',' };
	string packetDelimiter = "\n";
	bool isAlive = true;
	Thread serialThread;

	/// <summary>
	/// Default constructor, need to set serialPortID and baudRate, then call OpenSerialPort() to make it do things
	/// </summary>
	public SensorReader()
	{
		lastPacketsOfType = new Dictionary<SensorPacketDictionaryIndex, BasicSensorPacket>();
		latenciesByType = new Dictionary<SensorPacketDictionaryIndex, long>();
	}


	/// <summary>
	/// Convenience constructor
	/// </summary>
	/// <param name="serialPortID"></param>
	/// <param name="baudRate"></param>
	/// <exception cref="IOException">If the port failed to open</exception>
	public SensorReader(string serialPortID, int baudRate)
		: this()
	{
		this.serialPortID = serialPortID;
		this.baudRate = baudRate;
	}

	/// <summary>
	/// Open a new serial port, after first closing any open serial ports.
	/// </summary>
	/// <exception cref="IOException">If the port failed to open</exception>
	public void OpenSerialPort()
	{
		// housekeeping
		if (null != inputPort && inputPort.IsOpen)
		{
			inputPort.Close();
		}

		if (null != serialThread && serialThread.IsAlive)
		{
			serialThread.Abort();
		}

		// Let's go
		isAlive = true;

		inputPort = new SerialPort(this.serialPortID); // replace with code to allow user to initialize, from a configuration	
		inputPort.BaudRate = this.baudRate;
		inputPort.Parity = Parity.None;
		inputPort.StopBits = StopBits.One;
		inputPort.DataBits = 8;
		inputPort.Handshake = Handshake.None;
		inputPort.ReadTimeout = 5000; // 5 second timeout

		//inputPort.DataReceived += DataReceivedHandler; // DOESN'T WORK IN MONO
		//inputPort.DataReceived +=inputPort_DataReceived; // DOESN'T WORK IN MONO

		inputPort.NewLine = packetDelimiter;
		//inputPort.Encoding = System.Text.Encoding.UTF8;

		inputPort.Open();

		// listen for data in a separate thread so we don't block the run loop
		serialThread = new Thread(new ThreadStart(ReadSerialBinary));
		serialThread.Start();
	}

	/// <summary>
	/// close the serial port down. this MUST be called at some point or the Serial Port thread will continue to run until the device is unplugged.
	/// </summary>
	public void Close()
	{
		if (null != inputPort) { inputPort.Close(); }

		isAlive = false;
	}

	/// <summary>
	/// Stop the serial thread when this object gets garbage collected.
	/// </summary>
	public void OnDestroy()
	{
		Close();
	}

	/// <summary>
	/// poll the serial port.
	/// </summary>
	public void ReadSerialString()
	{
		inputPort.ReadBufferSize = 1024;

		while (isAlive)
		{
			string inputData = inputPort.ReadLine();

			Debug.Log("READ: " + inputData);

			BasicSensorPacket packet = BasicSensorPacket.ParseData(fieldDelimiters, inputData);
			SensorPacketDictionaryIndex index = new SensorPacketDictionaryIndex();
			if (packet != null)
			{
				index.sensorID = packet.sensorID;
				index.sensorVariant = packet.variant;
				lastPacketsOfType[index] = packet;
			}
		}
	}

	/// <summary>
	/// poll the serial port	
	/// </summary>
	public void ReadSerialBinary()
	{
		bool isInSync = false;
		isConnected = true;

		while (isAlive)
		{
			if (!isInSync)
			{
				try
				{
					Debug.LogError("Out of sync. resyncing...");
					findStartOfBinaryPacket(inputPort);
					isInSync = true;
					Debug.LogError("Back in sync.");
				}
				catch (TimeoutException ex)
				{
					Debug.LogWarning("Serial connection timed out. Terminating read thread:\n" + ex);
					isInSync = false;
					isAlive = false;
					isConnected = false;
				}
				catch (IOException ex)
				{
					Debug.LogWarning("IO Exception. Terminating read thread:\n" + ex);
					isInSync = false;
					isAlive = false;
					isConnected = false;
				}
				catch (Exception ex)
				{
					Debug.LogError("Serial Exception. Resyncing...\n" + ex);
					isInSync = false;
					continue;
				}
			}

			byte[] inputData;
			try
			{
				inputData = SerialReadBinaryPacket(inputPort);
				BasicSensorPacket packet = BasicSensorPacket.ParseData(inputData);
				SensorPacketDictionaryIndex index = new SensorPacketDictionaryIndex();
				if (null != packet)
				{
					index.sensorID = packet.sensorID;
					index.sensorVariant = packet.variant;
					long lastTime = -1;

					BasicSensorPacket previousPacket;
					if (lastPacketsOfType.TryGetValue(index, out previousPacket))
					{
						lastTime = previousPacket.timestamp;
					}
					lastPacketsOfType[index] = packet;


					if (lastTime > 0)
					{
						latenciesByType[index] = packet.timestamp - lastTime;
					}
				}
			}
			catch (TimeoutException)
			{
				Debug.LogWarning("Serial connection timed out. Terminating read thread.");
				isInSync = false;
				isAlive = false;
				isConnected = false;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("error reading byte stream... trying to re-sync." + ex);
				isInSync = false;
			}
		}
	}

	/// <summary>
	/// Retrieve the latency for a particular packet type, i.e., the time interval between receiving two packets of a specific type. 
	/// returns -1 if we have not yet received two packets of the specified type.
	/// </summary>
	/// <param name="sensorId"></param>
	/// <param name="variant"></param>
	/// <returns></returns>
	public long GetLastLatency(int sensorId, PacketVariant variant)
	{
		long latency = -1;

		SensorPacketDictionaryIndex mIndex = new SensorPacketDictionaryIndex();
		mIndex.sensorID = sensorId;
		mIndex.sensorVariant = variant;

		latenciesByType.TryGetValue(mIndex, out latency);

		return latency;
	}

	/// <summary>
	/// Retreive the last received packet of a particuluar type from a particular sensor, or null if no packet of that type has been received
	/// </summary>
	/// <param name="sensorID"></param>
	/// <param name="variant"></param>
	/// <returns></returns>
	public BasicSensorPacket GetLastPacket(int sensorID, PacketVariant variant)
	{
		BasicSensorPacket p;

		SensorPacketDictionaryIndex index = new SensorPacketDictionaryIndex();
		index.sensorID = sensorID;
		index.sensorVariant = variant;

		if (!lastPacketsOfType.TryGetValue(index, out p))
		{
			p = null;
		}

		return p;
	}


	/// <summary>
	/// Read in a single packet from the serial stream, where the packet has a header as described in BasicSensorPacket
	/// </summary>
	/// <param name="inputPort"></param>
	/// <returns>a single packet's worth of data from the serial stream, or an empty byte array if a serial port exception occurred</returns>
	public static byte[] SerialReadBinaryPacket(SerialPort inputPort)
	{
		int dataSize = -69;

		byte[] receivedPacket = new byte[0];
		// block until there's something to read

		//while (inputPort.BytesToRead == 0) ; // BytesToRead throws nullpointer exception in Mono

		byte[] header = new byte[BasicSensorPacket.HEADER_SIZE];

		// this structure ensures that we get as many bytes as we need.
		int receivedBytes = 0;
		while (receivedBytes < BasicSensorPacket.HEADER_SIZE)
		{
			receivedBytes += inputPort.Read(header, receivedBytes, BasicSensorPacket.HEADER_SIZE - receivedBytes);
		}

		dataSize = header[7]; // index of payload size

		// here's that same structure again :) maybe it should be a function but I'm too lazy
		byte[] data = new byte[dataSize];

		receivedBytes = 0;
		while (receivedBytes < dataSize)
		{
			receivedBytes += inputPort.Read(data, receivedBytes, dataSize - receivedBytes);
		}



		receivedPacket = ByteArrayHelpers.Concat(header, data);

		return receivedPacket;
	}

	/// <summary>
	/// scan the serial input stream so that a subsequent read will be at the beginning of a packet
	/// </summary>
	/// <param name="inputPort"></param>
	private static void findStartOfBinaryPacket(SerialPort inputPort)
	{

		while (!findStartOfBinaryPacketHelper(inputPort)) ;

		Debug.Log("Synchronized.");
	}

	private static bool findStartOfBinaryPacketHelper(SerialPort inputPort)
	{
		int theByte = inputPort.ReadByte();
		while (!Enum.IsDefined(typeof(PacketVariant), theByte))
		{
			theByte = inputPort.ReadByte();
		}

		// part of a header, read the rest of the header
		int bytesToRead = BasicSensorPacket.HEADER_SIZE - 2; // already read the first 2 bytes of the header
		byte[] headerRest = new byte[bytesToRead];

		// there's that lovely structure again :)
		int receivedBytes = 0;
		while (receivedBytes < bytesToRead)
		{
			receivedBytes += inputPort.Read(headerRest, receivedBytes, bytesToRead - receivedBytes);
		}


		bytesToRead = headerRest[5]; // how much data?

		byte[] dummyData = new byte[bytesToRead];
		receivedBytes = 0;
		while (receivedBytes < bytesToRead)
		{
			receivedBytes += inputPort.Read(dummyData, receivedBytes, bytesToRead - receivedBytes);
		}

		byte[] inputData = SerialReadBinaryPacket(inputPort);
		BasicSensorPacket packet = BasicSensorPacket.ParseData(inputData);

		return Enum.IsDefined(typeof(PacketVariant), packet.variant);
	}
}
// fin