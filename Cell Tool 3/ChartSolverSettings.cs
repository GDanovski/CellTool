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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using System.Windows.Forms;
using System.ComponentModel;

namespace Cell_Tool_3
{
    class ChartSolverSettings
    {
        private ResultsExtractor.MyForm form1;

        public FitChart fitChart1;
        public ParamPanel parametersPanel;
        public FitData fitData;

        private SolverFunctions SolverFunctions1;

        public ChartSolverSettings(ResultsExtractor.MyForm form1)
        {
            this.form1 = form1;
            SolverFunctions1 = new SolverFunctions(form1);
            fitChart1 = new FitChart();
            parametersPanel = new ParamPanel(SolverFunctions1, form1);
            fitData = new FitData(form1, parametersPanel);
        }
        public void Reload(MySolver.FitSettings curFit = null)
        {
            fitChart1.LoadData(curFit);
        }
        public class FitData : TreeView
        {
            ResultsExtractor.MyForm form1;

            public ContextMenu ContextMenu = new ContextMenu();

            private MenuItem ApplyBtn = new MenuItem();
            private MenuItem RenameBtn = new MenuItem();
            private MenuItem DeleteBtn = new MenuItem();

            public FitData(ResultsExtractor.MyForm form1, ParamPanel parametersPanel)
            {
                this.form1 = form1;
                this.Dock = DockStyle.Left;
                this.Width = 150;
                this.ShowNodeToolTips = false;
                this.BorderStyle = BorderStyle.None;
                this.BackColor = ResultsExtractor.Parametars.BackGround2Color;
                this.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                this.CheckBoxes = true;
                this.ShowRootLines = false;
                this.ShowPlusMinus = false;
                
                this.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(
                    delegate (object o, TreeNodeMouseClickEventArgs a) 
                    {
                        parametersPanel.LoadFitFromHistory(((FitTreeNode)a.Node).GetFitSettings());
                        parametersPanel.LoadFitFromHistory(((FitTreeNode)a.Node).GetFitSettings());
                    });

                parametersPanel.StoreBtn.Click += StoreBtn_click;

                BuildContextMenu();
                this.KeyDown += TV_KeyDown;
                this.NodeMouseClick += ContextMenu_NodeShow;
            }
            private void StoreBtn_click(object sender, EventArgs e)
            {
                FitTreeNode n = new FitTreeNode(form1.solverClass.parametersPanel.current);
                n.Text = "new";
                n.Checked = true;
                this.Nodes.Add(n);
                this.SelectedNode = n;
                EditBtn_Click(sender, e);
            }
            private void SaveBtn_click(object sender, EventArgs e)
            {
                if (form1.solverClass.fitData.SelectedNode != null)
                {
                    ((FitTreeNode)form1.solverClass.fitData.SelectedNode)
                        .SetFitSettings(form1.solverClass.parametersPanel.current);
                }
            }
            public void AddNode(MySolver.FitSettings f, string name)
            {
                FitTreeNode n = new FitTreeNode(f);
                n.Text = name;
                n.Checked = true;
                this.Nodes.Add(n);
            }
            #region Context Menu
            private void TV_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.Modifiers != Keys.Control) return;
                switch (e.KeyCode)
                {
                    case (Keys.A):
                        EditBtn_Click(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case (Keys.E):
                        EditBtn_Click(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case (Keys.D):
                        DeleteNode(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                }
            }
            private void DeleteNode(object sender, EventArgs e)
            {
                if (this.SelectedNode == null) return;
                this.Nodes.Remove(this.SelectedNode);
            }

            private void EditBtn_Click(object sender, EventArgs e)
            {
                if (this.SelectedNode == null) return;

                TextBox tb = new TextBox();
                tb.Tag = this.SelectedNode;
                tb.Text = this.SelectedNode.Text;
                tb.Location = new System.Drawing.Point(this.SelectedNode.Bounds.X, this.SelectedNode.Bounds.Y - 2);
                tb.Width = this.SelectedNode.Bounds.Width;
                tb.Height = this.SelectedNode.Bounds.Height;
                tb.Visible = true;
                this.Controls.Add(tb);
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
                    if (tb.Text != "")
                    {
                        this.SelectedNode.Text = tb.Text;
                    }
                    tb.Dispose();
                });
                tb.KeyDown += new KeyEventHandler(delegate (Object o, KeyEventArgs a)
                {
                    if (a.KeyCode == Keys.Enter && tb.Text != "")
                    {
                        a.Handled = true;
                        a.SuppressKeyPress = true;
                        this.SelectedNode.Text = tb.Text;
                        tb.Dispose();
                    }

                });
                this.AfterSelect += new TreeViewEventHandler(delegate (Object o, TreeViewEventArgs a)
                {
                    tb.Dispose();
                });
                this.MouseWheel += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
                {
                    tb.Dispose();
                });
            }
                 
            private void ApplyBtn_Click(object sender, EventArgs e)
            {
                form1.solverClass.parametersPanel.LoadFitFromHistory(((FitTreeNode)this.SelectedNode).GetFitSettings());
            }
            public MySolver.FitSettings GetFitSetingsFromNode(TreeNode n)
            {
                return ((FitTreeNode)n).GetFitSettings();
            }
            private void ContextMenu_NodeShow(object sender, TreeNodeMouseClickEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;
                this.SelectedNode = e.Node;
                RenameBtn.Enabled = true;
                DeleteBtn.Enabled = true;
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
            
            private void BuildContextMenu()
            {
                ApplyBtn.Text = "Review fit";
                ApplyBtn.Click += ApplyBtn_Click;
                ApplyBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(ApplyBtn);

                MenuItem SaveBtn = new MenuItem();
                SaveBtn.Text = "Save fit settings";
                SaveBtn.Click += SaveBtn_click;
                SaveBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(SaveBtn);                

                RenameBtn.Text = "Rename";
                RenameBtn.Click += EditBtn_Click;
                RenameBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(RenameBtn);


                DeleteBtn.Text = "Delete";
                DeleteBtn.Click += DeleteNode;
                DeleteBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(DeleteBtn);

                ContextMenu.MenuItems.Add("-");

                MenuItem CheckBtn = new MenuItem();
                CheckBtn.Text = "Check all";
                CheckBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                    foreach (TreeNode n in this.Nodes)
                        n.Checked = true;
                });
                ContextMenu.MenuItems.Add(CheckBtn);

