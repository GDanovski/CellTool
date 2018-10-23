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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class TabPage
    {
        public int FileTypeIndex;
        // add here different tab pages
        public TifFileInfo tifFI = null;
        public Panel CorePanel = new Panel();
       
        //save page options
        public bool Saved = true;
        public List<string> History = new List<string>();
        
        public void Save(ImageAnalyser IA)
        {
            TifFileInfo tifFI = this.tifFI;

            if (!tifFI.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }

            string dir = tifFI.Dir;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            //Add handlers to the backgroundworker
            //Reports when is finished

            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                FileEncoder.SaveTif(tifFI, dir, IA);
                //report progress
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    Saved = true;
                    if(tifFI != null)
                        tifFI.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
            });

            //Start background worker
            tifFI.available = false;
            IA.FileBrowser.StatusLabel.Text = "Saving Tif Image...";

            IA.EnabletrackBars(false);
            bgw.RunWorkerAsync();
            //continue when the sae is done
            while (bgw.IsBusy)
            {
                Application.DoEvents(); //This call is very important if you want to have a progress bar and want to update it
                                        //from the Progress event of the background worker.
                Thread.Sleep(10);     //This call waits if the loop continues making sure that the CPU time gets freed before
                                        //re-checking.
            }

            IA.EnabletrackBars(true);
        }
        public void Visible(Boolean status)
        {
            //hide if false and show if true
            if (status == true)
            {
                tifFI.tpTaskbar.TopBar.Visible = true;
            }
            else
            {
                tifFI.tpTaskbar.TopBar.Visible = false;
            }
        }
        public void Delete()
        {
            CorePanel.Dispose();
            tifFI.tpTaskbar.TopBar.Dispose();
            //Release resurse for Tif image
            tifFI.Delete();
            tifFI = null;
        }
        public void OpenFile (int FileType)
        {
            FileTypeIndex = FileType;
            switch (FileType)
            {
                case 0:
                    tifFI = new TifFileInfo();
                    break;
                default:
                    tifFI = new TifFileInfo();
                    break;
            }
        }
    }
}
