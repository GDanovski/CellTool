using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;

namespace CTFileDialog
{    
    public partial class OpenFileDialog : Form
    {
        private bool _AddExtention;
        private bool _CheckFileExists;
        private bool _CheckPathExists;
        private string _FileName;
        private string _Filter;
        private int _FilterIndex;
        private string _InitialDirectory;
        private string[] extentions;
        private TreeNode topNode;

        public OpenFileDialog()
        {
            InitializeComponent();

            this.Icon = Cell_Tool_3.Properties.Resources.CT_done;
            this.DirTreeView.ImageList = new ImageList();
            this.DirTreeView.ImageList.Images.Add(Cell_Tool_3.Properties.Resources.FolderIcon );
            this.FileTreeView.ImageList = new ImageList();
            this.FileTreeView.ImageList.Images.Add(Cell_Tool_3.Properties.Resources.paste_mini_1);

            this.FilterIndex = 0;
            this.Filter = "All files|*.*";
            this.AddExtentionsToCombobox();
            this.AddExtention = true;
            this.CheckFileExists = true;
            this.CheckPathExists = true;
            this.InitialDirectory = "";
            this.FileName = "";

            this.DirTreeView.BeforeExpand += DirTreeView_BeforeExpand;
            this.DirTreeView.AfterExpand += DirTreeView_AfterExpand;
            this.DirTreeView.AfterSelect += DirTreeView_AfterSelect;
            this.FileTreeView.AfterSelect += FileTreeView_AfterSelect;
            this.FileTreeView.NodeMouseDoubleClick += FileTreeView_DoubleClick;
            this.button_Desktop.Click += button_NavigateToDir;
            this.button_MyComputer.Click += button_NavigateToDir;
            this.button_Personal.Click += button_NavigateToDir;

            this.FileTreeView.HideSelection = false;
            this.DirTreeView.HideSelection = false;
        }
        
        private void OpenFileDialog_Load(object sender, EventArgs e)
        {
            OpenBtn.Text = this.Text;
            
            this.GetHardDrives();
            this.SetInitialDirectory();
        }
        
        public bool AddExtention
        {
            set
            {
                this._AddExtention = value;
            }
            get
            {
                return this._AddExtention;
            }
        }
        public bool CheckFileExists
        {
            set
            {
                this._CheckFileExists = value;
            }
            get
            {
                return this._CheckFileExists;
            }
        }
        public bool CheckPathExists
        {
            set
            {
                this._CheckPathExists = value;
            }
            get
            {
                return this._CheckPathExists;
            }
        }
        public string FileName
        {
            set
            {
                this._FileName = value;
            }
            get
            {
                return this._FileName;
            }
        }
        public string InitialDirectory
        {
            set
            {
                this._InitialDirectory = value;
                SetInitialDirectory();
            }
            get
            {
                return this._InitialDirectory;
            }
        }
        public string Filter
        {
            set
            {
                if (this._Filter != value)
                {
                    this._Filter = value;
                    AddExtentionsToCombobox();
                }
            }
            get
            {
                return this._Filter;
            }
        }
        public int FilterIndex
        {
            set
            {
                this._FilterIndex = value;
            }
            get
            {
                return this._FilterIndex;
            }
        }
       
