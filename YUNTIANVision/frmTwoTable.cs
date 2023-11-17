using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace YUNTIANVision
{
    public partial class frmTwoTable : Form
    {
        public frmTwoTable()
        {
            InitializeComponent();
            frmTwoCamera.myShowTwoDelegate1 += new showTwoDelegate1(showTwoTable1);
            frmTwoCamera.myShowTwoDelegate2 += new showTwoDelegate2(showTwoTable2);
            frmTwoCamera.myCloseTwoDelegate += new closeTwoDelegate(closeTwoTable);
            frmMain.controlEvent += new controlDelegate(runControl);
            frmTwoImformation.clearSqlEvent += new clearSqlDelegate2(clearSql);
            reference += ReferenceItems;
            reference2 += ReferenceItems2;
        }
        private event Action reference;
        private event Action reference2;
        /// <summary>
        /// OK类型种类
        /// </summary>
        public static List<string> OKType = new List<string>();
        /// <summary>
        /// NG类型种类
        /// </summary>
        public static List<string> NGType = new List<string>();
        /// <summary>
        /// 深度学习模型句柄
        /// </summary>
        public static IntPtr studyHandle;
        string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
        public static int test_num;
        #region 相机1添加数据事件
        public void ReferenceItems()
        {
            TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
            nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
            foreach (DataGridViewRow row in this.dataGridView3.Rows)
            {
                if (dataGridView3.Rows.Count - 1 > row.Index)
                {
                    double ngNum = 0;
                    bool isAdd = false;
                    row.Cells[4].Value = frmTwoImformation.AllNum1;
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
                                        foreach (var item2 in nodeInfo.NodeInfo)
                                        {
                                            if (row.Cells[0].Value != null && !String.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                                            {
                                                if ((string)row.Cells[0].Value == item2.NodeName)
                                                {
                                                    if (item2.NodeId <= 3 && item2.NodeType == 1)
                                                    {
                                                        if (HTCSharpDemo.Program.res1[item2.NodeId].Count == 1)
                                                            row.Cells[3].Value = "OK"; //判定
                                                        else
                                                            row.Cells[3].Value = "定位失败"; //判定
                                                        goto loop;
                                                    }
                                                }
                                            }
                                        }
                                        if (frmTwoCamera.result1=="OK")
                                            row.Cells[3].Value = "OK"; //判定
                                        else
                                            row.Cells[3].Value = "NG";
                                        loop:
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
                    row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / frmTwoImformation.AllNum1 * 100).ToString("F2") + "%"; //百分比
                }
            }

            string strConnectString = "data source=" + SqlHelper.DBPath;
            SqlHelper.Connection = new SQLiteConnection(strConnectString);
            if (SqlHelper.Connection.State == ConnectionState.Closed)
            {
                SqlHelper.Connection = new SQLiteConnection(strConnectString);
                SqlHelper.Connection.Open();
            }
            string query = @"delete from TwoCameraNGTableOne";
            SQLiteCommand cmd1 = new SQLiteCommand(query);
            cmd1.Connection = SqlHelper.Connection;
            cmd1.CommandType = CommandType.Text;
            cmd1.CommandText = query;
            cmd1.ExecuteNonQuery();
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                if (row.Index < dataGridView3.Rows.Count - 1)
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = SqlHelper.Connection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"insert into TwoCameraNGTableOne(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                    cmd.Parameters.Add(new SQLiteParameter("@节点", row.Cells[0].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@类型", row.Cells[1].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@结果个数", row.Cells[2].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@判定", "--"));
                    cmd.Parameters.Add(new SQLiteParameter("@总数", row.Cells[4].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@占比", row.Cells[5].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@面积", "0"));
                    cmd.Parameters.Add(new SQLiteParameter("@分数", "0"));
                    cmd.ExecuteScalar();
                }
            }
        }
        private async void showTwoTable1()
        {
            await Task.Run(() =>
            {
                dataGridView3.BeginInvoke(reference);
            });

            // 在 UI 线程上更新 ListView
            if (this.dataGridView3.InvokeRequired)
            {
                // 如果不在 UI 线程上，则异步调用
                this.dataGridView3.BeginInvoke(new Action(() =>
                {
                    this.dataGridView3.Refresh();
                }));
            }
            else
            {
                // 在 UI 线程上更新 ListView
                this.dataGridView3.Refresh();
            }
        }
        #endregion

        #region 相机2添加数据事件
        public void ReferenceItems2()
        {
            TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
            nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
            foreach (DataGridViewRow row in this.dataGridView4.Rows)
            {
                if (dataGridView4.Rows.Count - 1 > row.Index)
                {
                    double ngNum = 0;
                    bool isAdd = false;
                    row.Cells[4].Value = frmTwoImformation.AllNum2;
                    foreach (var item in HTCSharpDemo.Program.res2)
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
                                        foreach (var item2 in nodeInfo.NodeInfo)
                                        {
                                            if (row.Cells[0].Value != null && !String.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                                            {
                                                if ((string)row.Cells[0].Value == item2.NodeName)
                                                {
                                                    if (item2.NodeId <= 3 && item2.NodeType == 1)
                                                    {
                                                        if (HTCSharpDemo.Program.res2[item2.NodeId].Count == 1)
                                                            row.Cells[3].Value = "OK"; //判定
                                                        else
                                                            row.Cells[3].Value = "定位失败"; //判定
                                                        goto loop;
                                                    }
                                                }
                                            }
                                        }
                                        if (frmTwoCamera.result2=="OK")
                                            row.Cells[3].Value = "OK"; //判定
                                        else
                                            row.Cells[3].Value = "NG";
                                        loop:
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
                    row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / frmTwoImformation.AllNum2 * 100).ToString("F2") + "%"; //百分比
                }
            }

            string strConnectString = "data source=" + SqlHelper.DBPath;
            SqlHelper.Connection = new SQLiteConnection(strConnectString);
            if (SqlHelper.Connection.State == ConnectionState.Closed)
            {
                SqlHelper.Connection = new SQLiteConnection(strConnectString);
                SqlHelper.Connection.Open();
            }
            string query = @"delete from TwoCameraNGTableTwo";
            SQLiteCommand cmd1 = new SQLiteCommand(query);
            cmd1.Connection = SqlHelper.Connection;
            cmd1.CommandType = CommandType.Text;
            cmd1.CommandText = query;
            cmd1.ExecuteNonQuery();
            foreach (DataGridViewRow row in dataGridView4.Rows)
            {
                if (row.Index < dataGridView4.Rows.Count - 1)
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = SqlHelper.Connection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"insert into TwoCameraNGTableTwo(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                    cmd.Parameters.Add(new SQLiteParameter("@节点", row.Cells[0].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@类型", row.Cells[1].Value));
                    cmd.Parameters.Add(new SQLiteParameter("@结果个数", row.Cells[2].Value));
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
        }
        private async void showTwoTable2()
        {
            await Task.Run(() =>
            {
                dataGridView4.BeginInvoke(reference2);
            });

            // 在 UI 线程上更新 ListView
            if (this.dataGridView4.InvokeRequired)
            {
                // 如果不在 UI 线程上，则异步调用
                this.dataGridView4.BeginInvoke(new Action(() =>
                {
                    this.dataGridView4.Refresh();
                }));
            }
            else
            {
                // 在 UI 线程上更新 ListView
                this.dataGridView4.Refresh();
            }
        }
        #endregion

        #region load/close
        private void frmTwoTable_Load(object sender, EventArgs e)
        {
            dataGridView3.RowHeadersVisible = false;
            dataGridView4.RowHeadersVisible = false;
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
            loadTable1();
            loadTable2();
        }
        private void closeTwoTable()
        {
            this.Close();
        }
        private void frmTwoTable_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmTwoCamera.myShowTwoDelegate1 -= new showTwoDelegate1(showTwoTable1);
            frmTwoCamera.myShowTwoDelegate2 -= new showTwoDelegate2(showTwoTable2);
            frmTwoCamera.myCloseTwoDelegate -= new closeTwoDelegate(closeTwoTable);
            frmMain.controlEvent -= new controlDelegate(runControl);
            frmTwoImformation.clearSqlEvent -= new clearSqlDelegate2(clearSql);
        }
        #endregion

        #region 初始化表格
        
        private void loadDataGridView1()
        {
            try
            {
                TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                SqlHelper.DBPath = AppDomain.CurrentDomain.BaseDirectory + "YT.db";
                string strConnectString = "data source=" + SqlHelper.DBPath;
                SqlHelper.Connection = new SQLiteConnection(strConnectString);
                SqlHelper.Connection.Open();
                dataGridView3.DataSource = null;
                dataGridView3.Rows.Clear();
                dataGridView3.Columns.Clear();

                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from TwoCameraNGTableOne";
                cmd.ExecuteNonQuery();
                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "TwoCameraNGTableOne");
                dataGridView3.DataSource = dataSet.Tables["TwoCameraNGTableOne"].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(ex.Message);
            }
        }
        private void loadDataGridView2()
        {
            try
            {
                TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                dataGridView4.Rows.Clear();
                dataGridView4.Columns.Clear();
                dataGridView4.DataSource = null;
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from TwoCameraNGTableTwo";
                cmd.ExecuteNonQuery();
                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "TwoCameraNGTableTwo");
                dataGridView4.DataSource = dataSet.Tables["TwoCameraNGTableTwo"].DefaultView;
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
                progressBar1.BeginInvoke(new Action(() => { loadDataGridView1();
                    loadDataGridView2();
                }));
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 80)
                    {
                        if (!HTCSharpDemo.Program.loadDeepStudyHandle(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径"), ref studyHandle,ref test_num))
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

        #region 添加OK类型
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
        #endregion

        #region 删除ok类型
        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(OKpath))
            {
                MessageBox.Show("当前还未添加ok类型,删除失败，请先添加");
                return;
            }
            if (String.IsNullOrEmpty(cbOKItem.Text))
            {
                MessageBox.Show("还未选择要删除的项");
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

        #region 运行期间禁止操作软件
        private void runControl(string str)
        {
            if (str == "继续开始自动运行...")
                this.BeginInvoke(new Action(() =>
                {
                    if (studyHandle == IntPtr.Zero)
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
            if (str == "停止...")
                this.BeginInvoke(new Action(() =>
                {
                    if (studyHandle == IntPtr.Zero)
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

        #region 数据清零
        private void clearSql()
        {
            try
            {
                TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                dataGridView3.DataSource = null;
                dataGridView3.Rows.Clear();
                dataGridView3.Columns.Clear();
                foreach (var item in node.NodeInfo)
                {
                    if (item.NodeId == TreeFileHelper.dingweiNodeId)
                    {
                        SQLiteCommand cmd1 = new SQLiteCommand();
                        cmd1.Connection = SqlHelper.Connection;
                        cmd1.CommandType = CommandType.Text;
                        cmd1.CommandText = @"insert into TwoCameraNGTableOne(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                        cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                        cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                        cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
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
                            cmd1.CommandText = @"insert into TwoCameraNGTableOne(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                            cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                            cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                            cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                            cmd1.ExecuteScalar();
                        }
                    }
                }
                dataGridView3.DataSource = null;
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from TwoCameraNGTableOne";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "TwoCameraNGTableOne");
                dataGridView3.DataSource = dataSet.Tables["TwoCameraNGTableOne"].DefaultView;
                clearSql2();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }

        private void clearSql2()
        {
            try
            {
                TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                dataGridView4.DataSource = null;
                dataGridView4.Rows.Clear();
                dataGridView4.Columns.Clear();
                foreach (var item in node.NodeInfo)
                {
                    if (item.NodeId == TreeFileHelper.dingweiNodeId)
                    {
                        SQLiteCommand cmd1 = new SQLiteCommand();
                        cmd1.Connection = SqlHelper.Connection;
                        cmd1.CommandType = CommandType.Text;
                        cmd1.CommandText = @"insert into TwoCameraNGTableTwo(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                        cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                        cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                        cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
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
                            cmd1.CommandText = @"insert into TwoCameraNGTableTwo(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                            cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                            cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@面积", "--"));
                            cmd1.Parameters.Add(new SQLiteParameter("@分数", "--"));
                            cmd1.ExecuteScalar();
                        }
                    }
                }
                dataGridView4.DataSource = null;
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SqlHelper.Connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select * from TwoCameraNGTableTwo";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                sda.Fill(dataSet, "TwoCameraNGTableTwo");
                dataGridView4.DataSource = dataSet.Tables["TwoCameraNGTableTwo"].DefaultView;

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

        private void loadTable1()
        {
            if (!SqlHelper.ExistTable("TwoCameraNGTableOne", SqlHelper.DBPath))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"create table TwoCameraNGTableOne(节点 varchar(10),类型 varchar(10),结果个数 varchar(10),判定 varchar(10),总数 varchar(10),占比 varchar(10),面积 varchar(10),分数 varchar(10))";
                    command.ExecuteNonQuery();
                    TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                    node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                    foreach (var item in node.NodeInfo)
                    {
                        if (item.NodeId == 0)
                        {
                            SQLiteCommand cmd1 = new SQLiteCommand();
                            cmd1.Connection = SqlHelper.Connection;
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandText = @"insert into TwoCameraNGTableOne(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                            cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
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
                                cmd1.CommandText = @"insert into TwoCameraNGTableOne(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                                cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                                cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                                cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
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
        private void loadTable2()
        {
            if (!SqlHelper.ExistTable("TwoCameraNGTableTwo", SqlHelper.DBPath))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"create table TwoCameraNGTableTwo(节点 varchar(10),类型 varchar(10),结果个数 varchar(10),判定 varchar(10),总数 varchar(10),占比 varchar(10),面积 varchar(10),分数 varchar(10))";
                    command.ExecuteNonQuery();
                    TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
                    node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                    foreach (var item in node.NodeInfo)
                    {
                        if (item.NodeId == 0)
                        {
                            SQLiteCommand cmd1 = new SQLiteCommand();
                            cmd1.Connection = SqlHelper.Connection;
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandText = @"insert into TwoCameraNGTableTwo(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                            cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                            cmd1.Parameters.Add(new SQLiteParameter("@类型", item.ClassNames[0]));
                            cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                            cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                            cmd1.Parameters.Add(new SQLiteParameter("@面积", "-"));
                            cmd1.Parameters.Add(new SQLiteParameter("@分数", "-"));
                            cmd1.ExecuteScalar();
                        }
                        else if (item.NodeType == 0 || item.NodeType == 2)
                        {
                            foreach (var item1 in item.ClassNames)
                            {
                                SQLiteCommand cmd1 = new SQLiteCommand();
                                cmd1.Connection = SqlHelper.Connection;
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = @"insert into TwoCameraNGTableTwo(节点,类型,结果个数,判定,总数,占比,面积,分数) values(@节点,@类型,@结果个数,@判定,@总数,@占比,@面积,@分数)";
                                cmd1.Parameters.Add(new SQLiteParameter("@节点", item.NodeName));
                                cmd1.Parameters.Add(new SQLiteParameter("@类型", item1));
                                cmd1.Parameters.Add(new SQLiteParameter("@结果个数", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@判定", "NG"));
                                cmd1.Parameters.Add(new SQLiteParameter("@总数", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@占比", "0"));
                                cmd1.Parameters.Add(new SQLiteParameter("@面积", "-"));
                                cmd1.Parameters.Add(new SQLiteParameter("@分数", "-"));
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

        #region 编辑NG类型
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
            if (File.Exists(OKpath))
            {
                string[] OKstr = File.ReadAllLines(OKpath);
                if (OKstr.Contains(tbAddNG.Text.Trim()))
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

        #region NG/OK类型互转
        private void btOKToNG_Click(object sender, EventArgs e)
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

            if(!File.Exists(NGpath))
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
            if (cbNGItem.Items.Count > 0)
                cbNGItem.SelectedIndex = 0;
        }

        private void btNGToOK_Click(object sender, EventArgs e)
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

