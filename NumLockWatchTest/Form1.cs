﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Timers;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Data.Odbc;

namespace NumLockWatchTest
{
    public partial class Form1 : Form
    {
        public class win32api
        {
            [DllImport("user32.dll")]
            public static extern uint keybd_event(byte b1, byte b2, uint ui1 , UIntPtr uIntPtr1 );
        }

        public Form1()
        {
            InitializeComponent();

            timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_func;
            timer.Start();

            bool regOk = OpenRegKey();
            if (regOk == false)
            {
                MessageBox.Show("キーボードのDeviceIDを登録してください。");
                this.ShowInTaskbar = true;
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
                this.Show();
            }

        }
        private bool OpenRegKey()
        {
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\NumlockWatcher\ID\", true);
            if (registryKey == null)
            {
                registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\NumlockWatcher\ID\", true);
            }

            if(registryKey == null)
            {
                textBox1.Clear();
                textBox1.AppendText("レジストリが開けませんでした。");
                return false;
            }

            var val = registryKey.GetValue("stringID");
            if (val == null)
            {
                registryKey.SetValue("stringID", "");
                val = registryKey.GetValue("stringID");
                if (val == null)
                {
                    textBox1.Clear();
                    textBox1.AppendText("レジストリが開けませんでした。");
                    return false;
                }
            }

            if (val.ToString() == "")
            {
                textBox1.Clear();
                textBox1.AppendText("レジストリキーを作成しました。");
                return false;
            }
            else
            {
                textBox2.AppendText(val.ToString());

            }
            return true;
        }

        private void Taskmenu_Close(object sender, EventArgs e) 
        {
            this.notifyIcon1.Dispose();
            Application.Exit();
        }

        private void Timer_func(object sender, ElapsedEventArgs e)
        {
            find = find10keyKbd();

            if (Control.IsKeyLocked(Keys.NumLock))
            {
                if (find)
                {
                    return;
                }
                else
                {
                    win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
                    win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
                }
            }
            else
            {
                if (!find)
                {
                    return;
                }
                else
                {
                    win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
                    win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
                }
            }

            if (find)
                find = false;

            timer.Interval = 60000;
        }

        public void updateText()
        {
            textBox1.Clear();

            if (str1 != null)
            {
                textBox1.AppendText(str1);
                str1 = "";
            }
            else
            {
                textBox1.AppendText("Devices is None...");
            }
        }

        public void PrintDevices()
        {
            IList<ManagementBaseObject> Devices = GetkbdDevices();
            string str = "------------------ DEVICE ------------------\r\n";

            foreach (ManagementBaseObject Device in Devices)
            {
                str1 += (str);
                foreach (var property in Device.Properties)
                {
                   str1 += string.Format("{0}: {1}\r\n", property.Name, property.Value);
                }
            }
        }
        public bool find10keyKbd()
        {
            IList<ManagementBaseObject> Devices = GetkbdDevices();
            foreach (ManagementBaseObject Device in Devices)
            {
                foreach (var property in Device.Properties)
                {
                    if(property.Name == "DeviceID")
                    {
                        if(textBox2.Text != "")
                        {
                            if (property.Value.ToString().StartsWith(textBox2.Text))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static IList<ManagementBaseObject> GetkbdDevices()
        {
            List<ManagementBaseObject> comDevices = new List<ManagementBaseObject>();

            ManagementObjectCollection curMoc = QueryMi("SELECT * FROM Win32_Keyboard");
            foreach (ManagementBaseObject device in curMoc)
            {
                comDevices.Add(device);
            }

            return comDevices;
        }


        // run a query against Windows Management Infrastructure (MI) and return the resulting collection
        public static ManagementObjectCollection QueryMi(string query)
        {
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection result = managementObjectSearcher.Get();

            managementObjectSearcher.Dispose();
            return result;
        } 

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CLOSE:
                    this.ShowInTaskbar = false;
                    this.Hide();
                    return;
                case WM_KEYDOWN:
                    break;
                case WM_DEVICECHANGE:
                    timer.Start();
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        private void run_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.AppendText("serching...");

            PrintDevices();
            updateText();
        }
        private void Form_keyDown(ref Message m)
        {
        }

        private void Form1_RightToLeftLayoutChanged(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
/*
            textBox1.AppendText(e.KeyData.ToString() + "\r\n");
            textBox1.AppendText(e.KeyCode.ToString() + "\r\n");
            textBox1.AppendText(e.KeyValue.ToString() + "\r\n");
*/
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            return;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void OnUpdateText(object sender, PaintEventArgs e)
        {
            if(timer.Enabled == false)
            {
                toolStripTextBox1.Text = "停止中";
            }
            else
            {
                toolStripTextBox1.Text = "監視中";
            }

            if (Control.IsKeyLocked(Keys.NumLock))
            {
                toolStripTextBox2.Text = "NumLock ON";
            }
            else
            {
                toolStripTextBox2.Text = "NumLock OFF";
            }
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            if (timer.Enabled == false)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void toolStripTextBox2_Click(object sender, EventArgs e)
        {
            win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
            win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            this.Show();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue.Equals(0x0d))
            {
                Setregkey();
            }
        }

        private void textBox2_Layout(object sender, LayoutEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Setregkey();
        }
        private void Setregkey()
        {
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\NumlockWatcher\ID", true);
            if (registryKey != null)
            {
                registryKey.SetValue("stringID", textBox2.Text);
            }
            else
            {
                textBox1.Clear();
                textBox1.AppendText("レジストリが開けませんでした。");
            }

            timer.Interval = 100;
        }
    }
}