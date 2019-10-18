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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Cell_Tool_3
{
    class Updater
    {
        public static void UpdateSettings()
        {
            
            //This will load settings from the previous version

            if (Properties.Settings.Default.UpdateSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
				Security.SaveSettings(Properties.Settings.Default);
            }
            //Load settings for MacOS/LinuxOS
            Helpers.Settings.LoadSettings();
            //Check for update
            CheckForUpdateWhenStarts();
        }
        private static void LicenseAgreement()
        {
            if (Properties.Settings.Default.ShowLicense)
            {
                Form msgForm = new Form();

                msgForm.Height = 600;
                msgForm.Width = 600;
                msgForm.Icon = Properties.Resources.CT_done;
                msgForm.Text = "CellTool License Agreement";

                RichTextBox rtb = new RichTextBox();
                rtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(link_LinkClicked);
                rtb.Dock = DockStyle.Fill;
                rtb.ReadOnly = true;
                rtb.Text = Properties.Resources.LicenseAgreementCT;

                msgForm.Controls.Add(rtb);

                Panel okBox = new Panel();
                okBox.Height = 40;
                okBox.Dock = DockStyle.Bottom;
                msgForm.Controls.Add(okBox);

                Button okBtn = new Button();
                okBtn.Text = "Agree";
                okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
                okBtn.ForeColor = System.Drawing.Color.Black;
                okBtn.Location = new System.Drawing.Point(20, 10);
                okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                okBox.Controls.Add(okBtn);

                Button cancelBtn = new Button();
                cancelBtn.Text = "Decline";
                cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
                cancelBtn.Location = new System.Drawing.Point(msgForm.Width - cancelBtn.Width - 40, 10);
                cancelBtn.ForeColor = System.Drawing.Color.Black;
                cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                okBox.Controls.Add(cancelBtn);

                okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    Properties.Settings.Default.ShowLicense = false;
                    Properties.Settings.Default.Save();
                    msgForm.Close();
                });

                cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    msgForm.Close();
                });

                // TODO - change status label
                msgForm.ShowDialog();
                msgForm.Dispose();
            }

            if (Properties.Settings.Default.ShowLicense)
            {
                Environment.Exit(0);
            }
        }
        private static void link_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
        private static void CheckForUpdateWhenStarts()
        {
            LicenseAgreement();
        }
  
    }
}
