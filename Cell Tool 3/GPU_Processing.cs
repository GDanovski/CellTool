using System;
using System.Runtime.InteropServices;
using Cloo;

namespace Cell_Tool_3
{
    class GPU_Processing
    {
        ComputePlatform platform;
        ComputeContext context;
        ComputeCommandQueue Rotate_queue;
        ComputeProgram program;
        ComputeKernel Rotate_Kernel;
        ComputeBuffer<ushort> InputImageBuffer_C0, InputImageBuffer_C1, InputImageBuffer_segmented;
        ComputeBuffer<ushort> OutputImageBuffer;
        GCHandle arrCHandle;

        public ushort[] result;
        ushort[] image1d_C0, image1d_C1, image1d_segmented;

        public GPU_Processing(ushort[] image1d_C0, ushort[] image1d_C1, ushort[] image1d_segmented)
        {

            this.image1d_C0 = image1d_C0;
            this.image1d_C1 = image1d_C1;
            this.image1d_segmented = image1d_segmented;
            // pick first platform
            platform = ComputePlatform.Platforms[0];
            // create context with all gpu devices
            context = new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            Rotate_queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            // load opencl source and
            // create program with opencl source
            program = new ComputeProgram(context, CalculateKernel);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            // load chosen kernel from program
            Rotate_Kernel = program.CreateKernel("RotateImage");

            InputImageBuffer_C0 = new ComputeBuffer<ushort>(context,
            ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, image1d_C0);

            if (image1d_C1 != null)
            {
                InputImageBuffer_C1 = new ComputeBuffer<ushort>(context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, image1d_C1);
            }
            if (image1d_segmented != null)
            {
                InputImageBuffer_segmented = new ComputeBuffer<ushort>(context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, image1d_segmented);
            }
        }
        static string CalculateKernel
        {
            get
            {
                // you could put your matrix algorithm here an take the result in array m3
                return @"
                kernel void RotateImage(global ushort* image1d, global ushort* sheared1d,
                                        double rotation00, double rotation01,
                                        double rotation10, double rotation11,
                                        double rotation20, double rotation21,
                                        int sizeX, int sizeY, int sizeZ, int maxSize)  {
                    
                    int xy = get_global_id(0);
                    int x = xy % sizeX;
                    int y = xy / sizeX;
                    
                    int halfX = sizeX / 2;
                    int halfY = sizeY / 2;
                    int halfZ = sizeZ / 2;
                    int halfMax = maxSize / 2;
                    
                    double p1 = y - halfY;
                    double p0 = x - halfX;
                    for (int z = 0; z < sizeZ; z += 1)
                    {
                        double p2 = z - halfZ;
                        int NewImgIndex = x + sizeX * (y + sizeY * z);
                        
                        int OriginalIndex = maxSize * 
                                            ((int)(p0 * rotation01 + p1 * rotation11 + p2 * rotation21 + halfMax)) + 
                                             (int)(p0 * rotation00 + p1 * rotation10 + p2 * rotation20 + halfMax);
                        if (image1d[NewImgIndex] > sheared1d[OriginalIndex]) sheared1d[OriginalIndex] = image1d[NewImgIndex];
                    }             
                }
        ";
            }
        }
        public void RotateImage(double[,] rotation,
            int sizeX, int sizeY, int sizeZ, int maxSize, int numRectangle, int channel)
        {
            result = new ushort[maxSize * maxSize];

            if (numRectangle == 0)
            {
                if (channel == 0) Rotate_Kernel.SetMemoryArgument(0, InputImageBuffer_C0);
                else Rotate_Kernel.SetMemoryArgument(0, InputImageBuffer_C1);
            }
            else
            {
                Rotate_Kernel.SetMemoryArgument(0, InputImageBuffer_segmented);
            }
            // TODO - why is result.Length not enough for the size? 
            OutputImageBuffer = new ComputeBuffer<ushort>(context,
                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.AllocateHostPointer, 16 * 1024 * 1024);

            Rotate_Kernel.SetMemoryArgument(1, OutputImageBuffer);// set the integer array
            Rotate_Kernel.SetValueArgument(2, rotation[0, 0]); // set the array size
            Rotate_Kernel.SetValueArgument(3, rotation[0, 1]); // set the array size
            Rotate_Kernel.SetValueArgument(4, rotation[1, 0]); // set the array size
            Rotate_Kernel.SetValueArgument(5, rotation[1, 1]); // set the array size
            Rotate_Kernel.SetValueArgument(6, rotation[2, 0]); // set the array size
            Rotate_Kernel.SetValueArgument(7, rotation[2, 1]); // set the array size

            Rotate_Kernel.SetValueArgument(8, sizeX); // set the array size
            Rotate_Kernel.SetValueArgument(9, sizeY); // set the array size
            Rotate_Kernel.SetValueArgument(10, sizeZ); // set the array size
            Rotate_Kernel.SetValueArgument(11, maxSize); // set the array size

            Rotate_queue.Execute(Rotate_Kernel, null, new long[] { sizeX * sizeY }, null, null);        // execute kernel
            Rotate_queue.Finish();

            arrCHandle = GCHandle.Alloc(result, GCHandleType.Pinned);

            Rotate_queue.Read<ushort>(OutputImageBuffer, true, 0, result.Length, arrCHandle.AddrOfPinnedObject(), null);

            arrCHandle.Free();
            OutputImageBuffer.Dispose();

        }
        public void Cleanup()
        {
            InputImageBuffer_C0.Dispose();
            InputImageBuffer_C1.Dispose();
            Rotate_Kernel.Dispose();
            program.Dispose();
            Rotate_queue.Dispose();
            context.Dispose();
        }
    }
}