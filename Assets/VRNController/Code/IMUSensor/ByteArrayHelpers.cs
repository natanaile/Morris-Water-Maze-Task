using System;
using System.Text;
/// <summary>
/// This class provides utility methods for dealing with byte arrays.
/// </summary>
public class ByteArrayHelpers
{
	/// <summary>
	/// Concatenate two byte arrays together, e.g. a, b becomes [a, b]
	/// </summary>
	/// <param name="a">first array (this will appear at the beginning)</param>
	/// <param name="b">second array (this will appear at the end)</param>
	/// <returns></returns>
	public static byte[] Concat(byte[] a, byte[] b)
	{
		byte[] c = new byte[a.Length + b.Length];
		a.CopyTo(c, 0);
		b.CopyTo(c, a.Length);

		return c;
	}


	/// <summary>
	/// Parse an array of bytesBigEndian into an integer.
	/// </summary>
	/// <param name="bytes">component bytesBigEndian of the desired integer</param>
	/// <param name="isBigEndian"></param>
	/// <returns>the integer representation of the transmitted value.</returns>
	public static uint GetUnsignedIntFromBytes(byte[] bytes, bool isBigEndian)
	{
		uint result = 0;

		if (bytes.Length <= 4)
		{

			for (int index = 0; index < bytes.Length; index++)
			{
				int indexMod = index;
				if (isBigEndian)
				{
					indexMod = bytes.Length - index - 1;
				}

				uint val = (uint)bytes[index] & 0xff;
				uint inc = (val) << (8 * indexMod);
				result |= inc;
			}
		}
		else
		{
			throw new ArgumentException("argument 'bytes' must be of length 4 or less");
		}
		return result;
	}

	/// <summary>
	/// parse an array of bytes into an integer.
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="isBigEndian"></param>
	/// <returns>the integer representation of the transmitted value.</returns>
	public static int GetSignedIntFromBytes(byte[] bytes, bool isBigEndian)
	{
		int result = 0;

		if (bytes.Length <= 4)
		{

			result = (int)GetUnsignedIntFromBytes(bytes, isBigEndian);

			if (bytes.Length < 4)
			{
				if (!isBigEndian && bytes[bytes.Length - 1] > 127 // fix little-endian sign
						|| isBigEndian && bytes[0] > 127)         // fix big-endian sign
				{
					result = (result - 1) - ((1 << (bytes.Length * 8)) - 1);
				}
			}

		}
		else
		{
			throw new ArgumentException("argument 'bytes' must be of Length 4 or less");
		}
		return result;
	}

	/// <summary>
	/// parse an int into its four constituent bytes
	/// </summary>
	/// <param name="x"></param>
	/// <param name="isBigEndian"></param>
	/// <returns>a byte array containing the 4-bytes represented by x.</returns>
	public static byte[] GetBytesFromInt(int x, bool isBigEndian)
	{
		byte[] bytes = new byte[4];

		for (int index = 0; index < bytes.Length; index++)
		{
			int indexMod = index;
			if (isBigEndian)
			{
				indexMod = bytes.Length - index - 1;
			}

			bytes[index] = (byte)((x >> indexMod * 8) & 0xff);
		}

		return bytes;
	}

	/// <summary>
	/// convert a signed byte to an unsigned integer
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public static uint ToUnsignedInt(byte x)
	{
		return x;
	}

	/// <summary>
	/// Convert an incoming data field to a float.
	/// </summary>
	/// <param name="rawData"></param>
	/// <returns></returns>
	public static float RawDataToFloat(string rawData)
	{
		int hexData = (int)Convert.ToInt32(rawData, 16);
		byte[] bytesBigEndianIN = BitConverter.GetBytes(hexData);
		byte[] bytes = bytesBigEndianIN;

		if (BitConverter.IsLittleEndian)
		{
			// flip, since incoming data is BigEndian and our system is little-endian
			byte[] bytesLittleEndianOut = new byte[4];
			bytesLittleEndianOut[0] = bytesBigEndianIN[3];
			bytesLittleEndianOut[1] = bytesBigEndianIN[2];
			bytesLittleEndianOut[2] = bytesBigEndianIN[1];
			bytesLittleEndianOut[3] = bytesBigEndianIN[0];

			bytes = bytesLittleEndianOut;
		}
		float floatData = BitConverter.ToSingle(bytes, 0);

		return floatData;
	}

