namespace CTFileDialog
{
    partial class OpenFileDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OpenBtn = new System.Windows.Forms.Button();
            this.extentionsCmbBox = new System.Windows.Forms.ComboBox();
            this.FileName_textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DirTreeView = new System.Windows.Forms.TreeView();
            this.FileTreeView = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.button_Desktop = new System.Windows.Forms.Button();
            this.button_Personal = new System.Windows.Forms.Button();
            this.button_MyComputer = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CancelBtn);
            this.panel1.Controls.Add(this.OpenBtn);
            this.panel1.Controls.Add(this.extentionsCmbBox);
            this.panel1.Controls.Add(this.FileName_textBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 373);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 94);
            this.panel1.TabIndex = 0;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(594, 54);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // OpenBtn
            // 
            this.OpenBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenBtn.Location = new System.Drawing.Point(513, 54);
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.Size = new System.Drawing.Size(75, 23);
            this.OpenBtn.TabIndex = 3;
            this.OpenBtn.Text = "Open";
            this.OpenBtn.UseVisualStyleBackColor = true;
            this.OpenBtn.Click += new System.EventHandler(this.OpenBtn_Click);
            // 
            // extentionsCmbBox
            // 
            this.extentionsCmbBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.extentionsCmbBox.FormattingEnabled = true;
            this.extentionsCmbBox.Location = new System.Drawing.Point(513, 14);
            this.extentionsCmbBox.Name = "extentionsCmbBox";
            this.extentionsCmbBox.Size = new System.Drawing.Size(156, 21);
            this.extentionsCmbBox.TabIndex = 2;
            this.extentionsCmbBox.SelectedIndexChanged += new System.EventHandler(this.extentionsCmbBox_SelectedIndexChanged);
            // 
            // FileName_textBox
            // 
            this.FileName_textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileName_textBox.Location = new System.Drawing.Point(84, 14);
            this.FileName_textBox.Name = "FileName_textBox";
            this.FileName_textBox.Size = new System.Drawing.Size(423, 20);
            this.FileName_textBox.TabIndex = 1;
            this.FileName_textBox.TextChanged += new System.EventHandler(this.FileName_textBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File name:";
            // 
            // DirTreeView
            // 
            this.DirTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.DirTreeView.Location = new System.Drawing.Point(116, 0);
            this.DirTreeView.MinimumSize = new System.Drawing.Size(200, 4);
            this.DirTreeView.Name = "DirTreeView";
            this.DirTreeView.Size = new System.Drawing.Size(298, 373);
            this.DirTreeView.TabIndex = 3;
            // 
            // FileTreeView
            // 
            this.FileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTreeView.Location = new System.Drawing.Point(417, 0);
            this.FileTreeView.MinimumSize = new System.Drawing.Size(200, 4);
            this.FileTreeView.Name = "FileTreeView";
            this.FileTreeView.ShowLines = false;
            this.FileTreeView.Size = new System.Drawing.Size(275, 373);
            this.FileTreeView.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button_MyComputer);
            this.panel2.Controls.Add(this.button_Personal);
            this.panel2.Controls.Add(this.button_Desktop);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(116, 373);
            this.panel2.TabIndex = 6;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(414, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 373);
            this.splitter2.TabIndex = 7;
            this.splitter2.TabStop = false;
            // 
            // button_Desktop
            // 
            this.button_Desktop.Location = new System.Drawing.Point(9, 12);
            this.button_Desktop.Name = "button_Desktop";
            this.button_Desktop.Size = new System.Drawing.Size(98, 72);
            this.button_Desktop.TabIndex = 0;
            this.button_Desktop.Text = "Desktop";
            this.button_Desktop.UseVisualStyleBackColor = true;
            // 
            // button_Personal
            // 
            this.button_Personal.Location = new System.Drawing.Point(9, 93);
            this.button_Personal.Name = "button_Personal";
            this.button_Personal.Size = new System.Drawing.Size(98, 72);
            this.button_Personal.TabIndex = 1;
            this.button_Personal.Text = "Personal";
            this.button_Personal.UseVisualStyleBackColor = true;
            // 
            // button_MyComputer
            // 
            this.button_MyComputer.Location = new System.Drawing.Point(9, 175);
            this.button_MyComputer.Name = "button_MyComputer";
            this.button_MyComputer.Size = new System.Drawing.Size(98, 72);
            this.button_MyComputer.TabIndex = 2;
            this.button_MyComputer.Text = "My Computer";
            this.button_MyComputer.UseVisualStyleBackColor = true;
            // 
            // OpenFileDialog
            // 
            this.AcceptButton = this.OpenBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(692, 467);
            this.Controls.Add(this.FileTreeView);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.DirTreeView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 300);
            this.Name = "OpenFileDialog";
            this.Text = "Open";
            this.Load += new System.EventHandler(this.OpenFileDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox FileName_textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OpenBtn;
        private System.Windows.Forms.ComboBox extentionsCmbBox;
        private System.Windows.Forms.TreeView DirTreeView;
        private System.Windows.Forms.TreeView FileTreeView;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Button button_MyComputer;
        private System.Windows.Forms.Button button_Personal;
        private System.Windows.Forms.Button button_Desktop;
    }
}