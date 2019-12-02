using System;
using System.Collections.Generic;
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

        }
        public void initProgram()
        {

        }
        public void ClearProgram()
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
        
    }
}
