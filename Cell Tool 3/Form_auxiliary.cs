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

/*
 This class creates a Windows.Form with a fixed position and size.
 The motivation is that on a Mac-OS, GLControl objects behave better
 when each is tehtered to a separate form - this allows better control
 of position on the screen.
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    public class Form_auxiliary : Form
    {
        private System.ComponentModel.IContainer components;
        private BackgroundWorker bgw = new BackgroundWorker(); // for continuously checking for size changes
        private Panel parentPanel; // the panel whose properties this form will use

        private bool last_state_visible = true;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        /*
         * Receive the parent Panel, whose location and size this form will continuously assume.
         * Receive the location and position offsets, relative to the parent panel.
         * Remove the title bar and disable resizing and maximization.
         */
        public Form_auxiliary(Panel parentPanel, int X_offset, int Y_offset, int W_offset, int H_offset, string Name)
        {
            this.Name = Name;
            this.parentPanel = parentPanel;
            setInitialProperties(); // set the static properties of the Form
            startParentMonitor(X_offset, Y_offset, W_offset, H_offset); // upon a change in the parent's location and size, update the form accordingly 
            this.Hide();

        }

        /* When an external caller wants to hide or show the form, they must call SetVisible(false) instead of Hide();
         * SetVisible(true) instead of Show(); Then the background worker will take this into account and decide whether to hide or show.
         */
        public void SetVisibleState(bool VisibleState) { this.last_state_visible = VisibleState; }

        private void setInitialProperties()
        {
            // Set the scale and size properties, initially to 0
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Location = new Point(0, 0);
            this.Size = new Size(0, 0);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ResumeLayout(false);

            // Fix the position and disable moving; remove title bar and set on top
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = false;
            this.ControlBox = false;
            this.Text = null;
            //this.TopMost = true;

            

            
        //if (this.Name == "RawImage" || this.Name == "Extractor") { this.TopMost = true;  }
        //else { this.TopMost = false; }


    }

       
        /*
         * Change the location and size of the form according to the given Panel
         * X, Y, width and height offsets are relative to those of the parent panel
         */
        public void startParentMonitor(int X_offset, int Y_offset, int W_offset, int H_offset)
        {

            // Make sure the bgw can be stopped, and is reporting progress
            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            // On every time interval, report progress to trigger size updates, until canceled
            bgw.DoWork += delegate (Object o, DoWorkEventArgs a)
            {
                while (!bgw.CancellationPending)
                {
                    Thread.Sleep(100);
                    ((BackgroundWorker)o).ReportProgress(0);
                }
                if (bgw.CancellationPending) { a.Cancel = true; }
            };

            // Upon a progress report, update the Form
            bgw.ProgressChanged += delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    if (parentPanel.IsDisposed) { bgw.CancelAsync(); }
                    else
                    {
                        this.Location = parentPanel.PointToScreen(new Point(X_offset, Y_offset));
                        this.Size = new Size(parentPanel.Size.Width + W_offset, parentPanel.Size.Height + H_offset);



                        this.SetWindowState();

                    } // end if panel is disposed
                } // end if progress reported
            };
            //Start the background worker
            bgw.RunWorkerAsync();
        }

        
        private void SetWindowLevels()
        {

            Form MainForm = getMainForm();
            Form ImgForm = getFormByName("RawImage");
            Form BrigtnessForm = getFormByName("Brightness");
            Form SegmentationForm = getFormByName("Segmentation");
            Form ExtractorForm = getFormByName("Extractor");

            foreach (Form formInstance in Application.OpenForms)
            {
                if (formInstance is Form_auxiliary)
                {
                    if (formInstance.ContainsFocus) { MainForm.Focus(); }
                }
            }
            if (this.Name.Contains("Extractor"))
            {
                if (((Form_auxiliary)ImgForm).last_state_visible == false)
                {
                    this.last_state_visible &= true;
                }
                else
                {
                    this.last_state_visible &= false;
                }
            }
            if (MainForm.ContainsFocus && last_state_visible && parentPanel.Visible) { this.Show(); }
            else { this.Hide(); }

        }

        private void SetWindowState()
        {
            if (getMainForm().WindowState == FormWindowState.Minimized) { this.Hide(); }
            else { this.SetWindowLevels(); }
            /*
            if (ApplicationActive() && last_state_visible) {
                this.Show();
            } else { this.Hide();  }
            */
        }

        /*
         * 
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        public bool ApplicationActive()
        {
            bool active = false;
            IntPtr foregroundWindow = GetForegroundWindow();

            foreach (Form formInstance in Application.OpenForms)
            {
                active |= (foregroundWindow == formInstance.Handle);
            }

            Console.WriteLine(active);
            return active;
        }
        */

        private Form getFormByName(string givenName)
        {
            Form pointerForm = null;
            foreach (Form formInstance in Application.OpenForms)
            {
                if (formInstance.Name.Equals(givenName)) { pointerForm = formInstance; }
            }
            return pointerForm;
        }

        private Form getMainForm()
        {
            Form mainForm = null;
            foreach (Form formInstance in Application.OpenForms)
            {
                if (formInstance is CellToolMainForm) { mainForm = formInstance; }
            }
            return mainForm;
        }




    } // end class
} // end namespace