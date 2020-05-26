using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Management;
using System.Timers;
using System.Runtime.InteropServices;

namespace NumLockWatchTest
{
    public partial class Form1 : Form
    {
        public class Win32api
        {
            [DllImport("user32.dll")]
            public static extern uint keybd_event(byte b1, byte b2, uint ui1, UIntPtr uIntPtr1);
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
                _ = MessageBox.Show(text: "キーボードのDeviceIDを登録してください。");
                ShowInTaskbar = true;
                WindowState = System.Windows.Forms.FormWindowState.Normal;
                Show();
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
            Application_Exit(sender, e);
        }

        private void Timer_func(object sender, ElapsedEventArgs e)
        {
            find = Find10keyKbd();

            if (Control.IsKeyLocked(Keys.NumLock))
            {
                if (find)
                {
                    return;
                }
                else
                {
                    _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
                    _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
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
                    _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
                    _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
                }
            }

            if (find)
                find = false;

            timer.Interval = 60000;
        }

        public void UpdateText()
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
        public bool Find10keyKbd()
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
                    ShowInTaskbar = false;
                    Hide();
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

        private void Run_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.AppendText("serching...");

            PrintDevices();
            UpdateText();
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

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void OnUpdateText(object sender, PaintEventArgs e)
        {
            toolStripTextBox1.Text = timer.Enabled == false ? "停止中" : "監視中";

            toolStripTextBox2.Text = IsKeyLocked(Keys.NumLock) ? "NumLock ON" : "NumLock OFF";
        }

        private void ToolStripTextBox1_Click(object sender, EventArgs e)
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

        private void ToolStripTextBox2_Click(object sender, EventArgs e)
        {
            _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
            _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = System.Windows.Forms.FormWindowState.Normal;
            Show();
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void TextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue.Equals(0x0d))
            {
                Setregkey();
            }
        }

        private void TextBox2_Layout(object sender, LayoutEventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
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

        private void Reg_erase_button_Click(object sender, EventArgs e)
        {
            if( MessageBox.Show("レジストリの登録を削除して終了します。","info", MessageBoxButtons.YesNo ) == DialogResult.Yes )
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\NumlockWatcher", false);
                Application_Exit(sender, e);
            }
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            timer.Dispose();
            notifyIcon1.Dispose();
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IList<ManagementBaseObject> Devices = GetkbdDevices();

            dataGridView1.ColumnCount = 1;
            dataGridView1.Columns[0].HeaderText = "DeviceID";

            foreach (ManagementBaseObject Device in Devices)
            {
                foreach (var property in Device.Properties)
                {
                    if (property.Name == "DeviceID")
                    {
                        dataGridView1.Rows.Add(property.Value.ToString());
                    }
                }
            }

            dataGridView1.AutoResizeColumns();

            button2.Text = timer.Enabled == false ? "停止中" : "監視中";
            button3.Text = IsKeyLocked(Keys.NumLock) ? "NumLock ON" : "NumLock OFF";


        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox2.Clear();
            textBox2.AppendText(dataGridView1.CurrentCell.Value.ToString());
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (timer.Enabled == false)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }

            button2.Text = timer.Enabled == false ? "停止中" : "監視中";

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYDOWN, (UIntPtr)0);
            _ = Win32api.keybd_event(VK_NUMLOCK, 0, KEYEVENTF_KEYUP, (UIntPtr)0);

            button3.Text = IsKeyLocked(Keys.NumLock) ? "NumLock ON" : "NumLock OFF";

        }
    }
}