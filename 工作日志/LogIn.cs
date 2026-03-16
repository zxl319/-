using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 对账平台
{
    public partial class LogIn : Form
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string usercode = user.Text.ToString();
            string userpassword = password.Text.ToString();

            //判断账号密码


            //成功
            if (usercode == "admin")
            {
                this.Hide();
                Main xx = new Main();
                xx.ShowDialog();
                this.Close();
            }
            //失败
            else
            {
                MessageBox.Show("用户名或密码输入错误，请重试！");
                user.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LogIn_Load(object sender, EventArgs e)
        {
            user.Focus();
        }

        private void user_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                password.Focus();
            }
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }
    }
}
