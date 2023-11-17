using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;

namespace LW.ZOOM
{
    public class ZoomImage
    {
        private double ImgRow1
        {
            set;
            get;
        }
        private double ImgCol1
        {
            set;
            get;
        }
        private double ImgRow2
        {
            set;
            get;
        }
        private double ImgCol2
        {
            set;
            get;
        }
        private double ImgWidth
        {
            set;
            get;
        }

        private double ImgHeight
        {
            set;
            get;
        }

        private HWindowControl ShowWnd
        {
            set;
            get;
        }

        public double StartX
        {
            set;
            get;
        }

        public double StartY
        {
            set;
            get;
        }

        public bool MouseStatus
        {
            set;
            get;
        }

        public ZoomImage(double row1, double col1, double row2, double col2, double imgW, double imgH, HWindowControl window)
        {
            this.ImgRow1 = row1;
            this.ImgCol1 = col1;
            this.ImgRow2 = row2;
            this.ImgCol2 = col2;
            this.ImgWidth = imgW;
            this.ImgHeight = imgH;
            this.ShowWnd = window;
        }

        public void zoomImage(double x, double y, double scale, HObject image)
        {
            double lengthC, lengthR;
            double percentC, percentR;
            int lenC, lenR;

            percentC = (x - this.ImgCol1) / (this.ImgCol2 - this.ImgCol1);
            percentR = (y - this.ImgRow1) / (this.ImgRow2 - this.ImgRow1);

            lengthC = (this.ImgCol2 - this.ImgCol1) * scale;
            lengthR = (this.ImgRow2 - this.ImgRow1) * scale;

            this.ImgCol1 = x - lengthC * percentC;
            this.ImgCol2 = x + lengthC * (1 - percentC);

            this.ImgRow1 = y - lengthR * percentR;
            this.ImgRow2 = y + lengthR * (1 - percentR);

            lenC = (int)Math.Round(lengthC);
            lenR = (int)Math.Round(lengthR);

            System.Drawing.Rectangle rect = this.ShowWnd.ImagePart;
            rect.X = (int)Math.Round(ImgCol1);
            rect.Y = (int)Math.Round(ImgRow1);
            rect.Width = (lenC > 0) ? lenC : 1;
            rect.Height = (lenR > 0) ? lenR : 1;
            this.ShowWnd.ImagePart = rect;
            repaint(image);
        }

        public void repaint(HObject image)
        {
            try
            {
                HOperatorSet.SetSystem("flush_graphic", "false");
                //清除显示窗口
                this.ShowWnd.HalconWindow.ClearWindow();
                HOperatorSet.SetSystem("flush_graphic", "true");
                this.ShowWnd.HalconWindow.DispObj(image);
            }
            catch { }
        }

        public void moveImage(double motionX, double motionY, HObject image)
        {
            double lengthC, lengthR;
            int lenC, lenR;

            ImgRow1 -= motionY;
            ImgRow2 -= motionY;

            ImgCol1 -= motionX;
            ImgCol2 -= motionX;

            lengthC = (this.ImgCol2 - this.ImgCol1);
            lengthR = (this.ImgRow2 - this.ImgRow1);
            lenC = (int)Math.Round(lengthC);
            lenR = (int)Math.Round(lengthR);

            System.Drawing.Rectangle rect = this.ShowWnd.ImagePart;
            rect.X = (int)Math.Round(ImgCol1);
            rect.Y = (int)Math.Round(ImgRow1);
            rect.Width = (lenC > 0) ? lenC : 1;
            rect.Height = (lenR > 0) ? lenR : 1;
            this.ShowWnd.ImagePart = rect;

            repaint(image);
        }

    }
}
