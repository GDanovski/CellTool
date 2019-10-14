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

// Code located in this class is responsible for the disign of the form interface.
// Also here are all the voids conected with changing the interface

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Input;


namespace Cell_Tool_3
{
    class Interface
    {
        Form MainForm;
        //Global Variables
        //Colors
        private Color BackGroundColor = Color.DimGray;
        private Color BackGround2Color = Color.FromArgb(255, 60, 60, 60);
        private Color ShriftColor = Color.White;
        //private Color TaskBtnClickedColor = Color.FromArgb(255,150,150,150);
        private Color TaskBtnClickedColor = Color.FromArgb(255, 125, 125, 125);
        private Color TaskBtnColor = Color.DarkGray;
        private Color TitlePanelColor = Color.CornflowerBlue;
        //private Color TitlePanelColor = Color.FromArgb(255, 40, 190, 115);
        //private Color TitlePanelColor = Color.FromArgb(255, 40, 190, 115); //laim
        //private Color TitlePanelColor = Color.FromArgb(255, 200, 50, 0); //keremida papki
        //private Color TitlePanelColor = Color.FromArgb(255, 215, 90, 0); //orange
        //private Color TitlePanelColor = Color.FromArgb(255, 0, 150, 150); //cyan dark
        //private Color TitlePanelColor = Color.FromArgb(255, 150, 0, 200); //purple
        //private Color TitlePanelColor = Color.FromArgb(255, 70, 140, 255); //CornflowerBlue

        //Activ Account
        public int ActiveAccountIndex = 0;
        //BackGround Panel
        public Panel MainPanel = new Panel();
        /////////////////////////////////////////
        //Task bar
        public ToolStrip taskTS;
        public ToolStripComboBox ZoomValue = new ToolStripComboBox();
        //Roi type
        public ToolStripButton TrackingBtn = new ToolStripButton();
        public ToolStripButton StaticBtn = new ToolStripButton();
        public ToolStripButton DoubleRoiBtn = new ToolStripButton();
        //roi shape
        public ToolStripButton PolygonBtn = new ToolStripButton();
        public ToolStripButton OvalBtn = new ToolStripButton();
        public ToolStripButton RectangularBtn = new ToolStripButton();
        public ToolStripButton FreehandBtn = new ToolStripButton();
        public ToolStripButton MagicWandBtn = new ToolStripButton();
        //Undo & Redo
        public ToolStripButton ReDoBtn = new ToolStripButton();
        public ToolStripButton UnDoBtn = new ToolStripButton();
        //File options
        public ToolStripButton NewBtn = new ToolStripButton();
        public ToolStripButton OpenBtn = new ToolStripButton();
        public ToolStripButton SaveBtn = new ToolStripButton();
        public ToolStripButton SaveAsBtn = new ToolStripButton();
        public ToolStripButton SaveAllBtn = new ToolStripButton();
        public ToolStripButton ExportBtn = new ToolStripButton();
        ////////////////////////////////////////////////
        //Menu

        //File
        public ToolStripMenuItem ExitToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem CloseAllToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem CloseToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ExportToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ExportAllBtn = new ToolStripMenuItem();
        public ToolStripMenuItem SaveAllToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SaveAsToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SaveToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem OpenToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem NewToolStripMenuItem = new ToolStripMenuItem();
        //Edit
        public EditMenu editMenu1 = null;
        public ToolStripMenuItem RedoToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem UndoToolStripMenuItem = new ToolStripMenuItem();
        //View
        public ToolStripMenuItem RestoreToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem RotateRightToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem RotateLeftToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ZoomOutToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ZoomInToolStripMenuItem = new ToolStripMenuItem();
        //Developer menu
        public ToolStripMenuItem DeveloperToolStripMenuItem = new ToolStripMenuItem();
        //Account settings part
        private ToolStripMenuItem AccountToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ChangePassToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem LogOutToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ExportAccToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ImportAccToolStripMenuItem = new ToolStripMenuItem();
        //Setting
        public ToolStripMenuItem ThemeToolStripMenuItem = new ToolStripMenuItem();
        //Update
        public ToolStripMenuItem UpdateToolStripMenuItem = new ToolStripMenuItem();
        private bool update = false;
        //HotKeys
        public ToolStripMenuItem HotKeysToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SmartBtnToolStripMenuItem = new ToolStripMenuItem();
        ////////////////////////////////////////////
        /// StatusBar
        public ToolStripProgressBar StatusProgressBar = new ToolStripProgressBar();
        public ToolStripStatusLabel StatusLabel = new ToolStripStatusLabel();
        ///////////////////////////////
        //FileBrowser

        public CTFileBrowser FileBrowser = new CTFileBrowser();
        //TabPageControl
        public TabPageControl TabPages = new TabPageControl();
        //Analyser tools
        public ImageAnalyser IA = new ImageAnalyser();
        public CTHotKeys HotKays;
        public CTSmartButtons SmartButtons;
        public void ShowLogo()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Bitmap bmp = Properties.Resources.G_R_done_1_ALL_rights_reserved1;
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString("Version: " + version, new Font("Tahoma", 8), Brushes.White, 1,5);
            g.Flush();
            g = null;

            // Set Logo Form settings
            Form LogoForm = new Form();
            LogoForm.StartPosition = FormStartPosition.CenterScreen;
            LogoForm.WindowState = FormWindowState.Normal;
            LogoForm.Width = 275;
            LogoForm.Height = 222;
            LogoForm.Icon = Properties.Resources.CT_done;
            LogoForm.BackgroundImage = bmp;
            LogoForm.ControlBox = false;
            LogoForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            LogoForm.Show();
            //Force update GUI
            Application.DoEvents();
            LogoForm.Invalidate();
            LogoForm.Update();
            LogoForm.Refresh();
            Application.DoEvents();

