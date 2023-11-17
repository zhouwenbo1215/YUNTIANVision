using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YUNTIANVision
{
    internal class ListViewHelper:Control
    {
        public  ListView listView1 =new ListView();
        private int width;
        public  ListViewHelper(ListView listView ,int wid)
        {
            width =wid;
            this.listView1 =listView;
        }
        public void addHeader()
        {
            listView1.Columns.Add("", 0);
            listView1.Columns.Add("节点", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("类型", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("结果个数", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("判定", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("面积", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("分数", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("位置X", width / 8, HorizontalAlignment.Center);
            listView1.Columns.Add("位置Y", width / 8, HorizontalAlignment.Center);
        }
        public void addItem(string name, string type, int num, string result)
        {
            ListViewItem lv = new ListViewItem();
            lv.SubItems.Add(name);
            lv.SubItems.Add(type);
            lv.SubItems.Add(num.ToString());
            lv.SubItems.Add(result);
            listView1.Items.Add(lv);
        }
        public void addQueItem(string name, string type, int num, string result, int area, float score, int x, int y)
        {
            ListViewItem lv = new ListViewItem();
            lv.SubItems.Add(name);
            lv.SubItems.Add(type);
            lv.SubItems.Add(num.ToString());
            lv.SubItems.Add(result);
            lv.SubItems.Add(area.ToString());
            lv.SubItems.Add(score.ToString());
            lv.SubItems.Add(x.ToString());
            lv.SubItems.Add(y.ToString());
            listView1.Items.Add(lv);
        }
    }
}
