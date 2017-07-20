using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using href.Controls.PropGridEx;
using Microsoft.Xna.Framework;
using UnrealMono;

namespace AssetEditor
{
    //ExpandableObjectConverter, Browsable, GetProperties
    public class VectorTypeConverter<T> : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is T)
            {
                if (value is Vector3)
                {
                    Vector3 sl = (Vector3)value;
                    return sl.X + ";" + sl.Y + ";" + sl.Z;
                }
                if (value is Vector2)
                {
                    Vector2 sl = (Vector2)value;
                    return sl.X + ";" + sl.Y;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                if (typeof(T)== typeof(Vector3))
                {
                    try
                    {
                        string s = (string)value;
                        string[] parameters = s.Split(';');
                        Vector3 sl = new Vector3();
                        sl.X = float.Parse(parameters[0]);
                        sl.Y = float.Parse(parameters[1]);
                        sl.Z = float.Parse(parameters[2]);
                        return sl;
                    }
                    catch
                    {
                        throw new ArgumentException("Can not convert '" + (string)value + "' to type Vector3");
                    }
                }
                if (typeof(T) == typeof(Vector2))
                {
                    try
                    {
                        string s = (string)value;
                        string[] parameters = s.Split(';');
                        Vector2 sl = new Vector2();
                        sl.X = float.Parse(parameters[0]);
                        sl.Y = float.Parse(parameters[1]);
                        return sl;
                    }
                    catch
                    {
                        throw new ArgumentException("Can not convert '" + (string)value + "' to type Vector2");
                    }
                }
                
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            Vector3 member = (Vector3)value;
            MemberDescriptor member1 = new MemberDescriptor(member, 0);
            MemberDescriptor member2 = new MemberDescriptor(member, 1);
            MemberDescriptor member3 = new MemberDescriptor(member, 2);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { member1, member2, member3 });
        }

        private class MemberDescriptor : SimplePropertyDescriptor
        {
            public MemberDescriptor(Vector3 member, int index)
                : base(member.GetType(), index.ToString(), typeof(string))
            {
                Member = member;
                this.index = index;
            }

            private int index;
            public Vector3 Member;
            //public PropertyDescriptor Member;

            public override object GetValue(object component)
            {
                if (index == 0)
                    return Member.X;
                if (index == 1)
                    return Member.Y;
                return Member.Z;
            }

            public override void SetValue(object component, object value)
            {
                //Member.SetValue(component, value);
                if (index == 0)
                    Member.X = float.Parse((string)value);
                else if (index == 1)
                    Member.Y = float.Parse((string)value);
                else
                    Member.Z = float.Parse((string)value);
            }
        }
    }

    public class ListConverter<T> : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            List<T> members = value as List<T>;
            if (members == null)
                return "-";

            return string.Join(", ", members.Select(m => m.ToString()));
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            List<T> members = value as List<T>;
            if (members != null)
            {
                foreach (T member in members)
                {
                    list.Add(new MemberDescriptor<T>(member, list.Count));
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        private class MemberDescriptor<T> : SimplePropertyDescriptor
        {
            public MemberDescriptor(T member, int index)
                : base(member.GetType(), index.ToString(), typeof(string))
            {
                Member = member;
            }

            public T Member { get; private set; }

            public override object GetValue(object component)
            {
                return Member.ToString(); //Member.name
            }

            public override void SetValue(object component, object value)
            {
                //Member = new Vector3((int)value, (int)value, (int)value);  //Member.name
            }
        }
    }

    public class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        public IDictionary dictionary;
        public Type baseType;

        public DictionaryPropertyGridAdapter(Type baseType)
        {
            this.baseType = baseType;
            dictionary = new Hashtable();

            if (this.baseType != null)
            {
                //foreach (FieldInfo fieldInfo in baseType.GetFields())
                //{
                //    UnrealMonoAttribute unrealMonoAttribute = fieldInfo.GetCustomAttribute<UnrealMonoAttribute>();
                //    if (unrealMonoAttribute != null && !unrealMonoAttribute.hidden)
                //    {
                //        //dictionary.Add(unrealMonoAttribute.customName != "" ? unrealMonoAttribute.customName : fieldInfo.Name, Activator.CreateInstance(fieldInfo.FieldType));
                //        dictionary.Add(fieldInfo, Activator.CreateInstance(fieldInfo.FieldType));
                //    }
                //}
                //foreach (PropertyInfo propertyInfo in baseType.GetProperties())
                //{
                //}
                foreach (MemberInfo memberInfo in this.baseType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)) //TO1 FIND ALL
                {
                    UnrealMonoAttribute unrealMonoAttribute = memberInfo.GetCustomAttribute<UnrealMonoAttribute>();
                    if (unrealMonoAttribute != null && !unrealMonoAttribute.hidden)
                    {
                        //dictionary.Add(unrealMonoAttribute.customName != "" ? unrealMonoAttribute.customName : fieldInfo.Name, Activator.CreateInstance(fieldInfo.FieldType));

                        Type memberType = null;
                        if (memberInfo.MemberType == MemberTypes.Field)
                        {
                            memberType = unrealMonoAttribute.serializeAs != null ? unrealMonoAttribute.serializeAs : ((FieldInfo)memberInfo).FieldType;
                        }
                        else if (memberInfo.MemberType == MemberTypes.Property)
                        {
                            memberType = unrealMonoAttribute.serializeAs != null ? unrealMonoAttribute.serializeAs : ((PropertyInfo)memberInfo).PropertyType;
                        }
                        if (memberType != null)
                        {
                            //if memberInfo.type is a list of components, let us load the names of the prefabs or of the base class
                            //if (memberType == typeof(List<CComponent>)) //typeof(IEnumerable).IsAssignableFrom(memberType)
                            //{
                            //    if (!dictionary.Contains(memberType))
                            //        dictionary.Add(memberType, new List<string>());
                            //}
                            //else if (memberType.IsSubclassOf(typeof(CSceneComponent)) || memberType == typeof(CSceneComponent))
                            //{
                            //    if (!dictionary.Contains(memberType))
                            //        dictionary.Add(memberType, "");
                            //}
                            //if memberInfo.type is a texture, let's load a directory
                            //else if ()
                            //{
                            //}
                            //else
                            //if (memberType.GetConstructors().Length > 0 && memberType.GetConstructor(Type.EmptyTypes) == null)
                            //{
                            //    if (!dictionary.Contains(memberType))
                            //    {
                            //        dictionary.Add(memberType, "");
                            //        Console.WriteLine("Warning: Unsopported member: " + memberInfo);
                            //    }
                            //}
                            //else
                            {
                                dictionary.Add(memberInfo, Activator.CreateInstance(memberType));
                            }
                        }
                    }
                }
            }
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            ArrayList properties = new ArrayList();
            foreach (DictionaryEntry e in dictionary)
            {
                MemberInfo memberInfo = (MemberInfo)e.Key;

                string name = memberInfo.Name;

                if (memberInfo != null)
                {
                    UnrealMonoAttribute unrealMonoAttribute = memberInfo.GetCustomAttribute<UnrealMonoAttribute>();
                    if (unrealMonoAttribute != null && !unrealMonoAttribute.hidden)
                    {
                        name = unrealMonoAttribute.customName != "" ? unrealMonoAttribute.customName : memberInfo.Name;

                        Type memberType = null;
                        if (memberInfo.MemberType == MemberTypes.Field)
                        {
                            memberType = ((FieldInfo)memberInfo).FieldType;
                        }
                        else if (memberInfo.MemberType == MemberTypes.Property)
                        {
                            memberType = ((PropertyInfo)memberInfo).PropertyType;
                        }
                        name += " (" + memberInfo.MemberType + " " + memberType + ")";
                    }
                }
                properties.Add(new DictionaryPropertyDescriptor(dictionary, e.Key, name));
            }

            PropertyDescriptor[] props = (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }
    }

    //TO delete
    public class TemplateCollection<T> : CollectionBase
    {
        public T this[int index]
        {
            get { return (T)List[index]; }
        }
        public void Add(T emp)
        {
            this.List.Add(emp);
        }
        public void Remove(T emp)
        {
            this.List.Remove(emp);
        }
    }

    public class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        public IDictionary dictionary;
        public object key;

        internal DictionaryPropertyDescriptor(IDictionary dictionary, object key, string name)
            : base(name, null)
        {
            this.dictionary = dictionary;
            this.key = key;

            //if (key is IEnumerable)
            //{
            //    Type mmm = key.GetType().GetNestedTypes()[0];
            //    if (mmm == typeof(CComponent))
            //    {
            //        key = new TemplateCollection<CComponent>();
            //    }
            //    else if (mmm == typeof(CSceneComponent))
            //    {
            //        key = new TemplateCollection<CSceneComponent>();
            //    }
            //}
            //this.key = key;
        }

        public override string Category
        {
            get
            {
                MemberInfo memberInfo = (MemberInfo)key;
                if (memberInfo != null)
                {
                    UnrealMonoAttribute unrealMonoAttribute = memberInfo.GetCustomAttribute<UnrealMonoAttribute>();
                    if (unrealMonoAttribute != null)
                    {
                        return unrealMonoAttribute.category;
                    }
                }
                return "";
            }
        }

        public override bool IsBrowsable { get { return true; } }

        //TO: public override string DisplayName { get; }

        public override Type PropertyType
        {
            get { return dictionary[key].GetType(); }
        }

        public override void SetValue(object component, object value)
        {
            dictionary[key] = value;
        }

        public override object GetValue(object component)
        {
            return dictionary[key];
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            //TO1 true if != from default value?

            /* It uses a poorly documented feature of PropertyGrid.
             * When PG displays a value, it decides whether to show the value in bold or regular type by first looking for a DefaultValue attribute for the property.
             * If not found, it then uses reflection to try to find a method with the name ShouldSerializeXXX where XXX must be an exact match with the property name.
             * If found, like in this example, it calls the method and displays the value in bold type when it returns true, regular type when it returns false.
             * 
             * Give the property you own attribute, derived from System.ComponentModel.DefaultValueAttribute.  I haven't tried it.
             */
            return true;
        }
    }




    class FileSelectorEditor : UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                return openFile.FileName;
            }
            return value;
        }
    }









    

    // A TypeCategoryTab property tab lists properties by the 
    // category of the type of each property.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class TypeCategoryTab : PropertyTab
    {
        [BrowsableAttribute(true)]
        // This string contains a Base-64 encoded and serialized example property tab image.
        private string img = "AAEAAAD/////AQAAAAAAAAAMAgAAAFRTeXN0ZW0uRHJhd2luZywgVmVyc2lvbj0xLjAuMzMwMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2EFAQAAABVTeXN0ZW0uRHJhd2luZy5CaXRtYXABAAAABERhdGEHAgIAAAAJAwAAAA8DAAAA9gAAAAJCTfYAAAAAAAAANgAAACgAAAAIAAAACAAAAAEAGAAAAAAAAAAAAMQOAADEDgAAAAAAAAAAAAD///////////////////////////////////9ZgABZgADzPz/zPz/zPz9AgP//////////gAD/gAD/AAD/AAD/AACKyub///////+AAACAAAAAAP8AAP8AAP9AgP////////9ZgABZgABz13hz13hz13hAgP//////////gAD/gACA/wCA/wCA/wAA//////////+AAACAAAAAAP8AAP8AAP9AgP////////////////////////////////////8L";

        public TypeCategoryTab()
        {
        }

        // Returns the properties of the specified component extended with 
        // a CategoryAttribute reflecting the name of the type of the property.
        public override System.ComponentModel.PropertyDescriptorCollection GetProperties(object component, System.Attribute[] attributes)
        {
            PropertyDescriptorCollection props;
            if (attributes == null)
                props = TypeDescriptor.GetProperties(component);
            else
                props = TypeDescriptor.GetProperties(component, attributes);

            PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];
            for (int i = 0; i < props.Count; i++)
            {
                // Create a new PropertyDescriptor from the old one, with 
                // a CategoryAttribute matching the name of the type.
                propArray[i] = TypeDescriptor.CreateProperty(props[i].ComponentType, props[i], new CategoryAttribute(props[i].PropertyType.Name));
            }
            return new PropertyDescriptorCollection(propArray);
        }

        public override System.ComponentModel.PropertyDescriptorCollection GetProperties(object component)
        {
            return this.GetProperties(component, null);
        }

        // Provides the name for the property tab.
        public override string TabName
        {
            get
            {
                return "Properties by Type";
            }
        }

        // Provides an image for the property tab.
        public override System.Drawing.Bitmap Bitmap
        {
            get
            {
                Bitmap bmp = new Bitmap(DeserializeFromBase64Text(img));
                return bmp;
            }
        }

        // This method can be used to retrieve an Image from a block of Base64-encoded text.
        private Image DeserializeFromBase64Text(string text)
        {
            Image img = null;
            byte[] memBytes = Convert.FromBase64String(text);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(memBytes);
            img = (Image)formatter.Deserialize(stream);
            stream.Close();
            return img;
        }
    }








    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class RawMemberTab : PropertyTab
    {
        [BrowsableAttribute(true)]
        // This string contains a Base-64 encoded and serialized example property tab image.
        private string img = "AAEAAAD/////AQAAAAAAAAAMAgAAAFRTeXN0ZW0uRHJhd2luZywgVmVyc2lvbj0xLjAuMzMwMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2EFAQAAABVTeXN0ZW0uRHJhd2luZy5CaXRtYXABAAAABERhdGEHAgIAAAAJAwAAAA8DAAAA9gAAAAJCTfYAAAAAAAAANgAAACgAAAAIAAAACAAAAAEAGAAAAAAAAAAAAMQOAADEDgAAAAAAAAAAAAD///////////////////////////////////9ZgABZgADzPz/zPz/zPz9AgP//////////gAD/gAD/AAD/AAD/AACKyub///////+AAACAAAAAAP8AAP8AAP9AgP////////9ZgABZgABz13hz13hz13hAgP//////////gAD/gACA/wCA/wCA/wAA//////////+AAACAAAAAAP8AAP8AAP9AgP////////////////////////////////////8L";
        
        public RawMemberTab()
        {
        }

        public RawMemberTab(IServiceProvider serviceProvider)
        {
        }

        public RawMemberTab(IDesignerHost designerHost)
        {
        }

        /// <summary>
        /// extend everything
        /// </summary>
        public override bool CanExtend(object extendee)
        {
            return true;
        }

        /// <summary>
        /// the tab's iumage
        /// </summary>
        public override System.Drawing.Bitmap Bitmap
        {
            get
            {
                Bitmap bmp = new Bitmap(DeserializeFromBase64Text(img));
                return bmp;
            }
        }

        /// <summary>
        /// the tab's name
        /// </summary>
        public override string TabName
        {
            get { return "Raw View"; }
        }

        /// <summary>
        /// used to filter implemented interfaces in a type
        /// </summary>
        /// <returns>true if the requested interfaces are implemented</returns>
        protected static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            Type t = criteriaObj as Type;
            if (t == null)
                return false;
            if (typeObj == t)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Add the instance fields of an object
        /// </summary>
        private static void AddTypeFields(object component, Type type, List<PropertyDescriptor> fields, List<string> addedMemberNames)
        {

            // stop at List / ArrayList / Dictionary / SortedList / Hashtable 
            if ((type == typeof(ArrayList)) ||
                (type == typeof(Hashtable)) ||
                (type == typeof(SortedList)))
                return;

            // get all instance FieldInfos
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo field = fieldInfos[i];

                /*
                // ignore statics
                if (field.IsStatic)
                    continue;
                 */

                // ignore EventHandlers
                if (field.FieldType.IsSubclassOf(typeof(Delegate)))
                    continue;

                // ignore doublette names
                if (addedMemberNames.Contains(field.Name))
                    continue;

                // this one made it in the list... 
                addedMemberNames.Add(field.Name);
                fields.Add(new FieldMemberDescriptor(type, field));
            }
        }

        public override System.ComponentModel.PropertyDescriptorCollection GetProperties(object component)
        {
            return this.GetProperties(component, null);
        }

        /// <summary>
        /// Gather all PropertyDescriptors for the RawMemberTab
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            if (component == null)
            {
                return new PropertyDescriptorCollection(null, true);
            }

            // use a stack to reverse hierarchy
            // if fieldnames occure in more than one class
            // use the one from the class that is highest in the class hierarchy
            Stack<Type> objectHierarchy = new Stack<Type>();
            Type curType = component.GetType();
            while (curType != null)
            {
                objectHierarchy.Push(curType);
                curType = curType.BaseType;
            }

            // list of the PropertyDescriptors that will be returned
            List<PropertyDescriptor> fields = new List<PropertyDescriptor>();
            // list to rememnber already added names
            List<string> addedMemberNames = new List<string>();

            // special treatment for classes implementing IList
            IList list = component as IList;
            if (list != null)
            {
                // add an ListItemMemberDescriptor fore each item in the list
                for (int i = 0; i < list.Count; i++)
                {
                    fields.Add(new ListItemMemberDescriptor(list, i));
                }
            }

            // special treatment for classes implementing IDictionary
            IDictionary dict = component as IDictionary;
            if (dict != null)
            {
                foreach (Object key in dict.Keys)
                {
                    // add an DictionaryItemMemberDescriptor fore each key in the list
                    fields.Add(new DictionaryItemMemberDescriptor(dict, key));
                }
            }
            
            // add all the instance fields
            while (objectHierarchy.Count > 0)
            {
                AddTypeFields(component, objectHierarchy.Pop(), fields, addedMemberNames);
            }
            
            return new PropertyDescriptorCollection(fields.ToArray());
        }

        // This method can be used to retrieve an Image from a block of Base64-encoded text.
        private Image DeserializeFromBase64Text(string text)
        {
            Image img = null;
            byte[] memBytes = Convert.FromBase64String(text);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(memBytes);
            img = (Image)formatter.Deserialize(stream);
            stream.Close();
            return img;
        }
    }
}