	/// <summary>
	/// convert a 4-byte representation of a float to a <c>float</c> primitive (UNTESTED)
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="isBigEndian"></param>
	/// <returns></returns>
	public static float BytesToFloat(byte[] bytes, bool isBigEndian)
	{
		if (bytes.Length != 4)
		{
			throw new ArgumentException("bytes.Length must be 4, not " + bytes.Length);
		}

		float dataFloat = BitConverter.ToSingle(bytes, 0);

		return dataFloat;
	}

	/// <summary>
	/// How should the byte array be displayed?
	/// </summary>
	public enum ByteArrayMode
	{
		/// <summary>
		/// Represent the bytes as hexadecimal numbers
		/// </summary>
		BYTE_ARRAY_MODE_HEX,

		/// <summary>
		/// represent the bytes as decimal numbers
		/// </summary>
		BYTE_ARRAY_MODE_DEC
	}

	/// <summary>
	/// print out an array of bytes
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="mode"></param>
	/// <returns></returns>
	public static String GetByteArrayString(byte[] bytes, ByteArrayMode mode)
	{
		StringBuilder sBuilder = new StringBuilder();

		foreach (byte b in bytes)
		{
			switch (mode)
			{
				case ByteArrayMode.BYTE_ARRAY_MODE_HEX:
					sBuilder.Append(String.Format("0x{0:X} ", b));
					break;

				case ByteArrayMode.BYTE_ARRAY_MODE_DEC:
					sBuilder.Append(String.Format("{0:D} ", b));
					break;

				default:
					break;
			}
		}
		return sBuilder.ToString();
	}

	static int Main(string[] args)
	{
		System.Console.WriteLine("ByteArrayHelpers main");
		try
		{
			byte[] bytesBigEndian = new byte[]
            {
                (byte) 0xde,
                (byte) 0xad,
                (byte) 0xbe,
                (byte) 0xef,
            };

			byte[] bytesLittleEndian = new byte[]
            {
                (byte) 0xef,
                (byte) 0xbe,
                (byte) 0xad,
                (byte) 0xde,
            };

			uint bigEndian = GetUnsignedIntFromBytes(bytesBigEndian, true);
			uint littleEndian = GetUnsignedIntFromBytes(bytesLittleEndian, false);

			System.Console.WriteLine("big_endian: {0:D} ({0:X})\n", bigEndian);
			System.Console.WriteLine("little_endian: {0:D} ({0:X})\n", littleEndian);
			int positive = 12;
			int negative = -12;

			byte[] positiveBytes = GetBytesFromInt(positive, false);
			byte[] negativeBytes = GetBytesFromInt(negative, false);
			byte[] negativeByte = new byte[] {negativeBytes[0], negativeBytes[1] };
			int signedPos = GetSignedIntFromBytes(positiveBytes, false);
			int signedNeg = GetSignedIntFromBytes(negativeByte, false);
			uint unsignedPos = GetUnsignedIntFromBytes(positiveBytes, false);
			uint unsignedNeg = GetUnsignedIntFromBytes(negativeByte, false);


			System.Console.WriteLine("Signed: {0:D} ({0:X} -> {1} -> {2:D} ({2:X})\n",
					positive,
					GetByteArrayString(positiveBytes, ByteArrayMode.BYTE_ARRAY_MODE_HEX),
					signedPos);

			System.Console.WriteLine("Unsigned: {0:D} ({0:X} -> {1} -> {2:D} ({2:X})\n",
					positive,
					GetByteArrayString(positiveBytes, ByteArrayMode.BYTE_ARRAY_MODE_HEX),
					unsignedPos);

			System.Console.WriteLine("Signed: {0:D} ({0:X} -> {1} -> {2:D} ({2:X})\n",
					negative,
					GetByteArrayString(negativeByte, ByteArrayMode.BYTE_ARRAY_MODE_HEX),
					signedNeg);

			System.Console.WriteLine("Unsigned: {0:D} ({0:X} -> {1} -> {2:D} ({2:X})\n",
					negative,
					GetByteArrayString(negativeByte, ByteArrayMode.BYTE_ARRAY_MODE_HEX),
					unsignedNeg);

			System.Console.WriteLine("Byte");

		}
		catch (Exception ex)
		{
			System.Console.WriteLine(ex);
		}
		return 0;
	}
}