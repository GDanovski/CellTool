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

//This is the main form.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    public partial class CellToolMainForm : Form
    {
        Interface Interface = new Interface();
        Security SecurityControl = new Security();
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                    if(this.ActiveControl == Interface.FileBrowser.TreeViewExp || 
                        this.ActiveControl == Interface.FileBrowser.Vbox ||
                        this.ActiveControl is TrackBar ||
                        this.ActiveControl is ComboBox)
                        return base.ProcessCmdKey(ref msg, keyData);
                    Interface.zTrackBar_Backward();
                    return true;
                case Keys.Up:
                    if (this.ActiveControl == Interface.FileBrowser.TreeViewExp ||
                        this.ActiveControl == Interface.FileBrowser.Vbox ||
                        this.ActiveControl is TrackBar ||
                        this.ActiveControl is ComboBox)
                        return base.ProcessCmdKey(ref msg, keyData);
                    Interface.zTrackBar_Forward();
                    return true;
                case Keys.Right:
                    if (this.ActiveControl == Interface.FileBrowser.TreeViewExp ||
                        this.ActiveControl == Interface.FileBrowser.Vbox ||
                        this.ActiveControl is TrackBar)
                        return base.ProcessCmdKey(ref msg, keyData);
                    Interface.TimeTrackBar_Forward();
                    return true;
                case Keys.Left:
                    if (this.ActiveControl == Interface.FileBrowser.TreeViewExp ||
                        this.ActiveControl == Interface.FileBrowser.Vbox ||
                        this.ActiveControl is TrackBar)
                        return base.ProcessCmdKey(ref msg, keyData);
                    Interface.TimeTrackBar_Backward();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
            //return true;
        }
        public CellToolMainForm()
        {
            //For global mouse move detection
            GlobalMouseHandler gmh = new GlobalMouseHandler();
            gmh.TheMouseMoved += new MouseMovedEvent(gmh_TheMouseMoved);
            Application.AddMessageFilter(gmh);
           //Start the component
            InitializeComponent();
            
        }
       
        void gmh_TheMouseMoved()
        {
            //Get cursor position
           Point cur_pos = System.Windows.Forms.Cursor.Position;
           //Check is Drag Drop panel visible - is program in stage dragdrop
           if(Interface.FileBrowser.DragDropPanel.Visible == true)
            {
                //Check if mouse button is up outside the targets for drag and drop
                Interface.FileBrowser.DragDrop_Release();
                //change location of the drag drop panel
                //Interface.FileBrowser.DragDropPanel.Location = 
                    //new System.Drawing.Point((cur_pos.X - this.Location.X), (cur_pos.Y - this.Location.Y - 20));
                //bring drag drop panel to front
                //Interface.FileBrowser.DragDropPanel.BringToFront();
            }
        }
        //Create event that check for mouse move events globaly
        public delegate void MouseMovedEvent();

        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;
            private const int WM_LMOUSEUP = 0x0202;

            public event MouseMovedEvent TheMouseMoved;
          
            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {

                if (m.Msg == WM_MOUSEMOVE)
                {
                    if (TheMouseMoved != null)
                    {
                        TheMouseMoved();
                    }
                   
                }
                else if (m.Msg == WM_LMOUSEUP)
                {
                    TheMouseMoved();
                }
                // Always allow message to continue to the next filter control
                return false;
            }
           
            #endregion
        }
        private void CellToolMainForm_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            
            this.SuspendLayout();
            //Hide main form
            this.Hide();
            this.AllowDrop = true;
            this.DragDrop += new DragEventHandler(Form_DragDropFiles);
            this.DragOver += new DragEventHandler(Form_DragEnter);
            //Show Log0
            Updater.UpdateSettings();
            Interface.ShowLogo();

            //Security control check - is It ok to continue with initializing the program
            SecurityControl.Initialize();
            SecurityControl.ChooseAccount();
            Interface.ActiveAccountIndex = SecurityControl.AccIndex;			
            //set main form propertties
            Interface.MainFormInitialize(this);

            //Developer menu
            Interface.IA.PlugIns = new PlugInEngine(Interface.DeveloperToolStripMenuItem, Interface.IA);
            //HotKeys
            Interface.HotKays = new CTHotKeys(Interface);
            Interface.SmartButtons = new CTSmartButtons(Interface);
            //add hendlers for the interface controls
            AddHendlers();
            
            //resize
            this.Resize += new EventHandler(Form1_Resize);
            this.MinimumSize = new Size(400, 400);
            //Add Account settings Hendlers
            Interface.LogOutToolStripMenuItem.Click += new EventHandler(LogOut_event);
            Interface.ChangePassToolStripMenuItem.Click += new EventHandler(ChangePass_event);
            //Load Account Settings
            LoadAccountSettings();
            //Start File 
            Interface.TabPages.ImageMainPanel.Visible = false;

            this.ResumeLayout(true);
            base.Show();

            Form_StartWithFile();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            if (this.Height < 300) { this.Height = 300; }
            if (this.Width < 300) { this.Width = 300; }
            
            int fbW = Interface.FileBrowser.DataSourcesPanel.Width;
            int propW = Interface.TabPages.propertiesPanel.Width;
            if (this.Width < fbW + propW + 100)
            {
                Interface.FileBrowser.DataSourcesPanel.Width = 15;
            }
            else if (Interface.TabPages.hidePropAndBrows == false &
                this.Width >= int.Parse(SecurityControl.settings.DataSourcesPanelValues[SecurityControl.AccIndex]) 
                + int.Parse(SecurityControl.settings.PropertiesPanelWidth[SecurityControl.AccIndex]) + 100)
            {
                if (SecurityControl.settings.DataSourcesPanelVisible[SecurityControl.AccIndex] == "y")
                {
                    Interface.FileBrowser.DataSourcesPanel.Width = int.Parse(SecurityControl.settings.DataSourcesPanelValues[SecurityControl.AccIndex]);
                }
            }

            if (this.Width < 15 + propW + 100)
            {
                Interface.TabPages.propertiesPanel.Width = 15;
            }
            else if (Interface.TabPages.hidePropAndBrows == false & 
                this.Width >= 15 + int.Parse(SecurityControl.settings.PropertiesPanelWidth[SecurityControl.AccIndex]) + 100)
            {
                if (SecurityControl.settings.PropertiesPanelVisible[SecurityControl.AccIndex] == "y")
                {
                    Interface.TabPages.propertiesPanel.Width = int.Parse(SecurityControl.settings.PropertiesPanelWidth[SecurityControl.AccIndex]);
                }
            }

        }
        private void Form_StartWithFile()
        {
            String[] dirList = Environment.GetCommandLineArgs();
            if (dirList.Length > 0)
            {
                foreach (string dir in dirList)
                {
                    if (dir.Length > 5)
                    {
                        if (dir.Substring(dir.Length - 4, 4) != ".exe") {
                            TreeNode node = Interface.FileBrowser.CheckForFile(OSStringConverter.GetWinString(dir));
                            if (node != null) { Interface.FileBrowser.Openlabel.Tag = node; }
                            Interface.FileBrowser.Openlabel.Text = "'" + OSStringConverter.GetWinString(dir) + "'";
                            Interface.FileBrowser.Openlabel.Text = "";
                        }
                    }
                   
                }
            }
        }
        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void Form_DragDropFiles(object sender,DragEventArgs e)
        {
           string[] dirList = e.Data.GetData(DataFormats.FileDrop) as string[];
         foreach (string dir in dirList)
            {
                TreeNode node = Interface.FileBrowser.CheckForFile(OSStringConverter.GetWinString(dir));
                if (node != null) { Interface.FileBrowser.Openlabel.Tag = node; }
                Interface.FileBrowser.Openlabel.Text = "'" + OSStringConverter.GetWinString(dir) + "'";
                Interface.FileBrowser.Openlabel.Text = "";
            }
        }
        public void LogOut_event(object sender, EventArgs e)
        {
            this.Hide();
            //Log Out
            SecurityControl.AccIndex = -1;
            //Start Login form
            SecurityControl.LogOut_event(sender,e);
            //take index of the account
            Interface.ActiveAccountIndex = SecurityControl.AccIndex;
            //Write acc name on the menu item
            Interface.AccBox_Add();
            //restore form
            LoadAccountSettings();
            this.Show();
        }
        public void ChangePass_event(object sender, EventArgs e)
        {
            //Start Change Pass event
            SecurityControl.ChangePass_event(sender, e);
        }       
        private void LoadAccountSettings()
        {
            //Configurate all account specific setiings here

            //Delete opened images and configurate properties
            if (Interface.FileBrowser.ActiveAccountIndex != SecurityControl.AccIndex)
            {
                if (Interface.TabPages.Collections.Count > 0)
                    for (int i = Interface.TabPages.Collections.Count - 1; i >= 0; i--)
                        Interface.TabPages.TabCollections[i].Saved = true;
                    
                Interface.CloseAllToolStripMenuItem.PerformClick();
                Interface.IA.BandC.panel.Visible = false;
            }
            //data source panel
            Interface.FileBrowser.DataSourcesPanelWidth = int.Parse(SecurityControl.settings.DataSourcesPanelValues[SecurityControl.AccIndex]);
            Interface.FileBrowser.ActiveAccountIndex = SecurityControl.AccIndex;
            if (SecurityControl.settings.DataSourcesPanelVisible[SecurityControl.AccIndex] == "y")
            {
                Interface.FileBrowser.DataSourcesPanel.Width = int.Parse(SecurityControl.settings.DataSourcesPanelValues[SecurityControl.AccIndex]);
            }
            else
            {
                Interface.FileBrowser.DataSourcesPanel.Width = 15;
            }
            //TreeView
            Interface.FileBrowser.TreeViewExp.Height = int.Parse(SecurityControl.settings.TreeViewSize[SecurityControl.AccIndex]);
            Interface.FileBrowser.TreeViewExp_load(SecurityControl.settings.TreeViewContent[SecurityControl.AccIndex]);
            //vbox and treeview settings
            Interface.FileBrowser.Vbox.Nodes.Clear();
            Interface.FileBrowser.Vbox_TreenodesList.Clear();
            //add all
            TreeNode n = new TreeNode();
            n.Text = "All";
            n.Tag = "All";
            n.ImageIndex = 0;
            n.SelectedImageIndex = 0;
            n.Checked = false;
            Interface.FileBrowser.Vbox.Nodes.Add(n);

            TreeNode n1 = new TreeNode();
            n1.Text = "All";
            n1.Tag = "All";
            n1.Checked = true;
            Interface.FileBrowser.Vbox_TreenodesList.Add(n1);

            if (SecurityControl.settings.VBoxVisible[SecurityControl.AccIndex] == "n")
            {
                if (SecurityControl.settings.TreeViewVisible[SecurityControl.AccIndex] == "n")
                {
                    Interface.FileBrowser.VBoxTitlePanel.Dock = DockStyle.Top;
                    Interface.FileBrowser.TreeViewExp.Dock = DockStyle.Top;
                    Interface.FileBrowser.TreeViewExp.Visible = false;
                }
                else
                {
                    Interface.FileBrowser.VBoxTitlePanel.Dock = DockStyle.Bottom;
                    Interface.FileBrowser.TreeViewExp.Dock = DockStyle.Fill;
                    Interface.FileBrowser.TreeViewExp.Visible = true;
                }
                Interface.FileBrowser.Vbox.Visible = false;
            }
            else
            {
                if (SecurityControl.settings.TreeViewVisible[SecurityControl.AccIndex] == "n")
                {
                    Interface.FileBrowser.TreeViewExp.Visible = false;
                }
                else
                {
                    Interface.FileBrowser.TreeViewExp.Visible = true;
                }
                Interface.FileBrowser.VBoxTitlePanel.Dock = DockStyle.Top;
                Interface.FileBrowser.TreeViewExp.Dock = DockStyle.Top;
                Interface.FileBrowser.Vbox.Visible = true;
            }
            //Properties panel
            Interface.TabPages.ActiveAccountIndex = SecurityControl.AccIndex;
            if (SecurityControl.settings.PropertiesPanelVisible[SecurityControl.AccIndex] == "n")
            {
                Interface.TabPages.propertiesPanel.Width = 15;
            }
            else
            {
                Interface.TabPages.propertiesPanel.Width = int.Parse(SecurityControl.settings.PropertiesPanelWidth[SecurityControl.AccIndex]);
            }
            //Brightness and Contrast
            Interface.IA.BandC.panel.Height = int.Parse(SecurityControl.settings.BandC[SecurityControl.AccIndex]);
            //Meta
            Interface.IA.Meta.panel.Height = int.Parse(SecurityControl.settings.Meta[SecurityControl.AccIndex]);

            //Chart
            Interface.IA.chart.Properties.LoadFunctions();
            //Properties
			Interface.IA.Segmentation.AutoSetUp.ApplyToNewCheckB.Checked = bool.Parse(SecurityControl.settings.AutoProtocolSettings[SecurityControl.AccIndex]);
            Interface.IA.Segmentation.AutoSetUp.LoadSettings();
            //HotKeys
            Interface.HotKays.LoadAccountSettings();
            Interface.SmartButtons.LoadAccSettings();
            //Account Export/Import
            Interface.ExportAccToolStripMenuItem.Click += ExportSettings_Click;
            Interface.ImportAccToolStripMenuItem.Click += ImportSettings_Click;
        }
        public void ExportSettings_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            string formatMiniStr = ".CTProfile";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;
            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Title = "Export settings";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = saveFileDialog1.FileName;

                List<string> vals = SecurityControl.PrepareSettingsForExport();

                try
                {
                    System.IO.File.WriteAllText(dir, string.Join("$", vals));
                }
                catch
                {
                    MessageBox.Show("File is not avaliable!");
                }
                vals = null;
            }

            saveFileDialog1 = null;
        }
        public void ImportSettings_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string formatMiniStr = ".CTProfile";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;

            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Title = "Import settings:";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] vals = System.IO.File.ReadAllText(ofd.FileName)
                        .Split(new string[] { "$" }, StringSplitOptions.None);

                    foreach (string str in vals)
                        SecurityControl.ApplySettingsFromImport(str);

                    LoadAccountSettings();
                    Interface.IA.ReloadImages();

                    vals = null;
                }
                catch
                {
                    MessageBox.Show("File is not avaliable!");
                }
            }
            ofd = null;
        }
        private void AddHendlers()
        {
            //Add all hendlers
            //Open File
            Interface.FileBrowser.Openlabel.TextChanged += new EventHandler(Interface.TabPages.Openlabel_textChanged);
            //Rename File from TreeView
            Interface.FileBrowser.renameLabel.TextChanged += new EventHandler(Interface.TabPages.treeNode_Rename);
            //Menu
            Interface.NewToolStripMenuItem.Click += new EventHandler(Interface.TabPages.OpenEmptyResultsExtractor);
            Interface.CloseToolStripMenuItem.Click += new EventHandler(Interface.TabPages.DeleteSelected);
            Interface.CloseAllToolStripMenuItem.Click += new EventHandler(Interface.TabPages.DeleteAll);
            Interface.OpenToolStripMenuItem.Click += new EventHandler(OpenFile);
            Interface.SaveToolStripMenuItem.Click += new EventHandler(Interface.TabPages.SaveFile);
            Interface.SaveAllToolStripMenuItem.Click += new EventHandler(Interface.TabPages.SaveAllFile);
            Interface.SaveAsToolStripMenuItem.Click += new EventHandler(Interface.TabPages.saveAs);
            Interface.ExportToolStripMenuItem.Click += Interface.IA.RoiMan.ExportRoiAsIJMacro;
            Interface.ExportToolStripMenuItem.Click += Interface.TabPages.ExportResultsExtractorData;
            Interface.ExportBtn.Click += Interface.IA.RoiMan.ExportRoiAsIJMacro;
            Interface.ExportBtn.Click += Interface.TabPages.ExportResultsExtractorData;
            Interface.ExportAllBtn.Click += Interface.IA.RoiMan.ExportAllResults;
            //taskBar
            Interface.NewBtn.Click += new EventHandler(Interface.TabPages.OpenEmptyResultsExtractor);
            Interface.OpenBtn.Click += new EventHandler(OpenFile);
            Interface.SaveBtn.Click += new EventHandler(Interface.TabPages.SaveFile);
            Interface.SaveAsBtn.Click += new EventHandler(Interface.TabPages.saveAs);
            Interface.SaveAllBtn.Click += new EventHandler(Interface.TabPages.SaveAllFile);
            
            Interface.HotKeysToolStripMenuItem.Click += Interface.HotKays.HotKeysToolStripMenuItem_Click;
            Interface.SmartBtnToolStripMenuItem.Click += Interface.SmartButtons.SmartBtnToolStripMenuItem_Click;
        }
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string formatStr = "TIF files (*.tif)|*.tif|All files (*.*)|*.*";
            /*
            foreach (string formatMiniStr in Interface.TabPages.myFileDecoder.Formats)
            {
                formatStr += "|" + formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;
            }*/
                        
            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
                       
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                
                TreeNode node = Interface.FileBrowser.CheckForFile(OSStringConverter.GetWinString(ofd.FileName));
                if(node != null) { Interface.FileBrowser.Openlabel.Tag = node; }
                Interface.FileBrowser.Openlabel.Text = "'" + OSStringConverter.GetWinString(ofd.FileName) + "'";
                Interface.FileBrowser.Openlabel.Text = "";
            }
        }
    }
}
