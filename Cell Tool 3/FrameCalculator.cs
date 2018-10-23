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
    class FrameCalculator
    {
        public int Frame(TifFileInfo fi)
        {
            //get Values
           
            int fr = fi.frame + 1;
            int Zstack = fi.zValue + 1;
            int ColorStack = fi.cValue + 1;

            int ColorStackCount = fi.sizeC;
            int ZstackCount = fi.sizeZ;
           
            //Calculate
            int newFr = (fr - 1) * ColorStackCount * ZstackCount +
                (Zstack * ColorStackCount - (ColorStackCount - ColorStack)) - 1;
           
            //Return results
            return newFr;
        }
        public int FrameC(TifFileInfo fi,int C)
        {
            //get Values

            int fr = fi.frame + 1;
            int Zstack = fi.zValue + 1;
            int ColorStack = C + 1;

            int ColorStackCount = fi.sizeC;
            int ZstackCount = fi.sizeZ;

            //Calculate
            int newFr = (fr - 1) * ColorStackCount * ZstackCount +
                (Zstack * ColorStackCount - (ColorStackCount - ColorStack)) - 1;

            //Return results
            return newFr;
        }
        public int[] FrameCalculateTZ(TifFileInfo fi, int C, int imageN)
        {
            int ColorStack = C + 1;
            int ColorStackCount = fi.sizeC;
            int ZstackCount = fi.sizeZ;
            for (int i = fi.cValue; i < fi.imageCount; i+= fi.sizeC)
                for (int fr = 1; fr <= fi.sizeT; fr++)
                    for (int Zstack = 1; Zstack <= ZstackCount; Zstack++)
                    {
                        //Calculate
                        int newFr = (fr - 1) * ColorStackCount * ZstackCount +
                            (Zstack * ColorStackCount - (ColorStackCount - ColorStack)) - 1;
                        //Return results
                        if (newFr == imageN)
                        {
                            return new int[] {fr-1, Zstack - 1 };
                        }
                    }
            return new int[] { 0, 0};
        }

        public static int[] GetDimmensionMatrix(TifFileInfo fi)
        {


            //if (fi.dimensionOrder == "XYCZT") return null;

            int[] res = new int[fi.imageCount];

            switch (fi.dimensionOrder)
            {
                case "XYZCT":
                    int[] samp = new int[fi.sizeC * fi.sizeZ];
                    int n = 0;
                    for(int c = 0; c<fi.sizeC; c++)
                        for (int z = c; z < samp.Length; z+= fi.sizeC,n++)
                        {
                            samp[z] = n;
                        }

                    for (int t = 0; t<fi.imageCount; t+= samp.Length)
                    {
                        Array.Copy(samp, 0, res, t, samp.Length);

                        for (int s = 0; s < samp.Length; s++)
                            samp[s] += samp.Length;
                    }

                    samp = null;
                    break;
                default:
                    for (int i = 0; i < fi.imageCount; i++)
                        res[i] = i;
                    break;
            }

            return res;
            
        }

    }
}
