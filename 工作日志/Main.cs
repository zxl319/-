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
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void 添加配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            医院信息 A1 = new 医院信息();
            A1.Owner = this;
            A1.ShowDialog();
        }


        private void 添加服务器信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            服务器信息 A1 = new 服务器信息();
            A1.Owner = this;
            A1.ShowDialog();

        }

        private void 添加个人账号信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            个人账号 A1 = new 个人账号();
            A1.Owner = this;
            A1.ShowDialog();

        }

        private void 对账日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            对账日志 A1 = new 对账日志();
            A1.Owner = this;
            A1.ShowDialog();
        }

        private void 下载账单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            下载账单 A1 = new 下载账单();
            A1.Owner = this;
            A1.ShowDialog();
        }

        private void 消息弹窗ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            消息弹窗 A1 = new 消息弹窗();
            A1.Owner = this;
            A1.ShowDialog();
        }

        private void 下载支付宝账单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            下载支付宝账单 A1 = new 下载支付宝账单();
            A1.Owner = this;
            A1.ShowDialog();

        }

        private void 自动下载配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            自动抽取 A1 = new 自动抽取();
            A1.Owner = this;
            A1.ShowDialog();
        }
    }
}
