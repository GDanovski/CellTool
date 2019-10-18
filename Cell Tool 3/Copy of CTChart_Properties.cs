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

namespace Cell_Tool_3
{
    class CTChart_Properties
    {
        //controls
        public ImageAnalyser IA;
        public Ncalc_Adapter Ncalc;
        private PropertiesPanel_Item PropPanel;
        public Panel panel;
        public ComboBox xAxisTB;
        public ComboBox yAxisTB;

        private Form OptionForm = new Form();
        private ComboBox nameCBox = new ComboBox();
        private TextBox fTBox = new TextBox();
        public string[] functions = null;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        public CTChart_Properties(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            this.IA = IA;
            Ncalc = new Ncalc_Adapter(IA);

            PropPanel = new PropertiesPanel_Item();
            PropPanel_Initialize(propertiesPanel, PropertiesBody);
            //GLControl event
            //IA.GLControl1.MouseDown += GLControl_MouseClick_tracking;
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        public void BackColor(Color color)
        {
            //format background color
            PropPanel.BackColor(color);
        }
        public void ForeColor(Color color)
        {
            //format fore color
            PropPanel.ForeColor(color);
        }
        public void TitleColor(Color color)
        {
            //format title color
            PropPanel.TitleColor(color);
        }
        private void PropPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            PropPanel.Initialize(propertiesPanel);
            PropPanel.Resizable = false;
            PropPanel.Name.Text = "Chart Properties";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;

            panel.Visible = false;

            BuildOptions();
        }
        private void BuildOptions()
        {
            Label xAxisLabel = new Label();
            xAxisLabel.Text = "X axis:";
            xAxisLabel.Location = new Point(5, 35);
            panel.Controls.Add(xAxisLabel);
            xAxisLabel.BringToFront();

            xAxisTB = new ComboBox();
            xAxisTB.Location = new Point(50, 32);
            xAxisTB.Width = 65;
            panel.Controls.Add(xAxisTB);
            xAxisTB.BringToFront();
            xAxisTB.DropDownStyle = ComboBoxStyle.DropDownList;
            xAxisTB.Items.Clear();
            xAxisTB.Items.Add(" T slice");
            xAxisTB.Items.Add(" T (sec.)");
            xAxisTB.Items.Add(" Z slice");
            xAxisTB.Items.Add(" T (min.)");
            xAxisTB.Items.Add(" T (hr.)");
            xAxisTB.SelectedIndex = 0;
            xAxisTB.AutoSize = false;
            xAxisTB.SelectedIndexChanged += xAxisTB_ChangeIndex;

            Label yAxisLabel = new Label();
            yAxisLabel.Text = "Y axis:";
            yAxisLabel.Location = new Point(5, 60);
            panel.Controls.Add(yAxisLabel);
            yAxisLabel.BringToFront();

            yAxisTB = new ComboBox();
            yAxisTB.Location = new Point(50, 57);
            yAxisTB.Width = 65;
            panel.Controls.Add(yAxisTB);
            yAxisTB.BringToFront();
            yAxisTB.Items.AddRange(new string[] { " Area", " Mean", " Min", " Max", "Total" });
            yAxisTB.DropDownStyle = ComboBoxStyle.DropDownList;
            yAxisTB.SelectedIndex = 1;
            yAxisTB.AutoSize = false;
            yAxisTB.SelectedIndexChanged += yAxisTB_ChangeIndex;

            //function btn
            Button btn = new Button();
            btn.Width = 21;
            btn.Height = 21;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = IA.FileBrowser.BackGround2Color1;
            btn.ForeColor = IA.FileBrowser.ShriftColor1;
            btn.Image = new Bitmap(Properties.Resources.settings, new Size(18, 18));
            btn.FlatAppearance.BorderSize = 0;
            btn.Text = "";
            PropPanel.Panel.Controls.Add(btn);
            btn.BringToFront();
            btn.Location = new Point(120, 57);
            btn.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
            {
                TurnOnToolTip.SetToolTip(btn, "Function editor");
            });
            btn.Click += optionForm_Show;

