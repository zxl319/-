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

namespace 对账平台
{
    public partial class 医院信息 : Form
    {
        private string idd = null;
        DataSet ds = new DataSet();

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

        public 医院信息()
        {
            InitializeComponent();
            SX_Click();
        }
        private void SX_Click()
        {
            dataGridView1.DataSource = null;
            string sqlgzrz = "SELECT  ID ,YYMC 医院名称, HOSPITAL_ID 医院ID,CASE WHEN BZ='0' THEN '正常' ELSE '作废' END 作废标志 FROM gzrz.dbo.YYXX WITH(NOLOCK) ";

            ds = SQLHelper.GetDataSet(sqlgzrz.ToString());
            dataGridView1.DataSource = ds.Tables[0];
            dataGridView1.Columns[1].Width = 160;//调整医院名称列宽
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string YYMC = this.YYMC.Text;
            string HOSPITAL_ID = this.HOSPITAL_ID.Text;
            string id = this.ID.Text;

            string sqlxz = "INSERT INTO gzrz.dbo.YYXX (YYMC, HOSPITAL_ID, BZ)" +
        " VALUES  ( '" + YYMC + "' ,'" + HOSPITAL_ID + "' ,'0') ";

            conn.Open();
            SqlCommand cmd1 = new SqlCommand(sqlxz, conn);

            if ((cmd1.ExecuteNonQuery()) == 1)
            {
                MessageBox.Show("新增成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show("新增失败！请联系管理员检查数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            conn.Close();
            SX_Click();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string YYMC = this.YYMC.Text;
            string HOSPITAL_ID = this.HOSPITAL_ID.Text;
            string id= this.ID.Text;
            if (id == "")
            {
                MessageBox.Show("请选择一条！");
            }
            else
            {
                conn.Open();
                string sqlxg = "UPDATE gzrz.dbo.YYXX SET YYMC='" + YYMC + "',HOSPITAL_ID='" + HOSPITAL_ID + "' WHERE ID='" + id + "'";
                SqlCommand cmd1 = new SqlCommand(sqlxg, conn);

                if ((cmd1.ExecuteNonQuery()) == 1)
                {
                    MessageBox.Show("修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    MessageBox.Show("修改失败！请联系管理员检查数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                conn.Close();
                SX_Click();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string YYMC = this.YYMC.Text;
            string HOSPITAL_ID = this.HOSPITAL_ID.Text;
            string id = this.ID.Text;
            if (id == "")
            {
                MessageBox.Show("请选择一条！");
            }
            else
            {
                conn.Open();
                string sqlxg = "UPDATE gzrz.dbo.YYXX SET bz='1' WHERE ID='" + id + "'";
                SqlCommand cmd1 = new SqlCommand(sqlxg, conn);

                if ((cmd1.ExecuteNonQuery()) == 1)
                {
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    MessageBox.Show("删除成功！请联系管理员检查数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                conn.Close();
                SX_Click();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                string ID = dataGridView1.CurrentRow.Cells["ID"].Value.ToString();
                string YYMC = dataGridView1.CurrentRow.Cells["医院名称"].Value.ToString();
                string HOSPITAL_ID = dataGridView1.CurrentRow.Cells["医院ID"].Value.ToString();
                if (ID != "")
                {
                    this.YYMC.Text = YYMC;
                    this.HOSPITAL_ID.Text = HOSPITAL_ID;
                    this.ID.Text = ID;
                }
                else
                {
                    MessageBox.Show("无记录可操作！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch
            {
                MessageBox.Show("无记录可操作！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
