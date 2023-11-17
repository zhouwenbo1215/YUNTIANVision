using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;   // 用 DllImport 需用此 命名空间
using System.Diagnostics.Contracts;
using System.Windows.Forms;
using System.IO;
using YUNTIANVision;
using YUNTIANVision.HTDLModel;
using static HTCSharpDemo.Program;

namespace HTCSharpDemo
{
    public class Program
    {

        public enum TeArrLen
        {
            PATH_LEN = 256,
            MAX_DETECT_NUM = 1024
        };

        //error information 
        public enum ErrorCode
        {
            ErrorNone = 0,            // No error.
            ErrorGpuOom = 1,            // Gpu oom
            ErrorClassNumUnequal = 2,            // The number of model types is inconsistent with the number of area thresholds
            ErrorVideoCardDriver = 3,            // Video card driver error
            ErrorPath = 4,            // The path error
            ErrorJsonFile = 5,            // The json file error
            ErrorDeleteFolderFile = 6,            // The delete folder error
            ErrorCreateFolder = 7,            // The create folder error
            ErrorSampleNum = 8,            // Insufficient marked quantity
            ErrorParameter = 9,            // Interface parameter error
            ErrorMultiRstNum = 10,           // 多节点激活节点数跟获取的数量不一致
            ErrorImagEmpty = 11,           // 图像错误
            ErrorInitTreeName = 11,           // 初始化多节点树，节名存在错误
            ErrorUnknown = 10001,        // 未知错误.
        };

        //* 节点类型：缺陷检测、ocr、分类
        public enum NodeType
        {
            NTDefect = 0,   //缺陷是0
            NTOcr = 1,    // 定位是1
            NTClassify = 2   //分类是2
        };

        //* 图像格式
        public struct Point
        {
            public int x;
            public int y;
        };

        //* OCR检测结果
        [StructLayout(LayoutKind.Sequential)]
        public struct OcrResult
        {
            public int left_x;
            public int left_y;
            public int right_x;
            public int right_y;
            //* 检出类别,对应标注
            public int class_id;
            //* 类别得分,得分越高越可信
            public float score;
        };

        //图像数据首地址
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageHt
        {
            public IntPtr data;                                        // 图像的数据首地址
            public int width;                                          // 图像的宽度
            public int height;                                         // 图像的高度
            public int channels;                                       // 图像的通道数
            public int width_step;                                     // 图像每行的步长（bytes）
        };

        //* 缺陷检测结果
        [StructLayout(LayoutKind.Sequential)]
        public struct DetectResult
        {
            public int x;                          //缺陷所在外接矩形位置x
            public int y;                          //y
            public int width;                      //宽
            public int height;                     //高
            public int class_id;                   //缺陷类别（从1开始）
            public int area;                       //缺陷面积大小 分类和定位不会给出
            public float score;                    //得分
                                                   //以下缺陷检测检出信息
            public int points_2d_size;             //
            public IntPtr points_2d;           //对应的缺陷像素点
            public int contour_size;               //存在多少轮廓
            public IntPtr each_contour_size;         //每个轮廓对应的像素点数量
            public IntPtr contour_2d;          //缺陷轮廓点
        };

