using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using static YUNTIANVision.NGTypePara;
using YUNTIANVision.HTDLModel;

namespace YUNTIANVision
{
    public delegate void loadNGSignalDic();
    public delegate void updateClassIdisOKState(string node_name,string class_id,bool isCheck);
    public partial class frmNGSignalSet : Form
    {
        public event loadNGSignalDic loadNGEvent;
        public event updateClassIdisOKState updateClassIdisOKStateEvent;
        string NGTypeClassIdIsOKDirector;
        string categroyWithPlcAddr_Director;
        string CamName;
        public TreeFileHelper treeFileHelper;
        private List<string> classSetOK = new List<string>();
        Dictionary<string, string[]> nodeNameWithClassNameList = new Dictionary<string, string[]>();

        public frmNGSignalSet(string tag,TreeFileHelper treeFileHelper)
        {
            InitializeComponent();
            CamName = tag;
            this.treeFileHelper = treeFileHelper;
            switch (tag)
            {
                case "NG1":
                    NGTypeClassIdIsOKDirector = AppDomain.CurrentDomain.BaseDirectory +  "NGTypeSignalSet1.txt";
                    categroyWithPlcAddr_Director = AppDomain.CurrentDomain.BaseDirectory +  "OutType1.txt";
                    break;
                case "NG2":
                    NGTypeClassIdIsOKDirector = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_1.txt";
                    categroyWithPlcAddr_Director = AppDomain.CurrentDomain.BaseDirectory + "OutType2_1.txt";
                    break;
                case "NG3":
                    NGTypeClassIdIsOKDirector = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_2.txt";
                    categroyWithPlcAddr_Director = AppDomain.CurrentDomain.BaseDirectory + "OutType2_2.txt";
                    break;
                default:
                    break;
            }
            if (!File.Exists(NGTypeClassIdIsOKDirector))
                File.Create(NGTypeClassIdIsOKDirector).Close();
            if (!File.Exists(categroyWithPlcAddr_Director))
                File.Create(categroyWithPlcAddr_Director).Close();
        }

        #region load/close事件
        private void frmNGSignalSet_Load(object sender, EventArgs e)
        {
            if(treeFileHelper != null)
            {
                outTypeControl(false);
                bt_save.Enabled = false;
                loadOutSignal(); //初始化输出信号种类对应的PLC地址    
                loadNode();//初始化节点
                addNGOutType();//初始化输出种类
                isOK(); //强制OK勾选框是否有被勾选上

                cb_nodeOutSignalWithNodeType.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
                cb_nodeOutSignalWithNodeType.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
                cb_nodeOutSignalWithNodeClassId.SelectedIndexChanged -= comboBox2_SelectedIndexChanged;
                cb_nodeOutSignalWithNodeClassId.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
                cb_nodeOutSignalWithOutCategory.SelectedIndexChanged -= cbNGType_SelectedIndexChanged;
                cb_nodeOutSignalWithOutCategory.SelectedIndexChanged += cbNGType_SelectedIndexChanged;
            }
        }
        #endregion
        
        public void setClassSetOK(ref List<string> classes)
        {
            classSetOK.Clear();
            foreach (string className in classes)
            {
                classSetOK.Add(className);
            }
        }

