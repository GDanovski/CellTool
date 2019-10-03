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

// Code located in this class is responsible for the security, 
// including the trial form and trial settings, administrative tool and account settings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
namespace Cell_Tool_3
{
    class Security
    {
        private string AdminPass = Properties.Settings.Default.AccPass[0];
        private string LicenseKey = "123456";
        private Boolean StartProgram = false;
        public Properties.Settings settings = Properties.Settings.Default;
        private DateTime date = DateTime.Today.Date;
        private Form TrialForm = new Form();
        private TextBox PassBox = new TextBox();
        private Boolean ChangeAdminPass = false;
        public void Initialize()
        {
            //settings.Reset();
            rescueSettings();
            if (settings.TrialActive == false)
            {
                settings.EndTrialDate = date.AddDays(31);
                settings.TrialActive = true;
                SaveSettings(settings);
            }
            StartProgram = true;
            return;
            isProgramBlocked();
            IsTrialFinished();
            if (IsTrialActive() == false)
            {
               TrialForm_Initialize();
            }
        }
        private void IsTrialFinished()
        {
            if (settings.TrialActive == false)
            {
                settings.EndTrialDate = date.AddDays(31);
                SaveSettings(settings);
            }
            else if(DateTime.Compare(settings.EndTrialDate,date) == -1)
            {
                // Initializes the variables to pass to the MessageBox.Show method.
                string message = "Do you want to restart trial period?";
                string caption = "Your trial version has expired!";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
              // Displays the MessageBox.
              result = MessageBox.Show(message, caption, buttons);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    //settings.Reset();
                    settings.TrialActive = false;
                    settings.EndTrialDate = date.AddDays(31);
                    SaveSettings(settings);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
       }
        private Boolean isProgramBlocked()
        {
            //Check how many time wrong pass has been entered
            IncorrectPassCounter();
            // Check is the program blocked
            Boolean answer = true;
            if (settings.BlockProgram == true)
            {
                MessageBox.Show("Program - blocked!");
                Environment.Exit(0);
            }
            else
            {
                answer = false;
            }
            return answer;
        }
        private void IncorrectPassCounter()
        {
            return;
            // Check is the program blocked
            if (settings.IncorrectPass > 2)
            {
                settings.BlockProgram = true;
                SaveSettings(settings);
            }
        }
        private Boolean IsTrialActive()
        {
            //Check is trial active
            Boolean answer = false;
            if (settings.TrialActive == true)
            {
                answer = true;
            }
             return answer;
                        
        }
        private void TrialForm_Initialize()
        {
            //Form properties
           
            TrialForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            TrialForm.Width = 390;
            TrialForm.Height = 119;
            TrialForm.Text = "Add License Key:";
            TrialForm.StartPosition = FormStartPosition.CenterScreen;
            TrialForm.WindowState = FormWindowState.Normal;
            TrialForm.Icon = Properties.Resources.CT_done;
            TrialForm.FormClosed += new FormClosedEventHandler(TrialForm_Closing);
            //text box properties
                       PassBox.Width = 350;
            PassBox.Height = 20;
            PassBox.UseSystemPasswordChar = true;
            PassBox.Location = new System.Drawing.Point(12, 12);
            PassBox.KeyDown += new KeyEventHandler(PressBox_Enter);
            TrialForm.Controls.Add(PassBox);
            //add button
            Button OkBtn = new Button();
            OkBtn.Text = "Activate";
            OkBtn.Width = 75;
            OkBtn.Height = 23;
            OkBtn.Location = new System.Drawing.Point(143, 45);
            OkBtn.Click += new EventHandler(OkBtn_Click);
            TrialForm.Controls.Add(OkBtn);
            // Show dialog
            PassBox_config();

            // TODO - change status label
            TrialForm.ShowDialog();
        }
        private void PassBox_config()
        {
            PassBox.Text = "";
            PassBox.Focus();
        }
        private void OkBtn_Click(object sender, EventArgs e)
        {
            OkEvent();
        }
        private void PressBox_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OkEvent();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            
        }
        private void OkEvent()
        {
            isProgramBlocked();
            if (PassBox.Text == LicenseKey)
            {
                settings.IncorrectPass = 0;
                settings.TrialActive = true;
                SaveSettings(settings);
                StartProgram = true;
                TrialForm.Close();
            }
            else if (PassBox.Text != "")
            {
                PassBox_config();
                settings.IncorrectPass += 1;
                SaveSettings(settings);
                MessageBox.Show("Wrong key!");
            }
        }
              
        private void TrialForm_Closing(object sender, EventArgs e)
        {
            if (StartProgram == false)
            {
                Environment.Exit(0);
            }
         }

        //Accaunt settings
        private Form AccForm = new Form();
        private Panel NewAccPanel = new Panel();
        private Panel PassPanel = new Panel();
        private Panel AccListPanel = new Panel();

        private Form ChangePassForm = new Form();
           
