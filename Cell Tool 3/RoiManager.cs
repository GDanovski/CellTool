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
using System.ComponentModel;

namespace Cell_Tool_3
{
    class RoiManager
    {
        public MenuItem BiggestMI = new MenuItem();
        public MenuItem BiggestHMI = new MenuItem();
        public MenuItem BiggestWMI = new MenuItem();
        public MenuItem AutoRoisMI = new MenuItem();
        public MenuItem ShowLabelsMI = new MenuItem();
        public MenuItem FractureMeasureMI = new MenuItem();
        public MenuItem ConcatenateMI = new MenuItem();
        public ToolStripButton LoadBtn = new ToolStripButton();
        public ToolStripButton ExportBtn = new ToolStripButton();
        public List<ROI> SelectedROIsList = new List<ROI>();//currently selectet rois
        public int RoiType = 0;//static-tracking
        public int RoiShape = 0;//rectangle, oval, polygon, freehand
        public bool turnOnStackRoi = false;//true if the roi must be with stacks
        public ROI current = null;
        //Context menu properties
        public bool showLabels = true;
        private bool CopyMode = false;
        private bool CutMode = false;
        private TifFileInfo OldFi;
        private int SeletedC;
        private ROI[] MoveROIs = null;
        private ROI[] OriginalROIs = null;
        private List<ROI> OriginalROIList = null;
        //controls
        public ImageAnalyser IA;
        private PropertiesPanel_Item PropPanel;
        private Panel InsidePanel = new Panel();
        public Panel panel;
        //Variable controls
        Label RoiName = new Label();
        Label RoiTypeL;
        public TreeView roiTV;
        CTTextBox x_tb = null;
        CTTextBox y_tb = null;
        CTTextBox w_tb = null;
        CTTextBox h_tb = null;
        CTTextBox d_tb = null;
        CTTextBox n_tb = null;
        CTTextBox startT_tb = null;
        CTTextBox finishT_tb = null;
        CTTextBox startZ_tb = null;
        CTTextBox finishZ_tb = null;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();

        const float DEG2RAD = (float)(3.14159 / 180.0);
        //History
        string HistBuf = "";
        #region Initialize
        public RoiManager(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            this.IA = IA;
            PropPanel = new PropertiesPanel_Item();
            PropPanel_Initialize(propertiesPanel, PropertiesBody);

            AddTaskBar();

            OptionsPanel_Add();

            CreateContextMenu();
            //draw new roi events
            IA.GLControl1.MouseDown += GLControl_MouseDown;
            IA.GLControl1.MouseMove += GLControl_MouseMove;
            IA.GLControl1.MouseUp += GLControl_MouseUp;
            IA.GLControl1.MouseMove += GlControl_MouseMoveChangeCursor;
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
            PropPanel.Initialize(propertiesPanel, true);
            PropPanel.Resizable = true;
            PropPanel.Name.Text = "ROI Manager";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;

            panel.Visible = false;
        }
        private void AddTaskBar()
        {
            // build panel

            Panel p = new Panel();
            p.Dock = DockStyle.Top;
            p.Height = 5;
            panel.Controls.Add(p);
            p.BringToFront();

            Color TaskPanelColor = IA.FileBrowser.BackGround2Color1;
            Panel TaskPanel = new Panel();
            TaskPanel.Height = 30;
            TaskPanel.Dock = DockStyle.Top;
            TaskPanel.BackColor = TaskPanelColor;
            panel.Controls.Add(TaskPanel);
            TaskPanel.BringToFront();

            // add task bar
            ToolStrip taskTS = new ToolStrip();
            taskTS.GripStyle = ToolStripGripStyle.Hidden;
            taskTS.Renderer = new MySR();
            {
                taskTS.BackColor = IA.FileBrowser.BackGroundColor1;
                taskTS.ForeColor = IA.FileBrowser.ShriftColor1;
                taskTS.Dock = DockStyle.Top;
                taskTS.ImageScalingSize = new System.Drawing.Size(18, 18);
                TaskPanel.Controls.Add(taskTS);
            }
            //add buttons to taskBar
            ToolStripButton AddBtn = new ToolStripButton();
            {
                AddBtn.Text = "Add new ROI (Ctrl + T)";
                AddBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                AddBtn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 2);
                AddBtn.Image = Properties.Resources.plus;
                taskTS.Items.Add(AddBtn);
                AddBtn.Click += new EventHandler(AddBtn_Click);
            }

            {
                ExportBtn.Text = "Export ROI set";
                ExportBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                ExportBtn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 2);
                ExportBtn.Image = Properties.Resources.export;
                taskTS.Items.Add(ExportBtn);
                ExportBtn.Click += ExportRoiSet_Click;
            }