        //* 多节点中单个节点的测试结果
        [StructLayout(LayoutKind.Sequential)]
        public struct NodeResult
        {
            public int node_id;                    //节点ID

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)(TeArrLen.PATH_LEN))]
            public char[] node_name;

            public int node_type;             //节点类型 定位ocr 分割 分类
            public int detect_results_num;         //检出目标个数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)(TeArrLen.MAX_DETECT_NUM))]
            public DetectResult[] detect_results; //检出结果
        };

        public class result
        {
            public int x;                          //缺陷所在外接矩形位置x
            public int y;                          //y
            public int width;                      //宽
            public int height;                     //高
            public string class_id;                   //缺陷类别（从1开始）
            public int area;                       //缺陷面积大小 分类和定位不会给出
            public float score;                    //得分
                                                   //以下缺陷检测检出信息
            public int points_2d_size;             //
            public IntPtr points_2d;           //对应的缺陷像素点
            public int contour_size;               //存在多少轮廓
            public IntPtr each_contour_size;         //每个轮廓对应的像素点数量
            public IntPtr contour_2d;          //缺陷轮廓点
        }

        //* 初始化句柄
        [DllImport("ht_ai_test.dll", EntryPoint = "InitTreeModel", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public  static extern int InitTreeModel(ref IntPtr pHandle, string strProjectJson, byte[] byteArray_name, int iNodeNum, string strDeviceType, int iDeviceID);
        //static extern int InitTreeModel(ref IntPtr pHandle, string strProjectJson, string[] strNodeNames, int iNodeNum, string strDeviceType, int iDeviceID);

        [DllImport("ht_ai_test.dll", EntryPoint = "TreePredict", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TreePredict(IntPtr pHandle, ref ImageHt pFrame, [In, Out, MarshalAs(UnmanagedType.LPArray)] NodeResult[] pstNodeRst, int iNodeNum);

        [DllImport("ht_ai_test.dll", EntryPoint = "DrawNodeResult", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int DrawNodeResult(ref ImageHt pFrame, NodeResult pstNodeRst, bool bShow);

        [DllImport("ht_ai_test.dll", EntryPoint = "ReadImage", CallingConvention = CallingConvention.Cdecl)]
        static extern int ReadImage(string strImageName, ref ImageHt pFrame);

        [DllImport("ht_ai_test.dll", EntryPoint = "SaveImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SaveImage(string strImageName, ref ImageHt pFrame);

        //* 释放句柄
        [DllImport("ht_ai_test.dll", EntryPoint = "ReleaseTree", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReleaseTree(IntPtr pHandle);

        //* 释放句柄
        [DllImport("ht_ai_test.dll", EntryPoint = "ReleasePredictResult", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReleasePredictResult([In, Out, MarshalAs(UnmanagedType.LPArray)] NodeResult[] pstNodeRst, int iNodeNum);

        public Dictionary<int, List<result>> res1 = new Dictionary<int, List<result>>(); // 每一个节点id对应的结果
        public Dictionary<int, List<result>> res2 = new Dictionary<int, List<result>>(); // 每一个节点id对应的结果

        public static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static bool loadDeepStudyHandle(string config, ref IntPtr m_TreePredictHandle,ref Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo)
        {
            if (!File.Exists(config))
            {
                return false;
            }

            int ErrorNone = 0;
            int ret = ErrorNone;

            int test_num = dic_nodeIdWithNodeInfo.Count;
            List<string> l_nodeNames = new List<string>();
            foreach (var item in dic_nodeIdWithNodeInfo)
            {
                NodeInfo nodeInfo = item.Value;
                l_nodeNames.Add(nodeInfo.NodeName);
            }
            string[] test_node_name_c = l_nodeNames.ToArray();
            //test_node_name_c = { "TreeFileHelper.fenleiName", "TreeFileHelper.dingweiName", $"TreeFileHelper.quexianName" };
            //* 指定GPU
            string device = "GPU";
            //* 指定GPU的ID,可参照任务管理器中的ID号
            int device_id = 0;
            m_TreePredictHandle = IntPtr.Zero; //TreePredictorHandle handle = nullptr;
            //* 初始化句柄,AI加载项目
            //* 初始化句柄
            const int path_len = 256;
            byte[] byteArray_name = Enumerable.Repeat((byte)0x00, test_num * path_len).ToArray();
            for (int i = 0; i < test_num; i++)
            {
                byte[] name_byte = Encoding.Default.GetBytes(test_node_name_c[i]);
                for (int j = 0; j < (path_len - 1) && j < name_byte.Length; j++)
                {
                    byteArray_name[j + i * path_len] = name_byte[j];
                }
            }
            int iRet = -1;
            try
            {
                
                iRet = InitTreeModel(ref m_TreePredictHandle, config, byteArray_name, test_num, device, device_id);
                if (iRet == (int)ErrorCode.ErrorUnknown)
                {
                    MessageBox.Show("模型加载出现未知错误,错误码: "+iRet);
                    return false; 
                } else if(iRet == (int)ErrorCode.ErrorInitTreeName)
                {
                    MessageBox.Show("模型加载时初始化多节点树，节名存在错误,错误码: " + iRet);
                    return false;
                }else if(iRet == (int)ErrorCode.ErrorMultiRstNum)
                {
                    MessageBox.Show("模型加载时，多节点激活节点数跟获取的数量不一致,错误码: " + iRet);
                    return false;
                }
                else if(iRet != (int)ErrorCode.ErrorNone)
                {
                    MessageBox.Show("加载模型出现错误,错误码: " + iRet);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                
                return false;
            }
            return true;
        }

        public List<string> DeepStudy1(IntPtr m_TreePredictHandle, ImageHt pFrame,int test_num, ref Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo, List<string> OKType)
        {
            int ErrorNone = 0;
            int ret = ErrorNone;
            List<string> detectedNGItemsNGtype = new List<string>();

            NodeResult[] pstNodeRst = new NodeResult[test_num];
            //* 调用检测
            ret = TreePredict(m_TreePredictHandle, ref pFrame, pstNodeRst, test_num);

            //SaveImage("ceshi.bmp", ref pFrame);
            //* 检测无异常
            if (ErrorNone == ret)
            {
                res1.Clear();

                foreach (NodeResult nodeRst in pstNodeRst)
                {

                    List<result> results = new List<result>();
                    for(int i = 0; i < nodeRst.detect_results_num; i++)
                    {
                        DetectResult detect_result = nodeRst.detect_results[i];
                        if (detect_result.area != 0 || detect_result.score != 0 || detect_result.x != 0 || detect_result.y != 0
                        || detect_result.width != 0 || detect_result.height != 0)
                        {
                            NodeInfo nodeInfo = dic_nodeIdWithNodeInfo[nodeRst.node_id];
                            string detect_class_name;
                            if (nodeRst.node_type == (int)NodeType.NTDefect) //缺陷类别（从1开始）
                            {
                                detect_class_name = nodeInfo.ClassNames[detect_result.class_id - 1];
                            }
                            else
                            {
                                detect_class_name = nodeInfo.ClassNames[detect_result.class_id];
                            }

                            if (!OKType.Contains(detect_class_name))
                            {
                                HTCSharpDemo.Program.result re = new result();
                                re.x = detect_result.x;
                                re.y = detect_result.y;
                                re.width = detect_result.width;
                                re.height = detect_result.height;
                                re.score = detect_result.score;
                                re.area = detect_result.area;
                                re.points_2d = detect_result.points_2d;
                                re.contour_2d = detect_result.contour_2d;
                                re.class_id = detect_class_name;
                                re.contour_size = detect_result.contour_size;
                                re.each_contour_size = detect_result.each_contour_size;
                                re.points_2d_size = detect_result.points_2d_size;

                                detectedNGItemsNGtype.Add(re.class_id);
                                results.Add(re);
                            }
                        }
                    }
                    res1.Add(nodeRst.node_id, results);
                }
            }
            //全部检测结束就释放。
            ReleasePredictResult(pstNodeRst, test_num);  

            return detectedNGItemsNGtype;
        }      
        public List<string> DeepStudy2(IntPtr m_TreePredictHandle, ImageHt pFrame,int test_num, ref Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo, List<string> OKType)
        {
            #region
            int ErrorNone = 0;
            int ret = ErrorNone;
            List<string> detectedNGItemsNGtype = new List<string>();
            NodeResult[] pstNodeRst = new NodeResult[test_num];
            //* 调用检测
            ret = TreePredict(m_TreePredictHandle, ref pFrame, pstNodeRst, test_num);
            //* 检测无异常
            if (ErrorNone == ret)
            {
                res2.Clear();
                foreach (NodeResult nodeRst in pstNodeRst)
                {
                    List<result> results = new List<result>();
                    for (int i = 0; i < nodeRst.detect_results_num; i++)
                    {
                        DetectResult detect_result = nodeRst.detect_results[i];

                        if (detect_result.area != 0 || detect_result.score != 0 || detect_result.x != 0 || detect_result.y != 0
                            || detect_result.width != 0 || detect_result.height != 0)
                        {

                            NodeInfo nodeInfo = dic_nodeIdWithNodeInfo[nodeRst.node_id];
                            string detect_class_name;
                            if (nodeRst.node_type == (int)NodeType.NTDefect) //缺陷类别（从1开始）
                            {
                                detect_class_name = nodeInfo.ClassNames[detect_result.class_id - 1];
                            }
                            else  //定位分类从0开始
                            {
                                detect_class_name = nodeInfo.ClassNames[detect_result.class_id];
                            }
                            if (!OKType.Contains(detect_class_name))
                            {
                                HTCSharpDemo.Program.result re = new result();
                                re.x = detect_result.x;
                                re.y = detect_result.y;
                                re.width = detect_result.width;
                                re.height = detect_result.height;
                                re.score = detect_result.score;
                                re.area = detect_result.area;
                                re.points_2d = detect_result.points_2d;
                                re.contour_2d = detect_result.contour_2d;
                                re.class_id = detect_class_name;
                                re.contour_size = detect_result.contour_size;
                                re.each_contour_size = detect_result.each_contour_size;
                                re.points_2d_size = detect_result.points_2d_size;

                                detectedNGItemsNGtype.Add(re.class_id);
                                results.Add(re);
                            }
                        }
                    }
                    res2.Add(nodeRst.node_id, results);
                }
            }
            //全部检测结束就释放。
            ReleasePredictResult(pstNodeRst, test_num);

            return detectedNGItemsNGtype;
        } 
    }
}
#endregion