using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using UnrealMono;
using AbstractShooter;
using System.IO;

namespace AssetEditor
{
    public partial class AssetEditor : Form
    {
        public AssetEditor()
        {
            InitializeComponent();

            List<Type> classes = GetClasses("AbstractShooter", "AbstractShooter", typeof(UnrealMono.AActor));

            //comboBox1.Items.Add(new { Text = "Text", Value = "Value" });
            classes.ForEach(type => comboBox1.Items.Add(type.Name));
        }

        List<Type> GetClasses(string assemblyName, string nameSpaceName, Type typeFilter, string nameFilter = "")
        {
            AppDomain.CurrentDomain.GetAssemblies()
                          .SelectMany(t => t.GetTypes())
                          .Where(t => t.IsClass && t.Namespace == @nameSpaceName);

            //Load or Get
            Assembly assembly = Assembly.Load(assemblyName);

            List<Type> classes = assembly.GetTypes().Where(p =>
                 //p.Namespace.Contains(nameSpaceName) &&
                 p.IsClass &&
                 !p.IsAbstract &&
                 p.IsSubclassOf(typeFilter) //&&
                 //p.BaseType == typeFilter &&
                 //p.Name.Contains(nameFilter)
            ).ToList();

            //objects.Add((T)Activator.CreateInstance(type, constructorArgs));

            return classes;
        }

        private void aboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Filippo Tarpini", "About");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        //string GetClasses(string assembly, string nameSpace, Type typeFilter, string nameFilter = "")
        //{
        //    List<string> classeslist = new List<string>();
        //    foreach (Type asd in GetClasses())
        //    {
        //        classeslist.Add(asd.ToString());
        //    }

        //    return classes;
        //}
        
        //static void Main(string[] args)
        //{
        //    Serialize();
        //    Deserialize();
        //}

        //static void Serialize()
        //{
        //    AbstractShooter.ParticleSettings obj = new AbstractShooter.ParticleSettings();

        //    //Opens a file and serializes the object into it in binary format.
        //    Stream streame = File.Open("data.xml", FileMode.Create);
        //    FileStream fs = new FileStream("DataFile.dat", FileMode.Create);
        //    //SoapFormatter formatter = new SoapFormatter();
        //    BinaryFormatter formatter = new BinaryFormatter();

        //    try
        //    {
        //        formatter.Serialize(streame, obj);
        //    }
        //    catch (SerializationException e)
        //    {
        //        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
        //        throw;
        //    }
        //    finally
        //    {
        //        streame.Close();
        //        fs.Close();
        //    }
        //}


        //static void Deserialize()
        //{

        //    //Empties obj.
        //    AbstractShooter.ParticleSettings obj = null;

        //    //Opens file "data.xml" and deserializes the object from it.
        //    Stream streame = File.Open("data.xml", FileMode.Open);
        //    FileStream fs = new FileStream("DataFile.dat", FileMode.Open);

        //    BinaryFormatter formatter = new BinaryFormatter();

        //    try
        //    {
        //        obj = (AbstractShooter.ParticleSettings)formatter.Deserialize(streame);
        //    }
        //    catch (SerializationException e)
        //    {
        //        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
        //        throw;
        //    }
        //    finally
        //    {
        //        streame.Close();
        //        fs.Close();
        //    }
        //}
    }
}