        //Index of acc!!!
        public int AccIndex = -1;
        //login
        private TextBox passTbox = new TextBox();
        private TextBox accTbox = new TextBox();
        //create acc
        //private TextBox adPassTbox = new TextBox();
        private TextBox RePassTbox = new TextBox();
        private TextBox passTbox1 = new TextBox();
        private TextBox accTbox1 = new TextBox();
        //Change Pass
        TextBox NewPassTextBox = new TextBox();
        TextBox oldPassTextBox = new TextBox();
        TextBox reNewPassTextBox = new TextBox();
        //Admin
        ListBox AccListBox = new ListBox();
        public void ChooseAccount()
        {
            // Acc Form Properties
            AccForm.Text = "Login";
            AccForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            AccForm.MaximizeBox = false;
            AccForm.MinimizeBox = false;
            AccForm.StartPosition = FormStartPosition.CenterScreen;
            AccForm.WindowState = FormWindowState.Normal;
            AccForm.Width = 200;
            AccForm.Height = 140;
            AccForm.Icon = Properties.Resources.CT_done;
            AccForm.FormClosing += new FormClosingEventHandler(AccForm_Closing);
                       
            //PassPanel
            {
                PassPanel.BackColor = Color.DimGray;
                PassPanel.ForeColor = Color.White;
                PassPanel.Dock = DockStyle.Fill;
                AccForm.Controls.Add(PassPanel);
            
                //labels
                Label accLabel = new Label();
                accLabel.Text = "Account:";
                accLabel.Width = 60;
                accLabel.Location = new System.Drawing.Point(10,12);
                PassPanel.Controls.Add(accLabel);

                Label passLabel = new Label();
                passLabel.Text = "Password:";
                passLabel.Width = 60;
                passLabel.Location = new System.Drawing.Point(10, 42);
                PassPanel.Controls.Add(passLabel);

                //text boxes
                accTbox.Location = new System.Drawing.Point(70, 10);
                accTbox.Width = 100;
                accTbox.KeyDown += new KeyEventHandler(Login_EnterEvent);
                PassPanel.Controls.Add(accTbox);
                AutoComenceTbox();

                passTbox.Location = new System.Drawing.Point(70, 40);
                passTbox.UseSystemPasswordChar = true;
                passTbox.Width = 100;
                passTbox.KeyDown += new KeyEventHandler(Login_EnterEvent);
                PassPanel.Controls.Add(passTbox);
                
                Button LoginAccBtn = new Button();
                LoginAccBtn.FlatStyle = FlatStyle.Standard;
                LoginAccBtn.BackColor = SystemColors.ButtonFace;
                LoginAccBtn.ForeColor = Color.Black;
                LoginAccBtn.Width = 80;
                LoginAccBtn.Location = new System.Drawing.Point(50, 70);
                LoginAccBtn.Text = "Login";
                LoginAccBtn.Click += new EventHandler(Login_Event);
                PassPanel.Controls.Add(LoginAccBtn);

            }
            //New Acc Panel
            {
                NewAccPanel.BackColor = Color.DimGray;
                NewAccPanel.ForeColor = Color.White;
                NewAccPanel.Dock = DockStyle.Fill;
                NewAccPanel.Visible = false;
                AccForm.Controls.Add(NewAccPanel);

                //labels
                Label accLabel = new Label();
                accLabel.Text = "Account:";
                accLabel.Width = 60;
                accLabel.Location = new System.Drawing.Point(10, 12);
                NewAccPanel.Controls.Add(accLabel);

                Label passLabel = new Label();
                passLabel.Text = "Password:";
                passLabel.Width = 60;
                passLabel.Location = new System.Drawing.Point(10, 42);
                NewAccPanel.Controls.Add(passLabel);

                Label repassLabel = new Label();
                repassLabel.Text = "Repeat:";
                repassLabel.Width = 60;
                repassLabel.Location = new System.Drawing.Point(10, 72);
                NewAccPanel.Controls.Add(repassLabel);
               
                //text boxes
               
                accTbox1.Location = new System.Drawing.Point(70, 10);
                accTbox1.Width = 100;
                accTbox1.KeyDown += new KeyEventHandler(create_EnterEvent);
                NewAccPanel.Controls.Add(accTbox1);

                passTbox1.Location = new System.Drawing.Point(70, 40);
                passTbox1.UseSystemPasswordChar = true;
                passTbox1.Width = 100;
                passTbox1.KeyDown += new KeyEventHandler(create_EnterEvent);
                NewAccPanel.Controls.Add(passTbox1);

                RePassTbox.Location = new System.Drawing.Point(70, 70);
                RePassTbox.UseSystemPasswordChar = true;
                RePassTbox.Width = 100;
                RePassTbox.KeyDown += new KeyEventHandler(create_EnterEvent);
                NewAccPanel.Controls.Add(RePassTbox);
                /*
                adPassTbox.Location = new System.Drawing.Point(70, 120);
                adPassTbox.UseSystemPasswordChar = true;
                adPassTbox.Width = 100;
                adPassTbox.KeyDown += new KeyEventHandler(create_EnterEvent);
                NewAccPanel.Controls.Add(adPassTbox);
                */
                Button BackBtn = new Button();
                BackBtn.FlatStyle = FlatStyle.Standard;
                BackBtn.BackColor = SystemColors.ButtonFace;
                BackBtn.ForeColor = Color.Black;
                BackBtn.Width = 70;
                BackBtn.Location = new System.Drawing.Point(10, 120);
                BackBtn.Text = "Back";
                BackBtn.Click += new EventHandler(BackAdminBtn_click);
                NewAccPanel.Controls.Add(BackBtn);

                Button LoginAccBtn = new Button();
                LoginAccBtn.FlatStyle = FlatStyle.Standard;
                LoginAccBtn.BackColor = SystemColors.ButtonFace;
                LoginAccBtn.ForeColor = Color.Black;
                LoginAccBtn.Width = 70;
                LoginAccBtn.Location = new System.Drawing.Point(100, 120);
                LoginAccBtn.Text = "Create";
                LoginAccBtn.Click += new EventHandler(create_Event);
                NewAccPanel.Controls.Add(LoginAccBtn);
            }
            //Admin Block
            {
                //Change acc settings
                //Panel
                AccListPanel.BackColor = Color.DimGray;
                AccListPanel.ForeColor = Color.White;
                AccListPanel.Dock = DockStyle.Fill;
                AccListPanel.Visible = false;
                AccForm.Controls.Add(AccListPanel);
                //Add ListBox
                AccListBox.Dock = DockStyle.Top;
                AccListBox.BackColor = Color.DimGray;
                AccListBox.ForeColor = Color.White;
                AccListBox.SelectionMode = SelectionMode.MultiExtended;
                AccListBox.Height = 85;
                foreach (string Str in settings.AccList)
                {
                    AccListBox.Items.Add(Str);
                }
                AccListPanel.Controls.Add(AccListBox);

                //Back Button
                Button BackBtn = new Button();
                BackBtn.FlatStyle = FlatStyle.Standard;
                BackBtn.BackColor = SystemColors.ButtonFace;
                BackBtn.ForeColor = Color.Black;
                BackBtn.Width = 50;
                BackBtn.Location = new System.Drawing.Point(10, 110);
                BackBtn.Text = "Back";
                BackBtn.Click += new EventHandler(BackBtn_click);
                AccListPanel.Controls.Add(BackBtn);
                //Delete Acc
                Button DelBtn = new Button();
                DelBtn.FlatStyle = FlatStyle.Standard;
                DelBtn.BackColor = SystemColors.ButtonFace;
                DelBtn.ForeColor = Color.Black;
                DelBtn.Width = 50;
                DelBtn.Location = new System.Drawing.Point(70, 110);
                DelBtn.Text = "Delete";
                DelBtn.Click += new EventHandler(DeleteAcc);
                AccListPanel.Controls.Add(DelBtn);
                //NewAccBtn
                Button NewAccBtn = new Button();
                NewAccBtn.FlatStyle = FlatStyle.Standard;
                NewAccBtn.BackColor = SystemColors.ButtonFace;
                NewAccBtn.ForeColor = Color.Black;
                NewAccBtn.Width = 50;
                NewAccBtn.Location = new System.Drawing.Point(130, 110);
                NewAccBtn.Text = "Create";
                NewAccBtn.Click += new EventHandler(newAccBtn_click);
                AccListPanel.Controls.Add(NewAccBtn);

                //AdminPassChangeBtn
                Button AdminPassChangeBtn = new Button();
                AdminPassChangeBtn.FlatStyle = FlatStyle.Standard;
                AdminPassChangeBtn.BackColor = SystemColors.ButtonFace;
                AdminPassChangeBtn.ForeColor = Color.Black;
                AdminPassChangeBtn.Width = 170;
                AdminPassChangeBtn.Location = new System.Drawing.Point(10, 85);
                AdminPassChangeBtn.Text = "Change Admin password";
                AdminPassChangeBtn.Click += new EventHandler(changeAdminPass_ShowDialog);
                AccListPanel.Controls.Add(AdminPassChangeBtn);
            }
            //Change Pass Form
            ChangePassForm.Text = "Login";
            ChangePassForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            ChangePassForm.MaximizeBox = false;
            ChangePassForm.MinimizeBox = false;
            ChangePassForm.StartPosition = FormStartPosition.CenterScreen;
            ChangePassForm.WindowState = FormWindowState.Normal;
            ChangePassForm.Width = 200;
            ChangePassForm.Height = 180;
            ChangePassForm.Icon = Properties.Resources.CT_done;
            
            //PassPanel
            {
                Panel ChPassPanel = new Panel();
                ChPassPanel.BackColor = Color.DimGray;
                ChPassPanel.ForeColor = Color.White;
                ChPassPanel.Dock = DockStyle.Fill;
                ChangePassForm.Controls.Add(ChPassPanel);
                //Labels
                Label NewPassLabel = new Label();
                NewPassLabel.Text = "New password:";
                NewPassLabel.Width = 60;
                NewPassLabel.Location = new System.Drawing.Point(10, 12);
                ChPassPanel.Controls.Add(NewPassLabel);

                Label reNewPassLabel = new Label();
                reNewPassLabel.Text = "Retype:";
                reNewPassLabel.Width = 60;
                reNewPassLabel.Location = new System.Drawing.Point(10, 42);
                ChPassPanel.Controls.Add(reNewPassLabel);

                Label oldPassLabel = new Label();
                oldPassLabel.Text = "Old password:";
                oldPassLabel.Width = 60;
                oldPassLabel.Location = new System.Drawing.Point(10, 72);
                ChPassPanel.Controls.Add(oldPassLabel);
                //Text Box

                NewPassTextBox.Width = 100;
                NewPassTextBox.Location = new System.Drawing.Point(80, 10);
                NewPassTextBox.UseSystemPasswordChar = true;
                NewPassTextBox.KeyDown += new KeyEventHandler(ChangePassTBox_KeyDown);
                ChPassPanel.Controls.Add(NewPassTextBox);

                reNewPassTextBox.Width = 100;
                reNewPassTextBox.Location = new System.Drawing.Point(80, 40);
                reNewPassTextBox.UseSystemPasswordChar = true;
                reNewPassTextBox.KeyDown += new KeyEventHandler(ChangePassTBox_KeyDown);
                ChPassPanel.Controls.Add(reNewPassTextBox);

                oldPassTextBox.Width = 100;
                oldPassTextBox.Location = new System.Drawing.Point(80, 70);
                oldPassTextBox.UseSystemPasswordChar = true;
                oldPassTextBox.KeyDown += new KeyEventHandler(ChangePassTBox_KeyDown);
                ChPassPanel.Controls.Add(oldPassTextBox);

                //Button
                Button ChangeBtn = new Button();
                ChangeBtn.FlatStyle = FlatStyle.Standard;
                ChangeBtn.BackColor = SystemColors.ButtonFace;
                ChangeBtn.ForeColor = Color.Black;
                ChangeBtn.Width = 70;
                ChangeBtn.Location = new System.Drawing.Point(50, 110);
                ChangeBtn.Text = "Change";
                ChangeBtn.Click += new EventHandler(ChangePassBtn_click);
                ChPassPanel.Controls.Add(ChangeBtn);
            }

            AccListPanel.VisibleChanged += Panel_VisibleChange;
            PassPanel.VisibleChanged += Panel_VisibleChange;
            NewAccPanel.VisibleChanged += Panel_VisibleChange;

            // TODO - change status label
            AccForm.ShowDialog();
        }
        private void Panel_VisibleChange(object sender, EventArgs e)
        {
            Panel pnl = (Panel)sender;

            foreach (Control ctr in pnl.Controls)
                ctr.Visible = pnl.Visible;

            pnl.Update();
            pnl.Invalidate();
            pnl.Refresh();

            AccForm.Update();
            AccForm.Invalidate();
            AccForm.Refresh();
            /*
            if (!((Panel)sender).Visible) return;

            AccForm.Controls.Clear();
            AccForm.Controls.Add((Panel)sender);*/
        }
        private void changeAdminPass_ShowDialog(object sender, EventArgs e)
        {
            ChangeAdminPass = true;
            ChangePassForm.ShowDialog();
        }
        private void AutoComenceTbox()
        {
            //Name of existing acc
            var AccStrList = new AutoCompleteStringCollection();
            foreach (string AutoStr in settings.AccList)
            {
                AccStrList.Add(AutoStr);
            }
            // Set textBox property
            accTbox.AutoCompleteCustomSource = AccStrList;
            accTbox.AutoCompleteMode = AutoCompleteMode.Append;
            accTbox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }
       
