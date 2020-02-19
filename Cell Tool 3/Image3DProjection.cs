/* Keep the data ready for a 3D view using textures.
 * Keep 3 orthogonal stacks, so that on every angle view, one is best to show. */

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Cell_Tool_3
{
    class Image3DProjection
    {
        public ushort[][] Zstack_1d_C0, Zstack_1dSmall_C0, Zstack_1d_C1, Zstack_1dSmall_C1;
        public ushort[][] Zstack_1d_segmented, Zstack_1dSmall_segmented;

        private TifFileInfo fi;
        public int factor = 1;
        public int sizeX, sizeY, sizeZ;
        public bool gpu = true;
        public bool drawPlane = false;

        public Image3DProjection(TifFileInfo fi, int downsample_factor)
        {
            this.fi = fi;

            factor = downsample_factor;

            sizeX = fi.sizeX;
            sizeY = fi.sizeY;
            sizeZ = fi.sizeZ;

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
                                fi.bitsPerPixel == 16 ? fi.image16bit[Z_new * factor][y * factor][x * factor] :
                                fi.image8bit[Z_new * factor][y * factor][x * factor];

                            if (fi.image16bitFilter != null || fi.image8bitFilter != null)
                                Zstack_1dSmall_segmented[frame][x + (fi.sizeX / factor) * (y + (fi.sizeY / factor) * z)] =
                                fi.bitsPerPixel == 16 ? fi.image16bitFilter[Z_new * factor][y * factor][x * factor] :
                                fi.image8bitFilter[Z_new * factor][y * factor][x * factor];
                        }
                        else
                            Zstack_1dSmall_C1[frame][x + (fi.sizeX / factor) * (y + (fi.sizeY / factor) * z)] =
                                fi.bitsPerPixel == 16 ? fi.image16bit[Z_new * factor][y * factor][x * factor] :
                                fi.image8bit[Z_new * factor][y * factor][x * factor];
                    }
                }
            });
        }

        public ushort[] GetImage1D(int numRectangle, int C, int frame)
        {

            return factor != 1 ?
                (C == 0 ? Zstack_1dSmall_C0[frame] : Zstack_1dSmall_C1[frame]) :
                (C == 0 ? Zstack_1d_C0[frame] : Zstack_1d_C1[frame]);

        }

        public ushort[] GetSegmentedImage()
        {
            return Zstack_1d_segmented[fi.frame];

        }



        public void ProjectionEvent(TifFileInfo fi)
        {


            int final_ImageCount = fi.imageCount / fi.sizeZ;
            int ZC = (fi.sizeC * fi.sizeZ);
            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][][][] ResImage8 = new byte[final_ImageCount][][][];

                    for (int i = 0; i < final_ImageCount; i++)
                        ResImage8[i] = new byte[fi.sizeZ][][];
                    //order the frames for processing

                    for (int i = 0, frame = 0; i < fi.imageCount; i += ZC, frame += fi.sizeC)
                        for (int c = 0; c < fi.sizeC; c++)
                            for (int imageN = i + c, z = 0; imageN < i + ZC; imageN += fi.sizeC, z++)
                                ResImage8[frame + c][z] = fi.image8bitFilter[imageN];

                    fi.image8bit = ProjectionEvent(ResImage8, fi);
                    //
                    break;
                case 16:
                    ushort[][][][] ResImage16 = new ushort[final_ImageCount][][][];

                    for (int i = 0; i < final_ImageCount; i++)
                        ResImage16[i] = new ushort[fi.sizeZ][][];
                    //order the frames for processing

                    for (int i = 0, frame = 0; i < fi.imageCount; i += ZC, frame += fi.sizeC)
                        for (int c = 0; c < fi.sizeC; c++)
                            for (int imageN = i + c, z = 0; imageN < i + ZC; imageN += fi.sizeC, z++)
                                ResImage16[frame + c][z] = fi.image16bitFilter[imageN];

                    fi.image16bit = ProjectionEvent(ResImage16, fi);
                    break;
            }


            fi.imageCount = final_ImageCount;
            fi.sizeZ = 1;
            fi.zValue = 0;


        }



        private byte[][][] ProjectionEvent(byte[][][][] ResImage, TifFileInfo fi)
        {
            Parallel.For(0, ResImage.Length, i =>
            {
                byte[][][] images = ResImage[i];

                for (int x = 0; x < fi.sizeX; x++)
                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        byte val = 0;

                        foreach (byte[][] image in images)
                            if (image != null && val < image[y][x])
                                val = image[y][x];

                        images[0][y][x] = val;
                    }

            });

            byte[][][] newImage = new byte[ResImage.Length][][];
            for (int i = 0; i < ResImage.Length; i++)
                newImage[i] = ResImage[i][0];

            return newImage;
        }
        private ushort[][][] ProjectionEvent(ushort[][][][] ResImage, TifFileInfo fi)
        {
            Parallel.For(0, ResImage.Length, i =>
            {
                ushort[][][] images = ResImage[i];
                for (int x = 0; x < fi.sizeX; x++)
                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        ushort val = 0;

                        foreach (ushort[][] image in images)
                            if (image != null && val < image[y][x])
                                val = image[y][x];

                        images[0][y][x] = val;
                    }

            });

            ushort[][][] newImage = new ushort[ResImage.Length][][];
            for (int i = 0; i < ResImage.Length; i++)
                newImage[i] = ResImage[i][0];

            return newImage;
        }

    }
}
