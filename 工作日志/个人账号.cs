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
using System.Collections;

namespace 对账平台
{
    public partial class 个人账号 : Form
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

        public 个人账号()
        {
            InitializeComponent();
            string sqlgzrz = "SELECT  ID ,FZMC FROM gzrz.dbo.FZXX where BZ='0'";

            var qsDatable = SQLHelper.GetDataSet(sqlgzrz).Tables[0];

            ArrayList arr1 = new ArrayList();
            if (qsDatable != null && qsDatable.Rows.Count > 0)
            {
                for (int i = 0; i < qsDatable.Rows.Count; i++)
                {
                    string id = qsDatable.Rows[i]["ID"].ToString();
                    string FZMC = qsDatable.Rows[i]["FZMC"].ToString();
                    arr1.Add(new DictionaryEntry(id, FZMC));
                }
            }
            FZ.DataSource = arr1;
            FZ.DisplayMember = "Value";
            FZ.ValueMember = "Key";
            FZ.SelectedIndex = 0;
            dataGridView1.DataSource = null;
            SX_Click();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string GRMC = this.GRMC.Text;
            string FZ = this.FZ.Text;
            string id = this.ID.Text;

            string sqlxz = "INSERT INTO gzrz.dbo.GRXX (GRMC, FZ, BZ)" +
        " VALUES  ( '" + GRMC + "' ,'" + FZ + "' ,'0') ";

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
        private void SX_Click()
        {
            dataGridView1.DataSource = null;
            string sqlgzrz = "SELECT  ID ,GRMC 姓名,FZ 分组,CASE WHEN BZ = '1' THEN '作废'  ELSE '正常' END 标志 FROM gzrz.dbo.GRXX  ";

            ds = SQLHelper.GetDataSet(sqlgzrz.ToString());
            dataGridView1.DataSource = ds.Tables[0];


        }

        private void button2_Click(object sender, EventArgs e)
        {
            string GRMC = this.GRMC.Text;
            string FZ = this.FZ.SelectedItem.ToString();
            string id = this.ID.Text;
            if (id == "")
            {
                MessageBox.Show("请选择一条！");
            }
            else
            {
                conn.Open();
                string sqlxg = "UPDATE gzrz.dbo.GRXX SET GRMC='" + GRMC + "',FZ='" + FZ + "' WHERE ID='" + id + "'";
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
            string GRMC = this.GRMC.Text;
            string FZ = this.FZ.SelectedItem.ToString();
            string id = this.ID.Text;
            if (id == "")
            {
                MessageBox.Show("请选择一条！");
            }
            else
            {
                conn.Open();
                string sqlxg = "UPDATE gzrz.dbo.GRXX SET bz='1' WHERE ID='" + id + "'";
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

        private void button5_Click(object sender, EventArgs e)
        {
            string GRMC = this.GRMC.Text;
            string FZ = this.FZ.SelectedItem.ToString();
            string id = this.ID.Text;
            if (id == "")
            {
                MessageBox.Show("请选择一条！");
            }
            else
            {
                conn.Open();
                string sqlxg = "UPDATE gzrz.dbo.GRXX SET bz='0' WHERE ID='" + id + "'";
                SqlCommand cmd1 = new SqlCommand(sqlxg, conn);

                if ((cmd1.ExecuteNonQuery()) == 1)
                {
                    MessageBox.Show("恢复成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    MessageBox.Show("恢复成功！请联系管理员检查数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                conn.Close();
                SX_Click();
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            try
            { 
                string ID = dataGridView1.CurrentRow.Cells["ID"].Value.ToString();
                string GRMC = dataGridView1.CurrentRow.Cells["姓名"].Value.ToString();
                string FZ = dataGridView1.CurrentRow.Cells["分组"].Value.ToString();
                if (ID != "")
                {
                    this.GRMC.Text = GRMC;
                    this.FZ.Text = FZ;
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
