using System;
using System.Collections.Generic;
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
        private _3DTiffFileInfo fi3D;
        private Matrix4[] mviewdata;

        private int ibo_elements;
        private float time = 0.0f;
        public float zoom = -3f;
        public float depth = 40f;
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
            time += (float)0.05;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(this.fi3D.vertdata.Length * Vector3.SizeInBytes), this.fi3D.vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(this.fi3D.coldata.Length * Vector3.SizeInBytes), this.fi3D.coldata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);
            zoom = -30f;
            depth = (float)2 * fi.sizeZ;


            mviewdata[0] =
                Matrix4.CreateRotationY(0.55f * time) *
                Matrix4.CreateRotationX(0.15f * time) *
                Matrix4.CreateTranslation(0f, 0f, zoom) *
                Matrix4.CreatePerspectiveFieldOfView(1.3f, GLcontrol1.Width / (float)GLcontrol1.Height, 1f, depth);

            GL.UniformMatrix4(uniform_mview, false, ref mviewdata[0]);

            GL.UseProgram(pgmID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(this.fi3D.indicedata.Length * sizeof(int)), this.fi3D.indicedata, BufferUsageHint.StaticDraw);

            ///
            GL.Viewport(0, 0, GLcontrol1.Width, GLcontrol1.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);

            // GL.DrawArrays(BeginMode.Triangles, 0, 3);
            GL.DrawElements(BeginMode.Triangles, this.fi3D.indicedata.Length, DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);


            GL.Flush();
            GLcontrol1.SwapBuffers();

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

            this.fi3D = new _3DTiffFileInfo(fi);
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

        }
        public void GLControl1_MouseClick(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {

        }
        public void GLControl1_MouseDown(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {

        }
        public void GLControl1_MouseMove(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {

        }
        public void GLControl1_MouseUp(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {

        }

        class _3DTiffFileInfo
        {
            public int[] indicedata;
            public Vector3[] vertdata;
            public Vector3[] coldata;
            public _3DTiffFileInfo(TifFileInfo fi)
            {
                int[] microIndicedata = new int[]{
                //front
                0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };
                Vector3[] myColdata = new Vector3[] { new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f,  1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f,  1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f)};


                long pxlCount = fi.sizeX * fi.sizeY * fi.sizeZ;

                this.vertdata = new Vector3[pxlCount * 8];
                this.indicedata = new int[pxlCount * microIndicedata.Length];
                this.coldata = new Vector3[pxlCount * myColdata.Length];

                for (int i = 0; i < this.indicedata.Length; i += microIndicedata.Length)
                {
                    Array.Copy(microIndicedata, 0, this.indicedata, i, microIndicedata.Length);

                    for (int y = 0; y < microIndicedata.Length; y++)
                        microIndicedata[y] += myColdata.Length;
                }


                for (int i = 0; i < this.coldata.Length; i += myColdata.Length)
                {
                    Array.Copy(myColdata, 0, this.coldata, i, myColdata.Length);
                }
               
                for (float x = 0, pointer = 0; x < fi.sizeX; x++)
                    for (float y = 0; y < fi.sizeY; y++)
                        for (float z = 0; z < fi.sizeZ; z++)
                            foreach (var val in getVertData(x, y, z))
                                this.vertdata[(int)pointer++] = val;
               
            }
            private Vector3[] getVertData(float x, float y, float z)
            {
                return new Vector3[]
                    {
                new Vector3(-0.4f+x, -0.4f+y,  -0.4f+z),
                new Vector3(0.4f+x, -0.4f+y,  -0.4f+z),
                new Vector3(0.4f+x, 0.4f+y,  -0.4f+z),
                new Vector3(-0.4f+x, 0.4f+y,  -0.4f+z),
                new Vector3(-0.4f+x, -0.4f+y,  0.4f+z),
                new Vector3(0.4f+x, -0.4f+y,  0.4f+z),
                new Vector3(0.4f+x, 0.4f+y,  0.4f+z),
                new Vector3(-0.4f+x, 0.4f+y,  0.4f+z)
                    };
            }
        }
    }
}