            //Stop the tread
            Thread.Sleep(3000);
            LogoForm.Close();
            LogoForm.Dispose();
        }

        public void MainFormInitialize(Form MainForm)
        {
            this.MainForm = MainForm;
            // Set Main Form settings
            MainForm.SuspendLayout();
            MainForm.Icon = Properties.Resources.CT_done;
            MainForm.Text = "CellTool";
            MainForm.WindowState = FormWindowState.Maximized;
            
            //MainForm closing
            MainForm.FormClosing += new FormClosingEventHandler(CloseProgram);
            //MainPanel options
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.BackColor = BackGroundColor;
            MainForm.Controls.Add(MainPanel);
            StatusLabel.TextChanged += new EventHandler(StatusLabel_TextChange);
            //StatusProgressBar.VisibleChanged += StatusProgressBar_visibleChanged;
            MainPanel.CursorChanged += new EventHandler(delegate (Object o, EventArgs a)
            {
                MainForm.Cursor = MainPanel.Cursor;
                //MainForm.Refresh();
            });
            //Add MenuItem
            Menu(MainPanel);
            TaskBar(MainPanel);
            AccBox_Add();
            StatusPanel();
            //File browsers
            FileBrowser.Initialize(ActiveAccountIndex, MainPanel, BackGroundColor, BackGround2Color, ShriftColor,  TitlePanelColor, TaskBtnColor, TaskBtnClickedColor);
            FileBrowser.StatusLabel = StatusLabel;
            //Tab Pages control
            TabPages.OpenPanel = FileBrowser.OpenPanel;
            TabPages.Initialize(MainForm, ActiveAccountIndex, MainPanel, BackGroundColor, BackGround2Color, ShriftColor, TitlePanelColor, TaskBtnColor, TaskBtnClickedColor);
            //add formats
            TabPages.myFileDecoder.LoadExtensions();
            TabPages.myFileDecoder.StatusLabel = StatusLabel;
            TabPages.myFileDecoder.StatusBar = StatusProgressBar;
            FileBrowser.Formats = TabPages.myFileDecoder.Formats;
            TabPages.FileBrowser = FileBrowser;
            //add analyser nodes
            IA.TabPages = TabPages;
            TabPages.IA = IA;
            IA.FileBrowser = FileBrowser;
            IA.BandC.IA = IA;
            IA.Meta.IA = IA;
            IA.IDrawer.IA = IA;
            IA.zoomValue = ZoomValue;
            editMenu1.setIA = IA;

            IA.Initialize(UnDoBtn,ReDoBtn);
            //MainForm.Controls.Add((Panel)IA.GLControl1.Parent);
            //((Panel)IA.GLControl1.Parent).BringToFront();

            TabPages.AddPlugIns();
            //Add hot keys to main form
            MainForm.KeyPreview = true;
            MainForm.KeyDown += new KeyEventHandler(Form1_KeyPress);
            
            // Show form on the screen
            MainForm.ResumeLayout(true);
            
            //MainForm.Show();
            //Animation
            InitializeAnimations();
        }
        private void InitializeAnimations()
        {
            System.Windows.Forms.Timer tTimer = new System.Windows.Forms.Timer();
            bool tTimerRunning = false;
            System.Windows.Forms.Timer zTimer = new System.Windows.Forms.Timer();
            bool zTimerRunning = false;

            TifFileInfo oldFI = null;
            //Time
            TabPages.tPlayStop.Click += new EventHandler(delegate (object o, EventArgs a) 
            {
                if (zTimerRunning) return;

                TabPages.tPlayStop.Image = Properties.Resources.Play;
                tTimer.Stop();
                oldFI = null;

                if (tTimerRunning)
                {
                    tTimerRunning = false;
                    return;
                }
                
                TifFileInfo fi = null;
                try
                {
                    fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
                }
                catch { }
                if (fi == null) return;
                if (!fi.loaded) return;
                if (fi.frame + 1 >= fi.sizeT) return;

                oldFI = fi;

                tTimerRunning = true;
                TabPages.tPlayStop.Image = Properties.Resources.Stop;
                tTimer.Start();
            });

            tTimer.Tick += new EventHandler(delegate (object o, EventArgs a) 
            {
                TifFileInfo fi = null;
                try
                {
                    fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
                }
                catch { }

                if (fi == null ||
                fi != oldFI ||
                !fi.loaded ||
                fi.frame + 1 >= fi.sizeT)
                {
                    oldFI = null;
                    tTimerRunning = false;
                    TabPages.tPlayStop.Image = Properties.Resources.Play;
                    tTimer.Stop();
                }

                TimeTrackBar_Forward();
            });
            tTimer.Interval = 40;
       //Z

            TabPages.zPlayStop.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                if (tTimerRunning) return;
                TabPages.zPlayStop.Image = Properties.Resources.Play;
                zTimer.Stop();
                oldFI = null;

                if (zTimerRunning)
                {
                    zTimerRunning = false;
                    return;
                }

                TifFileInfo fi = null;
                try
                {
                    fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
                }
                catch { }
                if (fi == null) return;
                if (!fi.loaded) return;
                if (fi.zValue + 1 >= fi.sizeZ) return;

                oldFI = fi;

                zTimerRunning = true;
                TabPages.zPlayStop.Image = Properties.Resources.Stop;
                zTimer.Start();
            });

            zTimer.Tick += new EventHandler(delegate (object o, EventArgs a)
            {
                TifFileInfo fi = null;
                try
                {
                    fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
                }
                catch { }

                if (fi == null ||
                fi != oldFI ||
                !fi.loaded ||
                fi.zValue + 1 >= fi.sizeZ)
                {
                    oldFI = null;
                    zTimerRunning = false;
                    TabPages.zPlayStop.Image = Properties.Resources.Play;
                    zTimer.Stop();
                }

                zTrackBar_Forward();
            });

            zTimer.Interval = 40;
        }
        public void AccBox_Add()
        {
             AccountToolStripMenuItem.Text = Properties.Settings.Default.AccList[ActiveAccountIndex];
        }
        private void CloseProgram(object sander, FormClosingEventArgs e)
        {
            Application.Exit();
            return;

            if (update) return;
            Helpers.Settings.SaveSettings();
            // Check is it ok to close the program
            string message = "Do you want to exit the program?";
            string caption = "CellTool";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons);


            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                e.Cancel = false;
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
            }
        }
        private void CloseProgram1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void Form1_KeyPress(object sender, KeyEventArgs e)
        {
            //if (Control.ModifierKeys == Keys.Control)
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z:
                        UnDoBtn.PerformClick();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.Y:
                        ReDoBtn.PerformClick();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.N:
                        NewToolStripMenuItem.PerformClick();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.O:
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        OpenBtn.PerformClick();
                        break;
                    case Keys.S:
                        if (e.Shift)
                        {
                            SaveAsBtn.PerformClick();
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        else
                        {
                            SaveBtn.PerformClick();
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    case Keys.A:
                        if (e.Shift)
                        {
                            SaveAllBtn.PerformClick();
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        else
                        {
                            IA.RoiMan.selectAllRois(e);

                        }
                        break;
                    case Keys.E:
                        ExportBtn.PerformClick();
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    case Keys.Oemplus:
                        if (ZoomValue.SelectedIndex < ZoomValue.Items.Count - 1)
                        {
                            ZoomValue.SelectedIndex += 1;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    case Keys.OemMinus:
                        if (ZoomValue.SelectedIndex > 0)
                        {
                            ZoomValue.SelectedIndex -= 1;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    case Keys.Add:
                        if (ZoomValue.SelectedIndex < ZoomValue.Items.Count - 1)
                        {
                            ZoomValue.SelectedIndex += 1;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    case Keys.Subtract:
                        if (ZoomValue.SelectedIndex > 0)
                        {
                            ZoomValue.SelectedIndex -= 1;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    case Keys.T:
                        IA.RoiMan.AddBtn_Click(sender, e);
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    case Keys.D:
                        IA.RoiMan.DeleteBtn_Click(sender, e);
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    case Keys.M:
                        IA.RoiMan.MeasureBtn_Click(sender, e);
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    case Keys.C:
                        if (!(MainForm.ActiveControl is TextBox &&
                            ((TextBox)MainForm.ActiveControl).SelectionLength > 0))
                            IA.RoiMan.CopyRois(sender, e);
                        break;
                    case Keys.X:
                        if (!(MainForm.ActiveControl is TextBox &&
                            ((TextBox)MainForm.ActiveControl).SelectionLength > 0))
                            IA.RoiMan.CutRois(sender, e);
                        break;
                    case Keys.V:
                        if (!(MainForm.ActiveControl is TextBox &&
                            ((TextBox)MainForm.ActiveControl).SelectionLength > 0))
                            IA.RoiMan.PasteRois(sender, e);
                        break;
                    default:
                        if (HotKays.CheckForKey(e.KeyCode))
                        {
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;

                }
            }
            else
                switch (e.KeyCode)
                {
                    case Keys.F2:
                        if (!IA.FileBrowser.Vbox.Focused && !IA.FileBrowser.TreeViewExp.Focused &&
                            IA.RoiMan.SelectedROIsList.Count > 0)
                        {
                            IA.RoiMan.RenameTb_Add(IA.RoiMan.SelectedROIsList[0]);
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                }
        }
       
        public void TimeTrackBar_Forward()
        {
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;
            if (!fi.loaded) return;
            if (fi.frame + 1 >= fi.sizeT) return;
            
            fi.frame = fi.frame + 1;
            IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);
            IA.ReloadImages();
                        
        }
        public void TimeTrackBar_Backward()
        {
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;
            if (!fi.loaded) return;
            if (fi.frame  <= 0) return;

            fi.frame = fi.frame - 1;
            IA.TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);
            
            IA.ReloadImages();
        }
        public void zTrackBar_Forward()
        {
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;
            if (!fi.loaded) return;
            if (fi.zValue + 1 >= fi.sizeZ) return;

            fi.zValue = fi.zValue + 1;
            IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);
            IA.ReloadImages();
        }
        public void zTrackBar_Backward()
        {
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { }
            if (fi == null) return;
            if (!fi.loaded) return;
            if (fi.zValue <= 0) return;

            fi.zValue = fi.zValue - 1;
            IA.TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);
            IA.ReloadImages();
        }
       
        private void Menu(Panel MainPanel)
        {
            Panel MenuPanel = new Panel();
            MenuPanel.Height = 25;
            MenuPanel.Dock = DockStyle.Top;
            MainPanel.Controls.Add(MenuPanel);
            //Start Menu
            MenuStrip StartMenu = new MenuStrip();           
            StartMenu.AutoSize = true;
            StartMenu.Dock = DockStyle.Fill;
            //StartMenu.BackColor = BackGroundColor;
            //StartMenu.ForeColor = ShriftColor;
            MenuPanel.Controls.Add(StartMenu);
            //File menu 
            { 
                ToolStripMenuItem FileToolStripMenuItem = new ToolStripMenuItem();
                FileToolStripMenuItem.Text = "File";
                //FileToolStripMenuItem.BackColor = BackGroundColor;
                //FileToolStripMenuItem.ForeColor = ShriftColor;
                FileToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                FileToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                StartMenu.Items.Add(FileToolStripMenuItem);
                
                NewToolStripMenuItem.Text = "New Results Extractor";
                //NewToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
                NewToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + N)";
                NewToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(NewToolStripMenuItem);
               
                OpenToolStripMenuItem.Text = "Open";
                //OpenToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;
                OpenToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + O)";
                OpenToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(OpenToolStripMenuItem);

                ToolStripSeparator Separator1 = new ToolStripSeparator();
                FileToolStripMenuItem.DropDownItems.Add(Separator1);
               
                SaveToolStripMenuItem.Text = "Save";
                SaveToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + S)";
                SaveToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(SaveToolStripMenuItem);
               
                SaveAsToolStripMenuItem.Text = "Save as";
                SaveAsToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + Shift + S)";
                SaveAsToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(SaveAsToolStripMenuItem);
                
                SaveAllToolStripMenuItem.Text = "Save All";
                SaveAllToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + Shift + A)";
                SaveAllToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(SaveAllToolStripMenuItem);
                
                ExportToolStripMenuItem.Text = "Export";
                ExportToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + E)";
                ExportToolStripMenuItem.ShowShortcutKeys = true;
                FileToolStripMenuItem.DropDownItems.Add(ExportToolStripMenuItem);
                
                ExportAllBtn.Text = "Auto Export";
                FileToolStripMenuItem.DropDownItems.Add(ExportAllBtn);

                ToolStripSeparator Separator2 = new ToolStripSeparator();
                FileToolStripMenuItem.DropDownItems.Add(Separator2);
                
                CloseToolStripMenuItem.Text = "Close";
                FileToolStripMenuItem.DropDownItems.Add(CloseToolStripMenuItem);
                
                CloseAllToolStripMenuItem.Text = "Close all";
                FileToolStripMenuItem.DropDownItems.Add(CloseAllToolStripMenuItem);

                ToolStripSeparator Separator3 = new ToolStripSeparator();
                FileToolStripMenuItem.DropDownItems.Add(Separator3);
                                
                ExitToolStripMenuItem.Text = "Exit";
                ExitToolStripMenuItem.Click += new EventHandler(CloseProgram1);
                FileToolStripMenuItem.DropDownItems.Add(ExitToolStripMenuItem);
            }
            //Edit menu
            {
                ToolStripMenuItem EditToolStripMenuItem = new ToolStripMenuItem();
                EditToolStripMenuItem.Text = "Edit";
                EditToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                EditToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                StartMenu.Items.Add(EditToolStripMenuItem);
                
                UndoToolStripMenuItem.Text = "Undo";
                UndoToolStripMenuItem.Enabled = false;
                UndoToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + Z)";
                SaveToolStripMenuItem.ShowShortcutKeys = true;
                EditToolStripMenuItem.DropDownItems.Add(UndoToolStripMenuItem);
                UndoToolStripMenuItem.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                   UnDoBtn.PerformClick();
                });

                RedoToolStripMenuItem.Text = "Redo";
                RedoToolStripMenuItem.Enabled = false;
                RedoToolStripMenuItem.ShortcutKeyDisplayString = "(Ctrl + Y)";
                RedoToolStripMenuItem.ShowShortcutKeys = true;
                EditToolStripMenuItem.DropDownItems.Add(RedoToolStripMenuItem);
                RedoToolStripMenuItem.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    ReDoBtn.PerformClick();
                });

                editMenu1 = new EditMenu(EditToolStripMenuItem);
            }
            //View menu
            {
                ToolStripMenuItem ViewToolStripMenuItem = new ToolStripMenuItem();
                ViewToolStripMenuItem.Text = "View";
                ViewToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                ViewToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                StartMenu.Items.Add(ViewToolStripMenuItem);
               
                ZoomInToolStripMenuItem.Text = "Zoom in (Ctrl +)";
                ViewToolStripMenuItem.DropDownItems.Add(ZoomInToolStripMenuItem);
                ZoomInToolStripMenuItem.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    if (ZoomValue.SelectedIndex < ZoomValue.Items.Count - 1)
                    {
                        ZoomValue.SelectedIndex++;
                    }
                });

                ZoomOutToolStripMenuItem.Text = "Zoom out (Ctrl -)";
                ViewToolStripMenuItem.DropDownItems.Add(ZoomOutToolStripMenuItem);
                ZoomOutToolStripMenuItem.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    if (ZoomValue.SelectedIndex > 0)
                    {
                        ZoomValue.SelectedIndex--;
                    }
                });

                //ToolStripSeparator Separator7 = new ToolStripSeparator();
                //ViewToolStripMenuItem.DropDownItems.Add(Separator7);
              
                //RotateLeftToolStripMenuItem.Text = "Rotate left";
                //ViewToolStripMenuItem.DropDownItems.Add(RotateLeftToolStripMenuItem);
               
                //RotateRightToolStripMenuItem.Text = "Rotate right";
                //ViewToolStripMenuItem.DropDownItems.Add(RotateRightToolStripMenuItem);
                
                //ToolStripSeparator Separator8 = new ToolStripSeparator();
                //ViewToolStripMenuItem.DropDownItems.Add(Separator8);
                
                //RestoreToolStripMenuItem.Text = "Restore";
                //ViewToolStripMenuItem.DropDownItems.Add(RestoreToolStripMenuItem);
            }
            //Window menu
            {
                ToolStripMenuItem WindowToolStripMenuItem = new ToolStripMenuItem();
                WindowToolStripMenuItem.Text = "Window";
                WindowToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                WindowToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                //StartMenu.Items.Add(WindowToolStripMenuItem);
            }
            //Settings menu
            {
                ToolStripMenuItem SettingsToolStripMenuItem = new ToolStripMenuItem();
                SettingsToolStripMenuItem.Text = "Settings";
                SettingsToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                SettingsToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                //StartMenu.Items.Add(SettingsToolStripMenuItem);
                                
                ThemeToolStripMenuItem.Text = "Theme";
                SettingsToolStripMenuItem.DropDownItems.Add(ThemeToolStripMenuItem);
            }
            //Developer menu
            {
                DeveloperToolStripMenuItem.Text = "Plugins";
                DeveloperToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                DeveloperToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                StartMenu.Items.Add(DeveloperToolStripMenuItem);
            }
            //Help menu
            {
                ToolStripMenuItem HelpToolStripMenuItem = new ToolStripMenuItem();
                HelpToolStripMenuItem.Text = "Help";
                HelpToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                HelpToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                StartMenu.Items.Add(HelpToolStripMenuItem);

                ToolStripMenuItem AboutToolStripMenuItem = new ToolStripMenuItem();
                AboutToolStripMenuItem.Text = "About";
                HelpToolStripMenuItem.DropDownItems.Add(AboutToolStripMenuItem);
                {
                    ToolStripMenuItem LicenseToolStripMenuItem = new ToolStripMenuItem();
                    LicenseToolStripMenuItem.Text = "License agreement";
                    AboutToolStripMenuItem.DropDownItems.Add(LicenseToolStripMenuItem);
                    LicenseToolStripMenuItem.Click += new EventHandler(License_Click);

                    ToolStripMenuItem LibrariesToolStripMenuItem = new ToolStripMenuItem();
                    LibrariesToolStripMenuItem.Text = "Used libraries";
                    AboutToolStripMenuItem.DropDownItems.Add(LibrariesToolStripMenuItem);
                    {
                        ToolStripMenuItem LibTifToolStripMenuItem = new ToolStripMenuItem();
                        LibTifToolStripMenuItem.Text = "LibTiff.Net";
                        LibrariesToolStripMenuItem.DropDownItems.Add(LibTifToolStripMenuItem);
                        LibTifToolStripMenuItem.Click += new EventHandler(LibTifLicense_Click);

                        ToolStripMenuItem BioFormatsToolStripMenuItem = new ToolStripMenuItem();
                        BioFormatsToolStripMenuItem.Text = "Bio-Formats";
                        LibrariesToolStripMenuItem.DropDownItems.Add(BioFormatsToolStripMenuItem);
                        BioFormatsToolStripMenuItem.Click += new EventHandler(BioFormatsLicense_Click);

                        ToolStripMenuItem ikvmToolStripMenuItem = new ToolStripMenuItem();
                        ikvmToolStripMenuItem.Text = "ikvm";
                        LibrariesToolStripMenuItem.DropDownItems.Add(ikvmToolStripMenuItem);
                        ikvmToolStripMenuItem.Click += new EventHandler(ikvmLicense_Click);

                        ToolStripMenuItem openTKToolStripMenuItem = new ToolStripMenuItem();
                        openTKToolStripMenuItem.Text = "OpenTK";
                        LibrariesToolStripMenuItem.DropDownItems.Add(openTKToolStripMenuItem);
                        openTKToolStripMenuItem.Click += new EventHandler(openTKLicense_Click);

                        ToolStripMenuItem NcalcToolStripMenuItem = new ToolStripMenuItem();
                        NcalcToolStripMenuItem.Text = "NCalc";
                        LibrariesToolStripMenuItem.DropDownItems.Add(NcalcToolStripMenuItem);
                        NcalcToolStripMenuItem.Click += new EventHandler(NcalcLicense_Click);

                        ToolStripMenuItem AccordToolStripMenuItem = new ToolStripMenuItem();
                        AccordToolStripMenuItem.Text = "Accord.NET";
                        LibrariesToolStripMenuItem.DropDownItems.Add(AccordToolStripMenuItem);
                        AccordToolStripMenuItem.Click += new EventHandler(AccordLicense_Click);

                        ToolStripMenuItem MathNetToolStripMenuItem = new ToolStripMenuItem();
                        MathNetToolStripMenuItem.Text = "Math.NET Numerics";
                        LibrariesToolStripMenuItem.DropDownItems.Add(MathNetToolStripMenuItem);
                        MathNetToolStripMenuItem.Click += new EventHandler(MathNET_NumericsLicense_Click);

                    }
                    ToolStripMenuItem SupportToolStripMenuItem = new ToolStripMenuItem();
                    SupportToolStripMenuItem.Text = "Support";
                    AboutToolStripMenuItem.DropDownItems.Add(SupportToolStripMenuItem);
                    SupportToolStripMenuItem.Click += new EventHandler(Support_Click);
                }

                ToolStripMenuItem TutorialsMenuItem = new ToolStripMenuItem();
                TutorialsMenuItem.Text = "Tutorials";
                TutorialsMenuItem.Click += Tutorials_Click;
                HelpToolStripMenuItem.DropDownItems.Add(TutorialsMenuItem);

                HotKeysToolStripMenuItem.Text = "Hot Keys";
                HelpToolStripMenuItem.DropDownItems.Add(HotKeysToolStripMenuItem);

                SmartBtnToolStripMenuItem.Text = "Smart Buttons";
                HelpToolStripMenuItem.DropDownItems.Add(SmartBtnToolStripMenuItem);

                
                ToolStripMenuItem CiteUsMenuItem = new ToolStripMenuItem();
                CiteUsMenuItem.Text = "Cite us";
                CiteUsMenuItem.Click += CiteUsMenuItem_Click;
                HelpToolStripMenuItem.DropDownItems.Add(CiteUsMenuItem);
            }
            //Account
            {
                AccountToolStripMenuItem.Image = Properties.Resources.accImage;
                AccountToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
                AccountToolStripMenuItem.DropDownOpened += new EventHandler(menuItem_Opened);
                AccountToolStripMenuItem.DropDownClosed += new EventHandler(menuItem_Closed);
                AccountToolStripMenuItem.Margin = new System.Windows.Forms.Padding(3, 1, 10, 2);
                StartMenu.Items.Add(AccountToolStripMenuItem);
                
                LogOutToolStripMenuItem.Text = "Log out";
                AccountToolStripMenuItem.DropDownItems.Add(LogOutToolStripMenuItem);

                ChangePassToolStripMenuItem.Text = "Change password";
                AccountToolStripMenuItem.DropDownItems.Add(ChangePassToolStripMenuItem);

                ExportAccToolStripMenuItem.Text = "Export settings";
                AccountToolStripMenuItem.DropDownItems.Add(ExportAccToolStripMenuItem);

                ImportAccToolStripMenuItem.Text = "Import settings";
                AccountToolStripMenuItem.DropDownItems.Add(ImportAccToolStripMenuItem);
            }
        }
        
        void menuItem_Opened(object sender, EventArgs e)
        {
            ToolStripMenuItem item1 = sender as ToolStripMenuItem;
            //item1.ForeColor = Color.Black;
        }
        void menuItem_Closed(object sender, EventArgs e)
        {
            ToolStripMenuItem item1 = sender as ToolStripMenuItem;
            //item1.ForeColor = ShriftColor;
        }
        void Support_Click(object sender, EventArgs e)
        {
            MessageBox.Show("e-mail: georgi_danovski@abv.bg");
        }
        void License_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.LicenseAgreementCT;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void Tutorials_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://dnarepair.bas.bg/software/CellTool/tutorials.html");
            }
            catch
            {
                
            }
        }
        
        void CiteUsMenuItem_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 600;
            msgForm.Width = 600;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "Cite us";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Rtf = Properties.Resources.Citation;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        private void link_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
        void AccordLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.AccordLicence;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void MathNET_NumericsLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.MathNET_NumericsLicense;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void NcalcLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.NcalcLicense;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void LibTifLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.LicenseAgreementLibTif;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void BioFormatsLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.BioFormats;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        void ikvmLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.ikvm;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }

        void openTKLicense_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();

            msgForm.Height = 300;
            msgForm.Width = 300;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "CellTool";

            RichTextBox rtb = new RichTextBox();
            rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.Text = Properties.Resources.OpenTK;

            msgForm.Controls.Add(rtb);

            // Linux change
            StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            StatusLabel.Text = "Ready";
        }
        private void TaskBar(Panel MainPanel)
        {
            // build panel
            Panel TaskPanel = new Panel();
            TaskPanel.Height = 26;
            TaskPanel.Dock = DockStyle.Top;
            TaskPanel.BackColor = TaskBtnColor;
            MainPanel.Controls.Add(TaskPanel);
            TaskPanel.BringToFront();
            // add task bar
            taskTS = new ToolStrip();
             taskTS.GripStyle = ToolStripGripStyle.Hidden;
            taskTS.Renderer = new MySR();
            {
                taskTS.BackColor = TaskBtnColor;
                taskTS.ForeColor = ShriftColor;
                taskTS.Dock = DockStyle.Top;
                taskTS.ImageScalingSize = new System.Drawing.Size(20, 20);
                TaskPanel.Controls.Add(taskTS);
            }
            //add buttons to taskBar
            {
                {
                    
                    UnDoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    UnDoBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    UnDoBtn.Text = "Undo (Ctrl + Z)";
                    UnDoBtn.BackColor = TaskBtnColor;
                    UnDoBtn.Image = Properties.Resources.UNDO_FIN;
                    taskTS.Items.Add(UnDoBtn);
                    UnDoBtn.EnabledChanged += new EventHandler(delegate (object o, EventArgs a)
                    {
                        UndoToolStripMenuItem.Enabled = UnDoBtn.Enabled;
                    });
                                        
                    ReDoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    ReDoBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    ReDoBtn.Text = "Redo (Ctrl + Y)";
                    ReDoBtn.BackColor = TaskBtnColor;
                    ReDoBtn.Image = Properties.Resources.REDO_FIN;
                    taskTS.Items.Add(ReDoBtn);
                    ReDoBtn.EnabledChanged += new EventHandler(delegate (object o, EventArgs a)
                    {
                        RedoToolStripMenuItem.Enabled = ReDoBtn.Enabled;
                    });
                }
                ToolStripSeparator Separator1 = new ToolStripSeparator();
                Separator1.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator1);
                {
                    
                    NewBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    NewBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    NewBtn.Text = "New Results Extractor (Ctrl + N)";
                    NewBtn.BackColor = TaskBtnColor;
                    NewBtn.Image = Properties.Resources.NewFile;
                    taskTS.Items.Add(NewBtn);
                    

                    OpenBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    OpenBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    OpenBtn.Text = "Open (Ctrl + O)";
                    OpenBtn.Image = Properties.Resources.openFile;
                    OpenBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(OpenBtn);
                }
                ToolStripSeparator Separator2 = new ToolStripSeparator();
                Separator2.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator2);
                {
                    
                    SaveBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    SaveBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    SaveBtn.Text = "Save (Ctrl + S)";
                    SaveBtn.BackColor = TaskBtnColor;
                    SaveBtn.Image = Properties.Resources.Save;
                    taskTS.Items.Add(SaveBtn);
                    
                    SaveAsBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    SaveAsBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    SaveAsBtn.Text = "Save as (Ctrl + Shift + S)";
                    SaveAsBtn.BackColor = TaskBtnColor;
                    SaveAsBtn.Image = Properties.Resources.SaveAs;
                    taskTS.Items.Add(SaveAsBtn);
                    
                    SaveAllBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    SaveAllBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    SaveAllBtn.Text = "Save all (Ctrl + Shift + A)";
                    SaveAllBtn.BackColor = TaskBtnColor;
                    SaveAllBtn.Image = Properties.Resources.SaveAll;
                    taskTS.Items.Add(SaveAllBtn);
                    
                    ExportBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    ExportBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    ExportBtn.Text = "Export (Ctrl + E)";
                    ExportBtn.BackColor = TaskBtnColor;
                    ExportBtn.Image = Properties.Resources.export;
                    taskTS.Items.Add(ExportBtn);
                    
                }
                ToolStripSeparator Separator3 = new ToolStripSeparator();
                Separator3.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator3);
                {
                    StaticBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    StaticBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    StaticBtn.Text = "Static ROI";
                    StaticBtn.Tag = "Static ROI";
                    StaticBtn.Image = DrawClicetBorder(Properties.Resources.Static_ROI);
                    StaticBtn.Click += new EventHandler(tracking_Static_Btn_click);
                    StaticBtn.BackColor = TaskBtnClickedColor;
                    taskTS.Items.Add(StaticBtn);
                  
                    TrackingBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    TrackingBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    TrackingBtn.Text = "Tracking ROI";
                    TrackingBtn.Tag = "Tracking ROI";
                    TrackingBtn.Image = Properties.Resources.Tracking_ROI_2;
                    TrackingBtn.Click += new EventHandler(tracking_Static_Btn_click);
                    TrackingBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(TrackingBtn);

                    ToolStripSeparator Separator4a = new ToolStripSeparator();
                    Separator4a.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                    taskTS.Items.Add(Separator4a);

                    DoubleRoiBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    DoubleRoiBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    DoubleRoiBtn.Text = "Stack ROI";
                    DoubleRoiBtn.Tag = "Stack ROI";
                    DoubleRoiBtn.Image = Properties.Resources.bulls_eye;
                    DoubleRoiBtn.Click += new EventHandler(DoubleRoiBtn_click);
                    DoubleRoiBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(DoubleRoiBtn);
                }
                ToolStripSeparator Separator4 = new ToolStripSeparator();
                Separator4.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator4);
                {
                    
                    RectangularBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    RectangularBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    RectangularBtn.Text = "Rectangular ROI";
                    RectangularBtn.Tag = "Rectangular ROI";
                    RectangularBtn.Image = DrawClicetBorder(Properties.Resources.Rectangle_1);
                    RectangularBtn.Click += new EventHandler(ShapeRoi_Change);
                    RectangularBtn.BackColor = TaskBtnClickedColor;
                    taskTS.Items.Add(RectangularBtn);
                    
                    OvalBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    OvalBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    OvalBtn.Text = "Oval ROI";
                    OvalBtn.Tag = "Oval ROI";
                    OvalBtn.Image = Properties.Resources.Circle;
                    OvalBtn.Click += new EventHandler(ShapeRoi_Change);
                    OvalBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(OvalBtn);
                                        
                    PolygonBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    PolygonBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    PolygonBtn.Text = "Polygonal ROI";
                    PolygonBtn.Tag = "Polygonal ROI";
                    PolygonBtn.Image = Properties.Resources.Polygon;
                    PolygonBtn.Click += new EventHandler(ShapeRoi_Change);
                    PolygonBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(PolygonBtn);

                    FreehandBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    FreehandBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    FreehandBtn.Text = "Freehand ROI";
                    FreehandBtn.Tag = "Freehand ROI";
                    FreehandBtn.Image = Properties.Resources.freeselection_1;
                    FreehandBtn.Click += new EventHandler(ShapeRoi_Change);
                    FreehandBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(FreehandBtn);

                    MagicWandBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    MagicWandBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                    MagicWandBtn.Text = "Magic wand ROI";
                    MagicWandBtn.Tag = "Magic wand ROI";
                    MagicWandBtn.Image = Properties.Resources.magic;
                    MagicWandBtn.Click += new EventHandler(ShapeRoi_Change);
                    MagicWandBtn.BackColor = TaskBtnColor;
                    taskTS.Items.Add(MagicWandBtn);
                }
                ToolStripSeparator Separator5 = new ToolStripSeparator();
                Separator5.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator5);
                //Zoom
                {
                    ToolStripLabel ZoomLabel = new ToolStripLabel();
                    ZoomLabel.Text = "Zoom:";
                    ZoomLabel.ForeColor = Color.Black;
                    taskTS.Items.Add(ZoomLabel);

                    ZoomValue.Items.AddRange(new string[] 
                        {6.25.ToString() +" %",12.5.ToString() +" %","25 %","50 %",
                        "100 %", "200 %", "300 %", "400 %", "500 %",
                        "600 %", "700 %", "800 %", "900 %", "1000 %" });
                    ZoomValue.DropDownStyle = ComboBoxStyle.DropDownList;
                    ZoomValue.SelectedIndex = 4;
                    ZoomValue.AutoSize = false;
                    ZoomValue.Width = 80;
                    taskTS.Items.Add(ZoomValue);
                }
                ToolStripSeparator Separator6 = new ToolStripSeparator();
                Separator6.Margin = new System.Windows.Forms.Padding(8, 1, 8, 2);
                taskTS.Items.Add(Separator6);
            }
        }
        
        private void ShapeRoi_Change(object sender, EventArgs e)
        {
            //Restore initial color of shape buttons
            {
                PolygonBtn.BackColor = TaskBtnColor;
                OvalBtn.BackColor = TaskBtnColor;
                RectangularBtn.BackColor = TaskBtnColor;
                FreehandBtn.BackColor = TaskBtnColor;
                MagicWandBtn.BackColor = TaskBtnColor;

                PolygonBtn.Image = Properties.Resources.Polygon;
                OvalBtn.Image = Properties.Resources.Circle;
                RectangularBtn.Image = Properties.Resources.Rectangle_1;
                FreehandBtn.Image = Properties.Resources.freeselection_1;
                MagicWandBtn.Image = Properties.Resources.magic;
            }
            //Set selected Color
            ToolStripButton btn = sender as ToolStripButton;
            btn.BackColor = TaskBtnClickedColor;
            btn.Image = DrawClicetBorder(btn.Image as Bitmap);
            //check wich is the type
            if (btn == RectangularBtn)
                IA.RoiMan.RoiShape = 0;
            else if (btn == OvalBtn)
                IA.RoiMan.RoiShape = 1;
            else if (btn == PolygonBtn)
                IA.RoiMan.RoiShape = 2;
            else if (btn == FreehandBtn)
                IA.RoiMan.RoiShape = 3;
            else if (btn == MagicWandBtn)
                IA.RoiMan.RoiShape = 4;

            IA.RoiMan.current = null;
            IA.ReloadImages();
        }
        
        private void tracking_Static_Btn_click(object sender, EventArgs e)
        {
            //Check is the button clicked
            ToolStripButton btn = sender as ToolStripButton;
            if(btn.BackColor == TaskBtnClickedColor)
            {
                return;
            }
            //change colors
            if (TrackingBtn.BackColor == TaskBtnClickedColor)//static
            {
                IA.RoiMan.RoiType = 0;
                TrackingBtn.BackColor = TaskBtnColor;
                StaticBtn.BackColor = TaskBtnClickedColor;
                TrackingBtn.Image = Properties.Resources.Tracking_ROI_2;
                StaticBtn.Image = DrawClicetBorder(Properties.Resources.Static_ROI);
            }
            else//tracking
            {
                IA.RoiMan.RoiType = 1;
                TrackingBtn.BackColor = TaskBtnClickedColor;
                StaticBtn.BackColor = TaskBtnColor;
                TrackingBtn.Image = DrawClicetBorder(Properties.Resources.Tracking_ROI_2);
                StaticBtn.Image = Properties.Resources.Static_ROI;
            }
            IA.RoiMan.current = null;
            IA.ReloadImages();
        }
        private Bitmap DrawClicetBorder(Bitmap source)
        {
            Bitmap bmp = new Bitmap(source);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawLine(new Pen(Color.FromArgb(50,50,50),2), new Point(0,0), new Point(0, bmp.Height));
                g.DrawLine(new Pen(Color.FromArgb(50, 50, 50), 2), new Point(0, 0), new Point(bmp.Width, 0));
            }

            return bmp;
        }
             
        private void DoubleRoiBtn_click(object sender, EventArgs e)
        {
            ToolStripButton btn = sender as ToolStripButton;
            if (btn.BackColor == TaskBtnClickedColor)
            {
                btn.BackColor = TaskBtnColor;
                IA.RoiMan.turnOnStackRoi = false;
                btn.Image = Properties.Resources.bulls_eye;
            }
            else
            {
                btn.BackColor = TaskBtnClickedColor;
                IA.RoiMan.turnOnStackRoi = true;
                btn.Image = DrawClicetBorder(Properties.Resources.bulls_eye);
            }

            IA.RoiMan.current = null;
            IA.ReloadImages();
        }
        private void StatusPanel()
        {
            //add Status Controlers
            Panel StatusPanel = new Panel();
            StatusPanel.Height = 30;
            StatusPanel.Dock = DockStyle.Bottom;
            StatusPanel.BackColor = BackGroundColor;
            MainPanel.Controls.Add(StatusPanel);
            StatusPanel.BringToFront();

            StatusStrip MainStatusBar = new StatusStrip();
            MainStatusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            MainStatusBar.BackColor = TitlePanelColor;
            MainStatusBar.ForeColor = ShriftColor;
            MainStatusBar.Dock = DockStyle.Bottom;
            StatusPanel.Controls.Add(MainStatusBar);
            StatusPanel.Height = MainStatusBar.Height;
            {
                
                StatusLabel.Text = "Ready";
                MainStatusBar.Items.Add(StatusLabel);
                                
                StatusProgressBar.Visible = false;
                MainStatusBar.Items.Add(StatusProgressBar);
                
                ToolStripStatusLabel AuthorLabel = new ToolStripStatusLabel();
                
                AuthorLabel.Alignment = ToolStripItemAlignment.Right;
                AuthorLabel.Text = "Copyright © <2018> Georgi Danovski";
                MainStatusBar.Items.Add(AuthorLabel);
            }
        }
        //Status Bar
        
        private void StatusLabel_TextChange(object sender, EventArgs e)
        {
            if (StatusLabel.Text == "Ready" | StatusLabel.Text == "Reading Tif Image...")
            {
                MainPanel.Cursor = Cursors.Default;
                StatusProgressBar.Visible = false;

                // Linux change
                MainForm.Enabled = true;
                MainForm.Focus();
            }
            else
            {
                MainPanel.Cursor = Cursors.WaitCursor;
                if (StatusProgressBar.Style != ProgressBarStyle.Marquee)
                {
                    StatusProgressBar.Minimum = 1;
                    StatusProgressBar.Maximum = 100;
                    StatusProgressBar.Step = 10;
                    StatusProgressBar.Value = 10;
                    StatusProgressBar.MarqueeAnimationSpeed = 30;
                    StatusProgressBar.Style = ProgressBarStyle.Marquee;
                }
                StatusProgressBar.Visible = true;
                MainForm.Refresh();
                MainForm.Update();
                
            }
        }
        private void StatusProgressBar_visibleChanged(object sender, EventArgs e)
        {
            if (StatusProgressBar.Visible == false) return;

            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 100;
            t.Tick += new EventHandler(delegate(object o, EventArgs a)
            {
                if (StatusProgressBar.Visible == false)
                {
                    t.Stop();
                    t.Dispose();
                }
                else
                {
                    //Application.DoEvents();
                }
            });

            if (StatusProgressBar.Visible == true) t.Start();

        }

    }
 }
