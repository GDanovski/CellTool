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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Cell_Tool_3
{
    public class MySR : ToolStripSystemRenderer
    {
        public MySR() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
    class CTFileBrowser
    {
        //Activ Account
        public int ActiveAccountIndex = 0;
        //Panel
        Panel MainPanel;
        public Panel DataSourcesPanel = new Panel();
        public int DataSourcesPanelWidth = 300;
        //OpenFile options
        public List<string> OpenList = new List<string>();
        public Label Openlabel = new Label();
        public Label renameLabel = new Label();

        public ToolStripStatusLabel StatusLabel = null;

        public Color BackGroundColor1 = Color.DimGray;
        public Color BackGround2Color1 = Color.FromArgb(255, 60, 60, 60);
        public Color ShriftColor1 = Color.White;
        public Color TitlePanelColor1 = Color.CornflowerBlue;
        public Color TaskBtnColor1;
        public Color TaskBtnClickColor1;

        //Resize panel

        private Panel ResizeTreeViewPanel = new Panel();
        private Boolean DataSourcesPanel_Resize = false;
        private Panel ResizePanel = new Panel();
        private int oldX;
        //Task bar
        public ToolStripButton RefreshBtn = new ToolStripButton();
        public ToolStripButton AddBtn = new ToolStripButton();
        public ToolStripButton NewBtn = new ToolStripButton();
        public ToolStripButton PropertiesBtn = new ToolStripButton();
        public ToolStripButton moveBtn = new ToolStripButton();
        public ToolStripButton DeleteBtn = new ToolStripButton();
        public ToolStripButton OpenBtn = new ToolStripButton();
        public ToolStripButton RenameBtn = new ToolStripButton();
        // Tree View
        public Panel TreeViewTitlePanel = new Panel();
        public TreeView TreeViewExp = new TreeView();
        //Virtual Box
        //public CheckedListBox Vbox = new CheckedListBox();
        public TreeView Vbox = new TreeView();
        public Panel VBoxTitlePanel = new Panel();
        public List<TreeNode> Vbox_TreenodesList = new List<TreeNode>(); // Index of the tree nodes and Vbox are the same
        //ToolTip
        private ToolTip TurnOnToolTip = new ToolTip();
        //Rename
        private TextBox RenameTb = new TextBox();
        //Formats list
        public List<string> Formats;
        //Search
        public TextBox searchTextbox = new TextBox();
        private Boolean searched = false;
        //Context menu
        ContextMenu TreeViewContextMenu = new ContextMenu();
        ContextMenu VboxContextMenu = new ContextMenu();
        TreeNode MoveNode = null;
        TreeNode MoveNodeCopyPaste = null;
        Boolean copyItem = false;
        Boolean cutItem = false;
        TreeNode doubleClickTarget = null;
        //Searched
        private int SearchCount = 0;
        //Drag and drop
        private bool DragDropEnter = false;
        private Boolean DragDropNode = false;
        public Panel OpenPanel = new Panel();
        public Panel DragDropPanel = new Panel();
        private Label DragDroplabel = new Label();
        private Boolean SorseFromVbox = false;
        TreeView DragDropSourceTree = null;

        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }

        public void Initialize(int ActiveAccountIndex1, Panel MainPanel1, Color BackGroundColor, Color BackGround2Color, Color ShriftColor, Color TitlePanelColor, Color TaskBtnColor, Color TaskBtnClickColor)
        {
            //MessageBox.Show(TitlePanelColor.R.ToString() + "\t" + TitlePanelColor.G.ToString() + "\t" + TitlePanelColor.B.ToString());
            ActiveAccountIndex = ActiveAccountIndex1;
            MainPanel = MainPanel1;
            //DragAndDrop
            MainPanel.Controls.Add(OpenPanel);
            OpenPanel.MouseMove += new MouseEventHandler(OpenPanel_MouseMove);
            OpenPanel.MouseUp += new MouseEventHandler(OpenPanel_MouseUp);
            OpenPanel.MouseLeave += new EventHandler(Panel_DragDrop_MouseLeave);

            DragDropPanel.Visible = false;
            //MainPanel.Controls.Add(DragDropPanel);
            //Save colors
            BackGroundColor1 = BackGroundColor;
            BackGround2Color1 = BackGround2Color;
            ShriftColor1 = ShriftColor;
            TitlePanelColor1 = TitlePanelColor;
            TaskBtnColor1 = TaskBtnColor;
            TaskBtnClickColor1 = TaskBtnClickColor;

            // Main Panel settings
            DataSourcesPanel.Dock = DockStyle.Left;
            DataSourcesPanel.BackColor = BackGround2Color;
            DataSourcesPanel.Width = 20;
            MainPanel.Controls.Add(DataSourcesPanel);
            DataSourcesPanel.BringToFront();

            //add Turn on/off button
            Button TurnOnBtn = new Button();
            TurnOnBtn.Tag = "Show/Hide Data sources";
            TurnOnBtn.BackColor = TaskBtnColor;
            TurnOnBtn.Text = "";
            TurnOnBtn.Dock = DockStyle.Right;

            TurnOnBtn.ForeColor = TaskBtnColor;
            TurnOnBtn.FlatStyle = FlatStyle.Flat;
            TurnOnBtn.TextImageRelation = TextImageRelation.Overlay;
            TurnOnBtn.FlatAppearance.BorderColor = TurnOnBtn.BackColor;
            //TurnOnBtn.Image = Properties.Resources.DataSourcesIcon;
            TurnOnBtn.Width = 10;
            DataSourcesPanel.Controls.Add(TurnOnBtn);
            //add tool tip to turn on/off button
            {
                TurnOnBtn.MouseHover += new EventHandler(Control_MouseOver);
                //Hide and show File Browser
                
                TurnOnBtn.Click += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var btn = (Control)o;
                    Properties.Settings settings = Properties.Settings.Default;

                    if (DataSourcesPanel.Width == 15)
                    {
                        DataSourcesPanel.Width = DataSourcesPanelWidth;
                        settings.DataSourcesPanelVisible[ActiveAccountIndex] = "y";
                    }
                    else
                    {
                        DataSourcesPanel.Width = 15;
                        settings.DataSourcesPanelVisible[ActiveAccountIndex] = "n";
                    }
                    settings.Save();
                });
            }
            //Add verticalTitle panel
            Panel verticalTitle = new Panel();
            verticalTitle.Dock = DockStyle.Right;
            verticalTitle.BackColor = BackGroundColor;
            verticalTitle.Width = 5;
            DataSourcesPanel.Controls.Add(verticalTitle);
            //Resize Panel
            verticalTitle.MouseMove += new MouseEventHandler(DataSourcesPanel_MouseMove);
            verticalTitle.MouseDown += new MouseEventHandler(DataSourcesPanel_MouseDown);
            verticalTitle.MouseUp += new MouseEventHandler(DataSourcesPanel_MouseUp);

            //Add Data Source title
            Panel DataSourceTitlePanel = new Panel();
            DataSourceTitlePanel.Dock = DockStyle.Top;
            DataSourceTitlePanel.BackColor = BackGroundColor;
            DataSourceTitlePanel.Height = 21;
            DataSourcesPanel.Controls.Add(DataSourceTitlePanel);
            DataSourceTitlePanel.BringToFront();

            Label DataSourceTitlelabel = new Label();
            DataSourceTitlelabel.ForeColor = ShriftColor;
            DataSourceTitlelabel.Width = 150;
            DataSourceTitlelabel.Text = "Data Sources";
            DataSourceTitlelabel.Location = new System.Drawing.Point(10, 5);
            DataSourceTitlePanel.Controls.Add(DataSourceTitlelabel);
            //Resize panel
            ResizePanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
            ResizePanel.Visible = false;
            ResizePanel.BackColor = Color.FromArgb(100, 10, 10, 10);
            ResizePanel.Width = 5;
            MainPanel.Controls.Add(ResizePanel);
            ResizePanel.BringToFront();

            //Task Panel
            // build panel
            Color TaskPanelColor = TaskBtnColor;
            Panel TaskPanel = new Panel();
            TaskPanel.Height = 30;
            TaskPanel.Dock = DockStyle.Top;
            TaskPanel.BackColor = TaskPanelColor;
            DataSourcesPanel.Controls.Add(TaskPanel);
            TaskPanel.BringToFront();
            // add task bar
            ToolStrip taskTS = new ToolStrip();
            taskTS.GripStyle = ToolStripGripStyle.Hidden;
            taskTS.Renderer = new MySR();
            {
                taskTS.BackColor = TaskPanelColor;
                taskTS.ForeColor = ShriftColor;
                taskTS.Dock = DockStyle.Top;
                taskTS.ImageScalingSize = new System.Drawing.Size(22, 22);
                TaskPanel.Controls.Add(taskTS);
            }
            //add buttons to taskBar

            {
                AddBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                AddBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                AddBtn.Text = "Add work directory";
                //AddBtn.BackColor = TaskPanelColor;
                AddBtn.Image = Properties.Resources.folder_add_icon;
                taskTS.Items.Add(AddBtn);
                AddBtn.Click += new EventHandler(AddBtn_click);

                NewBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                NewBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                NewBtn.Text = "New folder";
                //NewBtn.BackColor = TaskPanelColor;
                NewBtn.Image = Properties.Resources.newFolder;
                taskTS.Items.Add(NewBtn);
                NewBtn.Click += new EventHandler(NewFolderBtn_Click);
                /*                
                moveBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                moveBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                moveBtn.Text = "Move";
                //moveBtn.BackColor = TaskPanelColor;
                moveBtn.Image = Properties.Resources.move_folder;
                taskTS.Items.Add(moveBtn);
                 */
                RenameBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                RenameBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                RenameBtn.Text = "Rename";
                //RenameBtn.BackColor = TaskPanelColor;
                RenameBtn.Image = Properties.Resources.Rename_icon;
                taskTS.Items.Add(RenameBtn);
                RenameBtn.Click += new EventHandler(RenameBtn_Click);

                RefreshBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                RefreshBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                RefreshBtn.Text = "Refresh";
                //RefreshBtn.BackColor = TaskPanelColor;
                RefreshBtn.Image = Properties.Resources.refresh;
                taskTS.Items.Add(RefreshBtn);
                RefreshBtn.Click += new EventHandler(RefreshBtn_Click);

                PropertiesBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                PropertiesBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                PropertiesBtn.Text = "Properties";
                //PropertiesBtn.BackColor = TaskPanelColor;
                PropertiesBtn.Image = Properties.Resources.info;
                taskTS.Items.Add(PropertiesBtn);
                PropertiesBtn.Click += new EventHandler(PropertiesBtn_Click);

                DeleteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                DeleteBtn.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
                DeleteBtn.Text = "Delete";
                //DeleteBtn.BackColor = TaskPanelColor;
                DeleteBtn.Image = Properties.Resources.DeleteRed;
                taskTS.Items.Add(DeleteBtn);
                DeleteBtn.Click += new EventHandler(DeleteBtn_Click);
            }
            //add search bar
            {
                Panel SearchPanel = new Panel();
                SearchPanel.Height = 25;
                SearchPanel.Dock = DockStyle.Top;
                SearchPanel.BackColor = TaskPanelColor;
                DataSourcesPanel.Controls.Add(SearchPanel);
                SearchPanel.BringToFront();

                Panel SearchLeftPanel = new Panel();
                SearchLeftPanel.Width = 5;
                SearchLeftPanel.Dock = DockStyle.Left;
                SearchLeftPanel.BackColor = TaskPanelColor;
                SearchPanel.Controls.Add(SearchLeftPanel);

                Panel SearchBotPanel = new Panel();
                SearchBotPanel.Height = 5;
                SearchBotPanel.Dock = DockStyle.Bottom;
                SearchBotPanel.BackColor = TaskPanelColor;
                SearchPanel.Controls.Add(SearchBotPanel);

                Button searchButton = new Button();
                searchButton.Text = "";
                searchButton.Tag = "Search";
                searchButton.BackColor = TaskPanelColor;
                searchButton.FlatStyle = FlatStyle.Flat;
                searchButton.ForeColor = TaskPanelColor;
                searchButton.Dock = DockStyle.Right;
                searchButton.Width = 20;
                SearchPanel.Controls.Add(searchButton);
                searchButton.BringToFront();
                searchButton.Image = Properties.Resources.Search1;
                ToolTip TurnOnToolTip1 = new ToolTip();
                searchButton.MouseHover += new EventHandler(Control_MouseOver);
                searchButton.Click += new EventHandler(searchButton_Click);

                searchTextbox.ContextMenu = new ContextMenu();
                searchTextbox.Tag = "Search all";
                searchTextbox.Dock = DockStyle.Bottom;
                searchTextbox.ForeColor = TaskPanelColor;
                searchTextbox.Text = "Search all";
                SearchPanel.Controls.Add(searchTextbox);
                searchTextbox.BringToFront();
                searchTextbox.KeyDown += new KeyEventHandler(searchTextbox_keyDown);
                searchTextbox.MouseDown += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
                {
                    if (a.Button == MouseButtons.Right)
                    {

                        var Textbox1 = (Control)o;
                        if (Textbox1.Tag.ToString() == "Search all")
                        {
                            Textbox1.Tag = "Search in Virtual Box";
                        }
                        else if (Textbox1.Tag.ToString() == "Search in Virtual Box")
                        {
                            Textbox1.Tag = "Search in Folder";
                        }
                        else
                        {
                            Textbox1.Tag = "Search all";
                        }
                        if (Textbox1.Text == "Search all" | Textbox1.Text == "Search in Virtual Box" | Textbox1.Text == "Search in Folder")
                        {
                            Textbox1.Text = Textbox1.Tag.ToString();
                        }

                        TurnOnToolTip.SetToolTip(Textbox1, Textbox1.Tag.ToString() +
                            ". \nRight click to change");
                    }

                });
                searchTextbox.MouseHover += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var ctr = (Control)o;
                    TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString() + ". \nRight click to change");
                });
                searchTextbox.TextChanged += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var Textbox1 = (Control)o;
                    searchTextbox.ForeColor = Color.Black;
                    if (Textbox1.Text == Textbox1.Tag.ToString())
                    {
                        searchTextbox.ForeColor = TaskPanelColor;
                    }
                    else
                    {
                        searchTextbox.ForeColor = Color.Black;
                    }
                });
                searchTextbox.LostFocus += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var Textbox1 = (Control)o;
                    if (Textbox1.Text == "")
                    {
                        searchTextbox.ForeColor = TaskPanelColor;
                        Textbox1.Text = Textbox1.Tag as string;
                    }
                });
                searchTextbox.GotFocus += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var Textbox1 = (Control)o;
                    if (Textbox1.Text == Textbox1.Tag.ToString())
                    {
                        searchTextbox.ForeColor = Color.Black;
                        Textbox1.Text = "";
                    }
                });

                searchTextbox.TextChanged += new EventHandler(delegate (Object o, EventArgs a)
                {
                    restoreTreeViewnodesColor_event();
                });
            }
            // add TreeView and VirtualBox buttons
            {
                Panel BotPanel = new Panel();
                BotPanel.Dock = DockStyle.Bottom;
                BotPanel.BackColor = BackGround2Color;
                BotPanel.Height = 5;
                DataSourcesPanel.Controls.Add(BotPanel);
                BotPanel.BringToFront();

                TreeViewTitlePanel.Dock = DockStyle.Top;
                TreeViewTitlePanel.BackColor = TitlePanelColor;
                TreeViewTitlePanel.Height = 21;
                DataSourcesPanel.Controls.Add(TreeViewTitlePanel);
                TreeViewTitlePanel.BringToFront();
                TreeViewTitlePanel.Tag = "Show/Hide Directory Explorer";
                TreeViewTitlePanel.MouseHover += new EventHandler(Control_MouseOver);
                TreeViewTitlePanel.Click += new EventHandler(treeViewExp_Visible);
                TreeViewTitlePanel.MouseEnter += new EventHandler(Title_HighLight);
                TreeViewTitlePanel.MouseLeave += new EventHandler(Title_Normal);

                Label TreeViewTitlelabel = new Label();
                TreeViewTitlelabel.ForeColor = ShriftColor;
                TreeViewTitlelabel.Width = 150;
                TreeViewTitlelabel.Text = "Directory Explorer";
                TreeViewTitlelabel.Location = new System.Drawing.Point(10, 5);
                TreeViewTitlePanel.Controls.Add(TreeViewTitlelabel);
                TreeViewTitlelabel.Tag = "Show/Hide Directory Explorer";
                TreeViewTitlelabel.MouseHover += new EventHandler(Control_MouseOver);
                TreeViewTitlelabel.Click += new EventHandler(treeViewExp_Visible);
                TreeViewTitlelabel.MouseEnter += new EventHandler(Title_HighLight);
                TreeViewTitlelabel.MouseLeave += new EventHandler(Title_Normal);
                TreeViewExp.KeyDown += new KeyEventHandler(CtrKey_KeyDown);
                
                TreeViewExp.Dock = DockStyle.Top;
                TreeViewExp.ShowNodeToolTips = false;
                TreeViewExp.Height = 200;
                TreeViewExp.BorderStyle = BorderStyle.None;
                TreeViewExp.BackColor = BackGround2Color;
                TreeViewExp.ForeColor = ShriftColor;
                TreeViewExp.CheckBoxes = true;
                DataSourcesPanel.Controls.Add(TreeViewExp);
                TreeViewExp.BringToFront();
                TreeViewExp.ShowRootLines = false;
                TreeViewExp.ShowPlusMinus = false;

                TreeViewExp.NodeMouseDoubleClick += TreeViewExp_NodeDoubleClick;
              
                TreeViewExp.MouseLeave += new EventHandler(Panel_DragDrop_MouseLeave);
                TreeViewExp.MouseMove += new MouseEventHandler(TreeViewExp_MouseMove);
                TreeViewExp.MouseLeave += new EventHandler(TreeViewExp_MouseLeave);
                TreeViewExp.MouseWheel += new MouseEventHandler(TreeViewExp_MouseWheel);
                TreeViewExp.MouseMove += new MouseEventHandler(TreeView_DragDrop_MouseMove);
                TreeViewExp.MouseUp += new MouseEventHandler(TreeView_DragDrop_MouseUp);
                TreeViewExp.MouseDown += new MouseEventHandler(TreeView_DragDrop_MouseDown);
                TreeViewExp.MouseEnter += new EventHandler(TreeView_DragDrop_MouseEnter);
                TreeViewExp.AfterCheck += new TreeViewEventHandler(TreeNode_AfterCheck);

                ImageList il = TreeView_ImageList();
                TreeViewExp.ImageList = il;

                ResizeTreeViewPanel.Dock = DockStyle.Top;
                ResizeTreeViewPanel.BackColor = Color.Transparent;
                ResizeTreeViewPanel.Height = 5;
                DataSourcesPanel.Controls.Add(ResizeTreeViewPanel);
                ResizeTreeViewPanel.BringToFront();

                TreeViewExp.DockChanged += new EventHandler(delegate (Object o, EventArgs a)
                {
                    if (TreeViewExp.Dock == DockStyle.Fill & TreeViewExp.Visible == true)
                    {
                        ResizeTreeViewPanel.Visible = false;
                    }
                    else
                    {
                        ResizeTreeViewPanel.Visible = true;
                    }
                });
                TreeViewExp.VisibleChanged += new EventHandler(delegate (Object o, EventArgs a)
                {
                    if (TreeViewExp.Visible == true & TreeViewExp.Dock == DockStyle.Fill)
                    {
                        ResizeTreeViewPanel.Visible = false;
                    }
                    else
                    {
                        ResizeTreeViewPanel.Visible = true;
                    }
                });

                //Resize Panel
                ResizeTreeViewPanel.MouseMove += new MouseEventHandler(TreeViewResizePanel_MouseMove);
                ResizeTreeViewPanel.MouseDown += new MouseEventHandler(TreeViewResizePanel_MouseDown);
                ResizeTreeViewPanel.MouseUp += new MouseEventHandler(TreeViewResizePanel_MouseUp);

                VBoxTitlePanel.Dock = DockStyle.Top;
                VBoxTitlePanel.BackColor = TitlePanelColor;
                VBoxTitlePanel.Height = 21;
                DataSourcesPanel.Controls.Add(VBoxTitlePanel);
                VBoxTitlePanel.BringToFront();
                VBoxTitlePanel.Tag = "Show/Hide VirtualBox";
                VBoxTitlePanel.MouseHover += new EventHandler(Control_MouseOver);
                VBoxTitlePanel.Click += new EventHandler(VboxExp_Visible);
                VBoxTitlePanel.MouseEnter += new EventHandler(Title_HighLight);
                VBoxTitlePanel.MouseLeave += new EventHandler(Title_Normal);

                Label VBoxTitlelabel = new Label();
                VBoxTitlelabel.ForeColor = ShriftColor;
                VBoxTitlelabel.Width = 150;
                VBoxTitlelabel.Text = "Virtual Box";
                VBoxTitlelabel.Location = new System.Drawing.Point(10, 5);
                VBoxTitlePanel.Controls.Add(VBoxTitlelabel);
                VBoxTitlelabel.Tag = "Show/Hide VirtualBox";
                VBoxTitlelabel.MouseHover += new EventHandler(Control_MouseOver);
                VBoxTitlelabel.Click += new EventHandler(VboxExp_Visible);
                VBoxTitlelabel.MouseEnter += new EventHandler(Title_HighLight);
                VBoxTitlelabel.MouseLeave += new EventHandler(Title_Normal);

                Vbox.Dock = DockStyle.Fill;
                Vbox.ShowNodeToolTips = false;
                Vbox.Height = 200;
                Vbox.BorderStyle = BorderStyle.None;
                Vbox.BackColor = BackGround2Color;
                Vbox.ForeColor = ShriftColor;
                Vbox.CheckBoxes = true;
                DataSourcesPanel.Controls.Add(Vbox);
                Vbox.BringToFront();
                Vbox.ShowRootLines = false;
                Vbox.ShowPlusMinus = false;
                Vbox.MouseLeave += new EventHandler(Panel_DragDrop_MouseLeave);
                Vbox.MouseMove += new MouseEventHandler(TreeViewExp_MouseMove);
                Vbox.MouseLeave += new EventHandler(TreeViewExp_MouseLeave);
                Vbox.MouseWheel += new MouseEventHandler(TreeViewExp_MouseWheel);
                Vbox.AfterCheck += new TreeViewEventHandler(Vbox_AfterCheck);
                Vbox.MouseMove += new MouseEventHandler(TreeView_DragDrop_MouseMove);
                Vbox.MouseUp += new MouseEventHandler(TreeView_DragDrop_MouseUp);
                Vbox.MouseDown += new MouseEventHandler(TreeView_DragDrop_MouseDown);
                Vbox.MouseEnter += new EventHandler(TreeView_DragDrop_MouseEnter);
                Vbox.NodeMouseDoubleClick += Vbox_DoubleClick;

                Vbox.ImageList = il;
                //add all
                TreeNode n = new TreeNode();
                n.Text = "All";
                n.Tag = "All";
                n.ImageIndex = 0;
                n.SelectedImageIndex = 0;
                n.Checked = false;
                Vbox.Nodes.Add(n);

                TreeNode n1 = new TreeNode();
                n1.Text = "All";
                n1.Tag = "All";
                n1.Checked = true;
                Vbox_TreenodesList.Add(n1);
            }
            //TreeView context menu
            {
                MenuItem OpenMenuBtn = new MenuItem();
                OpenMenuBtn.Text = "Open";
                TreeViewContextMenu.MenuItems.Add(OpenMenuBtn);
                OpenMenuBtn.Click += new EventHandler(OpenMenuBtn_Click);

                MenuItem OpenCheckedMenuBtn = new MenuItem();
                OpenCheckedMenuBtn.Text = "Open checked";
                TreeViewContextMenu.MenuItems.Add(OpenCheckedMenuBtn);
                OpenCheckedMenuBtn.Click += new EventHandler(TreeViewExp_OpenChecked);

                MenuItem OpenSearchedMenuBtn = new MenuItem();
                OpenSearchedMenuBtn.Text = "Open searched";
                TreeViewContextMenu.MenuItems.Add(OpenSearchedMenuBtn);
                OpenSearchedMenuBtn.Click += new EventHandler(TreeViewExp_OpenSearched);

                MenuItem ExpandAllMenuBtn = new MenuItem();
                ExpandAllMenuBtn.Text = "Expand all";
                TreeViewContextMenu.MenuItems.Add(ExpandAllMenuBtn);
                ExpandAllMenuBtn.Click += new EventHandler(ExpandAllMenuBtn_Click);

                MenuItem CollapseAllMenuBtn = new MenuItem();
                CollapseAllMenuBtn.Text = "Collapse all";
                TreeViewContextMenu.MenuItems.Add(CollapseAllMenuBtn);
                CollapseAllMenuBtn.Click += new EventHandler(CollapseAllMenuBtn_Click);

                MenuItem OpenInNewWindowMenuBtn = new MenuItem();
                OpenInNewWindowMenuBtn.Text = "Open in new window";
                TreeViewContextMenu.MenuItems.Add(OpenInNewWindowMenuBtn);
                OpenInNewWindowMenuBtn.Click += new EventHandler(Explorer_Open);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem IncludeInVirtualBoxMenuBtn = new MenuItem();
                IncludeInVirtualBoxMenuBtn.Text = "Include in Virtual Box";
                TreeViewContextMenu.MenuItems.Add(IncludeInVirtualBoxMenuBtn);
                IncludeInVirtualBoxMenuBtn.Click += new EventHandler(IncludeInVirtualBoxMenuBtn_Click);

                MenuItem IncludeInVirtualAllSearchedBoxMenuBtn = new MenuItem();
                IncludeInVirtualAllSearchedBoxMenuBtn.Text = "Include Searched items in Virtual Box";
                TreeViewContextMenu.MenuItems.Add(IncludeInVirtualAllSearchedBoxMenuBtn);
                IncludeInVirtualAllSearchedBoxMenuBtn.Click += new EventHandler(TreeViewexp_checkAllSearched);

                MenuItem RefreshMenuBtn = new MenuItem();
                RefreshMenuBtn.Text = "Refresh";
                TreeViewContextMenu.MenuItems.Add(RefreshMenuBtn);
                RefreshMenuBtn.Click += new EventHandler(RefreshBtn_Click);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem cutMenuBtn = new MenuItem();
                cutMenuBtn.Text = "Cut";
                TreeViewContextMenu.MenuItems.Add(cutMenuBtn);
                cutMenuBtn.Click += new EventHandler(TreeNode_Cut);

                MenuItem copyMenuBtn = new MenuItem();
                copyMenuBtn.Text = "Copy";
                TreeViewContextMenu.MenuItems.Add(copyMenuBtn);
                copyMenuBtn.Click += new EventHandler(TreeNode_Copy);

                MenuItem PasteMenuBtn = new MenuItem();
                PasteMenuBtn.Text = "Paste";
                TreeViewContextMenu.MenuItems.Add(PasteMenuBtn);
                PasteMenuBtn.Click += new EventHandler(TreeNode_Paste);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem ReleaseMenuBtn = new MenuItem();
                ReleaseMenuBtn.Text = "Release";
                TreeViewContextMenu.MenuItems.Add(ReleaseMenuBtn);
                ReleaseMenuBtn.Click += new EventHandler(ReleaseBtn_Click);

                MenuItem DeleteMenuBtn = new MenuItem();
                DeleteMenuBtn.Text = "Delete";
                TreeViewContextMenu.MenuItems.Add(DeleteMenuBtn);
                DeleteMenuBtn.Click += new EventHandler(DeleteBtn_Click);

                MenuItem DeleteAllCheckedMenuBtn = new MenuItem();
                DeleteAllCheckedMenuBtn.Text = "Delete all checked";
                TreeViewContextMenu.MenuItems.Add(DeleteAllCheckedMenuBtn);
                DeleteAllCheckedMenuBtn.Click += new EventHandler(TreeViewexp_deleteAllChecked);

                MenuItem DeleteAllSearchedMenuBtn = new MenuItem();
                DeleteAllSearchedMenuBtn.Text = "Delete all searched";
                TreeViewContextMenu.MenuItems.Add(DeleteAllSearchedMenuBtn);
                DeleteAllSearchedMenuBtn.Click += new EventHandler(TreeViewexp_deleteAllSearched);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem RenameMenuBtn = new MenuItem();
                RenameMenuBtn.Text = "Rename";
                TreeViewContextMenu.MenuItems.Add(RenameMenuBtn);
                RenameMenuBtn.Click += new EventHandler(RenameBtn_Click);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem NewFolderMenuBtn = new MenuItem();
                NewFolderMenuBtn.Text = "New folder";
                TreeViewContextMenu.MenuItems.Add(NewFolderMenuBtn);
                NewFolderMenuBtn.Click += new EventHandler(NewFolderBtn_Click);

                TreeViewContextMenu.MenuItems.Add("-");

                MenuItem PropertiesMenuBtn = new MenuItem();
                PropertiesMenuBtn.Text = "Properties";
                TreeViewContextMenu.MenuItems.Add(PropertiesMenuBtn);
                PropertiesMenuBtn.Click += new EventHandler(PropertiesBtn_Click);

                TreeViewExp.MouseUp += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
                {
                    if (a.Button == MouseButtons.Right)
                    {
                        Point p = new System.Drawing.Point(a.X, a.Y);
                        TreeNode n = TreeViewExp.GetNodeAt(a.X, a.Y);
                        TreeViewExp.SelectedNode = n;
                        if ((n != null))
                        {
                            if (n.Tag.ToString() == "All" & ctr == Vbox)
                            {
                                return;
                            }
                            TreeViewContextMenu.Tag = n;

                            if (n.ImageIndex != 0)
                            {
                                OpenMenuBtn.Text = "Open";
                                ExpandAllMenuBtn.Enabled = false;
                                CollapseAllMenuBtn.Enabled = false;
                                RefreshMenuBtn.Enabled = false;
                            }
                            else if (n.IsExpanded == true)
                            {
                                OpenMenuBtn.Text = "Collapse";
                                ExpandAllMenuBtn.Enabled = true;
                                CollapseAllMenuBtn.Enabled = true;
                                RefreshMenuBtn.Enabled = true;
                            }
                            else
                            {
                                OpenMenuBtn.Text = "Expand";
                                CollapseAllMenuBtn.Enabled = true;
                                ExpandAllMenuBtn.Enabled = true;
                                RefreshMenuBtn.Enabled = true;
                            }

                            if (n.ImageIndex == 0)
                            {
                                NewFolderMenuBtn.Enabled = true;
                            }
                            else
                            {
                                NewFolderMenuBtn.Enabled = false;
                            }

                            if (n.Parent == null)
                            {
                                ReleaseMenuBtn.Enabled = true;
                            }
                            else
                            {
                                ReleaseMenuBtn.Enabled = false;
                            }

                            if (n.Checked == true)
                            {
                                IncludeInVirtualBoxMenuBtn.Text = "Exclude from Virtual Box";
                            }
                            else
                            {
                                IncludeInVirtualBoxMenuBtn.Text = "Include in Virtual Box";
                            }

                            if (n.Checked == true)
                            {
                                IncludeInVirtualAllSearchedBoxMenuBtn.Text = "Exclude searched items from Virtual Box";
                            }
                            else
                            {
                                IncludeInVirtualAllSearchedBoxMenuBtn.Text = "Include searched items from Virtual Box";
                            }


                            if (MoveNodeCopyPaste != null & (copyItem == true | cutItem == true))
                            {
                                PasteMenuBtn.Enabled = true;
                            }
                            else
                            {
                                PasteMenuBtn.Enabled = false;
                            }

                            if (n.BackColor == Color.Green)
                            {
                                IncludeInVirtualAllSearchedBoxMenuBtn.Enabled = true;
                            }
                            else
                            {
                                IncludeInVirtualAllSearchedBoxMenuBtn.Enabled = false;
                            }

                            if (searched == true & SearchCount > 0)
                            {
                                DeleteAllSearchedMenuBtn.Enabled = true;

                                //if (n.ImageIndex != 0)
                                {
                                    OpenSearchedMenuBtn.Enabled = true;
                                }

                            }
                            else
                            {

                                //if (n.ImageIndex != 0)
                                {
                                    OpenSearchedMenuBtn.Enabled = false;
                                }

                                DeleteAllSearchedMenuBtn.Enabled = false;
                            }

                            if (TreeNode_isItFullExpanded(TreeViewContextMenu.Tag as TreeNode) == true)
                            {
                                ExpandAllMenuBtn.Enabled = false;
                            }
                            else
                            {
                                ExpandAllMenuBtn.Enabled = true;
                            }

                            if (TreeNode_isItFullCollapsed(TreeViewContextMenu.Tag as TreeNode) == true)
                            {
                                CollapseAllMenuBtn.Enabled = false;
                            }
                            else
                            {
                                CollapseAllMenuBtn.Enabled = true;
                            }

                            if (TreeView_CheckForChecked() == true)
                            {

                                //if (n.ImageIndex != 0)
                                {
                                    OpenCheckedMenuBtn.Enabled = true;
                                }

                                DeleteAllCheckedMenuBtn.Enabled = true;
                            }
                            else
                            {

                                //if (n.ImageIndex != 0)
                                {
                                    OpenCheckedMenuBtn.Enabled = false;
                                }

                                DeleteAllCheckedMenuBtn.Enabled = false;
                            }

                            foreach (MenuItem mi in TreeViewContextMenu.MenuItems)
                            {
                                if (mi.Enabled == false)
                                {
                                    mi.Visible = false;
                                }
                                else
                                {
                                    mi.Visible = true;
                                }
                            }
                            //if (n.ImageIndex != 0)
                            {
                                OpenCheckedMenuBtn.Visible = true;
                                OpenSearchedMenuBtn.Visible = true;
                            }
                            TreeViewContextMenu.Show(TreeViewExp, p);
                        }

                    }
                });
            }
            //VboxContextMenu
            {
                MenuItem OpenMenuBtn = new MenuItem();
                OpenMenuBtn.Text = "Open";
                VboxContextMenu.MenuItems.Add(OpenMenuBtn);
                OpenMenuBtn.Click += new EventHandler(OpenVboxItem);

                MenuItem OpenCheckedMenuBtn = new MenuItem();
                OpenCheckedMenuBtn.Text = "Open checked";
                VboxContextMenu.MenuItems.Add(OpenCheckedMenuBtn);
                OpenCheckedMenuBtn.Click += new EventHandler(OpenVboxItem_Checked);

                MenuItem OpenSearchedMenuBtn = new MenuItem();
                OpenSearchedMenuBtn.Text = "Open searched";
                VboxContextMenu.MenuItems.Add(OpenSearchedMenuBtn);
                OpenSearchedMenuBtn.Click += new EventHandler(OpenVboxItem_Searched);

                VboxContextMenu.MenuItems.Add("-");

                MenuItem ExportMenuBtn = new MenuItem();
                ExportMenuBtn.Text = "Export";
                VboxContextMenu.MenuItems.Add(ExportMenuBtn);
                ExportMenuBtn.Click += new EventHandler(Vbox_Export);

                MenuItem ExportCheckedMenuBtn = new MenuItem();
                ExportCheckedMenuBtn.Text = "Export checked";
                VboxContextMenu.MenuItems.Add(ExportCheckedMenuBtn);
                ExportCheckedMenuBtn.Click += new EventHandler(Vbox_ExportChecked);

                MenuItem ExportSearchedMenuBtn = new MenuItem();
                ExportSearchedMenuBtn.Text = "Export searched";
                VboxContextMenu.MenuItems.Add(ExportSearchedMenuBtn);
                ExportSearchedMenuBtn.Click += new EventHandler(Vbox_ExportSearched);

                VboxContextMenu.MenuItems.Add("-");

                MenuItem ReleaseMenuBtn = new MenuItem();
                ReleaseMenuBtn.Text = "Release";
                VboxContextMenu.MenuItems.Add(ReleaseMenuBtn);
                ReleaseMenuBtn.Click += new EventHandler(Vbox_Releace);

                MenuItem ReleaseCheckedMenuBtn = new MenuItem();
                ReleaseCheckedMenuBtn.Text = "Release checked";
                VboxContextMenu.MenuItems.Add(ReleaseCheckedMenuBtn);
                ReleaseCheckedMenuBtn.Click += new EventHandler(Vbox_ReleaceChecked);

                MenuItem ReleaseSearchedMenuBtn = new MenuItem();
                ReleaseSearchedMenuBtn.Text = "Release searched";
                VboxContextMenu.MenuItems.Add(ReleaseSearchedMenuBtn);
                ReleaseSearchedMenuBtn.Click += new EventHandler(Vbox_ReleaceSearched);

                VboxContextMenu.MenuItems.Add("-");

                MenuItem PropertiesMenuBtn = new MenuItem();
                PropertiesMenuBtn.Text = "Properties";
                VboxContextMenu.MenuItems.Add(PropertiesMenuBtn);
                PropertiesMenuBtn.Click += new EventHandler(PropertiesBtn_Click);

                Vbox.MouseUp += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
                {
                    if (a.Button == MouseButtons.Right)
                    {
                        Point p = new System.Drawing.Point(a.X, a.Y);
                        TreeNode n = Vbox.GetNodeAt(a.X, a.Y);
                        if (n == null) { return; }
                        if (n.Tag.ToString() == "All")
                        {
                            return;
                        }
                        Vbox.SelectedNode = n;

                        ExportSearchedMenuBtn.Enabled = false;
                        ReleaseSearchedMenuBtn.Enabled = false;
                        OpenSearchedMenuBtn.Enabled = false;
                        foreach (TreeNode node in Vbox.Nodes)
                        {
                            if (node.BackColor == Color.Green)
                            {
                                ExportSearchedMenuBtn.Enabled = true;
                                ReleaseSearchedMenuBtn.Enabled = true;
                                OpenSearchedMenuBtn.Enabled = true;
                                break;
                            }
                        }
                        ExportCheckedMenuBtn.Enabled = false;
                        ReleaseCheckedMenuBtn.Enabled = false;
                        OpenCheckedMenuBtn.Enabled = false;
                        foreach (TreeNode node in Vbox.Nodes)
                        {
                            if (node.Checked == true)
                            {
                                ExportCheckedMenuBtn.Enabled = true;
                                ReleaseCheckedMenuBtn.Enabled = true;
                                OpenCheckedMenuBtn.Enabled = true;
                                break;
                            }
                        }

                        foreach (MenuItem mi in VboxContextMenu.MenuItems)
                        {
                            if (mi.Enabled == false)
                            {
                                mi.Visible = false;
                            }
                            else
                            {
                                mi.Visible = true;
                            }
                        }

                        if (n.ImageIndex != 0)
                        {
                            OpenCheckedMenuBtn.Visible = true;
                            OpenSearchedMenuBtn.Visible = true;
                        }
                        VboxContextMenu.Show(Vbox, p);
                    }
                });
            }
            DataSourcesPanel.Resize += DataSourcesPanel_resize;
        }
        private void DataSourcesPanel_resize(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            if ( int.Parse(settings.TreeViewSize[ActiveAccountIndex]) > DataSourcesPanel.Height - 150)
            {
                TreeViewExp.Height = DataSourcesPanel.Height - 150;
            }
            else
            {
                TreeViewExp.Height = int.Parse(settings.TreeViewSize[ActiveAccountIndex]);
            }
        }
        private void Vbox_DoubleClick(object sender, MouseEventArgs e)
        {
            // Get the node at the current mouse pointer location.
            TreeNode n = Vbox.GetNodeAt(e.X, e.Y);

            // Set a ToolTip only if the mouse pointer is actually paused on a node.
            if ((n != null))
            {
                if (n.ImageIndex != 0)
                {
                    Open_Event(n);
                }
            }
        }
        private void OpenPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & DragDropPanel.Visible == true)
            {
                if (DragDroplabel.Text != "       Open")
                {
                    DragDropEnter = true;
                    var p1 = (Control)sender;
                    DragDroplabel.Image = Properties.Resources.plus;
                    DragDropPanel.Tag = null;
                    DragDroplabel.Text = "       Open";
                    DragDropCursor();
                }

            }
            else
            {
                MainPanel.Cursor = Cursors.Default;
                DragDropNode = false;
                DragDropPanel.Visible = false;
                OpenPanel.Visible = false;
            }
        }
        private void OpenPanel_MouseUp(object sender, MouseEventArgs e)
        {
            OpenPanel.Visible = false;
            if (DragDropNode == true & DragDropPanel.Visible == true)
            {
                MainPanel.Cursor = Cursors.Default;
                DragDropPanel.Visible = false;
                if (MoveNode != null)
                {
                    //Open file
                    if (MoveNode.ImageIndex == 0)
                    {
                        if (MessageBox.Show("Do you want to open all tif images from " + MoveNode.Text + " ?",
                               "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            TreeViewExp_OpenSearched_Event(MoveNode);
                        }
                    }
                    else
                    {
                        if (DragDropSourceTree == TreeViewExp)
                        {
                            TreeNode_Open(MoveNode);
                        }
                        else
                        {
                            TreeNode_Open(Vbox_TreenodesList[Vbox.Nodes.IndexOf(MoveNode)]);
                        }
                    }

                }

                MoveNode = null;
            }
            DragDropNode = false;

        }
        bool doubleClicked = false;
        private void TreeView_DragDrop_MouseDown(object sender, MouseEventArgs e)
        {

            DragDropSourceTree = sender as TreeView;
            DragDropSourceTree.SelectedNode = DragDropSourceTree.GetNodeAt(e.X, e.Y);

            if (doubleClickTarget == DragDropSourceTree.GetNodeAt(e.X, e.Y))
                DragDropSourceTree.Cursor = Cursors.Default;

            if (doubleClickTarget == null)
                doubleClickTarget = DragDropSourceTree.GetNodeAt(e.X, e.Y);
           
            
            if (DragDropSourceTree.SelectedNode != null)
            {
                if (DragDropSourceTree == Vbox)
                {
                    if (DragDropSourceTree.SelectedNode.Tag.ToString() == "All")
                    {
                        return;
                    }

                    SorseFromVbox = true;
                }
                else
                {
                    if (DragDropSourceTree.Nodes.IndexOf(DragDropSourceTree.SelectedNode) > -1) return;
                    SorseFromVbox = false;
                }
                //set up the system
                {
                    DragDropPanel.Tag = null;
                    DragDropPanel.BackColor = Color.White;
                    DragDropPanel.ForeColor = Color.Black;
                    DragDropPanel.Controls.Add(DragDroplabel);
                    DragDroplabel.Location = new System.Drawing.Point(5, -1);
                    DragDropPanel.Height = 21;
                    DragDroplabel.ImageAlign = ContentAlignment.MiddleLeft;
                    DragDroplabel.TextAlign = ContentAlignment.MiddleLeft;
                    DragDroplabel.Text = "";
                    OpenPanel.Height = 2;
                    OpenPanel.BringToFront();

                    DragDropNode = true;

                }

            }
            
        }
        private void TreeView_DragDrop_MouseUp(object sender, MouseEventArgs e)
        {
            OpenPanel.Visible = false;
            doubleClickTarget = null;
            
            if (DragDropNode == true & DragDropPanel.Visible == true & doubleClicked == false)
            {
                MainPanel.Cursor = Cursors.Default;
                DragDropPanel.Visible = false;
                if (MoveNode != null)
                {
                    TreeNode node = DragDropSourceTree.GetNodeAt(e.X, e.Y);
                    if (DragDropSourceTree == Vbox & SorseFromVbox == false)
                    {
                        if (MoveNode.Checked == false)
                        {
                            MoveNode.Checked = true;
                            Vbox_Populate(MoveNode, true);
                        }
                    }
                    else if (node != null & DragDropSourceTree == TreeViewExp)
                    {
                        if (node != MoveNode & node.ImageIndex == 0)
                        {
                            if (MessageBox.Show("Do you want to copy " + MoveNode.Text + " to " + node.Text,
                               "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                PasteKey(node);
                            }
                        }
                    }

                }

                MoveNode = null;
            }

            DragDropNode = false;
            doubleClicked = false;

        }
        private void TreeView_DragDrop_MouseEnter(object sender, EventArgs e)
        {
            DragDropSourceTree = sender as TreeView;
        }

        private void Panel_DragDrop_MouseLeave(object sender, EventArgs e)
        {
            if (DragDropPanel.Visible == true & Control.MouseButtons == MouseButtons.Left)
            {
                DragDropPanel.Tag = null;
                if (DragDroplabel.Text != "       Copy")
                {
                    DragDroplabel.Image = Properties.Resources.EX;

                    DragDroplabel.Text = "       Copy";
                    DragDropCursor();
                }

            }
            else
            {
                MainPanel.Cursor = Cursors.Default;
                DragDropPanel.Visible = false;
                OpenPanel.Visible = false;
            }
            DragDropEnter = false;
        }

        public void DragDrop_Release()
        {
            if (DragDropEnter == false & Control.MouseButtons != MouseButtons.Left)
            {
                try
                {
                    MainPanel.Cursor = Cursors.Default;
                }
                catch { }
                DragDropNode = false;
                DragDropPanel.Visible = false;
                OpenPanel.Visible = false;
            }
        }
        private void DragDropCursor()
        {
            DragDroplabel.Width = TextRenderer.MeasureText(DragDroplabel.Text, DragDroplabel.Font).Width + 1;
            DragDropPanel.Width = TextRenderer.MeasureText(DragDroplabel.Text, DragDroplabel.Font).Width + 10;
            DragDropPanel.Width = DragDroplabel.Width + 20;

            Bitmap bmp = new Bitmap((DragDropPanel.Width + 25), (DragDropPanel.Height + 25));
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                DragDropPanel.DrawToBitmap(bmp, new Rectangle(6, 15, DragDropPanel.Width, DragDropPanel.Height));
                Cursors.Default.Draw(gr, new Rectangle(0, 0, 20, 20));
            }
            Bitmap Fbmp = new Bitmap(bmp.Width + bmp.Width, bmp.Height + bmp.Height);
            using (Graphics g = Graphics.FromImage(Fbmp))
            {
                g.DrawImage(bmp, bmp.Width, bmp.Height, bmp.Width, bmp.Height);
            }
            MainPanel.Cursor = new Cursor((Fbmp).GetHicon());
        }
        private void TreeView_DragDrop_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & DragDropNode == true & doubleClicked == false)
            {
                
                DragDropEnter = true;
                if (DragDropPanel.Visible == false)
                {
                    if (MoveNode != null)
                    {
                        DragDropPanel.Visible = true;
                        OpenPanel.Visible = true;
                    }
                    else {
                        MoveNode = DragDropSourceTree.GetNodeAt(e.X, e.Y);
                    }
                }

                //if (MoveNode == DragDropSourceTree.GetNodeAt(e.X, e.Y)) return;

                /*
                if (MoveNode == null)
                {
                    MainPanel.Cursor = Cursors.Default;
                    DragDropNode = false;
                    DragDropPanel.Visible = false;
                    OpenPanel.Visible = false;
                    return;
                }*/

                if (DragDropSourceTree == Vbox & MoveNode.Checked == false & SorseFromVbox == false)
                {
                    DragDropPanel.Tag = null;
                    if (DragDroplabel.Text != "       Add to Virtual Box")
                    {
                        DragDroplabel.Image = Properties.Resources.plus;
                        DragDroplabel.Text = "       Add to Virtual Box";

                        DragDropCursor();
                    }
                }
                else if (DragDropSourceTree == TreeViewExp)
                {
                    TreeNode node = DragDropSourceTree.GetNodeAt(e.X, e.Y);
                    if (node != null & node != doubleClickTarget)
                    {
                        if (node != MoveNode & node.ImageIndex == 0)
                        {
                            DragDropPanel.Tag = node;

                            if (DragDroplabel.Text != "       Copy to " + node.Text)
                            {
                                DragDroplabel.Text = "       Copy to " + node.Text;
                                DragDroplabel.Image = Properties.Resources.plus;
                                DragDropCursor();
                            }
                        }
                        else
                        {
                            DragDropPanel.Tag = null;

                            if (DragDroplabel.Text != "       Copy")
                            {
                                DragDroplabel.Image = Properties.Resources.EX;
                                DragDroplabel.Text = "       Copy";
                                DragDropCursor();
                            }
                        }
                    }
                    else if(node != null)
                    {
                            DragDropPanel.Tag = null;
                            DragDroplabel.Image = null;
                            DragDroplabel.Text = "";
                            MainPanel.Cursor = Cursors.Default;
                    }
                    else
                    {
                        DragDropPanel.Tag = null;

                        if (DragDroplabel.Text != "       Copy")
                        {
                            DragDroplabel.Image = Properties.Resources.EX;
                            DragDroplabel.Text = "       Copy";
                            DragDropCursor();
                        }
                    }
                }
                else
                {
                    DragDropPanel.Tag = null;

                    if (DragDroplabel.Text != "       Copy")
                    {
                        DragDroplabel.Text = "       Copy";
                        DragDroplabel.Image = Properties.Resources.EX;
                        DragDropCursor();
                    }
                }
            }
            else
            {
                MainPanel.Cursor = Cursors.Default;
                DragDropNode = false;
                DragDropPanel.Visible = false;
                OpenPanel.Visible = false;
                DragDrop_Release();
            }
           
        }
        private void Vbox_Export(object sender, EventArgs e)
        {
            if (Vbox.SelectedNode == null) { return; }
            TreeNode node = Vbox.SelectedNode;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            string formatMiniStr = Format_Extensions(node.Tag.ToString());
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;
            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.InitialDirectory = OSStringConverter.StringToDir(
                node.Tag.ToString().Substring(0, node.Tag.ToString().Length - (node.Text.Length + 1)));
            saveFileDialog1.FileName = node.Text;
            saveFileDialog1.OverwritePrompt = false;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = saveFileDialog1.FileName;

                if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == false)
                {
                    if (MessageBox.Show("Missing file: << " + node.Tag.ToString() + " >>",
                  "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    { return; }
                }
                OSFileManager.CopyFile(node.Tag.ToString(), dir,StatusLabel);

            }
        }
        private void Vbox_ExportChecked(object sender, EventArgs e)
        {
            if (Vbox.SelectedNode == null) { return; }
            TreeNode n = Vbox.SelectedNode;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Export checked items to:";
            fbd.SelectedPath = OSStringConverter.StringToDir(
                n.Tag.ToString().Substring(0, n.Tag.ToString().Length - (n.Text.Length + 1)));

            DialogResult result = fbd.ShowDialog();
            // OK button was pressed.
            if (result == DialogResult.OK)
            {
                int count = 0;
                foreach (TreeNode node in Vbox.Nodes)
                {
                    if (node.Checked == true && node != Vbox.Nodes[0])
                    {
                        string dir = OSStringConverter.GetWinString(fbd.SelectedPath) + "\\" + node.Text;
                        bool saveInd = true;
                        if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == false)
                        {
                            if (MessageBox.Show("Missing file: << " + node.Tag.ToString() + " >>",
                          "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                            { saveInd = false; }
                        }

                        if (saveInd == true && node.Tag.ToString() != dir)
                        {
                            OSFileManager.CopyFile(node.Tag.ToString(), dir,StatusLabel);
                            count++;
                        }
                    }
                }
                MessageBox.Show("Files exported: " + count.ToString());
            }

        }
        private void Vbox_ExportSearched(object sender, EventArgs e)
        {
            if (Vbox.SelectedNode == null) { return; }
            TreeNode n = Vbox.SelectedNode;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Export searched items to:";
            fbd.SelectedPath = OSStringConverter.StringToDir(
                n.Tag.ToString().Substring(0, n.Tag.ToString().Length - (n.Text.Length + 1)));

            DialogResult result = fbd.ShowDialog();
            // OK button was pressed.
            if (result == DialogResult.OK)
            {
                int count = 0;
                foreach (TreeNode node in Vbox.Nodes)
                {
                    if (node.BackColor == Color.Green)
                    {
                        string dir = OSStringConverter.GetWinString(fbd.SelectedPath) + "\\" + node.Text;
                        Boolean saveInd = true;
                        if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == false)
                        {
                            if (MessageBox.Show("Missing file: << " + node.Tag.ToString() + " >>",
                          "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                            { saveInd = false; }
                        }

                        if (saveInd == true & node.Tag.ToString() != dir)
                        {
                            OSFileManager.CopyFile(node.Tag.ToString(), dir,StatusLabel);
                            count++;
                        }
                    }
                }
                MessageBox.Show("Files exported: " + count.ToString());
            }

        }
        private void Vbox_Releace(object sender, EventArgs e)
        {
            if (Vbox.SelectedNode != null)
            {
                int index = Vbox.SelectedNode.Index;
                Vbox_TreenodesList[index].Checked = false;
                Treenode_UncheckParent(Vbox_TreenodesList[index]);
                Vbox_TreenodesList.RemoveAt(index);
                Vbox.Nodes.RemoveAt(index);
            }
        }
        private void Vbox_ReleaceSearched(object sender, EventArgs e)
        {
            for (int i = Vbox.Nodes.Count - 1; i > 0; i--)
            {
                if (Vbox.Nodes[i].BackColor == Color.Green)
                {
                    Vbox_TreenodesList[i].Checked = false;
                    Treenode_UncheckParent(Vbox_TreenodesList[i]);
                    Vbox_TreenodesList.RemoveAt(i);
                    Vbox.Nodes.RemoveAt(i);
                }
            }
        }
        private void Vbox_ReleaceChecked(object sender, EventArgs e)
        {
            for (int i = Vbox.Nodes.Count - 1; i > 0; i--)
            {
                if (Vbox.Nodes[i].Checked == true)
                {
                    Vbox_TreenodesList[i].Checked = false;
                    Treenode_UncheckParent(Vbox_TreenodesList[i]);
                    Vbox_TreenodesList.RemoveAt(i);
                    Vbox.Nodes.RemoveAt(i);
                }
            }
        }
        private void CtrKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Delete_Event();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (TreeViewExp.SelectedNode != null)
                {
                    Open_Event(TreeViewExp.SelectedNode);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }

            }
            if (e.KeyCode == Keys.F2)
            {
                if (TreeViewExp.SelectedNode != null)
                {
                    RenameTb_Add(TreeViewExp);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else
                {
                    MessageBox.Show("There is no selected item!");
                }
            }
            if (e.KeyCode == Keys.F5)
            {
                if (TreeViewExp.SelectedNode != null)
                {
                    TreeViewExp_populate(TreeViewExp.SelectedNode);
                    TreeViewExp.SelectedNode.Expand();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (Control.ModifierKeys == Keys.Control)
            {
                if (TreeViewExp.SelectedNode != null)
                {
                    if (e.KeyCode == Keys.X)
                    {
                        copyItem = false;
                        cutItem = true;
                        MoveNodeCopyPaste = TreeViewExp.SelectedNode;
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (e.KeyCode == Keys.C)
                    {
                        copyItem = true;
                        cutItem = false;
                        MoveNodeCopyPaste = TreeViewExp.SelectedNode;
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (e.KeyCode == Keys.V)
                    {
                        TreeNode targetNode = TreeViewExp.SelectedNode;
                        PasteKey(targetNode);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }

                }
            }

        }

        private void TreeNode_Copy(object sender, EventArgs e)
        {
            copyItem = true;
            cutItem = false;
            MoveNodeCopyPaste = TreeViewContextMenu.Tag as TreeNode;
        }
        private void TreeNode_Cut(object sender, EventArgs e)
        {
            copyItem = false;
            cutItem = true;
            MoveNodeCopyPaste = TreeViewContextMenu.Tag as TreeNode;
        }
        private void TreeNode_Paste(object sender, EventArgs e)
        {
            TreeNode targetNode = TreeViewContextMenu.Tag as TreeNode;
            PasteKey(targetNode);
        }

        private void PasteKey(TreeNode targetNode)
        {
            StatusLabel.Text = "Copy items...";
            Boolean FromDragAndDrop = DragDropNode;
            TreeNode sourceNode = MoveNode;
            if (DragDropNode == false)
            {
                sourceNode = MoveNodeCopyPaste;
            }

            TreeNode Parent = null;
            Boolean SaveSource = false;
            if (copyItem == true | DragDropNode == true)
            {
                SaveSource = true;
            }

            var bgw = new BackgroundWorker();

            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                if (sourceNode == null | targetNode == null)
                {
                    ((BackgroundWorker)o).ReportProgress(1);

                    return;
                }

                if (sourceNode.Parent != null)
                {
                    Parent = sourceNode.Parent;
                }

                string Dir = sourceNode.Tag.ToString();
                string NewDir = targetNode.Tag.ToString();
                if (Directory.Exists(OSStringConverter.StringToDir(NewDir)) != true)
                {
                    MessageBox.Show("Target directory is not existing!");
                    ((BackgroundWorker)o).ReportProgress(1);
                    return;
                }
                NewDir += "\\" + sourceNode.Text;
                if (NewDir == Dir)
                {
                    ((BackgroundWorker)o).ReportProgress(1);
                    return;
                }


                if (SaveSource == false)
                {
                    if (Directory.Exists(OSStringConverter.StringToDir(Dir)) == true)
                    {
                        OSFileManager.MoveDirectory(Dir, NewDir,StatusLabel);
                    }
                    else if (File.Exists(OSStringConverter.StringToDir(Dir)) == true)
                    {
                       OSFileManager.MoveFile(Dir, NewDir,StatusLabel);
                    }

                }
                else
                {
                    if (Directory.Exists(OSStringConverter.StringToDir(Dir)) == true)
                    {
                        OSFileManager.CopyDirectory(Dir, NewDir,StatusLabel);
                    }
                    else if (File.Exists(OSStringConverter.StringToDir(Dir)) == true)
                    {
                        OSFileManager.CopyFile(Dir, NewDir,StatusLabel);
                    }
                }

            ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    if (SaveSource == false)
                    {
                        if (sourceNode.Parent == null)
                        {

                            Properties.Settings settings = Properties.Settings.Default;
                            string newStr = "";
                            foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                            {
                                if (line == "@")
                                {
                                    newStr += "@";
                                }
                                else if (line != sourceNode.Tag.ToString())
                                {
                                    newStr += "\t" + line;
                                }
                            }

                            settings.TreeViewContent[ActiveAccountIndex] = newStr;
                            settings.Save();
                        }
                        Vbox_RemoveItem(sourceNode);
                        sourceNode.Remove();
                    }
                    TreeViewExp_populate(targetNode);
                    if (Parent != null)
                    {
                        TreeViewExp_populate(Parent);
                    }
                    targetNode.Expand();
                }
                else
                {
                    MessageBox.Show("There is no selected item!");
                }

                targetNode = null;
                Parent = null;
                if (FromDragAndDrop == true)
                {
                    MoveNode = null;
                    DragDropNode = false;
                }
                else
                {
                    copyItem = false;
                    cutItem = false;
                    MoveNodeCopyPaste = null;
                }

                StatusLabel.Text = "Ready";
            });
            bgw.WorkerReportsProgress = true;

            bgw.RunWorkerAsync();
            /*
            TreeNode sourceNode = MoveNode;
            if (sourceNode == null | targetNode == null)
            {
                return;
            }
            StatusLabel.Text = "Copy items...";
            TreeNode Parent = null;
            if (sourceNode.Parent != null)
            {
                Parent = sourceNode.Parent;
            }
           
            string Dir = sourceNode.Tag.ToString();
            string NewDir = targetNode.Tag.ToString();
            if (Directory.Exists(NewDir) != true)
            {
                MessageBox.Show("Target directory is not existing!");
                return;
            }
            NewDir += "\\" + sourceNode.Text;
            if (NewDir == Dir)
            {
                return;
            }
            if (Directory.Exists(NewDir) == true | File.Exists(NewDir) == true)
            {
                if (MessageBox.Show("Do you want to overwrite << " + NewDir + " >> ?",
                   "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                { return; }
            }

            Boolean SaveSource = false;
            if (copyItem == true)
            {
                SaveSource = true;
            }

            if (SaveSource == false)
            {
                Vbox_RemoveItem(sourceNode);
                sourceNode.Remove();
                if (Directory.Exists(Dir) == true)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(Dir, NewDir, true);
                }
                else if (File.Exists(Dir) == true)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(Dir, NewDir, true);
                }
            }
            else
            {
                if (Directory.Exists(Dir) == true)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(Dir, NewDir, true);
                }
                else if (File.Exists(Dir) == true)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(Dir, NewDir, true);
                }
            }

            copyItem = false;
            cutItem = false;
            MoveNode = null;

            TreeViewExp_populate(targetNode);
            if (SaveSource == false & Parent != null)
            {
                TreeViewExp_populate(Parent);
            }
            targetNode.Expand();
            StatusLabel.Text = "Ready";
            */
        }
        private void TreeNode_checkAllSearched(TreeNode n, Boolean ItemChecked)
        {
            if (n.Nodes.Count > 0)
            {

                for (int i = n.Nodes.Count - 1; i >= 0; i--)
                {
                    TreeNode node = n.Nodes[i];
                    if (node.BackColor == Color.Green & node.Checked != ItemChecked)
                    {
                        node.Checked = ItemChecked;
                        if (node.Checked == true)
                        {
                            if (node.ImageIndex != 0)
                            {
                                Vbox_TreenodesList.Add(node);
                                Vbox_AddNode(node);
                            }
                            CheckAllChildNodes(node, node.Checked);
                        }
                        else
                        {
                            Vbox_RemoveItem(node);
                        }
                    }
                    else
                    {
                        TreeNode_checkAllSearched(node, ItemChecked);
                    }

                }

            }
        }
        private void TreeViewexp_checkAllSearched(object sender, EventArgs e)
        {
            Boolean ItemChecked = true;
            if ((sender as MenuItem).Text == "Exclude searched items from Virtual Box")
            {
                ItemChecked = false;
            }

            if (TreeViewExp.Nodes.Count > 0)
            {
                for (int i = TreeViewExp.Nodes.Count - 1; i >= 0; i--)
                {
                    TreeNode node = TreeViewExp.Nodes[i];
                    if (node.BackColor == Color.Green & node.Checked != ItemChecked)
                    {
                        node.Checked = ItemChecked;
                        if (node.Checked == true)
                        {
                            if (node.Nodes.Count < 1)
                            {
                                TreeViewExp_populate(node);
                            }
                            node.Checked = ItemChecked;
                            if (node.ImageIndex != 0)
                            {
                                Vbox_TreenodesList.Add(node);
                                Vbox_AddNode(node);
                            }
                            CheckAllChildNodes(node, node.Checked);
                        }
                        else
                        {
                            Vbox_RemoveItem(node);
                        }

                    }
                    else
                    {
                        TreeNode_checkAllSearched(node, ItemChecked);
                    }

                }
            }
        }
        private void TreeNode_deleteAllSearched(TreeNode n)
        {
            if (n.Nodes.Count > 0)
            {
                for (int i = n.Nodes.Count - 1; i >= 0; i--)
                {
                    TreeNode node = n.Nodes[i];
                    if (node != null)
                    {
                        if (node.BackColor == Color.Green)
                        {
                            if (Directory.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                //Directory.Delete(TreeViewExp.SelectedNode.Tag.ToString(), true);
                                OSFileManager.DeleteDirectory(node.Tag.ToString(),StatusLabel);
                            }
                            else if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                OSFileManager.DeleteFile(node.Tag.ToString(),StatusLabel);
                                //File.Delete(TreeViewExp.SelectedNode.Tag.ToString());
                            }
                            Vbox_RemoveItem(node);
                            node.Remove();
                        }
                        else
                        {
                            TreeNode_deleteAllSearched(node);
                        }
                    }
                }

            }
        }
        private void TreeViewexp_deleteAllSearched(object sender, EventArgs e)
        {
            if (TreeViewExp.Nodes.Count > 0)
            {
                if (MessageBox.Show("Do you want to delete all searched items from Directory Explorer?",
                   "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    for (int i = TreeViewExp.Nodes.Count - 1; i >= 0; i--)
                    {
                        TreeNode node = TreeViewExp.Nodes[i];
                        if (node.BackColor == Color.Green)
                        {
                            if (Directory.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                if (node.Parent == null)
                                {
                                    Properties.Settings settings = Properties.Settings.Default;
                                    string newStr = "";
                                    foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                                    {
                                        if (line == "@")
                                        {
                                            newStr += "@";
                                        }
                                        else if (line != node.Tag.ToString())
                                        {
                                            newStr += "\t" + line;
                                        }
                                    }
                                    settings.TreeViewContent[ActiveAccountIndex] = newStr;
                                    settings.Save();

                                }
                                //Directory.Delete(TreeViewExp.SelectedNode.Tag.ToString(), true);
                                OSFileManager.DeleteDirectory(node.Tag.ToString(),StatusLabel);


                            }
                            else if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                OSFileManager.DeleteFile(node.Tag.ToString(),StatusLabel);
                                //File.Delete(TreeViewExp.SelectedNode.Tag.ToString());
                            }
                            Vbox_RemoveItem(node);
                            node.Remove();
                        }
                        else
                        {
                            TreeNode_deleteAllSearched(node);
                        }
                    }
                }
                searched = false;
            }
        }
        private void ReleaseBtn_Click(object sender, EventArgs e)
        {
            if (TreeViewExp.SelectedNode != null)
            {
                if (TreeViewExp.SelectedNode.Parent == null)
                {
                    Properties.Settings settings = Properties.Settings.Default;
                    string newStr = "";
                    foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                    {
                        if (line == "@")
                        {
                            newStr += "@";
                        }
                        else if (line != TreeViewExp.SelectedNode.Tag.ToString())
                        {
                            newStr += "\t" + line;
                        }
                    }

                    settings.TreeViewContent[ActiveAccountIndex] = newStr;
                    settings.Save();

                    Vbox_RemoveItem(TreeViewExp.SelectedNode);
                    TreeViewExp.SelectedNode.Remove();
                }
            }
        }
        private void TreeNode_deleteAllChecked(TreeNode n)
        {
            if (n.Nodes.Count > 0)
            {
                for (int i = n.Nodes.Count - 1; i >= 0; i--)
                {
                    TreeNode node = n.Nodes[i];
                    if (node != null)
                    {
                        if (node.Checked == true)
                        {
                            if (Directory.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                //Directory.Delete(TreeViewExp.SelectedNode.Tag.ToString(), true);
                                OSFileManager.DeleteDirectory(node.Tag.ToString(),StatusLabel);
                            }
                            else if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                OSFileManager.DeleteFile(node.Tag.ToString(),StatusLabel);
                                //File.Delete(TreeViewExp.SelectedNode.Tag.ToString());
                            }
                            Vbox_RemoveItem(node);
                            node.Remove();
                        }
                        else
                        {
                            TreeNode_deleteAllChecked(node);
                        }
                    }
                }

            }
        }
        private void TreeViewexp_deleteAllChecked(object sender, EventArgs e)
        {
            if (TreeViewExp.Nodes.Count > 0)
            {
                if (MessageBox.Show("Do you want to delete all checked items from Directory Explorer?",
                   "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    for (int i = TreeViewExp.Nodes.Count - 1; i >= 0; i--)
                    {
                        TreeNode node = TreeViewExp.Nodes[i];
                        if (node.Checked == true)
                        {
                            if (Directory.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                if (node.Parent == null)
                                {
                                    Properties.Settings settings = Properties.Settings.Default;
                                    string newStr = "";
                                    foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                                    {
                                        if (line == "@")
                                        {
                                            newStr += "@";
                                        }
                                        else if (line != node.Tag.ToString())
                                        {
                                            newStr += "\t" + line;
                                        }
                                    }
                                    settings.TreeViewContent[ActiveAccountIndex] = newStr;
                                    settings.Save();

                                }
                                //Directory.Delete(TreeViewExp.SelectedNode.Tag.ToString(), true);
                                OSFileManager.DeleteDirectory(node.Tag.ToString(),StatusLabel);


                            }
                            else if (File.Exists(OSStringConverter.StringToDir(node.Tag.ToString())) == true)
                            {
                                OSFileManager.DeleteFile(node.Tag.ToString(),StatusLabel);
                                //File.Delete(TreeViewExp.SelectedNode.Tag.ToString());
                            }
                            Vbox_RemoveItem(node);
                            node.Remove();
                        }
                        else
                        {
                            TreeNode_deleteAllChecked(node);
                        }
                    }
                }
            }
        }
        private Boolean TreeNode_CheckForChecked(TreeNode n)
        {
            if (n.Nodes.Count > 0)
            {
                foreach (TreeNode node in n.Nodes)
                {
                    if (node.Checked == true)
                    {
                        return true;
                    }
                    else
                    {
                        if (TreeNode_CheckForChecked(node) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (n.ImageIndex != 0)
            {
                if (n.Checked == true)
                {
                    return true;
                }
            }
            return false;

        }
        private Boolean TreeView_CheckForChecked()
        {
            if (TreeViewExp.Nodes.Count > 0)
            {
                foreach (TreeNode node in TreeViewExp.Nodes)
                {
                    if (node.Checked == true)
                    {
                        return true;
                    }
                    else
                    {
                        if (TreeNode_CheckForChecked(node) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private Boolean TreeNode_isItFullCollapsed(TreeNode node)
        {
            Boolean res = true;

            if (node.Nodes.Count > 0)
            {
                if (node.IsExpanded != true)
                {
                    foreach (TreeNode n in node.Nodes)
                    {
                        res = TreeNode_isItFullExpanded(n);
                    }
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }
        private Boolean TreeNode_isItFullExpanded(TreeNode node)
        {

            if (node.Nodes.Count > 0)
            {
                if (node.IsExpanded == true)
                {
                    foreach (TreeNode n in node.Nodes)
                    {

                        if (TreeNode_isItFullExpanded(n) == false)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (node.Parent == null)
            {
                return false;
            }
            return true;
        }

        private void IncludeInVirtualBoxMenuBtn_Click(object sender, EventArgs e)
        {
            TreeNode n = TreeViewContextMenu.Tag as TreeNode;
            if (n.Nodes.Count < 1 & n.ImageIndex == 0)
            {
                TreeViewExp_populate(n);
            }

            if (n.Checked == true)
            {
                n.Checked = false;

                Vbox_RemoveItem(n);
            }
            else
            {
                n.Checked = true;
                if (n.ImageIndex != 0)
                {
                    Vbox_TreenodesList.Add(n);
                    Vbox_AddNode(n);
                }
            }
            CheckAllChildNodes(n, n.Checked);
        }


        private void Explorer_Open(object sender, EventArgs e)
        {
            try
            {
                string dir = OSStringConverter.StringToDir((TreeViewContextMenu.Tag as TreeNode).Tag.ToString());
                System.Diagnostics.Process.Start("explorer.exe", dir);
            }
            catch
            {
                MessageBox.Show("This option is avaliable only for OS Windows!");
            }
        }
        private void ExpandAllMenuBtn_Click(object sender, EventArgs e)
        {
            MenuItem ctr = sender as MenuItem;
            TreeNode n = TreeViewContextMenu.Tag as TreeNode;

            if (n.Nodes.Count < 1)
            {
                TreeViewExp_populate(n);
            }
            n.ExpandAll();

        }

        private void CollapseAllMenuBtn_Click(object sender, EventArgs e)
        {
            MenuItem ctr = sender as MenuItem;
            TreeNode n = TreeViewContextMenu.Tag as TreeNode;

            TreeNode_colapseAll(n);
            n.Collapse();
        }
        private void TreeNode_colapseAll(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    TreeNode_colapseAll(n);
                }
                node.Collapse();
            }
        }
        private void Open_Event(TreeNode n)
        {
            
            if (n.ImageIndex == 0)
            {
                if (n.IsExpanded == true)
                {
                    n.Collapse();
                }
                else
                {
                    if (n.Nodes.Count < 1)
                    {
                        TreeViewExp_populate(n);
                    }
                    n.Expand();
                }
            }
            else
            {
                //OpenFile
                TreeNode_Open(n);
            }
        }
        private void OpenMenuBtn_Click(object sender, EventArgs e)
        {
            TreeNode n = TreeViewContextMenu.Tag as TreeNode;
            Open_Event(n);
        }


        private void restorePupNodeCololor(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    n.BackColor = BackGround2Color1;
                    restorePupNodeCololor(n);
                }
            }
        }
        private void restoreNodeColor(TreeView tv)
        {
            if (tv.Nodes.Count > 0)
            {
                foreach (TreeNode n in tv.Nodes)
                {
                    n.BackColor = BackGround2Color1;
                    restorePupNodeCololor(n);
                }
            }
        }
        public void restoreTreeViewnodesColor_event()
        {
            if (searched == true)
            {
                restoreNodeColor(Vbox);
                restoreNodeColor(TreeViewExp);
                searched = false;
            }
        }

        private void Search_CoreEvent(TreeView tv)
        {
            string str = searchTextbox.Text;
            foreach (TreeNode node in tv.Nodes)
            {
                if (node.Text.IndexOf(str) > -1)
                {
                    node.BackColor = Color.Green;
                    SearchCount++;
                }
                SearchInPup(node, str);
            }
            if (tv == Vbox)
            {
                for (int i = 0; i < tv.Nodes.Count; i++)
                {
                    if (tv.Nodes[i].BackColor == Color.Green)
                    {
                        Vbox_TreenodesList[i].BackColor = tv.Nodes[i].BackColor;
                        extendParent(Vbox_TreenodesList[i]);
                        SearchCount++;
                    }
                }
            }

        }
        private void extendParent(TreeNode node)
        {
            if (node.Parent != null)
            {
                TreeNode n = node.Parent;
                n.Expand();
                extendParent(n);
            }
        }
        private void SearchInPup(TreeNode node, string str)
        {
            if (node.Nodes.Count >= 1)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    if (n.Text.IndexOf(str) > -1)
                    {
                        SearchCount++;
                        n.BackColor = Color.Green;
                        extendParent(n);
                    }
                    SearchInPup(n, str);
                }
            }
        }
        private void searchTextbox_keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchTextBox_SearchEvent();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }
        private void searchButton_Click(object sender, EventArgs e)
        {
            SearchTextBox_SearchEvent();
        }

        private void SearchTextBox_SearchEvent()
        {
            SearchCount = 0;
            if (searchTextbox.Text == "Search all" |
                searchTextbox.Text == "Search in Virtual Box" |
                searchTextbox.Text == "Search in Folder" |
                searchTextbox.Text == "") { return; }
            searched = true;
            if (searchTextbox.Tag.ToString() == "Search all")
            {
                Search_CoreEvent(Vbox);
                Search_CoreEvent(TreeViewExp);
            }
            else if (searchTextbox.Tag.ToString() == "Search in Virtual Box")
            {
                Search_CoreEvent(Vbox);
            }
            else if (searchTextbox.Tag.ToString() == "Search in Folder")
            {

                if (TreeViewExp.SelectedNode != null)
                {
                    SearchInPup(TreeViewExp.SelectedNode, searchTextbox.Text);
                }
                else
                {
                    MessageBox.Show("There is no selected folder!");
                }
            }
            if (SearchCount == 0)
            {
                searchTextbox.ForeColor = Color.Red;
            }
            else
            {
                searchTextbox.ForeColor = Color.Black;
            }
        }
        private void Vbox_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag.ToString() == "All")
            {
                foreach (TreeNode n in Vbox.Nodes)
                {
                    if (n != e.Node)
                    {
                        n.Checked = e.Node.Checked;
                    }
                }
            }
        }
        private void NewFolderBtn_Click(object sender, EventArgs e)
        {
            if (TreeViewExp.SelectedNode != null & TreeViewExp.ContainsFocus == true)
            {
                string path = TreeViewExp.SelectedNode.Tag.ToString();
                bool cont = false;
                int count = 1;
                while (cont == false)
                {
                    if (Directory.Exists(OSStringConverter.StringToDir(path + "\\New Folder")) == false)
                    {
                        Directory.CreateDirectory(OSStringConverter.StringToDir(path + "\\New Folder"));
                        TreeNode n = TreeNod_AddFolderNode(TreeViewExp.SelectedNode, path + "\\New Folder");
                        TreeViewExp.SelectedNode = n;
                        cont = true;
                    }
                    else if (Directory.Exists(OSStringConverter.StringToDir(path + "\\New Folder" + count.ToString())) == false)
                    {
                        Directory.CreateDirectory(OSStringConverter.StringToDir(path + "\\New Folder" + count.ToString()));
                        TreeNode n = TreeNod_AddFolderNode(TreeViewExp.SelectedNode, path + "\\New Folder" + count.ToString());
                        TreeViewExp.SelectedNode = n;
                        cont = true;
                    }
                    else
                    {
                        count++;
                    }
                }

                RenameTb_Add(TreeViewExp);
            }
            else
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        private string Format_Extensions(string str)
        {
            string name = "";
            int length = str.Length;
            int count = length - 1;

            while (count >= 1 & str.Substring(count, 1) != ".")
            {
                count -= 1;
            }
            if (count >= 1)
            {
                name = str.Substring(count, (length - count));
            }
            return name;

        }
        private void RenameEvent(TreeView tv)
        {
            TreeNode node = RenameTb.Tag as TreeNode;
            string name = RenameTb.Text;
            if (RenameTb.Text == "")
            {
                RenameTb.Visible = false;
                MessageBox.Show("Name is not correct!");
                return;
            }
            //file extension
            string ext = Format_Extensions(name);
            if (ext == "" & node.ImageIndex != 0)
            {
                //copy old file extension
                string oldFormat = Format_Extensions(node.Tag.ToString());
                name += oldFormat;
            }
            ext = Format_Extensions(name);
            if (ext != "" & node.ImageIndex != 0)
            {
                //check is it correct file type
                Boolean okExtension = false;
                foreach (string format in Formats)
                {
                    if (format == ext)
                    {
                        okExtension = true;
                    }
                }
                if (okExtension == false)
                {
                    RenameTb.Visible = false;
                    MessageBox.Show("Unsupported file type!");
                    return;
                }
            }

            if(Directory.Exists(OSStringConverter.StringToDir(
                node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + name)))
            {
                RenameTb.Visible = false;
                MessageBox.Show("Directory exists!");
                return;
            }
            else if (File.Exists(OSStringConverter.StringToDir(
                node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + name)))
            {
                RenameTb.Visible = false;
                MessageBox.Show("File exists!");
                return;
            }

            if (node.Parent != null)
            {
                foreach (TreeNode n in node.Parent.Nodes)
                {
                    if (n.Text == name)
                    {
                        RenameTb.Visible = false;
                        MessageBox.Show("Selected name already exists!");
                        return;
                    }
                }
                RenameTb.Visible = false;
                TreeNode_Rename(node, name);
            }
            else
            {
                Properties.Settings settings = Properties.Settings.Default;
                string newStr = "";
                foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                {
                    if (line == node.Tag.ToString())
                    {
                        newStr += "\t" + node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + name;
                    }
                    else if (line == "@")
                    {
                        newStr += "@";
                    }
                    else
                    {
                        newStr += "\t" + line;
                    }
                }
                settings.TreeViewContent[ActiveAccountIndex] = newStr;
                settings.Save();
                foreach (TreeNode n in tv.Nodes)
                {

                    if (n.Text == name)
                    {
                        RenameTb.Visible = false;
                        MessageBox.Show("Selected name already exists!");
                        return;
                    }
                }
                TreeNode_Rename(node, name);
                RenameTb.Visible = false;

            }
        }
        private void TreeNode_AllPup(TreeNode node)
        {
            if (node.Nodes.Count >= 1)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    n.Tag = node.Tag.ToString() + "\\" + n.Text;
                    TreeNode_AllPup(n);
                }
            }
        }
        private void TreeNode_Rename(TreeNode node, string newName)
        {
            if (node.Tag.ToString() != node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + newName)
                if (node.ImageIndex == 0)
                {
                    try
                    {
                        Directory.Move(node.Tag.ToString(), node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + newName);
                    }
                    catch
                    {
                        MessageBox.Show("Directory is not available for raname now. Retry later!");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        File.Move(node.Tag.ToString(), node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + newName);
                    }
                    catch
                    {
                        MessageBox.Show("File is not available for raname now. Retry later!");
                        return;
                    }
                }
            node.Tag = node.Tag.ToString().Substring(0, node.Tag.ToString().Length - node.Text.Length) + newName;
            node.Text = newName;
            TreeNode_AllPup(node);
            Vbox_RefreshDir();
        }
        private void RenameTb_Add(TreeView tv)
        {
            TextBox tb = new TextBox();
            RenameTb = tb;
            tb.Tag = tv.SelectedNode;
            tb.Text = tv.SelectedNode.Text;
            tb.Location = new System.Drawing.Point(tv.SelectedNode.Bounds.X, tv.SelectedNode.Bounds.Y - 2);
            tb.Width = tv.SelectedNode.Bounds.Width;
            tb.Height = tv.SelectedNode.Bounds.Height;
            tb.Visible = true;
            tv.Controls.Add(tb);
            tb.Focus();
            if (!string.IsNullOrEmpty(tb.Text))
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
                if(tb.Visible)
                    RenameEvent(tv);

                renameLabel.Text = "y";
                renameLabel.Text = "";

                tb.Visible = false;
            });
            tb.KeyDown += new KeyEventHandler(delegate (Object o, KeyEventArgs a)
            {
                if (a.KeyCode == Keys.Enter)
                {
                    tb.Visible = false;
                    /*
                    RenameEvent(tv);
                    renameLabel.Text = "y";
                    renameLabel.Text = "";
                    */
                    a.Handled = true;
                    a.SuppressKeyPress = true;
                }

            });
            tv.AfterSelect += new TreeViewEventHandler(delegate (Object o, TreeViewEventArgs a)
            {
                tb.Visible = false;
            });
            tv.MouseWheel += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
            {
                tb.Visible = false;
            });
        }
        private void RenameBtn_Click(object sender, EventArgs e)
        {
            if (TreeViewExp.SelectedNode != null & TreeViewExp.ContainsFocus == true)
            {
                RenameTb_Add(TreeViewExp);
            }
            else
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            if (TreeViewExp.SelectedNode != null & TreeViewExp.ContainsFocus == true)
            {
                StatusLabel.Text = "Refreshing...";
                TreeViewExp.SuspendLayout();

                TreeViewExp_populate(TreeViewExp.SelectedNode);
                Refresh_AfterSave();
                TreeViewExp.SelectedNode.Expand();

                TreeViewExp.ResumeLayout();
                StatusLabel.Text = "Ready";
            }
            else
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        private void PropertiesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (TreeViewExp.SelectedNode != null & TreeViewExp.ContainsFocus == true)
                {
                    ShowFileProperties(OSStringConverter.StringToDir(TreeViewExp.SelectedNode.Tag.ToString()));
                }
                else if (Vbox.SelectedNode != null & Vbox.ContainsFocus == true & Vbox.SelectedNode.Tag.ToString() != "All")
                {
                    ShowFileProperties(OSStringConverter.StringToDir(Vbox.SelectedNode.Tag.ToString()));
                }
                else
                {
                    MessageBox.Show("There is no selected item!");
                }
            }
            catch
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        //FileProperties
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }
        //

        private void Title_HighLight(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            if (ctr is Label)
            {
                if (ctr.Text == "Directory Explorer")
                {
                    //TreeViewTitlePanel.BackColor = Color.LightSkyBlue;
                    int R = TreeViewTitlePanel.BackColor.R;
                    int G = TreeViewTitlePanel.BackColor.G;
                    int B = TreeViewTitlePanel.BackColor.B;
                    if (R + 40 <= 255) { R += 40; } else { R = 255; }
                    if (G + 40 <= 255) { G += 40; } else { G = 255; }
                    if (B + 40 <= 255) { B += 40; } else { B = 255; }
                    TreeViewTitlePanel.BackColor = Color.FromArgb(255, R, G, B);
                }
                else
                {
                    //VBoxTitlePanel.BackColor = Color.LightSkyBlue;
                    int R = VBoxTitlePanel.BackColor.R;
                    int G = VBoxTitlePanel.BackColor.G;
                    int B = VBoxTitlePanel.BackColor.B;
                    if (R + 40 <= 255) { R += 40; } else { R = 255; }
                    if (G + 40 <= 255) { G += 40; } else { G = 255; }
                    if (B + 40 <= 255) { B += 40; } else { B = 255; }
                    VBoxTitlePanel.BackColor = Color.FromArgb(255, R, G, B);
                }

            }
            else
            {
                //ctr.BackColor = Color.LightSkyBlue;
                int R = ctr.BackColor.R;
                int G = ctr.BackColor.G;
                int B = ctr.BackColor.B;
                if (R + 40 <= 255) { R += 40; } else { R = 255; }
                if (G + 40 <= 255) { G += 40; } else { G = 255; }
                if (B + 40 <= 255) { B += 40; } else { B = 255; }
                ctr.BackColor = Color.FromArgb(255, R, G, B);
            }
        }
        private void Title_Normal(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            if (ctr is Label)
            {
                if (ctr.Text == "Directory Explorer")
                {
                    TreeViewTitlePanel.BackColor = TitlePanelColor1;
                }
                else
                {
                    VBoxTitlePanel.BackColor = TitlePanelColor1;
                }

            }
            else { ctr.BackColor = TitlePanelColor1; }
        }
        private void treeViewExp_Visible(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            if (TreeViewExp.Visible == true)
            {
                TreeViewExp.Visible = false;
                VBoxTitlePanel.Dock = DockStyle.Top;
                settings.TreeViewVisible[ActiveAccountIndex] = "n";
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
            }
            else
            {
                TreeViewExp.Visible = true;
                if (Vbox.Visible == false)
                {
                    VBoxTitlePanel.Dock = DockStyle.Bottom;
                }
                settings.TreeViewVisible[ActiveAccountIndex] = "y";
            }
            settings.Save();
        }
        private void VboxExp_Visible(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (Vbox.Visible == true)
            {
                Vbox.Visible = false;
                if (TreeViewExp.Visible == true)
                {
                    VBoxTitlePanel.Dock = DockStyle.Bottom;
                }
                TreeViewExp.Dock = DockStyle.Fill;
                ResizeTreeViewPanel.BringToFront();
                VBoxTitlePanel.BringToFront();
                TreeViewExp.BringToFront();
                settings.VBoxVisible[ActiveAccountIndex] = "n";
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
            }
            else
            {
                Vbox.Visible = true;
                VBoxTitlePanel.Dock = DockStyle.Top;
                TreeViewExp.Dock = DockStyle.Top;
                ResizeTreeViewPanel.BringToFront();
                VBoxTitlePanel.BringToFront();
                Vbox.BringToFront();
                settings.VBoxVisible[ActiveAccountIndex] = "y";
            }
            settings.Save();

        }
        private void DataSourcesPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (DataSourcesPanel_Resize == true)
            {
                int razlika = ResizePanel.Location.X - (oldX - e.X);
                {
                    if (razlika < 600 & razlika > 60)
                    {
                        oldX = e.X;
                        ResizePanel.Location = new System.Drawing.Point(razlika, DataSourcesPanel.Location.Y);
                    }
                }

            }
            else {

                if (DataSourcesPanel.Width >= 20)
                {
                    pnl.Cursor = Cursors.SizeWE;
                }
                else
                {
                    pnl.Cursor = Cursors.Default;
                }
            }
        }
        private void DataSourcesPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (DataSourcesPanel.Width > 25)
            {
                DataSourcesPanel_Resize = true;
                ResizePanel.Location = new Point(pnl.Location.X, DataSourcesPanel.Location.Y);
                oldX = e.X;
                ResizePanel.Width = 5;
                ResizePanel.Height = DataSourcesPanel.Height;
                ResizePanel.BringToFront();
                ResizePanel.Visible = true;
            }
            else
            {
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
            }
        }
        private void DataSourcesPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (DataSourcesPanel_Resize == true)
            {
                DataSourcesPanel.Width = ResizePanel.Location.X + ResizePanel.Width;

                if (DataSourcesPanel.Width < 60)
                {
                    DataSourcesPanel.Width = 60 + ResizePanel.Width;
                }
                else if (DataSourcesPanel.Width > 600)
                {
                    DataSourcesPanel.Width = 600 + ResizePanel.Width;
                }
                DataSourcesPanelWidth = DataSourcesPanel.Width;
                Properties.Settings settings = Properties.Settings.Default;
                settings.DataSourcesPanelValues[ActiveAccountIndex] = Convert.ToString(DataSourcesPanelWidth);
                settings.Save();

                Panel pnl = sender as Panel;
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
                pnl.Cursor = Cursors.Default;
            }
        }
        /////
        private void TreeViewResizePanel_MouseMove(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (DataSourcesPanel_Resize == true & TreeViewExp.Visible == true & Vbox.Visible == true)
            {
                int razlika = ResizePanel.Location.Y - (oldX - e.Y);
                {
                    if (razlika < DataSourcesPanel.Height + DataSourcesPanel.Location.Y - 50 &
                        razlika > DataSourcesPanel.Location.Y + TreeViewExp.Location.Y + 23)
                    {
                        oldX = e.Y;
                        ResizePanel.Location = new System.Drawing.Point(0, razlika);
                    }
                }

            }
            else if (DataSourcesPanel.Width != 20 & TreeViewExp.Visible == true & Vbox.Visible == true)
            {
                pnl.Cursor = Cursors.SizeNS;
                ResizePanel.Visible = false;
            }
            else
            {
                pnl.Cursor = Cursors.Default;
                ResizePanel.Visible = false;
            }

        }
        private void TreeViewResizePanel_MouseDown(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (DataSourcesPanel.Width > 25 & TreeViewExp.Visible == true & Vbox.Visible == true)
            {
                DataSourcesPanel_Resize = true;
                ResizePanel.Location = new Point(0, pnl.Location.Y + DataSourcesPanel.Location.Y);
                oldX = e.Y;
                ResizePanel.Height = 5;
                ResizePanel.Width = TreeViewExp.Width;
                ResizePanel.Visible = true;
            }
            else
            {
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
            }
        }
        private void TreeViewResizePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (DataSourcesPanel_Resize == true)
            {
                TreeViewExp.Height = ResizePanel.Location.Y - DataSourcesPanel.Location.Y - TreeViewExp.Location.Y;

                if (TreeViewExp.Height < 23)
                {
                    TreeViewExp.Height = 23;
                }
                else if (TreeViewExp.Height > DataSourcesPanel.Height - 50)
                {
                    TreeViewExp.Height = DataSourcesPanel.Height - 50;
                }

                Properties.Settings settings = Properties.Settings.Default;
                settings.TreeViewSize[ActiveAccountIndex] = Convert.ToString(TreeViewExp.Height);
                settings.Save();

                Panel pnl = sender as Panel;
                ResizePanel.Visible = false;
                DataSourcesPanel_Resize = false;
                pnl.Cursor = Cursors.Default;
            }
        }
        //Tree View
        public void TreeViewExp_load(string str)
        {
            TreeViewExp.Nodes.Clear();
            foreach (string line in str.Split(new[] { "\t" }, StringSplitOptions.None))
            {
                if (line != "@")
                {
                    try
                    {
                        TreeViewExp_AddFolderNode(line);
                    }
                    catch { }
                }
            }
        }
        private string FileNameFromDir(string str)
        {
            int length = str.Length;
            int count = length - 1;
            while (count >= 0 & str.Substring(count, 1) != "\\")
            {
                count -= 1;
            }
            count += 1;
            string name = str.Substring(count, (length - count));

            return name;
        }
        private string FileUpperDirFromDir(string str)
        {
            int length = str.Length;
            int count = length - 1;
            while (count >= 0 & str.Substring(count, 1) != "\\")
            {
                count -= 1;
            }

            string name = str.Substring(0, count);

            return name;
        }

        private void TreeViewExp_MouseMove(object sender, MouseEventArgs e)
        {
            ctr = sender as TreeView;
            //Point p = TreeViewExp.PointToClient(new Point(e.X, e.Y));
            TreeNode node = ctr.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                TreeViewExp_DrawNodeBorder(node);
            }
        }
        TreeView ctr = null;
        private Rectangle NodeBorder = new Rectangle();
        private void TreeViewExp_MouseLeave(object sender, EventArgs e)
        {
            TreeViewExp_DeleteDrawnNodeBorder();
        }
        private void TreeViewExp_MouseWheel(object sender, MouseEventArgs e)
        {
            TreeViewExp_DeleteDrawnNodeBorder();
        }
        private void TreeViewExp_DeleteDrawnNodeBorder()
        {
            if (ctr == null) { return; }
            Pen p = new Pen(BackGround2Color1);
            Graphics g = ctr.CreateGraphics();
            g.DrawRectangle(p, NodeBorder);
        }
        private void TreeViewExp_DrawNodeBorder(TreeNode n)
        {
            if (ctr == null) { return; }
            TreeViewExp_DeleteDrawnNodeBorder();
            if (n != ctr.SelectedNode)
            {
                Graphics g = ctr.CreateGraphics();
                Pen p = new Pen(TitlePanelColor1);
                NodeBorder = n.Bounds;
                g.DrawRectangle(p, NodeBorder);
            }
        }
        private Boolean TreeView_NodeExists(string Dir)
        {
            Boolean exists = false;
            foreach (TreeNode node in TreeViewExp.Nodes)
            {
                if (Dir == node.Tag.ToString())
                {
                    exists = true;
                    return exists;
                }
            }
            return exists;
        }
        public Boolean TreeViewExp_AddFolderNode(string Dir)
        {

            if (TreeView_NodeExists(Dir) == false)
            {
                TreeNode n = new TreeNode();
                n.Tag = Dir;
                n.Text = FileNameFromDir(Dir);
                TreeViewExp.Nodes.Add(n);
                n.ImageIndex = 0;
                n.SelectedImageIndex = 0;
                return true;
            }
            else
            {
                MessageBox.Show("Work directory is already added!");
                return false;
            }
            //TreeViewExp_populate(n);
        }
        public void TreeViewExp_NodeDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Get the node at the current mouse pointer location.
                TreeNode n = TreeViewExp.GetNodeAt(e.X, e.Y);
                DragDrop_Release();
                if (n != doubleClickTarget) return;
                //((TreeView)sender).Cursor = Cursors.Default;
                doubleClicked = true;
                // Set a ToolTip only if the mouse pointer is actually paused on a node.
                if ((n != null))
                {
                    if (n.ImageIndex == 0 & n.Nodes.Count < 1)
                    {
                        StatusLabel.Text = "Loading files...";
                        TreeViewExp.SuspendLayout();
                        TreeViewExp_populate(n);
                        n.Expand();
                        TreeViewExp.ResumeLayout();
                        StatusLabel.Text = "Ready";
                    }
                    else if (n.ImageIndex != 0)
                    {
                        Open_Event(n);
                    }
                }

            }
            if (e.Button == MouseButtons.Right)
            {
                // Get the node at the current mouse pointer location.
                TreeNode n = TreeViewExp.GetNodeAt(e.X, e.Y);
                if (n != doubleClickTarget) return;
                //((TreeView)sender).Cursor = Cursors.Default;
                doubleClicked = true;
                // Set a ToolTip only if the mouse pointer is actually paused on a node.
                if ((n != null))
                {
                    TreeViewExp_populate(n);
                    n.Expand();
                }
            }
        }

        public void DeleteBtn_Click(object sender, EventArgs e)
        {
            Delete_Event();
        }
        public void Delete_Event()
        {
            if (TreeViewExp.ContainsFocus == true & TreeViewExp.SelectedNode != null)
            {
                if (MessageBox.Show("Do you want to delete " + TreeViewExp.SelectedNode.Tag.ToString() + "?",
                    "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (Directory.Exists(OSStringConverter.StringToDir(
                        TreeViewExp.SelectedNode.Tag.ToString())) == true)
                    {
                        if (TreeViewExp.SelectedNode.Parent == null)
                        {
                            Properties.Settings settings = Properties.Settings.Default;
                            string newStr = "";
                            foreach (string line in settings.TreeViewContent[ActiveAccountIndex].Split(new[] { "\t" }, StringSplitOptions.None))
                            {
                                if (line == "@")
                                {
                                    newStr += "@";
                                }
                                else if (line != TreeViewExp.SelectedNode.Tag.ToString())
                                {
                                    newStr += "\t" + line;
                                }
                            }
                            settings.TreeViewContent[ActiveAccountIndex] = newStr;
                            settings.Save();

                        }
                        //Directory.Delete(TreeViewExp.SelectedNode.Tag.ToString(), true);
                        OSFileManager.DeleteDirectory(TreeViewExp.SelectedNode.Tag.ToString(),StatusLabel);

                    }
                    else if (File.Exists(OSStringConverter.StringToDir(
                        TreeViewExp.SelectedNode.Tag.ToString())) == true)
                    {
                        OSFileManager.DeleteFile(TreeViewExp.SelectedNode.Tag.ToString(),StatusLabel);
                        //File.Delete(TreeViewExp.SelectedNode.Tag.ToString());

                    }
                    Vbox_RemoveItem(TreeViewExp.SelectedNode);
                    TreeViewExp.SelectedNode.Remove();
                }
            }
            else if (Vbox.ContainsFocus == true & Vbox.SelectedNode != null)
            {
                if (Vbox.SelectedNode.Tag.ToString() == "All") { return; }
                int index = Vbox.SelectedNode.Index;
                Vbox_TreenodesList[index].Checked = false;
                Treenode_UncheckParent(Vbox_TreenodesList[index]);
                Vbox_TreenodesList.RemoveAt(index);
                Vbox.Nodes.RemoveAt(index);
            }
            else
            {
                MessageBox.Show("There is no selected item!");
            }
        }
        public void AddBtn_click(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Add work directory:";
            if (Directory.Exists(OSStringConverter.StringToDir(settings.OldWorkDir[ActiveAccountIndex])))
            {
                fbd.SelectedPath = OSStringConverter.StringToDir(settings.OldWorkDir[ActiveAccountIndex]);
            }
            DialogResult result = fbd.ShowDialog();
            // OK button was pressed.
            if (result == DialogResult.OK)
            {
                StatusLabel.Text = "Adding work directory...";
                TreeViewExp.SuspendLayout();
                if (TreeViewExp_AddFolderNode(OSStringConverter.GetWinString(fbd.SelectedPath)) == true)
                {
                    settings.OldWorkDir[ActiveAccountIndex] = OSStringConverter.GetWinString(fbd.SelectedPath);
                    settings.TreeViewContent[ActiveAccountIndex] += "\t" + OSStringConverter.GetWinString(fbd.SelectedPath);
                    settings.Save();
                }
                
                TreeViewExp.ResumeLayout();
                StatusLabel.Text = "Ready";
            }
        }
        public void TreeNode_DeleteMissingAndPopulateExisting(DirectoryInfo directoryInfo, TreeNode parent)
        {
            List<TreeNode> delList = new List<TreeNode>();
            foreach (TreeNode node in parent.Nodes)
            {

                Boolean exists = false;
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    if (node.Tag.ToString() == OSStringConverter.GetWinString(directory.FullName))
                    {
                        exists = true;
                        TreeViewExp_populate(node);
                    }
                }

                foreach (var file in directoryInfo.GetFiles())
                {
                    if (node.Tag.ToString() == OSStringConverter.GetWinString(file.FullName))
                    {
                        exists = true;
                    }
                }
                if (exists == false)
                {
                    delList.Add(node);

                }

            }
            if (delList != null)
            {
                foreach (TreeNode tn in delList)
                {
                    Vbox_RemoveItem(tn);
                    tn.Remove();
                }
                delList.Clear();
            }
        }
        public void TreeViewExp_populate(TreeNode node)
        {

            string Dir = node.Tag.ToString();
            if (Directory.Exists(OSStringConverter.StringToDir(Dir)) == false) { return; }
            
            DirectoryInfo directoryInfo = new DirectoryInfo(OSStringConverter.StringToDir(Dir));
            // Delete missing files and directories and populate existing ones
            TreeNode_DeleteMissingAndPopulateExisting(directoryInfo, node);
            //var directoryNode = new TreeNode(Dir);
            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (TreeNode_NodeExists(OSStringConverter.GetWinString(directory.FullName), node) == false)
                {
                    TreeNode n = TreeNod_AddFolderNode(node, OSStringConverter.GetWinString(directory.FullName));
                    TreeViewExp_populate(n);
                }
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                if (TreeNode_NodeExists(OSStringConverter.GetWinString(file.FullName), node) == false)
                {
                    TreeNod_AddFileNode(node, OSStringConverter.GetWinString(file.FullName));
                }
            }
            Vbox_Refresh();
        }
        private bool TreeNode_NodeExists(string Dir, TreeNode Parent)
        {
            bool exists = false;
            foreach (TreeNode node in Parent.Nodes)
            {
                if (Dir == node.Tag.ToString())
                {
                    exists = true;
                    return exists;
                }
            }
            return exists;
        }

        public TreeNode TreeNod_AddFolderNode(TreeNode node, string Dir)
        {
            TreeNode n = new TreeNode();
            n.Tag = Dir;
            n.Text = FileNameFromDir(Dir);
            node.Nodes.Add(n);
            n.ImageIndex = 0;
            n.SelectedImageIndex = 0;
            n.Checked = false;
            Treenode_UncheckParent(n);
            return n;
        }

        private void CheckParentIfAllNodesChecked(TreeNode node)
        {
            Boolean checkedState = true;
            foreach (TreeNode n in node.Nodes)
            {
                if (n.Checked == false) { checkedState = false; }
            }
            if (checkedState == true)
            {
                node.Checked = true;
                if (node.Parent != null)
                {
                    CheckParentIfAllNodesChecked(node.Parent);
                }
            }
        }
        private void TreeNode_AfterCheck(object sender, TreeViewEventArgs e)
        {
            Boolean checkState = e.Node.Checked;
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                Vbox_Populate(e.Node, checkState);
            }
        }
        private void Vbox_Populate(TreeNode node, Boolean checkState)
        {
            if (node == null) { return; }
            if (node.ImageIndex != 0)
            {
                if (node.Checked == true)
                {
                    Vbox_TreenodesList.Add(node);
                    Vbox_AddNode(node);

                }
                else
                {
                    Vbox_RemoveItem(node);
                }

            }
            else if (node.Nodes.Count < 1)
            {
                TreeViewExp_populate(node);
            }
            node.Checked = checkState;
            Treenode_UncheckParent(node);
            if (node.Nodes.Count > 0)
            {
                CheckAllChildNodes(node, node.Checked);
            }
            if (node.Parent != null)
            {
                CheckParentIfAllNodesChecked(node.Parent);
            }
            Vbox_Refresh();
        }
        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                if (node.Checked != nodeChecked)
                {
                    node.Checked = nodeChecked;
                    if (node.ImageIndex != 0)
                    {
                        if (node.Checked == true)
                        {
                            Vbox_TreenodesList.Add(node);
                            Vbox_AddNode(node);
                            //Vbox.Items.Add(node.Text);
                        }
                        else
                        {
                            Vbox_RemoveItem(node);
                        }
                    }
                }
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    this.CheckAllChildNodes(node, nodeChecked);
                }
            }
        }
        //Vbox
        private void Vbox_RemoveItem(TreeNode node)
        {
            int index = -1;
            for (int z = 0; z < Vbox_TreenodesList.Count; z++)
            {
                if (node == Vbox_TreenodesList[z])
                {
                    index = z;
                    break;
                }
            }
            if (index != -1)
            {
                Treenode_UncheckParent(Vbox_TreenodesList[index]);
                Vbox_TreenodesList.RemoveAt(index);
                Vbox.Nodes.RemoveAt(index);
            }
            else
            {
                foreach (TreeNode pup in node.Nodes)
                {
                    Vbox_RemoveItem(pup);
                }
            }
        }
        private void Treenode_UncheckParent(TreeNode node)
        {
            if (node.Checked == false)
            {

                if (node.Parent != null)
                {
                    node.Parent.Checked = false;
                    Treenode_ClearAllParent(node.Parent);
                }
            }
        }
        private void Treenode_ClearAllParent(TreeNode node)
        {
            if (node.Parent != null)
            {
                node.Parent.Checked = false;
                Treenode_ClearAllParent(node.Parent);
            }
        }
        private void Vbox_Refresh()
        {
            try
            {
                List<int> index = null;
                for (int z = 0; z < Vbox_TreenodesList.Count; z++)
                {
                    if (Vbox_TreenodesList[z].Checked == false)
                    {
                        index.Add(z);
                    }
                }
                for (int z = index.Count - 1; z >= 0; z++)
                {
                    Vbox_TreenodesList.RemoveAt(index[z]);
                    Vbox.Nodes.RemoveAt(index[z]);
                }
            }
            catch { }
        }
        private void Vbox_RefreshDir()
        {

            {
                for (int z = 0; z < Vbox_TreenodesList.Count; z++)
                {
                    if (Vbox_TreenodesList[z].Tag.ToString() != Vbox.Nodes[z].Tag.ToString())
                    {
                        Vbox.Nodes[z].Tag = Vbox_TreenodesList[z].Tag.ToString();
                        Vbox.Nodes[z].Text = Vbox_TreenodesList[z].Text;
                    }
                }
            }

        }

        private void Vbox_AddNode(TreeNode node)
        {
            TreeNode n = new TreeNode();
            n.Text = node.Text;
            n.Tag = node.Tag;
            n.ImageIndex = node.ImageIndex;
            n.SelectedImageIndex = node.SelectedImageIndex;

            n.Checked = false;
            Vbox.Nodes.Add(n);
        }
        private void TreeViewExp_OpenChecked(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to open all checked tif images from the Data sources?",
                              "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            if (TreeViewExp.Nodes.Count < 1) { return; }
            foreach (TreeNode n in TreeViewExp.Nodes)
            {
                if (n.Checked == true)
                {
                    if (n.Nodes.Count < 1)
                    {
                        TreeViewExp_populate(n);
                    }
                    TreeViewExp_OpenSearched_Event(n);
                }
                else if (n.Nodes.Count > 0)
                {
                    treeNode_OpenChecked(n);
                }
            }

        }
        private void treeNode_OpenChecked(TreeNode node)
        {
            if (node.Nodes.Count < 1) { return; }
            foreach (TreeNode n in node.Nodes)
            {
                if (n.Checked == true)
                {
                    TreeViewExp_OpenSearched_Event(n);
                }
                else
                {
                    treeNode_OpenChecked(n);
                }
            }
        }
        private void TreeViewExp_OpenSearched(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to open all searched tif images from the Data sources?",
                              "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            if (TreeViewExp.Nodes.Count < 1) { return; }
            foreach (TreeNode n in TreeViewExp.Nodes)
            {
                if (n.BackColor == Color.Green)
                {
                    if (n.Nodes.Count < 1)
                    {
                        TreeViewExp_populate(n);
                    }

                    TreeViewExp_OpenSearched_Event(n);

                }
                else if (n.Nodes.Count > 0)
                {
                    treeNode_OpenSearched(n);
                }
            }

        }
        private void treeNode_OpenSearched(TreeNode node)
        {
            if (node.Nodes.Count < 1) { return; }
            foreach (TreeNode n in node.Nodes)
            {
                if (n.BackColor == Color.Green)
                {
                    TreeViewExp_OpenSearched_Event(n);
                }
                else
                {
                    treeNode_OpenSearched(n);
                }
            }
        }
        private void TreeViewExp_OpenSearched_Event(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    if (n.ImageIndex != 0)
                    {
                        TreeNode_Open(n);
                    }
                    else
                    {
                        TreeViewExp_OpenSearched_Event(n);
                    }
                }
            }
            else if (node.ImageIndex != 0)
            {
                TreeNode_Open(node);
            }
        }
        private void OpenVboxItem(object sender, EventArgs e)
        {
            TreeNode node = Vbox.SelectedNode;
            if (node == null) { return; }
            if (node.Tag.ToString() != "All")
            {
                TreeNode_Open(Vbox_TreenodesList[Vbox.Nodes.IndexOf(node)]);
            }
        }
        private void OpenVboxItem_Searched(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to open all searched tif images from the Virtual box?",
                              "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            foreach (TreeNode n in Vbox.Nodes)
            {
                if (n.BackColor == Color.Green)
                {
                    if (n.Tag.ToString() != "All")
                    {
                        TreeNode_Open(Vbox_TreenodesList[Vbox.Nodes.IndexOf(n)]);
                    }
                }
            }

        }
        private void OpenVboxItem_Checked(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to open all checked tif images from the Virtual box?",
                              "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            foreach (TreeNode n in Vbox.Nodes)
            {
                if (n.Checked == true)
                {
                    if (n.Tag.ToString() != "All")
                    {
                        TreeNode_Open(Vbox_TreenodesList[Vbox.Nodes.IndexOf(n)]);
                    }
                }
            }

        }

        private void checkForSavedFiles(TreeNode node)
        {
            if (node.ImageIndex != 0)
            {
                if(node.ImageIndex < 3)
                if (FileHaveResults(node) == true)
                {
                    node.ImageIndex = 2;
                    node.SelectedImageIndex = 2;
                }
                else
                {
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                }
            }
            else {
                if (node.Nodes.Count > 0)
                {
                    foreach (TreeNode n in node.Nodes)
                    {
                        checkForSavedFiles(n);
                    }
                }
            }
        }
        //Work Here for open files

        private Boolean FileHaveResults(TreeNode node)
        {
            string dir = node.Tag.ToString();
            int end = dir.LastIndexOf(".");
            if (File.Exists(OSStringConverter.StringToDir(
                dir.Replace(dir.Substring(end, dir.Length - end), ".txt"))) == true)
            {
                return true;
            }
            return false;
        }
        public void Refresh_AfterSave()
        {
            foreach (TreeNode node in TreeViewExp.Nodes)
            {
                checkForSavedFiles(node);
            }
            foreach (TreeNode node in Vbox.Nodes)
            {
                checkForSavedFiles(node);
            }
        }
        public void TreeNode_Open(TreeNode node)
        {
            if (node != null)
            {
                if (node.ImageIndex != 0)
                {
                    Openlabel.Tag = node;
                    Openlabel.Text = "'" + node.Tag.ToString() + "'";
                    Openlabel.Tag = null;
                    Openlabel.Text = "";
                }

            }
        }
        public TreeNode TreeNod_AddFileNode(TreeNode node, string Dir)
        {
            //Check extension
            int ext = -1;
            //check is it correct file type
            foreach (string format in Formats)
            {
                if (Dir.EndsWith(format))
                {
                    ext = Formats.IndexOf(format);
                }
            }
            if (ext == -1)
            {
                return null;
            }


            TreeNode n = new TreeNode();
            n.Tag = Dir;
            n.Text = FileNameFromDir(Dir);

            switch (ext)
            {
                case 1:
                    n.ImageIndex = 3;
                    n.SelectedImageIndex = 3;
                    break;
                case 2:
                    return null;
                default:
                    if (FileHaveResults(n) == false)
                    {
                        n.ImageIndex = 1;
                        n.SelectedImageIndex = 1;
                    }
                    else
                    {
                        n.ImageIndex = 2;
                        n.SelectedImageIndex = 2;
                    }
                    break;
                    //Add new case for new file format
            }

            node.Nodes.Add(n);
            n.Checked = false;
            Treenode_UncheckParent(n);
            return n;
        }
        private ImageList TreeView_ImageList()
        {
            ImageList il = new ImageList();
            il.ImageSize = new Size(13, 15);
            // Folder image
            il.Images.Add(Properties.Resources.FolderIcon);
            //tif image
            il.Images.Add(Properties.Resources.tifIcon);
            il.Images.Add(Properties.Resources.tifIcon_Green);
            il.Images.Add(Properties.Resources.RoiIcon);
            //add image for new formats

            return il;
        }
        public TreeNode CheckForFile(string dir)
        {
            if (TreeViewExp.Nodes.Count < 1) { return null; }

            TreeNode node = null;
            
            foreach (TreeNode n in TreeViewExp.Nodes)
                if (dir.StartsWith((string)n.Tag + "\\"))
                {
                    node = n;
                    break;
                }

            if (node == null) return null;
             
            bool StopLoop = false;
            while (StopLoop == false)
            {
                StopLoop = true;

                if (node.Nodes.Count == 0)
                {
                    if (node.ImageIndex == 0)
                        TreeViewExp_populate(node);
                    else if ((string)node.Tag == dir) return node;
                        return node;
                }

                if (node.Nodes.Count == 0) return null;

                foreach (TreeNode n in node.Nodes)
                    if ((string)n.Tag == dir)
                        return n;
                    else if (dir.StartsWith((string)n.Tag + "\\"))
                    {
                        node = n;
                        StopLoop = false;
                        break;
                    }

                if (StopLoop && node.ImageIndex==0)
                {
                    TreeViewExp_populate(node);
                    StopLoop = false;
                }
            }

            if (node != null && (string)node.Tag != dir)
                return null; 
            else
                return node;
        }
        public TreeNode CheckForFile1(string dir)
        {
            if (TreeViewExp.Nodes.Count < 1) { return null; }
            TreeNode node = null;
            string[] strList = dir.Split(new[] { "\\" }, StringSplitOptions.None);
            string curDir = strList[0];
            for (int i = 0; i < strList.Length; i++)
            {
                string str = strList[i];
                if (node == null)
                {
                    foreach (TreeNode n in TreeViewExp.Nodes)
                    {
                        if (n.Text == str & n.Tag as string == curDir + "\\" + str)
                        {
                            node = n;
                            TreeViewExp_populate(node);
                            break;
                        }
                    }
                }
                else
                {
                    node = CheckForFileInNode(node, str);
                }
                if (i != 0)
                {
                    curDir += "\\" + str;
                }

                if (node != null)
                {
                    if (node.Tag as string != curDir) { return null; }
                }
            }
            return node;
        }
        private TreeNode CheckForFileInNode(TreeNode node, string str)
        {
            if (node.Nodes.Count > 0)
            {

                foreach (TreeNode n in node.Nodes)
                {
                    if (n.Text == str)
                    {
                        node = n;
                        break;
                    }
                }

            }
            return node;
        }
    }

}
