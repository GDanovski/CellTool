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
    class SolverFunctions
    {
        private ResultsExtractor.MyForm form1;
        public SolverDialog dialog;        

        public SolverFunctions(ResultsExtractor.MyForm form1)
        {
            this.form1 = form1;
            dialog = new SolverDialog(form1);
        }
        public class FunctionValue
        {
            private string _Name;
            private string _formulaIf;
            private string _formula1;
            private string _formula2;
            private string[] _parameters;
            public FunctionValue()
            {

            }
            public FunctionValue(string input)
            {
                input.Replace("\n", "\t");
                string[] vals = input.Split(new string[] { "\t" }, StringSplitOptions.None);
                _Name = vals[0];
                _formula1 = vals[1];
                _formulaIf = vals[2];
                _formula2 = vals[3];
                _parameters = vals[4].Substring(0,vals[4].Length-1).Split(new string[] { ";" }, StringSplitOptions.None);
            }
            public string GetName
            {
                get { return _Name; }
            }
            public string SetName
            {
                set { _Name = value; }
            }
            public string GetFormula1
            {
                get { return _formula1; }
            }
            public string SetFormula1
            {
                set { _formula1 = value; }
            }
            public string GetFormula2
            {
                get { return _formula2; }
            }
            public string SetFormula2
            {
                set { _formula2 = value; }
            }
            public string GetFormulaIF
            {
                get { return _formulaIf; }
            }
            public string SetFormulaIF
            {
                set { _formulaIf = value; }
            }
            public string[] GetParameters
            {
                get { return _parameters; }
            }
            public string[] SetParameters
            {
                set { _parameters = value; }
            }
            public Dictionary<string, MySolver.Parameter> GetParametersDictionary()
            {
                Dictionary<string, MySolver.Parameter> dict = new Dictionary<string, MySolver.Parameter>();

                foreach (var p in _parameters)
                    if(!dict.Keys.Contains(p))
                        dict.Add(p, new MySolver.Parameter(p));

                return dict; 
            }
            
            public string ToStringLines()
            {
                return 
                   _Name + "\n" +
                   _formula1 + "\n" +
                   _formulaIf + "\n" +
                   _formula2 + "\n" + 
                   string.Join(";",_parameters) + ";";                
            }
            public string ToStringTabs()
            {
                return
                   _Name + "\t" +
                   _formula1 + "\t" +
                   _formulaIf + "\t" +
                   _formula2 + "\t" +
                   string.Join(";", _parameters) + ";";
            }
        }
        public class SolverDialog :Form
        {
            private ResultsExtractor.MyForm form1;
            public FunctionLibrary tw;

            private TextBox nameTB;
            private TextBox ifTB;
            private TextBox form1TB;
            private TextBox form2Tb;
            private TextBox parTB;

            public SolverDialog(ResultsExtractor.MyForm form1)
            {
                this.form1 = form1;
                this.SuspendLayout();

                //settings
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Normal;
                this.Icon = Properties.Resources.CT_done;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.BackColor = ResultsExtractor.Parametars.BackGroundColor;
                this.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                this.Width = 450;
                this.Height = 300;
                this.MinimumSize = new Size(300, 300);
                this.Text = "Solver Functions";
                //controls
                tw = new FunctionLibrary(form1);
                this.Controls.Add(tw);
                //Menu Panel
                Panel MenuPanel = new Panel();
                MenuPanel.Height = 30;
                MenuPanel.Dock = DockStyle.Top;
                this.Controls.Add(MenuPanel);

                MenuStrip Menu = new MenuStrip();
                Menu.BackColor = ResultsExtractor.Parametars.BackGroundColor;
                Menu.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                MenuPanel.Controls.Add(Menu);

                //Open Btn
                ToolStripMenuItem OpenBtn = new ToolStripMenuItem();
                OpenBtn.Text = "Load";
                OpenBtn.BackColor = ResultsExtractor.Parametars.BackGroundColor;
                OpenBtn.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                OpenBtn.Click += Load_Click;
                Menu.Items.Add(OpenBtn);

                ToolStripMenuItem ExportBtn = new ToolStripMenuItem();
                ExportBtn.Text = "Export";
                ExportBtn.BackColor = ResultsExtractor.Parametars.BackGroundColor;
                ExportBtn.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                ExportBtn.Click += Export_Click;
                Menu.Items.Add(ExportBtn);

                //Settings panel
                Panel setP = new Panel();
                setP.Width = 280;
                setP.Dock = DockStyle.Fill;
                this.Controls.Add(setP);
                setP.BringToFront();

                setP.SuspendLayout();

                int h = 5;
                nameTB = new TextBox();
                ifTB = new TextBox();
                form1TB = new TextBox();
                form2Tb = new TextBox();
                parTB = new TextBox();

                TextBox tb = nameTB;
                {
                    //label
                    Label lab = new Label();
                    lab.Text = "Name:";
                    lab.Width = 60;
                    lab.Location = new Point(5, h + 3);
                    setP.Controls.Add(lab);
                    //text box
                    tb.Width = 180;
                    tb.Location = new Point(70, h);
                    setP.Controls.Add(tb);
                    h += 30;
                    tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }

                tb = ifTB;
                {
                    //label
                    Label lab = new Label();
                    lab.Text = "IF:";
                    lab.Width = 60;
                    lab.Location = new Point(5, h + 3);
                    setP.Controls.Add(lab);
                    //text box
                    tb.Width = 180;
                    tb.Location = new Point(70, h);
                    setP.Controls.Add(tb);
                    h += 30;
                    tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }

                tb = form1TB;
                {
                    //label
                    Label lab = new Label();
                    lab.Text = "F1():";
                    lab.Width = 60;
                    lab.Location = new Point(5, h + 3);
                    setP.Controls.Add(lab);
                    //text box
                    tb.Width = 180;
                    tb.Location = new Point(70, h);
                    setP.Controls.Add(tb);
                    h += 30;
                    tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }

                tb = form2Tb;
                {
                    //label
                    Label lab = new Label();
                    lab.Text = "F2():";
                    lab.Width = 60;
                    lab.Location = new Point(5, h + 3);
                    setP.Controls.Add(lab);
                    //text box
                    tb.Width = 180;
                    tb.Location = new Point(70, h);
                    setP.Controls.Add(tb);
                    h += 30;
                    tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }

                tb = parTB;
                {
                    //label
                    Label lab = new Label();
                    lab.Text = "Variables:";
                    lab.Width = 60;
                    lab.Location = new Point(5, h + 3);
                    setP.Controls.Add(lab);
                    //text box
                    tb.Width = 180;
                    tb.Location = new Point(70, h);
                    setP.Controls.Add(tb);
                    h += 30;
                    tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }

                int w = 5;
                h += 30;

                Button btn = new Button();
                {
                    btn.Width = 80;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Save";
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.Location = new Point(w, h);
                    setP.Controls.Add(btn);
                    w += 85;
                    btn.Click += Save_Click;
                }

                btn = new Button();
                {
                    btn.Width = 80;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Save As New";
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.Location = new Point(w, h);
                    setP.Controls.Add(btn);
                    w += 85;
                    btn.Click += SaveAs_Click;
                }

                btn = new Button();
                {
                    btn.Width = 80;
                    btn.ForeColor = Color.Black;
                    btn.Text = "Delete";
                    btn.BackColor = SystemColors.ButtonFace;
                    btn.Location = new Point(w, h);
                    setP.Controls.Add(btn);
                    w += 85;
                    btn.Click += Delete_Click;
                }

                setP.ResumeLayout();
                //events
                this.FormClosing += Form_Closing;
                tw.AfterSelect += tw_NodeClick;
                tw_Refresh();

                this.ResumeLayout();
            }
            private void Export_Click(object sender, EventArgs e)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                string formatMiniStr = ".txt";
                string formatStr = "TXT files (*" + formatMiniStr + ")|*" + formatMiniStr;
                saveFileDialog1.Filter = formatStr;
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.Title = "Export";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string dir = saveFileDialog1.FileName;
                    if (dir.EndsWith(formatMiniStr) == false)
                        dir += formatMiniStr;

                    try
                    {
                        if (File.Exists(dir))
                            File.Delete(dir);
                    }
                    catch
                    {
                        MessageBox.Show("Save error!\nFile is opened in other program!");
                        return;
                    }

                    List<string> vals = new List<string>();

                    foreach(TreeNode n in tw.Nodes)
                    {
                        FunctionValue f = (FunctionValue)n.Tag;
                        vals.Add(f.GetName);
                        vals.Add(f.GetFormula2);
                        vals.Add(f.GetFormulaIF);
                        vals.Add(f.GetFormula1);
                        vals.Add(string.Join(";",f.GetParameters) + ";");
                        vals.Add("");
                    }

                    File.WriteAllLines(dir, vals);
                    vals = null;
                }
            }
            private void Load_Click(object sender, EventArgs e)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                string formatMiniStr = ".txt";
                string formatStr = "TXT files (*" + formatMiniStr + ")|*" + formatMiniStr;
                openFileDialog1.Filter = formatStr;
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Title = "Load";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string dir = openFileDialog1.FileName;
                       
                        List<string> vals = new List<string>();
                        using (StreamReader sr = new StreamReader(dir))
                        {
                            string str = sr.ReadLine();
                                                       
                            while (str != null)
                            {
                                vals.Add(str);
                                str = sr.ReadLine();
                            }
                        }

                        tw.SuspendLayout();

                        for (int i = 0; i < vals.Count; i += 6)
                        {
                            string str = vals[i] + "\t" +
                                vals[i + 3] + "\t" +
                                vals[i + 2] + "\t" +
                                vals[i + 1] + "\t" +
                                vals[i + 4];

                            FunctionValue f = new FunctionValue(str);
                            TreeNode n = new TreeNode(vals[i]);
                            n.Tag = f;
                            tw.Nodes.Add(n);
                        }

                        vals = null;

                        tw_Refresh();
                        tw.SaveFunctions();
                        tw.ResumeLayout();
                        funcCB_refresh(form1.solverClass.parametersPanel.cmbBox);
                    }
                    catch
                    {
                        MessageBox.Show("Incorrect input file!");
                    }
                }
            }
            private bool IsValide()
            {
                if (nameTB.Text != "" &&
                    ifTB.Text != "" &&
                    form1TB.Text != "" &&
                    form2Tb.Text != "" &&
                    parTB.Text != "")
                    return true;

                MessageBox.Show("Error: Empty field!");
                return false;
            }
            private void Save_Click(object sender,EventArgs e)
            {
                if (!IsValide()) return;

                if (tw.Nodes.Count == 0)
                {
                    SaveAs_Click(sender, e);
                    return;
                }

                for (int i = 0; i < tw.Nodes.Count; i++)
                    if (tw.Nodes[i].BackColor == ResultsExtractor.Parametars.TitlePanelColor)
                    {
                        FunctionValue f = (FunctionValue)tw.Nodes[i].Tag;
                        f.SetName = nameTB.Text;
                        tw.Nodes[i].Text = nameTB.Text;
                        f.SetFormulaIF = ifTB.Text;
                        f.SetFormula1 = form1TB.Text;
                        f.SetFormula2 = form2Tb.Text;
                        f.SetParameters = parTB.Text
                            .Substring(0, parTB.Text.Length-1)
                            .Split(new string[] { ";"},StringSplitOptions.None);

                        break;
                    }

                tw.SaveFunctions();
                funcCB_refresh(form1.solverClass.parametersPanel.cmbBox);
            }
            private void SaveAs_Click(object sender, EventArgs e)
            {
                if (!IsValide()) return;

                FunctionValue f = new FunctionValue();
                f.SetName = nameTB.Text;
                f.SetFormulaIF = ifTB.Text;
                f.SetFormula1 = form1TB.Text;
                f.SetFormula2 = form2Tb.Text;
                f.SetParameters = parTB.Text
                            .Substring(0, parTB.Text.Length - 1)
                            .Split(new string[] { ";" }, StringSplitOptions.None);

                TreeNode n = new TreeNode(nameTB.Text);
                n.Tag = f;
                tw.Nodes.Add(n);

                foreach (TreeNode n1 in tw.Nodes)
                    if (n1.BackColor != ResultsExtractor.Parametars.BackGround2Color)
                    {
                        n1.BackColor = ResultsExtractor.Parametars.BackGround2Color;
                    }
                n.BackColor = ResultsExtractor.Parametars.TitlePanelColor;

                tw.SaveFunctions();
                funcCB_refresh(form1.solverClass.parametersPanel.cmbBox);
            }
            private void Delete_Click(object sender, EventArgs e)
            {
                for (int i = 0; i < tw.Nodes.Count; i++)
                    if (tw.Nodes[i].BackColor == ResultsExtractor.Parametars.TitlePanelColor)
                    {
                        tw.Nodes.RemoveAt(i);

                        if (tw.Nodes.Count>1 && i>0)
                            tw.Nodes[i - 1].BackColor = ResultsExtractor.Parametars.TitlePanelColor;
                        else if(tw.Nodes.Count > 0)
                            tw.Nodes[0].BackColor = ResultsExtractor.Parametars.TitlePanelColor;

                        break;
                    }
               tw.SaveFunctions();
                funcCB_refresh(form1.solverClass.parametersPanel.cmbBox);
            }
            public void funcCB_refresh(ComboBox cmbBox)
            {
                int ind = 0;
                if (cmbBox.Items.Count >= 0)
                    ind = cmbBox.SelectedIndex;

                cmbBox.Items.Clear();
                FRAPA_Model.AllModels.LoadModelNames(cmbBox);
                

                foreach (TreeNode n in tw.Nodes)
                    cmbBox.Items.Add(n.Text);

                if (ind >= cmbBox.Items.Count)
                    ind = cmbBox.Items.Count - 1;

                if (ind >= 0)
                    cmbBox.SelectedIndex = ind;
            }
            private void tw_Refresh()
            {
                if (tw.Nodes.Count != 0)
                {
                    FunctionValue f = null;
                    foreach (TreeNode n in tw.Nodes)
                        if (n.BackColor == ResultsExtractor.Parametars.TitlePanelColor)
                        {
                            f = (FunctionValue)n.Tag;
                        }
                    if (f == null)
                    {
                        f = (FunctionValue)tw.Nodes[0].Tag;
                        tw.Nodes[0].BackColor = ResultsExtractor.Parametars.TitlePanelColor;
                    }

                    nameTB.Text = f.GetName;
                    ifTB.Text = f.GetFormulaIF;
                    form1TB.Text = f.GetFormula1;
                    form2Tb.Text = f.GetFormula2;
                    parTB.Text = string.Join(";", f.GetParameters) + ";";
                }
            }
            private void tw_NodeClick(object sender, TreeViewEventArgs e)
            {
                foreach (TreeNode n in tw.Nodes)
                    if (n.BackColor != ResultsExtractor.Parametars.BackGround2Color)
                    {
                        n.BackColor = ResultsExtractor.Parametars.BackGround2Color;
                    }

                e.Node.BackColor = ResultsExtractor.Parametars.TitlePanelColor;
                tw_Refresh();
            }
            private void Form_Closing(object sender, FormClosingEventArgs e)
            {
                this.Hide();
                e.Cancel = true;
            }

           public class FunctionLibrary : TreeView
            {
                private Properties.Settings settings = Properties.Settings.Default;
                private ResultsExtractor.MyForm form1;
                public FunctionLibrary(ResultsExtractor.MyForm form1)
                {
                    this.form1 = form1;
                    this.Dock = DockStyle.Left;
                    this.Width = 150;
                    this.ShowNodeToolTips = false;
                    this.BorderStyle = BorderStyle.None;
                    this.BackColor = ResultsExtractor.Parametars.BackGround2Color;
                    this.ForeColor = ResultsExtractor.Parametars.ShriftColor;
                    this.CheckBoxes = false;
                    this.ShowRootLines = false;
                    this.ShowPlusMinus = false;

                    LoadFunctions();
                }

                public void LoadFunctions()
                {
                    this.SuspendLayout();
                    this.Nodes.Clear();

                    string[] vals = settings.SolverFunctions[
                        form1.IA.TabPages.ActiveAccountIndex]
                        .Split(new string[] { "|" },StringSplitOptions.None);

                    foreach (string val in vals)
                        if (val != "@")
                        {
                            FunctionValue f = new FunctionValue(val);
                            TreeNode n = new TreeNode(f.GetName);
                            n.Tag = f;
                            this.Nodes.Add(n);
                        }

                    if(this.Nodes.Count > 0)
                    {
                        this.Nodes[0].BackColor = 
                            ResultsExtractor.Parametars.TitlePanelColor;
                    }

                    this.ResumeLayout();
                }
                public void SaveFunctions()
                {
                    string val = "@";
                    foreach(TreeNode n in this.Nodes)
                    {
                        val += "|" + ((FunctionValue)n.Tag).ToStringTabs();
                    }

                    settings.SolverFunctions[form1.IA.TabPages.ActiveAccountIndex] = val;
                    settings.Save();
                }
            }
        }
    }
}
