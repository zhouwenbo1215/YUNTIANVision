using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text.RegularExpressions;

namespace YUNTIANVision
{
    public partial class frmOneTable : Form
    {
        public frmOneTable()
        {
            InitializeComponent();
            frmOneCamera.myShowDelegate += new showOneDelegate(ReferenceItems);
            frmMain.controlEvent += new controlDelegate(runControl);
            frmOneCamera.myCloseTableDelegate += new closeOneDelegate(closeTable);
            frmOneImformation.clearSqlEvent += new clearSqlDelegate1(clearSql);
            //Reference += ReferenceItems;
        }
        //private event Action Reference;
        DataGridViewHelper dataGridView;
        /// <summary>
        /// OK类型种类
        /// </summary>
        public static List<string> OKType = new List<string>();
        /// <summary>
        /// NG类型种类
        /// </summary>
        public static List<string> NGType = new List<string>();
        
        /// <summary>
        /// 单相机深度学习句柄
        /// </summary>
        public static IntPtr studyHandle;

        #region load/close事件
        private void frmOneTable_Load(object sender, EventArgs e)
        {
            dataGridView = new DataGridViewHelper(dataGridView1);
            dataGridView.addHeader();

            string[] strs = File.ReadAllLines(OKpath);
            cbOKItem.Items.Clear();
            foreach (var item in strs)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbOKItem.Items.Add(item);
                    OKType.Add(item);
                }
            }
            if (cbOKItem.Items.Count > 0)
                cbOKItem.SelectedIndex = 0;
            string[] strs1 = File.ReadAllLines(NGpath);
            cbNGItem.Items.Clear();
            foreach (var item in strs1)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbNGItem.Items.Add(item);
                    NGType.Add(item);
                }
            }
            if (cbNGItem.Items.Count > 0)
                cbNGItem.SelectedIndex = 0;
            loadDb();
            loadTable();
        }

        private void closeTable()
        {
            this.Close();
        }
        private void frmOneTable_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmOneCamera.myShowDelegate -= new showOneDelegate(ReferenceItems);
            frmOneCamera.myCloseTableDelegate -= new closeOneDelegate(closeTable);
            frmMain.controlEvent -= new controlDelegate(runControl);
            frmOneImformation.clearSqlEvent -= new clearSqlDelegate1(clearSql);
            //Reference -= ReferenceItems;
        }
        #endregion

        #region 显示结果事件
        private async void ReferenceItems()
        {
            await Task.Run(new Action(() =>
            {
                TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                foreach (DataGridViewRow row in this.dataGridView1.Rows)
                {
                    if (dataGridView1.Rows.Count - 1 > row.Index)
                    {
                        double ngNum = 0;
                        bool isAdd = false;
                        row.Cells[4].Value = frmOneImformation.AllNum;
                        foreach (var item in HTCSharpDemo.Program.res1)
                        {
                            if (item.Value.Count > 0)
                            {
                                foreach (var item1 in item.Value)
                                {
                                    if (row.Cells[1].Value != null && !String.IsNullOrEmpty(row.Cells[1].Value.ToString()))
                                    {
                                        if (item1.class_id == (string)row.Cells[1].Value)
                                        {
                                            isAdd = true;
                                            if (row.Cells[2].Value != null && !String.IsNullOrEmpty(row.Cells[2].Value.ToString()) && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                                ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                            row.Cells[2].Value = ngNum + 1;  //结果出现次数
                                            if (row.Cells[0].Value != null && !String.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                                            {
                                                if ((string)row.Cells[0].Value == TreeFileHelper.dingweiName)
                                                {
                                                    if (HTCSharpDemo.Program.res1[TreeFileHelper.dingweiNodeId].Count == 1)
                                                        row.Cells[3].Value = "OK"; //判定
                                                    else
                                                        row.Cells[3].Value = "定位失败"; //判定
                                                }
                                                else
                                                {
                                                    if (frmOneCamera.result == "OK")
                                                        row.Cells[3].Value = "OK"; //判定
                                                    else
                                                        row.Cells[3].Value = "NG";
                                                }
                                            }
                                            row.Cells[6].Value = item1.area;  //面积
                                            row.Cells[7].Value = item1.score; //分数
                                        }
                                    }
                                }
                            }
                        }
                        if (!isAdd)
                        {
                            if (row.Cells[2].Value != null && !String.IsNullOrEmpty(row.Cells[2].Value.ToString()) && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                            row.Cells[2].Value = ngNum;
                            row.Cells[3].Value = "--";
                            row.Cells[6].Value = "--";
                            row.Cells[7].Value = "--";
                        }
                        row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / frmOneImformation.AllNum * 100).ToString("F2") + "%"; //百分比
                    }
                }

                string strConnectString = "data source=" + SqlHelper.DBPath;
                SqlHelper.Connection = new SQLiteConnection(strConnectString);
                if (SqlHelper.Connection.State == ConnectionState.Closed)
                {
                    SqlHelper.Connection.Open();
                }
                string query = @"delete from NGTable";
                SQLiteCommand cmd1 = new SQLiteCommand(query);
                cmd1.Connection = SqlHelper.Connection;
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandText = query;
                cmd1.ExecuteNonQuery();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Index < dataGridView1.Rows.Count - 1)
                    {
                        SQLiteCommand cmd = new SQLiteCommand();
                        cmd.Connection = SqlHelper.Connection;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"insert into NGTable(节点,类型,个数,判定,总数,占比,面积,分数) values(@节点,@类型,@个数,@判定,@总数,@占比,@面积,@分数)";
                        cmd.Parameters.Add(new SQLiteParameter("@节点", row.Cells[0].Value));
                        cmd.Parameters.Add(new SQLiteParameter("@类型", row.Cells[1].Value));
                        cmd.Parameters.Add(new SQLiteParameter("@个数", row.Cells[2].Value));
                        cmd.Parameters.Add(new SQLiteParameter("@判定", "--"));
                        cmd.Parameters.Add(new SQLiteParameter("@总数", row.Cells[4].Value));
                        cmd.Parameters.Add(new SQLiteParameter("@占比", row.Cells[5].Value));
                        cmd.Parameters.Add(new SQLiteParameter("@面积", "0"));
                        cmd.Parameters.Add(new SQLiteParameter("@分数", "0"));
                        cmd.ExecuteScalar();
                    }
                }
                if (SqlHelper.Connection.State == ConnectionState.Open)
                    SqlHelper.Connection.Close();
            }));
        }
        //private async void showTable()
        //{
        //    await Task.Run(() =>
        //    {
        //        this.dataGridView1.BeginInvoke(Reference);
        //    });

        //    // 在 UI 线程上更新 datagridview
        //    if (this.dataGridView1.InvokeRequired)
        //    {
        //        // 如果不在 UI 线程上，则异步调用
        //        this.dataGridView1.BeginInvoke(new Action(() =>
        //        {
        //            this.dataGridView1.Refresh();
        //        }));
        //    }
        //    else
        //    {
        //        // 在 UI 线程上更新 datagridview
        //        this.dataGridView1.Refresh();
        //    }
        //}
        #endregion

        #region 初始化数据库
        private void loadDb()
        {
            SqlHelper.DBPath = AppDomain.CurrentDomain.BaseDirectory + "YT.db";
            string strConnectString = "data source=" + SqlHelper.DBPath;
            if (File.Exists(SqlHelper.DBPath))
            {
                try
                {
                    SqlHelper.Connection = new SQLiteConnection(strConnectString);
                    SqlHelper.Connection.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                try
                {
                    SQLiteConnection.CreateFile(SqlHelper.DBPath);
                    SqlHelper.Connection = new SQLiteConnection(strConnectString);
                    SqlHelper.Connection.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void loadTable()
        {
            if (!SqlHelper.ExistTable("NGTable", SqlHelper.DBPath))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"create table NGTable(节点 varchar(10),类型 varchar(10),个数 varchar(10),判定 varchar(10),总数 varchar(10),占比 varchar(10),面积 varchar(10),分数 varchar(10))";
                    command.ExecuteNonQuery();
                    TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                    node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                    foreach (var item in node.NodeInfo)
                    {
                        if(item.NodeId ==0)
                        {
                            SQLiteCommand cmd1 = new SQLiteCommand();
                            cmd1.Connection = SqlHelper.Connection;
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandText = @"insert into NGTable(节点,类型,个数,判定,总数,占比,面积,分数) values(@节点,@类型,@个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                            cmd1.Parameters.Add(new SQLiteParameter("@个数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                            cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                            cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                            cmd1.ExecuteScalar();
                        }
                        else if(item.NodeType==0||item.NodeType==2)
                        {
                            foreach (var item1 in item.ClassNames)
                            {
                                SQLiteCommand cmd1 = new SQLiteCommand();
                                cmd1.Connection = SqlHelper.Connection;
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = @"insert into NGTable(节点,类型,个数,判定,总数,占比,面积,分数) values(@节点,@类型,@个数,@判定,@总数,@占比,@面积,@分数)";
                                cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                                cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                                cmd1.Parameters.Add(new SQLiteParameter("@个数", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                                cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                                cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                                cmd1.ExecuteScalar();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        #endregion

        #region 初始化表格
        string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
        private void loadDataGridView()
        {
            TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
            node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
            SqlHelper.DBPath = AppDomain.CurrentDomain.BaseDirectory + "YT.db";
            string strConnectString = "data source=" + SqlHelper.DBPath;
            try
            {

                SqlHelper.Connection = new SQLiteConnection(strConnectString);
                SqlHelper.Connection.Open();
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();


                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from NGTable";
                cmd.ExecuteNonQuery();
                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "NGTable");
                dataGridView1.DataSource = dataSet.Tables["NGTable"].DefaultView;
                if (SqlHelper.Connection.State == ConnectionState.Open)
                    SqlHelper.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(ex.Message);
            }
        }
        #endregion

        #region 加载方案
        private async void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(IniHelper.SaveSetIni.Read("图片设置", "保存路径")) ||
                !File.Exists(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径")))
            {
                MessageBox.Show("文件不存在请先设置好路径");
                return;
            }
            button1.Enabled = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                progressBar1.BeginInvoke(new Action(() => { loadDataGridView(); }));
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 80)
                    {
                        if (!HTCSharpDemo.Program.loadDeepStudyHandle(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径"), ref studyHandle))
                        {
                            cancellationTokenSource.Cancel();
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                progressBar1.Value = 0;
                                button1.Enabled = true;
                                button1.Text = "加载方案";
                            }));
                            MessageBox.Show("方案加载失败");
                            break;
                        }
                        else
                        {
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                button1.Enabled = false;
                                button1.Text = "加载完毕";
                            }));
                        }
                    }
                }
            }, token);
        }
        #endregion

        #region 清除数据事件
        private void clearSql()
        {
            try
            {
                TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                foreach (var item in node.NodeInfo)
                {
                    if (item.NodeId == TreeFileHelper.dingweiNodeId)
                    {
                        SQLiteCommand cmd1 = new SQLiteCommand();
                        cmd1.Connection = SqlHelper.Connection;
                        cmd1.CommandType = CommandType.Text;
                        cmd1.CommandText = @"insert into NGTable(节点,类型,个数,判定,总数,占比,面积,分数) values(@节点,@类型,@个数,@判定,@总数,@占比,@面积,@分数)";
                        cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                        cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                        cmd1.Parameters.Add(new SQLiteParameter("@个数", "0"));
                        cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                        cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                        cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                        cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                        cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                        cmd1.ExecuteScalar();
                    }
                    else if (item.NodeType == 0 || item.NodeType == 2)
                    {
                        foreach (var item1 in item.ClassNames)
                        {
                            SQLiteCommand cmd1 = new SQLiteCommand();
                            cmd1.Connection = SqlHelper.Connection;
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandText = @"insert into NGTable(节点,类型,个数,判定,总数,占比,面积,分数) values(@节点,@类型,@个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                            cmd1.Parameters.Add(new SQLiteParameter("@个数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                            cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                            cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                            cmd1.ExecuteScalar();
                        }
                    }
                }
                dataGridView1.DataSource = null;
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from NGTable";

                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "NGTable");
                dataGridView1.DataSource = dataSet.Tables["NGTable"].DefaultView;
                if (SqlHelper.Connection.State == ConnectionState.Open)
                    SqlHelper.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 运行过程禁止操作软件
        private void runControl(string str)
        {
            if (str == "继续开始自动运行...")
                this.BeginInvoke(new Action(() =>
                {
                    if(studyHandle==IntPtr.Zero)
                        button1.Enabled = false;
                    btAddOK.Enabled = false;
                    btDelOK.Enabled = false;
                    cbOKItem.Enabled = false;
                    tbAddOK.Enabled = false;
                    btAddNG.Enabled = false;
                    btDelNG.Enabled = false;
                    cbNGItem.Enabled = false;
                    tbAddNG.Enabled = false;
                    btOKToNG.Enabled = false;
                    btNGToOK.Enabled = false;
                }));
            else if (str == "停止...")
                this.BeginInvoke(new Action(() =>
                {
                    if(studyHandle==IntPtr.Zero)
                        button1.Enabled = true;
                    btAddOK.Enabled = true;
                    btDelOK.Enabled = true;
                    cbOKItem.Enabled = true;
                    tbAddOK.Enabled = true;
                    btAddNG.Enabled = true;
                    btDelNG.Enabled = true;
                    cbNGItem.Enabled = true;
                    tbAddNG.Enabled = true;
                    btOKToNG.Enabled = true;
                    btNGToOK.Enabled = true;
                }));
        }
        #endregion

        #region OK类型添加
        string OKpath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "OKSet.txt";
        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbAddOK.Text))
            {
                MessageBox.Show("请先输入要添加的OK类型");
                return;
            }
            if (!File.Exists(OKpath))
                File.Create(OKpath).Close();
            string[] strs = File.ReadAllLines(OKpath);
            if (strs.Contains(tbAddOK.Text.Trim()))
            {
                MessageBox.Show($"{tbAddOK.Text}类型已存在，请不要重复添加");
                return;
            }
            if (File.Exists(NGpath))
            {
                string[] NGstr = File.ReadAllLines(NGpath);
                if (NGstr.Contains(tbAddOK.Text.Trim()))
                {
                    MessageBox.Show("此类型是NG类型，请先从NG类型中删除再添加");
                    return;
                }
            }
            File.AppendAllText(OKpath, tbAddOK.Text.Trim() + "\r\n");
            string[] strs1 = File.ReadAllLines(OKpath);
            cbOKItem.Items.Clear();
            OKType.Clear();
            foreach (var item in strs1)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbOKItem.Items.Add(item);
                    OKType.Add(item);
                }
            }
            cbOKItem.SelectedIndex = 0;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(OKpath))
            {
                MessageBox.Show("当前还未添加ok类型,请先添加");
                return;
            }
            if (String.IsNullOrEmpty(cbOKItem.Text))
            {
                MessageBox.Show("还未选择要编辑的项");
                return;
            }
            string[] strs = File.ReadAllLines(OKpath);
            if (strs.Length == 0)
            {
                MessageBox.Show("还未添加OK类型，请先添加");
                return;
            }
            foreach (string str in strs)
            {
                if (str == cbOKItem.Text.Trim())
                {
                    List<string> strings = new List<string>(strs);
                    strings.Remove(str);
                    strs = strings.ToArray();
                    File.WriteAllLines(OKpath, strs);
                    break;
                }
            }
            cbOKItem.Items.Clear();
            OKType.Clear();
            foreach (string str in strs)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    cbOKItem.Items.Add(str);
                    OKType.Add(str);
                }
            }
            if (cbOKItem.Items.Count > 0)
                cbOKItem.SelectedIndex = 0;
        }
        #endregion

        #region 添加NG类型
        string NGpath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "NGSet.txt";
        private void button4_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbAddNG.Text))
            {
                MessageBox.Show("请先输入要添加的NG类型");
                return;
            }
            if (!File.Exists(NGpath))
                File.Create(NGpath).Close();
            string[] strs = File.ReadAllLines(NGpath);
            if (strs.Contains(tbAddNG.Text.Trim()))
            {
                MessageBox.Show($"{tbAddNG.Text}类型已存在，请不要重复添加");
                return;
            }
            if(File.Exists(OKpath))
            {
                string[] OKstr= File.ReadAllLines(OKpath);
                if(OKstr.Contains(tbAddNG.Text.Trim()))
                {
                    MessageBox.Show("此类型是OK类型，请先从OK类型中删除再添加");
                    return;
                }
            }
            File.AppendAllText(NGpath, tbAddNG.Text.Trim() + "\r\n");
            string[] strs1 = File.ReadAllLines(NGpath);
            cbNGItem.Items.Clear();
            NGType.Clear();
            foreach (var item in strs1)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbNGItem.Items.Add(item);
                    NGType.Add(item);
                }
            }
            cbNGItem.SelectedIndex = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!File.Exists(NGpath))
            {
                MessageBox.Show("当前还未添加NG类型,删除失败，请先添加");
                return;
            }
            if (String.IsNullOrEmpty(cbNGItem.Text))
            {
                MessageBox.Show("还未选择要删除的项");
                return;
            }
            string[] strs = File.ReadAllLines(NGpath);
            if (strs.Length == 0)
            {
                MessageBox.Show("还未添加NG类型，请先添加");
                return;
            }
            foreach (string str in strs)
            {
                if (str == cbNGItem.Text.Trim())
                {
                    List<string> strings = new List<string>(strs);
                    strings.Remove(str);
                    strs = strings.ToArray();
                    File.WriteAllLines(NGpath, strs);
                    break;
                }
            }
            cbNGItem.Items.Clear();
            NGType.Clear();
            foreach (string str in strs)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    cbNGItem.Items.Add(str);
                    NGType.Add(str);
                }
            }
            if (cbNGItem.Items.Count > 0)
                cbNGItem.SelectedIndex = 0;
        }
        #endregion

        #region OK/NG类型互转
        private void button2_Click_1(object sender, EventArgs e)
        {
            string OKToNG = cbOKItem.Text;
            if (!File.Exists(OKpath))
            {
                MessageBox.Show("当前还未添加ok类型,请先添加");
                return;
            }
            if (String.IsNullOrEmpty(cbOKItem.Text))
            {
                MessageBox.Show("还未选择要编辑的项");
                return;
            }
            string[] strs = File.ReadAllLines(OKpath);
            if (strs.Length == 0)
            {
                MessageBox.Show("还未添加OK类型，请先添加");
                return;
            }
            foreach (string str in strs)
            {
                if (str == OKToNG)
                {
                    List<string> strings = new List<string>(strs);
                    strings.Remove(str);
                    strs = strings.ToArray();
                    File.WriteAllLines(OKpath, strs);
                    break;
                }
            }
            cbOKItem.Items.Clear();
            OKType.Clear();
            foreach (string str in strs)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    cbOKItem.Items.Add(str);
                    OKType.Add(str);
                }
            }
            if (cbOKItem.Items.Count > 0)
                cbOKItem.SelectedIndex = 0;

            if (!File.Exists(NGpath))
                File.Create(NGpath).Close();
            string[] strs1 = File.ReadAllLines(NGpath);
            if (strs1.Contains(OKToNG))
            {
                MessageBox.Show($"{OKToNG}类型已存在，请不要重复添加");
                return;
            }
            File.AppendAllText(NGpath, OKToNG + "\r\n");
            string[] strs2 = File.ReadAllLines(NGpath);
            cbNGItem.Items.Clear();
            NGType.Clear();
            foreach (var item in strs2)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbNGItem.Items.Add(item);
                    NGType.Add(item);
                }
            }
            if(cbNGItem.Items.Count>0)
                cbNGItem.SelectedIndex = 0;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string NGToOK = cbNGItem.Text;
            if (!File.Exists(NGpath))
            {
                MessageBox.Show("当前还未添加NG类型,请先添加");
                return;
            }
            if (String.IsNullOrEmpty(cbNGItem.Text))
            {
                MessageBox.Show("还未选择要编辑的项");
                return;
            }
            string[] strs = File.ReadAllLines(NGpath);
            if (strs.Length == 0)
            {
                MessageBox.Show("还未添加NG类型，请先添加");
                return;
            }
            foreach (string str in strs)
            {
                if (str == NGToOK)
                {
                    List<string> strings = new List<string>(strs);
                    strings.Remove(str);
                    strs = strings.ToArray();
                    File.WriteAllLines(NGpath, strs);
                    break;
                }
            }
            cbNGItem.Items.Clear();
            NGType.Clear();
            foreach (string str in strs)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    cbNGItem.Items.Add(str);
                    NGType.Add(str);
                }
            }
            if (cbNGItem.Items.Count > 0)
                cbNGItem.SelectedIndex = 0;

            if (!File.Exists(OKpath))
                File.Create(OKpath).Close();
            string[] strs1 = File.ReadAllLines(OKpath);
            if (strs1.Contains(NGToOK))
            {
                MessageBox.Show($"{NGToOK}类型已存在，请不要重复添加");
                return;
            }
            File.AppendAllText(OKpath, NGToOK + "\r\n");
            string[] strs2 = File.ReadAllLines(OKpath);
            cbOKItem.Items.Clear();
            OKType.Clear();
            foreach (var item in strs2)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    cbOKItem.Items.Add(item);
                    OKType.Add(item);
                }
            }
            if (cbOKItem.Items.Count > 0)
                cbOKItem.SelectedIndex = 0;
        }

        #endregion

    }
}
