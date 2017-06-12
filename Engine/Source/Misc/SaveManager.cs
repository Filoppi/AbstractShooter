using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UnrealMono
{
	[System.Serializable]
	public struct EngineSettingsSave
	{
		public WindowState windowState;
		public bool isVSync;
		public bool isMute;
	}

	[System.Serializable]
	public class SerializationTest
	{
		public float testFloat = 55;
		public float testFloataaa = 55;
	}

    public static class SaveManager
	{
		public static void SaveSerializableClass(Object objectToSave)
		{
			//Opens a file and serializes the object into it in binary format.
			Stream stream = File.Open(objectToSave.GetType().Name + ".xml", FileMode.Create);
			//SoapFormatter formatter = new SoapFormatter();
			BinaryFormatter formatter = new BinaryFormatter();

			formatter.Serialize(stream, objectToSave);
			stream.Close();
		}

		public static void ReadSerializableClass(ref Object objectToSave)
		{
			//Opens file "data.xml" and deserializes the object from it.
			Stream stream = File.Open(objectToSave.GetType().Name + ".xml", FileMode.Open);
			BinaryFormatter formatter = new BinaryFormatter();

			objectToSave = (SerializationTest)formatter.Deserialize(stream);
			stream.Close();
		}
		public static Object ReadSerializableClass(Object objectToSave)
		{
			ReadSerializableClass(ref objectToSave);
			return objectToSave;

			/*//Opens file "data.xml" and deserializes the object from it.
			Stream stream = File.Open(objectToSave.GetType().Name + ".xml", FileMode.Open);
			BinaryFormatter formatter = new BinaryFormatter();

			objectToSave = (SerializationTest)formatter.Deserialize(stream);
			stream.Close();
			return objectToSave;*/
		}

		public static void SaveEngineSettings()
		{
			EngineSettingsSave engineSettingsSave;
			engineSettingsSave.windowState = UnrealMonoGame.windowState;
			engineSettingsSave.isVSync = UnrealMonoGame.isVSync;
			engineSettingsSave.isMute = SoundsManager.Mute;
			Save(engineSettingsSave);
		}

		public static void LoadAndSetEngineSettings()
		{
			EngineSettingsSave engineSettingsSave = new EngineSettingsSave();
			Object engineSettingsSaveObj = engineSettingsSave;
			Load(ref engineSettingsSaveObj);
			engineSettingsSave = (EngineSettingsSave)engineSettingsSaveObj;
			
			UnrealMonoGame.windowState = engineSettingsSave.windowState;
			UnrealMonoGame.isVSync = engineSettingsSave.isVSync;
			SoundsManager.Mute = engineSettingsSave.isMute;
		}
		
		public static void Save(Object objectToSave)
		{
			try
			{
				string saveName = objectToSave.GetType().Name;
				FileStream stream = new FileStream(saveName + ".bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
				BinaryWriter w = new BinaryWriter(stream);
				foreach (System.Reflection.FieldInfo fieldInfo in objectToSave.GetType().GetFields())
				{
					byte[] propertyAsByteArray = ObjectToByteArray(fieldInfo.GetValue(objectToSave));
					w.Write(fieldInfo.Name); //Writes name of the property
					w.Write(propertyAsByteArray.Length); //Writes length of the property
					w.Write(propertyAsByteArray); //Writes value of the property as a byte array
				}
				////w.Write(GameInstance.HiScore);
				//w.Write((int)UnrealMonoGame.windowState);
				//w.Write(UnrealMonoGame.isVSync);
				//w.Write(SoundsManager.Mute);
				w.Close();
				//stream.Close();
			}
			catch (Exception ex)
			{
				ExtendedException.ThrowException(ex, "Writing " + objectToSave + " failed");
			}
		}

		//Converts an object to a byte array
		private static byte[] ObjectToByteArray(Object obj)
		{
			if (obj == null)
			{
				return null;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		//Converts a byte array to an Object
		private static Object ByteArrayToObject(byte[] bytesArray)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			memoryStream.Write(bytesArray, 0, bytesArray.Length);
			memoryStream.Seek(0, SeekOrigin.Begin);
			Object obj = binaryFormatter.Deserialize(memoryStream);
			return obj;
		}

		public static void Load(ref Object objectToSave)
		{
			if (!File.Exists(objectToSave.GetType().Name + ".bin"))
			{
				Save(objectToSave);
				return;
			}

			try
			{
				string saveName = objectToSave.GetType().Name;
				FileStream stream = new FileStream(saveName + ".bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				BinaryReader r = new BinaryReader(stream);
				r.BaseStream.Seek(0, SeekOrigin.Begin);
				foreach (System.Reflection.FieldInfo fieldInfo in objectToSave.GetType().GetFields())
				{
					if (r.ReadString() == fieldInfo.Name)
					{
						Object uncastedValue = ByteArrayToObject(r.ReadBytes(r.ReadInt32()));
						//property.SetValue(engineSettingsSaveStructure, Convert.ChangeType(uncastedValue, property.GetType()));
						fieldInfo.SetValue(objectToSave, uncastedValue);
					}
					else break;
				}
				r.Close();
				//stream.Close();
			}
			catch (Exception ex)
			{
				ExtendedException.ThrowMessage(ex, "Loading " + objectToSave + " failed");
			}
		}
		public static Object Load(Object objectToSave)
		{
			Load(ref objectToSave);
			return objectToSave;
		}

		public static void ResetSave(Type saveTypeToReset)
		{
			try
			{
				Object emptySave = System.Activator.CreateInstance(saveTypeToReset);
				Save(emptySave);
			}
			catch (Exception ex)
			{
				ExtendedException.ThrowException(ex, "Resetting " + saveTypeToReset.ToString() + " failed");
			}
		}
	}
}
 