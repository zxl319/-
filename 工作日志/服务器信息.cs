using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Data;
using System.Collections;

namespace 对账平台
{
    public partial class 服务器信息 : Form
    {

        private SqlConnection conn = new SqlConnection("Data Source=127.0.0.1;Initial Catalog=gzrz;Persist Security Info=True;User ID=power ;Password=massunsoft009 ");
        public static SqlConnection CreateConn()
        {
            SqlConnection sqlcon = new SqlConnection("Data Source=127.0.0.1;Initial Catalog=gzrz;Persist Security Info=True;User ID=power ;Password=massunsoft009 ");
            return sqlcon;
        }
        /// <summary>
        /// 数据操作类
        /// </summary>
        public class SQLHelper
        {
            #region  ADO.NET访问组件
            private static SqlConnection conn;
            private static SqlCommand cmd;
            private static SqlDataReader sdr;
            private static SqlDataAdapter sda;
            private static DataSet ds;
            #endregion

            /// <summary>
            /// 增删改操作
            /// </summary>
            /// <param name="sql"></param>
            /// <returns></returns>
            public static int RunSQL(string sql)
            {
                int i = 0;
                try
                {
                    using (conn = CreateConn())
                    {
                        conn.Open();
                        using (cmd = new SqlCommand(sql, conn))
                        {
                            i = cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return i;
            }

            /// <summary>
            /// 查询，返回DataSet数据集
            /// </summary>
            /// <param name="sql"></param>
            /// <returns></returns>
            public static DataSet GetDataSet(string sql)
            {
                try
                {
                    using (conn = CreateConn())
                    {
                        using (sda = new SqlDataAdapter(sql, conn))
                        {
                            ds = new DataSet();
                            sda.Fill(ds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return ds;
            }

            /// <summary>
            /// 查询，返回SqlDataReader
            /// </summary>
            /// <param name="sql"></param>
            /// <returns></returns>
            public static SqlDataReader GetSqlDataReader(string sql)
            {
                try
                {
                    conn = CreateConn();
                    conn.Open();
                    cmd = new SqlCommand(sql, conn);
                    sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return sdr;
            }




        }

        public 服务器信息()
        {
            InitializeComponent();
            string sqlgzrz = "SELECT  ID ,YYMC FROM gzrz.dbo.YYXX ";

            var qsDatable = SQLHelper.GetDataSet(sqlgzrz).Tables[0];

            ArrayList arr1 = new ArrayList();
            if (qsDatable != null && qsDatable.Rows.Count > 0)
            {
                for (int i = 0; i < qsDatable.Rows.Count; i++)
                {
                    string id = qsDatable.Rows[i]["ID"].ToString();
                    string YYMC = qsDatable.Rows[i]["YYMC"].ToString();
                    arr1.Add(new DictionaryEntry(id, YYMC));
                }
            }
            yy.DataSource = arr1;
            yy.DisplayMember = "Value";
            yy.ValueMember = "Key";
            yy.SelectedIndex = 0;
        }
    }
}