                MenuItem unCheckBtn = new MenuItem();
                unCheckBtn.Text = "Uncheck all";
                unCheckBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                    foreach (TreeNode n in this.Nodes)
                        n.Checked = false;
                });
                ContextMenu.MenuItems.Add(unCheckBtn);

                MenuItem delCheckBtn = new MenuItem();
                delCheckBtn.Text = "Delete checked";
                delCheckBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                    for(int i = this.Nodes.Count-1;i>=0;i--)
                    if(this.Nodes[i].Checked)
                    {
                            this.Nodes.RemoveAt(i);
                    }

                });
                ContextMenu.MenuItems.Add(delCheckBtn);
            }
            #endregion Context menu
            public string DataToString()
            {
                List<string> str = new List<string>();
                foreach(var n in this.Nodes)
                {
                    try
                    {
                        str.Add(((FitTreeNode)n).ExportString());
                    }
                    catch { }
                }
                return string.Join("|",str);
            }
            public void StringToData(string str)
            {
                string[] strL = str.Split(new string[] { "|"},StringSplitOptions.None);
                foreach(string s in strL)
                {
                    FitTreeNode n = new FitTreeNode(s);
                    this.Nodes.Add(n);
                }
            }
           public class FitTreeNode : TreeNode
            {
                MySolver.FitSettings Data;
                public FitTreeNode(string str)
                {
                    this.Nodes.Clear();
                    LoadFromString(str);
                }
                public FitTreeNode(MySolver.FitSettings Data)
                {
                    MySolver.FitSettings NewData = new MySolver.FitSettings();

                    NewData.SetFormulaIF = Data.GetFormulaIF;
                    NewData.SetFormula1 = Data.GetFormula1;
                    NewData.SetFormula2 = Data.GetFormula2;

                    NewData.Parameters = new Dictionary<string, MySolver.Parameter>();
                    foreach (var par in Data.Parameters)
                    {
                        MySolver.Parameter p = new MySolver.Parameter(par.Key);
                        p.Name = par.Value.Name;
                        p.Value = par.Value.Value;
                        p.Min = par.Value.Min;
                        p.Max = par.Value.Max;
                        p.Variable = par.Value.Variable;

                        NewData.Parameters.Add(par.Key, p);
                    }

                    NewData.XVals = new double[Data.XVals.Length];
                    NewData.YVals = new double[Data.YVals.Length];
                    Array.Copy(Data.XVals, NewData.XVals, Data.XVals.Length);
                    Array.Copy(Data.YVals, NewData.YVals, Data.YVals.Length);
                    
                    this.Data = NewData;                    
                }
                public void SetFitSettings(MySolver.FitSettings Data)
                {
                    MySolver.FitSettings NewData = new MySolver.FitSettings();

                    NewData.SetFormulaIF = Data.GetFormulaIF;
                    NewData.SetFormula1 = Data.GetFormula1;
                    NewData.SetFormula2 = Data.GetFormula2;

                    NewData.Parameters = new Dictionary<string, MySolver.Parameter>();
                    foreach (var par in Data.Parameters)
                    {
                        MySolver.Parameter p = new MySolver.Parameter(par.Key);
                        p.Name = par.Value.Name;
                        p.Value = par.Value.Value;
                        p.Min = par.Value.Min;
                        p.Max = par.Value.Max;
                        p.Variable = par.Value.Variable;

                        NewData.Parameters.Add(par.Key, p);
                    }

                    NewData.XVals = new double[Data.XVals.Length];
                    NewData.YVals = new double[Data.YVals.Length];
                    Array.Copy(Data.XVals, NewData.XVals, Data.XVals.Length);
                    Array.Copy(Data.YVals, NewData.YVals, Data.YVals.Length);

                    this.Data = NewData;
                }
                public MySolver.FitSettings GetFitSettings()
                {
                    MySolver.FitSettings NewData = new MySolver.FitSettings();
                    
                    NewData.SetFormulaIF = Data.GetFormulaIF;
                    NewData.SetFormula1 = Data.GetFormula1;
                    NewData.SetFormula2 = Data.GetFormula2;

                    NewData.Parameters = new Dictionary<string, MySolver.Parameter>();
                    foreach(var par in Data.Parameters)
                    {
                        MySolver.Parameter p = new MySolver.Parameter(par.Key);
                        p.Name = par.Value.Name;
                        p.Value = par.Value.Value;
                        p.Min = par.Value.Min;
                        p.Max = par.Value.Max;
                        p.Variable = par.Value.Variable;

                        NewData.Parameters.Add(par.Key, p);
                    }

                    NewData.XVals = new double[Data.XVals.Length];
                    NewData.YVals = new double[Data.YVals.Length];
                    Array.Copy(Data.XVals, NewData.XVals, Data.XVals.Length);
                    Array.Copy(Data.YVals, NewData.YVals, Data.YVals.Length);

                    return NewData;
                }
                
                public string ExportString()
                {

                    string str = this.Text + "\t"
                        + this.Checked.ToString() + "\t"
                  + Data.GetFormulaIF.Replace("=", "!") + "\t" 
                   + Data.GetFormula1 + "\t" 
                    + Data.GetFormula2 + "\t"
                    + string.Join(":", Data.XVals) + "\t"
                    +string.Join(":", Data.YVals);

                    foreach (var par in Data.Parameters)
                    {
                        str += "\t"+ par.Value.Name + ":"
                        + par.Value.Value + ":"
                        + par.Value.Min + ":"
                        + par.Value.Max + ":"
                        + par.Value.Variable;
                    }
                    return str;
                }
                public void LoadFromString(string str)
                {
                    if (str == "") return;
                    string[] vals = str.Split(new string[] { "\t" }, StringSplitOptions.None);
                    this.Text = vals[0];
                    this.Checked = bool.Parse(vals[1]);
                    Data = new MySolver.FitSettings();
                    Data.SetFormulaIF = vals[2].Replace("!","=");
                    Data.SetFormula1 = vals[3];
                     Data.SetFormula2 = vals[4];
                    string[] val = vals[5].Split(new string[] { ":" }, StringSplitOptions.None);

                    Data.XVals = new double[val.Length];
                    for (int i = 0; i < val.Length; i++)
                        Data.XVals[i] = double.Parse(val[i]);

                    val = vals[6].Split(new string[] { ":" }, StringSplitOptions.None);
                    Data.YVals = new double[val.Length];
                    for (int i = 0; i < val.Length; i++)
                        Data.YVals[i] = double.Parse(val[i]);

                    for(int i = 7; i < vals.Length; i++)
                    {
                        val = vals[i].Split(new string[] { ":" }, StringSplitOptions.None);

                        MySolver.Parameter p = new MySolver.Parameter(val[0]);
                        p.Name = val[0];

                        try
                        {
                            p.Value = double.Parse(val[1]);
                        }
                        catch
                        {
                        }

                        if (val[2] == double.MinValue.ToString())
                            p.Min = double.MinValue;
                        else
                            p.Min = double.Parse(val[2]);

                        if (val[3] == double.MaxValue.ToString())
                            p.Max = double.MaxValue;
                        else
                            p.Max = double.Parse(val[3]);

                        p.Variable = bool.Parse(val[4]);

                        Data.Parameters.Add(p.Name, p);
                    }
                    

                }
            }
        }
        public class ParamPanel : Panel
        {
            private ResultsExtractor.MyForm form1;
            public SolverFunctions SolverFunctions1;
            public ComboBox cmbBox = new ComboBox();

            private List<ParameterBox> parBoxList = new List<ParameterBox>();
            public MySolver.FitSettings current;

            private GroupBox parGB = new GroupBox();
            public Button StoreBtn;

            public ParamPanel(SolverFunctions SolverFunctions1, ResultsExtractor.MyForm form1)
            {
                this.form1 = form1;
                this.SolverFunctions1 = SolverFunctions1;
                this.Dock = DockStyle.Top;
                this.Height = 60;

                Panel UpperPanel = new Panel();
                UpperPanel.Height = 60;
                UpperPanel.Dock = DockStyle.Top;
                this.Controls.Add(UpperPanel);

                Label protLabel = new Label();
                protLabel.Text = "Name:";
                protLabel.Width = 60;
                protLabel.Location = new Point(5, 8);
                UpperPanel.Controls.Add(protLabel);

                cmbBox.Location = new Point(65, 5);
                cmbBox.Width = 100;
                cmbBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                UpperPanel.Controls.Add(cmbBox);
                cmbBox.BringToFront();
                cmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbBox.Items.Clear();
                cmbBox.AutoSize = false;
                //fill the combobox
                cmbBox.SelectedIndexChanged += cmbBox_ChangeIndex;
                SolverFunctions1.dialog.funcCB_refresh(cmbBox);
                if (cmbBox.Items.Count > 0)
                    cmbBox.SelectedIndex = 0;
                 
                Button btn = new Button();
                {
                    btn.Width = 21;
                    btn.Height = 21;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = ResultsExtractor.Parametars.BackGround2Color;
                    btn.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                    btn.Image = new Bitmap(Properties.Resources.settings, new Size(18, 18));
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Text = "";
                    btn.Tag = "Edit";
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(165, 5);
                    btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    btn.Click += Edit_Click;
                }

                btn = new Button();
                {
                    btn.Width = 90;
                    btn.Height = 21;
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Solve";
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(5, 35);
                    btn.Click += Solve_Click;
                }


                btn = new Button();
                {
                    btn.Width = 90;
                    btn.Height = 21;
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Solve Repeats";
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(100, 35);
                    btn.Click += MultiSolve_Click;
                }

                btn = new Button();
                {
                    btn.Width = 90;
                    btn.Height = 21;
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Store";
                    StoreBtn = btn;
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(195, 35);
                    //btn.Click += Show_Click;
                }

                btn = new Button();
                {
                    btn.Width = 90;
                    btn.Height = 21;
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Refresh";
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(290, 35);
                    btn.Click += RefreshBtn_click;
                }

                btn = CancelBtn;
                {
                    btn.Width = 90;
                    btn.Height = 21;
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Cancel";
                    UpperPanel.Controls.Add(btn);
                    btn.BringToFront();
                    btn.Location = new Point(385, 35);
                    btn.Click += new EventHandler(delegate (object o, EventArgs a)
                    {
                        if (CancelBtn.Tag == null) return;
                        ((BackgroundWorker)btn.Tag).CancelAsync();
                        CancelBtn.Tag = null;
                        CancelBtn.Visible = false;
                        form1.StatusLabel.Text = "Ready";
                    });
                    btn.Visible = false;

                }

                parGB.Text = "Variables:";
                parGB.Dock = DockStyle.Fill;
                parGB.Visible = true;
                parGB.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                this.Controls.Add(parGB);
                parGB.BringToFront();
                parGB.Controls.Add(new ParameterTitles());
            }
            Button CancelBtn = new Button();
            public void Show_Click(object o, EventArgs e)
            {
                foreach (ParameterBox pBox in parBoxList)
                    if (pBox.Visible == true)
                    {
                        pBox.RefreshValues();
                    }

                try
                {
                    if(current.XVals == null)
                        CopyArray(form1.resultsCh.Navg);

                    form1.solverClass.fitChart1.LoadData(current);
                }
                catch
                {

                }
            }
            public void ForcedRefresh()
            {
                if (current != null)
                    try
                    {
                        foreach (ParameterBox pBox in parBoxList)
                            if (pBox.Visible == true)
                            {
                                pBox.RefreshValues();
                            }

                        //CopyArray(form1.resultsCh.Navg);
                        form1.solverClass.fitChart1.LoadData(current);
                    }
                    catch { }
            }
            private void RefreshBtn_click(object sender, EventArgs e)
            {
                if (current != null)
                    try
                    {
                        foreach (ParameterBox pBox in parBoxList)
                            if (pBox.Visible == true)
                            {
                                pBox.RefreshValues();
                            }

                        CopyArray(form1.resultsCh.Navg);
                        form1.solverClass.fitChart1.LoadData(current);
                    }
                    catch { }
            }
            private void MultiSolve_Click(object o, EventArgs e)
            {
                if (current == null) return;
                if (CancelBtn.Tag != null) return;

                foreach (ParameterBox pBox in parBoxList)
                    if (pBox.Visible == true)
                    {
                        pBox.RefreshValues();
                    }

                try
                {
                    form1.solverClass.fitChart1.LoadData(current);
                }
                catch
                {
                    MessageBox.Show("Incorrect initial values of the variables!");
                    return;
                }

                #region Prepare data
                Dictionary<string, MySolver.FitSettings> temp = new Dictionary<string, MySolver.FitSettings>();

                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode nSource in parent.Nodes)
                        if (nSource.Checked)
                        {
                            ResultsExtractor.DataNode n = (ResultsExtractor.DataNode)nSource.Tag;
                            //find smallest array size
                            int arraySize = form1.dataTV.Xaxis.Length;
                            if (n.Series.Length < arraySize) arraySize = n.Series.Length;

                            double[] Xvals = new double[arraySize];
                            double[] Yvals = new double[arraySize];

                            Array.Copy(form1.dataTV.Xaxis, Xvals, arraySize);

                            if (form1.NormCB.Checked)
                                Array.Copy(n.NormSeries, Yvals, arraySize);
                            else
                                Array.Copy(n.Series, Yvals, arraySize);

                            #region New fit
                            MySolver.FitSettings cur = new MySolver.FitSettings();

                            cur.SetFormulaIF = current.GetFormulaIF;
                            cur.SetFormula1 = current.GetFormula1;
                            cur.SetFormula2 = current.GetFormula2;
                            cur.Parameters = new Dictionary<string, MySolver.Parameter>();

                            foreach (var pair in current.Parameters)
                            {
                                MySolver.Parameter pOld = pair.Value;
                                MySolver.Parameter pNew = new MySolver.Parameter(pair.Key);
                                pNew.Value = pOld.Value;
                                pNew.Min = pOld.Min;
                                pNew.Max = pOld.Max;
                                pNew.Variable = pOld.Variable;
                                pNew.Name = pOld.Name;

                                cur.Parameters.Add(pair.Key, pNew);
                            }

                            cur.XVals = Xvals;
                            cur.YVals = Yvals;


                            temp.Add(n.Text.Replace("\t", " "), cur);
                            #endregion New Fit
                        }
                #endregion prepare data
                
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                bgw.WorkerSupportsCancellation = true;
                CancelBtn.Tag = bgw;
                CancelBtn.Visible = true;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o1, DoWorkEventArgs a)
                {
                /*
                int i = 1;
                foreach (var t in temp)
                {
                    t.Value.SolverFit();
                    ((BackgroundWorker)o1).ReportProgress(i++);
                }*/
                Parallel.ForEach(temp, (t)=>
                    {
                        t.Value.SolverFit();
                    });
                    
                    ((BackgroundWorker)o1).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(
                    delegate (Object o1, ProgressChangedEventArgs a)
                    {
                        if (a.ProgressPercentage == 0)
                        {
                            foreach(var t in temp)
                            {
                                form1.solverClass.fitData.AddNode(t.Value, t.Key);
                            }

                            CancelBtn.Tag = null;
                            CancelBtn.Visible = false;
                            form1.StatusLabel.Text = "Ready";
                        }
                        else
                        {
                            form1.StatusLabel.Text = "Solving("+ a.ProgressPercentage.ToString() + ")...";
                        }                        
                    });
                //Start background worker
                form1.StatusLabel.Text = "Parallel Solving...";
                //start bgw

                bgw.RunWorkerAsync();
            }
            private void Solve_Click(object o, EventArgs e)
            {
                if (CancelBtn.Tag != null) return;

                foreach (ParameterBox pBox in parBoxList)
                    if (pBox.Visible == true)
                    {
                        pBox.RefreshValues();
                    }

                try
                {
                    form1.solverClass.fitChart1.LoadData(current);
                }
                catch
                {
                    MessageBox.Show("Incorrect initial values of the variables!");
                    return;
                }

                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                bgw.WorkerSupportsCancellation = true;
                CancelBtn.Tag = bgw;
                CancelBtn.Visible = true;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o1, DoWorkEventArgs a)
                {
                    if(current.XVals==null || current.YVals == null)
                        CopyArray(form1.resultsCh.Navg);
                    try
                    {
                        current.SolverFit();
                        ((BackgroundWorker)o1).ReportProgress(0);
                    }
                    catch
                    {
                        ((BackgroundWorker)o1).ReportProgress(1);
                    }
                });
                
                bgw.ProgressChanged += new ProgressChangedEventHandler(
                    delegate (Object o1, ProgressChangedEventArgs a)
                    {
                        if (a.ProgressPercentage == 0)
                        {
                            form1.solverClass.fitChart1.LoadData(current);

                            for (int i = 0; i < current.Parameters.Count; i++)
                            {
                                parBoxList[i].LoadParameter(current.Parameters.ElementAt(i).Value);
                            }
                        }
                        else if (a.ProgressPercentage == 1)
                        {
                            MessageBox.Show("Error - Solver!!!");                            
                        }
                        CancelBtn.Tag = null;
                        CancelBtn.Visible = false;
                        form1.StatusLabel.Text = "Ready";
                    });
                //Start background worker
                form1.StatusLabel.Text = "Solving...";
                //start bgw

                bgw.RunWorkerAsync();

            }
            private void Edit_Click(object sender, EventArgs e)
            {

                form1.StatusLabel.Text = "Dialog open";
                SolverFunctions1.dialog.ShowDialog();
                form1.StatusLabel.Text = "Ready";
            }
            public void cmbBox_ChangeIndex(object sender, EventArgs e)
            {
                SolverFunctions.FunctionValue f = null;
                try
                {
                    if (cmbBox.SelectedIndex < FRAPA_Model.nModels)//Frap analysis
                    {
                        f = FRAPA_Model.AllModels.GetFrapaFunction(cmbBox.SelectedIndex);
                    }
                    else//other functions (custom functions)
                    {
                        f = (SolverFunctions.FunctionValue)SolverFunctions1.dialog.tw.Nodes[cmbBox.SelectedIndex - FRAPA_Model.nModels].Tag;
                    }
                }
                catch
                {
                    this.Height = 60;
                    return;
                }

                if (f == null)
                {
                    this.Height = 60;
                    return;
                }
                this.SuspendLayout();

                if(current == null)
                    current = new MySolver.FitSettings();

                current.SetFormulaIF = f.GetFormulaIF;
                current.SetFormula1 = f.GetFormula1;
                current.SetFormula2 = f.GetFormula2;
                current.Parameters = f.GetParametersDictionary();

                if (parBoxList.Count < current.Parameters.Count)
                {
                    //Extend if shorter
                    for (int i = parBoxList.Count; i < current.Parameters.Count; i++)
                    {
                        ParameterBox p = new ParameterBox();
                        parBoxList.Add(p);
                        parGB.Controls.Add(p);
                        p.BringToFront();
                        p._Value.LostFocus += Show_Click;
                        p._Value.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
                        {
                            if (a.KeyCode == Keys.Enter)
                            {
                                Show_Click(o, a);
                                a.Handled = true;
                                a.SuppressKeyPress = true;                                
                            }
                        });
                    }
                }
                else if (parBoxList.Count > current.Parameters.Count)
                {
                    //Disable unnesessery;
                    for (int i = current.Parameters.Count; i < parBoxList.Count; i++)
                    {
                        parBoxList[i].Visible = false;
                    }
                }
                //enable all that we need
                for (int i = 0; i < current.Parameters.Count; i++)
                {
                    if (cmbBox.SelectedIndex < FRAPA_Model.nModels)
                        FRAPA_Model.AllModels.SetConstValues(current.Parameters.ElementAt(i).Value);

                    parBoxList[i].LoadParameter(current.Parameters.ElementAt(i).Value);

                    if (cmbBox.SelectedIndex < FRAPA_Model.nModels)
                        FRAPA_Model.AllModels.CheckConstValues(cmbBox.SelectedIndex, parBoxList[i]);
                    else
                        parBoxList[i].IsConstant(false);

                    parBoxList[i].Visible = true;
                }

                int h = 60;

                if (current.Parameters.Count > 0)
                {
                    h += 45 + current.Parameters.Count * 30;
                }

                if (form1.solverClass != null)
                    form1.solverClass.fitChart1.LoadData(null);

                this.Height = h;

                this.ResumeLayout();

                ForcedRefresh();
            }
            public void LoadFitFromHistory(MySolver.FitSettings fit)
            {
                this.current = fit;
                CheckForFormulas(fit);

                if (parBoxList.Count < current.Parameters.Count)
                {
                    //Extend if shorter
                    for (int i = parBoxList.Count; i < current.Parameters.Count; i++)
                    {
                        ParameterBox p = new ParameterBox();
                        parBoxList.Add(p);
                        parGB.Controls.Add(p);
                        p.BringToFront();
                        p._Value.LostFocus += Show_Click;
                        p._Value.KeyDown += new KeyEventHandler(delegate (object o, KeyEventArgs a)
                        {
                            if (a.KeyCode == Keys.Enter)
                            {
                                Show_Click(o, a);
                                a.Handled = true;
                                a.SuppressKeyPress = true;
                            }
                        });
                    }
                }
                else if (parBoxList.Count > current.Parameters.Count)
                {
                    //Disable unnesessery;
                    for (int i = current.Parameters.Count; i < parBoxList.Count; i++)
                    {
                        parBoxList[i].Visible = false;
                    }
                }
                //enable all that we need
                for (int i = 0; i < current.Parameters.Count; i++)
                {
                    parBoxList[i].LoadParameter(current.Parameters.ElementAt(i).Value);

                    if (cmbBox.SelectedIndex <FRAPA_Model.nModels)
                        FRAPA_Model.AllModels.CheckConstValues(cmbBox.SelectedIndex, parBoxList[i]);
                    else
                        parBoxList[i].IsConstant(false);

                    parBoxList[i].Visible = true;
                }

                int h = 60;

                if (current.Parameters.Count > 0)
                {
                    h += 45 + current.Parameters.Count * 30;

                }

                if (form1.solverClass != null)
                    form1.solverClass.fitChart1.LoadData(current);

                this.Height = h;
            }
            public SolverFunctions.FunctionValue GetFunctionValueFormulas(MySolver.FitSettings fit)
            {
                if (FRAPA_Model.AllModels.GetModelIndex(fit.GetFormulaIF) != -1)
                {
                    cmbBox.SelectedIndex = FRAPA_Model.AllModels.GetModelIndex(fit.GetFormulaIF);
                    return null;
                }

                foreach (TreeNode n in SolverFunctions1.dialog.tw.Nodes)
                {
                    SolverFunctions.FunctionValue f1 = (SolverFunctions.FunctionValue)n.Tag;
                    if (f1.GetFormula1 == fit.GetFormula1 &&
                        f1.GetFormula2 == fit.GetFormula2 &&
                        f1.GetFormulaIF == fit.GetFormulaIF)
                    {
                        return f1;
                    }
                }

                return null;
            }
            public void CheckForFormulas(MySolver.FitSettings fit, string name = "LoadedFit")
            {
                if (FRAPA_Model.AllModels.GetModelIndex(fit.GetFormulaIF) != -1)
                {
                    cmbBox.SelectedIndex = FRAPA_Model.AllModels.GetModelIndex(fit.GetFormulaIF);
                    return;
                }

                foreach (TreeNode n in SolverFunctions1.dialog.tw.Nodes)
                {
                    SolverFunctions.FunctionValue f1 = (SolverFunctions.FunctionValue)n.Tag;
                    if (f1.GetFormula1 == fit.GetFormula1 &&
                        f1.GetFormula2 == fit.GetFormula2 &&
                        f1.GetFormulaIF == fit.GetFormulaIF)
                    {
                        cmbBox.SelectedIndex = SolverFunctions1.dialog.tw.Nodes.IndexOf(n) + FRAPA_Model.nModels;
                        return;
                    }
                }

                SolverFunctions.FunctionValue f = new SolverFunctions.FunctionValue();
                f.SetName = name;
                f.SetFormulaIF = fit.GetFormulaIF;
                f.SetFormula1 = fit.GetFormula1;
                f.SetFormula2 = fit.GetFormula2;

                string[] par = new string[fit.Parameters.Count];
                for (int i = 0; i < fit.Parameters.Count; i++)
                    par[i] = fit.Parameters.ElementAt(i).Key;

                f.SetParameters = par;

                TreeNode n1 = new TreeNode(name);
                n1.Tag = f;
                SolverFunctions1.dialog.tw.Nodes.Add(n1);
                SolverFunctions1.dialog.tw.SaveFunctions();
                cmbBox.Items.Add(name);

                cmbBox.SelectedIndex = SolverFunctions1.dialog.tw.Nodes.IndexOf(n1) + FRAPA_Model.nModels;
            }
            private void CopyArray(ResultsExtractor_CTChart.Series Navg)
            {
                double[] Xvals = new double[Navg.Points.Count];
                double[] Yvals = new double[Navg.Points.Count];
                for (int i = 0; i < Navg.Points.Count; i++)
                {
                    Xvals[i] = Navg.Points[i].X;
                    Yvals[i] = Navg.Points[i].Y;
                }

                current.XVals = Xvals;
                current.YVals = Yvals;
            }
            public class ParameterBox : Panel
            {
                private MySolver.Parameter par;
                public CheckBox _Name;
                public TextBox _Value;
                private TextBox _Min;
                private TextBox _Max;
                public ParameterBox()
                {
                    this.Width = 205;
                    this.Height = 30;
                    _Name = new CheckBox();
                    _Value = new TextBox();
                    _Min = new TextBox();
                    _Max = new TextBox();

                    int w = 5;

                    _Name.Location = new Point(w, 3);
                    _Name.Width = 45;
                    w += 50;
                    this.Controls.Add(_Name);

                    _Value.Location = new Point(w, 3);
                    _Value.Width = 45;
                    this.Controls.Add(_Value);
                    //_Value.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    w += 50;

                    _Min.Location = new Point(w, 3);
                    _Min.Width = 45;
                    this.Controls.Add(_Min);
                    //_Min.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    w += 50;

                    _Max.Location = new Point(w, 3);
                    _Max.Width = 45;
                    this.Controls.Add(_Max);
                    //_Max.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    w += 50;

                    this.Dock = DockStyle.Top;
                    this.Resize += This_Resize;
                }
                private void This_Resize(object sender, EventArgs e)
                {
                    this.SuspendLayout();
                    
                    int wCh = (this.Width - 25) / 4;
                    int w = 5;
                    _Name.Location = new Point(w, 3);
                    _Name.Width = wCh;
                    w += wCh + 5;
                    _Value.Location = new Point(w, 3);
                    _Value.Width = wCh;
                    w += wCh + 5;
                    _Min.Location = new Point(w, 3);
                    _Min.Width = wCh;
                    w += wCh + 5;
                    _Max.Location = new Point(w, 3);
                    _Max.Width = wCh;

                    this.ResumeLayout();
                }

                public void LoadParameter(MySolver.Parameter par)
                {
                    par.Value = CheckParameterValue(par.Value);
                    par.Min = CheckParameterValue(par.Min);
                    par.Max = CheckParameterValue(par.Max);

                    if (this.par != par) this.par = par;

                    _Name.Text = par.Name;
                    _Name.Checked = par.Variable;
                    _Value.Text = par.Value.ToString("0." + new string('#', 339));
                    _Min.Text = par.Min.ToString();
                    _Max.Text = par.Max.ToString();
                }
                private double CheckParameterValue(double par)
                {
                    double par1 = par;
                    if(par1.ToString() == double.MaxValue.ToString())
                    {
                        par1 -= 1;
                    }
                    else if (par1.ToString() == double.MinValue.ToString())
                    {
                        par1 += 1;
                    }
                    double a = new double();

                    if (!double.TryParse(par1.ToString("0." + new string('#', 339)), out a))
                    {
                        if (par1 > 0)
                        {
                            par1 -= 1;
                        }
                        else if (par1 < 0)
                        {
                            par1 += 1;
                        }
                    }
                    

                    return par1;
                }
                public void RefreshValues()
                {
                    if (!isValide()) return;

                    par.Variable = _Name.Checked;
                    par.Value = double.Parse(_Value.Text);
                    try
                    {
                        par.Min = double.Parse(_Min.Text);
                    }
                    catch
                    {
                        par.Min = double.MinValue;
                    }

                    try
                    {
                        par.Max = double.Parse(_Max.Text);
                    }
                    catch
                    {
                        par.Max = double.MaxValue;
                    }

                    if (par.Min > par.Max)
                    {
                        double temp = par.Min;
                        par.Min = par.Max;
                        par.Max = temp;
                    }

                    if (par.Value < par.Min || par.Value > par.Max)
                    {
                        par.Value = (par.Max - par.Min) / 2;
                    }

                    LoadParameter(par);
                }
                public bool isValide()
                {
                    double temp = 0;
                    if (double.TryParse(_Value.Text, out temp) &&
                        (double.TryParse(_Min.Text, out temp) || _Min.Text == "-1.79769313486232E+308") &&
                        (double.TryParse(_Max.Text, out temp) || _Max.Text == "1.79769313486232E+308"))
                        return true;

                    MessageBox.Show("Variable " + _Name.Text + ": Incorrect value!");
                    return false;
                }
                public void IsConstant(bool isConst,bool NotAcceptChanges = false)
                {

                    if (isConst && !NotAcceptChanges)
                    {
                        _Name.AutoCheck = false;
                        _Value.ReadOnly = false;
                        _Min.Visible = false;
                        _Max.Visible = false;
                    }
                    else if (isConst && NotAcceptChanges)
                    {
                        _Name.AutoCheck = false;
                        _Value.ReadOnly = true;
                        _Min.Visible = false;
                        _Max.Visible = false;
                    }
                    else
                    {
                        _Name.AutoCheck = true;
                        _Value.ReadOnly = false;
                        _Min.Visible = true;
                        _Max.Visible = true;
                    }
                }
            }
        }
        public class ParameterTitles : Panel
        {
            private Label _Name;
            private Label _Value;
            private Label _Min;
            private Label _Max;
            public ParameterTitles()
            {
                this.Width = 205;
                this.Height = 25;
                _Name = new Label();
                _Value = new Label();
                _Min = new Label();
                _Max = new Label();

                int w = 5;
                _Name.Text = "Name:";
                _Name.Location = new Point(w, 3);
                _Name.Width = 45;
                w += 50;
                this.Controls.Add(_Name);

                _Value.Location = new Point(w, 3);
                _Value.Text = "Value:";
                _Value.Width = 45;
                this.Controls.Add(_Value);
                //_Value.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                w += 50;

                _Min.Location = new Point(w, 3);
                _Min.Text = "Min:";
                _Min.Width = 45;
                this.Controls.Add(_Min);
                //_Min.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                w += 50;

                _Max.Location = new Point(w, 3);
                _Max.Text = "Max:";
                _Max.Width = 45;
                this.Controls.Add(_Max);
                //_Max.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                w += 50;

                this.Dock = DockStyle.Top;
                this.Resize += This_Resize;
            }
            private void This_Resize(object sender, EventArgs e)
            {
                this.SuspendLayout();
                //int w = 50;
                //int wCh = (this.Width - 65) / 3;

                int wCh = (this.Width - 25) / 4;
                int w = 5;
                _Name.Location = new Point(w, 3);
                _Name.Width = wCh;
                w += wCh + 5;

                _Value.Location = new Point(w, 3);
                _Value.Width = wCh;
                w += wCh + 5;
                _Min.Location = new Point(w, 3);
                _Min.Width = wCh;
                w += wCh + 5;
                _Max.Location = new Point(w, 3);
                _Max.Width = wCh;

                this.ResumeLayout();
            }
        }   


    public class FitChart : ResultsExtractor_CTChart
        {
            public ListBox Titles;
            private ResultsExtractor_CTChart.Series Raw;
            private ResultsExtractor_CTChart.Series Fit;
            private ResultsExtractor_CTChart.Series RawNavg;
            private ResultsExtractor_CTChart.Series FitNavg;
            private List<ResultsExtractor_CTChart.Series> FitFormulas;

            string[] colMatrix = new string[] {"#00b300", "#b300b3", "#00bfff", "#ffcc00", "#ff471a", "#cc6699", "#39e600"
                , "#00b3b3", "#ffcc66", "#7575a3", "#ff1a1a", "#ff0055", "#8a00e6", "#bf8040",
                "#53c68c", "#ace600", "#b33c00", "#ff6666"};
            public FitChart()
            {
                Titles = new ListBox();
                Titles.Dock = DockStyle.Top;
                Titles.Font = new Font("Arial", 8);
                Titles.SelectionMode = SelectionMode.None;
                Titles.Height = 10;
                Titles.BackColor = Color.White;
                Titles.ForeColor = Color.Black;
                Titles.BorderStyle = BorderStyle.None;
               
                this.Dock = DockStyle.Fill;
                this.Build(null);
                //Chart
                               
                Raw = new ResultsExtractor_CTChart.Series();
                Raw.Color = Color.Blue;
                this.ChartSeries.Add(Raw);

                Fit = new ResultsExtractor_CTChart.Series();
                Fit.Color = Color.Red;
                this.ChartSeries.Add(Fit);

                RawNavg = new ResultsExtractor_CTChart.Series();
                RawNavg.Color = Color.Green;
                this.ChartSeries.Add(RawNavg);

                FitNavg = new ResultsExtractor_CTChart.Series();
                FitNavg.Color = Color.Yellow;
                this.ChartSeries.Add(FitNavg);

                FitFormulas = new List<ResultsExtractor_CTChart.Series>();
            }
            public void LoadData(MySolver.FitSettings curFit)
            {
                this.SuspendLayout();
                
                Raw.Points.Clear();
                Fit.Points.Clear();
                RawNavg.Points.Clear();
                FitNavg.Points.Clear();
                foreach (Series s in FitFormulas)
                    s.Points.Clear();

                if (curFit != null)
                {
                    if(curFit.GetFormulaIF.StartsWith("FRAP"))//send to frapa model
                    {
                        LoadFRAPAData(curFit);
                        return;
                    }

                    double[] Xvals = curFit.XVals;
                    double[] Yvals = curFit.YVals;

                    string formula = curFit.FormulaForNcalc();
                    Expression e = new Expression(formula);

                    foreach (var kvp in curFit.Parameters)
                    {
                        MySolver.Parameter p = kvp.Value;
                        e.Parameters[p.Name] = p.Value;
                        //val = val.Replace(p.Name, p.Value.ToString());
                    }
                    double StDev = 0;
                    List<double> YFitVals = new List<double>();
                    //this.Series.Remove(Raw);
                    //this.Series.Remove(Fit);

                    for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                    {
                        //raw
                        Raw.Points.AddXY(Xvals[i], Yvals[i]);
                        //fit
                        e.Parameters["t"] = Xvals[i];

                        double val = 0;
                        double.TryParse(e.Evaluate().ToString(), out val);

                        Fit.Points.AddXY(Xvals[i], val);
                        YFitVals.Add(val);

                        StDev += Math.Pow(Yvals[i] - val, 2);
                    }
                    curFit.StDev = Math.Sqrt(StDev / (Fit.Points.Count));

                    //this.Series.Add(Raw);
                    //this.Series.Add(Fit);

                    this.Titles.Items.Clear();
                    
                    if (Fit.Points.Count > 0)
                    {
                        Titles.Height = 50;
                        this.Titles.Items.AddRange(new string[]{
                            "",
                            "\tRoot mean square deviation = " + curFit.StDev ,
                             "\tR-squared = " + Math.Pow(FRAPA_Model.ComputeCorelationCoeff(Yvals, YFitVals.ToArray()), 2) });
                    }
                    else
                    {
                        Titles.Height = 10;
                    }

                    YFitVals = null;

                    SubFormulas_Load(curFit);

                    this.ResumeLayout();

                    /*
                    List<PointF> pListRaw = new List<PointF>();
                    List<PointF> pListFit = new List<PointF>();

                    for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                    {
                        //raw
                        pListRaw.Add(new PointF((float)Xvals[i], (float)Yvals[i]));
                        //fit
                        e.Parameters["t"] = Xvals[i];

                        double val = 0;
                        double.TryParse(e.Evaluate().ToString(), out val);

                        pListRaw.Add(new PointF((float)Xvals[i], (float)val));

                        StDev += Math.Pow(Yvals[i] - val, 2);
                    }
                    */
                    this.GLDrawing_Start();
                }
                else
                {
                    this.Titles.Items.Clear();
                    Titles.Height = 10;
                    this.ResumeLayout();
                }
            }
            private void SubFormulas_Load( MySolver.FitSettings curFit)
            {
                List<string> l = curFit.FormulasForNcalc();

                foreach (Series s in FitFormulas)
                    s.Points.Clear();

                if (l.Count == 0) return;
                
                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                for (int i = 0; i < l.Count; i++)
                {
                    Series ser;
                    if (FitFormulas.Count > i)
                        ser = FitFormulas[i];
                    else
                    {
                        ser = new ResultsExtractor_CTChart.Series();
                        ser.Color = ColorTranslator.FromHtml(colMatrix[ColorIndex(i)]);
                        this.ChartSeries.Add(ser);
                        FitFormulas.Add(ser);
                    }

                    string formula = l[i];
                    Expression e = new Expression(formula);

                    foreach (var kvp in curFit.Parameters)
                    {
                        MySolver.Parameter p = kvp.Value;
                        e.Parameters[p.Name] = p.Value;
                        //val = val.Replace(p.Name, p.Value.ToString());
                    }

                    for (int i1 = 0; i1 < Xvals.Length && i1 < Yvals.Length; i1++)
                    {
                        //fit
                        e.Parameters["t"] = Xvals[i1];

                        double val = 0;
                        double.TryParse(e.Evaluate().ToString(), out val);

                        ser.Points.AddXY(Xvals[i1], val);
                    }
                }
                this.GLDrawing_Start();
            }
            private void LoadFRAPAData(MySolver.FitSettings curFit)
            {
                double[] Xvals = curFit.XVals;
                double[] Yvals = curFit.YVals;

                double[][] CalcFitVals = FRAPA_Model.AllModels.CalcFitVals(curFit.GetFormulaIF,Xvals, curFit.Parameters);
                double[] StDev = new double[2];

                int frame = (int)curFit.Parameters["from"].Value;
                //raw
                for (int i1 = 0; i1 < Xvals.Length && i1 < Yvals.Length; i1++)
                {
                    Raw.Points.AddXY(Xvals[i1], Yvals[i1]);
                }

                for (int i = 0; i < CalcFitVals.Length; i++)
                {
                    //create new chart series
                    Series ser;
                    if (FitFormulas.Count > i)
                        ser = FitFormulas[i];
                    else
                    {
                        ser = new Series();
                        ser.Color = ColorTranslator.FromHtml(colMatrix[ColorIndex(i)]);
                        this.ChartSeries.Add(ser);
                        FitFormulas.Add(ser);
                    }
                    
                    //Calculate the curves and stdev
                    
                    for (int i1 = frame; i1 < Xvals.Length && i1 < Yvals.Length; i1++)
                    {
                        //fit
                        ser.Points.AddXY(Xvals[i1], CalcFitVals[i][i1]);
                        StDev[i] += Math.Pow(Yvals[i1] - CalcFitVals[i][i1], 2);
                    }

                    if(ser.Points.Count!=0)
                        StDev[i] = Math.Sqrt(StDev[i] / (ser.Points.Count));

                    curFit.StDev = 0;
                }
                FitFormulas[0].Color = Color.Green;
                if (CalcFitVals.Length > 1)
                    FitFormulas[1].Color = Color.Magenta;

                this.Titles.Items.Clear();
                Titles.Height = 120;
                if (CalcFitVals.Length > 1)
                {
                    this.Titles.Items.AddRange(new string[]{
                    "",
                    "\tRoot mean square deviation:",
                    "\tFRAP eq.(green) = " + StDev[0] ,
                    "\tDiffusion eq. (magenta) = " + StDev[1] ,
                    "",
                   "\tR-squared:",
                    "\tFRAP eq.(green) = " + Math.Pow(FRAPA_Model.ComputeCorelationCoeff(Yvals, CalcFitVals[0],frame),2) ,
                    "\tDiffusion eq. (magenta) = " + Math.Pow(FRAPA_Model.ComputeCorelationCoeff(Yvals, CalcFitVals[1], frame),2) });
                }
                else
                {
                    this.Titles.Items.AddRange(new string[]{
                    "",
                    "\tRoot mean square deviation:",
                    "\tBinding + Diffusion eq. = " + StDev[0] ,
                    "",
                   "\tR-squared:",
                    "\tBinding + Diffusion eq. = " + Math.Pow(FRAPA_Model.ComputeCorelationCoeff(Yvals, CalcFitVals[0],frame),2)
                    });
                }
                this.GLDrawing_Start();
            }
            private int ColorIndex(int i)
            {
                int cur = i;

                while (cur >= colMatrix.Length)
                    cur -= colMatrix.Length;

                return cur;
            }
        }
    }
}
