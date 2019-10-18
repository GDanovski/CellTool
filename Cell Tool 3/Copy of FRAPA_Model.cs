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
using Microsoft.SolverFoundation.Services;
using Accord.Statistics.Models.Regression.Linear;
using System.Windows.Forms;
using System.IO;
//using Accord;//Accord.Math.Bessel
using MathNet;

namespace Cell_Tool_3
{
    class FRAPA_Model
    {
        public const int nModels = 5;
        #region Aquisition bleaching correction
        private static int[] Dialog(double[] Xvals, ToolStripStatusLabel StatusLabel)
        {
            int[] res = null;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "Acquisition bleaching correction";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;

            OptionForm.Width = 220;
            OptionForm.Height = 200;

            Label label1 = new Label();
            label1.Text = "Bleaching at:";
            label1.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label1.Location = new System.Drawing.Point(5, 15);
            OptionForm.Controls.Add(label1);

            TextBox tb1 = new TextBox();
            tb1.Text = "5";
            tb1.Width = 100;
            tb1.Location = new System.Drawing.Point(100, 15);
            tb1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb1);

            Label label2 = new Label();
            label2.Text = "Pre-Bleach:";
            label2.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label2.Location = new System.Drawing.Point(5, 45);
            OptionForm.Controls.Add(label2);

            TextBox tb2 = new TextBox();
            tb2.Text = "5";
            tb2.Width = 100;
            tb2.Location = new System.Drawing.Point(100, 45);
            tb2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb2);

            Label label3 = new Label();
            label3.Text = "Post-Bleach:";
            label3.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label3.Location = new System.Drawing.Point(5, 75);
            OptionForm.Controls.Add(label3);

