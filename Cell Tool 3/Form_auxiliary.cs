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
        public Form_auxiliary(Panel parentPanel, int X_offset, int Y_offset, int W_offset,int H_offset)
        {
            this.parentPanel = parentPanel;
            setInitialProperties(); // set the static properties of the Form
            startParentMonitor(X_offset, Y_offset, W_offset, H_offset); // upon a change in the parent's location and size, update the form accordingly 
            this.Hide();
            
        }

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
            this.TopMost = true;
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
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                while (!bgw.CancellationPending)
                {
                    Thread.Sleep(100);
                    ((BackgroundWorker)o).ReportProgress(0);
                }
                if (bgw.CancellationPending) { a.Cancel = true; }
            });

            // Upon a progress report, update the Form
            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    if (parentPanel.IsDisposed) { bgw.CancelAsync(); }
                    else
                    {
                        this.Location = parentPanel.PointToScreen(new Point(X_offset, Y_offset));
                        this.Size = new Size(parentPanel.Size.Width + W_offset, parentPanel.Size.Height + H_offset);
                        
                        foreach (Form formInstance in Application.OpenForms)
                        {
                            // Take the window state of the main form (e.g. minimized, maximized)
                            if (formInstance is CellToolMainForm)
                            {
                                if (formInstance.WindowState == FormWindowState.Minimized)
                                {
                                    this.WindowState = formInstance.WindowState;
                                }
                                else { this.WindowState = FormWindowState.Normal; }
                            }
                            

                        }
                        



                    } 
                }
            });
            //Start the background worker
            bgw.RunWorkerAsync();
        }
    } // end class
} // end namespace