/*
 CellTool - software for bio-image analysis
 Copyright (C) 2018  Georgi Danovski

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Cell_Tool_3
{
    class MetadataProp
    {
        public ImageAnalyser IA = null;
        private PropertiesPanel_Item PropPanel = new PropertiesPanel_Item();
        public Panel panel;
        private ToolTip TurnOnToolTip = new ToolTip();

        private Panel Panel1 = new Panel();
        private Label fileName = new Label();
        private Label filePath = new Label();
        private Label fileDimensions = new Label();
        private Label fileType = new Label();
        private Label filePixelSize = new Label();
        private TreeView tv = new TreeView();
        public Button MetaBtn = new Button();
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            var parent = ctr.Parent;
            if (parent.Width < ctr.Location.X + ctr.Width)
            {
                TurnOnToolTip.SetToolTip(ctr, ctr.Text);
            }
            else
            {
                TurnOnToolTip.RemoveAll();
            }
        }
        public void Initialize(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            //PropPanel properties
            PropPanel.Initialize(propertiesPanel);
            PropPanel.Resizable = false;

            PropPanel.Name.Text = "Metadata";

            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;
            panel.Visible = false;
            panel.VisibleChanged += VisibleChanged;
            panel.BringToFront();

            //Add button on top
            Button btn = MetaBtn;
            btn.Width = 21;
            btn.Height = 21;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = IA.FileBrowser.TitlePanelColor1;
            btn.ForeColor = IA.FileBrowser.ShriftColor1;
            btn.Image = new Bitmap(Properties.Resources.settings, new Size(18, 18));
            btn.FlatAppearance.BorderSize = 0;
            btn.Text = "";
            PropPanel.Panel.Controls.Add(btn);
            btn.BringToFront();
            btn.Location = new Point(PropPanel.Panel.Width - 25, 0);
            btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
            {
                TurnOnToolTip.SetToolTip(btn, "View metadata details");
            });
            btn.Click += StartDialog;
            //

            Panel1.Dock = DockStyle.Fill;
            Panel1.AutoScroll = false;

            panel.Controls.Add(Panel1);
            Panel1.BringToFront();

            {
                int Y = 2;
                //name
                Label l1 = new Label();
                l1.Text = "Name:";
                l1.Location = new Point(5, Y);
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
                l1.Height = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
                Panel1.Controls.Add(l1);

                Label l2 = fileName;
                l2.Text = "";
                l2.MouseHover += new EventHandler(Control_MouseOver);
                l2.Location = new Point(80, Y);
                Panel1.Controls.Add(l2);
            }
            {
                int Y = 22;
                //name
                Label l1 = new Label();
                l1.Text = "Path:";
                l1.Location = new Point(5, Y);
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
                l1.Height = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
                Panel1.Controls.Add(l1);

                Label l2 = filePath;
                l2.Text = "";
                l2.MouseHover += new EventHandler(Control_MouseOver);
                l2.Location = new Point(80, Y);
                Panel1.Controls.Add(l2);
            }
            {
                int Y = 42;
                //name
                Label l1 = new Label();
                l1.Text = "Type:";
                l1.Location = new Point(5, Y);
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
                l1.Height = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
                Panel1.Controls.Add(l1);

                Label l2 = fileType;
                l2.Text = "";
                l2.Location = new Point(80, Y);
                l2.MouseHover += new EventHandler(Control_MouseOver);
                Panel1.Controls.Add(l2);
            }
            {
                int Y = 62;
                //name
                Label l1 = new Label();
                l1.Text = "Dimensions:";
                l1.Location = new Point(5, Y);
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
                l1.Height = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
                Panel1.Controls.Add(l1);

                Label l2 = fileDimensions;
                l2.Text = "";
                l2.Location = new Point(80, Y);
                l2.MouseHover += new EventHandler(Control_MouseOver);
                Panel1.Controls.Add(l2);
            }
            {
                int Y = 82;
                //name
                Label l1 = new Label();
                l1.Text = "Voxel size:";
                l1.Location = new Point(5, Y);
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
                l1.Height = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
                Panel1.Controls.Add(l1);

                Label l2 = filePixelSize;
                l2.Text = "";
                l2.Location = new Point(80, Y);
                l2.MouseHover += new EventHandler(Control_MouseOver);
                Panel1.Controls.Add(l2);
            }
        }
        private void VisibleChanged(object sender, EventArgs e)
        {
            if (panel.Visible == false) { return; }
            UpdateInfo();
        }
        public void BackColor(Color color)
        {
            PropPanel.BackColor(color);
        }
        public void ForeColor(Color color)
        {
            PropPanel.ForeColor(color);
        }
        public void TitleColor(Color color)
        {
            PropPanel.TitleColor(color);
        }
        private void RefreshTagField(string str, Label l1)
        {
            if (l1.Text == str) { return; }
            l1.Text = str;
            int w = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            int h = TextRenderer.MeasureText(l1.Text, l1.Font).Height;
            if (l1.Width != w) { l1.Width = w; }
            if (l1.Height != h) { l1.Height = h; }
        }
        public void UpdateInfo()
        {
            if (IA.TabPages.SelectedIndex < 0) { return; }
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (fi == null) return;

            string str = IA.TabPages.FileNameFromDir(fi.Dir);
            RefreshTagField(str, fileName);

            str = fi.Dir;
            RefreshTagField(str, filePath);

            str = "X: " + fi.sizeX.ToString() +
                "    Y: " + fi.sizeY.ToString() +
                "    Z: " + fi.sizeZ.ToString() +
                "    T: " + fi.sizeT.ToString();
            RefreshTagField(str, fileDimensions);

            str = fi.sizeC.ToString() + " - color " +
                fi.bitsPerPixel.ToString() + "-bit image";
            RefreshTagField(str, fileType);

            str = "X: " + fi.umXY.ToString() +
                "um    Y: " + fi.umXY.ToString() +
                "um    Z: " + fi.umZ.ToString() + "um";
            RefreshTagField(str, filePixelSize);

        }
        public void StartDialog(object sender, EventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            tv.Tag = fi;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "Metadata";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.BackColor = IA.FileBrowser.BackGround2Color1;
            OptionForm.ForeColor = IA.FileBrowser.ShriftColor1;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;

            Panel core = new Panel();
            core.AutoScroll = true;
            RepeatT_Add(core, fi);
            int w = AddTags(core, fi);

            OptionForm.Width = 300;
            OptionForm.Height = 380;

            core.Dock = DockStyle.Fill;
            OptionForm.Controls.Add(core);
            OptionForm.FormClosing += OptionForm_Closing;

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private void OptionForm_Closing(object sender, EventArgs e)
        {
            List<double> l = new List<double>();
            for (int i = 0; i < tv.Nodes.Count; i++)
            {
                double[] l1 = (double[])tv.Nodes[i].Tag;
                l.Add(l1[0]);
                l.Add(l1[1]);
            }

            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            fi.TimeSteps = l;

            IA.ReloadImages();
            IA.MarkAsNotSaved();

            if(fi.imageCount != fi.sizeT * fi.sizeC * fi.sizeZ)
            {
                MessageBox.Show("Slices in the image are not equal to T*Z*C.\nPlease edit them to prevent errors!");
            }
        }

        private void RepeatT_Add(Panel Core, TifFileInfo fi)
        {
            Panel p1 = new Panel();
            p1.Height = 60;
            p1.Dock = DockStyle.Fill;
            Core.Controls.Add(p1);

            Label nameLabel = new Label();
            nameLabel.Location = new Point(5, 5);
            nameLabel.Width = 100;
            nameLabel.Text = "Time intervals:";
            p1.Controls.Add(nameLabel);

            tv = new TreeView();
            //tv.Dock = DockStyle.Right;
            tv.Width = 170;
            tv.Height = 65;
            tv.Location = new Point(110, 5);
            ToolTip tvToolTip = new ToolTip();
            tv.MouseHover += new EventHandler(delegate (object o, EventArgs a) {
                tvToolTip.SetToolTip(tv, "Right mouse click to modify");
            });
            //tv.BackColor = IA.FileBrowser.BackGround2Color1;
            //tv.ForeColor = IA.FileBrowser.ShriftColor1;
            //tv.BorderStyle = BorderStyle.None;
            tv.Scrollable = true;
            tv.ShowPlusMinus = false;
            tv.ShowRootLines = false;
            p1.Controls.Add(tv);

            Panel p2 = new Panel();
            p2.Height = 5;
            p2.Dock = DockStyle.Top;
            p1.Controls.Add(p2);

            for (int i = 0; i < fi.TimeSteps.Count; i += 2)
            {
                TreeNode tn1 = new TreeNode();
                tn1.Tag = new Double[2] { fi.TimeSteps[i], fi.TimeSteps[i + 1] };
                tn1.Text = "Repeat T - " + fi.TimeSteps[i] +
                    " times (" + fi.TimeSteps[i + 1] + " sec)";
                tv.Nodes.Add(tn1);
            }
            tv.NodeMouseClick += new TreeNodeMouseClickEventHandler(
               delegate (object o, TreeNodeMouseClickEventArgs e)
               {
                   if (e.Button == MouseButtons.Right)
                   {
                       TreeNode tn = e.Node;

                       ContextMenu menu = new ContextMenu();

                       MenuItem EditMB = new MenuItem();
                       EditMB.Text = "Edit";
                       menu.MenuItems.Add(EditMB);
                       EditMB.Click += new EventHandler(
                           delegate (object ob, EventArgs b)
                           {
                               Node_Edit(tn, true);
                           });

                       MenuItem AddMB = new MenuItem();
                       AddMB.Text = "Add";
                       menu.MenuItems.Add(AddMB);
                       AddMB.Click += new EventHandler(
                           delegate (object ob, EventArgs b)
                           {
                               TreeNode tn1 = new TreeNode();
                               tv.Nodes.Insert(tn.Index + 1, tn1);
                               tn1.Tag = new Double[2] { 0, 1 };
                               Node_Edit(tn1, false);
                           });

                       MenuItem sepMB = new MenuItem();
                       sepMB.Text = "-";
                       menu.MenuItems.Add(sepMB);

                       MenuItem UpMB = new MenuItem();
                       UpMB.Text = "Move Up";
                       menu.MenuItems.Add(UpMB);
                       if (tn.Index == 0)
                       {
                           UpMB.Enabled = false;
                       }
                       UpMB.Click += new EventHandler(
                           delegate (object ob, EventArgs b)
                           {
                               int ind = tn.Index;
                               if (ind > 0)
                               {
                                   tv.Nodes.RemoveAt(tn.Index);
                                   tv.Nodes.Insert(ind - 1, tn);
                               }
                           });

                       MenuItem DownMB = new MenuItem();
                       DownMB.Text = "Move Down";
                       menu.MenuItems.Add(DownMB);
                       if (tn.Index >= tv.Nodes.Count - 1)
                       {
                           DownMB.Enabled = false;
                       }
                       DownMB.Click += new EventHandler(
                       delegate (object ob, EventArgs b)
                       {
                           int ind = tn.Index;
                           if (ind < tv.Nodes.Count - 1)
                           {
                               tv.Nodes.RemoveAt(tn.Index);
                               tv.Nodes.Insert(ind + 1, tn);
                           }
                       });

                       MenuItem sepMB1 = new MenuItem();
                       sepMB1.Text = "-";
                       menu.MenuItems.Add(sepMB1);

                       MenuItem SaveMB = new MenuItem();
                       SaveMB.Text = "Save As TXT";
                       SaveMB.Click += saveMetadataTXT;
                       menu.MenuItems.Add(SaveMB);

                       MenuItem loadMB = new MenuItem();
                       loadMB.Text = "Load from TXT";
                       loadMB.Click += readMetadataTXT;
                       menu.MenuItems.Add(loadMB);

                       MenuItem sepMB2 = new MenuItem();
                       sepMB2.Text = "-";
                       menu.MenuItems.Add(sepMB2);

                       MenuItem DelMB = new MenuItem();
                       DelMB.Text = "Delete";
                       menu.MenuItems.Add(DelMB);
                       if (tv.Nodes.Count <= 1)
                       {
                           DelMB.Enabled = false;
                       }
                       DelMB.Click += new EventHandler(
                       delegate (object ob, EventArgs b)
                       {
                           if (tv.Nodes.Count <= 1)
                           {
                               return;
                           }
                           int ind = tn.Index;
                           tv.Nodes.RemoveAt(tn.Index);
                       });

                       menu.Show(tv, new Point(e.X, e.Y));
                   }

               });

        }
        private void saveMetadataTXT(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;


            SaveFileDialog ofd = new SaveFileDialog();
            string formatMiniStr = ".txt";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;

            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Title = "Save time steps:";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string txt = "";
                foreach (TreeNode n in tv.Nodes)
                {
                    txt += n.Text + "\n";
                }

                try
                {
                    File.WriteAllText(ofd.FileName, txt);
                }
                catch
                {
                    MessageBox.Show("Error - file is used by other program!");
                }
            }
        }
        private void readMetadataTXT(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;


            OpenFileDialog ofd = new OpenFileDialog();
            string formatMiniStr = ".txt";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;

            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Title = "Load time steps:";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
               try
                {

                    using (StreamReader sr = new StreamReader(ofd.FileName))
                    {
                        string txt = sr.ReadToEnd();
                        List<TreeNode> lN = new List<TreeNode>();
                        List<double> TimeSteps = new List<double>();

                        foreach (string row in txt.Split(new[] { "\n" }, StringSplitOptions.None))
                        {
                            if (row.IndexOf("Repeat T - ") > -1)
                            {
                                string str = row.Replace(" - fastest", "").Replace("	", "").Replace("Repeat T - ", "")
                                    .Replace(" times (", "-").Replace(" time (", "-")
                                    .Replace(" sec)", "").Replace(" ms)", "").Replace(" min)", "");
                                foreach (string val in str.Split(new[] { "-" }, StringSplitOptions.None))
                                {
                                    TimeSteps.Add(Single.Parse(val));
                                }
                                //change time unit to sec
                                if (row.IndexOf(" ms)") > -1)
                                {
                                    int z = TimeSteps.Count - 1;
                                    TimeSteps[z] /= 1000;
                                }
                                else if (row.IndexOf(" min)") > -1)
                                {
                                    int z = TimeSteps.Count - 1;
                                    TimeSteps[z] *= 60;
                                }

                                TreeNode tn1 = new TreeNode();
                                lN.Add(tn1);
                                tn1.Tag = new double[2] { TimeSteps[TimeSteps.Count - 2],
                                TimeSteps[TimeSteps.Count - 1] };
                                tn1.Text = "Repeat T - " + TimeSteps[TimeSteps.Count - 2].ToString() +
                        " times (" + TimeSteps[TimeSteps.Count - 1].ToString() + " sec)";
                            }
                        }

                        if (TimeSteps.Count > 1)
                        {
                            fi.TimeSteps = TimeSteps;
                            tv.Nodes.Clear();

                            foreach (TreeNode tn in lN)
                                tv.Nodes.Add(tn);
                        }

                        else
                       {
                            MessageBox.Show("There is no time step information in this file!");
                        }

                    }
                }
               catch
                {
                   MessageBox.Show("Error - file is used by other program!");
                }

            }
        }
        private void Node_Edit(TreeNode n,bool oldN)
        {
            double[] l = (double[])n.Tag;

            Form OptionForm1 = new Form();
            OptionForm1.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm1.Text = "Edit";
            OptionForm1.StartPosition = FormStartPosition.CenterScreen;
            OptionForm1.WindowState = FormWindowState.Normal;
            OptionForm1.BackColor = IA.FileBrowser.BackGround2Color1;
            OptionForm1.ForeColor = IA.FileBrowser.ShriftColor1;
            OptionForm1.MinimizeBox = false;
            OptionForm1.MaximizeBox = false;
            OptionForm1.KeyPreview = true;
            OptionForm1.Width = 200;
            OptionForm1.Height = 140;

            Label RepLab = new Label();
            RepLab.Text = "Repeats:";
            RepLab.Location = new Point(5, 8);
            RepLab.Width = 90;
            OptionForm1.Controls.Add(RepLab);

            TextBox RepTB = new TextBox();
            RepTB.Text = l[0].ToString();
            RepTB.Location = new Point(100, 5);
            RepTB.Width = 70;
            OptionForm1.Controls.Add(RepTB);

            Label TLab = new Label();
            TLab.Text = "Step (sec.):";
            TLab.Location = new Point(5, 33);
            TLab.Width = 90;
            OptionForm1.Controls.Add(TLab);

            TextBox TTB = new TextBox();
            TTB.Text = l[1].ToString();
            TTB.Location = new Point(100, 30);
            TTB.Width = 70;
            OptionForm1.Controls.Add(TTB);
            

            Button cancelBtn = new Button();
            cancelBtn.BackColor = SystemColors.ButtonFace;
            cancelBtn.ForeColor = Color.Black;
            cancelBtn.Text = "Cancel";
            cancelBtn.Location = new Point(100, 70);
            cancelBtn.Click += new EventHandler(delegate (object ob, EventArgs b)
            {
                OptionForm1.Close();
            });
            OptionForm1.Controls.Add(cancelBtn);

            Button okBtn = new Button();
            okBtn.BackColor = SystemColors.ButtonFace;
            okBtn.ForeColor = Color.Black;
            okBtn.Text = "Apply";
            okBtn.Location = new Point(85 - okBtn.Width, 70);
            OptionForm1.Controls.Add(okBtn);
            bool applyChanges = false;
            okBtn.Click += new EventHandler(delegate (object ob, EventArgs b)
            {
                try {
                    Convert.ToDouble(TTB.Text);
                    Convert.ToDouble(RepTB.Text);
                    applyChanges = true;
                    OptionForm1.Close();
                }
                catch
                {
                    MessageBox.Show("Incorrect value!");
                }
            });
            OptionForm1.KeyDown += new KeyEventHandler(
                delegate (object ob, KeyEventArgs b) {
                    if(b.KeyCode == Keys.Enter)
                    {
                        try
                        {
                            Convert.ToDouble(TTB.Text);
                            Convert.ToDouble(RepTB.Text);
                            applyChanges = true;
                            OptionForm1.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Incorrect value!");
                        }
                    }
                    else if(b.KeyCode == Keys.Escape)
                    {
                        OptionForm1.Close();
                    }
            });

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            OptionForm1.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";

            if (applyChanges == true)
            {
                l[0] = Convert.ToDouble(RepTB.Text);
                l[1] = Convert.ToDouble(TTB.Text);
            }
            if (oldN == true | applyChanges == true)
            {
                n.Text = "Repeat T - " + l[0].ToString() +
                    " times (" + l[1].ToString() + " sec)";
                n.Tag = l;
            }
            else
            {
                tv.Nodes.Remove(n);
            }
        }
        private int AddTags(Panel Core, TifFileInfo fi)
        {
            #region Tags
            {
                //
                string TagName = "Image description";
                string Val = fi.FileDescription;
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Button btn = new Button();
                btn.Location = new Point(tag.ValLabel.Location.X, 5);
                btn.BackColor = SystemColors.ButtonFace;
                btn.ForeColor = Color.Black;
                btn.Text = "View";
                btn.Height = tag.Core.Height;

                tag.Resize(5);
                tag.Core.Controls.Add(btn);
                tag.ValLabel.Visible = false;

                btn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    Form OptionForm = new Form();
                    OptionForm.FormBorderStyle = FormBorderStyle.Sizable;
                    OptionForm.Text = "File description";
                    OptionForm.Icon = Properties.Resources.CT_done;
                    OptionForm.MinimizeBox = false;
                    OptionForm.MaximizeBox = false;
                    OptionForm.StartPosition = FormStartPosition.CenterScreen;
                    OptionForm.WindowState = FormWindowState.Normal;
                    OptionForm.BackColor = IA.FileBrowser.BackGround2Color1;
                    OptionForm.ForeColor = IA.FileBrowser.ShriftColor1;
                    OptionForm.Width = 600;
                    OptionForm.Height = 600;

                    RichTextBox rtb = new RichTextBox();
                    rtb.Dock = DockStyle.Fill;
                    rtb.Text = tag.ValLabel.Text;
                    rtb.BackColor = IA.FileBrowser.BackGround2Color1;
                    rtb.ForeColor = IA.FileBrowser.ShriftColor1;
                    rtb.ReadOnly = true;
                    OptionForm.Controls.Add(rtb);

                    // Linux change
                    IA.FileBrowser.StatusLabel.Text = "Dialog open";
                    OptionForm.ShowDialog();
                    IA.FileBrowser.StatusLabel.Text = "Ready";

                    rtb.Dispose();
                    OptionForm.Dispose();
                });
            }
            {
                //
                string TagName = "Micropoint (frame):";
                string Val = fi.Micropoint.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try
                        {
                            int val1 = Convert.ToInt32(tag.Value.Text);
                            fi.Micropoint = val1;
                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            {
                //
                string TagName = "Size Z (um):";
                string Val = fi.umZ.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try
                        {
                            double val1 = Convert.ToDouble(tag.Value.Text);
                            fi.umZ = val1;
                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            {
                //
                string TagName = "Size XY (um):";
                string Val = fi.umXY.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try {
                            double val1 = Convert.ToDouble(tag.Value.Text);
                            fi.umXY = val1;
                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            /*
            {
                //
                string TagName = "C:";
                string Val = fi.sizeC.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            */
            {
                //
                string TagName = "C:";
                string Val = fi.sizeC.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try
                        {
                            int val1 = Convert.ToInt32(tag.Value.Text);
                            if (val1 < 1) val1 = 1;
                            fi.sizeC = val1;

                            int histArr = fi.histogramArray[0].Length;
                            #region Segmentation variables
                            fi.LutList = new List<Color>();
                            fi.MaxBrightness = new int[fi.sizeC];
                            fi.MinBrightness = new int[fi.sizeC];
                            fi.histogramArray = new int[fi.sizeC][];
                            fi.SegmentationCBoxIndex = new int[fi.sizeC];
                            fi.SegmentationProtocol = new int[fi.sizeC];
                            fi.thresholdsCBoxIndex = new int[fi.sizeC];
                            fi.sumHistogramChecked = new bool[fi.sizeC];
                            fi.thresholdValues = new int[fi.sizeC][];
                            fi.thresholdColors = new Color[fi.sizeC][];
                            fi.RefThresholdColors = new Color[fi.sizeC][];
                            fi.thresholds = new int[fi.sizeC];
                            fi.SpotColor = new Color[fi.sizeC];
                            fi.RefSpotColor = new Color[fi.sizeC];
                            fi.SelectedSpotThresh = new int[fi.sizeC];
                            fi.SpotThresh = new int[fi.sizeC];
                            fi.typeSpotThresh = new int[fi.sizeC];
                            fi.SpotTailType = new string[fi.sizeC];
                            fi.spotSensitivity = new int[fi.sizeC];
                            fi.roiList = new List<ROI>[fi.sizeC];
                            fi.tracking_MaxSize = new int[fi.sizeC];
                            fi.tracking_MinSize = new int[fi.sizeC];
                            fi.tracking_Speed = new int[fi.sizeC];

                            for (int i = 0; i < fi.sizeC; i++)
                            {
                                fi.LutList.Add(Color.White);
                                fi.sumHistogramChecked[i] = false;
                                fi.thresholdValues[i] = new int[5];
                                fi.thresholdColors[i] = new Color[]
                                {Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent};
                                fi.RefThresholdColors[i] = new Color[]
                                {Color.Black,Color.Orange,Color.Green,Color.Blue,Color.Magenta};
                                fi.SpotColor[i] = Color.Red;
                                fi.RefSpotColor[i] = Color.Red;
                                fi.SpotTailType[i] = "<";
                                fi.spotSensitivity[i] = 100;
                                fi.tracking_MaxSize[i] = 10000;
                                fi.tracking_MinSize[i] = 5;
                                fi.tracking_Speed[i] = 5;
                            }
                            #endregion Segmentation variables
                            //Buttons
                            fi.tpTaskbar.AddColorBtn();
                            fi.tpTaskbar.VisualizeColorBtns();
                            //Image BandC values

                            IA.BandC.PrepareArray(fi);
                            int curC = fi.cValue;
                            for (int c = 0; c < fi.sizeC; c++)
                            {
                                fi.cValue = c;
                                IA.BandC.calculateHistogramArray(fi, true);
                            }
                            fi.cValue = curC;
                            IA.BandC.calculateHistogramArray(fi, true);
                            //Reload image
                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            /*
            {
                //
                string TagName = "T:";
                string Val = fi.sizeT.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            */
            {
                //
                string TagName = "T:";
                string Val = fi.sizeT.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try
                        {
                            int val1 = Convert.ToInt32(tag.Value.Text);
                            if (val1 < 1) val1 = 1;
                            fi.sizeT = val1;

                            if (fi.frame >= fi.sizeT) fi.frame = 0;
                            IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);

                            if (fi.sizeT == 1 && IA.TabPages.tTrackBar.Panel.Visible)
                            {
                                IA.TabPages.tTrackBar.Panel.Visible = false;
                            }
                            else if (!IA.TabPages.tTrackBar.Panel.Visible)
                            {
                                IA.TabPages.tTrackBar.Panel.Visible = true;
                            }
                            
                            if (fi.roiList != null)
                                foreach (List<ROI> l in fi.roiList)
                                    if (l != null)
                                        foreach (ROI r in l)
                                        {
                                            if (fi.sizeT < r.ToT) r.ToT = fi.sizeT;
                                            if (fi.sizeT < r.FromT) r.FromT = fi.sizeT;
                                        }

                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            /*
            {
                //
                string TagName = "Z:";
                string Val = fi.sizeZ.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            */
            {
                //
                string TagName = "Z:";
                string Val = fi.sizeZ.ToString();
                bool changable = true;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                if (changable == true)
                {
                    tag.Value.TextChanged += new EventHandler(delegate (object sender, EventArgs e)
                    {
                        // Add value to metadata in fi
                        try
                        {
                            int val1 = Convert.ToInt32(tag.Value.Text);

                            if (val1 < 1) val1 = 1;
                            fi.sizeZ = val1;

                            if (fi.zValue >= fi.sizeZ) fi.zValue = 0;
                            IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);

                            if (fi.sizeZ == 1 && IA.TabPages.zTrackBar.Panel.Visible)
                            {
                                IA.TabPages.zTrackBar.Panel.Visible = false;
                            }
                            else if (!IA.TabPages.zTrackBar.Panel.Visible)
                            {
                                IA.TabPages.zTrackBar.Panel.Visible = true;
                            }

                            if (fi.roiList != null)
                                foreach (List<ROI> l in fi.roiList)
                                    if (l != null)
                                        foreach (ROI r in l)
                                        {
                                            if (fi.sizeZ < r.ToZ) r.ToZ = fi.sizeZ;
                                            if (fi.sizeZ < r.FromZ) r.FromZ = fi.sizeZ;
                                        }

                            IA.ReloadImages();
                            IA.MarkAsNotSaved();
                        }
                        catch
                        {
                            MessageBox.Show("Value is incorrect!");
                        }
                    });
                }
            }
            
            {
                //
                string TagName = "Y:";
                string Val = fi.sizeY.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
           
            {
                //
                string TagName = "X:";
                string Val = fi.sizeX.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();
                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            
            {
                //
                string TagName = "Bits per pixel:";
                string Val = fi.bitsPerPixel.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            {
                //
                string TagName = "Images:";
                string Val = fi.imageCount.ToString();
                bool changable = false;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            {
                //
                string TagName = "File Directory:";
                string Val = fi.Dir;
                bool changable = false;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            {
                //
                string TagName = "File Name:";
                string Val = IA.TabPages.FileNameFromDir(fi.Dir);
                bool changable = false;

                MetadataItem tag = new MetadataItem();

                tag.Initialize(Core, TagName, Val, changable);
                Label l1 = tag.ValLabel;
                l1.Width = TextRenderer.MeasureText(l1.Text, l1.Font).Width;
            }
            #endregion Tags
            //take max wight
            int w = 0;

            return w;
        }
    }
    class MetadataItem
    {
        public Label Value = new Label();
        public Panel Core = new Panel();
        public Label ValLabel = new Label();

        private Label nameLabel = new Label();
        private TextBox tagTB = new TextBox();
        private Button ApplyBtn = new Button();
        private Button CancelBtn = new Button();
        private ToolTip TurnOnToolTip = new ToolTip();
        public void Resize(int h)
        {
            Core.Height = Core.Height + h;
            nameLabel.Location = new Point(nameLabel.Location.X, nameLabel.Location.Y + h);
            ValLabel.Location = new Point(ValLabel.Location.X, ValLabel.Location.Y + h);
        }
        public void Initialize(Panel MainPanel, string TagName, string Val, bool changable)
        {
            Core.Height = 20;
            Core.Dock = DockStyle.Top;
            MainPanel.Controls.Add(Core);

            nameLabel.Location = new Point(5, 5);
            nameLabel.Width = 100;
            nameLabel.Text = TagName;
            Core.Controls.Add(nameLabel);
            
            if (changable == true)
            {
                tagTB.Text = Val;
                tagTB.Location = new Point(110, 2);
                tagTB.Width = 100;
                tagTB.Tag = "Change " + TagName.Substring(0,TagName.Length - 1);
                tagTB.MouseHover += new EventHandler(Control_MouseOver);
                Core.Controls.Add(tagTB);
                tagTB.TextChanged += tagTB_TextChanged;
                tagTB.KeyDown += tagTB_KeyPress;
                tagTB.LostFocus += tagTB_FocusLost;

                ApplyBtn.FlatAppearance.BorderSize = 0;
                ApplyBtn.FlatStyle = FlatStyle.Flat;
                ApplyBtn.Width = tagTB.Height;
                ApplyBtn.Height = tagTB.Height;
                ApplyBtn.Location = new System.Drawing.Point(tagTB.Location.X + tagTB.Width + 2, 0);
                ApplyBtn.Visible = false;
                ApplyBtn.Text = "";
                ApplyBtn.TextImageRelation = TextImageRelation.Overlay;
                ApplyBtn.Image = Properties.Resources.the_blue_tick_th;
                Core.Controls.Add(ApplyBtn);
                ApplyBtn.Click += new EventHandler(Applybtn_Click);
                ApplyBtn.Tag = "Apply";
                ApplyBtn.MouseHover += new EventHandler(Control_MouseOver);

                CancelBtn.FlatAppearance.BorderSize = 0;
                CancelBtn.FlatStyle = FlatStyle.Flat;
                CancelBtn.Width = tagTB.Height;
                CancelBtn.Height = tagTB.Height;
                CancelBtn.Location = new System.Drawing.Point(ApplyBtn.Location.X + ApplyBtn.Width, 0);
                CancelBtn.Visible = false;
                CancelBtn.Text = "";
                CancelBtn.TextImageRelation = TextImageRelation.Overlay;
                CancelBtn.Image = Properties.Resources.CancelRed;
                CancelBtn.Click += new EventHandler(CancelBtn_Click);
                Core.Controls.Add(CancelBtn);
                CancelBtn.Tag = "Cancel";
                CancelBtn.MouseHover += new EventHandler(Control_MouseOver);

                Value.Text = Val;
            }
            else
            {
                ValLabel.Location = new Point(110, 5);
                ValLabel.Width = 140;
                ValLabel.Text = Val;
                ValLabel.Tag = Val;
                ValLabel.MouseHover += new EventHandler(Control_MouseOver);
                Core.Controls.Add(ValLabel);
            }
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        private void tagTB_TextChanged(object sender, EventArgs e)
        {
            if (tagTB.Focused == false) { return; }

            if (tagTB.Text != Value.Text)
            {
                if (ApplyBtn.Visible == false) { ApplyBtn.Visible = true; }
                if (CancelBtn.Visible == false) { CancelBtn.Visible = true; }
            }
            else
            {
                if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
                if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
            }
        }

        private void tagTB_FocusLost(object sender, EventArgs e)
        {
            if (ApplyBtn.Focused == true | CancelBtn.Focused == true) { return; }
            tagTB.Text = Value.Text;
            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            tagTB.Text = Value.Text;
            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void tagTB_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFromTextBox1();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void Applybtn_Click(object sender, EventArgs e)
        {
            ApplyFromTextBox1();
        }
        private void ApplyFromTextBox1()
        {
            double val;
            try
            {
                val = double.Parse(tagTB.Text);
            }
            catch
            {
                MessageBox.Show("Value is not number!");
                tagTB.Focus();
                return;
            }
            Value.Text = tagTB.Text;
           
            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        public int Width()
        {
            int w = CancelBtn.Location.X + CancelBtn.Width + 10;
            return w;
        }
    }
}
