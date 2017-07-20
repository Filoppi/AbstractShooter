using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

using UnrealMono;
using AbstractShooter;
using System.IO;

namespace AssetEditor
{
    //TO: TextLabel(name) TextLabel(type) input box(string, number or enum or asset(reference)) and occasionally Browse button
    public partial class AssetEditor : Form
    {
        private enum AssetType
        {
            None,
            Actor,
            Component
            //Particles,
            //...
        }
        //private List<string> AssetTypeFormat = new List<string>(Enum.GetValues(typeof(AssetType)).Cast<AssetType>().Count()) { };
        private Dictionary<AssetType, string> assetTypesFormat = new Dictionary<AssetType, string>();
        private Dictionary<AssetType, Type> assetTypesType = new Dictionary<AssetType, Type>();
        private DictionaryPropertyGridAdapter classPropertyGridAdapter;

        private AssetType currentAssetType = AssetType.None;
        private string currentFileDirectory = "";
        private Type currentBaseClass;
        public Type CurrentBaseClass
        {
            get { return currentBaseClass; }
            set
            {
                currentBaseClass = value;
                saveToolStripMenuItem.Enabled = currentBaseClass != null;
                saveAsNameToolStripMenuItem.Enabled = currentBaseClass != null;

                classPropertyGridAdapter = currentBaseClass != null ? new DictionaryPropertyGridAdapter(currentBaseClass) : null;
                propertyGrid1.SelectedObject = classPropertyGridAdapter;

                //propertyGrid1.SelectedObject = new AActor();
            }
        }

        public List<Type> classes;

        public AssetEditor()
        {
            TypeDescriptor.AddAttributes(typeof(Microsoft.Xna.Framework.Graphics.Texture2D), new EditorAttribute(typeof(FileSelectorEditor), typeof(UITypeEditor)));
            
            TypeDescriptor.AddAttributes(typeof(Microsoft.Xna.Framework.Vector2), new TypeConverterAttribute(typeof(VectorTypeConverter<Microsoft.Xna.Framework.Vector2>)));
            TypeDescriptor.AddAttributes(typeof(Microsoft.Xna.Framework.Vector3), new TypeConverterAttribute(typeof(VectorTypeConverter<Microsoft.Xna.Framework.Vector3>)));
            TypeDescriptor.AddAttributes(typeof(List<Microsoft.Xna.Framework.Vector3>), new TypeConverterAttribute(typeof(ListConverter<Microsoft.Xna.Framework.Vector3>)));
            
            InitializeComponent();
            
            assetTypesFormat[AssetType.Actor] = "act";
            assetTypesFormat[AssetType.Component] = "cmp";
            assetTypesType[AssetType.Actor] = typeof(AActor);
            assetTypesType[AssetType.Component] = typeof(CComponent);

            classes = GetClasses("AbstractShooter", "AbstractShooter", typeof(UnrealMono.AActor));

            CurrentBaseClass = null;

            //propertyGrid1.DrawFlat = true;
            // add the RawPage now and for ever (static)
            propertyGrid1.PropertyTabs.AddTabType(typeof(TypeCategoryTab), PropertyTabScope.Static);
            propertyGrid1.PropertyTabs.AddTabType(typeof(RawMemberTab), PropertyTabScope.Static);
            //propertyGrid1.SetServiceProvider(serviceProvider);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void actorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Update classes
            classes = GetClasses("Engine", "Engine", typeof(UnrealMono.AActor));
            classes.AddRange(GetClasses("AbstractShooter", "AbstractShooter", typeof(UnrealMono.AActor)));

            NewActor();
        }

        private void componentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Update classes
            classes = GetClasses("Engine", "Engine", typeof(UnrealMono.CComponent));
            classes.AddRange(GetClasses("AbstractShooter", "AbstractShooter", typeof(UnrealMono.CComponent)));

            NewComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFileDirectory == "")
            {
                SaveAs();
            }
            else
            {
                Save();
            }
        }

        private void saveAsNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }


        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://filippotarpini.wixsite.com/portfolio");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Filippo Tarpini. 2017", "About");
        }

        private void NewActor()
        {
            Type lastCurrentBaseClass = CurrentBaseClass;
            
            AssetClassSelector classSelector = new AssetClassSelector(classes);
            classSelector.classSelected += SetCurrentBaseClass;
            classSelector.ShowDialog();

            if (CurrentBaseClass != lastCurrentBaseClass)
            {
                currentAssetType = AssetType.Actor;
                currentFileDirectory = "";

                UpdateFormTitle();
            }
        }

        private void NewComponent()
        {
            Type lastCurrentBaseClass = CurrentBaseClass;
            
            AssetClassSelector classSelector = new AssetClassSelector(classes);
            classSelector.classSelected += SetCurrentBaseClass;
            classSelector.ShowDialog();

            if (CurrentBaseClass != lastCurrentBaseClass)
            {
                currentAssetType = AssetType.Component;
                currentFileDirectory = "";

                UpdateFormTitle();
            }
        }

        private void SetCurrentBaseClass(Type type)
        {
            CurrentBaseClass = type;
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "Content";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "All supported files (*.act, *.cmp)|*.act;*.cmp|Actor (*.act)|*.act|Component (*.cmp)|*.cmp"; //|All files (*.*)|*.*
            //openFileDialog.FilterIndex = 0;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stream fileStream = null;
                    if ((fileStream = openFileDialog.OpenFile()) != null)
                    {
                        using (fileStream)
                        {
                            // Insert code to read the stream here.
                        }
                    }

                    string fileName = openFileDialog.FileName;
                    string text = File.ReadAllText(fileName);

                    currentFileDirectory = fileName;
                    //currentAssetType = (AssetType)openFileDialog.FilterIndex;
                    foreach (KeyValuePair<AssetType, string> assetTypeFormat in assetTypesFormat)
                    {
                        if (currentFileDirectory.EndsWith(assetTypeFormat.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            currentAssetType = assetTypeFormat.Key;
                            CurrentBaseClass = assetTypesType[assetTypeFormat.Key];
                            break;
                        }
                    }
                }
                catch (Exception ex) //or IOException?
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            UpdateFormTitle();
        }

        private void Save()
        {
            //Stream fileStream;
            //if ((fileStream = saveFileDialog.OpenFile()) != null)
            //{
            //    // Code to write the stream goes here.
            //    fileStream.Close();
            //}

            switch (currentAssetType)
            {
                case AssetType.Actor:
                    {
                        using (StreamWriter sw = new StreamWriter(currentFileDirectory))
                            sw.WriteLine("Actor");
                        break;
                    }
                case AssetType.Component:
                    {
                        using (StreamWriter sw = new StreamWriter(currentFileDirectory))
                            sw.WriteLine("Component");
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            UpdateFormTitle();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            
            saveFileDialog.FileName = "Unknown";
            saveFileDialog.InitialDirectory = "Content";
            saveFileDialog.RestoreDirectory = true;
            switch (currentAssetType)
            {
                case AssetType.Actor:
                    {
                        saveFileDialog.Filter = "Actor (*.act)|*.act";
                        //saveFileDialog.FilterIndex = (int)currentAssetType;
                        break;
                    }
                case AssetType.Component:
                    {
                        saveFileDialog.Filter = "Component (*.cmp)|*.cmp";
                        //saveFileDialog.FilterIndex = (int)currentAssetType;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFileDirectory = saveFileDialog.FileName;
                Save();
            }
        }
        
        void UpdateFormTitle()
        {
            Text = "Assets Editor: " + currentAssetType.ToString() + " " + CurrentBaseClass + " " + currentFileDirectory.Substring(currentFileDirectory.LastIndexOf('\\') + 1);
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
                 (p.IsSubclassOf(typeFilter) || p == typeFilter) &&
                 //p.BaseType == typeFilter &&
                 p.Name.Contains(nameFilter)
            ).ToList();
            
            //objects.Add((T)Activator.CreateInstance(type, constructorArgs));

            return classes;
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
