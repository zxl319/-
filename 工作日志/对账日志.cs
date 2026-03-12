using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace 对账平台
{
    public partial class 对账日志 : Form
    {
        DataSet ds = new DataSet();

        private SqlConnection conn = new SqlConnection("Data Source=127.0.0.1;Initial Catalog=gzrz;Persist Security Info=True;User ID=power ;Password=massunsoft009 ");
        public static SqlConnection CreateConn()
        {
            SqlConnection sqlcon = new SqlConnection("Data Source=127.0.0.1;Initial Catalog=DZPT;Persist Security Info=True;User ID=sa ;Password=admin123 ");
            return sqlcon;
        }
        /// <summary>
        /// 数据操作类
        /// </summary>
        public class  SQLHelper
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


        private void addGridButton()
        {
            DataGridViewButtonColumn dgv_button_col = new DataGridViewButtonColumn();
            DataGridViewCheckBoxColumn ck = new DataGridViewCheckBoxColumn();
            // 设定列的名字
            dgv_button_col.Name = "Detail";
            // 在所有按钮上表示"查看详情"
            dgv_button_col.UseColumnTextForButtonValue = true;
            dgv_button_col.Text = "查看差异账单明细";
            // 设置列标题
            dgv_button_col.HeaderText = "操作";
            int lastIndex = dataGridView1.Columns.Count;
            // 向DataGridView追加
            dataGridView1.Columns.Insert(lastIndex, dgv_button_col);

            //DataGridViewButtonColumn dgv_button_upLoad = new DataGridViewButtonColumn();
            //// 设定列的名字
            //dgv_button_upLoad.Name = "Upload";
            //// 在所有按钮上表示"查看详情"
            //dgv_button_upLoad.UseColumnTextForButtonValue = true;
            //dgv_button_upLoad.Text = "上传首页";
            //// 设置列标题
            //dgv_button_upLoad.HeaderText = "上传";
            //// 向DataGridView追加
            //dataGridView1.Columns.Insert(0, dgv_button_upLoad);

            //DataGridViewButtonColumn dgv_button_ssxx = new DataGridViewButtonColumn();
            //// 设定列的名字
            //dgv_button_ssxx.Name = "ssxx";
            //// 在所有按钮上表示"查看详情"
            //dgv_button_ssxx.UseColumnTextForButtonValue = true;
            //dgv_button_ssxx.Text = "查看分值信息";
            //// 设置列标题
            //dgv_button_ssxx.HeaderText = "检查";
            // 向DataGridView追加
            //dataGridView1.Columns.Insert(0, dgv_button_ssxx);
        }
        /// <summary>

        public 对账日志()
        {
            InitializeComponent();
            Type.SelectedIndex = 0;//初始化时间类型
            KSSJ.Value = DateTime.Today;
            JSSJ.Value = DateTime.Today.AddDays(1).AddMilliseconds(-1);

            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT * FROM DZPT.dbo.Reconciliation where 1=1");

            strSql.Append(@" and 账单日期=CONVERT( VARCHAR(8),GETDATE(),112)");
            
            dataGridView1.DataSource = null;

            ds = SQLHelper.GetDataSet(strSql.ToString());
            dataGridView1.DataSource = ds.Tables[0];
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            addGridButton();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT * FROM DZPT.dbo.Reconciliation where 1=1");
            strSql.Append(@" and 账单日期>='" + KSSJ.Value.ToString("yyyy-MM-dd") + "'");
            strSql.Append(@" and 账单日期<='" + JSSJ.Value.ToString("yyyy-MM-dd") + "'");

            ds = SQLHelper.GetDataSet(strSql.ToString());


            dataGridView1.DataSource = ds.Tables[0];


            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

        }
    }
}
