using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class CTSmartButtons
    {
        private Interface CTInterface;
        private SmartButtonForm dialog;
        private List<ToolStripButton> SmartButtonsList;
        public CTSmartButtons(Interface CTInterface)
        {
            SmartButtonsList = new List<ToolStripButton>();
            this.CTInterface = CTInterface;
            dialog = new SmartButtonForm(CTInterface);
        }
        public void SmartBtnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialog.RefreshValues();

            // Linux change
            this.CTInterface.FileBrowser.StatusLabel.Text = "Dialog open";
            dialog.ShowDialog();
            this.CTInterface.FileBrowser.StatusLabel.Text = "Ready";

            StoreToSettings();
            LoadAccSettings();
        }
        public void LoadAccSettings()
        {
            
            CTHotKeys.MemoryUnit mu;
            dialog.RefreshValues();

            for (int i = 0; i<dialog.SmartButtonsList.Items.Count; i++)
            {
                
                mu = mu_PluginOrProtocol((string)dialog.SmartButtonsList.Items[i]);

                if (mu == null)
                {
                    mu = new CTHotKeys.MemoryUnit((string)dialog.SmartButtonsList.Items[i]);
                    CTInterface.HotKays.SetAssociatedControl(mu);
                }

                if (mu.GetAssociatedControl == null) continue;

                if (SmartButtonsList.Count <= i)
                {
                    ToolStripButton btn = new ToolStripButton();
                    btn.Click += SmartButton_Click;
                    SmartButtonsList.Add(btn);

                    btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    btn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    btn.BackColor = CTInterface.FileBrowser.TaskBtnColor1;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.Image = Properties.Resources.Save;
                    CTInterface.taskTS.Items.Add(btn);
                }

                SmartButtonsList[i].Text = mu.GetName;
                SmartButtonsList[i].Tag = mu;
            }

            for (int i = SmartButtonsList.Count - 1;
                i > dialog.SmartButtonsList.Items.Count - 1; i--)
            {
                SmartButtonsList[i].Dispose();
                SmartButtonsList.RemoveAt(i);
            }
        }
        private CTHotKeys.MemoryUnit mu_PluginOrProtocol(string name)
        {
            CTHotKeys.MemoryUnit mu = null;

            foreach (var val in CTInterface.DeveloperToolStripMenuItem.DropDownItems)
                if( val is ToolStripMenuItem && ((ToolStripMenuItem) val).Text == name)
                {
                    mu = new CTHotKeys.MemoryUnit(name);
                    mu.SetAssociatedControl = val;
                    return mu;
                }

                for (int i = 1; i < CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items.Count; i++)
                {
                    string str = CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items[i].ToString();
                    if (str != name) continue;
                    //create memory unit
                    mu = new CTHotKeys.MemoryUnit(str);
                    //set associated control
                    {
                        Button btn = new Button();
                        btn.Tag = str;
                        btn.Click += CTInterface.HotKays.AutoSetUpBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    return mu;
                }

            return mu;
        }
        private void SmartButton_Click(object sender, EventArgs e)
        {
            CTHotKeys.MemoryUnit mu = (CTHotKeys.MemoryUnit)((ToolStripButton)sender).Tag;
            mu.ActivateAssociatedControl();
        }
        private void StoreToSettings()
        {

            string[] propArr = new string[dialog.SmartButtonsList.Items.Count];
            for (int i = 0; i < propArr.Length; i++)
                propArr[i] = (string)dialog.SmartButtonsList.Items[i];

            if (propArr.Length > 0)
                Properties.Settings.Default.
                SmartBtns[CTInterface.FileBrowser.ActiveAccountIndex] =
                "@\t" + string.Join("\t", propArr);
            else
                Properties.Settings.Default.
                            SmartBtns[CTInterface.FileBrowser.ActiveAccountIndex] =
                            "@";

            Properties.Settings.Default.Save();
        }

        class SmartButtonForm :Form
        {
            public ListBox SmartButtonsList;
            private ListBox Library;
            Interface CTInterface;
            public SmartButtonForm(Interface CTInterface)
            {
                this.CTInterface = CTInterface;
                this.SuspendLayout();

                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.Text = "Smart Buttons";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Normal;
                this.MinimizeBox = false;
                this.MaximizeBox = false;

                this.Width = 500;
                this.Height = 400;

                this.BackColor = CTInterface.FileBrowser.BackGroundColor1;
                this.ForeColor = CTInterface.FileBrowser.ShriftColor1;
                this.FormClosing += new FormClosingEventHandler(
                    delegate (object o, FormClosingEventArgs a)
                    {
                        this.Hide();
                        a.Cancel = true;
                    });
                
                Library = new ListBox();
                Library.BackColor = CTInterface.FileBrowser.BackGround2Color1;
                Library.ForeColor = CTInterface.FileBrowser.ShriftColor1;
                Library.Dock = DockStyle.Right;
                Library.Width = 200;
                this.Controls.Add(Library);

                SmartButtonsList = new ListBox();
                SmartButtonsList.BackColor = CTInterface.FileBrowser.BackGround2Color1;
                SmartButtonsList.ForeColor = CTInterface.FileBrowser.ShriftColor1;
                SmartButtonsList.Dock = DockStyle.Left;
                SmartButtonsList.Width = 200;
                this.Controls.Add(SmartButtonsList);

                {
                    Button btn = new Button();
                    btn.Text = "Add";
                    btn.Width = 75;
                    btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.Location = new System.Drawing.Point(205, 30);
                    btn.Click += AddBtn_Click;
                    this.Controls.Add(btn);
                }
                {
                    Button btn = new Button();
                    btn.Text = "Remove";
                    btn.Width = 75;
                    btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.Location = new System.Drawing.Point(205, 60);
                    btn.Click += RemoveBtn_Click;
                    this.Controls.Add(btn);
                }
                {
                    Button btn = new Button();
                    btn.Text = "Up";
                    btn.Width = 75;
                    btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.Location = new System.Drawing.Point(205, 90);
                    btn.Click += UpBtn_Click;
                    this.Controls.Add(btn);
                }
                {
                    Button btn = new Button();
                    btn.Text = "Down";
                    btn.Width = 75;
                    btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.Location = new System.Drawing.Point(205, 120);
                    btn.Click += DownBtn_Click;
                    this.Controls.Add(btn);
                }

                this.ResumeLayout();
            }
            private void AddBtn_Click(object sender,EventArgs e)
            {
                if (Library.SelectedItem == null) return;
                string str = (string)Library.SelectedItem;

                int ind = SmartButtonsList.Items.Count;
                if (SmartButtonsList.SelectedItem != null)
                    ind = SmartButtonsList.SelectedIndex+1;

                if (!SmartButtonsList.Items.Contains(str))
                    SmartButtonsList.Items.Insert(ind,str);
            }
            private void RemoveBtn_Click(object sender, EventArgs e)
            {
                if (SmartButtonsList.SelectedItem == null) return;
                SmartButtonsList.Items.RemoveAt(SmartButtonsList.SelectedIndex);
            }
            private void UpBtn_Click(object sender, EventArgs e)
            {
                if (SmartButtonsList.SelectedItem == null || SmartButtonsList.SelectedIndex < 1) return;
                int ind = SmartButtonsList.SelectedIndex;
                string str = (string)SmartButtonsList.SelectedItem;

                SmartButtonsList.Items.RemoveAt(ind);
                SmartButtonsList.Items.Insert(ind - 1, str);
            }
            private void DownBtn_Click(object sender, EventArgs e)
            {
                if (SmartButtonsList.SelectedItem == null || 
                    SmartButtonsList.SelectedIndex >= SmartButtonsList.Items.Count - 1) return;

                int ind = SmartButtonsList.SelectedIndex;
                string str = (string)SmartButtonsList.SelectedItem;

                SmartButtonsList.Items.RemoveAt(ind);
                SmartButtonsList.Items.Insert(ind + 1, str);
            }
            public void RefreshValues()
            {
                SmartButtonsList.Items.Clear();
                Library.Items.Clear();

                Library.Items.AddRange(CTInterface.HotKays.GetControlNames());

                foreach (var val in CTInterface.DeveloperToolStripMenuItem.DropDownItems)
                    if(val is ToolStripMenuItem)
                        Library.Items.Add(((ToolStripMenuItem)val).Text);

                foreach (var val in CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items)
                    if((string)val != "None")
                        Library.Items.Add(val);
                try
                {
                    string[] propArr = Properties.Settings.Default.
                    SmartBtns[CTInterface.FileBrowser.ActiveAccountIndex].Split('\t');

                    foreach (string str in propArr)
                        if (str != "@" && str != "")
                        {
                            SmartButtonsList.Items.Add(str);
                        }

                    propArr = null;
                }
                catch { }
            }

        }
    }
}
