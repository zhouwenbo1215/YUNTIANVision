using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using YUNTIANVision.HTDLModel;

namespace YUNTIANVision
{
    public class TreeFileHelper
    {
        // 节点id 和 节点检测分类数组
        private Dictionary<int, NodeInfo> _dic_nodeIdWithNodeInfo = new Dictionary<int, NodeInfo>();
        public Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo
        {
            get { return _dic_nodeIdWithNodeInfo; }
        }

        public int dingweiNodeId = int.MinValue;
        public int fenleiNodeId = int.MinValue;
        public int quexianNodeId = int.MinValue;
        public int fenlei2NodeId;
        /// 所有除了第一个定位节点的id集合
        /// </summary>
        public List<int> listDingweiId=new List<int>();

        private string _filePath;
        public string filePath
        { 
            get { return _filePath; }
            set { 
                if (!string.IsNullOrEmpty(value) && value != _filePath)
                {
                    _filePath = value;
                    LoadTreeFile(_filePath);
                }
            }
        }
        private class NodeInfos
        {
            public NodeInfo[] NodeInfo { get; set; }
        }

        private void LoadTreeFile(string config)
        {
            listDingweiId.Clear();
            _dic_nodeIdWithNodeInfo.Clear();

            string treefile = null;
            try
            {
                treefile = File.ReadAllText(config);
            }
            catch (Exception  e) {
                MessageBox.Show("模型路径: "+config +"不存在，请重新设置");
                return;
            }

            List<string> tmp_leaf_classNames_list = new List<string>();
            NodeInfos str =Newtonsoft.Json.JsonConvert.DeserializeObject<NodeInfos>(treefile);
            foreach (var item in str.NodeInfo)
            {
                _dic_nodeIdWithNodeInfo.Add(item.NodeId, item);

                if (item.NodeType==1&&item.ParentsNodeId!=-1)
                    listDingweiId.Add(item.NodeId);
                switch (item.ParentsNodeId)
                {
                    case -1:
                        if(item.NodeType == 1)
                            dingweiNodeId =item.NodeId;
                        break;
                    case 0:
                        if(item.NodeType == 2)
                            fenleiNodeId =item.NodeId;
                        break;
                    case 1:
                        if (item.NodeType == 0)
                            quexianNodeId = item.NodeId;
                        else if (item.NodeType == 2)
                            fenlei2NodeId = item.NodeId;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