        private void AddExtentionsToCombobox()
        {
            string[] rows = this.Filter.Split(new string[] { "|" }, StringSplitOptions.None);

            string[] names = new string[rows.Length / 2];
            this.extentions = new string[names.Length];

            for(int i = 0,j=0; i< rows.Length;j++)
            {
                names[j] = rows[i++] + " (" + rows[i] + ")";
                this.extentions[j] = rows[i++];
            }

            this.extentionsCmbBox.Items.Clear();
            this.extentionsCmbBox.Items.AddRange(names);
            this.extentionsCmbBox.SelectedIndex = this.FilterIndex;
        }
        private void GetHardDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    TreeNode node = new TreeNode();
                    node.Text = d.Name;
                    node.Tag = node.Text;
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    GetDirectoriesInNode(node);
                    DirTreeView.Nodes.Add(node);
                }
            }
        }
        private void GetDirectoriesInNode(TreeNode parent)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo((string)parent.Tag);
           
            List<TreeNode> nodes = new List<TreeNode>();
            try
            {
                foreach (var dirs in directoryInfo.GetDirectories())
                {
                    TreeNode node = new TreeNode();
                    node.Text = dirs.Name;
                    node.Tag = dirs.FullName;
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    nodes.Add(node);
                }
            }
            catch
            {
                Console.WriteLine("Access denied: " + directoryInfo.FullName);
            }

            parent.Nodes.Clear();
            parent.Nodes.AddRange(nodes.ToArray());
        }
        private string GetExtention
        {
            get
            {
                string ext = "";
                int index = extentionsCmbBox.SelectedIndex;
                if (extentions[index] != "*.*")
                {
                    ext = extentions[index].Replace("*", "");
                }
                return ext;
            }
        }
        private string GetDirSeparator
        {
            get
            {
                switch (System.Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        return "/";
                    case PlatformID.Unix:
                        return "/";
                    default:
                        return "\\";
                }
            }
        }
        private void SetInitialDirectory(string path = "")
        {
            if (path == "")
                path = this._InitialDirectory;

            if (path == "") return;           
            
            string[] names = null;
            if (path.Contains("\\"))
                names = path.Split(new string[] { "\\" }, StringSplitOptions.None);
            else if (path.Contains("/"))
                names = path.Split(new string[] { "/" }, StringSplitOptions.None);
           
            if (names.Length < 1) return;

            TreeNode current = null;
            TreeNode temp = null;
            current = GetChildNodeByName(DirTreeView.Nodes, names[0]);            
            
            for (int i = 1; i < names.Length; i++)
                if (current != null)
                {
                    temp = current;
                    current = GetChildNodeByName(current.Nodes, names[i]);                      
                }
                else
                {
                    break;
                }

            if (current != null) temp = current;
            DirTreeView.Select();
            DirTreeView.SelectedNode = temp;
            DirTreeView.TopNode = temp;

            temp = null;
            current = null;
        }
        private TreeNode GetChildNodeByName(TreeNodeCollection parent,string name)
        {            
            foreach (TreeNode node in parent)
                if (node.Text == name || node.Text == name + "\\" || node.Text == name + "/")
                {                    
                    GetDirectoriesInNode(node);
                    node.Expand();
                    return node;
                }

            return null;
        }
        private void GetFilesInNode(TreeNode parent = null)
        {
            if(parent == null && FileTreeView.Tag != null)
            {
                parent = (TreeNode)FileTreeView.Tag;
            }
            else
            {
                FileTreeView.Tag = parent;
            }
            if (parent == null) return;

            DirectoryInfo directoryInfo = new DirectoryInfo((string)parent.Tag);
            string ext = this.GetExtention;
            List<TreeNode> nodes = new List<TreeNode>();
            try
            {
                foreach (var dirs in directoryInfo.GetFiles())
                    if (dirs.Name.EndsWith(ext))
                    {
                        TreeNode node = new TreeNode();
                        node.Text = dirs.Name;
                        node.Tag = dirs.FullName;
                        node.ImageIndex = 0;
                        node.SelectedImageIndex = 0;
                        nodes.Add(node);
                    }
            }
            catch
            {
                Console.WriteLine("Access denied: " + directoryInfo.FullName);
            }

            FileTreeView.SuspendLayout();

            FileTreeView.Nodes.Clear();
            FileTreeView.Nodes.AddRange(nodes.ToArray());

            FileTreeView.ResumeLayout(true);
        }

        private void DirTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode parent = e.Node;
            this.topNode = this.DirTreeView.TopNode;

            foreach (TreeNode node in parent.Nodes)
            {
                GetDirectoriesInNode(node);
            }
        }
        private void DirTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (this.topNode != null)
                this.DirTreeView.TopNode = this.topNode;
        }
        private void DirTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!this.DirTreeView.Focused) return;
            TreeNode node = e.Node;
            if (node != null)
            {
                GetFilesInNode(node);
            }
            else
            {
                FileTreeView.Nodes.Clear();
            }

            FileName_textBox.Tag = null;
            FileName_textBox.Text = "";
        }
        private void FileTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!this.FileTreeView.Focused) return;

            TreeNode node = e.Node;
            if (node != null)
            {
                FileName_textBox.Tag = node;
                FileName_textBox.Text = node.Text;
            }
            else
            {
                FileName_textBox.Tag = null;
                FileName_textBox.Text = "";
            }
        }
        private void extentionsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetFilesInNode();
        }
        
        private void OpenBtn_Click(object sender, EventArgs e)
        {
            if (this.FileName_textBox.Tag == null && this.FileTreeView.SelectedNode != null)
            {
                this.FileName_textBox.Tag = this.FileTreeView.SelectedNode;
                this.FileName_textBox.Text = this.FileTreeView.SelectedNode.Text;
            }

            if (this.FileName_textBox.Tag == null && OpenBtn.Text != "Open" && FileName_textBox.Text != "")
            {
                TreeNode node = new TreeNode();
                node.Text = FileName_textBox.Text;
                node.Tag =(string)((TreeNode) FileTreeView.Tag).Tag + this.GetDirSeparator + node.Text;
                FileName_textBox.Tag = node;
            }

            if (this.FileName_textBox.Tag == null && OpenBtn.Text == "Open")
            {
                MessageBox.Show("There is no selected file!");
                return;
            }
            if (this.FileName_textBox.Tag == null && OpenBtn.Text != "Open" && FileName_textBox.Text == "")
            {
                MessageBox.Show("There is no selected file!");
                return;
            }            

            this.FileName = (string)((TreeNode)this.FileName_textBox.Tag).Tag;

            if (this.AddExtention)
            {
                string ext = GetExtention;

                if (!this.FileName.EndsWith(ext))
                    this.FileName += ext;
            }

            if (this.CheckFileExists && File.Exists(this.FileName))
            {
                if (MessageBox.Show("There is file with the same name.\nDo you want to overwrite it?",
                    this.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }
            if (!Directory.Exists(Path.GetDirectoryName(this.FileName)))
            {
                if (MessageBox.Show("Directory is not existing:\n\"" + Path.GetDirectoryName(this.FileName) + "\"\nDo you want to create it?",
                    this.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.FileName));
                }
                catch
                {
                    MessageBox.Show("Access denied!");
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void FileName_textBox_TextChanged(object sender, EventArgs e)
        {
            FileName_textBox.Tag = null;
        }
        private void FileTreeView_DoubleClick(object sender,TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            FileName_textBox.Text = node.Text;
            FileName_textBox.Tag = node;
            OpenBtn.PerformClick();
        }
        private void button_NavigateToDir(object sender, EventArgs e)
        {
            switch (((Button)sender).Text)
            {
                case "Desktop":
                    this.SetInitialDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                    break;
                case "My Computer":
                    DirTreeView.Select();
                    DirTreeView.SelectedNode = null;
                    foreach (TreeNode node in DirTreeView.Nodes)
                        node.Collapse();
                    break;
                case "Personal":
                    this.SetInitialDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    break;
            }
        }
    }
    public partial class SaveFileDialog : CTFileDialog.OpenFileDialog
    {
        public SaveFileDialog()
        {
            this.Text = "Save As";
        }
    }

}
