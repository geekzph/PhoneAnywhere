using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace weibo
{
    public partial class settings : Form
    {
        public settings()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
            {
                panel2.Visible = true;
                panel1.Visible = false;
            }
            if (listBox1.SelectedIndex == 2)
            {
                panel1.Visible = true;
                panel2.Visible = false;
            }
        }

        void dosettings()
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
            RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");

            RegistryKey run = Registry.CurrentUser;
            RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (checkBox1.Checked == true)
            {
                
                autorun.SetValue("PhoneAnywhere", Application.ExecutablePath);
            }
            else
            {
                autorun.SetValue("PhoneAnywhere","");
            }
            if (checkBox2.Checked == true)
            {
                key1.SetValue("auto", "yes");
            }
            else
            {
                key1.SetValue("auto", "");
            }
            key1.SetValue("name", textBox1.Text);
            key1.SetValue("pwd", textBox2.Text);
            if (checkBox2.Checked == true && textBox2.Text == "")
                MessageBox.Show("请输入密码");
            else
                this.Close();
                
        }
        private void button1_Click(object sender, EventArgs e)
        {
            dosettings();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void settings_Load(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
            RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
            try
            {
                if (key1.GetValue("auto").ToString() == "yes")
                    checkBox2.Checked = true;
                else
                    checkBox2.Checked = false;
            }
            catch
            {
                checkBox2.Checked = false;
            }
            try
            {
                textBox1.Text = key1.GetValue("name").ToString();
            }
            catch
            { }
            try
            {
                textBox2.Text = key1.GetValue("pwd").ToString();
            }
            catch
            { }
            RegistryKey run = Registry.CurrentUser;
            RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            try
            {
                if (autorun.GetValue("PhoneAnywhere").ToString() != "")
                    checkBox1.Checked = true;
                else
                    checkBox1.Checked = false;
            }
            catch
            { }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            { }
            ///checkBox2.Checked = true;
            else
            {
                RegistryKey run = Registry.CurrentUser;
                RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                autorun.SetValue("PhoneAnywhere", "");
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
            RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
            if(checkBox2.Checked==false)
            key1.SetValue("auto", "");
        }
    }
}
