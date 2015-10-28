using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public abstract class AbstractVRNSettings
{
	/// <summary>
	/// Last time this file wrote itself out to disk
	/// </summary>
	private long lastEdited;

	/// <summary>
	/// Use this in lieu of a constructor, this ensures that whatever VRNSettings object you have is based off whatever is stored in the filesystem.
	/// </summary>
	/// <param name="path">The path to the folder that contains the settings. (Generally pass Application.persistentDataPath)</param>
	/// <returns>Return an instance of VRNSettings. If a suitable file exists, the paramters will be loaded from it. otherwise, default settings will be used and a new file will be created.</returns>
	protected static AbstractVRNSettings Load(string path, Type type, AbstractVRNSettings theInstance)
	{
		//lock (syncLock)
		//{
		if (theInstance != null)
		{
			// check to see if the current instance is out of sync with the file system (choose the most recent of the two)
			DateTime lastWriteDateTime = File.GetLastWriteTimeUtc(path);
			long lastEditTime = lastWriteDateTime.Ticks;

			if (theInstance.lastEdited < lastEditTime) // was the file edited externally since this class last edited it?
			{
				theInstance = null; // need to re-load data
			}
		}

		if (theInstance == null) // not yet initialized
		{
			XmlSerializer mSerializer = new XmlSerializer(type);

			//Debug.Log("Open new file from thread ID " + System.Threading.Thread.CurrentThread.ManagedThreadId);

			FileStream mFileStream = new FileStream(path, FileMode.OpenOrCreate); // it may not exist, then rebuild with hard-coded defaults
			


			try
			{
				theInstance = mSerializer.Deserialize(mFileStream) as AbstractVRNSettings;
				theInstance.lastEdited = File.GetLastWriteTimeUtc(path).Ticks;
			}
			catch (XmlException)
			{
				Debug.Log("Settings file at " + path + " not found... rebuilding.");
				XmlWriterSettings mWriterSettings = new XmlWriterSettings();
				mWriterSettings.Encoding = Encoding.UTF8;
				mWriterSettings.Indent = true;
				XmlWriter mWriter = XmlWriter.Create(mFileStream, mWriterSettings);

				theInstance = (AbstractVRNSettings)Activator.CreateInstance(type);
				theInstance.lastEdited = DateTime.Now.Ticks;
				mSerializer.Serialize(mWriter, theInstance);
			}
			finally
			{
				Debug.Log("closing file at " + path);
				mFileStream.Close();
			}


			//}
		}

		return theInstance;
	}

	/// <summary>
	/// Store the current settings to disk, at the default path. overwrites whatever is currently on disk. call Save(string, string)
	/// </summary>
	public abstract void Save();

	/// <summary>
	/// called when it gets destroyed
	/// </summary>
	public void OnDestroy()
	{
		Save();
	}

	/// <summary>
	/// Store the current settings to disk. overwrites whatever is currently on disk.
	/// </summary>
	/// <param name="path">The path to the folder that contains the settings. (Generally pass Application.persistentDataPath)</param>
	public void Save(string path, string fileName, Type type)
	{
		Debug.Log("Saving VRNSettings file to path " + path);
		XmlSerializer mSerializer = new XmlSerializer(type);
		FileStream mFileStream = new FileStream(path + "/" + fileName, FileMode.Create); // blow away whatever is there.
		
		XmlWriterSettings mWriterSettings = new XmlWriterSettings();
		mWriterSettings.Encoding = Encoding.UTF8;
		mWriterSettings.Indent = true;
		XmlWriter mWriter = XmlWriter.Create(mFileStream,mWriterSettings);

		mSerializer.Serialize(mWriter, this);
		mFileStream.Close();
	}
}
