using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileRename
{
    public partial class AddKeywords : Form
    {
        public AddKeywords()
        {
            InitializeComponent();
        }

        public delegate void TextEventHandler(string strText);
        public TextEventHandler TextHandler;

        private void button1_Click(object sender, EventArgs e)
        {
            if (TextHandler != null)
            {
                TextHandler.Invoke(textBox1.Text);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
