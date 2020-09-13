using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace SecondTryFullScreen
{


    public partial class Form1 : Form
    {
        public Form form2;
        public Bitmap target;
        public Rectangle rectBounding;
        public float[] Negative;
        public float[] Blueinverse;
        private Form formformag;
        public bool updatemag = false;

        private IntPtr hwndMag;
        private float magnification;
        private RECT magWindowRect = new RECT();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Negative = new float[] {
                -1.0f,  0.0f,  0.0f,  0.0f,  0.0f ,
                0.0f,  -1.0f,  0.0f,  0.0f, 0.0f,
                0.0f,  0.0f,  -1.0f, 0.0f,  0.0f ,
                0.0f,  0.0f,  0.0f,  1.0f,  0.0f ,
                1.0f,  1.0f,  1.0f,  0.0f,  1.0f};

            Blueinverse = new float[] {
                0.0f,  0.0f,  -0.3f,  0.0f,  0.0f ,
                0.0f,  0.0f,  -0.6f,  0.0f, 0.0f,
                0.0f,  0.0f,  -0.1f, 0.0f,  0.0f ,
                0.0f,  0.0f,  0.0f,  1.0f,  0.0f ,
                0.0f,  0.0f,  1.0f,  0.0f,  1.0f};
            NativeMethods.MagInitialize();
           // NativeMethods.MagSetFullscreenColorEffect(Blueinverse);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form formformag = new Form();
            updatemag = true;
            magnification = 1.0f;
            this.formformag = formformag;
            SetupMagnifier();


        }

 
        public void SetupMagnifier()
        {
            Blueinverse = new float[] {
                0.0f,  0.0f,  -0.3f,  0.0f,  0.0f ,
                0.0f,  0.0f,  -0.6f,  0.0f, 0.0f,
                0.0f,  0.0f,  -0.1f, 0.0f,  0.0f ,
                0.0f,  0.0f,  0.0f,  1.0f,  0.0f ,
                0.0f,  0.0f,  1.0f,  0.0f,  1.0f};

            IntPtr hInst;
            hInst = NativeMethods.GetModuleHandle(null);
            formformag.AllowTransparency = true;
            formformag.TransparencyKey = Color.Empty;
            formformag.Opacity = 255;
            formformag.FormBorderStyle = FormBorderStyle.None;
            formformag.WindowState = FormWindowState.Maximized;
            int initialStyle = NativeMethods.GetWindowLong(formformag.Handle, -20);
            NativeMethods.SetWindowLong(formformag.Handle, -20, initialStyle | 0x80000 | 0x20);



            NativeMethods.GetClientRect(formformag.Handle, ref magWindowRect);
            hwndMag = NativeMethods.CreateWindow((int)ExtendedWindowStyles.WS_EX_CLIENTEDGE, NativeMethods.WC_MAGNIFIER,
                "MagnifierWindow", (int)WindowStyles.WS_CHILD | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR |
                (int)WindowStyles.WS_VISIBLE,
                magWindowRect.left, magWindowRect.top, magWindowRect.right, magWindowRect.bottom, formformag.Handle, IntPtr.Zero, hInst, IntPtr.Zero);


            Transformation matrix = new Transformation(magnification);
            NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
            NativeMethods.MagSetColorEffect(hwndMag, Negative);
           formformag.Show();


            Rectangle rectBounding = new Rectangle();
            rectBounding.X = 0;
            rectBounding.Y = 0;
            rectBounding.Width = 1920;
            rectBounding.Height = 1080;
            Bitmap target = new Bitmap(rectBounding.Width, rectBounding.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(rectBounding.Width, rectBounding.Height));
            }


            target.Save("foo2.png", System.Drawing.Imaging.ImageFormat.Png);


            // HBITMAP hBitmap =(HBITMAP)hwndMag
            //Bitmap b = Bitmap.FromHbitmap(hBitmap);
            //ForDrawToBitmap(b, new Rectangle(0, 0, 1080, 1920));
            //b.Save("Test.bmp");



        }

        public virtual void UpdateMaginifier()
        {
           /* if ((!initialized) || (hwndMag == IntPtr.Zero))
                return;*/

            POINT mousePoint = new POINT();
            RECT sourceRect = new RECT();

            NativeMethods.GetCursorPos(ref mousePoint);

            int width = (int)((magWindowRect.right - magWindowRect.left) / magnification);
            int height = (int)((magWindowRect.bottom - magWindowRect.top) / magnification);

            sourceRect.left = mousePoint.x - width / 2;
            sourceRect.top = mousePoint.y - height / 2;


            // Don't scroll outside desktop area.
            if (sourceRect.left < 0)
            {
                sourceRect.left = 0;
            }
            if (sourceRect.left > NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width)
            {
                sourceRect.left = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width;
            }
            sourceRect.right = sourceRect.left + width;

            if (sourceRect.top < 0)
            {
                sourceRect.top = 0;
            }
            if (sourceRect.top > NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height)
            {
                sourceRect.top = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height;
            }
            sourceRect.bottom = sourceRect.top + height;

            // Set the source rectangle for the magnifier control.
            NativeMethods.MagSetWindowSource(hwndMag, sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            NativeMethods.SetWindowPos(formformag.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);

            // Force redraw.
            NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, true);
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            while (updatemag == true)
            {

                UpdateMaginifier();



                Rectangle rectBounding = new Rectangle();
                rectBounding.X = 0;
                rectBounding.Y = 0;
                rectBounding.Width = 1920;
                rectBounding.Height = 1080;
                Bitmap target = new Bitmap(rectBounding.Width, rectBounding.Height);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new Size(rectBounding.Width, rectBounding.Height));
                }


                target.Save("foo2.png", System.Drawing.Imaging.ImageFormat.Png);


            }
        }
    }


}
