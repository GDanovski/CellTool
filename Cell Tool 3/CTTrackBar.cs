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
        class CTTrackBar
    {
        public ChangeValueControl Value = new ChangeValueControl();
        public Panel Panel = new Panel();
        public Label Name = new Label();
        public Panel NamePanel = new Panel();
        private Label maxLabel = new Label();

        public TrackBar TrackBar1 = new TrackBar();
        public TextBox TextBox1 = new TextBox();
        
        public Button ApplyBtn = new Button();
        private Button CancelBtn = new Button();
        private ToolTip TurnOnToolTip = new ToolTip();
        public void Initialize()
        {
            Panel.Height = 25;
            Panel.Dock = DockStyle.Top;
           
            Panel textBoxPanel = new Panel();
            textBoxPanel.Dock = DockStyle.Right;
            textBoxPanel.Width = 92;
            Panel.Controls.Add(textBoxPanel);

            //frames text box
            TextBox1.Width = 40;
            
            TextBox1.Location = new System.Drawing.Point(10, 0);
           TextBox1.BackColor = Color.White;
            TextBox1.ForeColor = Color.Black;
            textBoxPanel.Controls.Add(TextBox1);
            TextBox1.TextChanged += new EventHandler(TextBox1_TextChanged);
            TextBox1.LostFocus += new EventHandler(textBox1_FocusLost);
            TextBox1.KeyDown += new KeyEventHandler(textBox1_KeyPress);
            TextBox1.Tag = 
                "Use numbers to change value. \nApply changes by using enter keyboard key or apply button.";
            TextBox1.MouseHover += new EventHandler(Control_MouseOver);
            
            ApplyBtn.FlatAppearance.BorderSize = 0;
            ApplyBtn.FlatStyle = FlatStyle.Flat;
            ApplyBtn.Width = TextBox1.Height;
            ApplyBtn.Height = TextBox1.Height;
            ApplyBtn.Location = new System.Drawing.Point(10 + TextBox1.Width + 2, -1);
            ApplyBtn.Visible = false;
            ApplyBtn.Text = "";
            ApplyBtn.TextImageRelation = TextImageRelation.Overlay;
            ApplyBtn.Image = Properties.Resources.the_blue_tick_th;
            textBoxPanel.Controls.Add(ApplyBtn);
            ApplyBtn.Click += new EventHandler(Applybtn_Click);
            ApplyBtn.Tag = "Apply";
            ApplyBtn.MouseHover += new EventHandler(Control_MouseOver);

            CancelBtn.FlatAppearance.BorderSize = 0;
            CancelBtn.FlatStyle = FlatStyle.Flat;
            CancelBtn.Width = TextBox1.Height;
            CancelBtn.Height = TextBox1.Height;
            CancelBtn.Location = new System.Drawing.Point(ApplyBtn.Location.X + ApplyBtn.Width, -1);
            CancelBtn.Visible = false;
            CancelBtn.Text = "";
            CancelBtn.TextImageRelation = TextImageRelation.Overlay;
            CancelBtn.Image = Properties.Resources.CancelRed;
            CancelBtn.Click += new EventHandler(CancelBtn_Click);
            textBoxPanel.Controls.Add(CancelBtn);
            CancelBtn.Tag = "Cancel";
            CancelBtn.MouseHover += new EventHandler(Control_MouseOver);

            //Max Label
           
            maxLabel.Location = new Point(10 + TextBox1.Width + 2,2);
            textBoxPanel.Controls.Add(maxLabel);
            //Label

            NamePanel.Dock = DockStyle.Left;
            NamePanel.Width = 0;
            Panel.Controls.Add(NamePanel);
            
            Name.Text = "";
            Name.Location = new System.Drawing.Point(10, 2);
            NamePanel.Controls.Add(Name);
            //track bar
            TrackBar1.Dock = DockStyle.Fill;
            TrackBar1.Minimum = 1;
            TrackBar1.Maximum = 2;
            TrackBar1.TickStyle = TickStyle.None;
            TrackBar1.SmallChange = 1;
            TrackBar1.LargeChange = 1;
            TrackBar1.Value = 1;
            Panel.Controls.Add(TrackBar1);
            TrackBar1.BringToFront();
            TrackBar1.ValueChanged += new EventHandler(TrackBar1_ChangeValue);
            
       }
        
        public void BackColor(Color Color)
        {
            Panel.BackColor = Color;
            TrackBar1.BackColor = Color;
            //TextBox1.BackColor = Color;
            //TextBox1.BorderStyle = BorderStyle.None;
       }
        public void ButtonBackColor(Color Color)
        {
            ApplyBtn.BackColor = Color;
            ApplyBtn.ForeColor = Color;
            CancelBtn.BackColor = Color;
            CancelBtn.ForeColor = Color;
        }
        public void ForeColor(Color Color)
        {
            Panel.ForeColor = Color;
            //TextBox1.ForeColor = Color;
        }
        public void Refresh(int value, int minimum, int maximum)
        {
            bool focused = false;
            if (TrackBar1.Focused == true)
            {
                Panel.Focus();
                focused = true;
            }

            if (TrackBar1.Minimum != minimum) { TrackBar1.Minimum = minimum; }
            if (TrackBar1.Maximum != maximum)
            {
                TrackBar1.Value = TrackBar1.Minimum;
                TrackBar1.Maximum = maximum;
                maxLabel.Text = "(" + maximum.ToString() + ")";
            }
            //set value

            if (maximum < value) value = maximum;
            else if (minimum > value) value = minimum;

            TrackBar1.Value = value;
            TextBox1.Text = value.ToString();


            if (focused == true) TrackBar1.Focus();
            TrackBar1.Refresh();
            TextBox1.Refresh();
        }
        private void TrackBar1_ChangeValue(object sender, EventArgs e)
        {
            if(TrackBar1.Focused == false) { return; }

            TextBox1.Text = TrackBar1.Value.ToString();
            Value.ChangeValueFunction(TrackBar1.Value.ToString());

            if(ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (TextBox1.Focused == false) { return; }

            if (TextBox1.Text != TrackBar1.Value.ToString())
            {
                if (ApplyBtn.Visible == false) { ApplyBtn.Visible = true; }
                if (CancelBtn.Visible == false) { CancelBtn.Visible = true; }
            }
            else
            {
                if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
                if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
            }
        }

        private void textBox1_FocusLost(object sender, EventArgs e)
        {
            if(ApplyBtn.Focused == true | CancelBtn.Focused == true) { return; }
            TextBox1.Text = TrackBar1.Value.ToString();
            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            TextBox1.Text = TrackBar1.Value.ToString();
            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void textBox1_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFromTextBox1();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
           
        }
        private void Applybtn_Click(object sender, EventArgs e)
        {
            ApplyFromTextBox1();
        }
        private void ApplyFromTextBox1()
        {
            int val;
            try
            {
                val = int.Parse(TextBox1.Text);
            }
            catch
            {
                MessageBox.Show("Value is not number!");
                TextBox1.Focus();
                return;
            }
            if(val > TrackBar1.Maximum) { val = TrackBar1.Maximum; }
            if (val < TrackBar1.Minimum) { val = TrackBar1.Minimum; }
            TextBox1.Text = val.ToString();
            TrackBar1.Value = val;
           
            Value.ChangeValueFunction(TrackBar1.Value.ToString());

            if (ApplyBtn.Visible == true) { ApplyBtn.Visible = false; }
            if (CancelBtn.Visible == true) { CancelBtn.Visible = false; }
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
    }
}
