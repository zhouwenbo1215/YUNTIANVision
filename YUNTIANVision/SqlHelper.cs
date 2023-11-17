using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace YUNTIANVision
{
    internal class SqlHelper
    {
        /// <summary>
        /// 数据库链接对象
        /// </summary>
        public static SQLiteConnection Connection = null;
        /// <summary>
        /// 数据库路径
        /// </summary>
        public static string DBPath;
        /// <summary>
        /// 判断数据库中表格是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="path">数据库路径</param>
        /// <returns>是否存在</returns>
        public static bool ExistTable(string tableName, string path)
        {
            if (System.IO.File.Exists(path) == false)
                SQLiteConnection.CreateFile(path);

            using (SQLiteConnection con = new SQLiteConnection(string.Format("Data Source={0};Version=3;", path)))
            {
                con.Open();
                string count = "0";
                //开启事务
                using (SQLiteTransaction tr = con.BeginTransaction())
                {
                    using (SQLiteCommand cmd = con.CreateCommand())
                    {
                        string existSql = String.Format("select count(*)  from sqlite_master where type='table' and name = '{0}'", tableName);

                        cmd.Transaction = tr;
                        cmd.CommandText = existSql;
                        //使用result = cmd.ExecuteNonQuery();这句判断返回值的方法不正确，不论是否存在返回值都为-1

                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            count = reader[0].ToString();
                        }
                    }
                    //提交事务
                    tr.Commit();
                }
                con.Close();
                if (count == "0")
                    return false;//没有该表格
                else
                    return true;
            }
        }
    }
}
