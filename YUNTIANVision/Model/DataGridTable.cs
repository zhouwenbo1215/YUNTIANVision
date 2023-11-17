using System.Collections.Generic;
using System.Data;
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
        }

        public void updateRowData(ref Dictionary<int, List<result>> result,int NGNum,int TotalNum)
        {
            // 1.判断是否定位成功
            // 2.再判断其它项
            if(result.Count > 0)
            {
                List<result> dingwei_result = result[treeFileHelper.dingweiNodeId];
                if(dingwei_result.Count > 0) //定位失败
                {
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
                        dingweiRow["个数"] = classNameWithDetectNums[dingwei_result_tmp.class_id] + 1;
                        dingweiRow["判定"] = "NG";
                        dingweiRow["占比"] = 
                    }
                }
            }

            foreach(var tmp_result in result)
            {
                int NodeId = tmp_result.Key;
                
                if(tmp_result.Value.Count > 0)// 需要更新
                {
                    string nodeClass = 
                }
            }
        }
    }
}