            BuildDialog();
        }
        #region Function Editor
        public void BuildDialog()
        {
            OptionForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            OptionForm.Text = "Function editor";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.BackColor = IA.FileBrowser.BackGround2Color1;
            OptionForm.ForeColor = IA.FileBrowser.ShriftColor1;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;
            OptionForm.Width = 520;
            OptionForm.Height = 500;
            OptionForm.MinimumSize = new Size(500, 100);
            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
            {
                switch (a.KeyCode)
                {
                    case Keys.Escape:
                        OptionForm.Hide();
                        break;
                }
            });

            OptionForm.Hide();

            Panel OptionPanel = new Panel();
            OptionPanel.Dock = DockStyle.Top;
            OptionPanel.Height = 70;
            OptionForm.Controls.Add(OptionPanel);

            Label NameLabel = new Label();
            NameLabel.Text = "Name:";
            NameLabel.Width = 40;
            NameLabel.Location = new Point(5, 10);
            OptionPanel.Controls.Add(NameLabel);

            nameCBox.Width = 349;
            nameCBox.Location = new Point(45, 8);
            nameCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            nameCBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            OptionPanel.Controls.Add(nameCBox);
            nameCBox.SelectedIndexChanged += nameCBox_IndexChanged;

            Button addBtn = new Button();
            {
                //function btn
                Button btn = addBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.plus, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                OptionPanel.Controls.Add(btn);
                btn.BringToFront();
                btn.Location = new Point(392, 8);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Add new function");
                });
                btn.Click += AddBtn_Click;
            }

            Button renameBtn = new Button();
            {
                //function btn
                Button btn = renameBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.Rename_icon, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                OptionPanel.Controls.Add(btn);
                btn.BringToFront();
                btn.Location = new Point(416, 8);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Rename function");
                });
                btn.Click += EditBtn_Click;
            }

            Button deleteBtn = new Button();
            {
                //function btn
                Button btn = deleteBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.DeleteRed, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                OptionPanel.Controls.Add(btn);
                btn.BringToFront();

                btn.Location = new Point(437, 8);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Delete function");
                });
                btn.Click += DeleteBtn_Click;
            }

            Button saveBtn = new Button();
            {
                //function btn
                Button btn = saveBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.Save, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";

                btn.Location = new Point(458, 8);
                btn.BringToFront();
                OptionPanel.Controls.Add(btn);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Save function");
                });
                btn.Click += SaveBtn_Click;
            }

            Button errorBtn = new Button();
            {
                //function btn
                Button btn = errorBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.settings, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                btn.Location = new Point(479, 8);
                btn.BringToFront();
                OptionPanel.Controls.Add(btn);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Check for errors");
                });
                btn.Click += ErrorCheck;
            }

            Label fLabel = new Label();
            fLabel.Text = "f(x) = ";
            fLabel.Width = 40;
            fLabel.Location = new Point(5, 40);
            OptionPanel.Controls.Add(fLabel);

            fTBox.Width = 450;
            fTBox.Location = new Point(45, 38);
            fTBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            OptionPanel.Controls.Add(fTBox);

            RichTextBox rtb = new RichTextBox();
            rtb.BackColor = IA.FileBrowser.BackGround2Color1;
            rtb.ForeColor = Color.LightGreen;
            rtb.Text = Properties.Resources.NcalcOperators;
            rtb.Dock = DockStyle.Fill;
            OptionForm.Controls.Add(rtb);
            rtb.BringToFront();
            rtb.ReadOnly = true;

            string[] strList = new string[]
            {
                "Abs(-1)",
                "Sin(0)",
"Cos(0)",
"Tan(0)",
"Acos(1)",
"Asin(0)",
"Atan(0)",
"Exp(0)",
"Log(1, 10)",
"Log10(1)",
"Pow(3, 2)",
"Sqrt(4)",
"Mean",
"Area",
"Max",
"Min",
"Data sources:",
            "Functions:"};

            foreach (string str in strList)
            {
                rtb.Find(str);
                rtb.SelectionColor = IA.FileBrowser.ShriftColor1;
            }

            OptionForm.FormClosing += new FormClosingEventHandler(delegate (object o, FormClosingEventArgs a)
            {
                optionForm_Hide();
                a.Cancel = true;
            });
        }
        private void ErrorCheck(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null)
            {
                MessageBox.Show("There is no opened image!");
                return;
            }

            if (fi.loaded == false) return;
            if (fi.roiList == null) return;
            if (nameCBox.Items.Count == 0) return;
            try
            {
                int c, ind;
                ROI roi;
                List<string> res = new List<string>();

                IA.chart.Properties.Ncalc.LoadFunction(fi, nameCBox.SelectedIndex);

                for (c = 0; c < fi.sizeC; c++)
                    if (fi.tpTaskbar.ColorBtnList[c].ImageIndex == 0 && fi.roiList[c] != null)
                    {

                        for (ind = 0; ind < fi.roiList[c].Count; ind++)
                        {
                            roi = fi.roiList[c][ind];
                            if (roi.Results == null || roi.Checked == false) continue;

                            if (roi.ChartUseIndex[0] == true)
                            {
                                string val = IA.chart.Properties.Ncalc.ErrorCheck(c, roi, fi.roiList[c], fi);
                                if (val != "") res.Add(val);
                            }
                        }
                    }
                res.Add("Check finished!");
                MessageBox.Show(string.Join("\n", res));
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void AddBtn_Click(object sender, EventArgs e)
        {
            Form dialog = new Form();

            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.Text = "Add new function";
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.WindowState = FormWindowState.Normal;
            dialog.BackColor = IA.FileBrowser.BackGround2Color1;
            dialog.ForeColor = IA.FileBrowser.ShriftColor1;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.Width = 200;
            dialog.Height = 110;

            Label lab = new Label();
            lab.Text = "Name:";
            lab.Location = new Point(5, 10);
            lab.Width = 45;
            dialog.Controls.Add(lab);

            TextBox tBox = new TextBox();
            tBox.Location = new Point(50, 8);
            tBox.Width = 130;
            dialog.Controls.Add(tBox);

            Button saveBtn = new Button();
            saveBtn.Text = "Save";
            saveBtn.Location = new Point(55, 40);
            saveBtn.BackColor = SystemColors.ButtonFace;
            saveBtn.ForeColor = Color.Black;
            dialog.Controls.Add(saveBtn);

            saveBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                if (tBox.Text.IndexOf("=") > -1 || fTBox.Text.IndexOf("=") > -1)
                {
                    MessageBox.Show("Incorect characters!");
                    return;
                }

                List<string> lStr = functions.ToList();
                lStr.Add(tBox.Text + "=" + fTBox.Text + ";");

                functions = lStr.ToArray();

                nameCBox.Items.Clear();
                foreach (string str in functions)
                    if (functions[0].IndexOf("=") > -1)
                    {
                        string str1 = str.Substring(0, str.IndexOf("="));
                        nameCBox.Items.Add(str1);
                    }

                int ind = nameCBox.Items.Count - 1;

                if (functions.Length > 0)
                {
                    nameCBox.SelectedIndex = ind;
                    fTBox.Text = functions[ind].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
                }
                else
                    fTBox.Text = "";

                dialog.Close();
            });

            dialog.KeyPreview = true;
            dialog.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
            {
                switch (a.KeyCode)
                {
                    case Keys.Enter:
                        saveBtn.PerformClick();
                        break;
                    case Keys.Escape:
                        dialog.Close();
                        break;
                }
            });

            // Unix change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            dialog.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        public string GetFunction(int ind)
        {
            if (ind >= 5)
                return functions[ind - 5];
            else
                return "";

        }
        private string CheckForFunct(string input)
        {
            string[] vals = input.Split(new string[] { "=" }, StringSplitOptions.None);

            if (vals.Length == 2)
                for (int i = 0; i < functions.Length; i++)
                    if (vals[1] == functions[i].Split(new string[] { "=" }, StringSplitOptions.None)[1])
                    {
                        return functions[i];
                    }
            
            return input;
        }
        public int GetFunctionIndex(string input)
        {
            if (input == "")
                return -1;

            input = CheckForFunct(input);
            
            if (!functions.Contains(input))
            {
                List<string> lStr = functions.ToList();
                lStr.Add(input);

                functions = lStr.ToArray();

                nameCBox.Items.Clear();
                foreach (string str in functions)
                    if (functions[0].IndexOf("=") > -1)
                    {
                        string str1 = str.Substring(0, str.IndexOf("="));
                        nameCBox.Items.Add(str1);
                    }

                int ind = nameCBox.Items.Count - 1;

                if (functions.Length > 0)
                {
                    nameCBox.SelectedIndex = ind;
                    fTBox.Text = functions[ind].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
                }
                else
                    fTBox.Text = "";

                if (functions.Length > 0)
                    Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex] = "@\n" +
                        string.Join("\n", functions);
                else
                    Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex] = "@";

                Properties.Settings.Default.Save();

                LoadFunctions();
            }
            
            return functions.ToList().IndexOf(input)+5;
        }
        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (nameCBox.SelectedIndex > functions.Length) return;

            List<string> l = functions.ToList();
            l.RemoveAt(nameCBox.SelectedIndex);
            functions = l.ToArray();

            int ind = nameCBox.SelectedIndex;

            nameCBox.Items.Clear();

            foreach (string str in functions)
            {
                string str1 = str.Substring(0, str.IndexOf("="));
                nameCBox.Items.Add(str1);
            }

            if (ind < nameCBox.Items.Count)
                nameCBox.SelectedIndex = ind;
            else
                ind = nameCBox.Items.Count - 1;

            if (functions.Length > 0 && functions[ind].IndexOf("=") > -1)
            {
                nameCBox.SelectedIndex = ind;
                fTBox.Text = functions[ind].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
            }
            else
                fTBox.Text = "";
        }
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (fTBox.Text.IndexOf("=") > -1)
            {
                MessageBox.Show("Incorect characters!");
                return;
            }

            if (nameCBox.SelectedIndex<=functions.Length)
                functions[nameCBox.SelectedIndex] = nameCBox.Text + "=" + fTBox.Text + ";";
        }
        private void EditBtn_Click(object sender, EventArgs e)
        {
            Form dialog = new Form();

            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.Text = "Rename function";
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.WindowState = FormWindowState.Normal;
            dialog.BackColor = IA.FileBrowser.BackGround2Color1;
            dialog.ForeColor = IA.FileBrowser.ShriftColor1;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.Width = 200;
            dialog.Height = 110;

            Label lab = new Label();
            lab.Text = "Name:";
            lab.Location = new Point(5, 10);
            lab.Width = 45;
            dialog.Controls.Add(lab);

            TextBox tBox = new TextBox();
            tBox.Location = new Point(50, 8);
            tBox.Width = 130;
            tBox.Text = nameCBox.Text;
            dialog.Controls.Add(tBox);

            Button saveBtn = new Button();
            saveBtn.Text = "Rename";
            saveBtn.Location = new Point(55, 40);
            saveBtn.BackColor = SystemColors.ButtonFace;
            saveBtn.ForeColor = Color.Black;
            dialog.Controls.Add(saveBtn);

            saveBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                if (functions.Length < 1)
                {
                    dialog.Close();
                    return;
                }
                if (tBox.Text.IndexOf("=") > -1 || fTBox.Text.IndexOf("=") > -1)
                {
                    MessageBox.Show("Incorect characters!");
                    return;
                }

                functions[nameCBox.SelectedIndex] = tBox.Text + "=" + fTBox.Text + ";";

                nameCBox.Items.Clear();
                foreach (string str in functions)
                    if (functions[0].IndexOf("=") > -1)
                    {
                        string str1 = str.Substring(0, str.IndexOf("="));
                        nameCBox.Items.Add(str1);
                    }

                int ind = nameCBox.Items.Count - 1;

                if (functions.Length > 0)
                {
                    nameCBox.SelectedIndex = ind;
                    fTBox.Text = functions[ind].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
                }
                else
                    fTBox.Text = "";

                dialog.Close();
            });

            dialog.KeyPreview = true;
            dialog.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
            {
                switch (a.KeyCode)
                {
                    case Keys.Enter:
                        saveBtn.PerformClick();
                        break;
                    case Keys.Escape:
                        dialog.Close();
                        break;
                }
            });

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            dialog.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private void nameCBox_IndexChanged(object sender, EventArgs e)
        {
            if (functions.Length > nameCBox.SelectedIndex && 
                functions[nameCBox.SelectedIndex].IndexOf("=") > -1)
                fTBox.Text = functions[nameCBox.SelectedIndex].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
            else
                fTBox.Text = "";
        }
        private void optionForm_Show(object sender, EventArgs e)
        {
            string[] strArr = Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex].Split(new string[] { "\n" }, StringSplitOptions.None);
            functions = new string[strArr.Length - 1];
            Array.Copy(strArr, 1, functions, 0, functions.Length);

            nameCBox.Items.Clear();
            foreach (string str in functions)
                if(functions[0].IndexOf("=") > -1)
            {
                string str1 = str.Substring(0, str.IndexOf("="));
                nameCBox.Items.Add(str1);
            }

            int ind = 0;

            if (yAxisTB.SelectedIndex - 5 > 0) ind = yAxisTB.SelectedIndex - 5;

            if (functions.Length > 0)
            {
                nameCBox.SelectedIndex = ind;
                fTBox.Text = functions[ind].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace(";", "");
            }
            else
                fTBox.Text = "";

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private void optionForm_Hide()
        {
            if (functions.Length > 0)
                Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex] = "@\n" +
                    string.Join("\n", functions);
            else
                Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex] = "@";

            Properties.Settings.Default.Save();

            OptionForm.Hide();
            LoadFunctions();
            IA.ReloadImages();
        }
        public void LoadFunctions()
        {
            string[] strArr = Properties.Settings.Default.CTChart_Functions[IA.TabPages.ActiveAccountIndex].Split(new string[] { "\n" }, StringSplitOptions.None);
            functions = new string[strArr.Length - 1];
            Array.Copy(strArr, 1, functions, 0, functions.Length);

            int ind = yAxisTB.SelectedIndex;

            nameCBox.Items.Clear();
            yAxisTB.Items.Clear();

            yAxisTB.Items.AddRange(new string[] { " Area", " Mean", " Min", " Max", " Total" });
            foreach (string str in functions)
            {
                string str1 = str.Substring(0, str.IndexOf("="));
                yAxisTB.Items.Add(" " + str1);
                nameCBox.Items.Add(" " + str1);
            }

            if (ind < yAxisTB.Items.Count)
                yAxisTB.SelectedIndex = ind;
            else
                yAxisTB.SelectedIndex = 0;


        }
        #endregion Function Editor
        private void xAxisTB_ChangeIndex(object sender, EventArgs e)
        {           
            if (xAxisTB.Focused == false) return;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (fi.xAxisTB == xAxisTB.SelectedIndex) return;
            //Parse value
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            AddXAxisTBToHistory(fi);
            
            fi.xAxisTB = xAxisTB.SelectedIndex;

            AddXAxisTBToHistory(fi);
            //ReloadImage
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        private void yAxisTB_ChangeIndex(object sender, EventArgs e)
        {
            if (yAxisTB.Focused == false) return;
            //find selected image
            TifFileInfo fi = null; 
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (fi.yAxisTB == yAxisTB.SelectedIndex) return;

            //Parse value
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            AddYAxisTBToHistory(fi);

            fi.yAxisTB = yAxisTB.SelectedIndex;

            AddYAxisTBToHistory(fi);
            //ReloadImage
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        public void LoadFI(TifFileInfo fi)
        {

            if (xAxisTB.SelectedIndex != fi.xAxisTB)
            {
                xAxisTB.SelectedIndex = fi.xAxisTB;
                xAxisTB.Text = xAxisTB.Items[fi.xAxisTB].ToString();
            }

            if (yAxisTB.SelectedIndex != fi.yAxisTB)
            {
                if (yAxisTB.Items.Count <= fi.yAxisTB)
                    fi.yAxisTB = yAxisTB.Items.Count - 1;

                yAxisTB.SelectedIndex = fi.yAxisTB;
                yAxisTB.Text = yAxisTB.Items[fi.yAxisTB].ToString();
            }

            
        }
        public void AddXAxisTBToHistory(TifFileInfo fi)
        {
            fi.History.Add("Chart.XAxisType(" + fi.xAxisTB.ToString() + ")");
        }
        public void AddYAxisTBToHistory(TifFileInfo fi)
        {
            fi.History.Add("Chart.YAxisType(" + fi.yAxisTB.ToString() + ")");
        }
    }
}
