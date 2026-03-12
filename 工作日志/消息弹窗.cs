using System;
using System.Drawing;
using System.Windows.Forms;

namespace 对账平台
{
    public partial class 消息弹窗 : Form
    {
        // 声明系统托盘图标组件
        private NotifyIcon _notifyIcon;

        public 消息弹窗()
        {
            InitializeComponent();
            InitNotifyIcon(); // 初始化托盘组件
        }

        // 初始化系统托盘组件（适配新版.NET）
        private void InitNotifyIcon()
        {
            // 创建 NotifyIcon 实例
            _notifyIcon = new NotifyIcon();

            // 设置托盘图标（使用系统默认图标，也可替换为自定义.ico文件）
            _notifyIcon.Icon = SystemIcons.Information;

            // 设置是否在托盘显示图标（必须为true）
            _notifyIcon.Visible = true;

            // 绑定事件（可选）
            _notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        // 显示右下角弹窗提醒的核心方法（无BalloonTipEnabled也能正常显示）
        public void ShowBalloonNotification(string title, string message, int timeout = 5000)
        {
            // 空值校验
            if (string.IsNullOrEmpty(title)) title = "系统提醒";
            if (string.IsNullOrEmpty(message)) message = "暂无提醒内容";

            // 设置弹窗标题、内容、图标（核心属性保留）
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info; // 可选：Info/Warning/Error

            // 显示弹窗（timeout为显示时长，单位毫秒）
            _notifyIcon.ShowBalloonTip(timeout);
        }

        // 弹窗被点击的事件处理
        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // 点击弹窗后恢复窗体显示
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // 托盘图标点击事件
        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        // 窗体关闭时释放资源（关键：防止托盘图标残留）
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false; // 先隐藏
                _notifyIcon.Dispose(); // 再释放
            }
        }

        // 测试按钮点击事件（界面上的按钮触发弹窗）
        private void btnShowNotify_Click(object sender, EventArgs e)
        {
            // 调用弹窗方法
            ShowBalloonNotification("新消息提醒", "您今日有三笔异常对账信息，请及时核对查看！", 3000);
        }

        // WinForm设计器自动生成的组件初始化代码
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(消息弹窗));
            this.btnShowNotify = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnShowNotify
            // 
            this.btnShowNotify.Location = new System.Drawing.Point(50, 50);
            this.btnShowNotify.Name = "btnShowNotify";
            this.btnShowNotify.Size = new System.Drawing.Size(150, 40);
            this.btnShowNotify.TabIndex = 0;
            this.btnShowNotify.Text = "显示右下角提醒";
            this.btnShowNotify.UseVisualStyleBackColor = true;
            this.btnShowNotify.Click += new System.EventHandler(this.btnShowNotify_Click);
            // 
            // 消息弹窗
            // 
            this.ClientSize = new System.Drawing.Size(250, 150);
            this.Controls.Add(this.btnShowNotify);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "消息弹窗";
            this.Text = "右下角弹窗演示";
            this.ResumeLayout(false);

        }

        private Button btnShowNotify;
    }

}