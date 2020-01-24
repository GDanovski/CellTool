using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cell_Tool_3
{
    public class Form_GL_Test : Form
    {
        public Form_GL_Test()
        {

            Panel MainPanel = new Panel();
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.BackColor = Color.Gray;

            Button RefreshBtn = new Button();
            RefreshBtn.Text = "Refresh";
            RefreshBtn.Location = new Point(100, 500);
            

            MainPanel.Controls.Add(RefreshBtn);

            Label TestLabel = new Label();
            TestLabel.Text = "Test 1";
            TestLabel.Location = new Point(200, 50);
            TestLabel.Size = new Size(100, 80);
            MainPanel.Controls.Add(TestLabel);

            CTTrackBar TestBar1 = new CTTrackBar();
            TestBar1.Initialize();
            TestBar1.TextBox1.Text = "Test Bar";
            TestBar1.Panel.Location = new Point(320, 50);
            TestBar1.Panel.Size = new Size(100, 80);
            TestBar1.Panel.Visible = true;
            MainPanel.Controls.Add(TestBar1.Panel);

            this.Controls.Add(MainPanel);

            TestBar1.Panel.BringToFront();

            this.WindowState = FormWindowState.Maximized;
            this.Show();

            RefreshBtn.Click += new EventHandler(delegate (Object o, EventArgs e)
            {
                TestBar1.RefreshView();
            });
            
            
        }
        /*
        public void GLControl_Load(object sender, EventArgs e)
        {
            GLControl GLControl1 = sender as GLControl;
            GLControl1.MakeCurrent();

            
            //ImageTexture
            ContentPipe ImageTexture = new ContentPipe();
            ImageTexture.ReserveTextureID();
            ImageTexture.GenerateNumberTextures();
        }
        public void GLControl_Resize(object sender, EventArgs e)
        {
            
                
                GLControl GLControl1 = sender as GLControl;

                //Activate Control
                GLControl1.MakeCurrent();

                GL.Viewport(0, 0, GLControl1.Width, GLControl1.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
            
        }

        public void GLControl_Paint(object sender, EventArgs e)
        {
            //Global variables
            GLControl GLControl1 = sender as GLControl;
            GLDrawing_Start(GLControl1);

        }

        private void GLDrawing_Start(GLControl GLControl1)
        {
            if (GLControl1.Visible == false) { GLControl1.Visible = true; }

               
                //Activate Control
                GLControl1.MakeCurrent();
                GL.Disable(EnableCap.Texture2D);
                //Load background
                GL.ClearColor(Color.Yellow);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                //Prepare Projection
                GL.Ortho(0.0, (double)GLControl1.Width, (double)GLControl1.Height, 0.0, -1.0, 1.0);

                GL.MatrixMode(MatrixMode.Modelview);
                
                //Set viewpoint
                GL.Viewport(0, 0, GLControl1.Width, GLControl1.Height);

                GL.Disable(EnableCap.Blend);

                GLControl1.SwapBuffers();

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0.8f, 0.8f, 0.8f, 1f);

            int X = 10;
            int Y = 10;
            int W = 20;
            int H = 20;
            GL.Vertex2(X, Y);
            GL.Vertex2(X, H);
            GL.Vertex2(W, H);
            GL.Vertex2(W, Y);

            GL.End();

            GLControl1.SwapBuffers();

        }
        */
    }


}
