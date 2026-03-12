using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.Linq; // 用于XDocument解析

namespace 对账平台
{
    public partial class 自动抽取 : Form
    {
        public 自动抽取()
        {
            InitializeComponent();
            Initialization();
        }
        public void Initialization()
        {
            DBHelper db = new DBHelper();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT config_value FROM DZPT.dbo.Config WHERE config_name='Automatic_upload'");
            DataSet m = db.select_sb(sb.ToString());

            XDocument xDoc = XDocument.Parse(m.Tables[0].Rows[0][0].ToString()); 
            string WXupload = xDoc.Root.Element("WXupload")?.Value;
            string ZFBupload = xDoc.Root.Element("ZFBupload")?.Value;
            string YLupload = xDoc.Root.Element("YLupload")?.Value;

            if (WXupload == "1")
            {
                checkBox1.Checked = true;
            }
            if (ZFBupload == "1")
            {
                checkBox2.Checked = true;
            }
            if (YLupload == "1")
            {
                checkBox3.Checked = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DBHelper db = new DBHelper();
            string WXupload = "0";
            if (checkBox1.Checked == true)
            {
                 WXupload = "1";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("EXEC DZPT.dbo.UpdateXmlConfigNode @In_ConfigName = 'Automatic_upload', ");
            sb.Append("                      @In_ConfigValue = '"+WXupload+"' ,");
            sb.Append("                      @In_NodePath = N'/config/WXupload/text()'");
            int u = db.insert_sb(sb.ToString());
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            DBHelper db = new DBHelper();
            string YFBupload = "0";
            if (checkBox2.Checked == true)
            {
                YFBupload = "1";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("EXEC DZPT.dbo.UpdateXmlConfigNode @In_ConfigName = 'Automatic_upload', ");
            sb.Append("                      @In_ConfigValue = '" + YFBupload + "' ,");
            sb.Append("                      @In_NodePath = N'/config/YFBupload/text()'");
            int u = db.insert_sb(sb.ToString());

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            DBHelper db = new DBHelper();
            string YLupload = "0";
            if (checkBox3.Checked == true)
            {
                YLupload = "1";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("EXEC DZPT.dbo.UpdateXmlConfigNode @In_ConfigName = 'Automatic_upload', ");
            sb.Append("                      @In_ConfigValue = '" + YLupload + "' ,");
            sb.Append("                      @In_NodePath = N'/config/YLupload/text()'");
            int u = db.insert_sb(sb.ToString());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            DateTime yesterday = now.AddDays(-1);

            if (now.Hour == 10 && now.Minute == 0 && now.Second == 0)
            {
                下载账单.WXZD(yesterday.ToString("yyyy-MM-dd"));
            }
            if (now.Hour == 10 && now.Minute == 5 && now.Second == 0)
            {
                下载支付宝账单.ZFBZD(yesterday.ToString("yyyy-MM-dd"));
            }
        }
    }
}
