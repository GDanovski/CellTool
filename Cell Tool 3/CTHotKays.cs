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
    class CTHotKeys
    {
        private Interface CTInterface;
        private Dictionary<string, MemoryUnit> Memory;
        private Dictionary<string, MemoryUnit> AutoSettingsMemory;
        private Dictionary<string, MemoryUnit> KeyMemory;
        private Form DialogForm;
        public CTHotKeys(Interface CTInterface)
        {
            this.CTInterface = CTInterface;
            this.Memory = new Dictionary<string, MemoryUnit>();
            this.AutoSettingsMemory = new Dictionary<string, MemoryUnit>();
            this.KeyMemory = new Dictionary<string, MemoryUnit>();
            this.DialogForm = new Form();
            this.DialogForm.FormBorderStyle = FormBorderStyle.Sizable;
            this.DialogForm.Text = "Hot Keys";
            this.DialogForm.StartPosition = FormStartPosition.CenterScreen;
            this.DialogForm.WindowState = FormWindowState.Normal;
            this.DialogForm.MinimizeBox = false;
            this.DialogForm.MaximizeBox = false;
            this.DialogForm.Width = 400;
            this.DialogForm.Height = 400;
            this.DialogForm.MaximumSize = new Size(400, 10000);
            this.DialogForm.MinimumSize = new System.Drawing.Size(400, 400);
            this.DialogForm.BackColor = CTInterface.FileBrowser.BackGroundColor1;
            this.DialogForm.ForeColor = CTInterface.FileBrowser.ShriftColor1;
            this.DialogForm.FormClosing += new FormClosingEventHandler(
                delegate (object o, FormClosingEventArgs a)
                {
                    DialogForm.Hide();
                    a.Cancel = true;
                });

            //add save and cancel btns
            Panel p1 = new Panel();
            p1.Dock = DockStyle.Bottom;
            p1.Height = 50;
            DialogForm.Controls.Add(p1);

            Button save = new Button();
            save.BackColor = SystemColors.ButtonFace;
            save.ForeColor = Color.Black;
            save.Text = "Save";
            save.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            save.Location = new Point(DialogForm.Width / 2 - save.Width - 5, 10);
            p1.Controls.Add(save);
            save.Click += Save_Click;

            Button Cancel = new Button();
            Cancel.BackColor = SystemColors.ButtonFace;
            Cancel.ForeColor = Color.Black;
            Cancel.Text = "Cancel";
            Cancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Cancel.Location = new Point(DialogForm.Width / 2 + 5, 10);
            p1.Controls.Add(Cancel);
            Cancel.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                DialogForm.Hide();
            });


            //Add Titles
            Panel p2 = new Panel();
            p2.Dock = DockStyle.Top;
            p2.Height = 30;
            DialogForm.Controls.Add(p2);

            Label l1 = new Label();
            l1.Text = "Operation:";
            l1.Location = new Point(5, 5);
            p2.Controls.Add(l1);

            Label l2 = new Label();
            l2.Text = "Hot Key:";
            l2.Location = new Point(280, 5);
            p2.Controls.Add(l2);

            //Load memory controls
            Panel p3 = new Panel();
            p3.Dock = DockStyle.Fill;
            p3.BackColor = CTInterface.FileBrowser.BackGround2Color1;
            p3.AutoScroll = true;
            DialogForm.Controls.Add(p3);
            p3.BringToFront();
            DialogForm.Tag = p3;

            //Get the names of the controls
            string[] strArr = GetControlNames();

            //Load to memory
            foreach (string str in strArr)
            {
                //create memory unit
                MemoryUnit mu = new MemoryUnit(str);
                //set associated control
                SetAssociatedControl(mu);
                if (mu.GetAssociatedControl == null) continue;
                //add to memory
                Memory.Add(str, mu);
            }
            //Load plugins
            foreach (Object o in CTInterface.DeveloperToolStripMenuItem.DropDownItems)
                if (o is ToolStripMenuItem)
                {
                    //create memory unit
                    MemoryUnit mu = new MemoryUnit(((ToolStripMenuItem)o).Text);
                    //set associated control
                    mu.SetAssociatedControl = o;
                    if (mu.GetAssociatedControl == null) continue;
                    //add to memory
                    Memory.Add(((ToolStripMenuItem)o).Text, mu);
                }
            //Clear data
            strArr = null;

        }
        private void BuildForm()
        {
            Panel p = (Panel)DialogForm.Tag;
            p.SuspendLayout();
            p.Controls.Clear();

            for (int i = Memory.Count - 1; i >= 0; i--)
            {
                Memory.ElementAt(i).Value.RefreshKey();
                p.Controls.Add(Memory.ElementAt(i).Value.GetPanel);
            }

            #region  Load auto settings macros
            foreach (var val in AutoSettingsMemory)
            {
                val.Value.GetPanel.Dispose();
            }
            AutoSettingsMemory.Clear();

            for (int i = 1; i < CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items.Count; i++)
            {
                string str = CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items[i].ToString();
                //create memory unit
                MemoryUnit mu = new MemoryUnit(str);
                //set associated control
                {
                    Button btn = new Button();
                    btn.Tag = str;
                    btn.Click += AutoSetUpBtn_Click;
                    mu.SetAssociatedControl = btn;
                }

                if (mu.GetAssociatedControl == null) continue;
                //add to memory
                if (!AutoSettingsMemory.ContainsKey(str))
                {
                    AutoSettingsMemory.Add(str, mu);
                    p.Controls.Add(mu.GetPanel);
                    mu.GetPanel.BringToFront();
                }
            }

            foreach (var val in AutoSettingsMemory)
                foreach (var ref1 in KeyMemory)
                    if (ref1.Value.GetName == val.Value.GetName)
                    {
                        val.Value.SetKey = ref1.Value.GetKey;
                        ref1.Value.SetAssociatedControl = val.Value.GetAssociatedControl;
                        break;
                    }

            #endregion  Load auto settings macros
            p.ResumeLayout();
        }
        public void AutoSetUpBtn_Click(object sender, EventArgs e)
        {
            if (!CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items.Contains((string)((Button)sender).Tag)) return;

            int ind = CTInterface.IA.Segmentation.AutoSetUp.LibTB.Items.IndexOf((string)((Button)sender).Tag);
            CTInterface.IA.Segmentation.AutoSetUp.LibTB.Focus();
            if (CTInterface.IA.Segmentation.AutoSetUp.LibTB.SelectedIndex != ind)
            {
                CTInterface.IA.Segmentation.AutoSetUp.LibTB.SelectedIndex = ind;
            }
            else
            {
                CTInterface.IA.Segmentation.AutoBtn.PerformClick();
            }
        }
        private void Save_Click(object sender, EventArgs e)
        {
            //DialogForm.Hide();
            Dictionary<string, MemoryUnit> newKeys = new Dictionary<string, MemoryUnit>();
            string[] forbKeys = GetForbidenKeys();
            //check for duplicated keys
            foreach (var val in Memory)
                if (forbKeys.Contains(val.Value.GetKey) || newKeys.ContainsKey(val.Value.GetKey))
                {
                    MessageBox.Show("Key \"" + val.Value.GetKey + "\" is binded to more then one operations!");
                    newKeys = null;
                    return;
                }
                else if (val.Value.GetKey != "")
                {
                    newKeys.Add(val.Value.GetKey, val.Value);
                }

            #region  Load auto settings macros
            //check for duplicated keys
            foreach (var val in AutoSettingsMemory)
                if (newKeys.ContainsKey(val.Value.GetKey))
                {
                    MessageBox.Show("Key \"" + val.Value.GetKey + "\" is binded to more then one operations!");
                    newKeys = null;
                    return;
                }
                else if (val.Value.GetKey != "")
                {
                    newKeys.Add(val.Value.GetKey, val.Value);
                }
            #endregion  Load auto settings macros

            //apply changes
            foreach (var val in Memory)
            {
                val.Value.ApplyKey();
            }

            this.KeyMemory = newKeys;
            //save to profile settings
            string str = "@";
            foreach (var val in this.KeyMemory)
            {
                str += "\t" + val.Value.GetName + "#" + val.Value.GetKey;
            }

            Properties.Settings.Default.HotKeys[
                CTInterface.FileBrowser.ActiveAccountIndex] =
                str;

            Properties.Settings.Default.Save();
        }
        private string[] GetForbidenKeys()
        {
            return new string[]
            {
                Keys.O.ToString(),
                Keys.S.ToString(),
                Keys.A.ToString(),
                Keys.E.ToString(),
                Keys.Z.ToString(),
                Keys.Y.ToString(),
                Keys.OemMinus.ToString(),
                Keys.OemMinus.ToString(),
                Keys.Add.ToString(),
                Keys.Subtract.ToString(),
                Keys.T.ToString(),
                Keys.D.ToString(),
                Keys.M.ToString(),
                Keys.C.ToString(),
                Keys.X.ToString(),
                Keys.V.ToString(),
                Keys.F2.ToString(),
                Keys.F5.ToString(),
                Keys.N.ToString()
            };
        }
        public void LoadAccountSettings()
        {
            //Properties.Settings.Default.HotKeys[CTInterface.FileBrowser.ActiveAccountIndex] = "@";
            //Properties.Settings.Default.Save();
            //Get assigned hot keys
            KeyMemory.Clear();
            Dictionary<string, string> OldKeys = new Dictionary<string, string>();
            //read settings
            string[] propArr = Properties.Settings.Default.
                HotKeys[CTInterface.FileBrowser.ActiveAccountIndex].Split('\t');
            foreach (string str in propArr)
                if (str != "@")
                {
                    string[] strArr = str.Split('#');
                    try
                    {
                        if (!OldKeys.ContainsKey(strArr[0]) && !GetForbidenKeys().Contains(strArr[0]))
                            OldKeys.Add(strArr[0], strArr[1]);
                    }
                    catch { }
                    strArr = null;
                }
            propArr = null;
            //OldKeys.Add("Export processed image","K");

            foreach (var val in Memory)
            {
                //Add hot key
                if (OldKeys.ContainsKey(val.Key))
                    val.Value.SetKey = OldKeys[val.Key];
                else
                    val.Value.SetKey = "";
            }
            //add to hot key enjine
            foreach (var val in OldKeys)
                if (Memory.ContainsKey(val.Key))
                {
                    KeyMemory.Add(val.Value, Memory[val.Key]);
                }

            BuildForm();

            foreach (var val in AutoSettingsMemory)
            {
                //Add hot key
                if (OldKeys.ContainsKey(val.Key))
                    val.Value.SetKey = OldKeys[val.Key];
                else
                    val.Value.SetKey = "";
            }

            foreach (var val in OldKeys)
                if (AutoSettingsMemory.ContainsKey(val.Key))
                {
                    KeyMemory.Add(val.Value, AutoSettingsMemory[val.Key]);
                }
            //clear
            OldKeys = null;
        }
        public bool CheckForKey(Keys k)
        {
            if (k == Keys.ControlKey ||
                KeyMemory == null || KeyMemory.Count == 0 ||
                Memory == null || Memory.Count == 0) return false;

            if (KeyMemory.ContainsKey(k.ToString()))
            {
                KeyMemory[k.ToString()].ActivateAssociatedControl();
                return true;
            }

            return false;
        }
        public string[] GetControlNames()
        {
            return new string[]
           {
"Auto Export",
"Close Image",
"Close All Images",
"To 8 bit",
"To 16 bit",
"Projection",
"Crop",
"Substack",
"Export processed image",
"Merge channels",
"Extract channel",
"Delete channel",
"Merge Z planes",
"Extract Z plane",
"Delete Z plane",
"Merge T slices",
"Extract T slice",
"Delete T slice",
"Static ROI",
"Tracking ROI",
"Stack ROI",
"Rectangular ROI",
"Oval ROI",
"Polygonal ROI",
"Freehand ROI",
"Magic wand ROI",
"Add work directory",
"New folder",
"Properties",
"Show/Hide Raw Image",
"Show/Hide Processed Image",
"Show/Hide Results Chart",
"Auto processing",
"Auto bounds",
"Apply to all channels",
"View metadata",
"Export ROI set",
"Load ROI set",
"Concatenate ROIs",
"Export Color",
"ROI Hide Labels",
"ROI Auto find",
"ROI Max Size",
"ROI Max Hight",
"ROI Max Width",
"Reset",
"To Binary",
"3x3 Box blur",
"5x5 Box blur",
"3x3 Gaussian blur",
"5x5 Gaussian blur",
"3x3 Median",
"5x5 Median",
"3x3 Sharpen",
"5x5 Sharpen",
"5x5 Unsharp masking",
"3x3 Edge detection",
"3x3 Gradient detection",
"3x3 Sobel operator",
"3x3 Embos",
"5x5 Embos",
"Erode",
"Dilate",
"Open Filter",
"Close Filter",
"Fill Holes",
"Watershed",
"Clear segmentation",
"Otsu thresholding",
"K-Means",
"Sum Histogram",
"0 Threshold",
"1 Threshold",
"2 Threshold",
"3 Threshold",
"4 Threshold"
           };
        }
        public void SetAssociatedControl(MemoryUnit mu)
        {
            switch (mu.GetName)
            {
                case "Auto Export":
                    mu.SetAssociatedControl = CTInterface.ExportAllBtn;
                    break;
                case "Close Image":
                    mu.SetAssociatedControl = CTInterface.CloseToolStripMenuItem;
                    break;
                case "Close All Images":
                    mu.SetAssociatedControl = CTInterface.CloseAllToolStripMenuItem;
                    break;
                case "To 8 bit":
                    mu.SetAssociatedControl = CTInterface.editMenu1.Convert8bitToolStripMenuItem;
                    break;
                case "To 16 bit":
                    mu.SetAssociatedControl = CTInterface.editMenu1.Convert16bitToolStripMenuItem;
                    break;
                case "Projection":
                    mu.SetAssociatedControl = CTInterface.editMenu1.ProjectionToolStripMenuItem;
                    break;
                case "Crop":
                    mu.SetAssociatedControl = CTInterface.editMenu1.CropToolStripMenuItem;
                    break;
                case "Substack":
                    mu.SetAssociatedControl = CTInterface.editMenu1.SubstackToolStripMenuItem;
                    break;
                case "Export processed image":
                    mu.SetAssociatedControl = CTInterface.editMenu1.ProcessedAsRawMenuItem;
                    break;
                case "Merge channels":
                    mu.SetAssociatedControl = CTInterface.editMenu1.AddCToolStripMenuItem;
                    break;
                case "Extract channel":
                    mu.SetAssociatedControl = CTInterface.editMenu1.SplitCToolStripMenuItem;
                    break;
                case "Delete channel":
                    mu.SetAssociatedControl = CTInterface.editMenu1.DeleteCToolStripMenuItem;
                    break;
                case "Merge Z planes":
                    mu.SetAssociatedControl = CTInterface.editMenu1.AddZToolStripMenuItem;
                    break;
                case "Extract Z plane":
                    mu.SetAssociatedControl = CTInterface.editMenu1.SplitZToolStripMenuItem;
                    break;
                case "Delete Z plane":
                    mu.SetAssociatedControl = CTInterface.editMenu1.DeleteZToolStripMenuItem;
                    break;
                case "Merge T slices":
                    mu.SetAssociatedControl = CTInterface.editMenu1.AddTToolStripMenuItem;
                    break;
                case "Extract T slice":
                    mu.SetAssociatedControl = CTInterface.editMenu1.SplitTToolStripMenuItem;
                    break;
                case "Delete T slice":
                    mu.SetAssociatedControl = CTInterface.editMenu1.DeleteTToolStripMenuItem;
                    break;
                case "Static ROI":
                    mu.SetAssociatedControl = CTInterface.StaticBtn;
                    break;
                case "Tracking ROI":
                    mu.SetAssociatedControl = CTInterface.TrackingBtn;
                    break;
                case "Stack ROI":
                    mu.SetAssociatedControl = CTInterface.DoubleRoiBtn;
                    break;
                case "Rectangular ROI":
                    mu.SetAssociatedControl = CTInterface.RectangularBtn;
                    break;
                case "Oval ROI":
                    mu.SetAssociatedControl = CTInterface.OvalBtn;
                    break;
                case "Polygonal ROI":
                    mu.SetAssociatedControl = CTInterface.PolygonBtn;
                    break;
                case "Freehand ROI":
                    mu.SetAssociatedControl = CTInterface.FreehandBtn;
                    break;
                case "Magic wand ROI":
                    mu.SetAssociatedControl = CTInterface.MagicWandBtn;
                    break;
                case "Add work directory":
                    mu.SetAssociatedControl = CTInterface.FileBrowser.AddBtn;
                    break;
                case "New folder":
                    mu.SetAssociatedControl = CTInterface.FileBrowser.NewBtn;
                    break;
                case "Properties":
                    mu.SetAssociatedControl = CTInterface.FileBrowser.PropertiesBtn;
                    break;
                case "Show/Hide Raw Image":
                    {
                        Button btn = new Button();
                        btn.Tag = 0;
                        btn.Click += ShowGLControlImagesBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "Show/Hide Processed Image":
                    {
                        Button btn = new Button();
                        btn.Tag = 1;
                        btn.Click += ShowGLControlImagesBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "Show/Hide Results Chart":
                    {
                        Button btn = new Button();
                        btn.Tag = 2;
                        btn.Click += ShowGLControlImagesBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "Auto processing":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.AutoBtn;
                    break;
                case "Auto bounds":
                    mu.SetAssociatedControl = CTInterface.IA.BandC.autoDetect;
                    break;
                case "Apply to all channels":
                    mu.SetAssociatedControl = CTInterface.IA.BandC.applyToAll;
                    break;
                case "View metadata":
                    mu.SetAssociatedControl = CTInterface.IA.Meta.MetaBtn;
                    break;
                case "Export ROI set":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.ExportBtn;
                    break;
                case "Load ROI set":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.LoadBtn;
                    break;
                case "Concatenate ROIs":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.ConcatenateMI;
                    break;
                case "Export Color":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.FractureMeasureMI;
                    break;
                case "ROI Hide Labels":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.ShowLabelsMI;
                    break;
                case "ROI Auto find":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.AutoRoisMI;
                    break;
                case "ROI Max Size":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.BiggestMI;
                    break;
                case "ROI Max Hight":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.BiggestHMI;
                    break;
                case "ROI Max Width":
                    mu.SetAssociatedControl = CTInterface.IA.RoiMan.BiggestWMI;
                    break;
                case "Reset":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.ResetBtn;
                    break;
                case "To Binary":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.ToBinaryBtn;
                    break;
                case "3x3 Box blur":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Box3;
                    break;
                case "5x5 Box blur":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Box5;
                    break;
                case "3x3 Gaussian blur":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Gaus3;
                    break;
                case "5x5 Gaussian blur":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Gaus5;
                    break;
                case "3x3 Median":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Med3;
                    break;
                case "5x5 Median":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Med5;
                    break;
                case "3x3 Sharpen":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Sharp3;
                    break;
                case "5x5 Sharpen":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Sharp5;
                    break;
                case "5x5 Unsharp masking":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Unsharp5;
                    break;
                case "3x3 Edge detection":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Edge3;
                    break;
                case "3x3 Gradient detection":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Grad3;
                    break;
                case "3x3 Sobel operator":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Sobel3;
                    break;
                case "3x3 Embos":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Embos3;
                    break;
                case "5x5 Embos":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Embos5;
                    break;
                case "Erode":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Erode;
                    break;
                case "Dilate":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Dilate;
                    break;
                case "Open Filter":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Open;
                    break;
                case "Close Filter":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Close;
                    break;
                case "Fill Holes":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.FillHoles;
                    break;
                case "Watershed":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.MyFilters.Watershed;
                    break;
                case "Clear segmentation":
                    {
                        Button btn = new Button();
                        btn.Tag = 0;
                        btn.Click += SegmentBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "Otsu thresholding":
                    {
                        Button btn = new Button();
                        btn.Tag = 1;
                        btn.Click += SegmentBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "K-Means":
                    {
                        Button btn = new Button();
                        btn.Tag = 2;
                        btn.Click += SegmentBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "Sum Histogram":
                    mu.SetAssociatedControl = CTInterface.IA.Segmentation.Otsu1D.sumHistogramsCheckBox;
                    break;
                case "0 Threshold":
                    {
                        Button btn = new Button();
                        btn.Tag = 0;
                        btn.Click += SetThresholdBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "1 Threshold":
                    {
                        Button btn = new Button();
                        btn.Tag = 1;
                        btn.Click += SetThresholdBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "2 Threshold":
                    {
                        Button btn = new Button();
                        btn.Tag = 2;
                        btn.Click += SetThresholdBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "3 Threshold":
                    {
                        Button btn = new Button();
                        btn.Tag = 3;
                        btn.Click += SetThresholdBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                case "4 Threshold":
                    {
                        Button btn = new Button();
                        btn.Tag = 4;
                        btn.Click += SetThresholdBtn_Click;
                        mu.SetAssociatedControl = btn;
                    }
                    break;
                default:
                    MessageBox.Show("Error loading: " + mu.GetName);
                    break;
            }
        }
        private void SegmentBtn_Click(object sender, EventArgs e)
        {
            int ind = (int)((Button)sender).Tag;
            CTInterface.IA.Segmentation.SegmentationCBox.Focus();
            CTInterface.IA.Segmentation.SegmentationCBox.SelectedIndex = ind;
            CTInterface.IA.Segmentation.Otsu1D.ProcessBtn.PerformClick();
        }
        private void ShowGLControlImagesBtn_Click(object sender, EventArgs e)
        {
            int ind = (int)((Button)sender).Tag;

            TifFileInfo fi = CTInterface.IA.Segmentation.MyFilters.findFI();
            if (fi == null) return;

            if (fi.tpTaskbar.MethodsBtnList[ind].ImageIndex == 0)
                fi.tpTaskbar.MethodsBtnList[ind].ImageIndex = 1;
            else
                fi.tpTaskbar.MethodsBtnList[ind].ImageIndex = 0;

            CTInterface.IA.ReloadImages();
        }
        private void SetThresholdBtn_Click(object sender, EventArgs e)
        {
            int ind = (int)((Button)sender).Tag;
            CTInterface.IA.Segmentation.Otsu1D.thresholdsNumCB.Focus();
            CTInterface.IA.Segmentation.Otsu1D.thresholdsNumCB.SelectedIndex = ind;
        }
        public void HotKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildForm();
            // Linux change
            this.CTInterface.FileBrowser.StatusLabel.Text = "Dialog open";
            DialogForm.ShowDialog();
            this.CTInterface.FileBrowser.StatusLabel.Text = "Ready";
        }

        public class MemoryUnit
        {
            private string Key;
            private string Name;
            private Object AssociatedControl;
            private Panel Container;
            private Label NameLab;
            private TextBox KeyTB;
            private string buffer;

            public MemoryUnit(string Name, string Key = "")
            {
                this.Name = Name;
                this.Key = Key;

                Container = new Panel();
                Container.Dock = DockStyle.Top;
                Container.Height = 30;

                NameLab = new Label();
                NameLab.Width = 250;
                NameLab.Location = new Point(5, 5);
                NameLab.Text = Name;
                Container.Controls.Add(NameLab);

                KeyTB = new TextBox();
                KeyTB.Text = Key;
                KeyTB.Width = 90;
                KeyTB.Location = new Point(260, 3);
                KeyTB.KeyDown += KeyTB_KeyPressed;
                KeyTB.TextChanged += KeyTB_TextChanged;
                Container.Controls.Add(KeyTB);
            }

            public string GetKey
            {
                get
                {
                    return KeyTB.Text;
                }
            }
            public void ApplyKey()
            {
                Key = KeyTB.Text;
            }
            public void RefreshKey()
            {
                buffer = Key;
                KeyTB.Text = Key;
            }
            public string SetKey
            {
                set
                {
                    Key = value;
                    buffer = Key;
                    KeyTB.Text = Key;
                }
            }
            public string GetName
            {
                get { return Name; }
            }
            public string SetName
            {
                set
                {
                    Name = value;
                    NameLab.Text = Name;
                }
            }

            public Panel GetPanel
            {
                get { return Container; }
            }
            public Object GetAssociatedControl
            {
                get { return AssociatedControl; }
            }
            public Object SetAssociatedControl
            {
                set
                {
                    AssociatedControl = value;
                }
            }
            public void ActivateAssociatedControl()
            {
                if (AssociatedControl == null) return;
                try
                {
                    if (AssociatedControl is Button)
                    {
                        ((Button)AssociatedControl).PerformClick();
                    }
                    else if (AssociatedControl is ToolStripMenuItem)
                    {
                        ((ToolStripMenuItem)AssociatedControl).PerformClick();
                    }
                    else if (AssociatedControl is ToolStripButton)
                    {
                        ((ToolStripButton)AssociatedControl).PerformClick();
                    }
                    else if (AssociatedControl is CheckBox)
                    {
                        ((CheckBox)AssociatedControl).Focus();
                        ((CheckBox)AssociatedControl).Checked = !((CheckBox)AssociatedControl).Checked;
                    }
                    else if (AssociatedControl is MenuItem)
                    {
                        ((MenuItem)AssociatedControl).PerformClick();
                    }
                }
                catch { }
            }
            private void KeyTB_KeyPressed(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.ControlKey) return;
                if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                    buffer = "";
                else
                    buffer = e.KeyCode.ToString();
            }
            private void KeyTB_TextChanged(object sender, EventArgs e)
            {
                if (KeyTB.Text != buffer)
                {
                    KeyTB.Text = buffer;
                }
            }
        }

    }
}
