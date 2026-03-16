using Alipay.AopSdk.Core;
using Alipay.AopSdk.Core.Request;
using Alipay.AopSdk.Core.Response;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace 对账平台
{
    public partial class 下载支付宝账单 : Form
    {
        // 支付宝配置
        private static string _alipayAppId;
        private static string _alipayPrivateKey ;
        private static string _alipayPublicKey;
        private static string _alipayUrl;

        // 数据库配置（从App.config读取）
        private static readonly string _dbConnStr = ConfigurationManager.AppSettings["DB_ConnectionString"];

        // 账单常量
        private const string TRADE_BILL_TYPE = "trade";
        private const string TRADE_BILL_SUB_TYPE = "all"; 

        public 下载支付宝账单()
        {
            InitializeComponent();
            InitFormControls();
            btnDownloadBill.Text = "下载支付宝账单并直接入库";
        }

        #region 1. 基础初始化

        private void InitFormControls()
        {
            // 日期选择器
            dtpBillDate.Format = DateTimePickerFormat.Custom;
            dtpBillDate.CustomFormat = "yyyy-MM-dd";
            dtpBillDate.ShowUpDown = true;

            // DataGridView样式
            dgvBillData.AllowUserToAddRows = false;
            dgvBillData.ReadOnly = true;
            dgvBillData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBillData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
        #endregion

        #region 2. 数据库操作工具（核心）
        /// <summary>
        /// 执行SQL增删改（防SQL注入）
        /// </summary>
        private static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(_dbConnStr))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"数据库操作失败：{ex.Message}\nSQL：{sql}");
                }
            }
        }
        /// <summary>
        /// 批量插入支付宝账单数据（先删后插，避免重复）
        /// </summary>
        private static void BatchInsertAlipayBill(string billDate, List<string[]> billData)
        {
            // 1. 删除当日已有数据（按账单日期过滤）
            string deleteSql = $@"DELETE FROM [DZPT].[dbo].[AlipayTradeBill] 
                                  WHERE CONVERT(DATE, 完成时间) = @BillDate";
            ExecuteNonQuery(deleteSql, new[] { new SqlParameter("@BillDate", billDate) });

            // 2. 批量插入（事务保证一致性）
            using (SqlConnection conn = new SqlConnection(_dbConnStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    foreach (var row in billData)
                    {
                        if (row.Length < 10) continue; // 字段数不足跳过

                        string insertSql = $@"INSERT INTO [DZPT].[dbo].[AlipayTradeBill]
                                             (支付宝交易号,商户订单号,业务类型,商品名称,创建时间,完成时间,门店编号,门店名称,操作员,终端号,对方账户,[订单金额（元）],[商家实收（元）],[支付宝红包（元）],[集分宝（元）],[支付宝优惠（元）],[商家优惠（元）],[券核销金额（元）],券名称,[商家红包消费金额（元）],[卡消费金额（元）],[退款批次号/请求号],[服务费（元）],[分润（元）],备注)
                                             VALUES (@支付宝交易号,@商户订单号,@业务类型,@商品名称,@创建时间,@完成时间,@门店编号,@门店名称,
                                                     @操作员,@终端号,@对方账户,@订单金额,@商家实收,@支付宝红包,@集分宝,@支付宝优惠,@商家优惠,
                                                     @券核销金额,@券名称,@商家红包消费金额,@卡消费金额,@退款批次号,@服务费,@分润,@备注)";

                        SqlCommand cmd = new SqlCommand(insertSql, conn, tran);
                        // 参数化赋值（适配支付宝账单字段，可根据实际表头调整）
                        cmd.Parameters.AddWithValue("@支付宝交易号", row[0]);          // 支付宝交易号
                        cmd.Parameters.AddWithValue("@商户订单号", row[1]);    // 商户订单号
                        cmd.Parameters.AddWithValue("@业务类型", row[2]);      // 业务类型
                        cmd.Parameters.AddWithValue("@商品名称", row[3]);          // 商品名称
                        cmd.Parameters.AddWithValue("@创建时间", row[4]);        // 创建时间
                        cmd.Parameters.AddWithValue("@完成时间", row[5]);          // 完成时间
                        cmd.Parameters.AddWithValue("@门店编号", row[6] ?? "");            // 门店编号
                        cmd.Parameters.AddWithValue("@门店名称", row[7] ?? "");    // 门店名称
                        cmd.Parameters.AddWithValue("@操作员", row[8] ?? "");       // 操作员
                        cmd.Parameters.AddWithValue("@终端号", row[9] ?? "");     // 终端号
                        cmd.Parameters.AddWithValue("@对方账户", row[10]);          // 对方账户
                        cmd.Parameters.AddWithValue("@订单金额", row[11]);    // 订单金额
                        cmd.Parameters.AddWithValue("@商家实收", row[12]);      // 商家实收
                        cmd.Parameters.AddWithValue("@支付宝红包", row[13]);          // 支付宝红包
                        cmd.Parameters.AddWithValue("@集分宝", row[14]);        // 集分宝
                        cmd.Parameters.AddWithValue("@支付宝优惠", row[15]);          // 支付宝优惠
                        cmd.Parameters.AddWithValue("@商家优惠", row[16]);            // 商家优惠
                        cmd.Parameters.AddWithValue("@券核销金额", row[17]);    // 券核销金额
                        cmd.Parameters.AddWithValue("@券名称", row[18]);       // 券名称
                        cmd.Parameters.AddWithValue("@商家红包消费金额", row[19]);     // 商家红包消费金额
                        cmd.Parameters.AddWithValue("@卡消费金额", row[20]);    // 卡消费金额
                        cmd.Parameters.AddWithValue("@退款批次号", row[21]);       // 退款批次号
                        cmd.Parameters.AddWithValue("@服务费", row[22]);     // 服务费
                        cmd.Parameters.AddWithValue("@分润", row[23]);    // 分润
                        cmd.Parameters.AddWithValue("@备注", row[24] ?? "");       // 备注

                        cmd.ExecuteNonQuery();
                    }
                    tran.Commit(); // 提交事务
                }
                catch (Exception ex)
                {
                    tran.Rollback(); // 出错回滚
                    throw new Exception($"批量插入失败：{ex.Message}");
                }
            }
        }
        #endregion

        #region 3. 支付宝接口核心逻辑（新增ZIP解压）
        /// <summary>
        /// 获取支付宝账单下载链接
        /// </summary>
        private static string GetBillDownloadUrl(string billDate)
        {
            try
            {
                IAopClient client = new DefaultAopClient(
                    _alipayUrl,
                    _alipayAppId,
                    _alipayPrivateKey,
                    "json",
                    "1.0",
                    "RSA2",
                    _alipayPublicKey,
                    "UTF-8",
                    false
                );

                var request = new AlipayDataDataserviceBillDownloadurlQueryRequest();
                request.BizContent = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    bill_type = TRADE_BILL_TYPE,
                    bill_date = billDate,
                    bill_sub_type = TRADE_BILL_SUB_TYPE,
                    compress = "true" // 明确要求返回压缩包（支付宝默认返回ZIP）
                });

                AlipayDataDataserviceBillDownloadurlQueryResponse response = client.Execute(request);
                if (response.IsError)
                {
                    string errorMsg = response.SubMsg;
                    switch (response.SubCode)
                    {
                        case "DATA_NOT_EXIST": errorMsg += "（该日期无交易记录）"; break;
                        case "ILLEGAL_ARGUMENT": errorMsg += "（日期格式错误）"; break;
                        case "NO_PRIVILEGE": errorMsg += "（无下载权限）"; break;
                    }
                    throw new Exception($"支付宝接口错误：{errorMsg}（错误码：{response.SubCode}）");
                }

                return response.BillDownloadUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取链接失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 下载ZIP压缩包并解压读取内部CSV（核心修改）
        /// </summary>
        private static List<string[]> ParseAlipayBillZip(string downloadUrl)
        {
            int lineNumber = 0;
            var billData = new List<string[]>();

            // 1. 下载ZIP文件到内存流
            byte[] zipBytes;
            using (WebClient client = new WebClient())
            {
                zipBytes = client.DownloadData(downloadUrl);
            }

            // 2. 解压ZIP并读取CSV
            using (MemoryStream zipMs = new MemoryStream(zipBytes))
            using (ZipArchive archive = new ZipArchive(zipMs, ZipArchiveMode.Read))
            {
                // 找到ZIP内的CSV文件（支付宝账单ZIP内只有一个CSV）
                ZipArchiveEntry csvEntry = null;
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith("业务明细.csv", StringComparison.OrdinalIgnoreCase))
                    {
                        csvEntry = entry;
                        break;
                    }
                }

                if (csvEntry == null)
                {
                    throw new Exception("ZIP压缩包内未找到CSV账单文件");
                }

                // 3. 读取CSV内容并解析
                using (Stream csvStream = csvEntry.Open())
                using (StreamReader reader = new StreamReader(csvStream, Encoding.GetEncoding("GBK")))
                {
                    string line;
                    bool isHeader = true; // 跳过表头行
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // 跳过表头和汇总行
                        if (isHeader)
                        {
                            isHeader = false;
                            continue;
                        }
                        if (lineNumber >= 6 && !line.Contains("汇总") && !line.Contains("总交易笔数"))
                        {

                            // 解析CSV行（处理双引号包裹的字段）
                            string[] fields = ParseCsvLine(line);
                            if (fields.Length > 1) billData.Add(fields);
                        }
                    }
                }
            }

            return billData;
        }

        /// <summary>
        /// 通用CSV行解析（兼容支付宝格式）
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            fields.Add(currentField.ToString().Trim());
            return fields.ToArray();
        }
        #endregion

        #region 4. 核心按钮事件（下载→解压→解析→入库→查询）
        private async void btnDownloadBill_Click(object sender, EventArgs e)
        {
            ZFBZD(dtpBillDate.Value.ToString("yyyy-MM-dd"));
        }
        #endregion

        public static async void ZFBZD(string Datetime)
        {
            DBHelper db = new DBHelper();

            StringBuilder sd = new StringBuilder();
            sd.Append("SELECT config_value FROM DZPT.dbo.Config WHERE config_name='ZFB_Config'");
            DataSet op = db.select_sb(sd.ToString());

            XDocument xDoc = XDocument.Parse(op.Tables[0].Rows[0][0].ToString());
            _alipayUrl = xDoc.Root.Element("alipayUrl")?.Value;

            sd = new StringBuilder();
            sd.Append("SELECT * FROM DZPT.dbo.WxMerchant WHERE Flag='0'");
            DataSet oo = db.select_sb(sd.ToString());

            if (oo.Tables[0].Rows.Count == 1)
            {
                _alipayAppId = oo.Tables[0].Rows[0][0].ToString();
                _alipayPrivateKey = oo.Tables[0].Rows[0][2].ToString();
                _alipayPublicKey = oo.Tables[0].Rows[0][1].ToString();

                // 1. 获取账单日期
                string billDate = Datetime;

                // 2. 异步获取下载链接
                string downloadUrl = GetBillDownloadUrl(billDate);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new Exception("获取账单下载链接失败");
                }
                // 3. 异步下载ZIP并解析CSV（替换原有直接读取逻辑）
                List<string[]> billData = ParseAlipayBillZip(downloadUrl);

                // 4. 批量插入数据库
                BatchInsertAlipayBill(billDate, billData);

            }
            else if (oo.Tables[0].Rows.Count > 1)
            {
                for (int i = 0; i < oo.Tables[0].Rows.Count; i++)
                {
                    _alipayAppId = oo.Tables[0].Rows[i][0].ToString();
                    _alipayPrivateKey = oo.Tables[0].Rows[i][2].ToString();
                    _alipayPublicKey = oo.Tables[0].Rows[i][1].ToString();

                    // 1. 获取账单日期
                    string billDate = Datetime;

                    // 2. 异步获取下载链接
                    string downloadUrl = GetBillDownloadUrl(billDate);
                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        throw new Exception("获取账单下载链接失败");
                    }
                    // 3. 异步下载ZIP并解析CSV（替换原有直接读取逻辑）
                    List<string[]> billData = ParseAlipayBillZip(downloadUrl);

                    // 4. 批量插入数据库
                    BatchInsertAlipayBill(billDate, billData);

                }
            }
        }
    }
}