        private void newAccBtn_click(object sender,EventArgs e)
        {
            AccForm.Height = 200;
            AccForm.Text = "New";
            PassPanel.Visible = false;
            NewAccPanel.Visible = true;
        }
        private void BackAdminBtn_click (object sender, EventArgs e)
        {
            BackAdminBtn_click_Event();
        }
        private void BackAdminBtn_click_Event()
        {
            //Form Settings
            AccForm.Text = "Admin";
            //Text Box settings
            passTbox.Text = "";
            accTbox.Text = "";
            RePassTbox.Text = "";
            passTbox1.Text = "";
            accTbox1.Text = "";
            //Panels settings
            PassPanel.Visible = false;
            NewAccPanel.Visible = false;
            AccListPanel.Visible = true;
        }
        private void BackBtn_click(object sender, EventArgs e)
        {
            //Form Settings
            AccForm.Height = 140;
            AccForm.Text = "Login";
            //Text Box settings
            passTbox.Text = "";
            accTbox.Text = "";
            //adPassTbox.Text = "";
            RePassTbox.Text = "";
            passTbox1.Text = "";
            accTbox1.Text = "";
            //Auto commence
            AutoComenceTbox();
            //Panels settings
            NewAccPanel.Visible = false;
            AccListPanel.Visible = false;
            PassPanel.Visible = true;
        }
        private void Login_Event(object sender,EventArgs e)
        {
            LoginAcc();
        }
        private void Login_EnterEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode ==  Keys.Enter)
            {
            LoginAcc();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            
        }
        private void LoginAcc()
        {
            //AccIndex - login number
            //Check is it admin login
            if (accTbox.Text.ToUpper() == "Admin".ToUpper() & passTbox.Text == AdminPass)
            {
                Admin_Event();
                return;
            }
            //Check is Acc and Pass Correct
            for (int i = 0; i < settings.AccList.Count; i++)
            {
                if (settings.AccList[i].ToUpper() == accTbox.Text.ToUpper() & settings.AccPass[i] == passTbox.Text)
                {
                    AccIndex = i;
                    AccForm.Close();
                    return;
                }
            }
            MessageBox.Show("The account name or password are not correct!");

        }
        
