/* Keep the data ready for a 3D view using textures.
 * Keep 3 orthogonal stacks, so that on every angle view, one is best to show. */

using System.Threading.Tasks;

namespace Cell_Tool_3
{
    class Image3DProjection
    {
        public ushort[][] Zstack_1d_C0, Zstack_1dSmall_C0, Zstack_1d_C1, Zstack_1dSmall_C1;
        public ushort[][] Zstack_1d_segmented, Zstack_1dSmall_segmented;
        private byte[][][] Zstack8; // for 8 bits per pixel

        private TifFileInfo fi;
        public int factor = 1;
        public int sizeX, sizeY, sizeZ;
        public bool gpu = true;
        public bool drawPlane = false;

        public Image3DProjection(TifFileInfo fi, int downsample_factor) {
            this.fi = fi;

            factor = downsample_factor;

            sizeX = fi.sizeX;
            sizeY = fi.sizeY;
            sizeZ = fi.sizeZ;
            GenerateZstackDownsample();


            // 16 bits Channel 0 - full and downsampled
            Zstack_1d_C0 = new ushort[fi.sizeT][];
            Zstack_1dSmall_C0 = new ushort[fi.sizeT][];

            if (fi.sizeC > 1)
            {
                // 16 bits Channel 1 - full and downsampled
                Zstack_1d_C1 = new ushort[fi.sizeT][];
                Zstack_1dSmall_C1 = new ushort[fi.sizeT][];
            }

            // 16 bits Segmented - full and downsampled
            Zstack_1d_segmented = new ushort[fi.sizeT][];
            Zstack_1dSmall_segmented = new ushort[fi.sizeT][];
        
            

            Parallel.For(0, fi.sizeT, t =>
            {
               GenerateZstack1d(0, 0, t);
               if (fi.sizeC > 1)
                   GenerateZstack1d(0, 1, t);
            });
            factor = 1;
        }


        private void GenerateZstackDownsample()
        {
            sizeX = sizeX / factor;
            sizeY = sizeY / factor;
            sizeZ = sizeZ / factor;

           
            Zstack8 = new byte[fi.sizeZ * fi.sizeC * fi.sizeT + 1][][];

            for (int z = 0; z < fi.sizeZ * fi.sizeC * fi.sizeT; z++)
            {
               
                Zstack8[z/factor] = new byte[fi.sizeY/factor + 1][];
                for (int y = 0; y < fi.sizeY; y++)
                {
                    
                    Zstack8[z/factor][y/factor] = new byte[fi.sizeX / factor + 1];
                    for (int x = 0; x < fi.sizeX; x++)
                    {
                        if (fi.bitsPerPixel == 16)
                        {
                            
                        }
                        else
                            Zstack8[z/factor][y/factor][x/factor] = fi.image8bit[z][y][x];
                    }
                }
            }
        }
        public void GenerateZstack1d(int numRectangle, int C, int frame)
        {
            int ZCF = fi.sizeZ * fi.sizeC * frame + C;
            

            if (C == 0)
            {
                Zstack_1d_C0[frame] = new ushort[fi.sizeZ * fi.sizeY * fi.sizeX];
                Zstack_1dSmall_C0[frame] = new ushort[fi.sizeZ / factor * fi.sizeY / factor * fi.sizeX / factor];
                Zstack_1d_segmented[frame] = new ushort[fi.sizeZ * fi.sizeY * fi.sizeX];
                Zstack_1dSmall_segmented[frame] = new ushort[fi.sizeZ / factor * fi.sizeY / factor * fi.sizeX / factor];
            }

            else
            {
                Zstack_1d_C1[frame] = new ushort[fi.sizeZ * fi.sizeY * fi.sizeX];
                Zstack_1dSmall_C1[frame] = new ushort[fi.sizeZ / factor * fi.sizeY / factor * fi.sizeX / factor];
            }

            Parallel.For(0, fi.sizeZ, z =>
           {
               int Z_new = ZCF + z * fi.sizeC;
               for (int y = 0; y < fi.sizeY; y++)
               {
                   for (int x = 0; x < fi.sizeX; x++)
                   {
                       if (C == 0)
                       {
                           Zstack_1d_C0[frame][x + fi.sizeX * (y + fi.sizeY * z)] = 
                           fi.bitsPerPixel == 16 ? fi.image16bit[Z_new][y][x] : fi.image8bit[Z_new][y][x];

                           if (fi.image16bitFilter != null || fi.image8bitFilter != null)
                           Zstack_1d_segmented[frame][x + fi.sizeX * (y + fi.sizeY * z)] = 
                           fi.bitsPerPixel == 16 ? fi.image16bitFilter[Z_new][y][x] : fi.image8bitFilter[Z_new][y][x];
                       }
                       else
                           Zstack_1d_C1[frame][x + fi.sizeX * (y + fi.sizeY * z)] = 
                           fi.bitsPerPixel == 16 ? fi.image16bit[Z_new][y][x] : fi.image8bit[Z_new][y][x];
                   }
               }
           });

            Parallel.For(0, fi.sizeZ / factor, z =>
             {
                 int Z_new = ZCF / factor + z * fi.sizeC;
                 for (int y = 0; y < fi.sizeY / factor; y++)
                 {
                     for (int x = 0; x < fi.sizeX / factor; x++)
                     {
                         if (C == 0)
                         {
                             Zstack_1dSmall_C0[frame][x + (fi.sizeX / factor) * (y + (fi.sizeY / factor) * z)] =
                                 fi.bitsPerPixel == 16 ? fi.image16bit[Z_new * factor][y * factor][x * factor]:
                                 fi.image8bit[Z_new * factor][y * factor][x * factor];

                             if (fi.image16bitFilter != null || fi.image8bitFilter != null)
                                 Zstack_1dSmall_segmented[frame][x + (fi.sizeX / factor) * (y + (fi.sizeY / factor) * z)] =
                                 fi.bitsPerPixel == 16 ? fi.image16bitFilter[Z_new * factor][y * factor][x * factor]:
                                 fi.image8bitFilter[Z_new * factor][y * factor][x * factor];
                         }
                         else
                             Zstack_1dSmall_C1[frame][x + (fi.sizeX / factor) * (y + (fi.sizeY / factor) * z)] =
                                 fi.bitsPerPixel == 16 ? fi.image16bit[Z_new * factor][y * factor][x * factor]:
                                 fi.image8bit[Z_new * factor][y * factor][x * factor];
                     }
                 }
             });
        }

        public ushort[] GetImageForGPU(int numRectangle, int C)
        {

            return factor != 1 ?
                (C == 0 ? Zstack_1dSmall_C0[fi.frame] : Zstack_1dSmall_C1[fi.frame]) :
                (C == 0 ? Zstack_1d_C0[fi.frame] : Zstack_1d_C1[fi.frame]);

        }

        public ushort[] GetSegmentedImage()
        {
            return Zstack_1d_segmented[fi.frame];

        }
    }
}
