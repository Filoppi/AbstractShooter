using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AbstractShooter.States;

namespace AbstractShooter
{
    public struct SaveStructure
    {
        public int HiScore;
        public bool isMaxResolution;
        public bool isFullScreen;
        public bool isBorderless;
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
                w.Write(Game1.isMaxResolution);
                w.Write(Game1.isFullScreen);
                w.Write(Game1.isBorderless);
                w.Write(Game1.isVSync);
                w.Write(SoundsManager.Mute);
                stream.Close();
                w.Close();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public static void Load()
        {
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader r = new BinaryReader(stream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);
                SaveStructure saveStructure = new SaveStructure();
                saveStructure.HiScore = r.ReadInt32();
                saveStructure.isMaxResolution = r.ReadBoolean();
                saveStructure.isFullScreen = r.ReadBoolean();
                saveStructure.isBorderless = r.ReadBoolean();
                saveStructure.isVSync = r.ReadBoolean();
                saveStructure.isMute = r.ReadBoolean();
                r.Close();
                stream.Close();

                //if succeeded:
                GameInstance.HiScore = saveStructure.HiScore;
                Game1.isMaxResolution = saveStructure.isMaxResolution;
                Game1.isFullScreen = saveStructure.isFullScreen;
                Game1.isBorderless = saveStructure.isBorderless;
                Game1.isVSync = saveStructure.isVSync;
                SoundsManager.Mute = saveStructure.isMute;
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public static void ClearSave()
        {
            GameInstance.HiScore = 0;
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                BinaryWriter w = new BinaryWriter(stream);
                w.Write(GameInstance.HiScore);
                w.Write(false);
                w.Write(false);
                w.Write(false);
                w.Write(false);
                stream.Close();
                w.Close();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }
    }
}
