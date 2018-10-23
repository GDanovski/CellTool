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

namespace Cell_Tool_3
{
    //create new control
    class ChangeValueControl
    {
        //add event handler
        public event ChangedValueEventHandler Changed;
        //function for changing value
        public void ChangeValueFunction(string Value)
        {
            if (Changed != null)
                Changed(this, new ChangeValueEventArgs(Value));
        }
    }
    //declare event handler
    public delegate void ChangedValueEventHandler(object sender, ChangeValueEventArgs e);
    //declare new event arg
    public class ChangeValueEventArgs : EventArgs
    {
        private string m_Data;
        public ChangeValueEventArgs(string myData)
        {
            m_Data = myData;
        }
        public string Value { get { return m_Data; } }
    }
}