            TextBox tb3 = new TextBox();
            tb3.Text = "10";
            tb3.Width = 100;
            tb3.Location = new System.Drawing.Point(100, 75);
            tb3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb3);

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                int a = 0, b = 0, c = 0;
                if (!int.TryParse(tb1.Text, out a) ||
                !int.TryParse(tb2.Text, out b) ||
                !int.TryParse(tb3.Text, out c))
                {
                    MessageBox.Show("Value must be numeric!");
                    return;
                }

                if (a < 1 || a >= Xvals.Length
                || a - b < 0 || b < 1 ||
                a + c >= Xvals.Length || c < 1)
                {
                    MessageBox.Show("Incorrect value!");
                    return;
                }

                //event
                res = new int[] { a, b, c };
                OptionForm.Close();
            });

            cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                OptionForm.Close();
            });

            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        OptionForm.Close();
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });

            StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            OptionForm.Dispose();
            StatusLabel.Text = "Ready";

            return res;
        }

        public static void FrappaNormalise(ResultsExtractor.MyForm form1)
        {
            int[] dialogVals = Dialog(form1.dataTV.OriginalXaxis, form1.StatusLabel);
            if (dialogVals == null) return;

            int bleaching = dialogVals[0];
            int avgPreLength = dialogVals[1];
            int avgPostLength = dialogVals[2];

            if (form1.SubsetCB.Checked) form1.SubsetCB.Checked = false;
            if (form1.NormCB.Checked) form1.NormCB.Checked = false;

            Dictionary<string, List<TreeNode>> lib = new Dictionary<string, List<TreeNode>>();
            string[] names = null;

            foreach (TreeNode node in form1.dataTV.Store)
            {
                lib = new Dictionary<string, List<TreeNode>>();
                foreach (TreeNode n in node.Nodes)
                {
                    names = n.Text.Split(new string[] { "\t" }, StringSplitOptions.None);
                    n.Text = names[1];

                    if (lib.ContainsKey(names[0]))
                    {
                        lib[names[0]].Add(n);
                    }
                    else
                    {
                        lib.Add(names[0], new List<TreeNode>() { n });
                    }
                }
                node.Nodes.Clear();

                foreach (var kvp in lib)
                {
                    ResultsExtractor.DataNode FRAP = null;
                    ResultsExtractor.DataNode Whole = null;
                    ResultsExtractor.DataNode Zero = null;

                    foreach (var n in kvp.Value)
                    {
                        try
                        {
                            switch (n.Text.Substring(1, 9))
                            {
                                case "Mean_ROI1":
                                    FRAP = (ResultsExtractor.DataNode)n;
                                    break;
                                case "Mean_ROI2":
                                    Whole = (ResultsExtractor.DataNode)n;
                                    break;
                                case "Mean_ROI3":
                                    Zero = (ResultsExtractor.DataNode)n;
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (FRAP == null || Whole == null || Zero == null) continue;

                    ResultsExtractor.DataNode New = new ResultsExtractor.DataNode();
                    New.Text = kvp.Key + "\tAcqBleachCorr";
                    New.Series = new double[FRAP.OriginalSeries.Length];
                    New.OriginalSeries = new double[FRAP.OriginalSeries.Length];
                    New.NormSeries = new double[FRAP.OriginalSeries.Length];
                    New.ImageIndex = 1;
                    New.SelectedImageIndex = 1;
                    New.Tag = FRAP.Tag;
                    New.Checked = true;

                    //calculate the prebleaching
                    double IfrapPre = 0, IwholePre = 0, IwholePost = 0, val = 0;
                    //,Base = Zero.OriginalSeries.Average();
                    int newAvgPreLength = 0, newAvgPostLength = 0;

                    for (int i = bleaching - avgPreLength; i < bleaching; i++)
                        if (i >= 0 && i < FRAP.OriginalSeries.Length)
                        {
                            IfrapPre += (FRAP.OriginalSeries[i] - Zero.OriginalSeries[i]);
                            IwholePre += (Whole.OriginalSeries[i] - Zero.OriginalSeries[i]);
                            newAvgPreLength++;
                        }

                    for (int i = bleaching; i < bleaching + avgPostLength; i++)
                        if (i >= 0 && i < FRAP.OriginalSeries.Length)
                        {
                            IwholePost += (Whole.OriginalSeries[i] - Zero.OriginalSeries[i]);
                            newAvgPostLength++;
                        }

                    avgPreLength = newAvgPreLength;
                    avgPostLength = newAvgPostLength;

                    IfrapPre /= avgPreLength;
                    IwholePre /= avgPreLength;
                    IwholePost /= avgPostLength;

                    if (IfrapPre == 0) IfrapPre = 1;
                    if (IwholePre == 0) IfrapPre = 1;
                    if (IwholePost == 0) IfrapPre = 1;
                    //Bleaching depth
                    double bd = (IfrapPre - (FRAP.OriginalSeries[bleaching] - Zero.OriginalSeries[bleaching])) / IfrapPre;
                    //Gap ratio
                    double gap = IwholePost / IwholePre;
                    //double normalisation
                    for (int i = 0; i < FRAP.OriginalSeries.Length; i++)
                    {
                        //double normalization
                        val = (IwholePre / (Whole.OriginalSeries[i] - Zero.OriginalSeries[i])) *
                            ((FRAP.OriginalSeries[i] - Zero.OriginalSeries[i]) / IfrapPre);

                        //apply values
                        New.OriginalSeries[i] = val;
                        New.Series[i] = val;
                        New.NormSeries[i] = val;
                    }

                    //full scale normalization

                    double Ipost = New.OriginalSeries[bleaching];
                    for (int i = 0; i < New.OriginalSeries.Length; i++)
                    {
                        val = (New.OriginalSeries[i] - Ipost) / (1 - Ipost);

                        New.OriginalSeries[i] = val;
                        New.Series[i] = val;
                        New.NormSeries[i] = val;
                    }

                    New.Comment = "gr: " + gap + " | bd: " + bd;
                    New.RoiName = "acquisition bleaching correction";

                    node.Nodes.Add(New);
                }

            }
            //refresh vals
            form1.dataTV.RefreshAllNodes();
        }
        /*
        public static void FrappaNormalise(ResultsExtractor.MyForm form1)
        {
            int[] dialogVals = Dialog(form1.dataTV.OriginalXaxis);
            if (dialogVals == null) return;

            int bleaching = dialogVals[0];
            int avgPreLength = dialogVals[1];
            int avgPostLength = dialogVals[2];

            if (form1.SubsetCB.Checked) form1.SubsetCB.Checked = false;
            if (form1.NormCB.Checked) form1.NormCB.Checked = false;

            Dictionary<string, List<TreeNode>> lib = new Dictionary<string, List<TreeNode>>();
            string[] names = null;

            foreach (TreeNode node in form1.dataTV.Store)
            {
                lib = new Dictionary<string, List<TreeNode>>();
                foreach (TreeNode n in node.Nodes)
                {
                    names = n.Text.Split(new string[] { "\t" }, StringSplitOptions.None);
                    n.Text = names[1];

                    if (lib.ContainsKey(names[0]))
                    {
                        lib[names[0]].Add(n);
                    }
                    else
                    {
                        lib.Add(names[0], new List<TreeNode>() { n });
                    }
                }
                node.Nodes.Clear();

                foreach (var kvp in lib)
                {
                    ResultsExtractor.DataNode FRAP = null;
                    ResultsExtractor.DataNode Whole = null;
                    ResultsExtractor.DataNode Zero = null;

                    foreach (var n in kvp.Value)
                    {
                        switch (n.Text.Substring(1, 9))
                        {
                            case "Mean_ROI1":
                                FRAP = (ResultsExtractor.DataNode)n;
                                break;
                            case "Mean_ROI2":
                                Whole = (ResultsExtractor.DataNode)n;
                                break;
                            case "Mean_ROI3":
                                Zero = (ResultsExtractor.DataNode)n;
                                break;
                            default:
                                break;
                        }
                    }

                    if (FRAP == null || Whole == null || Zero == null) continue;

                    ResultsExtractor.DataNode New = new ResultsExtractor.DataNode();
                    New.Text = kvp.Key + "\tAcqBleachCorr";
                    New.Series = new double[FRAP.OriginalSeries.Length];
                    New.OriginalSeries = new double[FRAP.OriginalSeries.Length];
                    New.NormSeries = new double[FRAP.OriginalSeries.Length];
                    New.ImageIndex = 1;
                    New.SelectedImageIndex = 1;
                    New.Tag = FRAP.Tag;
                    New.Checked = true;

                    //calculate the prebleaching
                    double IfrapPre = 0, IwholePre = 0, IwholePost = 0, val = 0,
                        Base = Zero.OriginalSeries.Average();

                    for (int i = bleaching - avgPreLength; i < bleaching; i++)
                    {
                        IfrapPre += (FRAP.OriginalSeries[i] - Base);
                        IwholePre += (Whole.OriginalSeries[i] - Base);
                    }

                    for (int i = bleaching; i < bleaching + avgPostLength; i++)
                    {
                        IwholePost += (Whole.OriginalSeries[i] - Base);
                    }

                    IfrapPre /= avgPreLength;
                    IwholePre /= avgPreLength;
                    IwholePost /= avgPostLength;

                    if (IfrapPre == 0) IfrapPre = 1;
                    if (IwholePre == 0) IfrapPre = 1;
                    if (IwholePost == 0) IfrapPre = 1;
                    //Bleaching depth
                    double bd = (IfrapPre - (FRAP.OriginalSeries[bleaching] - Base)) / IfrapPre;
                    //Gap ratio
                    double gap = IwholePost / IwholePre;
                    //double normalisation
                    for (int i = 0; i < FRAP.OriginalSeries.Length; i++)
                    {
                        //double normalization
                        val = (IwholePre / (Whole.OriginalSeries[i] - Base)) *
                            ((FRAP.OriginalSeries[i] - Base) / IfrapPre);

                        //apply values
                        New.OriginalSeries[i] = val;
                        New.Series[i] = val;
                        New.NormSeries[i] = val;
                    }

                    //full scale normalization
                    double Ipost = New.OriginalSeries[bleaching];
                    for (int i = 0; i < New.OriginalSeries.Length; i++)
                    {
                        val = (New.OriginalSeries[i] - Ipost) / (1 - Ipost);

                        New.OriginalSeries[i] = val;
                        New.Series[i] = val;
                        New.NormSeries[i] = val;
                    }

                    New.Comment = "gr: " + gap + " | bd: " + bd;
                    New.RoiName = "acquisition bleaching correction";

                    node.Nodes.Add(New);
                }

            }
            //refresh vals
            form1.dataTV.RefreshAllNodes();
        }
        */
        #endregion Aquisition bleaching correction
        #region Foci FRAPA
        private static int[] FociDialog(double[] Xvals, ToolStripStatusLabel StatusLabel)
        {
            int[] res = null;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "Foci FRAPA normalization";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;

            OptionForm.Width = 220;
            OptionForm.Height = 200;

            Label label1 = new Label();
            label1.Text = "Irradiate at:";
            label1.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label1.Location = new System.Drawing.Point(5, 15);
            OptionForm.Controls.Add(label1);

            TextBox tb1 = new TextBox();
            tb1.Text = "5";
            tb1.Width = 100;
            tb1.Location = new System.Drawing.Point(100, 15);
            tb1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb1);

            Label label2 = new Label();
            label2.Text = "Bleaching at:";
            label2.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label2.Location = new System.Drawing.Point(5, 45);
            OptionForm.Controls.Add(label2);

            TextBox tb2 = new TextBox();
            tb2.Text = "5";
            tb2.Width = 100;
            tb2.Location = new System.Drawing.Point(100, 45);
            tb2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb2);

            Label label3 = new Label();
            label3.Text = "Pre-Bleach:";
            label3.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width + 5;
            label3.Location = new System.Drawing.Point(5, 75);
            OptionForm.Controls.Add(label3);

            TextBox tb3 = new TextBox();
            tb3.Text = "10";
            tb3.Width = 100;
            tb3.Location = new System.Drawing.Point(100, 75);
            tb3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(tb3);

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                int a = 0, b = 0, c = 0;
                if (!int.TryParse(tb1.Text, out a) ||
                !int.TryParse(tb2.Text, out b) ||
                !int.TryParse(tb3.Text, out c))
                {
                    MessageBox.Show("Value must be numeric!");
                    return;
                }

                if (a < 1 || a >= Xvals.Length ||
                b < 1 || b >= Xvals.Length ||
                 b - c <= a || c < 1)
                {
                    MessageBox.Show("Incorrect value!");
                    return;
                }

                //event
                res = new int[] { a, b, c };
                OptionForm.Close();
            });

            cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                OptionForm.Close();
            });

            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        OptionForm.Close();
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });

           StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            OptionForm.Dispose();
            StatusLabel.Text = "Ready";

            return res;
        }
        public static void FociFrappaNormalise(ResultsExtractor.MyForm form1)
        {
            int[] dialogVals = FociDialog(form1.dataTV.OriginalXaxis, form1.StatusLabel);
            if (dialogVals == null) return;

            int MP = dialogVals[0];
            int bleaching = dialogVals[1];
            int avgPreLength = dialogVals[2];

            if (form1.SubsetCB.Checked) form1.SubsetCB.Checked = false;
            if (form1.NormCB.Checked) form1.NormCB.Checked = false;

            Dictionary<string, List<TreeNode>> lib = new Dictionary<string, List<TreeNode>>();
            string[] names = null;

            foreach (TreeNode node in form1.dataTV.Store)
            {
                lib = new Dictionary<string, List<TreeNode>>();
                foreach (TreeNode n in node.Nodes)
                {
                    names = n.Text.Split(new string[] { "\t" }, StringSplitOptions.None);
                    n.Text = names[1];

                    if (lib.ContainsKey(names[0]))
                    {
                        lib[names[0]].Add(n);
                    }
                    else
                    {
                        lib.Add(names[0], new List<TreeNode>() { n });
                    }
                }
                node.Nodes.Clear();

                foreach (var kvp in lib)
                {
                    ResultsExtractor.DataNode MP1 = null;
                    ResultsExtractor.DataNode MP2 = null;

                    foreach (var n in kvp.Value)
                    {
                        if (n.Text.StartsWith(" Mean_ROI1.Layer1"))
                            continue;
                        else if (n.Text.StartsWith(" Mean_ROI2.Layer1"))
                            continue;
                        else if (n.Text.StartsWith(" Mean_ROI1"))
                            MP1 = (ResultsExtractor.DataNode)n;
                        else if (n.Text.StartsWith(" Mean_ROI2"))
                            MP2 = (ResultsExtractor.DataNode)n;
                    }

                    if (MP1 == null || MP2 == null) continue;

                    ResultsExtractor.DataNode New = new ResultsExtractor.DataNode();
                    New.Text = kvp.Key + "\tFociFrapa";
                    New.Series = new double[MP1.OriginalSeries.Length];
                    New.OriginalSeries = new double[MP1.OriginalSeries.Length];
                    New.NormSeries = new double[MP1.OriginalSeries.Length];
                    New.ImageIndex = 1;
                    New.SelectedImageIndex = 1;
                    New.Tag = MP1.Tag;
                    New.Checked = true;
                    //define variables
                    double I1frapPre = 0, I2frapPre = 0;

                    //calculate the maximum for normalization
                    for (int i = bleaching - avgPreLength; i < bleaching; i++)
                    {
                        I1frapPre += MP1.OriginalSeries[i];
                        I2frapPre += MP2.OriginalSeries[i];
                    }

                    I1frapPre /= avgPreLength;
                    I2frapPre /= avgPreLength;
                    if (I1frapPre == 0) I1frapPre = 1;
                    if (I2frapPre == 0) I2frapPre = 1;

                    //normalisation
                    for (int i = 0; i < MP1.OriginalSeries.Length; i++)
                    {
                        MP1.OriginalSeries[i] /= I1frapPre;
                        MP2.OriginalSeries[i] /= I2frapPre;
                    }
                    //corect the negative values
                    double Ipost = MP1.OriginalSeries[bleaching];
                    for (int i = 0; i < MP1.OriginalSeries.Length; i++)
                    {
                        MP1.OriginalSeries[i] -= Ipost;
                        MP2.OriginalSeries[i] -= Ipost;

                        New.OriginalSeries[i] = MP1.OriginalSeries[i] / MP2.OriginalSeries[i];
                        New.Series[i] = New.OriginalSeries[i];
                        New.NormSeries[i] = New.OriginalSeries[i];
                    }

                    //full scale normalization
                    New.Comment = "";
                    New.RoiName = "Foci FRAPA normalization";

                    node.Nodes.Add(New);
                }

            }
            //refresh vals
            form1.dataTV.RefreshAllNodes();
        }

        #endregion Foci FRAPA
        public class AllModels
        {
            public static void LoadModelNames(ComboBox cmb)
            {
                cmb.Items.AddRange(new string[]
                {
                    "FRAPA - rectangle ROI, single exp",
                    "FRAPA - rectangle ROI, double exp",
                    "FRAPA - oval ROI, single exp",
                    "FRAPA - oval ROI, double exp",
                    "FRAPA - Binding + Diffusion"
                });
            }
            public static int GetModelIndex(string ifFunc)
            {

                switch (ifFunc)
                {
                    case "FRAP_SingleRectangle":
                        return 0;
                    case "FRAP_DoubleRectangle":
                        return 1;
                    case "FRAP_SingleOval":
                        return 2;
                    case "FRAP_DoubleOval":
                        return 3;
                    case "FRAP_Binding+Diffusion":
                        return 4;
                    default:
                        return -1;
                }
            }
            public static SolverFunctions.FunctionValue GetFrapaFunction(int index)
            {
                //extract the model from here

                switch (index)
                {
                    case 0:
                        return Frapa_SingleRectangle.GetFrapaFunction();
                    case 1:
                        return Frapa_DoubleRectangle.GetFrapaFunction();
                    case 2:
                        return Frapa_SingleOval.GetFrapaFunction();
                    case 3:
                        return Frapa_DoubleOval.GetFrapaFunction();
                    case 4:
                        return Frapa_Binding_Diffusion.GetFrapaFunction();
                    default:
                        return null;
                }
            }
            public static void CheckConstValues(int index, ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                //choose constant type

                switch (index)
                {
                    case 0:
                        Frapa_SingleRectangle.CheckConstValues(par);
                        break;
                    case 1:
                        Frapa_DoubleRectangle.CheckConstValues(par);
                        break;
                    case 2:
                        Frapa_SingleOval.CheckConstValues(par);
                        break;
                    case 3:
                        Frapa_DoubleOval.CheckConstValues(par);
                        break;
                    case 4:
                        Frapa_Binding_Diffusion.CheckConstValues(par);
                        break;
                    default:
                        break;
                }

            }
            public static void SetConstValues(MySolver.Parameter par)
            {
                switch (par.Name)
                {
                    case "Io":
                    case "I":
                        par.Value = 1;
                        par.Min = 0;
                        par.Max = 1.5;
                        break;
                    case "A":
                        par.Value = 0.2;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "Ceq":
                        par.Value = 0.6;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "B":
                        par.Value = 0.4;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "D":
                        par.Value = 3;
                        par.Min = 0;
                        par.Max = 1000;
                        break;
                    case "b":
                        par.Value = 0.07;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "Kon":
                        par.Value = 0.05;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "a":
                        par.Value = 0.05;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "Koff":
                        par.Value = 0.01;
                        par.Min = 0;
                        par.Max = 1;
                        break;
                    case "w":
                        par.Value = 3;
                        break;
                    default:
                        break;
                }
            }
            public static Dictionary<string, MySolver.Parameter> Solve(string formula, double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                if (formula.StartsWith("If[FRAP_SingleRectangle"))
                    return Frapa_SingleRectangle.Solve(Xvals, Yvals, coefficients);
                else if (formula.StartsWith("If[FRAP_DoubleRectangle"))
                    return Frapa_DoubleRectangle.Solve(Xvals, Yvals, coefficients);
                else if (formula.StartsWith("If[FRAP_SingleOval"))
                    return Frapa_SingleOval.Solve(Xvals, Yvals, coefficients);
                else if (formula.StartsWith("If[FRAP_DoubleOval"))
                    return Frapa_DoubleOval.Solve(Xvals, Yvals, coefficients);
                else if (formula.StartsWith("If[FRAP_Binding+Diffusion"))
                    return Frapa_Binding_Diffusion.Solve(Xvals, Yvals, coefficients);
                else
                    return null;
            }
            public static double[][] CalcFitVals(string ifVal, double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                switch (ifVal)
                {
                    case "FRAP_SingleRectangle":
                        return Frapa_SingleRectangle.CalcFitVals(Xvals, coefficients);
                    case "FRAP_DoubleRectangle":
                        return Frapa_DoubleRectangle.CalcFitVals(Xvals, coefficients);
                    case "FRAP_SingleOval":
                        return Frapa_SingleOval.CalcFitVals(Xvals, coefficients);
                    case "FRAP_DoubleOval":
                        return Frapa_DoubleOval.CalcFitVals(Xvals, coefficients);
                    case "FRAP_Binding+Diffusion":
                        return Frapa_Binding_Diffusion.CalcFitVals(Xvals, coefficients);
                    default:
                        return null;
                }
            }
            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                switch (curFit.GetFormulaIF)
                {
                    case "FRAP_SingleRectangle":
                        return Frapa_SingleRectangle.ExportResults(curFit, name);
                    case "FRAP_DoubleRectangle":
                        return Frapa_DoubleRectangle.ExportResults(curFit, name);
                    case "FRAP_SingleOval":
                        return Frapa_SingleOval.ExportResults(curFit, name);
                    case "FRAP_DoubleOval":
                        return Frapa_DoubleOval.ExportResults(curFit, name);
                    case "FRAP_Binding+Diffusion":
                        return Frapa_Binding_Diffusion.ExportResults(curFit, name);
                    default:
                        return null;
                }
            }
        }
        #region Double exp, rectangular ROI
        public class Frapa_DoubleRectangle
        {
            public static SolverFunctions.FunctionValue GetFrapaFunction()
            {
                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = "FRAPA - rectangle ROI, double exp";
                f.SetFormulaIF = "FRAP_DoubleRectangle";
                f.SetFormula1 = "{}";
                f.SetFormula2 = "{}";
                f.SetParameters = new string[]
                {
                "w",
                "from",
                "Io",
                "A",
                "a",
                "B",
                "b",
                "T1/2",
                "I",
                "D"
                };

                return f;
            }
            public static void CheckConstValues(ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                switch (par._Name.Text)
                {
                    case ("w"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("from"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("T1/2"):
                        par.IsConstant(true, true);
                        par._Name.Checked = false;
                        break;
                    default:
                        par.IsConstant(false);
                        par._Name.Checked = true;
                        break;
                }
            }
            public static double[][] CalcFitVals(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                double[][] res = new double[2][];
                res[0] = new double[Xvals.Length];
                res[1] = new double[Xvals.Length];

                double wSq = Math.Pow(coefficients["w"].Value, 2);
                double K = 4 * Math.PI * coefficients["D"].Value;
                int frame = (int)coefficients["from"].Value;
                double Io = coefficients["Io"].Value;
                double A = coefficients["A"].Value;
                double a = coefficients["a"].Value;
                double B = coefficients["B"].Value;
                double b = coefficients["b"].Value;
                double I = coefficients["I"].Value;

                if (Xvals.Length > frame)
                {
                    double startT = Xvals[frame];
                    frame++;
                    for (; frame < Xvals.Length; frame++)
                    {
                        //res[0][frame] = A * (1 - Math.Exp(-T *(Xvals[frame]-startT)));//eq for half life
                        res[0][frame] = Io * (1 - A * Math.Exp(-a * (Xvals[frame] - startT))
                            - B * Math.Exp(-b * (Xvals[frame] - startT)));//eq for half life
                        res[1][frame] = I * (1 - Math.Pow(wSq / (wSq + K * (Xvals[frame] - startT)), 0.5));//diffusion
                    }
                }

                return res;
            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPAHalftime(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parIo = coefficients["Io"];
                MySolver.Parameter parA = coefficients["A"];
                MySolver.Parameter para = coefficients["a"];
                MySolver.Parameter parB = coefficients["B"];
                MySolver.Parameter parb = coefficients["b"];

                #region Add decisions
                Decision Io = null;

                if (parIo.Variable)
                    Io = new Decision(Domain.RealRange(parIo.Min, parIo.Max), "Io");
                else
                    Io = new Decision(Domain.RealRange(parIo.Value, parIo.Value), "Io");

                Io.SetInitialValue(parIo.Value);
                model.AddDecision(Io);

                Decision A = null;
                if (parA.Variable)
                    A = new Decision(Domain.RealRange(parA.Min, parA.Max), "A");
                else
                    A = new Decision(Domain.RealRange(parA.Value, parA.Value), "A");
                A.SetInitialValue(parA.Value);
                model.AddDecision(A);

                Decision a = null;
                if (para.Variable)
                    a = new Decision(Domain.RealRange(para.Min, para.Max), "a");
                else
                    a = new Decision(Domain.RealRange(para.Value, para.Value), "a");
                a.SetInitialValue(para.Value);
                model.AddDecision(a);

                Decision B = null;
                if (parB.Variable)
                    B = new Decision(Domain.RealRange(parB.Min, parB.Max), "B");
                else
                    B = new Decision(Domain.RealRange(parB.Value, parB.Value), "B");
                B.SetInitialValue(parB.Value);
                model.AddDecision(B);

                Decision b = null;
                if (parb.Variable)
                    b = new Decision(Domain.RealRange(parb.Min, parb.Max), "b");
                else
                    b = new Decision(Domain.RealRange(parb.Value, parb.Value), "b");
                b.SetInitialValue(parb.Value);
                model.AddDecision(b);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
                Io * (1 - A * Model.Exp(-a * xParam[i]) - B * Model.Exp(-b * xParam[i]))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPADiffusion(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parI = coefficients["I"];
                MySolver.Parameter parD = coefficients["D"];

                #region Add decisions

                Decision I = null;
                if (parI.Variable)
                    I = new Decision(Domain.RealRange(parI.Min, parI.Max), "I");
                else
                    I = new Decision(Domain.RealRange(parI.Value, parI.Value), "I");
                I.SetInitialValue(parI.Value);
                model.AddDecision(I);

                Decision D = null;
                if (parD.Variable)
                    D = new Decision(Domain.RealRange(parD.Min, parD.Max), "D");
                else
                    D = new Decision(Domain.RealRange(parD.Value, parD.Value), "D");
                D.SetInitialValue(parD.Value);
                model.AddDecision(D);

                Microsoft.SolverFoundation.Services.Parameter parW =
                            new Microsoft.SolverFoundation.Services.Parameter(
                                Domain.Real, "W");
                parW.SetBinding(Math.Pow(coefficients["w"].Value, 2));
                model.AddParameters(parW);

                Microsoft.SolverFoundation.Services.Parameter parPI =
                            new Microsoft.SolverFoundation.Services.Parameter(
                                Domain.Real, "PI");

                parPI.SetBinding(4 * Math.PI);
                model.AddParameters(parPI);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
               I * (1 - Model.Power(parW / (parW + parPI * D * xParam[i]), 0.5))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                var parHT = SolveFRAPAHalftime(Xvals, Yvals, coefficients);//halftime model
                var parDiff = SolveFRAPADiffusion(Xvals, Yvals, coefficients);//diffusion model

                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (var kvp in coefficients)
                {
                    MySolver.Parameter p = kvp.Value;

                    if (parHT.Keys.Contains(kvp.Key))
                        parHT.TryGetValue(kvp.Key, out p);
                    else if (parDiff.Keys.Contains(kvp.Key))
                        parDiff.TryGetValue(kvp.Key, out p);

                    //parameters.Add(kvp.Key, p);
                    parameters.Add(kvp.Key, new MySolver.Parameter
                   (p.Name, p.Value, p.Min, p.Max, p.Variable));
                }

                //parameters["T1/2"].Value = Math.Log(0.5) / -parameters["a"].Value;
                FindHalfTime(Xvals, parameters);

                return parameters;
            }

            private static void FindHalfTime(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                double Io = coefficients["Io"].Value;
                double A = coefficients["A"].Value;
                double a = coefficients["a"].Value;
                double B = coefficients["B"].Value;
                double b = coefficients["b"].Value;

                Decision t = new Decision(Domain.RealRange(0, Xvals[Xvals.Length - 1] - Xvals[frame]), "t");
                t.SetInitialValue(0);
                model.AddDecision(t);

                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                Model.Abs(Io * (1 - A * Model.Exp(-a * t) - B * Model.Exp(-b * t)) - Io * 0.5)//HalfTime formula
                );

                //solve
                Solution solution = solver.Solve();
                //return the result

                foreach (Decision parameter in model.Decisions)
                    if (parameter.Name == "t")
                    {
                        coefficients["T1/2"].Value = parameter.ToDouble();
                        break;
                    }

            }
            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                List<string> constNames = new List<string>() { "", "Const_" + name, "" };
                List<string> constVals = new List<string>() { "", "Value_" + name, "" };
                List<string> XValsL = new List<string>() { "", "Xvals_" + name, "" };
                List<string> YValsL = new List<string>() { "", "Raw_" + name, "" };
                List<string> Fit1ValsL = new List<string>() { "", "Fit_FRAP_eq_" + name, "" };
                List<string> Fit2ValsL = new List<string>() { "", "Fit_Diffusion_eq_" + name, "" };

                foreach (var p in curFit.Parameters)
                {
                    constNames.Add(p.Value.Name);
                    constVals.Add(p.Value.Value.ToString());
                }

                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[] StDev = new double[2];
                double[][] fitVals = CalcFitVals(Xvals, curFit.Parameters);
                int frame = (int)curFit.Parameters["from"].Value;

                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                {
                    Fit1ValsL.Add(fitVals[0][i].ToString());
                    Fit2ValsL.Add(fitVals[1][i].ToString());

                    XValsL.Add(Xvals[i].ToString());
                    YValsL.Add(Yvals[i].ToString());

                    if (i >= frame)
                    {
                        StDev[0] += Math.Pow(Yvals[i] - fitVals[0][i], 2);
                        StDev[1] += Math.Pow(Yvals[i] - fitVals[1][i], 2);
                    }
                }
                curFit.StDev = 0;
                StDev[0] = Math.Sqrt(StDev[0] / (Xvals.Length - frame));
                StDev[1] = Math.Sqrt(StDev[1] / (Xvals.Length - frame));

                constNames.Add("");
                constVals.Add("");
                constNames.Add("Root mean square deviation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(StDev[0].ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(StDev[1].ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Coefficient of correlation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[0], frame).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[1], frame).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("R-squared:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[0], frame), 2).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[1], frame), 2).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("FRAP eq.:");
                constVals.Add("f(t)=Io*(1-A*Exp[-a*t]-B*Exp[-b*t])");
                constNames.Add("Diffusion eq.:");
                constVals.Add("f(t)=I*(1-Pow[Pow[w,2]/(Pow[w,2]+4*PI*D*t),0.5])");

                #region Fit Statistics
                fitVals = null;

                #endregion Fit Statistics
                List<List<string>> FitRes = new List<List<string>>();

                FitRes.Add(constNames);
                FitRes.Add(constVals);
                FitRes.Add(XValsL);
                FitRes.Add(YValsL);
                FitRes.Add(Fit1ValsL);
                FitRes.Add(Fit2ValsL);

                return FitRes;
            }
        }
        #endregion double exp, rectangular ROI
        #region Single exp, rectangular ROI
        public class Frapa_SingleRectangle
        {
            public static SolverFunctions.FunctionValue GetFrapaFunction()
            {
                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = "FRAPA - rectangle ROI, single exp";
                f.SetFormulaIF = "FRAP_SingleRectangle";
                f.SetFormula1 = "{}";
                f.SetFormula2 = "{}";
                f.SetParameters = new string[]
                {
                "w",
                "from",
                "Io",
                "a",
                "T1/2",
                "I",
                "D"
                };

                return f;
            }
            public static void CheckConstValues(ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                switch (par._Name.Text)
                {
                    case ("w"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("from"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("T1/2"):
                        par.IsConstant(true, true);
                        par._Name.Checked = false;
                        break;
                    default:
                        par.IsConstant(false);
                        par._Name.Checked = true;
                        break;
                }
            }
            public static double[][] CalcFitVals(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                double[][] res = new double[2][];
                res[0] = new double[Xvals.Length];
                res[1] = new double[Xvals.Length];

                double wSq = Math.Pow(coefficients["w"].Value, 2);
                double K = 4 * Math.PI * coefficients["D"].Value;
                int frame = (int)coefficients["from"].Value;
                double Io = coefficients["Io"].Value;
                double a = coefficients["a"].Value;
                double I = coefficients["I"].Value;

                if (Xvals.Length > frame)
                {
                    double startT = Xvals[frame];
                    frame++;
                    for (; frame < Xvals.Length; frame++)
                    {
                        res[0][frame] = Io * (1 - Math.Exp(-a * (Xvals[frame] - startT)));//eq for half life
                        res[1][frame] = I * (1 - Math.Pow(wSq / (wSq + K * (Xvals[frame] - startT)), 0.5));//diffusion
                    }
                }

                return res;
            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPAHalftime(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parIo = coefficients["Io"];
                MySolver.Parameter para = coefficients["a"];

                #region Add decisions
                Decision Io = null;

                if (parIo.Variable)
                    Io = new Decision(Domain.RealRange(parIo.Min, parIo.Max), "Io");
                else
                    Io = new Decision(Domain.RealRange(parIo.Value, parIo.Value), "Io");

                Io.SetInitialValue(parIo.Value);
                model.AddDecision(Io);

                Decision a = null;
                if (para.Variable)
                    a = new Decision(Domain.RealRange(para.Min, para.Max), "a");
                else
                    a = new Decision(Domain.RealRange(para.Value, para.Value), "a");
                a.SetInitialValue(para.Value);
                model.AddDecision(a);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
                Io * (1 - Model.Exp(-a * xParam[i]))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPADiffusion(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parI = coefficients["I"];
                MySolver.Parameter parD = coefficients["D"];

                #region Add decisions

                Decision I = null;
                if (parI.Variable)
                    I = new Decision(Domain.RealRange(parI.Min, parI.Max), "I");
                else
                    I = new Decision(Domain.RealRange(parI.Value, parI.Value), "I");
                I.SetInitialValue(parI.Value);
                model.AddDecision(I);

                Decision D = null;
                if (parD.Variable)
                    D = new Decision(Domain.RealRange(parD.Min, parD.Max), "D");
                else
                    D = new Decision(Domain.RealRange(parD.Value, parD.Value), "D");
                D.SetInitialValue(parD.Value);
                model.AddDecision(D);

                Microsoft.SolverFoundation.Services.Parameter parW =
                            new Microsoft.SolverFoundation.Services.Parameter(
                                Domain.Real, "W");
                parW.SetBinding(Math.Pow(coefficients["w"].Value, 2));
                model.AddParameters(parW);

                Microsoft.SolverFoundation.Services.Parameter parPI =
                            new Microsoft.SolverFoundation.Services.Parameter(
                                Domain.Real, "PI");

                parPI.SetBinding(4 * Math.PI);
                model.AddParameters(parPI);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
               I * (1 - Model.Power(parW / (parW + parPI * D * xParam[i]), 0.5))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                var parHT = SolveFRAPAHalftime(Xvals, Yvals, coefficients);//halftime model
                var parDiff = SolveFRAPADiffusion(Xvals, Yvals, coefficients);//diffusion model

                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (var kvp in coefficients)
                {
                    MySolver.Parameter p = kvp.Value;

                    if (parHT.Keys.Contains(kvp.Key))
                        parHT.TryGetValue(kvp.Key, out p);
                    else if (parDiff.Keys.Contains(kvp.Key))
                        parDiff.TryGetValue(kvp.Key, out p);

                    //parameters.Add(kvp.Key, p);
                    parameters.Add(kvp.Key, new MySolver.Parameter
                   (p.Name, p.Value, p.Min, p.Max, p.Variable));
                }

                //parameters["T1/2"].Value = Math.Log(0.5) / -parameters["a"].Value;
                FindHalfTime(Xvals, parameters);

                return parameters;
            }

            private static void FindHalfTime(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                double Io = coefficients["Io"].Value;
                double a = coefficients["a"].Value;

                Decision t = new Decision(Domain.RealRange(0, Xvals[Xvals.Length - 1] - Xvals[frame]), "t");
                t.SetInitialValue(0);
                model.AddDecision(t);

                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                Model.Abs(Io * (1 - Model.Exp(-a * t)) - Io * 0.5)//HalfTime formula
                );

                //solve
                Solution solution = solver.Solve();
                //return the result

                foreach (Decision parameter in model.Decisions)
                    if (parameter.Name == "t")
                    {
                        coefficients["T1/2"].Value = parameter.ToDouble();
                        break;
                    }

            }
            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                List<string> constNames = new List<string>() { "", "Const_" + name, "" };
                List<string> constVals = new List<string>() { "", "Value_" + name, "" };
                List<string> XValsL = new List<string>() { "", "Xvals_" + name, "" };
                List<string> YValsL = new List<string>() { "", "Raw_" + name, "" };
                List<string> Fit1ValsL = new List<string>() { "", "Fit_FRAP_eq_" + name, "" };
                List<string> Fit2ValsL = new List<string>() { "", "Fit_Diffusion_eq_" + name, "" };

                foreach (var p in curFit.Parameters)
                {
                    constNames.Add(p.Value.Name);
                    constVals.Add(p.Value.Value.ToString());
                }

                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[] StDev = new double[2];
                double[][] fitVals = CalcFitVals(Xvals, curFit.Parameters);
                int frame = (int)curFit.Parameters["from"].Value;

                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                {
                    Fit1ValsL.Add(fitVals[0][i].ToString());
                    Fit2ValsL.Add(fitVals[1][i].ToString());

                    XValsL.Add(Xvals[i].ToString());
                    YValsL.Add(Yvals[i].ToString());

                    if (i >= frame)
                    {
                        StDev[0] += Math.Pow(Yvals[i] - fitVals[0][i], 2);
                        StDev[1] += Math.Pow(Yvals[i] - fitVals[1][i], 2);
                    }
                }
                curFit.StDev = 0;
                StDev[0] = Math.Sqrt(StDev[0] / (Xvals.Length - frame));
                StDev[1] = Math.Sqrt(StDev[1] / (Xvals.Length - frame));

                constNames.Add("");
                constVals.Add("");
                constNames.Add("Root mean square deviation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(StDev[0].ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(StDev[1].ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Coefficient of correlation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[0], frame).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[1], frame).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("R-squared:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[0], frame), 2).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[1], frame), 2).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("FRAP eq.:");
                constVals.Add("f(t)=Io*(1-Exp[-a*t])");
                constNames.Add("Diffusion eq.:");
                constVals.Add("f(t)=I*(1-Pow[Pow[w,2]/(Pow[w,2]+4*PI*D*t),0.5])");

                #region Fit Statistics
                fitVals = null;

                #endregion Fit Statistics
                List<List<string>> FitRes = new List<List<string>>();

                FitRes.Add(constNames);
                FitRes.Add(constVals);
                FitRes.Add(XValsL);
                FitRes.Add(YValsL);
                FitRes.Add(Fit1ValsL);
                FitRes.Add(Fit2ValsL);

                return FitRes;
            }
        }
        #endregion Single exp, rectangular ROI

        #region Double exp, oval ROI
        public class Frapa_DoubleOval
        {
            public static SolverFunctions.FunctionValue GetFrapaFunction()
            {
                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = "FRAPA - oval ROI, double exp";
                f.SetFormulaIF = "FRAP_DoubleOval";
                f.SetFormula1 = "{}";
                f.SetFormula2 = "{}";
                f.SetParameters = new string[]
                {
                "w",
                "from",
                "Io",
                "A",
                "a",
                "B",
                "b",
                "T1/2",
                "I",
                "D"
                };

                return f;
            }
            public static void CheckConstValues(ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                switch (par._Name.Text)
                {
                    case ("w"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("from"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("T1/2"):
                        par.IsConstant(true, true);
                        par._Name.Checked = false;
                        break;
                    default:
                        par.IsConstant(false);
                        par._Name.Checked = true;
                        break;
                }
            }
            public static double[][] CalcFitVals(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                double[][] res = new double[2][];
                res[0] = new double[Xvals.Length];
                res[1] = new double[Xvals.Length];


                int frame = (int)coefficients["from"].Value;
                double Io = coefficients["Io"].Value;
                double A = coefficients["A"].Value;
                double a = coefficients["a"].Value;
                double B = coefficients["B"].Value;
                double b = coefficients["b"].Value;
                double I = coefficients["I"].Value;
                double wSq = 0.5 * Math.Pow(coefficients["w"].Value, 2) / coefficients["D"].Value;

                if (Xvals.Length > frame)
                {
                    double startT = Xvals[frame];
                    frame++;
                    for (; frame < Xvals.Length; frame++)
                    {
                        res[0][frame] = Io * (1 - A * Math.Exp(-a * (Xvals[frame] - startT))
                             - B * Math.Exp(-b * (Xvals[frame] - startT)));//eq for half life
                                                                           //res[1][frame] = I * Math.Pow(1 - wSq / (wSq + K * (Xvals[frame] - startT)), 0.5);//diffusion
                        double K = wSq / (Xvals[frame] - startT);

                        res[1][frame] = I * Math.Exp(-K) * (MathNet.Numerics.SpecialFunctions.BesselI0(K) + MathNet.Numerics.SpecialFunctions.BesselI1(K));//diffusion}
                    }
                }

                return res;
            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPAHalftime(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parIo = coefficients["Io"];
                MySolver.Parameter parA = coefficients["A"];
                MySolver.Parameter para = coefficients["a"];
                MySolver.Parameter parB = coefficients["B"];
                MySolver.Parameter parb = coefficients["b"];

                #region Add decisions
                Decision Io = null;

                if (parIo.Variable)
                    Io = new Decision(Domain.RealRange(parIo.Min, parIo.Max), "Io");
                else
                    Io = new Decision(Domain.RealRange(parIo.Value, parIo.Value), "Io");

                Io.SetInitialValue(parIo.Value);
                model.AddDecision(Io);

                Decision A = null;
                if (parA.Variable)
                    A = new Decision(Domain.RealRange(parA.Min, parA.Max), "A");
                else
                    A = new Decision(Domain.RealRange(parA.Value, parA.Value), "A");
                A.SetInitialValue(parA.Value);
                model.AddDecision(A);

                Decision a = null;
                if (para.Variable)
                    a = new Decision(Domain.RealRange(para.Min, para.Max), "a");
                else
                    a = new Decision(Domain.RealRange(para.Value, para.Value), "a");
                a.SetInitialValue(para.Value);
                model.AddDecision(a);

                Decision B = null;
                if (parB.Variable)
                    B = new Decision(Domain.RealRange(parB.Min, parB.Max), "B");
                else
                    B = new Decision(Domain.RealRange(parB.Value, parB.Value), "B");
                B.SetInitialValue(parB.Value);
                model.AddDecision(B);

                Decision b = null;
                if (parb.Variable)
                    b = new Decision(Domain.RealRange(parb.Min, parb.Max), "b");
                else
                    b = new Decision(Domain.RealRange(parb.Value, parb.Value), "b");
                b.SetInitialValue(parb.Value);
                model.AddDecision(b);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
                Io * (1 - A * Model.Exp(-a * xParam[i]) - B * Model.Exp(-b * xParam[i]))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPADiffusion(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                SolveDeiffusionOval solver = new SolveDeiffusionOval();
                var res = solver.Solve(Xvals, Yvals, coefficients);
                return res;
            }

            public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                var parHT = SolveFRAPAHalftime(Xvals, Yvals, coefficients);//halftime model
                var parDiff = SolveFRAPADiffusion(Xvals, Yvals, coefficients);//diffusion model

                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (var kvp in coefficients)
                {
                    MySolver.Parameter p = kvp.Value;

                    if (parHT.Keys.Contains(kvp.Key))
                        parHT.TryGetValue(kvp.Key, out p);
                    else if (parDiff.Keys.Contains(kvp.Key))
                        parDiff.TryGetValue(kvp.Key, out p);

                    //parameters.Add(kvp.Key, p);
                    parameters.Add(kvp.Key, new MySolver.Parameter
                   (p.Name, p.Value, p.Min, p.Max, p.Variable));
                }

                //parameters["T1/2"].Value = Math.Log(0.5) / -parameters["a"].Value;
                FindHalfTime(Xvals, parameters);

                return parameters;
            }

            private static void FindHalfTime(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                double Io = coefficients["Io"].Value;
                double A = coefficients["A"].Value;
                double a = coefficients["a"].Value;
                double B = coefficients["B"].Value;
                double b = coefficients["b"].Value;

                Decision t = new Decision(Domain.RealRange(0, Xvals[Xvals.Length - 1] - Xvals[frame]), "t");
                t.SetInitialValue(0);
                model.AddDecision(t);

                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                Model.Abs(Io * (1 - A * Model.Exp(-a * t) - B * Model.Exp(-b * t)) - Io * 0.5)//HalfTime formula
                );

                //solve
                Solution solution = solver.Solve();
                //return the result

                foreach (Decision parameter in model.Decisions)
                    if (parameter.Name == "t")
                    {
                        coefficients["T1/2"].Value = parameter.ToDouble();
                        break;
                    }

            }
            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                List<string> constNames = new List<string>() { "", "Const_" + name, "" };
                List<string> constVals = new List<string>() { "", "Value_" + name, "" };
                List<string> XValsL = new List<string>() { "", "Xvals_" + name, "" };
                List<string> YValsL = new List<string>() { "", "Raw_" + name, "" };
                List<string> Fit1ValsL = new List<string>() { "", "Fit_FRAP_eq_" + name, "" };
                List<string> Fit2ValsL = new List<string>() { "", "Fit_Diffusion_eq_" + name, "" };

                foreach (var p in curFit.Parameters)
                {
                    constNames.Add(p.Value.Name);
                    constVals.Add(p.Value.Value.ToString());
                }

                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[] StDev = new double[2];
                double[][] fitVals = CalcFitVals(Xvals, curFit.Parameters);
                int frame = (int)curFit.Parameters["from"].Value;

                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                {
                    Fit1ValsL.Add(fitVals[0][i].ToString());
                    Fit2ValsL.Add(fitVals[1][i].ToString());

                    XValsL.Add(Xvals[i].ToString());
                    YValsL.Add(Yvals[i].ToString());

                    if (i >= frame)
                    {
                        StDev[0] += Math.Pow(Yvals[i] - fitVals[0][i], 2);
                        StDev[1] += Math.Pow(Yvals[i] - fitVals[1][i], 2);
                    }
                }
                curFit.StDev = 0;
                StDev[0] = Math.Sqrt(StDev[0] / (Xvals.Length - frame));
                StDev[1] = Math.Sqrt(StDev[1] / (Xvals.Length - frame));

                constNames.Add("");
                constVals.Add("");
                constNames.Add("Root mean square deviation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(StDev[0].ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(StDev[1].ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Coefficient of correlation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[0], frame).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[1], frame).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("R-squared:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[0], frame), 2).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[1], frame), 2).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("FRAP eq.:");
                constVals.Add("f(t)=Io*(1-A*Exp[-a*t]-B*Exp[-b*t])");
                constNames.Add("Diffusion eq.:");
                constVals.Add("f(t)=I*Exp[-(Pow[w,2]/D)/(2*t)]*(I0[(Pow[w,2]/D)/(2*t)]+I1[(Pow[w,2]/D)/(2*t)])");

                #region Fit Statistics
                fitVals = null;

                #endregion Fit Statistics
                List<List<string>> FitRes = new List<List<string>>();

                FitRes.Add(constNames);
                FitRes.Add(constVals);
                FitRes.Add(XValsL);
                FitRes.Add(YValsL);
                FitRes.Add(Fit1ValsL);
                FitRes.Add(Fit2ValsL);

                return FitRes;
            }
        }
        #endregion double exp, oval ROI
        #region Single exp, oval ROI
        public class Frapa_SingleOval
        {
            public static SolverFunctions.FunctionValue GetFrapaFunction()
            {
                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = "FRAPA - oval ROI, single exp";
                f.SetFormulaIF = "FRAP_SingleOval";
                f.SetFormula1 = "{}";
                f.SetFormula2 = "{}";
                f.SetParameters = new string[]
                {
                "w",
                "from",
                "Io",
                "a",
                "T1/2",
                "I",
                "D"
                };

                return f;
            }
            public static void CheckConstValues(ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                switch (par._Name.Text)
                {
                    case ("w"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("from"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("T1/2"):
                        par.IsConstant(true, true);
                        par._Name.Checked = false;
                        break;
                    default:
                        par.IsConstant(false);
                        par._Name.Checked = true;
                        break;
                }
            }
            public static double[][] CalcFitVals(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                double[][] res = new double[2][];
                res[0] = new double[Xvals.Length];
                res[1] = new double[Xvals.Length];

                int frame = (int)coefficients["from"].Value;
                double Io = coefficients["Io"].Value;
                double a = coefficients["a"].Value;
                double I = coefficients["I"].Value;
                double wSq = 0.5 * Math.Pow(coefficients["w"].Value, 2) / coefficients["D"].Value;

                if (Xvals.Length > frame)
                {
                    double startT = Xvals[frame];
                    frame++;
                    for (; frame < Xvals.Length; frame++)
                    {
                        res[0][frame] = Io * (1 - Math.Exp(-a * (Xvals[frame] - startT)));//eq for half life

                        double K = wSq / (Xvals[frame] - startT);

                        res[1][frame] = I * Math.Exp(-K) * (MathNet.Numerics.SpecialFunctions.BesselI0(K) + MathNet.Numerics.SpecialFunctions.BesselI1(K));//diffusion}
                    }
                }

                return res;
            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPAHalftime(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                // create solver model
                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                //variables
                MySolver.Parameter parIo = coefficients["Io"];
                MySolver.Parameter para = coefficients["a"];

                #region Add decisions
                Decision Io = null;

                if (parIo.Variable)
                    Io = new Decision(Domain.RealRange(parIo.Min, parIo.Max), "Io");
                else
                    Io = new Decision(Domain.RealRange(parIo.Value, parIo.Value), "Io");

                Io.SetInitialValue(parIo.Value);
                model.AddDecision(Io);

                Decision a = null;
                if (para.Variable)
                    a = new Decision(Domain.RealRange(para.Min, para.Max), "a");
                else
                    a = new Decision(Domain.RealRange(para.Value, para.Value), "a");
                a.SetInitialValue(para.Value);
                model.AddDecision(a);

                #endregion Add decisions

                #region raw data
                Set IndexSet = new Set(Domain.Any, "Index");

                Parameter xParam = new Parameter(Domain.Real, "XParam", IndexSet);
                Parameter yParam = new Parameter(Domain.Real, "YParam", IndexSet);
                Parameter Param = new Parameter(Domain.Real, "Param", IndexSet, IndexSet);

                var XUnPairedValues = new List<Tuple<string, double>>();
                var YUnPairedValues = new List<Tuple<string, double>>();

                for (int i = frame; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    XUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Xvals[i] - Xvals[frame]));
                    YUnPairedValues.Add(new Tuple<string, double>(i.ToString(), Yvals[i]));
                }

                xParam.SetBinding(XUnPairedValues, "Item2", "Item1");
                yParam.SetBinding(YUnPairedValues, "Item2", "Item1");

                model.AddParameters(xParam, yParam);

                #endregion raw data
                //Add the goal
                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                    Model.Sum(Model.ForEach(IndexSet, i =>
                Model.Power(yParam[i] - (
                Io * (1 - Model.Exp(-a * xParam[i]))//HalfTime formula
                )
                , 2)
                )));

                //solve
                Solution solution = solver.Solve();
                //return the result
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (Decision parameter in model.Decisions)
                {
                    MySolver.Parameter p;

                    if (!coefficients.TryGetValue(parameter.Name, out p))
                        p = new MySolver.Parameter(parameter.Name);

                    parameters.Add(parameter.Name, new MySolver.Parameter
                   (parameter.Name, parameter.ToDouble(), p.Min, p.Max, p.Variable));
                }

                return parameters;

            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPADiffusion(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                SolveDeiffusionOval solver = new SolveDeiffusionOval();
                var res = solver.Solve(Xvals, Yvals, coefficients);
                return res;
            }
            public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                var parHT = SolveFRAPAHalftime(Xvals, Yvals, coefficients);//halftime model
                var parDiff = SolveFRAPADiffusion(Xvals, Yvals, coefficients);//diffusion model

                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                foreach (var kvp in coefficients)
                {
                    MySolver.Parameter p = kvp.Value;

                    if (parHT.Keys.Contains(kvp.Key))
                        parHT.TryGetValue(kvp.Key, out p);
                    else if (parDiff.Keys.Contains(kvp.Key))
                        parDiff.TryGetValue(kvp.Key, out p);

                    //parameters.Add(kvp.Key, p);
                    parameters.Add(kvp.Key, new MySolver.Parameter
                   (p.Name, p.Value, p.Min, p.Max, p.Variable));
                }

                //parameters["T1/2"].Value = Math.Log(0.5) / -parameters["a"].Value;
                FindHalfTime(Xvals, parameters);

                return parameters;
            }

            private static void FindHalfTime(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                SolverContext solver = SolverContext.GetContext();
                solver.ClearModel();
                Model model = solver.CreateModel();

                double Io = coefficients["Io"].Value;
                double a = coefficients["a"].Value;

                Decision t = new Decision(Domain.RealRange(0, Xvals[Xvals.Length - 1] - Xvals[frame]), "t");
                t.SetInitialValue(0);
                model.AddDecision(t);

                model.AddGoal("SumOfSquaredErrors",
                    GoalKind.Minimize,
                Model.Abs(Io * (1 - Model.Exp(-a * t)) - Io * 0.5)//HalfTime formula
                );

                //solve
                Solution solution = solver.Solve();
                //return the result

                foreach (Decision parameter in model.Decisions)
                    if (parameter.Name == "t")
                    {
                        coefficients["T1/2"].Value = parameter.ToDouble();
                        break;
                    }

            }
            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                List<string> constNames = new List<string>() { "", "Const_" + name, "" };
                List<string> constVals = new List<string>() { "", "Value_" + name, "" };
                List<string> XValsL = new List<string>() { "", "Xvals_" + name, "" };
                List<string> YValsL = new List<string>() { "", "Raw_" + name, "" };
                List<string> Fit1ValsL = new List<string>() { "", "Fit_FRAP_eq_" + name, "" };
                List<string> Fit2ValsL = new List<string>() { "", "Fit_Diffusion_eq_" + name, "" };

                foreach (var p in curFit.Parameters)
                {
                    constNames.Add(p.Value.Name);
                    constVals.Add(p.Value.Value.ToString());
                }

                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[] StDev = new double[2];
                double[][] fitVals = CalcFitVals(Xvals, curFit.Parameters);
                int frame = (int)curFit.Parameters["from"].Value;

                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                {
                    Fit1ValsL.Add(fitVals[0][i].ToString());
                    Fit2ValsL.Add(fitVals[1][i].ToString());

                    XValsL.Add(Xvals[i].ToString());
                    YValsL.Add(Yvals[i].ToString());

                    if (i >= frame)
                    {
                        StDev[0] += Math.Pow(Yvals[i] - fitVals[0][i], 2);
                        StDev[1] += Math.Pow(Yvals[i] - fitVals[1][i], 2);
                    }
                }
                curFit.StDev = 0;
                StDev[0] = Math.Sqrt(StDev[0] / (Xvals.Length - frame));
                StDev[1] = Math.Sqrt(StDev[1] / (Xvals.Length - frame));

                constNames.Add("");
                constVals.Add("");
                constNames.Add("Root mean square deviation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(StDev[0].ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(StDev[1].ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Coefficient of correlation:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[0], frame).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[1], frame).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("R-squared:");
                constVals.Add("");
                constNames.Add("FRAP eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[0], frame), 2).ToString());
                constNames.Add("Diffusion eq.");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[1], frame), 2).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("FRAP eq.:");
                constVals.Add("f(t)=Io*(1-Exp[-a*t])");
                constNames.Add("Diffusion eq.:");
                constVals.Add("f(t)=I*Exp[-(Pow[w,2]/D)/(2*t)]*(I0[(Pow[w,2]/D)/(2*t)]+I1[(Pow[w,2]/D)/(2*t)])");

                #region Fit Statistics
                fitVals = null;

                #endregion Fit Statistics
                List<List<string>> FitRes = new List<List<string>>();

                FitRes.Add(constNames);
                FitRes.Add(constVals);
                FitRes.Add(XValsL);
                FitRes.Add(YValsL);
                FitRes.Add(Fit1ValsL);
                FitRes.Add(Fit2ValsL);

                return FitRes;
            }
        }
        #endregion Single exp, oval ROI

        #region Binding&Diffusion
        public class Frapa_Binding_Diffusion
        {

            public static SolverFunctions.FunctionValue GetFrapaFunction()
            {
                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = "FRAPA - Binding + Diffusion";
                f.SetFormulaIF = "FRAP_Binding+Diffusion";
                f.SetFormula1 = "{}";
                f.SetFormula2 = "{}";
                f.SetParameters = new string[]
                {
                "w",
                "from",
                "I",
                "Ceq",
                "D",
                "Koff",
                "Kon",
                "T1/2"
                };

                return f;
            }
            public static void CheckConstValues(ChartSolverSettings.ParamPanel.ParameterBox par)
            {
                switch (par._Name.Text)
                {
                    case ("w"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("from"):
                        par.IsConstant(true);
                        par._Name.Checked = false;
                        break;
                    case ("T1/2"):
                        par.IsConstant(true, true);
                        par._Name.Checked = false;
                        break;
                    default:
                        par.IsConstant(false);
                        par._Name.Checked = true;
                        break;
                }
            }
            public static double[][] CalcFitVals(double[] Xvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                double[][] res = new double[1][];
                res[0] = new double[Xvals.Length];

                int frame = (int)coefficients["from"].Value;
                double w = coefficients["w"].Value;
                double I = coefficients["I"].Value;
                double Ceq = coefficients["Ceq"].Value;
                //coefficients["Feq"].Value = 1 - Ceq;
                // double Feq = coefficients["Feq"].Value;
                double Feq = 1 - Ceq;
                double D = coefficients["D"].Value;
                double Kon = coefficients["Kon"].Value;
                double Koff = coefficients["Koff"].Value;

                if (Xvals.Length > frame)
                {
                    Laplace lap = new Laplace();
                    lap.InitStehfest(14);

                    double startT = Xvals[frame];
                    frame++;
                    for (; frame < Xvals.Length; frame++)
                    {
                        res[0][frame] = (I) * lap.InverseTransform(I, w, Feq, Ceq, Kon, Koff, D, (Xvals[frame] - startT));
                    }
                }

                return res;
            }
            private static Dictionary<string, MySolver.Parameter> SolveFRAPA(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;
                double startT = Xvals[frame];
                //calculate XY values
                double[] newXvals = new double[Xvals.Length - frame];
                double[] newYvals = new double[newXvals.Length];
                Array.Copy(Xvals, frame, newXvals, 0, newXvals.Length);

                for (int i = 0; i < newXvals.Length; i++)
                    newXvals[i] -= startT;

                Array.Copy(Yvals, frame, newYvals, 0, newXvals.Length);
                //create data set
                FRAPA_ReactionDiffusion.DataSet data = new FRAPA_ReactionDiffusion.DataSet("", "", newXvals, newYvals);

                List<FRAPA_ReactionDiffusion.Variable> Variables = new List<FRAPA_ReactionDiffusion.Variable>();
                string[] names = new string[] {
                "I",
                "w",
                "Kon",
                "Koff",
                "D",
                "Ceq",
                "T1/2"
                };

                foreach (string str in names)
                {
                    MySolver.Parameter par = coefficients[str];

                    FRAPA_ReactionDiffusion.Variable v = new FRAPA_ReactionDiffusion.Variable();
                    v.ConstName = par.Name;
                    v.ConstValue = par.Value;

                    if (par.Variable)
                    {
                        v.ConstMin = par.Min;
                        v.ConstMax = par.Max;
                    }
                    else
                    {
                        v.ConstMin = par.Value;
                        v.ConstMax = par.Value;
                    }

                    Variables.Add(v);
                }

                data.Variables = Variables;

                FRAPA_ReactionDiffusion.MySolver mySolver = new FRAPA_ReactionDiffusion.MySolver();
                mySolver.Solve(data, 1000);

                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                for (int i = 0; i < coefficients.Count; i++)
                {
                    MySolver.Parameter p = coefficients.ElementAt(i).Value;

                    parameters.Add(p.Name, new MySolver.Parameter
                (p.Name, p.Value, p.Min, p.Max, p.Variable));
                }

                foreach (var v in Variables)
                {
                    parameters[v.ConstName].Value = v.ConstValue;
                }

                return parameters;
            }

            public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                Dictionary<string, MySolver.Parameter> parameters = SolveFRAPA(Xvals, Yvals, coefficients);//halftime model

                return parameters;
            }

            public static List<List<string>> ExportResults(MySolver.FitSettings curFit, string name)
            {
                List<string> constNames = new List<string>() { "", "Const_" + name, "" };
                List<string> constVals = new List<string>() { "", "Value_" + name, "" };
                List<string> XValsL = new List<string>() { "", "Xvals_" + name, "" };
                List<string> YValsL = new List<string>() { "", "Raw_" + name, "" };
                List<string> Fit1ValsL = new List<string>() { "", "Fit_Binding+Diffusion_eq_" + name, "" };

                foreach (var p in curFit.Parameters)
                {
                    constNames.Add(p.Value.Name);
                    constVals.Add(p.Value.Value.ToString());
                    if (p.Value.Name == "Ceq")
                    {
                        constNames.Add("Feq");
                        constVals.Add((1 - p.Value.Value).ToString());
                    }
                }

                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[] StDev = new double[1];
                double[][] fitVals = CalcFitVals(Xvals, curFit.Parameters);
                int frame = (int)curFit.Parameters["from"].Value;

                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                {
                    Fit1ValsL.Add(fitVals[0][i].ToString());

                    XValsL.Add(Xvals[i].ToString());
                    YValsL.Add(Yvals[i].ToString());

                    if (i >= frame)
                    {
                        StDev[0] += Math.Pow(Yvals[i] - fitVals[0][i], 2);
                    }
                }

                curFit.StDev = 0;
                StDev[0] = Math.Sqrt(StDev[0] / (Xvals.Length - frame));

                constNames.Add("");
                constVals.Add("");
                constNames.Add("Root mean square deviation:");
                constVals.Add(StDev[0].ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Coefficient of correlation:");
                constVals.Add(ComputeCorelationCoeff(Yvals, fitVals[0], frame).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("R-squared:");
                constVals.Add(Math.Pow(ComputeCorelationCoeff(Yvals, fitVals[0], frame), 2).ToString());
                constNames.Add("");
                constVals.Add("");
                constNames.Add("Binding+Diffusion eq.:");
                constVals.Add("f(t)= (I)invlap(FRAP(p))");
                constNames.Add("FRAP(p)");
                constVals.Add("FRAP(p) = 1/p-(Feq/p)*(1-2K1(qw)I1(qw))*(1+(Kon/(p+Koff)))-Ceq/(p+Koff)");

                #region Fit Statistics
                fitVals = null;

                #endregion Fit Statistics
                List<List<string>> FitRes = new List<List<string>>();

                FitRes.Add(constNames);
                FitRes.Add(constVals);
                FitRes.Add(XValsL);
                FitRes.Add(YValsL);
                FitRes.Add(Fit1ValsL);

                return FitRes;
            }
            class Laplace
            {
                //int DefaultStehfest = 14;

                double[] V; // Stehfest coefficients 
                double ln2; // log of 2

                public void InitStehfest(int N)
                {
                    ln2 = Math.Log(2.0);
                    int N2 = N / 2;
                    int NV = 2 * N2;
                    V = new double[NV];
                    int sign = 1;
                    if ((N2 % 2) != 0)
                        sign = -1;
                    for (int i = 0; i < NV; i++)
                    {
                        int kmin = (i + 2) / 2;
                        int kmax = i + 1;
                        if (kmax > N2)
                            kmax = N2;
                        V[i] = 0;
                        sign = -sign;
                        for (int k = kmin; k <= kmax; k++)
                        {
                            V[i] = V[i] + (Math.Pow(k, N2) / Factorial(k)) * (Factorial(2 * k)
                                 / Factorial(2 * k - i - 1)) / Factorial(N2 - k)
                                 / Factorial(k - 1) / Factorial(i + 1 - k);
                        }
                        V[i] = sign * V[i];
                    }

                }

                public double InverseTransform(double R, double w, double Feq,
                    double Ceq, double Kon, double Koff, double Df, double t)
                {
                    double ln2t = ln2 / t;
                    double p = 0;
                    double y = 0;
                    double q = 0;

                    for (int i = 0; i < V.Length; i++)
                    {
                        p += ln2t;
                        q = Math.Sqrt((p / Df) * (1 + Kon / (p + Koff)));

                        y += V[i] * (
                            (1 / p) - (Feq / p) * (1 - 2 * MathNet.Numerics.SpecialFunctions.BesselK1(q * w) * MathNet.Numerics.SpecialFunctions.BesselI1(q * w)) *
                            (1 + Kon / (p + Koff)) - Ceq / (p + Koff)
                            );
                    }
                    return ln2t * y;
                }

                public double Factorial(int N)
                {
                    double x = 1;
                    if (N > 1)
                    {
                        for (int i = 2; i <= N; i++)
                            x = i * x;
                    }
                    return x;
                }
            }
        }
        #endregion Binding&Diffusion
        public static double ComputeCorelationCoeff(double[] values1, double[] values2, int frame = 0)
        {
            //source code from https://stackoverflow.com/questions/17447817/correlation-of-two-arrays-in-c-sharp

            if (values1.Length != values2.Length)
                throw new ArgumentException("values must be the same length");

            if (frame != 0)
            {
                double[] values1a = new double[values1.Length - frame];
                Array.Copy(values1, frame, values1a, 0, values1a.Length);
                values1 = values1a;
                values1a = null;

                double[] values2a = new double[values2.Length - frame];
                Array.Copy(values2, frame, values2a, 0, values2a.Length);
                values2 = values2a;
                values2a = null;
            }
            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }

        public static double ComputeR2Coeff(double[] values1, double[] values2, int frame = 0)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("values must be the same length");

            if (frame != 0)
            {
                double[] values1a = new double[values1.Length - frame];
                Array.Copy(values1, frame, values1a, 0, values1a.Length);
                values1 = values1a;
                values1a = null;

                double[] values2a = new double[values2.Length - frame];
                Array.Copy(values2, frame, values2a, 0, values2a.Length);
                values2 = values2a;
                values2a = null;
            }

            double yMean = values1.Average();
            double SStot = 0;
            double SSres = 0;

            for (int i = 0; i < values1.Length; i++)
            {
                SStot += Math.Pow(values1[i] - yMean, 2);
                SSres += Math.Pow(values1[i] - values2[i], 2);
            }


            return 1 - SSres / SStot;
        }

        class SolveDeiffusionOval
        {
            private List<double> newXvals;
            private List<double> newYvals;
            private double Wsq;

            public Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, Dictionary<string, MySolver.Parameter> coefficients)
            {
                //const
                int frame = (int)coefficients["from"].Value;

                this.newXvals = new List<double>();
                this.newYvals = new List<double>();

                for (int i = frame + 1; i < Xvals.Length & i < Yvals.Length; i++)
                {
                    this.newXvals.Add(Xvals[i] - Xvals[frame]);
                    this.newYvals.Add(Yvals[i]);
                }

                // create solver model
                Microsoft.SolverFoundation.Solvers.NelderMeadSolver solver =
                    new Microsoft.SolverFoundation.Solvers.NelderMeadSolver();
                Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams param =
                        new Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams();

                //variables
                MySolver.Parameter I = coefficients["I"];
                MySolver.Parameter D = coefficients["D"];
                this.Wsq = Math.Pow(coefficients["w"].Value, 2) / 2;

                // Objective function.
                int objId;
                solver.AddRow("obj", out objId);
                solver.AddGoal(objId, 0, true);

                // Define variables.
                int[] variab = new int[2];

                solver.AddVariable(I.Name, out variab[0]);
                if (I.Variable)
                {
                    solver.SetLowerBound(variab[0], I.Min);
                    solver.SetUpperBound(variab[0], I.Max);
                }
                else
                {
                    solver.SetLowerBound(variab[0], I.Value);
                    solver.SetUpperBound(variab[0], I.Value);
                }
                solver.SetValue(variab[0], I.Value);

                solver.AddVariable(D.Name, out variab[1]);
                if (D.Variable)
                {
                    solver.SetLowerBound(variab[1], D.Min);
                    solver.SetUpperBound(variab[1], D.Max);
                }
                else
                {
                    solver.SetLowerBound(variab[1], D.Value);
                    solver.SetUpperBound(variab[1], D.Value);
                }

                if (D.Value != 0)
                    solver.SetValue(variab[1], D.Value);
                else
                    solver.SetValue(variab[1], 1);

                // Assign objective function delegate.
                solver.FunctionEvaluator = FunctionValue;

                // Solve.
                param.IterationLimit = 1000;

                var solution = solver.Solve(param);

                //prepare results
                Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

                MySolver.Parameter p;

                if (!coefficients.TryGetValue("w", out p))
                    p = new MySolver.Parameter("w");

                parameters.Add("w", new MySolver.Parameter
               ("w", p.Value, p.Min, p.Max, p.Variable));


                if (!coefficients.TryGetValue("I", out p))
                    p = new MySolver.Parameter("I");

                parameters.Add("I", new MySolver.Parameter
               ("I", solution.GetValue(variab[0]), p.Min, p.Max, p.Variable));

                if (!coefficients.TryGetValue("D", out p))
                    p = new MySolver.Parameter("D");

                parameters.Add("D", new MySolver.Parameter
               ("D", solution.GetValue(variab[1]), p.Min, p.Max, p.Variable));

                return parameters;

            }
            private double FunctionValue(INonlinearModel model, int rowVid,
                ValuesByIndex values, bool newValues)
            {
                double I = values[model.GetIndexFromKey("I")];
                double D = values[model.GetIndexFromKey("D")];


                double Sum = 0;
                double K = this.Wsq / D;

                for (int i = 0; i < newXvals.Count; i++)
                {
                    double K1 = K / newXvals[i];
                    Sum += Math.Pow((newYvals[i] - (
                         I * Math.Exp(-K1) * (MathNet.Numerics.SpecialFunctions.BesselI0(K1) + MathNet.Numerics.SpecialFunctions.BesselI1(K1))//diffusion eq.
                        )), 2);
                }

                return Math.Sqrt(Sum / (newXvals.Count - 1));
            }
        }
    }
}
