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
    class PropertiesPanel_Item
    {
        private Panel PropertiesPanel;

        public Panel Panel = new Panel();
        public Panel Body = new Panel();
        public Label Name = new Label();
        public bool Resizable = false;

        private Panel NamePanel = new Panel();
        public Panel ResizePanel = new Panel();
        private Color TitleBackColor;

        private ToolTip TurnOnToolTip = new ToolTip();
        public int Height = 200;

        private bool resizing = false;
        private int oldY = 0;
        public void Initialize(Panel PropertiesPanel,bool ForRoiMan = false)
        {
            this.PropertiesPanel = PropertiesPanel;

            Panel.Dock = DockStyle.Top;
            Panel.Resize += new EventHandler(Panel_HeightChange);
            
            NamePanel.Dock = DockStyle.Top;
            NamePanel.Height = 21;
            Panel.Controls.Add(NamePanel);
            NamePanel.MouseHover += new EventHandler(Control_MouseOver);
            NamePanel.Click += new EventHandler(Control_Click);
            NamePanel.MouseEnter += new EventHandler(Title_HighLight);
            NamePanel.MouseLeave += new EventHandler(Title_Normal);

            Name.Width = 150;
            Name.Location = new System.Drawing.Point(10, 5);
            NamePanel.Controls.Add(Name);
            Name.MouseHover += new EventHandler(Control_MouseOver);
            Name.Click += new EventHandler(Control_Click);
            Name.MouseEnter += new EventHandler(Title_HighLight);
            Name.MouseLeave += new EventHandler(Title_Normal);

            Panel Resize1 = new Panel();
            Resize1.Tag = PropertiesPanel;
            Resize1.Dock = DockStyle.Bottom;
            Resize1.Height = 5;
            Panel.Controls.Add(Resize1);
            Resize1.MouseDown += new MouseEventHandler(Resize1_MouseDown);
            if(!ForRoiMan) Resize1.MouseUp += new MouseEventHandler(Resize1_MouseUp);
            else Resize1.MouseUp += new MouseEventHandler(RoiMan_Resize1_MouseUp);
            Resize1.MouseMove += new MouseEventHandler(Resize1_MouseMove);

            Body.Dock = DockStyle.Fill;
            Panel.Controls.Add(Body);

            ResizePanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
            ResizePanel.Visible = false;
            ResizePanel.BackColor = Color.FromArgb(100, 10, 10, 10);
            ResizePanel.Width = 5;
            PropertiesPanel.Controls.Add(ResizePanel);

            //reorder panels
           
            ResizePanel.BringToFront();
        }
        #region Title Panel Hendlers
       
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, "Show/Hide " + Name.Text);
        }
        //Add handler for resize
        private void Control_Click(object sender, EventArgs e)
        {
            Panel p = (Panel)PropertiesPanel.Parent;
            if (Panel.Height != 26)
            {
                Panel.Height = 26;
            }
            else
            {
                Panel.Height = Height;
            }
            //PropertiesPanel.Refresh();
            p.Refresh();
        }
        private void Title_HighLight(object sender, EventArgs e)
        {
            Panel ctr = NamePanel;
            int R = ctr.BackColor.R;
            int G = ctr.BackColor.G;
            int B = ctr.BackColor.B;
            if (R + 40 <= 255) { R += 40; } else { R = 255; }
            if (G + 40 <= 255) { G += 40; } else { G = 255; }
            if (B + 40 <= 255) { B += 40; } else { B = 255; }
            ctr.BackColor = Color.FromArgb(255, R, G, B);
            Name.BackColor = Color.FromArgb(255, R, G, B);
        }
        private void Title_Normal(object sender, EventArgs e)
        {
            NamePanel.BackColor = TitleBackColor;
            Name.BackColor = TitleBackColor;
        }
        #endregion

        #region Color options
        public Color BackColor (Color Color)
        {
            Panel.BackColor = Color;
            return Color;
        }
        public Color ForeColor(Color Color)
        {
            Panel.ForeColor = Color;
            Name.ForeColor = Color;
            return Color;
        }
        public Color TitleColor(Color Color)
        {
            Name.BackColor = Color;
            NamePanel.BackColor = Color;
            TitleBackColor = Color;
            return Color;
        }
        #endregion

        #region Resize Panel
       
        private void Panel_HeightChange(object sender, EventArgs e)
        {
            if (Panel.Height != 26)
            {
                Height = Panel.Height;
            }
        }
        private void Resize1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Resizable == false) { return; }
            if (Panel.Height > 26)
            {
                Panel pnl = sender as Panel;
                Panel PropertiesPanel = pnl.Tag as Panel;
                ResizePanel.Location = new Point(15, pnl.Location.Y + Panel.Location.Y + 21);
                ResizePanel.Width = PropertiesPanel.Width;
                ResizePanel.Height = 5;
                ResizePanel.BringToFront();
                ResizePanel.Visible = true;
                resizing = true;
                oldY = e.Y;
            }
        }
        private void RoiMan_Resize1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Resizable == false) { return; }
            if (resizing == true)
            {
                Panel pnl = sender as Panel;
                Panel PropertiesPanel = pnl.Tag as Panel;

                Panel.Height = ResizePanel.Location.Y
                    - (Panel.Location.Y + 21) + 200;
                if (Panel.Height < 38) { Panel.Height = 40; }
                Height = Panel.Height;
                ResizePanel.Visible = false;
                resizing = false;
                pnl.Cursor = Cursors.Default;
                
            }
        }
        private void Resize1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Resizable == false) { return; }
             if (resizing == true)
            {
                Panel pnl = sender as Panel;
                Panel PropertiesPanel = pnl.Tag as Panel;
               
                Panel.Height = ResizePanel.Location.Y 
                    - (Panel.Location.Y + 21);
                if(Panel.Height < 38) { Panel.Height = 40; }
                Height = Panel.Height;
                ResizePanel.Visible = false;
                resizing = false;
                pnl.Cursor = Cursors.Default;
            }
        }
        private void Resize1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Resizable == false) { return; }
            Panel pnl = sender as Panel;
            if (resizing == true)
            {
                int razlika = ResizePanel.Location.Y - (oldY - e.Y);
                Panel PropertiesPanel = pnl.Tag as Panel;
                if (razlika > Panel.Location.Y + 59)
                {
                    oldY = e.Y;
                    ResizePanel.Location = new System.Drawing.Point(15, razlika);
                    ResizePanel.BringToFront();
                }
                 ResizePanel.Visible = true;
            }
            else if (Panel.Height != 26)
            {
                pnl.Cursor = Cursors.SizeNS;
                ResizePanel.Visible = false;
            }
            else
            {
                pnl.Cursor = Cursors.Default;
                ResizePanel.Visible = false;
            }
        }
        #endregion
    }
    
}