        private void Admin_Event()
        {
            AccForm.Text = "Admin";
            AccListBox.Items.Clear();
            foreach (string Str in settings.AccList)
            {
                AccListBox.Items.Add(Str);
            }
            AccForm.Height = 180;
            
            PassPanel.Visible = false;
            NewAccPanel.Visible = false;
            AccListPanel.Visible = true;
        }
        private void CreateAccount()
        {
            //AccIndex - login number
                //Check are the name and pass ok
                if (accTbox1.Text == "")
                {
                    MessageBox.Show("The account name is not correct!");
                    return;
                }
                //Check is the account already existing
                foreach (string str in settings.AccList)
                {
                    if (str.ToUpper() == accTbox1.Text.ToUpper())
                    {
                        MessageBox.Show("The account name already exists!");
                        return;
                    }
                }
                //Check is the password correctly retyped
                if (passTbox1.Text != RePassTbox.Text)
                {
                    MessageBox.Show("New password is not correctly retyped!");
                    return;
                }
                //add acc and pass
                settings.AccList.Add(accTbox1.Text);
                settings.AccPass.Add(passTbox1.Text);
                AddSettings();
                SaveSettings(settings);

                //LogIn
                for (int i = 0; i < settings.AccList.Count; i++)
                {
                    if (settings.AccList[i] == accTbox1.Text)
                    {
                        AccIndex = i;
                        Admin_Event();
                        return;
                    }
                }

        }
        private void create_Event(object sender, EventArgs e)
        {
            CreateAccount();
        }
        private void create_EnterEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CreateAccount();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
           
        }
        private void AccForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (AccIndex == -1)
            {
                Environment.Exit(0);
            }
        }
        private void DeleteAcc(object sender, EventArgs e)
        {
            //Delete Accounts
            foreach (string name in AccListBox.SelectedItems)
            {
                Boolean deleted = false;
                int i = 1;
                while (deleted == false)
                {
                    if (i >= settings.AccList.Count)
                    {
                        deleted = true;
                    }
                    else if (name == settings.AccList[i])
                    {
                        settings.AccList.RemoveAt(i);
                        settings.AccPass.RemoveAt(i);
                        DeleteSettings(i);
                        SaveSettings(settings);
                        deleted = true;
                    }
                    i++;
                 }
             }
            //Refresh Acc List
            Admin_Event();
            AutoComenceTbox();
        }
       
        public void LogOut_event(object sender, EventArgs e)
        {
            //Delete all settings
            BackBtn_click(sender, e);
            //Start Dialog

            // TODO - change status label
            AccForm.ShowDialog();
        }
        public void ChangePass_event(object sender, EventArgs e)
        {
            NewPassTextBox.Text = "";
            reNewPassTextBox.Text = "";
            oldPassTextBox.Text = "";

            // TODO - change status label
            ChangePassForm.ShowDialog();
        }
        private void ChangePassTBox_KeyDown(object sender,KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangePass_Event();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
         
        }
        private void ChangePassBtn_click(object sender, EventArgs e)
        {
            ChangePass_Event();
        }
        private void ChangePassForm_clear()
        {
            NewPassTextBox.Text = "";
            reNewPassTextBox.Text = "";
            oldPassTextBox.Text = "";
            ChangePassForm.Close();
        }
        private void ChangePass_Event()
        {
           
            if (NewPassTextBox.Text == reNewPassTextBox.Text)
            {
                if (oldPassTextBox.Text == settings.AccPass[0] & ChangeAdminPass == true)
                {
                    ChangeAdminPass = false;
                    settings.AccPass[0] = NewPassTextBox.Text;
                    AdminPass = NewPassTextBox.Text;
                    SaveSettings(settings);
                    ChangePassForm_clear();
                    Admin_Event();
                }
                else if (oldPassTextBox.Text == settings.AccPass[AccIndex])
                {
                        settings.AccPass[AccIndex] = NewPassTextBox.Text;
                        SaveSettings(settings);
                        ChangePassForm_clear();
                }
                else
                {
                    MessageBox.Show("Wrong old password!");
                }
            }
            else
            {
                MessageBox.Show("New password is not retyped correctly!");
                
            }
        }
        private void DeleteSettings(int i)
        {
            try
            {
                //Remove acc value from each global variable
                settings.DataSourcesPanelVisible.RemoveAt(i);
                settings.DataSourcesPanelValues.RemoveAt(i);
                settings.VBoxVisible.RemoveAt(i);
                settings.TreeViewVisible.RemoveAt(i);
                settings.TreeViewSize.RemoveAt(i);
                settings.TreeViewContent.RemoveAt(i);
                settings.OldWorkDir.RemoveAt(i);
                settings.PropertiesPanelVisible.RemoveAt(i);
                settings.PropertiesPanelWidth.RemoveAt(i);
                settings.CustomColors.RemoveAt(i);
                //raw image
                settings.BandC.RemoveAt(i);
                settings.BandCVis.RemoveAt(i);
                settings.Meta.RemoveAt(i);
                settings.MetaVis.RemoveAt(i);
                //segmentation
                settings.SegmentLibPanelVis.RemoveAt(i);
                settings.SegmentDataPanelVis.RemoveAt(i);
                settings.SegmentHistPanelVis.RemoveAt(i);
                settings.SegmentTreshPanelVis.RemoveAt(i);
                settings.SegmentHistPanelHeight.RemoveAt(i);
                settings.SegmentSpotDetPanelVis.RemoveAt(i);
                //RoiManager
                settings.RoiManHeight.RemoveAt(i);
                settings.RoiManVis.RemoveAt(i);
                //Tracking
                settings.TrackingVis.RemoveAt(i);
                //Chart
                settings.CTChart_PropertiesVis.RemoveAt(i);
                settings.CTChart_SeriesVis.RemoveAt(i);
                settings.CTChart_SeriesHeight.RemoveAt(i);
                settings.CTChart_Functions.RemoveAt(i);
                settings.AutoProtocolSettings.RemoveAt(i);
                settings.ProtocolSettingsList.RemoveAt(i);
                //Results Extractor
                settings.ResultsExtractorFilters.RemoveAt(i);
                settings.SolverFunctions.RemoveAt(i);
                settings.ResultsExtractorSizes.RemoveAt(i);
                //HotKeys
                settings.HotKeys.RemoveAt(i);
                settings.SmartBtns.RemoveAt(i);
            }
            catch { }
            //Save Changes
            SaveSettings(settings);
        }
        private void AddSettings()
        {
            try
            {
                //Add new acc value for each global variable
                settings.DataSourcesPanelVisible.Add(settings.DataSourcesPanelVisible[0]);
                settings.DataSourcesPanelValues.Add(settings.DataSourcesPanelValues[0]);
                settings.VBoxVisible.Add(settings.VBoxVisible[0]);
                settings.TreeViewVisible.Add(settings.TreeViewVisible[0]);
                settings.TreeViewSize.Add(settings.TreeViewSize[0]);
                settings.TreeViewContent.Add(settings.TreeViewContent[0]);
                settings.OldWorkDir.Add(settings.OldWorkDir[0]);
                settings.PropertiesPanelWidth.Add(settings.PropertiesPanelWidth[0]);
                settings.PropertiesPanelVisible.Add(settings.PropertiesPanelVisible[0]);
                settings.CustomColors.Add(settings.CustomColors[0]);
                //raw image
                settings.BandC.Add(settings.BandC[0]);
                settings.BandCVis.Add(settings.BandCVis[0]);
                settings.Meta.Add(settings.Meta[0]);
                settings.MetaVis.Add(settings.MetaVis[0]);
                //segmentation
                settings.SegmentLibPanelVis.Add(settings.SegmentLibPanelVis[0]);
                settings.SegmentDataPanelVis.Add(settings.SegmentDataPanelVis[0]);
                settings.SegmentHistPanelVis.Add(settings.SegmentHistPanelVis[0]);
                settings.SegmentTreshPanelVis.Add(settings.SegmentTreshPanelVis[0]);
                settings.SegmentHistPanelHeight.Add(settings.SegmentHistPanelHeight[0]);
                settings.SegmentSpotDetPanelVis.Add(settings.SegmentSpotDetPanelVis[0]);
                //RoiManager
                settings.RoiManHeight.Add(settings.RoiManHeight[0]);
                settings.RoiManVis.Add(settings.RoiManVis[0]);
                //Tracking 
                settings.TrackingVis.Add(settings.TrackingVis[0]);
                //Chart
                settings.CTChart_PropertiesVis.Add(settings.CTChart_PropertiesVis[0]);
                settings.CTChart_SeriesVis.Add(settings.CTChart_SeriesVis[0]);
                settings.CTChart_SeriesHeight.Add(settings.CTChart_SeriesHeight[0]);
                settings.CTChart_Functions.Add(settings.CTChart_Functions[0]);
                //Protocols
                settings.AutoProtocolSettings.Add(settings.AutoProtocolSettings[0]);
                settings.ProtocolSettingsList.Add(settings.ProtocolSettingsList[0]);
                //Results Extractor
                settings.ResultsExtractorFilters.Add(settings.ResultsExtractorFilters[0]);
                settings.SolverFunctions.Add(settings.SolverFunctions[0]);
                settings.ResultsExtractorSizes.Add(settings.ResultsExtractorSizes[0]);
                //HotKeys
                settings.HotKeys.Add(settings.HotKeys[0]);
                settings.SmartBtns.Add(settings.SmartBtns[0]);
            }
            catch {}
            //Save Changes
            SaveSettings(settings);
        }
        private void rescueSettings()
        {
            //Remove acc value from each global variable
            rescue(settings.DataSourcesPanelVisible);
            rescue(settings.DataSourcesPanelValues);
            rescue(settings.VBoxVisible);
            rescue(settings.TreeViewVisible);
            rescue(settings.TreeViewSize);
            rescue(settings.TreeViewContent);
            rescue(settings.OldWorkDir);
            rescue(settings.PropertiesPanelVisible);
            rescue(settings.PropertiesPanelWidth);
            rescue(settings.CustomColors);
            //raw image
            rescue(settings.BandC);
            rescue(settings.BandCVis);
            rescue(settings.Meta);
            rescue(settings.MetaVis);
            //segmentation
            rescue(settings.SegmentLibPanelVis);
            rescue(settings.SegmentDataPanelVis);
            rescue(settings.SegmentHistPanelVis);
            rescue(settings.SegmentTreshPanelVis);
            rescue(settings.SegmentHistPanelHeight);
            rescue(settings.SegmentSpotDetPanelVis);
            //RoiManager
            rescue(settings.RoiManHeight);
            rescue(settings.RoiManVis);
            //Tracking
            rescue(settings.TrackingVis);
            //Chart
            rescue(settings.CTChart_PropertiesVis);
            rescue(settings.CTChart_SeriesVis);
            rescue(settings.CTChart_SeriesHeight);
            rescue(settings.CTChart_Functions);
            //Protocols
            rescue(settings.AutoProtocolSettings);
            rescue(settings.ProtocolSettingsList);
            //ResultsExtractor
            rescue(settings.ResultsExtractorFilters);
            rescue(settings.SolverFunctions);
            rescue(settings.ResultsExtractorSizes);
            //Hot Keys
            rescue(settings.HotKeys);
            rescue(settings.SmartBtns);
            //Save
            SaveSettings(settings);
        }
        private void rescue(System.Collections.Specialized.StringCollection vals)
        {
            int index = settings.AccList.Count;
            if (vals.Count >= index) return;

            string val = vals[0];
            int count = 0;

            while(count < index)
            {
                vals.Add(val);
                count++;
            }

        }
       public List<string> PrepareSettingsForExport()
        {
            List<string> vals = new List<string>();
            int i = AccIndex;

            //Add values
            vals.Add("DataSourcesPanelVisible>>" + settings.DataSourcesPanelVisible[i]);
            vals.Add("DataSourcesPanelValues>>" + settings.DataSourcesPanelValues[i]);
            vals.Add("VBoxVisible>>" + settings.VBoxVisible[i]);
            vals.Add("TreeViewVisible>>" + settings.TreeViewVisible[i]);
            vals.Add("TreeViewSize>>" + settings.TreeViewSize[i]);
            vals.Add("TreeViewContent>>" + settings.TreeViewContent[i]);
            vals.Add("OldWorkDir>>" + settings.OldWorkDir[i]);
            vals.Add("PropertiesPanelVisible>>" + settings.PropertiesPanelVisible[i]);
            vals.Add("PropertiesPanelWidth>>" + settings.PropertiesPanelWidth[i]);
            vals.Add("CustomColors>>" + settings.CustomColors[i]);
            //raw image
            vals.Add("BandC>>" + settings.BandC[i]);
            vals.Add("BandCVis>>" + settings.BandCVis[i]);
            vals.Add("Meta>>" + settings.Meta[i]);
            vals.Add("MetaVis>>" + settings.MetaVis[i]);
            //segmentation
            vals.Add("SegmentLibPanelVis>>" + settings.SegmentLibPanelVis[i]);
            vals.Add("SegmentDataPanelVis>>" + settings.SegmentDataPanelVis[i]);
            vals.Add("SegmentHistPanelVis>>" + settings.SegmentHistPanelVis[i]);
            vals.Add("SegmentTreshPanelVis>>" + settings.SegmentTreshPanelVis[i]);
            vals.Add("SegmentHistPanelHeight>>" + settings.SegmentHistPanelHeight[i]);
            vals.Add("SegmentSpotDetPanelVis>>" + settings.SegmentSpotDetPanelVis[i]);
            //RoiManager
            vals.Add("RoiManHeight>>" + settings.RoiManHeight[i]);
            vals.Add("RoiManVis>>" + settings.RoiManVis[i]);
            //Tracking
            vals.Add("TrackingVis>>" + settings.TrackingVis[i]);
            //Chart
            vals.Add("CTChart_PropertiesVis>>" + settings.CTChart_PropertiesVis[i]);
            vals.Add("CTChart_SeriesVis>>" + settings.CTChart_SeriesVis[i]);
            vals.Add("CTChart_SeriesHeight>>" + settings.CTChart_SeriesHeight[i]);
            vals.Add("CTChart_Functions>>" + settings.CTChart_Functions[i]);
            vals.Add("AutoProtocolSettings>>" + settings.AutoProtocolSettings[i]);
            vals.Add("ProtocolSettingsList>>" + settings.ProtocolSettingsList[i]);
            //Results Extractor
            vals.Add("ResultsExtractorFilters>>" + settings.ResultsExtractorFilters[i]);
            vals.Add("SolverFunctions>>" + settings.SolverFunctions[i]);
            vals.Add("ResultsExtractorSizes>>" + settings.ResultsExtractorSizes[i]);
            //HotKeys
            vals.Add("HotKeys>>" + settings.HotKeys[i]);
            vals.Add("SmartBtns>>" + settings.SmartBtns[i]);
            return vals;
        }
        public void ApplySettingsFromImport(string str)
        {
            string[] vals = str.Split(new string[] { ">>" }, StringSplitOptions.None);
            int i = AccIndex;

            switch (vals[0])
            {
                case "DataSourcesPanelVisible":
                    settings.DataSourcesPanelVisible[i] = vals[1];
                    break;
                case "DataSourcesPanelValues":
                    settings.DataSourcesPanelValues[i] = vals[1];
                    break;
                case "VBoxVisible":
                    settings.VBoxVisible[i] = vals[1];
                    break;
                case "TreeViewVisible":
                    settings.TreeViewVisible[i] = vals[1];
                    break;
                case "TreeViewSize":
                    settings.TreeViewSize[i] = vals[1];
                    break;
                case "TreeViewContent":
                    settings.TreeViewContent[i] = vals[1];
                    break;
                case "OldWorkDir":
                    settings.OldWorkDir[i] = vals[1];
                    break;
                case "PropertiesPanelVisible":
                    settings.PropertiesPanelVisible[i] = vals[1];
                    break;
                case "PropertiesPanelWidth":
                    settings.PropertiesPanelWidth[i] = vals[1];
                    break;
                case "CustomColors":
                    settings.CustomColors[i] = vals[1];
                    break;
                //raw image
                case "BandC":
                    settings.BandC[i] = vals[1];
                    break;
                case "BandCVis":
                    settings.BandCVis[i] = vals[1];
                    break;
                case "Meta":
                    settings.Meta[i] = vals[1];
                    break;
                case "MetaVis":
                    settings.MetaVis[i] = vals[1];
                    break;
                //segmentation
                case "SegmentLibPanelVis":
                    settings.SegmentLibPanelVis[i] = vals[1];
                    break;
                case "SegmentDataPanelVis":
                    settings.SegmentDataPanelVis[i] = vals[1];
                    break;
                case "SegmentHistPanelVis":
                    settings.SegmentHistPanelVis[i] = vals[1];
                    break;
                case "SegmentTreshPanelVis":
                    settings.SegmentTreshPanelVis[i] = vals[1];
                    break;
                case "SegmentHistPanelHeight":
                    settings.SegmentHistPanelHeight[i] = vals[1];
                    break;
                case "SegmentSpotDetPanelVis":
                    settings.SegmentSpotDetPanelVis[i] = vals[1];
                    break;
                //RoiManager
                case "RoiManHeight":
                    settings.RoiManHeight[i] = vals[1];
                    break;
                case "RoiManVis":
                    settings.RoiManVis[i] = vals[1];
                    break;
                //Tracking
                case "TrackingVis":
                    settings.TrackingVis[i] = vals[1];
                    break;
                //Chart
                case "CTChart_PropertiesVis":
                    settings.CTChart_PropertiesVis[i] = vals[1];
                    break;
                case "CTChart_SeriesVis":
                    settings.CTChart_SeriesVis[i] = vals[1];
                    break;
                case "CTChart_SeriesHeight":
                    settings.CTChart_SeriesHeight[i] = vals[1];
                    break;
                case "CTChart_Functions":
                    settings.CTChart_Functions[i] = vals[1];
                    break;
                case "AutoProtocolSettings":
                    settings.AutoProtocolSettings[i] = vals[1];
                    break;
                case "ProtocolSettingsList":
                    settings.ProtocolSettingsList[i] = vals[1];
                    break;
                //Results Extractor
                case "ResultsExtractorFilters":
                    settings.ResultsExtractorFilters[i] = vals[1];
                    break;
                case "SolverFunctions":
                    settings.SolverFunctions[i] = vals[1];
                    break;
                case "ResultsExtractorSizes":
                    settings.ResultsExtractorSizes[i] = vals[1];
                    break;
                //HotKeys
                case "HotKeys":
                    settings.HotKeys[i] = vals[1];
                    break;
                case "SmartBtns":
                    settings.SmartBtns[i] = vals[1];
                    break;
                default:
                    MessageBox.Show("Unknown setting: " + vals[0]);
                    break;
            }
        }
  
		public static void SaveSettings(Properties.Settings settings)
		{
			if ((System.Environment.OSVersion.Platform != PlatformID.MacOSX) &&
			    (System.Environment.OSVersion.Platform != PlatformID.Unix)) {
			
				settings.Save ();
			}
		}
	} 
}
