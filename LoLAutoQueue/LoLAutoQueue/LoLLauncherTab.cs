using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoLAutoQueue
{
    public partial class LoLLauncherTab : UserControl
    {
        public LoLLauncherTab()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoLLauncherClient client = new LoLLauncherClient(textBox1.Text, textBox2.Text, LoLLauncher.Region.NA, richTextBox1);
        }
    }
}
