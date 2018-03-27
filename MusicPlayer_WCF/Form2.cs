using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayer_WCF
{
    public partial class Form2 : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Form1 reference = new Form1();

        public Form2()
        {
            InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = 5000;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (reference != null)
                {
                    reference.Show();
                    this.Hide();
                }
                else
                {
                    this.Close();
                }
            }
            catch (System.ObjectDisposedException ex)
            {
                this.Close();

            }
 
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
           
        }


    }
}