            {
                LoadBtn.Text = "Load ROI set";
                LoadBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                LoadBtn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 2);
                LoadBtn.Image = Properties.Resources.openFile;
                taskTS.Items.Add(LoadBtn);
                LoadBtn.Click += LoadRoiSet_Click;
            }

            ToolStripButton MeasureBtn = new ToolStripButton();
            {
                MeasureBtn.Text = "Measure checked ROIs (Ctrl + M)";
                MeasureBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                MeasureBtn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 2);
                MeasureBtn.Image = Properties.Resources.paste_mini_1;
                taskTS.Items.Add(MeasureBtn);
                MeasureBtn.Click += MeasureBtn_Click;
            }

            ToolStripButton DeleteBtn = new ToolStripButton();
            {
                DeleteBtn.Text = "Delete selected ROI (Ctrl + D)";
                DeleteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                DeleteBtn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 2);
                DeleteBtn.Image = Properties.Resources.CancelRed;
                taskTS.Items.Add(DeleteBtn);
                DeleteBtn.Click += new EventHandler(DeleteBtn_Click);
            }
        }
        public void MeasureBtn_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }

            if (fi.roiList[fi.cValue] == null || fi.roiList[fi.cValue].Count == 0) return;

            //calculate the size of the result row
            int resultSize = 0;
            foreach (ROI roi in fi.roiList[fi.cValue])
                if (roi.Checked == true)
                {
                    if (roi.Results == null) RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    if (roi.Shape == 0 || roi.Shape == 1)
                        resultSize += roi.Results[fi.cValue].Length;
                    else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                        resultSize += 4 + roi.Stack * 4;

                }

            if (resultSize == 0) return;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            TreeNode node = IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Tag as TreeNode;

            string formatMiniStr = ".txt";
            string formatStr = "TAB delimited files (*" + formatMiniStr + ")|*" + formatMiniStr;
            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.InitialDirectory = node.Tag.ToString().Substring(0, node.Tag.ToString().Length - (node.Text.Length + 1));
            saveFileDialog1.FileName = node.Text.Replace(".tif", "");
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Title = "Measure ROIs to:";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = saveFileDialog1.FileName.Replace(".tif", "");
                if (dir.EndsWith(formatMiniStr) == false)
                    dir += formatMiniStr;
                //create result matrix
                double[] result;

                int t = 1;
                int z = 1;
                int position;
                string str;

                double time = 0;
                int timeIndex = 0;
                double timeT = fi.TimeSteps[timeIndex];
                try
                {
                    if (File.Exists(dir))
                        File.Delete(dir);
                }
                catch
                {
                    MessageBox.Show("File is used by other program!");
                    return;
                }
                IA.FileBrowser.StatusLabel.Text = "Saving results...";
                using (StreamWriter write = new StreamWriter(dir))
                {
                    //titles part
                    List<string> titles = new List<string>();
                    titles.Add("ImageN");
                    if (fi.sizeT > 1) titles.Add("T");
                    if (fi.sizeT > 1) titles.Add("T(sec.)");
                    if (fi.sizeZ > 1) titles.Add("Z");

                    int roiN = 1;
                    foreach (ROI roi in fi.roiList[fi.cValue])
                    {
                        if (roi.Checked == true && roi.Results[fi.cValue] != null)
                        {
                            string com = "";
                            if (roi.Comment != "") com = ": " + roi.Comment;

                            titles.Add("Area" + roiN.ToString() + com);
                            titles.Add("Mean" + roiN.ToString() + com);
                            titles.Add("Min" + roiN.ToString() + com);
                            titles.Add("Max" + roiN.ToString() + com);
                            if (roi.Stack > 0)
                                if (roi.Shape == 0 || roi.Shape == 1)
                                    for (int n = 1; n <= roi.Stack; n++)
                                    {
                                        titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                        titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                        titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                        titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);

                                        titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                        titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                        titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                        titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);

                                        titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                        titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                        titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                        titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);

                                        titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                        titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                        titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                        titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);

                                    }
                                else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                    for (int n = 1; n <= roi.Stack; n++)
                                    {
                                        titles.Add("Area" + roiN.ToString() + "." + n.ToString() + com);
                                        titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + com);
                                        titles.Add("Min" + roiN.ToString() + "." + n.ToString() + com);
                                        titles.Add("Max" + roiN.ToString() + "." + n.ToString() + com);
                                    }
                        }
                        roiN++;
                    }
                    write.WriteLine(string.Join("\t", titles));
                    //calculations
                    for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                    {
                        //extract row from rois
                        position = 0;
                        result = new double[resultSize];
                        foreach (ROI roi in fi.roiList[fi.cValue])
                        {
                            if (roi.Checked == true)
                            {
                                if (roi.Shape == 0 || roi.Shape == 1)
                                {
                                    if (roi.Results[i] != null
                                && roi.FromT <= t && roi.ToT >= t
                                && roi.FromZ <= z && roi.ToZ >= z)
                                    {
                                        Array.Copy(roi.Results[i], 0, result, position, roi.Results[i].Length);
                                    }

                                    position += roi.Results[fi.cValue].Length;
                                }
                                else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                {
                                    if (roi.Results[i] != null
                                && roi.FromT <= t && roi.ToT >= t
                                && roi.FromZ <= z && roi.ToZ >= z)
                                    {
                                        //main roi
                                        Array.Copy(roi.Results[i], 0, result, position, 4);
                                        position += 4;
                                        //layers
                                        for (int p = 4; p < roi.Results[i].Length; p += 16)
                                        {
                                            Array.Copy(roi.Results[i], p, result, position, 4);
                                            position += 4;
                                        }
                                    }
                                    else
                                    {
                                        position += 4;
                                        //layers
                                        for (int p = 4; p < roi.Results[i].Length; p += 16)
                                        {
                                            position += 4;
                                        }
                                    }

                                }
                            }

                        }
                        //write the line
                        if (CheckArrayForValues(result))
                        {
                            str = string.Join("\t", result);

                            if (fi.sizeZ > 1) str = z.ToString() + "\t" + str;
                            if (fi.sizeT > 1)
                            {
                                str = t.ToString() + "\t" + time.ToString() + "\t" + str;
                            }
                            str = i.ToString() + "\t" + str;
                            write.WriteLine(str);
                        }
                        //recalculate z and t

                        z += 1;
                        if (z > fi.sizeZ)
                        {
                            z = 1;
                            t += 1;

                            if (t > fi.sizeT)
                            {
                                t = 1;
                            }

                            if (t <= timeT)
                            {
                                time += fi.TimeSteps[timeIndex + 1];
                            }
                            else
                            {
                                timeIndex += 2;

                                if (timeIndex < fi.TimeSteps.Count)
                                    timeT += fi.TimeSteps[timeIndex];
                                else
                                {
                                    timeIndex -= 2;
                                    timeT += fi.imageCount;
                                }

                                time += fi.TimeSteps[timeIndex + 1];
                            }

                        }
                    }
                }
                IA.FileBrowser.Refresh_AfterSave();
                IA.FileBrowser.StatusLabel.Text = "Ready";
                //MessageBox.Show("Results are saved!");  
            }

        }
        private bool CheckArrayForValues(double[] input)
        {
            if (!IA.DeleteEmptyEnabled) return true;

            foreach (double val in input)
                if (val != 0)
                    return true;

            return false;
        }
        private void LoadRoiSet_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            OpenFileDialog ofd = new OpenFileDialog();
            string formatMiniStr = ".RoiSet";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;

            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Title = "Load ROI set from:";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName))
                {
                    string str = sr.ReadToEnd();
                    foreach (string val in str.Split(new string[] { "\r\n" }, StringSplitOptions.None))
                    {
                        if (val != "")
                            roi_new(val, fi);
                    }
                }
                IA.ReloadImages();
            }
        }
        public void LoadRoiSet_DragDrop(string dir)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            using (StreamReader sr = new StreamReader(dir))
            {
                string str = sr.ReadToEnd();
                foreach (string val in str.Split(new string[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (val != "")
                        roi_new(val, fi);
                }
            }

            IA.ReloadImages();

        }
        public void roi_new(string val, TifFileInfo fi)
        {

            string[] vals = val.Substring(8, val.Length - 9).Split(new string[] { "," }, StringSplitOptions.None);

            int chanel = Convert.ToInt32(vals[0]);

            if (chanel >= fi.sizeC) return;

            int RoiID = fi.ROICounter;
            fi.ROICounter++;

            string RoiInfo = vals[2];

            ROI current = new ROI();
            current.CreateFromHistory(RoiID, RoiInfo);

            if (fi.roiList[chanel] == null) fi.roiList[chanel] = new List<ROI>();
            fi.roiList[chanel].Add(current);

            RoiMeasure.Measure(current, fi, chanel, IA);
        }
        private void ExportRoiSet_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (roiTV.Nodes.Count == 0) return;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            TreeNode node = IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Tag as TreeNode;

            string formatMiniStr = ".RoiSet";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;
            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.InitialDirectory = node.Tag.ToString().Substring(0, node.Tag.ToString().Length - (node.Text.Length + 1));
            saveFileDialog1.FileName = node.Text;

            if(node.Text.LastIndexOf(".")>-1)
                saveFileDialog1.FileName = node.Text.Substring(0, node.Text.LastIndexOf("."));

            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Title = "Save ROI set to:";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = saveFileDialog1.FileName.Replace(".tif", "");

                if (dir.EndsWith(formatMiniStr) == false)
                    dir += formatMiniStr;

                if (dir.Substring(dir.Length - 7, 7) != ".RoiSet") dir += ".RoiSet";

                using (StreamWriter sw = new StreamWriter(dir))
                {

                    foreach (TreeNode n in roiTV.Nodes)
                    {
                        ROI roi = (ROI)n;
                        sw.WriteLine(roi_new(fi.cValue, roi));
                    }
                    //MessageBox.Show("Roi set saved to:\n" + dir);
                }
                IA.FileBrowser.CheckForFile(dir);
            }
        }
        #region ContextMenu
        public void RenameTb_Add(ROI node)
        {
            current = node;
            SelectedROIsList.Clear();
            SelectedROIsList.Add(node);

            TextBox tb = new TextBox();
            tb.Tag = node;
            tb.Text = node.Comment;
            tb.Location = new System.Drawing.Point(node.Bounds.X, node.Bounds.Y - 2);
            tb.Width = node.Bounds.Width;
            tb.Height = node.Bounds.Height;
            tb.Visible = true;
            roiTV.Controls.Add(tb);
            tb.Focus();
            if (!String.IsNullOrEmpty(tb.Text))
            {
                tb.SelectionStart = 0;
                tb.SelectionLength = tb.Text.Length;
            }

            tb.TextChanged += new EventHandler(delegate (Object o, EventArgs a)
            {
                int w = TextRenderer.MeasureText(tb.Text, tb.Font).Width + 5;
                if (tb.Width < w)
                {
                    tb.Width = w;
                }
            });

            tb.LostFocus += new EventHandler(delegate (Object o, EventArgs a)
            {
                if (tb.Text.IndexOf("=") > -1)
                {
                    MessageBox.Show("Symbol \"=\" is not allowed!");
                }
                else
                {
                    TifFileInfo fi = null;
                    try
                    {
                        fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    }
                    catch { return; }
                    if (fi == null) return;

                    #region History
                    if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(node) > -1)
                        addToHistoryOldInfo(roi_getStat(node, fi, "Comment"), fi);

                    #endregion History

                    node.Comment = tb.Text;

                    #region History
                    if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(node) > -1)
                        addToHistoryNewInfo(roi_getStat(node, fi, "Comment"), fi);
                    #endregion History
                }

                tb.Dispose();
            });
            tb.KeyDown += new KeyEventHandler(delegate (Object o, KeyEventArgs a)
            {
                if (a.KeyCode == Keys.Enter)
                {
                    if (tb.Text.IndexOf("=") > -1)
                    {
                        MessageBox.Show("Symbol \"=\" is not allowed!");
                    }
                    else
                    {
                        TifFileInfo fi = null;
                        try
                        {
                            fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                        }
                        catch { return; }
                        if (fi == null) return;

                        #region History
                        if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(node) > -1)
                            addToHistoryOldInfo(roi_getStat(node, fi, "Comment"), fi);

                        #endregion History

                        node.Comment = tb.Text;

                        #region History
                        if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(node) > -1)
                            addToHistoryNewInfo(roi_getStat(node, fi, "Comment"), fi);
                        #endregion History


                    }

                    tb.Dispose();

                    a.Handled = true;
                    a.SuppressKeyPress = true;
                }

            });
            roiTV.AfterSelect += new TreeViewEventHandler(delegate (Object o, TreeViewEventArgs a)
            {
                tb.Dispose();
            });
            roiTV.MouseWheel += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
            {
                tb.Dispose();
            });
        }
        private void RenameBtn_Click(object sender, EventArgs e)
        {
            ROI node = (ROI)((MenuItem)sender).Tag;
            if (node != null)
            {
                RenameTb_Add(node);
            }
            else
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        private void CreateContextMenu()
        {
            ContextMenu menu = new ContextMenu();

            MenuItem RenameMI = new MenuItem();
            RenameMI.Text = "Rename";
            menu.MenuItems.Add(RenameMI);
            RenameMI.Click += RenameBtn_Click;

            MenuItem sepMB00 = new MenuItem();
            sepMB00.Text = "-";
            menu.MenuItems.Add(sepMB00);
            
            ConcatenateMI.Text = "Concatenate ROIs";
            menu.MenuItems.Add(ConcatenateMI);
            ConcatenateMI.Click += ConcatenateRoi_Click;
            
            FractureMeasureMI.Text = "Export Color";
            menu.MenuItems.Add(FractureMeasureMI);
            ColorFractureRoiMeasure colFract = new ColorFractureRoiMeasure(IA);
            FractureMeasureMI.Click += colFract.ExportAllResults;

            menu.MenuItems.Add("-");

            MenuItem CheckMI = new MenuItem();
            CheckMI.Text = "Check";
            menu.MenuItems.Add(CheckMI);
            CheckMI.Click += ROI_Check;

            MenuItem CheckAllMI = new MenuItem();
            CheckAllMI.Text = "Check All";
            menu.MenuItems.Add(CheckAllMI);
            CheckAllMI.Click += ROI_CheckAll;

            MenuItem UnCheckAllMI = new MenuItem();
            UnCheckAllMI.Text = "Uncheck All";
            menu.MenuItems.Add(UnCheckAllMI);
            UnCheckAllMI.Click += ROI_CheckAll;
            
            ShowLabelsMI.Text = "Show Labels";
            menu.MenuItems.Add(ShowLabelsMI);
            ShowLabelsMI.Click += ShowLabelsMI_Click;
            
            AutoRoisMI.Text = "Auto find";
            menu.MenuItems.Add(AutoRoisMI);
            AutoRoisMI.Click += AutoRoisMI_Click;

            MenuItem sepMB0 = new MenuItem();
            sepMB0.Text = "-";
            menu.MenuItems.Add(sepMB0);
            
            BiggestWMI.Text = "Max Width";
            menu.MenuItems.Add(BiggestWMI);
            BiggestWMI.Click += BiggestWMI_Click;
            
            BiggestHMI.Text = "Max Hight";
            menu.MenuItems.Add(BiggestHMI);
            BiggestHMI.Click += BiggestHMI_Click;

           
            BiggestMI.Text = "Max Size";
            menu.MenuItems.Add(BiggestMI);
            BiggestMI.Click += BiggestMI_Click;

            MenuItem sepMB1 = new MenuItem();
            sepMB1.Text = "-";
            menu.MenuItems.Add(sepMB1);

            MenuItem CopyMI = new MenuItem();
            CopyMI.Text = "Copy";
            menu.MenuItems.Add(CopyMI);
            CopyMI.Click += CopyRois;

            MenuItem CutMI = new MenuItem();
            CutMI.Text = "Cut";
            menu.MenuItems.Add(CutMI);
            CutMI.Click += CutRois;

            MenuItem PasteMI = new MenuItem();
            PasteMI.Text = "Paste";
            menu.MenuItems.Add(PasteMI);
            PasteMI.Click += PasteRois;

            MenuItem sepMB2 = new MenuItem();
            sepMB2.Text = "-";
            menu.MenuItems.Add(sepMB2);

            MenuItem DeleteMI = new MenuItem();
            DeleteMI.Text = "Delete";
            menu.MenuItems.Add(DeleteMI);
            DeleteMI.Click += DeleteBtn_Click;

            roiTV.NodeMouseClick += new TreeNodeMouseClickEventHandler(
                delegate (object sender, TreeNodeMouseClickEventArgs e)
                {

                    //retrive context
                    if (e.Button != MouseButtons.Right) return;
                    ROI node = (ROI)roiTV.GetNodeAt(e.X, e.Y);

                    RenameMI.Tag = node;

                    if (SelectedROIsList.Count > 0)
                        if (!(SelectedROIsList.IndexOf(node) > -1))
                            return;
                    //restore 
                    sepMB0.Visible = false;
                    BiggestHMI.Visible = false;
                    BiggestWMI.Visible = false;
                    BiggestMI.Visible = false;
                    CheckMI.Enabled = true;
                    DeleteMI.Enabled = true;
                    CopyMI.Enabled = true;
                    CutMI.Enabled = true;
                    CheckAllMI.Enabled = true;
                    UnCheckAllMI.Enabled = true;
                    PasteMI.Enabled = false;
                    //configurate menu
                    if (node.Checked == true)
                        CheckMI.Text = "Uncheck";
                    else
                        CheckMI.Text = "Check";

                    if (CopyMode == true | CutMode == true)
                        PasteMI.Enabled = true;

                    if (showLabels == true)
                        ShowLabelsMI.Text = "Hide Labels";
                    else
                        ShowLabelsMI.Text = "Show Labels";

                    if (node.returnBiggest == true)
                    {
                        sepMB0.Visible = true;
                        BiggestHMI.Visible = true;
                        BiggestWMI.Visible = true;
                        BiggestMI.Visible = true;
                    }

                    if (node.Shape != 0 && node.Shape != 1)
                    {
                        BiggestWMI.Visible = false;
                        BiggestHMI.Visible = false;
                    }
                    //show menu 
                    menu.Show(roiTV, new Point(e.X, e.Y));
                });

            roiTV.MouseDown += new MouseEventHandler(
            delegate (object sender, MouseEventArgs e)
            {
                    //retrive context
                    ROI node = (ROI)roiTV.GetNodeAt(e.X, e.Y);

                if (node == null && e.Button != MouseButtons.Right)
                {
                    current = null;
                    SelectedROIsList.Clear();
                    IA.ReloadImages();
                    return;
                }

                if (SelectedROIsList.IndexOf(node) > -1) return;

                if (e.Button != MouseButtons.Right) return;

                    //restore
                    sepMB0.Visible = false;
                BiggestHMI.Visible = false;
                BiggestWMI.Visible = false;
                BiggestMI.Visible = false;
                CheckMI.Enabled = true;
                DeleteMI.Enabled = true;
                CopyMI.Enabled = true;
                CutMI.Enabled = true;
                CheckAllMI.Enabled = true;
                UnCheckAllMI.Enabled = true;
                PasteMI.Enabled = false;
                    //configurate menu
                    if (node == null)
                {
                    CheckMI.Enabled = false;
                    DeleteMI.Enabled = false;
                    CopyMI.Enabled = false;
                    CutMI.Enabled = false;
                }
                else if (node.Checked == true)
                    CheckMI.Text = "Uncheck";
                else
                    CheckMI.Text = "Check";

                if (node != null)
                {
                    SelectedROIsList.Clear();
                    current = node;
                    SelectedROIsList.Add(node);
                    IA.ReloadImages();
                }

                if (roiTV.Nodes.Count == 0)
                {
                    CheckAllMI.Enabled = false;
                    UnCheckAllMI.Enabled = false;
                }

                if (CopyMode == true | CutMode == true)
                    PasteMI.Enabled = true;

                if (showLabels == true)
                    ShowLabelsMI.Text = "Hide Labels";
                else
                    ShowLabelsMI.Text = "Show Labels";
                    //show menu
                    menu.Show(roiTV, new Point(e.X, e.Y));
            });
        }
        private void AutoRoisMI_Click(object sender, EventArgs e)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            /*
            // OpenFile
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.
                System.IO.StreamReader sr = new
                System.IO.StreamReader(openFileDialog1.FileName);
                fi.FileDescription = sr.ReadToEnd();
                sr.Close();
            }
            else return;
            */
            //check the data
            foreach (string strLine in fi.FileDescription.Split(new string[] { "\n" }, StringSplitOptions.None))
                if (strLine.IndexOf("_FRAPPA") > -1) addRoiFromProtocol(strLine, fi, frame);
            //Add from Info file
            FillRoisFromInfoFile(fi, frame);
            //Clear selected roi list
            SelectedROIsList.Clear();
            FillRoiManagerList(fi);
            //Redraw
            IA.ReloadImages();
        }
        private void FillRoisFromInfoFile(TifFileInfo fi, int frame)
        {
            string dir = fi.Dir.Substring(0, fi.Dir.LastIndexOf("\\"));
            string name = fi.Dir.Substring(fi.Dir.LastIndexOf("\\") + 1, fi.Dir.Length - fi.Dir.LastIndexOf("\\") - 1);
            List<string> roiInfo = new List<string>();

            if (!File.Exists(dir + "\\RoiInfo.txt")) return;

            try
            {
                using (StreamReader sr = new StreamReader(dir + "\\RoiInfo.txt"))
                {
                    string str = sr.ReadLine();
                    while (str != null)
                    {
                        if (str.StartsWith(name))
                        {
                            roiInfo.Add(str);
                        }
                        str = sr.ReadLine();
                    }
                }
            }
            catch
            {
                MessageBox.Show("RoiInfo.txt - file used by other program!");
                return;
            }


            foreach (string str in roiInfo)
            {
                try
                {
                    string info = str.Substring(str.IndexOf("=") + 1, str.Length - str.IndexOf("=") - 1);
                    AddRoiFromInfoFile(info, fi, frame);
                }
                catch
                {
                    MessageBox.Show("Error!\n" + str);
                    return;
                }
            }

            roiInfo = null;
        }
        private void AddRoiFromInfoFile(string str, TifFileInfo fi, int frame)
        {
            string type = str.Substring(0, str.IndexOf("("));
            string[] values = str.Substring(str.IndexOf("(") + 1, str.Length - 2 - str.IndexOf("(")).Split(',');

            switch (type)
            {
                case "oval":
                    {
                        int x = int.Parse(values[0]);
                        int y = int.Parse(values[1]);
                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x, y);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 1, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = int.Parse(values[2])-1;
                        roi.Height = int.Parse(values[3])-1;
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "rectangle":
                    {
                        int x = int.Parse(values[0]);
                        int y = int.Parse(values[1]);
                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x, y);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 0, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = int.Parse(values[2])-1;
                        roi.Height = int.Parse(values[3])-1;
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "polygon":
                    {
                        List<Point> pList = new List<Point>();
                        for (int x = 0, y = 1; y < values.Length; x+=2, y+=2)
                            pList.Add(new Point(int.Parse(values[x]), int.Parse(values[y])));

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 2, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, pList.ToArray());

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "freehand":
                    {
                        List<Point> pList = new List<Point>();
                        for (int x = 0, y = 1; y < values.Length; x+=2, y+=2)
                            pList.Add(new Point(int.Parse(values[x]), int.Parse(values[y])));

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 3, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, pList.ToArray());

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
            }
        }
        private void addRoiFromProtocol(string strLine, TifFileInfo fi, int frame)
        {
            string[] vals = strLine.Split(new string[] { "\t" }, StringSplitOptions.None);
            if (vals.Length < 2) return;

            string type = vals[1];

            int shape = RoiShape;
            if (RoiShape != 0 && RoiShape != 1) shape = 0;

            vals = vals[2].Replace("NumberOfPoints( ", "").Replace(") : ( ", "\n")
                .Replace(") ( ", "\n").Replace(", ", "\n").Replace(")", "\n")
                .Split(new string[] { "\n" }, StringSplitOptions.None);
            switch (type)
            {
                case "Point":
                    {
                        int x = int.Parse(vals[1]) - fi.xCompensation;
                        int y = int.Parse(vals[2]) - fi.yCompensation;
                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x - 5, y - 5);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, shape, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = 9;
                        roi.Height = 9;
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "Rectangle":
                    {
                        int x = int.Parse(vals[1]) - fi.xCompensation;
                        int y = int.Parse(vals[2]) - fi.yCompensation;
                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x, y);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 0, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = int.Parse(vals[3]) - int.Parse(vals[1]);
                        roi.Height = int.Parse(vals[4]) - int.Parse(vals[2]);
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "Line":
                    {
                        int x = int.Parse(vals[1]) - (int.Parse(vals[1]) - int.Parse(vals[3])) / 2 - fi.xCompensation;
                        int y = int.Parse(vals[2]) - (int.Parse(vals[2]) - int.Parse(vals[4])) / 2 - fi.yCompensation;

                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x - 5, y - 5);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, shape, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = 9;
                        roi.Height = 9;
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                case "Ellipse":
                    {
                        int x = int.Parse(vals[1]) - fi.xCompensation;
                        int y = int.Parse(vals[2]) - fi.yCompensation;
                        if (!(x >= 0 && y >= 0
                            && x < fi.sizeX && y < fi.sizeY)) break;

                        Point p = new Point(x, y);

                        ROI roi = new ROI(fi.ROICounter, fi.imageCount, 1, 0, turnOnStackRoi);
                        fi.ROICounter++;

                        roi.Width = int.Parse(vals[3]) - int.Parse(vals[1]);
                        roi.Height = int.Parse(vals[4]) - int.Parse(vals[2]);
                        roi.FromT = 1;
                        roi.FromZ = 1;
                        roi.ToT = fi.sizeT;
                        roi.ToZ = fi.sizeZ;

                        if (turnOnStackRoi == true)
                        {
                            roi.Stack = 1;
                            roi.D = 3;
                        }

                        roi.SetLocation(frame, new Point[] { p });

                        if (fi.roiList[fi.cValue] == null)
                            fi.roiList[fi.cValue] = new List<ROI>();

                        fi.roiList[fi.cValue].Add(roi);
                        RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                    }
                    break;
                default:
                    break;
            }

        }

        private void BiggestHMI_Click(object sender, EventArgs e)
        {
            if (current == null) return;
            int imageN = current.biggestH;

            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //calculate Z and T
            int c = fi.cValue;
            FrameCalculator FC = new FrameCalculator();
            int[] res = FC.FrameCalculateTZ(fi, c, imageN);
            fi.frame = res[0];
            fi.zValue = res[1];

            if (fi.sizeZ > 1)
                IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);

            if (fi.sizeT > 1)
                IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);

            IA.ReloadImages();
        }
        private void BiggestMI_Click(object sender, EventArgs e)
        {
            if (current == null) return;
            int imageN = current.biggestW;
            if (current.Width < current.Height) imageN = current.biggestH;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //calculate Z and T
            int c = fi.cValue;
            FrameCalculator FC = new FrameCalculator();
            int[] res = FC.FrameCalculateTZ(fi, c, imageN);
            fi.frame = res[0];
            fi.zValue = res[1];
            if (fi.sizeZ > 1)
                IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);

            if (fi.sizeT > 1)
                IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);
            IA.ReloadImages();
        }
        private void BiggestWMI_Click(object sender, EventArgs e)
        {
            if (current == null) return;
            int imageN = current.biggestW;

            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //calculate Z and T
            int c = fi.cValue;
            FrameCalculator FC = new FrameCalculator();
            int[] res = FC.FrameCalculateTZ(fi, c, imageN);

            fi.frame = res[0];
            fi.zValue = res[1];

            if (fi.sizeZ > 1)
                IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);

            if (fi.sizeT > 1)
                IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);

            IA.ReloadImages();
        }
        private void ShowLabelsMI_Click(object sender, EventArgs e)
        {
            showLabels = !showLabels;
            IA.ReloadImages();
        }
        public void CopyRois(object sender, EventArgs e)
        {
            //if (IA.GLControl1.Focused == false & roiTV.Focused == false) return;
            if (IA.FileBrowser.TreeViewExp.Focused || IA.FileBrowser.Vbox.Focused) return;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //restore
            CopyMode = false;
            CutMode = false;
            //prepare lists
            if (SelectedROIsList.Count == 0) return;

            MoveROIs = new ROI[SelectedROIsList.Count];
            OriginalROIs = null;

            for (int i = 0; i < SelectedROIsList.Count; i++)
            {
                MoveROIs[i] = SelectedROIsList[i].Duplicate();
            }
            //Chose type
            this.OldFi = fi;
            this.SeletedC = fi.cValue;
            CopyMode = true;
        }
        public void CutRois(object sender, EventArgs e)
        {
            //if (IA.GLControl1.Focused == false & roiTV.Focused == false) return;
            if (IA.FileBrowser.TreeViewExp.Focused || IA.FileBrowser.Vbox.Focused) return;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //restore
            CopyMode = false;
            CutMode = false;
            //prepare lists
            if (SelectedROIsList.Count == 0) return;

            MoveROIs = new ROI[SelectedROIsList.Count];
            OriginalROIs = new ROI[SelectedROIsList.Count];

            for (int i = 0; i < SelectedROIsList.Count; i++)
            {
                MoveROIs[i] = SelectedROIsList[i].Duplicate();
                OriginalROIs[i] = SelectedROIsList[i];
            }

            OriginalROIList = fi.roiList[fi.cValue];
            //Chose type
            this.OldFi = fi;
            this.SeletedC = fi.cValue;
            CutMode = true;
        }
        public void PasteRois(object sender, EventArgs e)
        {
            if (CopyMode == false && CutMode == false) return;
            //if (IA.GLControl1.Focused == false & roiTV.Focused == false) return;
            if (IA.FileBrowser.TreeViewExp.Focused || IA.FileBrowser.Vbox.Focused) return;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            //Check is there anything for copy
            if (MoveROIs == null) { fi = null; return; }

            if (fi.roiList[fi.cValue] == null)
                fi.roiList[fi.cValue] = new List<ROI>();

            foreach (ROI roi in MoveROIs)
            {
                try
                {
                    if (roi.Type == 1)
                    {
                        if ((OldFi.sizeZ == 1 && fi.sizeZ != 1) |
                                (OldFi.sizeT == 1 && fi.sizeT != 1))
                        {
                            MessageBox.Show("Tracking roi can NOT be paste in image with diferent Z and T");
                            continue;
                        }

                        PrepareTrackingRoiForPaste(fi, roi);
                    }
                    fi.roiList[fi.cValue].Add(roi);
                    roi.getID = fi.ROICounter;
                    fi.ROICounter++;

                    #region History
                    addToHistoryOldInfo(roi_delete(fi.cValue, roi.getID), fi);
                    addToHistoryNewInfo(roi_new(fi.cValue, roi), fi);
                    #endregion History

                    RoiMeasure.Measure(roi, fi, fi.cValue, IA);
                }
                catch { }
            }

            if (CutMode == true)
                foreach (ROI roi in OriginalROIs)
                {
                    if (roi.Type == 1)
                        if ((OldFi.sizeZ == 1 && fi.sizeZ != 1) |
                                (OldFi.sizeT == 1 && fi.sizeT != 1))
                            continue;

                    #region History
                    addToHistoryOldInfo(roi_new(OldFi.cValue, roi), OldFi);
                    addToHistoryNewInfo(roi_delete(OldFi.cValue, roi.getID), OldFi);
                    #endregion History

                    OriginalROIList.Remove(roi);
                }

            //restore
            current = MoveROIs[MoveROIs.Length - 1];
            SelectedROIsList.Clear();
            SelectedROIsList.Add(current);

            OriginalROIs = null;
            MoveROIs = null;
            OriginalROIList = null;

            OldFi = null;
            CopyMode = false;
            CutMode = false;
            //refresh
            IA.ReloadImages();
        }
        private void PrepareTrackingRoiForPaste(TifFileInfo fi, ROI roi)
        {
            if (roi.Shape == 3)
            {
                PasteTrackingPolygon(fi, roi);
                return;
            }
            Point lastP = new Point();
            Point[] OldPList = roi.GetLocationAll()[0];
            Point[] NewPList = new Point[fi.imageCount];

            for (int OldFrame = this.SeletedC, frame = fi.cValue;
                frame < fi.imageCount; OldFrame += OldFi.sizeC, frame += fi.sizeC)
            {
                if (OldFrame < OldFi.imageCount)
                    lastP = OldPList[OldFrame];

                NewPList[frame].X = lastP.X;
                NewPList[frame].Y = lastP.Y;
            }

            roi.SetLocationAll(new Point[][] { NewPList });
        }
        private void PasteTrackingPolygon(TifFileInfo fi, ROI roi)
        {

            Point[] lastP = null;
            Point[][] OldPList = roi.GetLocationAll();
            Point[][] NewPList = new Point[fi.imageCount][];

            for (int OldFrame = this.SeletedC, frame = fi.cValue;
                frame < fi.imageCount; OldFrame += OldFi.sizeC, frame += fi.sizeC)
            {
                if (OldFrame < OldFi.imageCount)
                    lastP = OldPList[OldFrame];

                //if (lastP == null) continue;

                NewPList[frame] = new Point[lastP.Length];

                for (int i = 0; i < lastP.Length; i++)
                {
                    NewPList[frame][i].X = lastP[i].X;
                    NewPList[frame][i].Y = lastP[i].Y;
                }
            }

            roi.SetLocationAll(NewPList);
        }
        private void ROI_Check(object sender, EventArgs e)
        {
            MenuItem MI = (MenuItem)sender;
            roiTV.SuspendLayout();
            roiTV.Nodes.Clear();

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            foreach (ROI roi in SelectedROIsList)
            {
                if (MI.Text == "Check" & roi.Checked != true)
                {
                    roi.Checked = true;
                }
                else if (roi.Checked != false)
                {
                    roi.Checked = false;
                }

            }

            IA.ReloadImages();
            roiTV.ResumeLayout();
        }
        private void ROI_CheckAll(object sender, EventArgs e)
        {
            MenuItem MI = (MenuItem)sender;

            //check is there any file info
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            roiTV.SuspendLayout();
            roiTV.Nodes.Clear();

            foreach (ROI roi in fi.roiList[fi.cValue])
            {

                if (MI.Text == "Check All")
                {
                    roi.Checked = true;
                }
                else if (roi.Checked != false)
                {
                    roi.Checked = false;
                }
            }

            IA.ReloadImages();
            roiTV.ResumeLayout();
        }

        #endregion ContextMenu
        public void FillRoiManagerList(TifFileInfo fi)
        {
            List<ROI> l = fi.roiList[fi.cValue];
            //clear if roi list is empty
            if (l == null)
            {
                roiTV.Nodes.Clear();
                return;
            }
            //clear if nodes are not rois from the list
            for (int i = roiTV.Nodes.Count - 1; i >= 0; i--)
            {
                ROI roi = (ROI)roiTV.Nodes[i];
                if (!(l.IndexOf(roi) > -1)) roiTV.Nodes.RemoveAt(i);
            }
            //check is order ok
            for (int i = 0; i < l.Count; i++)
            {
                string str = "Tracking ";
                if (l[i].Type == 0) str = "Static ";

                if (l[i].Stack > 0) str += "Stack ";

                str += "ROI " + (i + 1).ToString();

                if (l[i].Comment != "")
                    str += ": " + l[i].Comment;

                if (l[i].Text != str)
                {
                    l[i].Name = "ROI " + (i + 1).ToString();
                    l[i].Text = str;
                }


                if (roiTV.Nodes.Count <= i)
                {
                    roiTV.Nodes.Add(l[i]);
                }
            }
            roiTV.SelectedNode = null;
            //select the ROI
            if (!(SelectedROIsList.IndexOf(current) > -1))
                SelectedROIsList.Clear();

            if (SelectedROIsList.Count > 0)
                if (!(roiTV.Nodes.IndexOf(SelectedROIsList[0]) > -1)
                    | current == null)
                    SelectedROIsList.Clear();


            foreach (ROI node in roiTV.Nodes)
            {
                node.ImageIndex = node.Shape;
                node.SelectedImageIndex = node.Shape;
                if (node == current)
                {
                    node.BackColor = IA.FileBrowser.TitlePanelColor1;
                }
                else if (SelectedROIsList.IndexOf(node) > -1)
                {
                    node.BackColor = IA.FileBrowser.TitlePanelColor1;
                }
                else
                {
                    node.BackColor = IA.FileBrowser.BackGround2Color1;
                }
            }

        }
        public void AddBtn_Click(object sender, EventArgs e)
        {
            //check for current roi
            if (current == null) return;
            //check is there any file info
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;
            //create list
            if (fi.roiList[fi.cValue] == null) fi.roiList[fi.cValue] = new List<ROI>();
            //check is current roi in the list
            if (fi.roiList[fi.cValue].IndexOf(current) > -1) return;
            //add current roi to the list
            fi.roiList[fi.cValue].Add(current);
            SelectedROIsList.Clear();
            SelectedROIsList.Add(current);
            #region History
            addToHistoryOldInfo(roi_delete(fi.cValue, current.getID), fi);
            addToHistoryNewInfo(roi_new(fi.cValue, current), fi);
            #endregion History
            //reload to screen
            RoiMeasure.Measure(current, fi, fi.cValue, IA);
            IA.ReloadImages();
        }
        public void selectAllRois(KeyEventArgs e)
        {
            if (IA.GLControl1.Focused == false & roiTV.Focused == false) return;

            SelectedROIsList.Clear();

            foreach (TreeNode node in roiTV.Nodes)
            {
                SelectedROIsList.Add((ROI)node);
                if (current == null)
                    current = (ROI)node;
            }

            e.SuppressKeyPress = true;
            e.Handled = true;
            IA.ReloadImages();

        }
        private void selectedRoiChanged(ROI roi)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if (SelectedROIsList.IndexOf(roi) > -1)
                {
                    SelectedROIsList.Remove(roi);
                    if (SelectedROIsList.Count > 0)
                        current = SelectedROIsList[SelectedROIsList.Count - 1];
                    else
                        current = null;
                }
                else
                {
                    current = roi;
                    SelectedROIsList.Add(roi);
                }
            }
            else if (Control.ModifierKeys == Keys.Shift)
            {
                if (SelectedROIsList.Count > 0)
                {
                    int startI = roiTV.Nodes.IndexOf(SelectedROIsList[0]);
                    int stopI = roiTV.Nodes.IndexOf(roi);

                    SelectedROIsList.Clear();

                    if (startI <= stopI)
                        for (int i = startI; i <= stopI; i++)
                            SelectedROIsList.Add((ROI)roiTV.Nodes[i]);
                    else
                        for (int i = startI; i >= stopI; i--)
                            SelectedROIsList.Add((ROI)roiTV.Nodes[i]);
                }
                else
                {
                    SelectedROIsList.Add(roi);
                }

                current = roi;
            }
            else
            {
                SelectedROIsList.Clear();
                SelectedROIsList.Add(roi);

                if (current == roi)
                {
                    //check is there any file info
                    TifFileInfo fi = null;
                    try
                    {
                        fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    }
                    catch { return; }
                    if (fi == null) return;

                    FillRoiManagerList(fi);
                    return;
                }
                current = roi;
            }
        }
        private void roiTV_selectedNodeChange(object sender, EventArgs e)
        {
            selectedRoiChanged((ROI)roiTV.SelectedNode);
            IA.ReloadImages();
        }
        public void DeleteBtn_Click(object sender, EventArgs e)
        {
            //check for current roi
            if (current == null) return;
            //check is there any file info
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            foreach (ROI roi in IA.RoiMan.SelectedROIsList)
            {
                current = roi;
                //check is current roi in the list
                if (fi.roiList[fi.cValue] != null)
                    if (fi.roiList[fi.cValue].IndexOf(current) > -1)
                        fi.roiList[fi.cValue].Remove(current);
                    else return;

                if (roiTV.Nodes.IndexOf(current) > -1)
                    roiTV.Nodes.Remove(current);

                #region History
                addToHistoryOldInfo(roi_new(fi.cValue, current), fi);
                addToHistoryNewInfo(roi_delete(fi.cValue, current.getID), fi);
                #endregion History
            }

            IA.RoiMan.SelectedROIsList.Clear();
            current = null;
            //reload to screen
            IA.ReloadImages();
        }
        private void roiTV_CheckNode(object sender, EventArgs e)
        {
            IA.ReloadImages();
        }
        private void OptionsPanel_Add()
        {

            Panel p = InsidePanel;
            p.Dock = DockStyle.Fill;
            p.Height = 5;
            panel.Controls.Add(p);
            p.BringToFront();

            GroupBox gb = new GroupBox();
            gb.Text = "Options:";
            gb.Dock = DockStyle.Bottom;
            gb.Height = 200;
            gb.ForeColor = IA.FileBrowser.ShriftColor1;
            // p.Controls.Add(gb);
            panel.Controls.Add(gb);
            //gb.BringToFront();

            //add variable controls

            RoiName.Text = "Name: ROI 1";
            RoiName.Width = TextRenderer.MeasureText(RoiName.Text, RoiName.Font).Width;
            RoiName.Location = new Point(5, 20);
            gb.Controls.Add(RoiName);

            Label RoiType = new Label();
            RoiTypeL = RoiType;
            RoiType.Text = "Type: Tracking Oval";
            RoiType.Width = TextRenderer.MeasureText(RoiType.Text, RoiType.Font).Width;
            RoiType.Location = new Point(5, 45);
            gb.Controls.Add(RoiType);

            x_tb = CTTextBox_Add(5, 70, gb, "X:", "X location");
            x_tb.Value.Changed += x_tb_textChanged;
            y_tb = CTTextBox_Add(5, 95, gb, "Y:", "Y location");
            y_tb.Value.Changed += y_tb_textChanged;
            startT_tb = CTTextBox_Add(5, 120, gb, "from T:", "The first time frame of which ROI is avaliable");
            startT_tb.Value.Changed += fromT_tb_textChanged;
            startZ_tb = CTTextBox_Add(5, 145, gb, "from Z:", "The first Z frame of which ROI is avaliable");
            startZ_tb.Value.Changed += fromZ_tb_textChanged;
            n_tb = CTTextBox_Add(5, 170, gb, "Stack:", "Number of layers in ROI stack");
            n_tb.Value.Changed += n_tb_textChanged;

            w_tb = CTTextBox_Add(125, 70, gb, "W:", "Width");
            w_tb.Value.Changed += w_tb_textChanged;
            h_tb = CTTextBox_Add(125, 95, gb, "H:", "Height");
            h_tb.Value.Changed += h_tb_textChanged;
            finishT_tb = CTTextBox_Add(125, 120, gb, "to T:", "The last time frame of which ROI is avaliable");
            finishT_tb.Value.Changed += ToT_tb_textChanged;
            finishZ_tb = CTTextBox_Add(125, 145, gb, "to Z:", "The last Z frame of which ROI is avaliable");
            finishZ_tb.Value.Changed += ToZ_tb_textChanged;
            d_tb = CTTextBox_Add(125, 170, gb, "D:", "Width of layer");
            d_tb.Value.Changed += d_tb_textChanged;
            //Roi List Control
            TreeView tv = new TreeView();
            tv.Dock = DockStyle.Fill;
            tv.BackColor = IA.FileBrowser.BackGround2Color1;
            tv.ForeColor = IA.FileBrowser.ShriftColor1;
            tv.BorderStyle = BorderStyle.None;
            tv.Scrollable = true;
            tv.ShowPlusMinus = false;
            tv.ShowRootLines = false;
            tv.CheckBoxes = true;
            p.Controls.Add(tv);
            roiTV = tv;
            roiTV.BringToFront();

            ImageList il = new ImageList();
            il.Images.Add(ExtendedBitmap(Properties.Resources.Rectangle_1));
            il.Images.Add(ExtendedBitmap(Properties.Resources.Circle));
            il.Images.Add(ExtendedBitmap(Properties.Resources.Polygon));
            il.Images.Add(ExtendedBitmap(Properties.Resources.freeselection_1));
            tv.ImageList = il;
            //select node event
            tv.AfterSelect += roiTV_selectedNodeChange;
            tv.AfterCheck += roiTV_CheckNode;
        }
        private Bitmap ExtendedBitmap(Bitmap source)
        {
            Image i = source;
            Bitmap b = new Bitmap(i.Width + 4, i.Height + 4);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(
                    new SolidBrush(IA.FileBrowser.TaskBtnClickColor1),
                    new Rectangle(0, 0, b.Width, b.Height));
                g.DrawImage(i, 1, 1, i.Width, i.Height);
            }

            return b;
        }

        private void x_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            if (current.Shape != 0 & current.Shape != 1) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "Location"), fi);
            //addToHistoryNewInfo(current.getStatus("Check"), fi);
            #endregion History

            Point[] points = current.GetLocation(frame);
            points[0].X = val;
            current.SetLocation(frame, points);

            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "Location"), fi);

            RoiMeasure.Measure(current, fi, fi.cValue, IA);
            IA.ReloadImages();
        }
        private void y_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            if (current.Shape != 0 & current.Shape != 1) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "Location"), fi);
            //addToHistoryNewInfo(current.getStatus("Check"), fi);
            #endregion History

            Point[] points = current.GetLocation(frame);
            points[0].Y = val;
            current.SetLocation(frame, points);

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "Location"), fi);
            #endregion History

            RoiMeasure.Measure(current, fi, fi.cValue, IA);

            IA.ReloadImages();
        }
        private void w_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            if (current.Shape != 0 & current.Shape != 1) return;
            int val = int.Parse(e.Value);
            val -= 1;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (current.Type == 1)
            {
                int pXchange = val - (current.Width-1)/ 2;
                
                FrameCalculator FC = new FrameCalculator();
                int frame = FC.Frame(fi);

                ResizeTrackingOvalAndRectangle(frame, fi, pXchange, 0);
                val += (val + 2);

                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryOldInfo(roi_getStat(current, fi, "Location"), fi);
                //addToHistoryNewInfo(current.getStatus("Check"), fi);
                #endregion History

                Point[] points = current.GetLocation(frame);
                points[0].X -= pXchange;
                current.SetLocation(frame, points);

                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryNewInfo(roi_getStat(current, fi, "Location"), fi);
                #endregion History
            }
            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "W"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History
            current.Width = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "W"), fi);
            #endregion History

            RoiMeasure.Measure(current, fi, fi.cValue, IA);
            IA.ReloadImages();
        }
        private void h_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            if (current.Shape != 0 & current.Shape != 1) return;

            int val = int.Parse(e.Value);
            val -= 1;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            if (current.Type == 1)
            {
                int pYchange = val - (current.Height-1) / 2;

                FrameCalculator FC = new FrameCalculator();
                int frame = FC.Frame(fi);

                ResizeTrackingOvalAndRectangle(frame, fi, 0, pYchange);
                val += (val + 2);

                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryOldInfo(roi_getStat(current, fi, "Location"), fi);
                //addToHistoryNewInfo(current.getStatus("H"), fi);
                #endregion History

                Point[] points = current.GetLocation(frame);
                points[0].Y -= pYchange;
                current.SetLocation(frame, points);

                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryNewInfo(roi_getStat(current, fi, "Location"), fi);
                #endregion History
            }
            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "H"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History
            
            current.Height = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "H"), fi);
            #endregion History

            RoiMeasure.Measure(current, fi, fi.cValue, IA);
            IA.ReloadImages();
        }
        private void fromT_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (val < 1) val = 1;
            if (val > fi.sizeT) val = fi.sizeT;
            if (val > current.ToT) val = current.ToT;
            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "fromT"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History
            current.FromT = val;
            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "fromT"), fi);
            #endregion History
            IA.ReloadImages();
        }
        private void ToT_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (val < 1) val = 1;
            if (val > fi.sizeT) val = fi.sizeT;
            if (val < current.FromT) val = current.FromT;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "toT"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History

            current.ToT = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "toT"), fi);
            #endregion History

            IA.ReloadImages();
        }
        private void fromZ_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (val < 1) val = 1;
            if (val > fi.sizeZ) val = fi.sizeZ;
            if (val > current.ToZ) val = current.ToZ;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "fromZ"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History

            current.FromZ = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "fromZ"), fi);
            #endregion History

            IA.ReloadImages();
        }
        private void ToZ_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;

            int val = int.Parse(e.Value);

            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (val < 1) val = 1;
            if (val > fi.sizeZ) val = fi.sizeZ;
            if (val < current.FromZ) val = current.FromZ;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "toZ"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History

            current.ToZ = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "toZ"), fi);
            #endregion History

            IA.ReloadImages();
        }
        private void n_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            //if (current.Shape != 0 & current.Shape != 1) return;

            int val = int.Parse(e.Value);

            if (val < 0)
                MessageBox.Show("Value must be positive!");
            else
            {
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }
                if (fi == null) return;
                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryOldInfo(roi_getStat(current, fi, "Stack"), fi);
                //addToHistoryNewInfo(current.getStatus("W"), fi);
                #endregion History

                current.Stack = val;

                #region History
                if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    addToHistoryNewInfo(roi_getStat(current, fi, "Stack"), fi);
                #endregion History

                RoiMeasure.Measure(current, fi, fi.cValue, IA);
            }
            IA.ReloadImages();
        }
        private void d_tb_textChanged(object sender, ChangeValueEventArgs e)
        {
            if (current == null) return;
            //if (current.Shape != 0 & current.Shape != 1) return;

            int val = int.Parse(e.Value);
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryOldInfo(roi_getStat(current, fi, "D"), fi);
            //addToHistoryNewInfo(current.getStatus("W"), fi);
            #endregion History

            current.D = val;

            #region History
            if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                addToHistoryNewInfo(roi_getStat(current, fi, "D"), fi);
            #endregion History

            RoiMeasure.Measure(current, fi, fi.cValue, IA);
            IA.ReloadImages();
        }
        private void clear_ROI_selection()
        {
            x_tb.Disable();
            y_tb.Disable();
            w_tb.Disable();
            h_tb.Disable();
            d_tb.Disable();
            n_tb.Disable();
            startT_tb.Disable();
            finishT_tb.Disable();
            startZ_tb.Disable();
            finishZ_tb.Disable();

            RoiName.Text = "Name: ";
            RoiTypeL.Text = "Type: ";
        }
        public void fillTextBox(TifFileInfo fi)
        {
            if (current == null)
            {
                this.clear_ROI_selection();
            }
            else
            {
                //check the name          
                RoiName.Text = "Name: " + current.Name;
                //check the type
                string str = "Tracking ";
                if (current.Type == 0) str = "Static ";
                switch (current.Shape)
                {
                    default:
                        str += "Rectangle";
                        break;
                    case 1:
                        str += "Oval";
                        break;
                    case 2:
                        str += "Polygon";
                        break;
                    case 3:
                        str += "Freehand selection";
                        break;
                }

                if (RoiTypeL.Text != "Type: " + str)
                {
                    RoiTypeL.Text = "Type: " + str;
                    RoiTypeL.Width = TextRenderer.MeasureText(RoiTypeL.Text, RoiTypeL.Font).Width;
                }


                FrameCalculator FC = new FrameCalculator();
                int frame = FC.Frame(fi);

                if (current.Shape == 0 | current.Shape == 1)
                {
                    Point[] points = current.GetLocation(frame);
                    if (points == null)
                    {
                        this.clear_ROI_selection();
                        return;
                    }
                    x_tb.Enable();
                    y_tb.Enable();
                    w_tb.Enable();
                    h_tb.Enable();
                    x_tb.SetValue(points[0].X.ToString());
                    y_tb.SetValue(points[0].Y.ToString());
                    if (current.Type == 0)
                    {
                        w_tb.label.Text = "W:";
                        w_tb.label.Tag = "Width";
                        w_tb.SetValue((current.Width+1).ToString());
                        h_tb.label.Text = "H:";
                        h_tb.label.Tag = "Height";
                        h_tb.SetValue((current.Height+1).ToString());
                    }
                    else if (current.Type == 1)
                    {
                        w_tb.label.Text = "Rx:";
                        w_tb.label.Tag = "X axis radius";
                        w_tb.SetValue((1+(current.Width - 1) / 2).ToString());
                        h_tb.label.Text = "Ry:";
                        h_tb.label.Tag = "Y axis radius";
                        h_tb.SetValue((1+(current.Height - 1) / 2).ToString());
                    }
                }
                else
                {
                    x_tb.Disable();
                    y_tb.Disable();
                    w_tb.Disable();
                    h_tb.Disable();
                }

                if (current.turnOnStackRoi == true)
                {
                    d_tb.Enable();
                    d_tb.SetValue(current.D.ToString());
                    n_tb.Enable();
                    n_tb.SetValue(current.Stack.ToString());
                }
                else
                {
                    d_tb.Disable();
                    n_tb.Disable();
                }

                if (fi.sizeT > 1)
                {
                    startT_tb.Enable();
                    startT_tb.SetValue(current.FromT.ToString());
                    finishT_tb.Enable();
                    finishT_tb.SetValue(current.ToT.ToString());
                }
                else
                {
                    startT_tb.Disable();
                    finishT_tb.Disable();
                }

                if (fi.sizeZ > 1)
                {
                    startZ_tb.Enable();
                    startZ_tb.SetValue(current.FromZ.ToString());
                    finishZ_tb.Enable();
                    finishZ_tb.SetValue(current.ToZ.ToString());
                }
                else
                {
                    startZ_tb.Disable();
                    finishZ_tb.Disable();
                }
            }
        }
        private CTTextBox CTTextBox_Add(int X, int Y, GroupBox gb, string title, string tag)
        {
            /*
           CTTextBox proba = new CTTextBox();
           proba.SetValue("0");
           proba.panel.Location = new Point(40, 40);
           gb.Controls.Add(proba.panel);
           proba.Value.Changed += new ChangedValueEventHandler(
               delegate (object o, ChangeValueEventArgs e)
               {
                   MessageBox.Show(e.Value);
               });
               */

            Label lb = new Label();
            lb.Text = title;
            lb.Tag = tag;
            lb.Width = 40;
            lb.Location = new Point(X, Y + 3);
            gb.Controls.Add(lb);
            lb.MouseHover += Control_MouseOver;

            CTTextBox proba = new CTTextBox();
            proba.label = lb;
            proba.SetValue("0");
            proba.panel.Location = new Point(X + lb.Width, Y);
            proba.panel.Width = 80;
            gb.Controls.Add(proba.panel);

            return proba;
        }
        #endregion Initialize

        #region Create current row
        public bool DrawNewRoiMode = false;
        public Point lastPoint = new Point(0, 0);
        private Point IsPointInAnyImage(TifFileInfo fi, MouseEventArgs e)
        {
            double zoom = fi.zoom;
            double X1 = e.X / zoom - IA.IDrawer.valX;
            double Y1 = e.Y / zoom - IA.IDrawer.valY;
            int X = Convert.ToInt32(X1);
            if (X > X1) { X--; }
            int Y = Convert.ToInt32(Y1);
            if (Y > Y1) { Y--; }

            Point p = new Point(X, Y);

            for (int i = 0; i < fi.sizeC; i++)
            {
                if (IA.IDrawer.coRect != null && IA.IDrawer.coRect[0] != null &&
                    (IA.IDrawer.coRect[0][i].Contains(p) == true &
                    fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0) |
                    (IA.IDrawer.coRect[1][i].Contains(p) == true &
                    fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0))
                {
                    p.X -= IA.IDrawer.coRect[0][i].X;
                    p.Y -= IA.IDrawer.coRect[0][i].Y;
                    return p;
                }
            }

            p.X = -10000;
            p.Y = -10000;
            return p;

        }
        private PointF IsPointFInImage(TifFileInfo fi, MouseEventArgs e)
        {
            double zoom = fi.zoom;
            double X1 = e.X / zoom - IA.IDrawer.valX;
            double Y1 = e.Y / zoom - IA.IDrawer.valY;
            int X = Convert.ToInt32(X1);
            if (X > X1) { X--; }
            int Y = Convert.ToInt32(Y1);
            if (Y > Y1) { Y--; }

            Point p = new Point(X, Y);

            if (IA.IDrawer.coRect != null && IA.IDrawer.coRect[0] != null &&
                IA.IDrawer.coRect[0][fi.cValue].Contains(p) == true &&
                fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
            {
                X1 -= IA.IDrawer.coRect[0][fi.cValue].X;
                Y1 -= IA.IDrawer.coRect[0][fi.cValue].Y;
            }
            else
            {
                X1 = -10000;
                Y1 = -10000;
            }

            return new PointF((float)X1, (float)Y1);
        }
        private Point IsPointInImage(TifFileInfo fi, MouseEventArgs e)
        {
            double zoom = fi.zoom;
            double X1 = e.X / zoom - IA.IDrawer.valX;
            double Y1 = e.Y / zoom - IA.IDrawer.valY;
            int X = Convert.ToInt32(X1);
            if (X > X1) { X--; }
            int Y = Convert.ToInt32(Y1);
            if (Y > Y1) { Y--; }

            Point p = new Point(X, Y);


            if (IA.IDrawer.coRect[0][fi.cValue].Contains(p) == true &
                fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
            {
                p.X -= IA.IDrawer.coRect[0][fi.cValue].X;
                p.Y -= IA.IDrawer.coRect[0][fi.cValue].Y;
            }
            else
            {
                p.X = -10000;
                p.Y = -10000;
            }

            return p;
        }
        private Point CalculatePoint(TifFileInfo fi, MouseEventArgs e)
        {
            double zoom = fi.zoom;
            double X1 = e.X / zoom - IA.IDrawer.valX;
            double Y1 = e.Y / zoom - IA.IDrawer.valY;
            X1 -= IA.IDrawer.coRect[0][fi.cValue].X;
            Y1 -= IA.IDrawer.coRect[0][fi.cValue].Y;
            int X = Convert.ToInt32(X1);
            if (X > X1) { X--; }
            int Y = Convert.ToInt32(Y1);
            if (Y > Y1) { Y--; }

            Point p = new Point(X, Y);

            return p;
        }

        private ROI CacheROI = null;
        private int CacheType = 0;
        private int CacheShape = 0;
        public void GLControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (DrawNewRoiMode == true | MoveCurrentRoi == true
                | activResizeCurrent == true) return;
            //Return if there is no opened image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;
            if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 1) return;
            if (fi.selectedPictureBoxColumn != 0)
            {
                return;
            }

            if (current != null && Control.ModifierKeys == Keys.Alt && current.Type == 1 && current.Shape == 3)
            {
                CacheROI = current;
                current.Checked = false;
                current = null;
                CacheType = RoiType;
                CacheShape = RoiShape;
                RoiType = 0;
                RoiShape = 3;
            }
            else
            {
                CacheROI = null;
            }

            //calculate the point
            Point p = IsPointInImage(fi, e);
            PointF pF = IsPointFInImage(fi, e);
            pF.X -= 0.5f;
            pF.Y -= 0.5f;

            if (p.X == -10000 | p.Y == -10000) return;

            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);

            bool dontResize = GLControl_MouseClick_ChangeRoi(fi, pF);

            if (current != null && current.Checked == true)
            {
                if (dontResize == false)
                    if (ResizeCurrent_mouseDown(fi, p, pF, frame)) return;

                if (MoveCurrent_mouseDown(fi, p, frame)) return;
            }


            if (RoiType != 0) return;

            if (RoiShape == 2 & current != null)
            {
                current = null;
                IA.IDrawer.DrawToScreen();
                return;
            }
            //create new current roi

            current = new ROI(fi.ROICounter, fi.imageCount, RoiShape, RoiType, turnOnStackRoi);
            fi.ROICounter++;

            current.FromT = 1;
            current.FromZ = 1;
            current.ToT = fi.sizeT;
            current.ToZ = fi.sizeZ;

            if (turnOnStackRoi == true)
            {
                current.Stack = 1;
                current.D = 3;
            }

            current.SetLocation(frame, new Point[] { p });

            lastPoint = p;
            DrawNewRoiMode = true;
            //Clear selected roi list
            SelectedROIsList.Clear();
            FillRoiManagerList(fi);
            //Redraw
            IA.IDrawer.DrawToScreen();
            IA.IDrawer.BindTexture(fi);
        }
        public void GLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (DrawNewRoiMode == false & MoveCurrentRoi == false & activResizeCurrent == false) return;
            if (current == null)
            {
                DrawNewRoiMode = false;
                MoveCurrentRoi = false;
                activResizeCurrent = false;
                return;
            }
            //Return if there is no opened image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            //calculate frame
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            //Check is resizing on
            if (ResizeCurrent_mouseMove(fi, CalculatePoint(fi, e), frame)) return;
            //Check is current roi moving
            if (MoveCurrent_mouseMove(fi, CalculatePoint(fi, e), frame)) return;

            //calculate the point
            Point p = IsPointInImage(fi, e);
            if (p.X == -10000 | p.Y == -10000) return;

            if (current.Shape == 0 | current.Shape == 1)//rectangle and oval
            {
                //apply changes
                Point Location = current.GetLocation(frame)[0];
                int newX = p.X - Location.X;
                int newY = p.Y - Location.Y;
                if (newX > 0) current.Width = newX;
                if (newY > 0) current.Height = newY;

            }
            else if (current.Shape == 2)
            {
                lastPoint = p;
            }
            else if (current.Shape == 3)//freehand
            {
                //apply changes
                Point[] points = current.GetLocation(frame);
                int length = points.Length;
                Array.Resize(ref points, length + 1);
                points[length] = p;

                current.SetLocation(frame, points);
            }

            IA.IDrawer.tryDrawingWithoutReload();

        }
        public void GLControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (DrawNewRoiMode == false & MoveCurrentRoi == false & activResizeCurrent == false) return;

            if (current == null)
            {
                DrawNewRoiMode = false;
                MoveCurrentRoi = false;
                activResizeCurrent = false;
                return;
            }
            if (ResizeCurrent_mouseUp()) return;
            if (MoveCurrent_mouseUp(sender, e)) return;

            //Return if there is no opened image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;
            //calculate frame
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            //Restore settings for free hande roi resize
            if (CacheROI != null)
            {
                Point[] points = current.GetLocation(frame);

                if (points.Length > 2)
                {
                    #region History
                    if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(CacheROI) > -1)
                        addToHistoryOldInfo(roi_getStat(CacheROI, fi, "Location"), fi);
                    #endregion History

                    CacheROI.SetLocation(frame, points);

                    if (fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(CacheROI) > -1)
                        addToHistoryNewInfo(roi_getStat(CacheROI, fi, "Location"), fi);
                }

                RoiType = CacheType;
                RoiShape = CacheShape;
                current = CacheROI;
                CacheROI = null;
                DrawNewRoiMode = false;
                current.Checked = true;
                RoiMeasure.Measure(current, fi, fi.cValue, IA);
                IA.ReloadImages();
                return;
            }

            if (current.Shape == 0 | current.Shape == 1)//rectangle, oval
            {
                Point[] points = current.GetLocation(frame);

                if (current.Width < 0)
                {
                    int buf = Math.Abs(current.Width);
                    current.Width = buf;
                    points[0].X -= buf;
                }

                if (current.Height < 0)
                {
                    int buf = Math.Abs(current.Height);
                    current.Height = buf;
                    points[0].Y -= buf;
                }

                current.SetLocation(frame, points);

                //delete if the roi is too small
                if (current.Width < 1 | current.Width < 1) current = null;
                //refresh screen
                DrawNewRoiMode = false;
                IA.ReloadImages();
            }
            else if (current.Shape == 3)//freehand
            {
                Point[] points = current.GetLocation(frame);
                //delete if the roi is too small
                if (points.Length < 2) current = null;
                //refresh screen

                DrawNewRoiMode = false;
                IA.ReloadImages();
            }
            else if (current.Shape == 2)//polygon
            {
                //calculate the point
                Point p = IsPointInImage(fi, e);
                if (p.X == -10000 | p.Y == -10000) return;

                Point[] points = current.GetLocation(frame);
                //exit if in range of the first one
                float lineWidth = (float)(3 / fi.zoom);
                if (lineWidth < 1f) { lineWidth = 1f; }

                RectangleF rect = new RectangleF(
                    (points[0].X - lineWidth),
                    (points[0].Y - lineWidth),
                    (lineWidth + lineWidth),
                    (lineWidth + lineWidth));

                if (rect.Contains(p) & points.Length > 2)//polygon ready
                {
                    DrawNewRoiMode = false;
                    if (points.Length > 3)
                    {
                        Point[] newPoints = new Point[points.Length - 1];
                        for (int z = 0; z < newPoints.Length; z++)
                        {
                            newPoints[z].X = points[z + 1].X;
                            newPoints[z].Y = points[z + 1].Y;
                        }

                        current.SetLocation(frame, newPoints);
                    }
                    else current = null;

                    IA.ReloadImages();
                }
                else
                {
                    //apply changes
                    int length = points.Length;
                    Array.Resize(ref points, length + 1);
                    if (length == 1)
                    {
                        points[1].X = points[0].X;
                        points[1].Y = points[0].Y;
                    }
                    else
                    {
                        points[length].X = p.X;
                        points[length].Y = p.Y;
                    }

                    current.SetLocation(frame, points);


                    IA.IDrawer.tryDrawingWithoutReload();
                }
            }

        }
        #endregion Create current roi

        #region Move current roi
        Point oldPoint;
        public bool MoveCurrentRoi = false;
        private bool MoveCurrent_mouseDown(TifFileInfo fi, Point p, int frame)
        {
            if (current == null)
            {
                MoveCurrentRoi = false;
                return false;
            }
            Point[] points = current.GetLocation(frame);
            switch (current.Shape)
            {
                case 0:
                    MoveCurrentRoi = IsPointInRectangle(p, new Rectangle(
                        points[0].X, points[0].Y, current.Width, current.Height));
                    break;
                case 1:
                    MoveCurrentRoi = IsPointInOval(p, new Rectangle(
                       points[0].X, points[0].Y, current.Width, current.Height));
                    break;
                case 2:
                    MoveCurrentRoi = IsPointInPolygon(p, points);
                    break;
                case 3:
                    MoveCurrentRoi = IsPointInPolygon(p, points);
                    break;
            }

            oldPoint = p;

            if (MoveCurrentRoi == true)
            {
                if (fi != null && fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    HistBuf = current.getRoiResizeToHistory(fi.cValue, frame);
                IA.GLControl1.Cursor = Cursors.Hand;
                IA.IDrawer.DrawToScreen();
                IA.IDrawer.BindTexture(fi);
            }

            return MoveCurrentRoi;
        }
        private bool MoveCurrent_mouseMove(TifFileInfo fi, Point p, int frame)
        {
            if (MoveCurrentRoi == false) return false;

            if (current.Shape == 0 | current.Shape == 1)
            {
                Point[] points = current.GetLocation(frame);

                int X = p.X - oldPoint.X;
                int Y = p.Y - oldPoint.Y;
                /*
                if (points[0].X + X>= 0 & 
                    points[0].Y +Y >= 0 &
                   points[0].X + X < fi.sizeX - current.Width &
                   points[0].Y + Y < fi.sizeY - current.Height)
                {*/
                points[0].X += X;
                points[0].Y += Y;

                current.SetLocation(frame, points);
                //}

                oldPoint = p;
            }
            else if (current.Shape == 2 | current.Shape == 3)
            {
                int X = p.X - oldPoint.X;
                int Y = p.Y - oldPoint.Y;

                Point[] points = current.GetLocation(frame);
                /*
                bool change = true;
                foreach (Point pn in points)
                    if (pn.X + X < 0 |
                        pn.Y + Y < 0 |
                        pn.X + X >= fi.sizeX - current.Width |
                        pn.Y + Y >= fi.sizeY - current.Height)
                    {
                        //change = false;
                        break;
                    }

                if (change == true)
                {
                */
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].X += X;
                    points[i].Y += Y;
                }

                current.SetLocation(frame, points);
                //}

                oldPoint = p;
            }

            IA.IDrawer.tryDrawingWithoutReload();

            return MoveCurrentRoi;
        }
        private bool MoveCurrent_mouseUp(object sender, MouseEventArgs e)
        {
            if (MoveCurrentRoi == true)
            {
                #region History
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { }
                if (fi != null && fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                {
                    FrameCalculator FC = new FrameCalculator();
                    int frame = FC.Frame(fi);
                    string newHistBuf = current.getRoiResizeToHistory(fi.cValue, frame);
                    if (newHistBuf != HistBuf)
                    {
                        addToHistoryOldInfo(HistBuf, fi);
                        addToHistoryNewInfo(newHistBuf, fi);
                    }
                }
                #endregion History

                RoiMeasure.Measure(current, fi, fi.cValue, IA);

                MoveCurrentRoi = false;
                GlControl_MouseMoveChangeCursor(sender, e);
                IA.ReloadImages();
                return true;
            }
            else
            {
                MoveCurrentRoi = false;
                return false;
            }
        }

        public bool IsPointInRectangle(Point p, Rectangle rect)
        {
            bool res = false;
            if (rect.Contains(p)) res = true;
            return res;
        }
        public bool IsPointInPolygon(Point p, Point[] polygon)
        {
            double minX = polygon[0].X;
            double maxX = polygon[0].X;
            double minY = polygon[0].Y;
            double maxY = polygon[0].Y;

            for (int i = 1; i < polygon.Length; i++)
            {
                Point q = polygon[i];
                minX = Math.Min(q.X, minX);
                maxX = Math.Max(q.X, maxX);
                minY = Math.Min(q.Y, minY);
                maxY = Math.Max(q.Y, maxY);
            }

            if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y) &&
                     p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
        public bool IsPointInOval(Point p, Rectangle rect)
        {
            bool res = false;
            //fast check
            if (rect.Contains(p) == false) return false;
            //slow check
            double Rx = rect.Width / 2;
            double Ry = rect.Height / 2;
            double X = rect.X + Rx;
            double Y = rect.Y + Ry;

            double dX = (Math.Pow((X - p.X), 2)) / (Math.Pow(Rx, 2));
            double dY = (Math.Pow((Y - p.Y), 2)) / (Math.Pow(Ry, 2));

            if ((dX + dY) <= 1) res = true;

            return res;
        }
        #endregion Move current row

        #region Calculate Resize rectangles
        public RectangleF[] ResizeSpotsRectangles;
        public void PrepareResizeSpotsRectangle(TifFileInfo fi, int frame)
        {
            PointF[] points = CalculateResizeSpots(frame);

            if (points == null)
            {
                ResizeSpotsRectangles = null;
                return;
            }

            ResizeSpotsRectangles = new RectangleF[points.Length];

            float R = (float)(5 / fi.zoom);
            float halfR = R / 2;

            for (int i = 0; i < points.Length; i++)
            {
                RectangleF rect = ResizeSpotsRectangles[i];
                rect.X = points[i].X - halfR;
                rect.Y = points[i].Y - halfR;
                rect.Width = R;
                rect.Height = R;
                ResizeSpotsRectangles[i] = rect;
            }

        }
        private PointF[] CalculateResizeSpots(int frame)
        {
            if (current == null) return null;
            if (current.Checked == false) return null;

            Point[] points = current.GetLocation(frame);
            PointF[] ResizeSpots;
            switch (current.Shape)
            {
                case 0:
                    ResizeSpots = calculateRectangleResizeSpots(points);
                    break;
                case 1:
                    ResizeSpots = calculateOvalResizeSpots(points);
                    break;
                case 2:
                    ResizeSpots = calculatePolygonResizeSpots(points);
                    break;
                default:
                    ResizeSpots = null;
                    break;
            }

            return ResizeSpots;
        }
        private PointF[] calculateRectangleResizeSpots(Point[] points)
        {
            PointF[] ResizeSpots = new PointF[8];

            float X = (float)points[0].X;
            float Y = (float)points[0].Y;
            float W = (float)current.Width + X;
            float H = (float)current.Height + Y;

            float halfW = (float)(current.Width / 2 + X);
            float halfH = (float)(current.Height / 2 + Y);

            ///Matrix
            /// 0   1   2
            /// 7   -   3
            /// 6   5   4
            ///

            ResizeSpots[0].X = X;
            ResizeSpots[0].Y = Y;

            ResizeSpots[1].X = halfW;
            ResizeSpots[1].Y = Y;

            ResizeSpots[2].X = W;
            ResizeSpots[2].Y = Y;

            ResizeSpots[3].X = W;
            ResizeSpots[3].Y = halfH;

            ResizeSpots[4].X = W;
            ResizeSpots[4].Y = H;

            ResizeSpots[5].X = halfW;
            ResizeSpots[5].Y = H;

            ResizeSpots[6].X = X;
            ResizeSpots[6].Y = H;

            ResizeSpots[7].X = X;
            ResizeSpots[7].Y = halfH;

            return ResizeSpots;
        }
        private PointF[] calculateOvalResizeSpots(Point[] points)
        {
            PointF[] ResizeSpots = new PointF[8];

            float X = (float)points[0].X;
            float Y = (float)points[0].Y;
            float W = (float)current.Width;
            float H = (float)current.Height;

            List<float> Xarr = new List<float>();
            List<float> Yarr = new List<float>();
            //calculate dots
            {
                float xradius = W / 2;
                float yradius = H / 2;

                float x = X + xradius;
                float y = Y + yradius;

                for (int i = 0; i < 360; i += 45)
                {
                    //convert degrees into radians
                    float degInRad = i * DEG2RAD;
                    double newX = x + Math.Cos(degInRad) * xradius;
                    double newY = y + Math.Sin(degInRad) * yradius;
                    Xarr.Add((float)newX);
                    Yarr.Add((float)newY);
                }
            }
            ///Matrix
            /// 0   1   2
            /// 7   -   3
            /// 6   5   4
            ///

            ResizeSpots[0].X = Xarr[5];
            ResizeSpots[0].Y = Yarr[5];
            ResizeSpots[1].X = Xarr[6];
            ResizeSpots[1].Y = Yarr[6];
            ResizeSpots[2].X = Xarr[7];
            ResizeSpots[2].Y = Yarr[7];
            ResizeSpots[3].X = Xarr[0];
            ResizeSpots[3].Y = Yarr[0];
            ResizeSpots[4].X = Xarr[1];
            ResizeSpots[4].Y = Yarr[1];
            ResizeSpots[5].X = Xarr[2];
            ResizeSpots[5].Y = Yarr[2];
            ResizeSpots[6].X = Xarr[3];
            ResizeSpots[6].Y = Yarr[3];
            ResizeSpots[7].X = Xarr[4];
            ResizeSpots[7].Y = Yarr[4];

            return ResizeSpots;
        }
        private PointF[] calculatePolygonResizeSpots(Point[] points)
        {
            PointF[] ResizeSpots = new PointF[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                ResizeSpots[i].X = (float)points[i].X;
                ResizeSpots[i].Y = (float)points[i].Y;
            }

            return ResizeSpots;
        }
        #endregion Calculate Resize rectangles

        #region Roi resize events
        int PointIndex = 0;
        bool activResizeCurrent = false;
        Point ResizeCurrentPoint = Point.Empty;
        private bool ResizeCurrent_mouseDown(TifFileInfo fi, Point p, PointF pF, int frame)
        {
            //Check
            if (current == null)
            {
                activResizeCurrent = false;
                return false;
            }

            if (current.Checked == false)
            {
                activResizeCurrent = false;
                return false;
            }

            if (activResizeCurrent == true) return true;
            if (ResizeSpotsRectangles == null) return false;

            for (int i = 0; i < ResizeSpotsRectangles.Length; i++)
            {
                if (ResizeSpotsRectangles[i].Contains(pF))
                {
                    PointIndex = i;
                    ResizeCurrentPoint = p;
                    activResizeCurrent = true;
                    break;
                }
            }

            if (activResizeCurrent == true)
            {
                if (fi != null && fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                    HistBuf = current.getRoiResizeToHistory(fi.cValue, frame);
                IA.IDrawer.DrawToScreen();
                IA.IDrawer.BindTexture(fi);
            }

            return activResizeCurrent;
        }
        private bool ResizeCurrent_mouseMove(TifFileInfo fi, Point p, int frame)
        {
            //Check
            if (current == null)
            {
                activResizeCurrent = false;
                return false;
            }

            if (activResizeCurrent == false) return false;
            //Resize Event
            switch (current.Shape)
            {
                case 0:
                    resizeOvalAndRect(p, frame, fi);
                    break;
                case 1:
                    resizeOvalAndRect(p, frame, fi);
                    break;
                case 2:
                    resizePolygon(p, frame);
                    break;
                default:
                    break;
            }
            ResizeCurrentPoint = p;
            //end event
            IA.IDrawer.tryDrawingWithoutReload();
            return activResizeCurrent;
        }
        private void resizeOvalAndRect(Point p, int frame, TifFileInfo fi)
        {
            Point[] points = current.GetLocation(frame);
            int dX = 0;
            int dY = 0;
            if (current.Shape == 1)
            {
                PointF[] RectPoints = CalculateResizeSpots(frame);
                dX = (int)RectPoints[0].X - points[0].X;
                dY = (int)RectPoints[0].Y - points[0].Y;
            }

            int pXchange = 0;
            int pYchange = 0;
            int delt = 0;

            switch (PointIndex)
            {
                case 0:
                    if (p.X - dX < current.Width + points[0].X &
                        p.X - dX != points[0].X)
                    {
                        current.Width += (points[0].X - p.X + dX);
                        if (current.Type == 1) current.Width += (points[0].X - p.X + dX);
                        delt = p.X - dX;
                        pXchange = points[0].X - delt;
                        points[0].X = delt;
                    }

                    if (p.Y - dY < current.Height + points[0].Y &
                        p.Y - dY != points[0].Y)
                    {
                        current.Height += (points[0].Y - p.Y + dY);
                        if (current.Type == 1) current.Height += (points[0].Y - p.Y + dY);
                        delt = p.Y - dY;
                        pYchange = points[0].Y - delt;
                        points[0].Y = delt;
                    }
                    break;
                case 1:
                    if (p.Y < current.Height + points[0].Y &
                       p.Y != points[0].Y)
                    {
                        current.Height += (points[0].Y - p.Y);
                        if (current.Type == 1) current.Height += (points[0].Y - p.Y);
                        pYchange = points[0].Y - p.Y;
                        points[0].Y = p.Y;
                    }
                    break;
                case 2:
                    if (p.Y - dY < current.Height + points[0].Y &
                         p.Y - dY != points[0].Y)
                    {
                        current.Height += (points[0].Y - p.Y + dY);
                        if (current.Type == 1) current.Height += (points[0].Y - p.Y + dY);
                        delt = p.Y - dY;
                        pYchange = points[0].Y - delt;
                        points[0].Y = delt;
                    }

                    if (p.X + dX > points[0].X)
                    {
                        int dif = -current.Width + (p.X + dX - points[0].X);
                        current.Width += dif;

                        if (current.Type == 1)
                        {
                            pXchange = dif;
                            points[0].X -= dif;
                            current.Width += dif;
                        }

                    }

                    break;
                case 3:
                    if (p.X > points[0].X)
                    {
                        int dif = -current.Width + (p.X - points[0].X);
                        current.Width += dif;

                        if (current.Type == 1)
                        {
                            pXchange = dif;
                            points[0].X -= dif;
                            current.Width += dif;
                        }

                    }
                    break;
                case 4:
                    if (p.X + dX > points[0].X)
                    {
                        int dif = -current.Width + (p.X + dX - points[0].X);
                        current.Width += dif;

                        if (current.Type == 1)
                        {
                            pXchange = dif;
                            points[0].X -= dif;
                            current.Width += dif;
                        }

                    }

                    if (p.Y + dY > points[0].Y)
                    {
                        int dif = (p.Y + dY - points[0].Y) - current.Height;
                        current.Height += dif;
                        if (current.Type == 1)
                        {
                            pYchange = dif;
                            points[0].Y -= dif;
                            current.Height += dif;
                        }
                    }
                    break;
                case 5:

                    if (p.Y > points[0].Y)
                    {
                        int dif = (p.Y - points[0].Y) - current.Height;
                        current.Height += dif;
                        if (current.Type == 1)
                        {
                            pYchange = dif;
                            points[0].Y -= dif;
                            current.Height += dif;
                        }
                    }
                    break;
                case 6:
                    if (p.X - dX < current.Width + points[0].X &
                        p.X - dX != points[0].X)
                    {
                        current.Width += (points[0].X - p.X + dX);
                        if (current.Type == 1) current.Width += (points[0].X - p.X + dX);
                        delt = p.X - dX;
                        pXchange = points[0].X - delt;
                        points[0].X = delt;
                    }

                    if (p.Y + dY > points[0].Y)
                    {
                        int dif = (p.Y + dY - points[0].Y) - current.Height;
                        current.Height += dif;
                        if (current.Type == 1)
                        {
                            pYchange = dif;
                            points[0].Y -= dif;
                            current.Height += dif;
                        }
                    }
                    break;
                case 7:
                    if (p.X < current.Width + points[0].X &
                         p.X != points[0].X)
                    {
                        current.Width += (points[0].X - p.X);
                        if (current.Type == 1) current.Width += (points[0].X - p.X);
                        pXchange = points[0].X - p.X;
                        points[0].X = p.X;
                    }
                    break;

            }
            current.SetLocation(frame, points);

            //for tracking
            ResizeTrackingOvalAndRectangle(frame, fi, pXchange, pYchange);
        }
        public void ResizeTrackingOvalAndRectangle(int frame, TifFileInfo fi, int pXchange, int pYchange)
        {
            if (current.Type == 1)
            {
                Point[] points = current.GetLocationAll()[0];

                for (int fr = fi.cValue; fr < points.Length; fr += fi.sizeC)
                {
                    if (fr == frame) continue;
                    if (points[fr] == Point.Empty) continue;

                    points[fr].X -= pXchange;
                    points[fr].Y -= pYchange;
                }
                current.SetLocationAll(new Point[][] { points });
            }
        }

        private void resizePolygon(Point p, int frame)
        {
            Point[] points = current.GetLocation(frame);

            points[PointIndex].X = p.X;
            points[PointIndex].Y = p.Y;

            current.SetLocation(frame, points);
        }
        private bool ResizeCurrent_mouseUp()
        {
            if (activResizeCurrent == true)
            {
                #region History
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { }
                if (fi != null && fi.roiList[fi.cValue] != null && fi.roiList[fi.cValue].IndexOf(current) > -1)
                {
                    FrameCalculator FC = new FrameCalculator();
                    int frame = FC.Frame(fi);
                    string newHistBuf = current.getRoiResizeToHistory(fi.cValue, frame);
                    if (newHistBuf != HistBuf)
                    {
                        addToHistoryOldInfo(HistBuf, fi);
                        addToHistoryNewInfo(newHistBuf, fi);
                    }
                }
                #endregion History

                RoiMeasure.Measure(current, fi, fi.cValue, IA);

                activResizeCurrent = false;
                ResizeCurrentPoint = Point.Empty;
                IA.ReloadImages();
                return true;
            }
            else
            {
                activResizeCurrent = false;
                ResizeCurrentPoint = Point.Empty;
                return false;
            }
        }
        #endregion Roi resize events

        #region GLControl cursor change
        public void GlControl_MouseMoveChangeCursor(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle | e.Button == MouseButtons.Left) return;

            if (DrawNewRoiMode == true | MoveCurrentRoi == true | activResizeCurrent == true) return;

            //Return if there is no opened image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            //calculate frame
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);

            //calculate the point
            PointF p = IsPointFInImage(fi, e);
            p.X -= 0.5f;
            p.Y -= 0.5f;
            Point p1 = IsPointInAnyImage(fi, e);
            if (p1.X != -10000 | p1.Y != -10000)
            {
                if (p.X == -10000 | p.Y == -10000)
                {
                    IA.GLControl1.Cursor = Cursors.Cross;
                    return;
                }
            }
            else
            {
                IA.GLControl1.Cursor = Cursors.Default;
                return;
            }

            bool check = false;

            for (int i = 0; i < RoiAccessRectList.Count; i++)
                if (fi.selectedPictureBoxColumn == 0 &&
                    RoiAccessRectList[i].Contains(p) &&
                    fi.roiList[fi.cValue][i].Checked == true)
                {
                    IA.GLControl1.Cursor = Cursors.Hand;
                    check = true;
                    break;
                }

            if (activResizeCurrent == false & ResizeSpotsRectangles != null)
                for (int i = 0; i < ResizeSpotsRectangles.Length; i++)
                    if (fi.selectedPictureBoxColumn == 0 &&
                        ResizeSpotsRectangles[i].Contains(p) &&
                        current != null &&
                        current.Checked == true)
                    {
                        if (current.Shape == 0 | current.Shape == 1)
                            IA.GLControl1.Cursor = RectAndOvalCursors(i);
                        else
                            IA.GLControl1.Cursor = Cursors.Hand;

                        check = true;
                        break;
                    }


            if (check == false)
            {
                IA.GLControl1.Cursor = Cursors.Cross;
            }
        }
        private Cursor RectAndOvalCursors(int index)
        {
            Cursor cur;
            switch (index)
            {
                default:
                    cur = Cursors.SizeNWSE;
                    break;
                case 1:
                    cur = Cursors.SizeNS;
                    break;
                case 2:
                    cur = Cursors.SizeNESW;
                    break;
                case 3:
                    cur = Cursors.SizeWE;
                    break;
                case 4:
                    cur = Cursors.SizeNWSE;
                    break;
                case 5:
                    cur = Cursors.SizeNS;
                    break;
                case 6:
                    cur = Cursors.SizeNESW;
                    break;
                case 7:
                    cur = Cursors.SizeWE;
                    break;
            }
            return cur;
        }
        #endregion GLControl cursor change

        #region GLControl activate roi
        public List<RectangleF> RoiAccessRectList = new List<RectangleF>();
        public bool GLControl_MouseClick_ChangeRoi(TifFileInfo fi, PointF pF)
        {
            for (int i = RoiAccessRectList.Count - 1; i >= 0; i--)
                if (RoiAccessRectList[i].Contains(pF))
                {
                    selectedRoiChanged(fi.roiList[fi.cValue][i]);
                    current = fi.roiList[fi.cValue][i];

                    if (current != fi.roiList[fi.cValue][i])
                        return true;
                    else
                        return false;
                }

            return false;
        }
        public void calculateRoiAccessRect(TifFileInfo fi)
        {
            RoiAccessRectList.Clear();

            if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex != 0) return;
            if (fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex != 0) return;

            FrameCalculator FC = new FrameCalculator();
            int imageN = FC.Frame(fi);

            if (IA.IDrawer.coRect == null) return;

            Rectangle Borders = Rectangle.Empty;
            try
            {
                Borders = IA.IDrawer.coRect[0][fi.cValue];
            }
            catch { return; }

            RectangleF BordersF =
               new RectangleF(0, 0, Borders.Width, Borders.Height);

            float W = 13f / (float)fi.zoom;
            float H = 15f / (float)fi.zoom;
            float lineSpace = 7 / (float)fi.zoom;

            if (fi.roiList[fi.cValue] != null)
                for (int i = 0; i < fi.roiList[fi.cValue].Count; i++)
                {
                    ROI roi = fi.roiList[fi.cValue][i];

                    string str = (i + 1).ToString();
                    int symb = str.Length;

                    PointF midP = roi.GetMidPoint(imageN);
                    float X = midP.X - (lineSpace * (symb / 2)) - 3 / (float)fi.zoom;
                    float Y = midP.Y - (H / 2);

                    RectangleF rect = new RectangleF(X, Y, 0, H);
                    RoiAccessRectList.Add(rect);

                    if (showLabels == false) continue;
                    if (BordersF.Contains(new PointF(X, Y)) == false) continue;
                    if (BordersF.Contains(new PointF(X, Y + H)) == false) continue;
                    if (roi.Checked == false) continue;

                    foreach (char val in str)
                        if (BordersF.Contains(new PointF(rect.Width + W, rect.Y)))
                            rect.Width += W;

                    RoiAccessRectList[i] = rect;
                }

        }
        #endregion GLControl activate roi
        #region History
        public void addToHistoryOldInfo(String val, TifFileInfo fi)
        {
            //prepare array and settings
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            //apply
            fi.History.Add(val);
        }
        public void addToHistoryNewInfo(String val, TifFileInfo fi)
        {
            //apply
            fi.History.Add(val);
            //finish changes
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
        }
        public string roi_delete(int C, int RoiID)
        {
            //roi.delete(chanelN, roiID)->delete roi
            return "roi.delete(" + C.ToString() + "," +
                RoiID.ToString() + ")";
        }
        public string roi_new(int C, ROI roi)
        {
            return "roi.new(" + C.ToString() + "," +
                roi.getID.ToString() + "," + roi.roi_getAllInfo() + ")";
        }
        public string roi_getStat(ROI roi, TifFileInfo fi, string stat)
        {
            FrameCalculator FC = new FrameCalculator();
            int imageN = FC.Frame(fi);

            int chanelN = fi.cValue;

            return "roi.change(" + chanelN.ToString() + "," +
                roi.getID.ToString() + "," +
                imageN.ToString() + "," +
                roi.getStatus(stat, imageN) + ")";
        }

        #endregion History

        #region Save Roi as IJ macro
        public void ExportRoiAsIJMacro(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }

            if (fi.roiList[fi.cValue] == null || fi.roiList[fi.cValue].Count == 0) return;

            string val = IA.chart.GetResults(fi, fi.cValue);

            if (val == "")
            {
                MessageBox.Show("Results can't be calculated!\n Turn on Chart control.");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            TreeNode node = IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Tag as TreeNode;

            string formatMiniStr = ".txt";
            string formatStr = "TAB delimited files (*" + formatMiniStr + ")|*" + formatMiniStr;
            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;
            //saveFileDialog1.InitialDirectory = node.Tag.ToString().Substring(0, node.Tag.ToString().Length - (node.Text.Length + 1));
            saveFileDialog1.InitialDirectory = fi.Dir.Substring(0, fi.Dir.LastIndexOf("\\"));
            saveFileDialog1.FileName = node.Text.Replace(".tif", "");
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Title = "Mesure ROIs to:";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = saveFileDialog1.FileName.Replace(".tif", "");
                if (dir.EndsWith(formatMiniStr) == false)
                    dir += formatMiniStr;

                File.WriteAllText(dir, val);
                IA.FileBrowser.Refresh_AfterSave();
            }
        }
        public void ExportAllResults(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }

            TreeNode node = IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Tag as TreeNode;

            //string dir = node.Tag.ToString().Replace(".tif", "");
            string dir = fi.Dir.Replace(".tif", "");

            //background worker

            //var bgw = new BackgroundWorker();
            //bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            //bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                for (int c = 0; c < fi.sizeC; c++)
                    if (fi.roiList[c] != null && fi.roiList[c].Count != 0)
                    {
                        string dir1 = dir + "_Ch" + c +"_"+
                        fi.LutList[c].ToString().Replace("Color [","").Replace("]","") +
                        ".txt";
                        string dir2 = dir + "_Ch" + c + "_" + 
                        fi.LutList[c].ToString().Replace("Color [", "").Replace("]", "") +
                        "_Results.txt";
                        //calculate the size of the result row
                        int resultSize = 0;
                        foreach (ROI roi in fi.roiList[c])
                            if (roi.Checked == true)
                            {
                                if (roi.Results == null) RoiMeasure.Measure(roi, fi, c, IA);
                                if (roi.Shape == 0 || roi.Shape == 1)
                                    resultSize += roi.Results[c].Length;
                                else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                    resultSize += 4 + roi.Stack * 4;

                            }
                        //chart result
                        string val = IA.chart.GetResults(fi, c);
                        if (val != "")
                        {
                            try
                            {
                                File.WriteAllText(dir1, val);
                            }
                            catch
                            {
                                MessageBox.Show("File is used by other program!");
                                //((BackgroundWorker)o).ReportProgress(1);
                            }
                        }
                        //standart results
                        if (resultSize == 0) continue;

                        {
                            //create result matrix
                            double[] result;

                            int t = 1;
                            int z = 1;
                            int position;
                            string str;

                            double time = 0;
                            int timeIndex = 0;
                            double timeT = fi.TimeSteps[timeIndex];
                            try
                            {
                                if (File.Exists(dir2))
                                    File.Delete(dir2);
                            }
                            catch
                            {
                                //((BackgroundWorker)o).ReportProgress(1);
                                MessageBox.Show("File is used by other program!");
                                continue;
                            }

                            using (StreamWriter write = new StreamWriter(dir2))
                            {
                                //titles part
                                List<string> titles = new List<string>();
                                titles.Add("ImageN");
                                if (fi.sizeT > 1) titles.Add("T");
                                if (fi.sizeT > 1) titles.Add("T(sec.)");
                                if (fi.sizeZ > 1) titles.Add("Z");

                                int roiN = 1;
                                foreach (ROI roi in fi.roiList[c])
                                {
                                    if (roi.Checked == true && roi.Results[c] != null)
                                    {
                                        string com = "";
                                        if (roi.Comment != "") com = ": " + roi.Comment;

                                        titles.Add("Area" + roiN.ToString() + com);
                                        titles.Add("Mean" + roiN.ToString() + com);
                                        titles.Add("Min" + roiN.ToString() + com);
                                        titles.Add("Max" + roiN.ToString() + com);
                                        if (roi.Stack > 0)
                                            if (roi.Shape == 0 || roi.Shape == 1)
                                                for (int n = 1; n <= roi.Stack; n++)
                                                {
                                                    titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                    titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                    titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                    titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);

                                                    titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                    titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                    titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                    titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);

                                                    titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                    titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                    titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                    titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);

                                                    titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                    titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                    titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                    titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);

                                                }
                                            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                                for (int n = 1; n <= roi.Stack; n++)
                                                {
                                                    titles.Add("Area" + roiN.ToString() + "." + n.ToString() + com);
                                                    titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + com);
                                                    titles.Add("Min" + roiN.ToString() + "." + n.ToString() + com);
                                                    titles.Add("Max" + roiN.ToString() + "." + n.ToString() + com);
                                                }
                                    }
                                    roiN++;
                                }
                                write.WriteLine(string.Join("\t", titles));
                                //calculations
                                for (int i = c; i < fi.imageCount; i += fi.sizeC)
                                {
                                    //extract row from rois
                                    position = 0;
                                    result = new double[resultSize];
                                    foreach (ROI roi in fi.roiList[c])
                                    {
                                        if (roi.Checked == true)
                                        {
                                            if (roi.Shape == 0 || roi.Shape == 1)
                                            {
                                                if (roi.Results[i] != null
                                            && roi.FromT <= t && roi.ToT >= t
                                            && roi.FromZ <= z && roi.ToZ >= z)
                                                {
                                                    Array.Copy(roi.Results[i], 0, result, position, roi.Results[i].Length);
                                                }

                                                position += roi.Results[c].Length;
                                            }
                                            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                            {
                                                if (roi.Results[i] != null
                                            && roi.FromT <= t && roi.ToT >= t
                                            && roi.FromZ <= z && roi.ToZ >= z)
                                                {
                                                    //main roi
                                                    Array.Copy(roi.Results[i], 0, result, position, 4);
                                                    position += 4;
                                                    //layers
                                                    for (int p = 4; p < roi.Results[i].Length; p += 16)
                                                    {
                                                        Array.Copy(roi.Results[i], p, result, position, 4);
                                                        position += 4;
                                                    }
                                                }
                                                else
                                                {
                                                    position += 4;
                                                    //layers
                                                    for (int p = 4; p < roi.Results[i].Length; p += 16)
                                                    {
                                                        position += 4;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //write the line
                                    if (CheckArrayForValues(result))
                                    {
                                        str = string.Join("\t", result);

                                        if (fi.sizeZ > 1) str = z.ToString() + "\t" + str;
                                        if (fi.sizeT > 1)
                                        {
                                            str = t.ToString() + "\t" + time.ToString() + "\t" + str;
                                        }
                                        str = i.ToString() + "\t" + str;
                                        write.WriteLine(str);
                                    }
                                    //recalculate z and t

                                    z += 1;
                                    if (z > fi.sizeZ)
                                    {
                                        z = 1;
                                        t += 1;

                                        if (t > fi.sizeT)
                                        {
                                            t = 1;
                                        }

                                        if (t <= timeT)
                                        {
                                            time += fi.TimeSteps[timeIndex + 1];
                                        }
                                        else
                                        {
                                            timeIndex += 2;

                                            if (timeIndex < fi.TimeSteps.Count)
                                                timeT += fi.TimeSteps[timeIndex];
                                            else
                                            {
                                                timeIndex -= 2;
                                                timeT += fi.imageCount;
                                            }

                                            time += fi.TimeSteps[timeIndex + 1];
                                        }

                                    }
                                }
                            }
                        }
                    }

            //((BackgroundWorker)o).ReportProgress(0);
            }//);

           // bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                //if (a.ProgressPercentage == 0)
                {
                    IA.FileBrowser.Refresh_AfterSave();
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
                //else
                //{
                //    MessageBox.Show("File is used by other program!");
               // }
            }//);
            //Start background worker
            //IA.FileBrowser.StatusLabel.Text = "Saving results...";
            //start bgw
            //bgw.RunWorkerAsync();

            fi.available = true;
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }

        private void ConcatenateRoi_Click(object sender, EventArgs e)
        {
            //Check for selected ROI
            if (IA.RoiMan.SelectedROIsList.Count == 0)
            {
                MessageBox.Show("There is no selected ROI!");
                return;
            }
            //Find file info
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;
            //Return if image is not avaliable
            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }
            //select first selected ROI
            ROI firstROI = IA.RoiMan.SelectedROIsList[0];

            if(firstROI.Checked == false)
            {
                MessageBox.Show("Selected ROI is not checked!");
                return;
            }

            if (firstROI.Type == 0)
            {
                MessageBox.Show("Selected ROI type must be tracking!");
                return;
            }
            //extract Time and Z intervals
            int Tint = firstROI.ToT - firstROI.FromT;
            if (Tint <= 0) Tint = 1;

            int Zint = firstROI.ToZ - firstROI.FromZ;
            if (Zint <= 0) Zint = 1;

            int firstRoiIntervalSize = Zint * Tint * fi.sizeC;
            //check are roi intervals set 
            if (firstRoiIntervalSize == fi.imageCount)
            {
                MessageBox.Show("Selected ROI Time and Z intervals are not correct!");
                return;
            }

            List<ROI> roiList = new List<ROI>();
            foreach (ROI roi in fi.roiList[fi.cValue])
            {
                if (roi == firstROI) continue;
                if (roi.Checked == false) continue;
                if (roi.Shape != firstROI.Shape &&
                        !(roi.Shape > 1 && firstROI.Shape > 1)) continue;

                //extract Time and Z intervals
                Tint = roi.ToT - roi.FromT;
                if (Tint <= 0) Tint = 1;

                Zint = roi.ToZ - roi.FromZ;
                if (Zint <= 0) Zint = 1;
                //calculate interval size
                int Interval = Zint * Tint * fi.sizeC;

                if (Interval == fi.imageCount) continue;

                if (Interval + firstRoiIntervalSize <= fi.imageCount &&
                    (roi.FromT > firstROI.ToT || firstROI.FromT > roi.ToT))
                    roiList.Add(roi);
            }
            //Check for Rois;
            if (roiList.Count == 0)
            {
                MessageBox.Show("There is no ROI with the required parameters for concatenation!");
                return;
            }

            ROI secoundROI = null;
            ComboBox roiCB = new ComboBox();

            //Create dialog
            Form OptionForm = new Form();
            OptionForm.SuspendLayout();

            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "ROI Concatenate";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;
            OptionForm.BackColor = IA.FileBrowser.BackGround2Color1;
            OptionForm.ForeColor = IA.FileBrowser.ShriftColor1;

            OptionForm.Width = 300;
            OptionForm.Height = 150;

            Panel stackBox = new Panel();
            stackBox.Height = 80;
            stackBox.Dock = DockStyle.Top;
            OptionForm.Controls.Add(stackBox);

            Label firstROILab = new Label();
            firstROILab.Location = new System.Drawing.Point(10, 15);
            firstROILab.Text = "First ROI: " + firstROI.Text;
            firstROILab.Width = TextRenderer.MeasureText(firstROILab.Text, firstROILab.Font).Width + 5;
            stackBox.Controls.Add(firstROILab);

            Label secROILab = new Label();
            secROILab.Location = new System.Drawing.Point(10, 45);
            secROILab.Text = "Concatenate with:";
            secROILab.Width = 100;
            stackBox.Controls.Add(secROILab);

            foreach (ROI roi in roiList)
                roiCB.Items.Add(roi.Text);

            roiCB.SelectedIndex = 0;
            roiCB.Width = 150;
            roiCB.Location = new System.Drawing.Point(110, 43);
            stackBox.Controls.Add(roiCB);

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                secoundROI = roiList[roiCB.SelectedIndex];
                OptionForm.Close();
            });

            cancelBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                secoundROI = null;
                OptionForm.Close();
            });

            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
            {
                switch (a.KeyCode)
                {
                    case Keys.Escape:
                        secoundROI = null;
                        OptionForm.Close();
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });

            OptionForm.ResumeLayout();

            OptionForm.ShowDialog();
            OptionForm.Dispose();

            if (secoundROI == null) return;

            //remove secound roi and add it to history
            if (fi.roiList[fi.cValue] != null)
                if (fi.roiList[fi.cValue].IndexOf(secoundROI) > -1)
                    fi.roiList[fi.cValue].Remove(secoundROI);

            if (roiTV.Nodes.IndexOf(secoundROI) > -1)
                roiTV.Nodes.Remove(secoundROI);
           
            #region History
            addToHistoryOldInfo(roi_new(fi.cValue, secoundROI), fi);
            addToHistoryNewInfo(roi_delete(fi.cValue, secoundROI.getID), fi);
            #endregion History

            //remove first roi and add it to history
            if (fi.roiList[fi.cValue] != null)
                if (fi.roiList[fi.cValue].IndexOf(firstROI) > -1)
                    fi.roiList[fi.cValue].Remove(firstROI);

            if (roiTV.Nodes.IndexOf(firstROI) > -1)
                roiTV.Nodes.Remove(firstROI);

            #region History
            addToHistoryOldInfo(roi_new(fi.cValue, firstROI), fi);
            addToHistoryNewInfo(roi_delete(fi.cValue, firstROI.getID), fi);
            #endregion History
            //static to tracking compensation
            if (secoundROI.Type == 0)
            {
                int factorW = (secoundROI.Width - firstROI.Width) / 2;
                int factorH = (secoundROI.Height - firstROI.Height) / 2;
                StaticToTrackingRoi(secoundROI.GetLocation(fi.cValue), factorW, factorH, secoundROI);
            }
            //fill the coordinates to the new roi
            for (int frame = fi.cValue, t = 0, z = 0; frame < fi.imageCount; frame+=fi.sizeC, z++)
            {
                //z and t counter
                if (z >= fi.sizeZ)
                {
                    z = 0;
                    t++;
                }
                //3 types of images with t and z or only with t or z
                if (fi.sizeT > 1 && fi.sizeZ > 1)
                {
                    if (t >= secoundROI.FromT - 1 && t < secoundROI.ToT &&
                    z >= secoundROI.FromZ - 1 && z < secoundROI.ToZ)
                        firstROI.SetLocation(frame, secoundROI.GetLocation(frame));
                }
                else if (fi.sizeT > 1)
                {
                    if (t >= secoundROI.FromT - 1 && t < secoundROI.ToT)
                        firstROI.SetLocation(frame, secoundROI.GetLocation(frame));
                }
                else if (fi.sizeZ > 1)
                {
                    if (z >= secoundROI.FromZ - 1 && t < secoundROI.ToZ)
                        firstROI.SetLocation(frame, secoundROI.GetLocation(frame));
                }
            }
            //calculate the new bounderies
            if (firstROI.FromT > secoundROI.FromT)
                firstROI.FromT = secoundROI.FromT;

            if (firstROI.FromZ > secoundROI.FromZ)
                firstROI.FromZ = secoundROI.FromZ;

            if (firstROI.ToT < secoundROI.ToT)
                firstROI.ToT = secoundROI.ToT;

            if (firstROI.ToZ < secoundROI.ToZ)
                firstROI.ToZ = secoundROI.ToZ;
            //change the roi id and add it to the roiList
            firstROI.getID = fi.ROICounter;
            fi.ROICounter++;
            fi.roiList[fi.cValue].Add(firstROI);
            SelectedROIsList.Add(firstROI);
            current = firstROI;
            //Delete the sec roi
            secoundROI.Delete();
            //recalculate the roi
            RoiMeasure.Measure(firstROI, fi, fi.cValue, IA);
            //add the new roi to history
            #region History
            addToHistoryOldInfo(roi_delete(fi.cValue, firstROI.getID), fi);
            addToHistoryNewInfo(roi_new(fi.cValue, firstROI), fi);
            #endregion History
            //reload the image
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        private Point[] StaticToTrackingRoi(Point[] points, int factorW, int factorH, ROI secROI)
        {
            if(secROI.Type == 0 && secROI.Shape < 2)
            {
                for(int i = 0; i < points.Length; i++)
                {
                    points[i].X += factorW;
                    points[i].Y += factorH;
                }
            }
            return points;
        }

        public void ExportRoiAsIJMacro1(object sender,EventArgs e)
        {
            //Return if there is no opened image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (fi.roiList[fi.cValue] == null) return;

            List<string> result = new List<string>();

            result.Add("//CellTool3\n//roi macro for ImageJ\n//CopyRights Georgi Todorov Danovski, Bulgaria, 2016\n//https://www.facebook.com/georgi.danovski\n//e-mail: georgi_danovski@abv.bg\n\n");

            result.Add("macro  \"Import_CT_RoiSet[q]\"\n{\nroiManager(\"reset\");\nopen(\"" + fi.Dir.Replace("\\","\\\\") + "\");\n");

            foreach (ROI roi in fi.roiList[fi.cValue])
            {
                Point[][] points = roi.GetLocationAll();
                int n = 1;
                string[] xcoord;
                string[] ycoord;
                Point[] pL;
                //if (roi.Type == 1)
                //foreach (Point[] pL in points)
                for (int ind = fi.cValue; ind < points.Length; ind += fi.sizeC)
                {
                    pL = points[ind];

                    if (pL != null)
                        switch (roi.Shape)
                        {
                            case 0:
                                break;
                            case 1:
                                break;

                            case 2:
                                xcoord = new string[pL.Length];
                                ycoord = new string[pL.Length];

                                for (int i = 0; i < pL.Length; i++)
                                {
                                    xcoord[i] = pL[i].X.ToString();
                                    ycoord[i] = pL[i].Y.ToString();
                                }

                                result.Add("\nsetSlice(" + n.ToString() +
                                    ");\ntype = \"polygon\";\nxcoord = newArray(" +
                                    string.Join(",", xcoord) + ");\nycoord = newArray(" +
                                    string.Join(",", ycoord) +
                                    ");\nmakeSelection(type, xcoord, ycoord);\nroiManager(\"Add\");\n"
                                    );
                                n++;
                                xcoord = null;
                                ycoord = null;
                                break;
                            case 3:
                                xcoord = new string[pL.Length];
                                ycoord = new string[pL.Length];

                                for (int i = 0; i < pL.Length; i++)
                                {
                                    xcoord[i] = pL[i].X.ToString();
                                    ycoord[i] = pL[i].Y.ToString();
                                }

                                result.Add("\nsetSlice(" + n.ToString() +
                                    ");\ntype = \"freehand\";\nxcoord = newArray(" +
                                    string.Join(",", xcoord) + ");\nycoord = newArray(" +
                                    string.Join(",", ycoord) +
                                    ");\nmakeSelection(type, xcoord, ycoord);\nroiManager(\"Add\");\n"
                                    );
                                n++;
                                xcoord = null;
                                ycoord = null;
                                break;
                            case 4:
                                xcoord = new string[pL.Length];
                                ycoord = new string[pL.Length];

                                for (int i = 0; i < pL.Length; i++)
                                {
                                    xcoord[i] = pL[i].X.ToString();
                                    ycoord[i] = pL[i].Y.ToString();
                                }
                                result.Add("\nsetSlice(" + n.ToString() +
                                    ");\ntype = \"freehand\";\nxcoord = newArray(" +
                                    string.Join(",", xcoord) + ");\nycoord = newArray(" +
                                    string.Join(",", ycoord) +
                                    ");\nmakeSelection(type, xcoord, ycoord);\nroiManager(\"Add\");\n"
                                    );
                                n++;
                                xcoord = null;
                                ycoord = null;
                                break;
                        }
                }
            }

            result.Add("\nxcoord = newArray();\nycoord = newArray();\n}\n");

            string res = string.Join("\n", result);
            string path = Application.StartupPath + "\\IJM.ijm";

            File.WriteAllText(path, res);

            MessageBox.Show("IJ macro exported!");
        }
       
        #endregion Save Roi as IJ macro
    }
}
