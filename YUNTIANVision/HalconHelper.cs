using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.IO;

namespace YUNTIANVision
{
    internal class HalconHelper
    {
        /// <summary>
        /// 在窗口显示文字
        /// </summary>
        /// <param name="hWindow">窗口句柄</param>
        /// <param name="x">显示x坐标</param>
        /// <param name="y">显示y坐标</param>
        /// <param name="font">字体大小</param>
        /// <param name="color">字体颜色</param>
        /// <param name="str">字体内容</param>
       public static void showResultString(HTuple hWindow,int x,int y,int font,string color,string str)
       {
            HOperatorSet.SetColor(hWindow, $"{color}");
            HOperatorSet.SetTposition(hWindow, x, y);
            HOperatorSet.SetFont(hWindow, "微软雅黑-72");
            HOperatorSet.WriteString(hWindow, $"{str}");
       }
        /// <summary>
        /// 显示缺陷区域
        /// </summary>
        /// <param name="hWindow">窗口句柄</param>
        /// <param name="x1">左上角x坐标</param>
        /// <param name="y1">左上角y坐标</param>
        /// <param name="x2">右下角x坐标</param>
        /// <param name="y2">右下角y坐标</param>
        public static void showRoi(HTuple hWindow,string color, int x1,int y1,int x2,int y2)
        {
            HOperatorSet.SetColor(hWindow, $"{color}");
            HOperatorSet.SetDraw(hWindow, "margin");
            HOperatorSet.GenRectangle1(out HObject hObject, x1, y1, x2,y2);
            HOperatorSet.DispObj(hObject,hWindow);
        }
        /// <summary>
        /// 保存原图还是渲染图
        /// </summary>
        /// <param name="image">输出渲染图</param>
        /// <param name="hwindow">窗口句柄</param>
        /// <param name="image1">要保存的图片</param>
        /// <param name="path">保存路径</param>
        public static void saveImage(HTuple hwindow,HObject image,string path)
        {
            string imageType= IniHelper.SaveSetIni.Read("图片设置","保存类型");
            if (String.IsNullOrEmpty(imageType))
                try { HOperatorSet.WriteImage(image, "bmp", 0, path); }
                catch (Exception ex) { }
            else
            {
                if (imageType == "渲染图")
                {
                    HOperatorSet.DumpWindowImage(out HObject image1, hwindow);
                    HOperatorSet.WriteImage(image1, "bmp", 0, path);
                }
                else if (imageType == "原图")
                    try { HOperatorSet.WriteImage(image, "bmp", 0, path); }
                    catch (Exception ex) { }
            }
        }
        /// <summary>
        /// 居中显示图片输出图片左上角右下角坐标
        /// </summary>
        /// <param name="picWidth">图片宽</param>
        /// <param name="picHeight">图片高</param>
        /// <param name="winWidth">窗口宽</param>
        /// <param name="winHeight">窗口高</param>
        /// <param name="row1">输出左上角y坐标</param>
        /// <param name="column1">输出左上角x坐标</param>
        /// <param name="row2">输出右下角y坐标</param>
        /// <param name="column2">输出右下角x坐标</param>
        public static void imageLocation(int picWidth,int picHeight,int winWidth,int winHeight,out double row1,out double column1,out double row2,out double column2)
        {
            double ratioWidth = (1.0) * picWidth / winWidth;
            double ratioHeight = (1.0) * picHeight / winHeight;
            if (ratioWidth >= ratioHeight)
            {
                row1 = -(1.0) * ((winHeight * ratioWidth) - picHeight) / 2;
                column1 = 0;
                row2 = row1 + winHeight * ratioWidth;
                column2 = column1 + winWidth * ratioWidth;
            }
            else
            {
                row1 = 0;
                column1 = -(1.0) * ((winWidth * ratioHeight) - picWidth) / 2;
                row2 = row1 + winHeight * ratioHeight;
                column2 = column1 + winWidth * ratioHeight;
            }
        }
        /// <summary>
        /// 判断文件名是否存在，存在则在文件名后添加后缀
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string fileNameIsExist(string filePath,string fileName)
        {
            string newFileName=null;
            if (File.Exists(filePath+fileName+".bmp"))
            {
                int suffix = 1;
                newFileName = fileName;
                while (File.Exists(filePath+newFileName + ".bmp"))
                {
                    suffix++;
                    newFileName = $"{fileName}_{suffix}";
                }
                return filePath + newFileName;
            }
            return filePath+fileName;
        }
    }
}
