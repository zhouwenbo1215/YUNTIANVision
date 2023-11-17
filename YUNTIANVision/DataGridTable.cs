<<<<<<< HEAD
﻿using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using YUNTIANVision.HTDLModel;
using static HTCSharpDemo.Program;

namespace YUNTIANVision.Model
{
    public class DataGridTable
    {
        private TreeFileHelper treeFileHelper;
        private List<string> filterClassNames;
        private DataTable dataTable;
        private Dictionary<string, int> classNameWithDetectNums = new Dictionary<string, int>();
        private DataGridView dataGridView;

        public DataGridTable(TreeFileHelper treeFileHelper, List<string> filterClassNames)
        {
            dataTable = new DataTable();

            this.treeFileHelper = treeFileHelper;
            this.filterClassNames = filterClassNames;
            dataTable.Columns.Add("节点", typeof(string));
            dataTable.Columns.Add("类型", typeof(string));
            dataTable.Columns.Add("个数", typeof(int));
            dataTable.Columns.Add("判定", typeof(string));
            dataTable.Columns.Add("占比", typeof(string));
            dataTable.Columns.Add("面积", typeof(string));
            dataTable.Columns.Add("分数", typeof(string));
        }

        public void InitDataGridTable()
        {
            foreach (var nodeIdWithNodeInfo in treeFileHelper.dic_nodeIdWithNodeInfo)
            {
                NodeInfo nodeInfo = nodeIdWithNodeInfo.Value;
                foreach (var className in nodeInfo.ClassNames)
                {
                    //过滤
                    if (filterClassNames.Contains(className))
                        continue;

                    if (nodeInfo.NodeType == 1)
                    {
                        dataTable.Rows.Add(nodeInfo.NodeName, "定位失败", "0", "--", "0", "--", "--");
                        classNameWithDetectNums.Add("1", 0);
                    }
                    else
                    {
                        dataTable.Rows.Add(nodeInfo.NodeName, className, "0", "--", "0", "--", "--");
                        classNameWithDetectNums.Add(className, 0);
                    }
                }
            }
            dataGridView.DataSource = dataTable;
        }

        public void updateRowData(ref Dictionary<int, List<result>> result,int TotalNum)
        {
            // 1.判断是否定位成功
            // 2.再判断其它项
            if(result.Count > 0)
            {
                List<result> dingwei_result = result[treeFileHelper.dingweiNodeId];
                if(dingwei_result.Count > 0) //定位失败
                {
                    //取第一项
                    result dingwei_result_tmp = dingwei_result[0];

                    NodeInfo nodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo[treeFileHelper.dingweiNodeId];

                    //找到定位这一行
                    // 定义两个筛选条件
                    string filterExpression = "节点 = " + nodeInfo.NodeName + " AND 类型 = " + dingwei_result_tmp.class_id;
                    // 使用 Select 方法筛选出符合条件的行
                    DataRow[] filteredRows = dataTable.Select(filterExpression);
                    if(filteredRows.Length > 0)
                    {
                        DataRow dingweiRow = filteredRows[0];
                        int geshu = classNameWithDetectNums[dingwei_result_tmp.class_id] + 1;
                        classNameWithDetectNums[dingwei_result_tmp.class_id] = geshu;
                        dingweiRow["个数"] = geshu;
                        dingweiRow["判定"] = "NG";
                        dingweiRow["占比"] = ((double)geshu / TotalNum * 100).ToString("F2") + "%";
                        dingweiRow["面积"] = dingwei_result_tmp.area.ToString();
                        dingweiRow["分数"] = dingwei_result_tmp.score.ToString();
                    } 
                }
                else //定位成功，检测其它NG项目
                {
                    foreach (var detect_NGItem in result)
                    {
                        if (detect_NGItem.Key == treeFileHelper.dingweiNodeId)
                            continue;
                        if (detect_NGItem.Value.Count > 0)
                        {
                            NodeInfo nodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo[detect_NGItem.Key];

                            result jianceResult = detect_NGItem.Value[0];

                            //取第一项
                            string class_name = jianceResult.class_id;

                            //过滤
                            if (filterClassNames.Contains(class_name))
                                continue;

                            //有其它异常项
                            //找到定位这一行
                            // 定义两个筛选条件
                            string filterExpression = "节点 = " + nodeInfo.NodeName + " AND 类型 = " + class_name;
                            // 使用 Select 方法筛选出符合条件的行
                            DataRow[] filteredRows = dataTable.Select(filterExpression);
                            if (filteredRows.Length > 0)
                            {
                                DataRow dingweiRow = filteredRows[0];
                                int geshu = classNameWithDetectNums[class_name] + 1;
                                classNameWithDetectNums[class_name] = geshu;
                                dingweiRow["个数"] = geshu;
                                dingweiRow["判定"] = "NG";
                                dingweiRow["占比"] = ((double)geshu / TotalNum * 100).ToString("F2") + "%";
                                dingweiRow["面积"] = jianceResult.area.ToString();
                                dingweiRow["分数"] = jianceResult.score.ToString();
                            }
                        }

                        //只取一项结果
                        break;
                    }
                }
            }
        }

        public class TotalData
        {
            public int AllNum;
            public int OKNum;
            public int NGNum;
        }
    }
}
=======
﻿//using System.Collections.Generic;
//using System.Data;
//using System.Windows.Forms;
//using YUNTIANVision.HTDLModel;
//using static HTCSharpDemo.Program;

