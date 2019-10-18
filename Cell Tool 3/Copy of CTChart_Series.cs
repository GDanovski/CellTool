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
    class CTChart_Series
    {
        //controls
        public ImageAnalyser IA;
        private PropertiesPanel_Item PropPanel;
        private TreeView tv = new TreeView();
        public Panel panel;
        //Settings
        List<Color> RefColors = new List<Color>();
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        public CTChart_Series(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            this.IA = IA;

            PropPanel = new PropertiesPanel_Item();
            PropPanel_Initialize(propertiesPanel, PropertiesBody);

            string[] colMatrix = new string[] {"blue","red","#00b300", "#b300b3", "#00bfff", "#ffcc00", "#ff471a", "#cc6699", "#39e600"
                , "#00b3b3", "#ffcc66", "#7575a3", "#ff1a1a", "#ff0055", "#8a00e6", "#bf8040",
                "#53c68c", "#ace600", "#b33c00", "#ff6666"};

            tv.ImageList = new ImageList();
            tv.ImageList.ImageSize = new Size(20, 16);

            foreach (string val in colMatrix)
            {
                RefColors.Add(ColorTranslator.FromHtml(val));
                createImagesFortTV(ColorTranslator.FromHtml(val));
            }
        }

        private void createImagesFortTV(Color col)
        {
            Bitmap bmp = new Bitmap(18, 16);

            using (Graphics gr = Graphics.FromImage(bmp))
            {
                SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                gr.FillRectangle(blueBrush, 0, 0, bmp.Width, bmp.Height);
                blueBrush = new SolidBrush(col);
                gr.FillRectangle(blueBrush, 4, 1, bmp.Width - 6, bmp.Height - 3);
            }

            tv.ImageList.Images.Add(bmp);
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
            tv.BackColor = color;
        }
        public void ForeColor(Color color)
        {
            //format fore color
            PropPanel.ForeColor(color);
            tv.ForeColor = color;
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
            PropPanel.Resizable = true;
            PropPanel.Name.Text = "Chart Series";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;

            panel.Visible = false;

            BuildOptions();
        }
        private void BuildOptions()
        {
            Panel BotPanel = new Panel();
            BotPanel.Dock = DockStyle.Top;
            BotPanel.Height = 5;
            panel.Controls.Add(BotPanel);
            BotPanel.BringToFront();

            tv.Dock = DockStyle.Fill;
            tv.ShowNodeToolTips = false;
            tv.Height = 200;
            tv.BorderStyle = BorderStyle.None;
            tv.CheckBoxes = true;
            panel.Controls.Add(tv);
            tv.BringToFront();
            tv.ShowRootLines = false;
            tv.ShowPlusMinus = false;
            tv.NodeMouseDoubleClick += Node_DoubleClick;
            tv.AfterCheck += node_Check;
            tv.NodeMouseClick += Node_Click;
        }

        public void LoadFI(TifFileInfo fi)
        {
            CheckRoiSettings(fi);
            LoadTV(fi);
        }
        #region TreeView
        private void LoadTV(TifFileInfo fi)
        {
            TreeNode selectedNode = tv.SelectedNode;
            if(selectedNode!= null && selectedNode.Parent != null)
            {
                selectedNode = selectedNode.Parent;
            }

            //tv.SuspendLayout();
            tv.BeginUpdate();
            tv.Nodes.Clear();
            TreeNode n;

            ROI roi;

            if (fi.roiList != null && fi.roiList[fi.cValue] != null)
            {
                for (int i = 0; i < fi.roiList[fi.cValue].Count; i++)
                {

                    roi = fi.roiList[fi.cValue][i];

                    n = new TreeNode();
                    n.Checked = roi.ChartUseIndex[0];
                    n.Tag = roi;
                    n.Text = "ROI" + (i + 1).ToString();
                    n.ImageIndex = -1;

                    for (int ind = 0; ind < RefColors.Count; ind++)
                        if (RefColors[ind] == roi.colors[0])
                        {
                            n.ImageIndex = ind;
                            n.SelectedImageIndex = ind;
                            break;
                        }

                    if (n.ImageIndex == -1)
                    {
                        n.ImageIndex = RefColors.Count;
                        createImagesFortTV(roi.colors[0]);
                        RefColors.Add(roi.colors[0]);
                    }

                    if (roi.Checked)
                        tv.Nodes.Add(n);

                    int position = 1;

                    //if (roi.Shape == 0 || roi.Shape == 1)
                    for (int z = 0; z < roi.Stack; z++)
                    {
                        n = new TreeNode();
                        n.Checked = roi.ChartUseIndex[0];
                        n.Tag = roi;
                        n.Text = "ROI" + (i + 1).ToString() + ".Layer" + (z + 1).ToString();
                        n.ImageIndex = -1;

                        for (int ind = 0; ind < RefColors.Count; ind++)
                            if (RefColors[ind] == roi.colors[z + 1])
                            {
                                n.ImageIndex = ind;
                                n.SelectedImageIndex = ind;
                                break;
                            }

                        if (n.ImageIndex == -1)
                        {
                            n.ImageIndex = RefColors.Count;
                            createImagesFortTV(roi.colors[0]);
                            RefColors.Add(roi.colors[0]);
                        }
                        if (roi.Shape == 0 || roi.Shape == 1)
                        {
                            n.Nodes.Add("Left Up");
                            n.Nodes.Add("Right Up");
                            n.Nodes.Add("Left Down");
                            n.Nodes.Add("Right Down");

                            n.Checked = false;
                            for (int p = position, nNode = 0; p < position + 4; p++, nNode++)
                                if (roi.ChartUseIndex[p] == true)
                                {
                                    n.Nodes[nNode].Checked = true;
                                    n.Checked = true;
                                }

                            foreach (TreeNode n1 in n.Nodes)
                            {
                                n1.ImageIndex = 10000;
                                n1.SelectedImageIndex = 10000;
                            }

                            if (roi.expanded[z] == true)
                                n.ExpandAll();
                            else
                                n.Collapse();
                        }
                        else
                        {
                            n.Checked = roi.ChartUseIndex[position];
                        }

                        if (roi.Checked)
                            tv.Nodes.Add(n);

                        position += 4;
                    }
                }
            }
            
            tv.EndUpdate();

            if (selectedNode != null)
                foreach (TreeNode n1 in tv.Nodes)
                    if (n1.Text == selectedNode.Text)
                    {
                        n1.EnsureVisible();
                        if (n1.Nodes.Count > 0)
                        {
                            n1.Nodes[n1.Nodes.Count - 1].EnsureVisible();
                        }
                        tv.SelectedNode = n1;
                        break;
                    }
        }
        private void Node_Click(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (MouseButtons.Right != e.Button) return;
            tv.SelectedNode = e.Node;
            TreeNode n = e.Node;
            if (n == null || n.Tag == null) return;

            ROI roi = (ROI)n.Tag;
            if (roi == null) roi = (ROI)n.Parent.Tag;

            int ind = 0;

            if (n.Text.IndexOf("Layer") > -1)
            {
                ind = int.Parse(n.Text.Substring(n.Text.LastIndexOf("r") + 1, n.Text.Length - n.Text.LastIndexOf("r") - 1));
            }

            //ColorBrowser
            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.AllowFullOpen = true;
            colorDialog1.AnyColor = true;
            colorDialog1.FullOpen = true;
            colorDialog1.Color = roi.colors[ind];
            //set Custom Colors
            if (IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] != "@")
            {
                List<int> colorsList = new List<int>();
                foreach (string j in IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex]
                    .Split(new[] { "\t" }, StringSplitOptions.None))
                {
                    colorsList.Add(int.Parse(j));
                }
                colorDialog1.CustomColors = colorsList.ToArray();
            }
            // Show the color dialog.
            DialogResult result = colorDialog1.ShowDialog();
            //Copy Custom Colors
            int[] colors = (int[])colorDialog1.CustomColors.Clone();
            string txt = "@";
            if (colors.Length > 0)
            {
                txt = colors[0].ToString();
                for (int j = 1; j < colors.Length; j++)
                {
                    txt += "\t" + colors[j].ToString();
                }
            }
            IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] = txt;
            IA.settings.Save();

            if (result == DialogResult.OK)
            {
                IA.AddToHistoryOld("Chart.SetSeriesColor(" + GetChanel(roi).ToString() + ","
                    + roi.getID.ToString() + "," + ind.ToString() + "," + ColorTranslator.ToHtml(roi.colors[ind]) + ")");

                roi.colors[ind] = colorDialog1.Color;

                IA.AddToHistoryNew("Chart.SetSeriesColor(" + GetChanel(roi).ToString() + ","
                   + roi.getID.ToString() + "," + ind.ToString() + "," + ColorTranslator.ToHtml(colorDialog1.Color) + ")");


                foreach (Color col in RefColors)
                    if (colorDialog1.Color == col)
                    {
                        IA.ReloadImages();
                        return;
                    }

                RefColors.Add(colorDialog1.Color);
                createImagesFortTV(colorDialog1.Color);

                IA.ReloadImages();
            }
        }
        private void Node_DoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode n = e.Node;
            if (n == null || n.Tag == null) return;
            tv.SelectedNode = e.Node;
            ROI roi = (ROI)n.Tag;
            if (roi == null) return;

            if (n.Text.IndexOf("Layer") > -1)
            {
                int ind = int.Parse(n.Text.Substring(n.Text.LastIndexOf("r") + 1, n.Text.Length - n.Text.LastIndexOf("r") - 1)) - 1;
                roi.expanded[ind] = !roi.expanded[ind];
            }
        }

        private bool suppressCheck = false;
        private void node_Check(object sender, TreeViewEventArgs e)
        {
            TreeNode n = e.Node;
            if (n == null) return;
            if (!suppressCheck)
            {
                suppressCheck = true;
                tv.SelectedNode = e.Node;
                ROI roi = (ROI)n.Tag;
                if (roi == null)
                {
                    TreeNode parent = n.Parent;
                    roi = (ROI)parent.Tag;

                    int ind = 1 + 4 * (int.Parse(parent.Text.Substring(parent.Text.LastIndexOf("r") + 1
                        , parent.Text.Length - parent.Text.LastIndexOf("r") - 1)) - 1)
                        + parent.Nodes.IndexOf(n);

                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    roi.ChartUseIndex[ind] = n.Checked;

                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                }
                else if (n.Text.IndexOf("Layer") > -1)
                {
                    int ind = 1 + 4 * (int.Parse(n.Text.Substring(n.Text.LastIndexOf("r") + 1, n.Text.Length - n.Text.LastIndexOf("r") - 1)) - 1);

                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    roi.ChartUseIndex[ind] = n.Checked;

                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    ind++;

                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    roi.ChartUseIndex[ind] = n.Checked;

                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    ind++;

                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    roi.ChartUseIndex[ind] = n.Checked;
                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    ind++;
                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    roi.ChartUseIndex[ind] = n.Checked;
                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

                    ind++;
                }
                else
                {
                    IA.AddToHistoryOld("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + ",0," + roi.ChartUseIndex[0].ToString() + ")");

                    roi.ChartUseIndex[0] = n.Checked;

                    IA.AddToHistoryNew("Chart.SetSeriesChecked(" + GetChanel(roi).ToString() + ","
                        + roi.getID.ToString() + ",0," + roi.ChartUseIndex[0].ToString() + ")");
                }

                IA.ReloadImages();

                suppressCheck = false;
            }
        }

        #endregion TreeView

        #region Check roi settings

        private void CheckRoiSettings(TifFileInfo fi)
        {
            for (int c = 0; c < fi.sizeC; c++)
            {
                if (fi.roiList != null && fi.roiList[c] != null)
                    foreach (ROI roi in fi.roiList[c])
                        AsignColorToRoi(roi, fi.roiList[c]);

            }
        }
        private void AsignColorToRoi(ROI roi, List<ROI> roiList)
        {
            if (roi.expanded == null)
            {
                roi.expanded = new bool[roi.Stack];
                for (int i = 0; i < roi.expanded.Length; i++)
                    roi.expanded[i] = true;
            }
            else if (roi.expanded.Length < roi.Stack)
            {
                bool[] b = new bool[roi.Stack];

                for (int i = 0; i < b.Length; i++)
                    b[i] = true;

                Array.Copy(roi.expanded, b, roi.expanded.Length);
                roi.expanded = b;
            }
            else if (roi.expanded.Length > roi.Stack)
            {
                bool[] b = new bool[roi.Stack];
                Array.Copy(roi.expanded, b, roi.Stack);
                roi.expanded = b;
            }

            if (roi.colors != null && roi.colors.Length == 1 + roi.Stack &&
                roi.ChartUseIndex != null && roi.ChartUseIndex.Length == 1 + roi.Stack * 4)
                return;

            if (roi.colors == null) roi.colors = new Color[1 + roi.Stack];
            if (roi.ChartUseIndex == null) roi.ChartUseIndex = new bool[1 + roi.Stack * 4];

            if (roi.colors.Length == 1 + roi.Stack)
            {
                for (int i = 0; i < roi.colors.Length; i++)
                    roi.colors[i] = FindColor(roiList);
            }

            else if (roi.colors.Length < 1 + roi.Stack)
            {
                List<Color> cols = new List<Color>();

                foreach (Color col in roi.colors)
                    cols.Add(col);

                for (int i = cols.Count - 1; i < 1 + roi.Stack; i++)
                {
                    cols.Add(FindColor(roiList));
                    roi.colors = cols.ToArray();
                }
            }
            else if (roi.colors.Length > 1 + roi.Stack)
            {
                Color[] cols = new Color[1 + roi.Stack];
                Array.Copy(roi.colors, cols, 1 + roi.Stack);
                roi.colors = cols;
            }

            if (roi.ChartUseIndex.Length == 1 + roi.Stack * 4)
            {
                for (int i = 0; i < roi.ChartUseIndex.Length; i++)
                    roi.ChartUseIndex[i] = true;
            }
            else if (roi.ChartUseIndex.Length < 1 + roi.Stack * 4)
            {
                List<bool> ChartUseIndex = new List<bool>();

                foreach (bool b in roi.ChartUseIndex)
                    ChartUseIndex.Add(b);

                for (int i = ChartUseIndex.Count - 1; i < 1 + roi.Stack * 4; i++)
                    ChartUseIndex.Add(true);

                roi.ChartUseIndex = ChartUseIndex.ToArray();
            }
            else if (roi.ChartUseIndex.Length > 1 + roi.Stack * 4)
            {
                bool[] b = new bool[1 + roi.Stack * 4];
                Array.Copy(roi.ChartUseIndex, b, b.Length);
                roi.ChartUseIndex = b;
            }

        }
        private Color FindColor(List<ROI> roiList)
        {
            bool[] colorCheck = new bool[RefColors.Count];

            foreach (ROI roi in roiList)
                if (roi.colors != null)
                    foreach (Color col in roi.colors)
                        for (int i = 0; i < colorCheck.Length; i++)
                            if (col == RefColors[i])
                            {
                                colorCheck[i] = true;
                                break;
                            }

            for (int i = 0; i < colorCheck.Length; i++)
                if (colorCheck[i] == false)
                    return RefColors[i];

            Random rnd = new Random();
            byte[] b = new byte[3];
            rnd.NextBytes(b);
            Color colNew = Color.FromArgb(255, b[0], b[1], b[2]);

            RefColors.Add(colNew);
            createImagesFortTV(colNew);

            return colNew;
        }
        #endregion Check roi settings
        #region History
        /*
        Chart.SetSeriesColor(chanel,roiID, index, color.HTML) -> change the color of the series
        Chart.SetSeriesChecked(chanel,roiID, index, bool) -> enable/disable
        */
        private int GetChanel(ROI roi)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return -1; }
            if (fi == null) { return -1; }

            if (fi.roiList == null) return -1;

            for (int i = 0; i < fi.roiList.Length; i++)
                if (fi.roiList[i] != null && fi.roiList[i].IndexOf(roi) > -1)
                    return i;

            return -1;
        }

        #endregion History
    }
}
