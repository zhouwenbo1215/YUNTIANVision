using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUNTIANVision
{
    internal class NGTypePara
    {
        public class NGTypeConfig
        {
            /// <summary>
            /// 节点名
            /// </summary>
            public string Node { get; set; }
            /// <summary>
            /// 节点名对应的类型
            /// </summary>
            public string NGType { get; set; }
            /// <summary>
            /// 类型输出的NG类型
            /// </summary>
            public string OutType { get; set; }
            /// <summary>
            /// 是否是OK项
            /// </summary>
            public bool isOK { get; set; }
        }
        public class NGType
        {
            public List<NGTypeConfig> NGTypeConfigs = new List<NGTypeConfig>();
        }

        public class OutSignalSet
        {
            public string OutSignalName { set; get; }
            public bool isEnable { set; get;}    
            public string OutSignal { get; set; }
        }
        public class OutSignalSets
        {
            public List<OutSignalSet> outsignals = new List<OutSignalSet>();
        }
    }
}