//namespace YUNTIANVision.Model
//{
//    public class DataGridTable
//    {
//        private TreeFileHelper treeFileHelper;
//        private List<string> filterClassNames;
//        private DataTable dataTable;
//        private Dictionary<string, int> classNameWithDetectNums = new Dictionary<string, int>();
//        private DataGridView dataGridView;

//        public DataGridTable(TreeFileHelper treeFileHelper, List<string> filterClassNames)
//        {
//            dataTable = new DataTable();

//            this.treeFileHelper = treeFileHelper;
//            this.filterClassNames = filterClassNames;
//            dataTable.Columns.Add("节点", typeof(string));
//            dataTable.Columns.Add("类型", typeof(string));
//            dataTable.Columns.Add("个数", typeof(int));
//            dataTable.Columns.Add("判定", typeof(string));
//            dataTable.Columns.Add("占比", typeof(string));
//            dataTable.Columns.Add("面积", typeof(string));
//            dataTable.Columns.Add("分数", typeof(string));
//        }

//        public void InitDataGridTable()
//        {
//            foreach (var nodeIdWithNodeInfo in treeFileHelper.dic_nodeIdWithNodeInfo)
//            {
//                NodeInfo nodeInfo = nodeIdWithNodeInfo.Value;
//                foreach (var className in nodeInfo.ClassNames)
//                {
//                    //过滤
//                    if (filterClassNames.Contains(className))
//                        continue;

//                    if (nodeInfo.NodeType == 1)
//                    {
//                        dataTable.Rows.Add(nodeInfo.NodeName, "定位失败", "0", "--", "0", "--", "--");
//                        classNameWithDetectNums.Add("1", 0);
//                    }
//                    else
//                    {
//                        dataTable.Rows.Add(nodeInfo.NodeName, className, "0", "--", "0", "--", "--");
//                        classNameWithDetectNums.Add(className, 0);
//                    }
//                }
//            }
//            dataGridView.DataSource = dataTable;
//        }

//        public void updateRowData(ref Dictionary<int, List<result>> result,int TotalNum)
//        {
//            // 1.判断是否定位成功
//            // 2.再判断其它项
//            if(result.Count > 0)
//            {
//                List<result> dingwei_result = result[treeFileHelper.dingweiNodeId];
//                if(dingwei_result.Count > 0) //定位失败
//                {
//                    //取第一项
//                    result dingwei_result_tmp = dingwei_result[0];

//                    NodeInfo nodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo[treeFileHelper.dingweiNodeId];

//                    //找到定位这一行
//                    // 定义两个筛选条件
//                    string filterExpression = "节点 = " + nodeInfo.NodeName + " AND 类型 = " + dingwei_result_tmp.class_id;
//                    // 使用 Select 方法筛选出符合条件的行
//                    DataRow[] filteredRows = dataTable.Select(filterExpression);
//                    if(filteredRows.Length > 0)
//                    {
//                        DataRow dingweiRow = filteredRows[0];
//                        int geshu = classNameWithDetectNums[dingwei_result_tmp.class_id] + 1;
//                        classNameWithDetectNums[dingwei_result_tmp.class_id] = geshu;
//                        dingweiRow["个数"] = geshu;
//                        dingweiRow["判定"] = "NG";
//                        dingweiRow["占比"] = ((double)geshu / TotalNum * 100).ToString("F2") + "%";
//                        dingweiRow["面积"] = dingwei_result_tmp.area.ToString();
//                        dingweiRow["分数"] = dingwei_result_tmp.score.ToString();
//                    } 
//                }
//                else //定位成功，检测其它NG项目
//                {
//                    foreach (var detect_NGItem in result)
//                    {
//                        if (detect_NGItem.Key == treeFileHelper.dingweiNodeId)
//                            continue;
//                        if (detect_NGItem.Value.Count > 0)
//                        {
//                            NodeInfo nodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo[detect_NGItem.Key];

//                            result jianceResult = detect_NGItem.Value[0];

//                            //取第一项
//                            string class_name = jianceResult.class_id;

//                            //过滤
//                            if (filterClassNames.Contains(class_name))
//                                continue;

//                            //有其它异常项
//                            //找到定位这一行
//                            // 定义两个筛选条件
//                            string filterExpression = "节点 = " + nodeInfo.NodeName + " AND 类型 = " + class_name;
//                            // 使用 Select 方法筛选出符合条件的行
//                            DataRow[] filteredRows = dataTable.Select(filterExpression);
//                            if (filteredRows.Length > 0)
//                            {
//                                DataRow dingweiRow = filteredRows[0];
//                                int geshu = classNameWithDetectNums[class_name] + 1;
//                                classNameWithDetectNums[class_name] = geshu;
//                                dingweiRow["个数"] = geshu;
//                                dingweiRow["判定"] = "NG";
//                                dingweiRow["占比"] = ((double)geshu / TotalNum * 100).ToString("F2") + "%";
//                                dingweiRow["面积"] = jianceResult.area.ToString();
//                                dingweiRow["分数"] = jianceResult.score.ToString();
//                            }
//                        }

//                        //只取一项结果
//                        break;
//                    }
//                }
//            }
//        }

//        public class TotalData
//        {
//            public int AllNum;
//            public int OKNum;
//            public int NGNum;
//        }
//    }
//}
>>>>>>> 8a865a9 (增加加载失败提醒)