        #region 初始化信号输出地址类型
        private void loadOutSignal()
        {
            NGTypePara.OutSignalSets signalSets = new NGTypePara.OutSignalSets();
            string jsonFromFile = File.ReadAllText(categroyWithPlcAddr_Director);
            signalSets = JsonConvert.DeserializeObject<OutSignalSets>(jsonFromFile);
            if(signalSets!=null)
            {
                foreach (var item in signalSets.outsignals)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                        TextBox tb = (TextBox)this.tableLayoutPanel2.Controls["tb_OutSignalPLCAddr" + i.ToString()];
                        if (item.OutSignalName == cb.Text.Trim())
                        {
                            cb.Checked = item.isEnable;
                            tb.Text = item.OutSignal;
                        }
                    }
                }
            }
            else
            {
                NGTypePara.OutSignalSets sets = new NGTypePara.OutSignalSets();
                for (int i = 1; i <= 10; i++)
                {
                    NGTypePara.OutSignalSet sss= new NGTypePara.OutSignalSet();
                    CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                    TextBox tb = (TextBox)this.tableLayoutPanel2.Controls["tb_OutSignalPLCAddr" + i.ToString()];
                    sss.OutSignalName= cb.Text.Trim();
                    sss.isEnable = cb.Checked;
                    sss.OutSignal = tb.Text;
                    sets.outsignals.Add(sss);
                }
                string json = JsonConvert.SerializeObject(sets, Formatting.Indented);
                File.WriteAllText(categroyWithPlcAddr_Director, json);
            }
        }
        #endregion
        
        #region 初始化节点
        private void loadNode()
        {
            cb_nodeOutSignalWithNodeClassId.Items.Clear();

            int firstNodeId = int.MinValue;
            foreach (var item in treeFileHelper.dic_nodeIdWithNodeInfo)
            {
                if (item.Value.ParentsNodeId > -1 && item.Value.NodeType == 1)
                    continue;
                else
                {
                    if (!cb_nodeOutSignalWithNodeType.Items.Contains(item.Value.NodeName))
                    {
                        cb_nodeOutSignalWithNodeType.Items.Add(item.Value.NodeName);
                        nodeNameWithClassNameList.Add(item.Value.NodeName, item.Value.ClassNames);
                        if(firstNodeId == int.MinValue)
                        {
                            firstNodeId = item.Value.NodeId;
                        }
                    }
                }
            }
            if (cb_nodeOutSignalWithNodeType.Items.Count > 0)
                cb_nodeOutSignalWithNodeType.SelectedIndex = 0;

            if(firstNodeId != int.MinValue)
            {
                NodeInfo nodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo[firstNodeId];
                foreach (var item1 in nodeInfo.ClassNames)
                {
                    cb_nodeOutSignalWithNodeClassId.Items.Add(item1);
                }
                if (cb_nodeOutSignalWithNodeClassId.Items.Count > 0)
                    cb_nodeOutSignalWithNodeClassId.SelectedIndex = 0;
            }
            

            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            if (ngTypes == null)
            {
                NGTypePara.NGType types = new NGTypePara.NGType();
                File.Create(NGTypeClassIdIsOKDirector).Close();
                foreach (var item1 in treeFileHelper.dic_nodeIdWithNodeInfo)
                {
                    if (item1.Value.ParentsNodeId > -1 && item1.Value.NodeType == 1)
                        continue;
                    foreach (var item2 in item1.Value.ClassNames)
                    {
                        if (!item2.Equals("其他") || !item2.Equals("OK"))
                        {
                            NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                            nGTypePara.Node = item1.Value.NodeName;
                            nGTypePara.NGType = item2;
                            nGTypePara.OutType = "缺陷类别1";
                            nGTypePara.isOK = false;
                            types.NGTypeConfigs.Add(nGTypePara);
                        }
                        else
                        {
                            NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                            nGTypePara.Node = item1.Value.NodeName;
                            nGTypePara.NGType = item2;
                            nGTypePara.OutType = "缺陷类别1";
                            nGTypePara.isOK = true;
                            types.NGTypeConfigs.Add(nGTypePara);
                        }
                    }
                }
                string json = JsonConvert.SerializeObject(types, Formatting.Indented);
                File.WriteAllText(NGTypeClassIdIsOKDirector, json);
            }
        }
        #endregion

        #region 添加 ng输出信号种类到下拉框
        private void addNGOutType()
        {
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            cb_nodeOutSignalWithOutCategory.Items.Clear();
            switch (CamName)
            {
                case "NG1":
                    if (frmOneCamera.ngdic.Count > 0)
                    {
                        foreach (var item in frmOneCamera.ngdic.Keys)
                        {
                            cb_nodeOutSignalWithOutCategory.Items.Add(item);
                        }

                        foreach (var item in ngTypes.NGTypeConfigs)
                        {
                            for (int i = 0; i < cb_nodeOutSignalWithOutCategory.Items.Count; i++)
                            {
                                if (cb_nodeOutSignalWithOutCategory.Items[i].ToString() == item.OutType)
                                {
                                    cb_nodeOutSignalWithOutCategory.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case "NG2":
                    if (frmTwoCamera.dic_defectClassificationswithPLCaddress_left.Count > 0)
                    {
                        foreach (var item in frmTwoCamera.dic_defectClassificationswithPLCaddress_left.Keys)
                        {
                            cb_nodeOutSignalWithOutCategory.Items.Add(item);
                        }

                        foreach (var item in ngTypes.NGTypeConfigs)
                        {
                            for (int i = 0; i < cb_nodeOutSignalWithOutCategory.Items.Count; i++)
                            {
                                if (cb_nodeOutSignalWithOutCategory.Items[i].ToString() == item.OutType)
                                {
                                    cb_nodeOutSignalWithOutCategory.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case "NG3":
                    if (frmTwoCamera.dic_defectClassificationswithPLCaddress_right.Count > 0)
                    {
                        foreach (var item in frmTwoCamera.dic_defectClassificationswithPLCaddress_right.Keys)
                        {
                            cb_nodeOutSignalWithOutCategory.Items.Add(item);
                        }
                        foreach (var item in ngTypes.NGTypeConfigs)
                        {
                            for (int i = 0; i < cb_nodeOutSignalWithOutCategory.Items.Count; i++)
                            {
                                if (cb_nodeOutSignalWithOutCategory.Items[i].ToString() == item.OutType)
                                {
                                    cb_nodeOutSignalWithOutCategory.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            
        }
        #endregion

        #region 初始化判断ng的输出类型,判断ok是否勾上
        private void isOK()
        {
            try
            {
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null)
                {
                    // 取消绑定事件
                    ck_checkIsOK.CheckedChanged -= cbIsOK_CheckedChanged;
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {

                        if (cb_nodeOutSignalWithNodeClassId.SelectedItem!=null && item.NGType == cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString() && item.Node == cb_nodeOutSignalWithNodeType.SelectedItem.ToString())
                        {
                            if (item.isOK || classSetOK.Contains(item.NGType))
                            {
                                ck_checkIsOK.Checked = true;
                                //cb_nodeOutSignalWithOutCategory.Enabled = false;
                            }
                            else
                                ck_checkIsOK.Checked = false;
                            break;
                        }
                    }
                    // 重新绑定事件
                    ck_checkIsOK.CheckedChanged += cbIsOK_CheckedChanged;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
            
        }
        #endregion

        #region 节点变化事件
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            editControl(false);
            cb_nodeOutSignalWithNodeClassId.Items.Clear();
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            
            foreach (var item in nodeNameWithClassNameList)
            {
                if (cb_nodeOutSignalWithNodeType.SelectedItem.ToString() == item.Key)
                {
                    foreach (var item1 in item.Value)
                    {
                        //if(!treeFileHelper.noAddItem.Contains(item1))
                        cb_nodeOutSignalWithNodeClassId.Items.Add(item1);
                    }
                    if (cb_nodeOutSignalWithNodeClassId.Items.Count > 0)
                        cb_nodeOutSignalWithNodeClassId.SelectedIndex = 0;
                }
            }
            foreach (var item1 in ngTypes.NGTypeConfigs)
            {
                if (cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString() == item1.NGType&& cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString()==item1.Node)
                {
                    ck_checkIsOK.Checked = item1.isOK;
                    if (item1.isOK)
                    {
                        //cb_nodeOutSignalWithOutCategory.Enabled = false;
                    }
                    else
                    {
                        for (int i = 0; i < cb_nodeOutSignalWithOutCategory.Items.Count; i++)
                        {
                            if (cb_nodeOutSignalWithOutCategory.Items[i].ToString() == item1.OutType)
                                cb_nodeOutSignalWithOutCategory.SelectedIndex = i;
                        }
                    }
                }
            }
            editControl(true);
        }

        #endregion

        #region 节点检测结果种类事件
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            editControl(false);
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            // 取消绑定事件
            ck_checkIsOK.CheckedChanged -= cbIsOK_CheckedChanged;
            foreach (var item in ngTypes.NGTypeConfigs)
            {
                if(item.NGType == cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString() && item.Node == cb_nodeOutSignalWithNodeType.SelectedItem.ToString())
                {
                    ck_checkIsOK.Checked = item.isOK;
                    if (item.isOK)
                    {
                        //cb_nodeOutSignalWithOutCategory.Enabled = false;
                    } 
                    else
                    {
                        for (int i = 0; i < cb_nodeOutSignalWithOutCategory.Items.Count; i++)
                        {
                            if (cb_nodeOutSignalWithOutCategory.Items[i].ToString() == item.OutType)
                                cb_nodeOutSignalWithOutCategory.SelectedIndex = i;
                        }
                    }
                }
            }
            // 重新绑定事件
            ck_checkIsOK.CheckedChanged += cbIsOK_CheckedChanged;
            string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
            File.WriteAllText(NGTypeClassIdIsOKDirector, json);
            editControl(true);
        }
        #endregion

        #region 输出信号类别事件
        private void cbNGType_SelectedIndexChanged(object sender, EventArgs e)
        {
            editControl(false);
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            foreach (var item in ngTypes.NGTypeConfigs)
            {
                if (cb_nodeOutSignalWithNodeClassId.SelectedItem!= null && item.NGType == cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString() && item.Node == cb_nodeOutSignalWithNodeType.SelectedItem.ToString())
                    item.OutType = cb_nodeOutSignalWithOutCategory.SelectedItem.ToString();
            }
            string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
            File.WriteAllText(NGTypeClassIdIsOKDirector, json);
            //frmOneCamera.ngdic = addDic();
            loadNGEvent?.Invoke();
            editControl(true);
        }
        #endregion

        #region 强制OK事件
        private void cbIsOK_CheckedChanged(object sender, EventArgs e)
        {
            editControl(false);
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            string jsonFromFile = File.ReadAllText(NGTypeClassIdIsOKDirector);
            ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
            foreach (var item in ngTypes.NGTypeConfigs)
            {
                if (item.NGType == cb_nodeOutSignalWithNodeClassId.SelectedItem.ToString() && item.Node == cb_nodeOutSignalWithNodeType.SelectedItem.ToString())
                    item.isOK = ck_checkIsOK.Checked;
            }
            //if (ck_checkIsOK.Checked)
            //    cb_nodeOutSignalWithOutCategory.Enabled = false;
            //else
            //    cb_nodeOutSignalWithOutCategory.Enabled = true;
            string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
            File.WriteAllText(NGTypeClassIdIsOKDirector, json);
            if(!string.IsNullOrEmpty(cb_nodeOutSignalWithNodeType.Text) && !string.IsNullOrEmpty(cb_nodeOutSignalWithNodeClassId.Text))
                updateClassIdisOKStateEvent?.Invoke(cb_nodeOutSignalWithNodeType.Text,cb_nodeOutSignalWithNodeClassId.Text, ck_checkIsOK.Checked);
            editControl(true);
        }
        #endregion

        #region 修改设置的时候禁止操作
        private void editControl(bool login)
        {
            //cb_nodeOutSignalWithNodeType.Enabled = login;
            //cb_nodeOutSignalWithNodeClassId.Enabled = login;
            //cbNGOutType.Enabled = login;
            //cbIsOK.Enabled = login;
        }
        #endregion

        #region 输出信号控件控制
        private void outTypeControl(bool login)
        {
            cb_OutTypePLCAddrIsChek1.Enabled = login;
            cb_OutTypePLCAddrIsChek2.Enabled = login; 
            cb_OutTypePLCAddrIsChek3.Enabled = login;
            cb_OutTypePLCAddrIsChek4.Enabled = login;
            cb_OutTypePLCAddrIsChek5.Enabled = login;
            cb_OutTypePLCAddrIsChek6.Enabled = login;
            cb_OutTypePLCAddrIsChek7.Enabled = login;
            cb_OutTypePLCAddrIsChek8.Enabled = login;
            cb_OutTypePLCAddrIsChek9.Enabled = login;
            cb_OutTypePLCAddrIsChek10.Enabled = login;
            
            tb_OutSignalPLCAddr1.Enabled = login;
            tb_OutSignalPLCAddr2.Enabled = login;
            tb_OutSignalPLCAddr3.Enabled = login;
            tb_OutSignalPLCAddr4.Enabled = login;
            tb_OutSignalPLCAddr5.Enabled = login;
            tb_OutSignalPLCAddr6.Enabled = login;
            tb_OutSignalPLCAddr7.Enabled = login;
            tb_OutSignalPLCAddr8.Enabled = login;
            tb_OutSignalPLCAddr9.Enabled = login;
            tb_OutSignalPLCAddr10.Enabled = login;
        }
        #endregion

        #region 编辑plc地址
        private void button1_Click(object sender, EventArgs e)
        {
            outTypeControl(true);
            bt_editing.Enabled= false;
            bt_save.Enabled= true;
        }
        #endregion

        #region 保存plc地址
        private void button2_Click_1(object sender, EventArgs e)
        {
            int count = 0; // 用于计数选中的复选框数量
            for (int i = 1; i <= 10; i++)
            {
                CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                TextBox tb = (TextBox)this.tableLayoutPanel2.Controls["tb_OutSignalPLCAddr" + i.ToString()];
                if (cb.Checked)
                {
                    count++;
                    if(String.IsNullOrEmpty(tb.Text))
                    {
                        MessageBox.Show($"缺陷类别{i}的地址还未添加，请先添加");
                        return;
                    }
                }
            }
            if(count==0)
            {
                MessageBox.Show("请至少勾选一个NG信号");
                return;
            }
            NGTypePara.NGType NodeClassIdWithPlcAddrIsOK_list = new NGTypePara.NGType();
            string json1 = File.ReadAllText(NGTypeClassIdIsOKDirector);
            NodeClassIdWithPlcAddrIsOK_list = JsonConvert.DeserializeObject<NGType>(json1);
            for (int i = 1; i <= 10; i++)
            {
                CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                if (!cb.Checked && NodeClassIdWithPlcAddrIsOK_list.NGTypeConfigs.Where(a => a.OutType == cb.Text).Any())
                {
                    MessageBox.Show("此类型已被设置，无法无法取消");
                    return;
                }
            }
            NGTypePara.OutSignalSets categroyWithPlcAddr_list = new NGTypePara.OutSignalSets();
            string jsonFromFile = File.ReadAllText(categroyWithPlcAddr_Director);
            categroyWithPlcAddr_list = JsonConvert.DeserializeObject<OutSignalSets>(jsonFromFile);
            cb_nodeOutSignalWithOutCategory.Items.Clear();
            if(categroyWithPlcAddr_list!=null)
            {
                foreach (var item in categroyWithPlcAddr_list.outsignals)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                        TextBox tb = (TextBox)this.tableLayoutPanel2.Controls["tb_OutSignalPLCAddr" + i.ToString()];
                        if (item.OutSignalName == cb.Text)
                        {
                            item.isEnable = cb.Checked;
                            item.OutSignal = tb.Text.Trim();
                            if (cb.Checked)
                                cb_nodeOutSignalWithOutCategory.Items.Add(cb.Text);
                        }
                    }
                }
                if(cb_nodeOutSignalWithOutCategory.Items.Count>0)
                    cb_nodeOutSignalWithOutCategory.SelectedIndex = 0;
                string json = JsonConvert.SerializeObject(categroyWithPlcAddr_list, Formatting.Indented);
                File.WriteAllText(categroyWithPlcAddr_Director, json);
            }
            else
            {
                NGTypePara.OutSignalSets signals = new NGTypePara.OutSignalSets();
                for (int i = 1;i <= 10;i++)
                {
                    NGTypePara.OutSignalSet signalSet=new NGTypePara.OutSignalSet();
                    CheckBox cb = (CheckBox)this.tableLayoutPanel2.Controls["cb_OutTypePLCAddrIsChek" + i.ToString()];
                    TextBox tb = (TextBox)this.tableLayoutPanel2.Controls["tb_OutSignalPLCAddr" + i.ToString()];
                    signalSet.OutSignalName = cb.Text;
                    signalSet.isEnable = cb.Checked;
                    signalSet.OutSignal = tb.Text.Trim();
                    signals.outsignals.Add(signalSet);
                    if (cb.Checked)
                        cb_nodeOutSignalWithOutCategory.Items.Add(cb.Text) ;
                    string json = JsonConvert.SerializeObject(signals, Formatting.Indented);
                    File.WriteAllText(categroyWithPlcAddr_Director, json);
                }
                if (cb_nodeOutSignalWithOutCategory.Items.Count > 0)
                    cb_nodeOutSignalWithOutCategory.SelectedIndex = 0;
            }
            
            foreach (var item in NodeClassIdWithPlcAddrIsOK_list.NGTypeConfigs)
            {
                if(string.IsNullOrEmpty(item.OutType))
                 item.OutType = "缺陷类别1";
            }
            string json2 = JsonConvert.SerializeObject(NodeClassIdWithPlcAddrIsOK_list, Formatting.Indented);
            File.WriteAllText(NGTypeClassIdIsOKDirector, json2);
            bt_editing.Enabled = true;
            bt_save.Enabled = false;
            loadNGEvent?.Invoke();
            outTypeControl(false);
        }
        #endregion
    }
}
