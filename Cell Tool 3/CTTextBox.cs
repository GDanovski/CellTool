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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Cell_Tool_3
{
    class CTTextBox
    {
        public ChangeValueControl Value = new ChangeValueControl();
        public Panel panel;

        public Label label;
        private TextBox tb;
        private Button acceptBtn;
        private Button cancelbtn;

        private ToolTip TurnOnToolTip = new ToolTip();
        #region Initialize
        public CTTextBox()
        {
            panel = new Panel();
            {
                panel.Height = 20;
                panel.Width = 90;
                panel.Resize += new EventHandler(delegate (object o, EventArgs a) 
                {
                    if (panel.Width < 60) panel.Width = 60;
                    if (tb != null) tb.Width = panel.Width - 40;
                });
            }

            tb = new TextBox();
            {
                tb.Text = "0";
                tb.Tag = "0";
                tb.Dock = DockStyle.Left;
                tb.Width = 50;

                tb.TextChanged += tb_TextChanged;
                tb.LostFocus += tb_LostFocus;
                tb.KeyDown += tb_EnterPress;

                panel.Controls.Add(tb);
            }

            acceptBtn = new Button();
            {
                Button btn = acceptBtn;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                btn.Image = Properties.Resources.the_blue_tick_th;
                btn.Tag = "Apply changes";
                btn.Width = 20;
                btn.Dock = DockStyle.Left;
                panel.Controls.Add(btn);
                btn.BringToFront();
                btn.Visible = false;
                btn.MouseHover += Control_MouseOver;
                btn.Click += acceptBtn_Click;
            }

            cancelbtn = new Button();
            {
                Button btn = cancelbtn;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                btn.Image = Properties.Resources.CancelRed;
                btn.Tag = "Cancel";
                btn.Width = 20;
                btn.Dock = DockStyle.Left;
                panel.Controls.Add(btn);
                btn.BringToFront();
                btn.Visible = false;
                btn.MouseHover += Control_MouseOver;
                btn.Click += cancelBtn_Click;
            }
        }
        #endregion Initialize

        #region Events
        public void Enable()
        {
            if (tb.Enabled == false)
                tb.Enabled = true;
        }
        public void Disable()
        {
            SetValue("0");
            if(tb.Enabled == true)
                tb.Enabled = false;
        }
        private void tb_LostFocus(object sender, EventArgs e)
        {
            if (acceptBtn.Focused || cancelbtn.Focused) return;

            tb.Text = (string)tb.Tag;

            acceptBtn.Visible = false;
            cancelbtn.Visible = false;
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        private void tb_EnterPress(object sender,  KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ValueChangeFunction();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void acceptBtn_Click(object sender, EventArgs e)
        {
            tb.Select();
            ValueChangeFunction();
        }
        private void cancelBtn_Click(object sender, EventArgs e)
        {
            tb.Select();
            tb.Text = (string)tb.Tag;

            acceptBtn.Visible = false;
            cancelbtn.Visible = false;
        }
        private void tb_TextChanged(object sender,EventArgs e)
        {
            if (tb.Focused == false) return;

            string oldVal = (string)tb.Tag;
            
            if(tb.Text != oldVal)
            {
                if (acceptBtn.Visible == false)
                {
                    acceptBtn.Visible = true;
                    acceptBtn.BringToFront();
                }

                if (cancelbtn.Visible == false)
                {
                    cancelbtn.Visible = true;
                    cancelbtn.BringToFront();
                }
            }
            else
            {
                if (acceptBtn.Visible == true) acceptBtn.Visible = false;
                if (cancelbtn.Visible == true) cancelbtn.Visible = false;
            }            
        }
        #endregion Events
        public void SetValue(string val)
        {
            bool focused = tb.Focused;
            if (tb.Focused == true) panel.Focus();

            tb.Text = val;
            tb.Tag = val;

            if (focused == true) tb.Focus();
            acceptBtn.Visible = false;
            cancelbtn.Visible = false;
        }
        private void ValueChangeFunction()
        {
            try
            {
                Int32.Parse(tb.Text);
            }
            catch
            {
                tb.Focus();
                MessageBox.Show("Value must be integer!");
                return;
            }

            if ((string)tb.Tag != tb.Text)
            {
                tb.Tag = tb.Text;
                Value.ChangeValueFunction(tb.Text);
            }

            acceptBtn.Visible = false;
            cancelbtn.Visible = false;
        }
    }
}
