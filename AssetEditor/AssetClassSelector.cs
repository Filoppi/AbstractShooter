using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetEditor
{
    public partial class AssetClassSelector : Form
    {
        public Type selectedType;
        
        public delegate void ClassSelectedEventHandler(Type type);
        public event ClassSelectedEventHandler classSelected;

        public AssetClassSelector()
        {
            InitializeComponent();
        }

        public AssetClassSelector(List<Type> types)
        {
            InitializeComponent();
            
            types.ForEach(type => comboBox1.Items.Add(type));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            selectedType = (Type)comboBox1.SelectedItem;

            classSelected?.Invoke(selectedType);
            
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedType = (Type)comboBox1.SelectedItem;
            
            okButton.Enabled = selectedType != null;
        }
    }
}
