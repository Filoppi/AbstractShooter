using System;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace AbstractShooter
{
    public struct SaveStructure
    {
        public int HiScore;
        public WindowState windowState;
        public bool isVSync;
        public bool isMute;
    }

    static class SaveManager
    {
        public static void Save()
        {
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                BinaryWriter w = new BinaryWriter(stream);
                w.Write(GameInstance.HiScore);
                w.Write((int)Game1.windowState);
                w.Write(Game1.isVSync);
                w.Write(SoundsManager.Mute);
                w.Close();
                //stream.Close();
            }
            catch (Exception ex)
            {
                ExtendedException.ThrowException(ex, "Writing save failed");
            }
        }

        public static void Load()
        {
            if (!File.Exists("Save.bin"))
            {
                Save();
                return;
            }

            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader r = new BinaryReader(stream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);
                SaveStructure saveStructure = new SaveStructure();
                saveStructure.HiScore = r.ReadInt32();
                saveStructure.windowState = (WindowState)r.ReadInt32();
                saveStructure.isVSync = r.ReadBoolean();
                saveStructure.isMute = r.ReadBoolean();
                r.Close();
                //stream.Close();

                //if succeeded:
                GameInstance.HiScore = saveStructure.HiScore;
                Game1.windowState = saveStructure.windowState;
                Game1.isVSync = saveStructure.isVSync;
                SoundsManager.Mute = saveStructure.isMute;
            }
            catch (Exception ex)
            {
                ExtendedException.ThrowMessage(ex, "Loading save failed");
            }
        }

        public static void ResetSave()
        {
            GameInstance.HiScore = 0;
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                BinaryWriter w = new BinaryWriter(stream);
                w.Write(0);
                w.Write(0);
                w.Write(false);
                w.Write(false);
                w.Close();
                //stream.Close();
            }
            catch (Exception ex)
            {
                ExtendedException.ThrowException(ex, "Writing save failed");
            }
        }
    }
}
