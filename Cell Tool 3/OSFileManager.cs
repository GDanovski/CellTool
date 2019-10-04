using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;

namespace Cell_Tool_3
{
    class OSFileManager
    {
        public static void CopyFile(string Dir, string NewDir, ToolStripStatusLabel StatusLabel)
        {
            if (!File.Exists(Dir)) return;
            try
            {
                if (File.Exists(NewDir)) File.Delete(NewDir);
            }
            catch
            {
                MessageBox.Show("Target directory is not avaliable!");
                return;
            }
            
            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                File.Copy(Dir, NewDir, true);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Copy File", Dir, NewDir, bgw,StatusLabel);

        }
        public static void CopyDirectory(string Dir, string NewDir, ToolStripStatusLabel StatusLabel)
        {
            if (!Directory.Exists(Dir)) return;
            try
            {
                if (Directory.Exists(NewDir)) Directory.Delete(NewDir, true);
            }
            catch
            {
                MessageBox.Show("Target directory is not avaliable!");
                return;
            }

            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                DirectoryCopy(Dir, NewDir, true);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Copy Directory", Dir, NewDir, bgw,StatusLabel);

        }

        public static void DeleteDirectory(string Dir, ToolStripStatusLabel StatusLabel)
        {
            if (!Directory.Exists(Dir)) return;
            
            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                Directory.Delete(Dir, true);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Delete Directory", Dir, "", bgw,StatusLabel);

        }
        public static void DeleteFile(string Dir, ToolStripStatusLabel StatusLabel)
        {
            if (!File.Exists(Dir)) return;
           
            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                File.Delete(Dir);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Delete File", Dir, "", bgw, StatusLabel);

        }
        public static void MoveFile(string Dir, string NewDir, ToolStripStatusLabel StatusLabel)
        {
            if (!File.Exists(Dir)) return;
            try
            {
                if (File.Exists(NewDir)) File.Delete(NewDir);
            }
            catch
            {
                MessageBox.Show("Target directory is not avaliable!");
                return;
            }
            
            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                File.Move(Dir, NewDir);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Move File", Dir, NewDir, bgw, StatusLabel);

        }
        public static void MoveDirectory(string Dir, string NewDir, ToolStripStatusLabel StatusLabel)
        {
            if (!Directory.Exists(Dir)) return;
            try
            {
                if (Directory.Exists(NewDir)) Directory.Delete(NewDir, true);
            }
            catch
            {
                MessageBox.Show("Target directory is not avaliable!");
                return;
            }

            var bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {

                Directory.Move(Dir, NewDir);

                ((BackgroundWorker)o).ReportProgress(0);
            });
            InfoForm form = new InfoForm();
            form.SetUp("Move Directory", Dir, NewDir, bgw, StatusLabel);
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        private class InfoForm : Form
        {
            private Label lab_From;
            private Label lab_To;
            private ProgressBar pb;
            private BackgroundWorker bgw;

            public InfoForm()
            {
                this.Text = "FileSystem";
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ControlBox = false;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Normal;
                this.Width = 300;
                this.Height = 150;
                this.Icon = Properties.Resources.CT_done;

                lab_From = new Label();
                lab_To = new Label();
                pb = new ProgressBar();

                Label contr = this.lab_From;
                {
                    contr.Location = new System.Drawing.Point(5, 5);
                    contr.Text = "From Directory: ";
                    contr.Width = this.Width - 10;
                    contr.TextChanged += Label_TextChanged;
                    this.Controls.Add(contr);
                }

                contr = this.lab_To;
                {
                    contr.Location = new System.Drawing.Point(5, 35);
                    contr.Text = "To Directory: ";
                    contr.Width = this.Width - 10;
                    contr.TextChanged += Label_TextChanged;
                    this.Controls.Add(contr);
                }

                this.pb = new ProgressBar();
                pb.Location = new System.Drawing.Point(5, 75);
                pb.Width = this.Width - 30;
                this.pb.Minimum = 1;
                this.pb.Maximum = 100;
                this.pb.Step = 10;
                this.pb.Value = 10;
                this.pb.MarqueeAnimationSpeed = 30;
                this.pb.Style = ProgressBarStyle.Marquee;
                this.pb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                this.Controls.Add(pb);
            }
            public void SetUp(string name, string FromDir, string ToDir, BackgroundWorker bgw, ToolStripStatusLabel StatusLabel)
            {
                this.Text = name;
                this.FromDir = FromDir;
                this.ToDir = ToDir;
                if (ToDir == "") this.lab_To.Visible = false;

                this.bgw = bgw;
                this.bgw.WorkerReportsProgress = true;

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    this.Close();
                    this.Dispose();
                });

                bgw.RunWorkerAsync();

                StatusLabel.Text = "Dialog open";
                this.ShowDialog();
                StatusLabel.Text = "Ready";
            }
            private void Label_TextChanged(object sender, EventArgs e)
            {
                int w = TextRenderer.MeasureText(((Label)sender).Text, ((Label)sender).Font).Width + 20;
                ((Label)sender).Width = w;

                if (this.Width < w) this.Width = w;
            }
            public string FromDir
            {
                get
                {
                    return this.lab_From.Text;
                }
                set
                {
                    this.lab_From.Text += value;
                }
            }
            public string SetOperationName
            {
                get
                {
                    return this.Text;
                }
                set
                {
                    this.Text = value;
                }
            }
            public string ToDir
            {
                get
                {
                    return this.lab_To.Text;
                }
                set
                {
                    this.lab_To.Text += value;
                }
            }
            public BackgroundWorker GetBackgroundWorker
            {
                get
                {
                    return this.bgw;
                }
                set
                {
                    this.bgw = value;
                }
            }
        }
    }
}
