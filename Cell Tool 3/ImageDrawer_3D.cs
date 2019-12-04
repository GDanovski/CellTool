using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cell_Tool_3
{
    /// <summary>
    /// Includes events for 3D visualization
    /// </summary>
    class ImageDrawer_3D
    {
        /*
            GLControl1.MouseMove += ShowXYVal;
            GLControl1.MouseLeave += HideXYVal;
            GLControl1.MouseClick += GLControl1_MouseClick;
            GLControl1.MouseWheel += GLControl1_MouseWheel;
            GLControl1.MouseDoubleClick += HidePropertiesAndFileBrowser_DoubleClick;
            GLControl1.MouseDown += GLControl1_MouseDown;
            GLControl1.MouseMove += GLControl1_MouseMove;
            GLControl1.MouseUp += GLControl1_MouseUp;*/
        //program ID
        private int pgmID;
        //shaders IDs
        private int vsID;
        private int fsID;

        private int attribute_vcol;
        private int attribute_vpos;
        private int uniform_mview;

        //set Vertex Buffer Object
        private int vbo_position;
        private int vbo_color;
        private int vbo_mview;
        //buffers data
        private List<_3DTiffFileInfo> fi3Dlist;
        private Matrix4[] mviewdata;

        private int ibo_elements;

        // The time steps of each rotation; time starts when rotating and stops when rotation ceases;
        // Z refers to zoom
        float timeX = 0.0f;
        float timeY = 0.0f;

        private float speed = 0.5f;
        public float zoom = -30f;
        public float depth = 100f;

        private Vector2 MousePosition = new Vector2(-1, -1);
        /// <summary>
        /// Checks is the image 3D
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public bool isImage3D(TifFileInfo fi)
        {
            return fi.is3D;
        }

        /// <summary>
        /// Draw the image
        /// </summary>
        /// <param name="GLcontrol1"></param>
        /// <param name="fi"></param>       
        public void StartDrawing(GLControl GLcontrol1, TifFileInfo fi)
        {
            if (fi == null) return;
            GLcontrol1.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int numChannelToDraw = 0;
            int pointer = 0;
            for (int channel = 0; channel < fi.sizeC; channel++)
            {
                int numRectangleToDraw = 0;
                if (fi.tpTaskbar.ColorBtnList[channel].ImageIndex != 0) continue;

                if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                {
                    SetViewport(GLcontrol1, fi, numRectangleToDraw++, numChannelToDraw, channel, pointer++);
                }
                if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                {
                    SetViewport(GLcontrol1, fi, numRectangleToDraw++, numChannelToDraw, channel, pointer++);
                }
                numChannelToDraw++;
            }

            // Composite view
            if (fi.sizeC > 1 && fi.tpTaskbar.ColorBtnList[fi.sizeC].ImageIndex == 0)
            {

                if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                    for (int channel = 0; channel < fi.sizeC; channel++)
                    {
                        SetViewport(GLcontrol1, fi, 0, numChannelToDraw, channel, pointer++);
                        
                    }

                if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                    for (int channel = 0; channel < fi.sizeC; channel++)
                    {
                        SetViewport(GLcontrol1, fi, 1, numChannelToDraw, channel, pointer++);                       
                    }
            }

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);

            GL.Disable(EnableCap.DepthTest);

            GL.Flush();
            GLcontrol1.SwapBuffers();
        }
        private void SetViewport(GLControl GLcontrol1, TifFileInfo fi, int numRectangle, int numChannel, int channel,int pointer)
        {
            zoom = -100f;
            depth = 10000f;

            float fractionOfWindow = (float)(fi.sizeX * fi.zoom / GLcontrol1.Width);
            _3DTiffFileInfo fi3D = this.fi3Dlist[pointer];
            //this.fi3D.LoadShape(fi, channel);
            //fi3D.LoadColors(fi, channel);

            // Implement the actual transformation depending on the time flow in each direction
            mviewdata[0] =
                Matrix4.CreateRotationY(0.5f * timeX) *
                Matrix4.CreateRotationX(0.5f * timeY) *
                Matrix4.CreateTranslation(0f, 0f, zoom) *
                //Matrix4.CreateScale((float)(0.1 * fi.zoom), (float)(0.1 * fi.zoom), 1f) *
                Matrix4.CreatePerspectiveFieldOfView(1.3f, GLcontrol1.Width / (float)GLcontrol1.Height, 1f, depth);

            GL.UniformMatrix4(uniform_mview, false, ref mviewdata[0]);

            GL.UseProgram(pgmID);


            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(fi3D.vertdata.Length * Vector3.SizeInBytes), fi3D.vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);

            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(fi3D.coldata.Length * Vector3.SizeInBytes), fi3D.coldata, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(fi3D.indicedata.Length * sizeof(int)), fi3D.indicedata, BufferUsageHint.StaticDraw);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
            int X = numRectangle * (int)(fractionOfWindow * GLcontrol1.Width);
            int Y = (int)((1 - (1 + numChannel) * fractionOfWindow) * GLcontrol1.Height);
            int W = (int)(fractionOfWindow * GLcontrol1.Width);
            int H = (int)(fractionOfWindow * GLcontrol1.Height);

            GL.Viewport(X, Y, W, H);

            GL.DrawElements(BeginMode.Triangles, fi3D.indicedata.Length, DrawElementsType.UnsignedInt, 0);

        }
        public void Calculate3Dfi(TifFileInfo fi)
        {
            Clear3Dfi();
            
            for (int channel = 0; channel < fi.sizeC; channel++)
            {
                if (fi.tpTaskbar.ColorBtnList[channel].ImageIndex != 0) continue;

                if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                {
                    this.fi3Dlist.Add(CreateFi3D(fi, channel));
                }
                if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                {
                    this.fi3Dlist.Add(CreateFi3D(fi, channel));
                }
            }

            // Composite view
            if (fi.sizeC > 1 && fi.tpTaskbar.ColorBtnList[fi.sizeC].ImageIndex == 0)
            {

                if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                    for (int channel = 0; channel < fi.sizeC; channel++)
                    {
                        this.fi3Dlist.Add(CreateFi3D(fi, channel));
                    }

                if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                    for (int channel = 0; channel < fi.sizeC; channel++)
                    {
                        this.fi3Dlist.Add(CreateFi3D(fi, channel));
                    }
            }

        }
        private _3DTiffFileInfo CreateFi3D(TifFileInfo fi, int C)
        {
            _3DTiffFileInfo fi3D = new _3DTiffFileInfo(fi);
            fi3D.LoadShape(fi, C);
            fi3D.LoadColors(fi, C);
            return fi3D;
        }
        private void Clear3Dfi()
        {
            if (this.fi3Dlist != null)
            {
                foreach (_3DTiffFileInfo fi3D in this.fi3Dlist)
                    fi3D.Dispose();

                this.fi3Dlist.Clear();
            }
            else
                this.fi3Dlist = new List<_3DTiffFileInfo>();
        }

        public void initProgram(GLControl GLcontrol1, TifFileInfo fi)
        {
            GLcontrol1.MakeCurrent();
            //Create the program and get its ID
            this.pgmID = GL.CreateProgram();
            //create the shaders and get their IDs
            loadShader("vs.glsl", ShaderType.VertexShader, this.pgmID, out this.vsID);
            loadShader("fs.glsl", ShaderType.FragmentShader, this.pgmID, out this.fsID);
            //link the shaders in the program
            GL.LinkProgram(pgmID);
            //report if there are any errors
            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            this.attribute_vpos = GL.GetAttribLocation(this.pgmID, "vPosition");
            this.attribute_vcol = GL.GetAttribLocation(this.pgmID, "vColor");
            this.uniform_mview = GL.GetUniformLocation(this.pgmID, "modelview");

            if (this.attribute_vpos == -1 || this.attribute_vcol == -1 || this.uniform_mview == -1)
            {
                Console.WriteLine("Error binding attributes");
            }
            //create buffers
            GL.GenBuffers(1, out this.vbo_position);
            GL.GenBuffers(1, out this.vbo_color);
            GL.GenBuffers(1, out this.vbo_mview);
            GL.GenBuffers(1, out ibo_elements);

            this.mviewdata = new Matrix4[]{
                Matrix4.Identity
            };
            
        }
        private void loadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
        public void ClearProgram(GLControl GLcontrol1)
        {
            Clear3Dfi();
        }
        public void GLControl1_MouseClick(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {

        }
        public void GLControl1_MouseDown(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
        }
        public void GLControl1_MouseMove(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            if (Math.Abs(e.X - MousePosition.X) < 10 && Math.Abs(e.X - MousePosition.X) < 10) return;

            if (MousePosition.X == -1) return;

            if (e.X > MousePosition.X) { timeX += speed; }
            else { timeX -= speed; }
            if (e.Y > MousePosition.Y) { timeY += speed; }
            else { timeY -= speed; }

            StartDrawing(GLcontrol1, fi);

            MousePosition.X = e.X;
            MousePosition.Y = e.Y;

        }
        public void GLControl1_MouseUp(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            MousePosition = new Vector2(-1, -1);
        }

        class _3DTiffFileInfo : IDisposable
        {
            public int[] indicedata;
            public Vector3[] vertdata;
            public Vector4[] coldata;
            private const int numVerticesInPrimitiveShape = 8; // 8 vertices in a cube
            private const int numFacesInPrimitiveShape = 6; // 6 faces in a cube  
            private int numMicroIndicedata;
            public _3DTiffFileInfo(TifFileInfo fi)
            {
                int[] microIndicedata = new int[] {
                0, 7, 3,     0, 4, 7,     //front
                1, 2, 6,     6, 5, 1,     //back
                0, 2, 1,     0, 3, 2,     //left
                4, 5, 6,     6, 7, 4,     //right
                2, 3, 6,     6, 3, 7,     //top
                0, 1, 5,     0, 5, 4      //bottom
            };
                numMicroIndicedata = microIndicedata.Length;
                long pxlCount = fi.sizeX * fi.sizeY * fi.sizeZ;

                this.vertdata = new Vector3[pxlCount * numVerticesInPrimitiveShape];
                this.indicedata = new int[pxlCount * microIndicedata.Length];
                this.coldata = new Vector4[pxlCount * numVerticesInPrimitiveShape];


                for (int i = 0; i < this.indicedata.Length; i += microIndicedata.Length)
                {
                    Array.Copy(microIndicedata, 0, this.indicedata, i, microIndicedata.Length);

                    for (int y = 0; y < microIndicedata.Length; y++)

                        microIndicedata[y] += numVerticesInPrimitiveShape;
                }
            }
            public void Dispose()
            {
                indicedata = null;
                vertdata = null;
                coldata = null;
            }
            public void LoadShape(TifFileInfo fi, int C)
            {
                int startZ = fi.sizeZ * fi.sizeC * (fi.frame) + C;
                int VertexCount = 0;
                int value = 0;
                int length = 0;
                for (float z = 0, frame = startZ; z < fi.sizeZ; z += 1, frame += fi.sizeC)
                    for (float x = 0; x < fi.sizeX; x++)
                        for (float y = 0; y < fi.sizeY; y++)
                        {
                            // Load the colors - each of the 6 faces of a cube is colored by the voxel value
                            switch (fi.bitsPerPixel)
                            {
                                case 8:
                                    value = (int)fi.image8bit[(int)frame][(int)y][(int)x];
                                    break;
                                case 16:
                                    value = (int)fi.image16bit[(int)frame][(int)y][(int)x];
                                    break;
                            }
                            if ((value - fi.MinBrightness[C]) < 0) continue;

                            // Load the vertex locations
                            foreach (var val in getVertData(x - fi.sizeX / 2, y - fi.sizeY / 2, z - fi.sizeZ / 2, 0.25f))
                                this.vertdata[(int)VertexCount++] = val;

                            length++;
                        }

                // Delete unnecessary vertices
                //TODO create submasive
                Trim(length);
                /*
                for (int i = VertexCount + 1; i < vertdata.Length; i++)
                {
                    vertdata[i] = new Vector3();
                }*/
            }
            private void Trim(int length)
            {
                //TODO optimize this code
                int newIndicedataLength = length * numMicroIndicedata;
                int newVertdataLength = length * numVerticesInPrimitiveShape;
                int newColdataLength = length * numVerticesInPrimitiveShape;

                int[] newIndicedata = new int[newIndicedataLength];
                Vector3[] newVertdata = new Vector3[newVertdataLength];
                Vector4[] newColdata = new Vector4[newColdataLength];

                Array.Copy(this.indicedata,newIndicedata, newIndicedataLength);
                Array.Copy(this.vertdata, newVertdata, newVertdataLength);
                Array.Copy(this.coldata, newColdata, newColdataLength);

                this.indicedata = newIndicedata;
                this.coldata = newColdata;
                this.vertdata = newVertdata;
            }

            public void LoadColors(TifFileInfo fi, int C)
            {
                int startZ = fi.sizeZ * fi.sizeC * (fi.frame) + C;
                int colorCount = 0;
                int value = 0;
                float R = (float)(fi.LutList[C].R / 255f);
                float G = (float)(fi.LutList[C].G / 255f);
                float B = (float)(fi.LutList[C].B / 255f);
                Vector4 currentCol;

                for (float z = 0, frame = startZ; z < fi.sizeZ; z += 1, frame += fi.sizeC)
                    for (float x = 0; x < fi.sizeX; x++)
                        for (float y = 0; y < fi.sizeY; y++)
                        {
                            // Load the colors - each of the 6 faces of a cube is colored by the voxel value
                            //TODO 8bit images

                            switch (fi.bitsPerPixel)
                            {
                                case 8:
                                    value = (int)fi.image8bit[(int)frame][(int)y][(int)x];
                                    break;
                                case 16:
                                    value = (int)fi.image16bit[(int)frame][(int)y][(int)x];
                                    break;
                            }
                            if ((value - fi.MinBrightness[C]) < 0) continue;

                            currentCol = new Vector4(R, G, B, fi.adjustedLUT[C][value]);
                            
                            for (int i = 0; i < numFacesInPrimitiveShape; i++)
                            {
                                this.coldata[colorCount++] = currentCol;
                            }

                        }
            }
            private Vector3[] getVertData(float x, float y, float z, float size)
            {
                return new Vector3[]
                    {
                new Vector3(-size+x, -size+y,  -size+z),
                new Vector3(size+x, -size+y,  -size+z),
                new Vector3(size+x, size+y,  -size+z),
                new Vector3(-size+x, size+y,  -size+z),
                new Vector3(-size+x, -size+y,  size+z),
                new Vector3(size+x, -size+y,  size+z),
                new Vector3(size+x, size+y,  size+z),
                new Vector3(-size+x, size+y,  size+z)

                    };
            }
        }
    }
}