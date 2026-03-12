using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text;

namespace 对账平台
{
    public partial class 下载账单 : Form, IDisposable
    {
        // 配置参数
        private static string MCH_ID;
        private static string API_KEY;
        private static string APPID;
        private static string MCH_PRIVATE_KEY; // 商户私钥（V3签名用）
        private static string SERIAL_NO;       // 商户证书序列号（V3签名用）
        private static string APPLY_BILL_URL;
        private static string DOWNLOAD_BILL_URL;
        private static HttpClient _httpClient;
        private static char _csvSeparator = ','; // CSV分隔符


        public 下载账单()
        {
            InitializeComponent();
            button1.Text = "申请并下载微信交易账单1、111";

            // 初始化HttpClient（设置超时和默认请求头）
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 读取配置（需在App.config中配置）
            MCH_ID = ConfigurationManager.AppSettings["WxPay_MchId"];
            API_KEY = ConfigurationManager.AppSettings["WxPay_ApiKey"];
            APPID = ConfigurationManager.AppSettings["WxPay_AppId"];
            SERIAL_NO = ConfigurationManager.AppSettings["WxPay_SerialNo"];             // 证书序列号

            // 2. 相对路径读取私钥文件（核心修改）
            // 获取程序运行根目录（发布后是exe所在目录，调试时是bin/Debug目录）
            string appRootPath = AppDomain.CurrentDomain.BaseDirectory;
            // 拼接相对路径：根目录 → Config → apiclient_key.pem
            string privateKeyFilePath = Path.Combine(appRootPath, "Config", "apiclient_key.pem");

            // 校验文件是否存在
            if (!File.Exists(privateKeyFilePath))
            {
                // 兼容调试场景（bin/Debug/Config 不存在时，向上找项目根目录的Config）
                string projectRootPath = Path.GetFullPath(Path.Combine(appRootPath, "../../../"));
                privateKeyFilePath = Path.Combine(projectRootPath, "Config", "apiclient_key.pem");

                if (!File.Exists(privateKeyFilePath))
                {
                    throw new FileNotFoundException("商户私钥文件不存在", privateKeyFilePath);
                }
            }

            // 读取私钥文件内容（UTF-8编码，无BOM）
            MCH_PRIVATE_KEY = File.ReadAllText(privateKeyFilePath, new UTF8Encoding(false));

            // 配置校验
            var missingConfigs = new List<string>();
            if (string.IsNullOrEmpty(MCH_ID)) missingConfigs.Add("商户号(MchId)");
            if (string.IsNullOrEmpty(API_KEY)) missingConfigs.Add("V2密钥(ApiKey)");
            if (string.IsNullOrEmpty(APPID)) missingConfigs.Add("AppId");
            if (string.IsNullOrEmpty(MCH_PRIVATE_KEY)) missingConfigs.Add("商户私钥(MchPrivateKey)");
            if (string.IsNullOrEmpty(SERIAL_NO)) missingConfigs.Add("证书序列号(SerialNo)");

            if (missingConfigs.Any())
            {
                MessageBox.Show($"请先在App.config中配置以下微信支付信息：\n{string.Join("\n", missingConfigs)}",
                    "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                button1.Enabled = false;
            }


            
        }


        #region 工具方法1：V2接口MD5签名
        private static string CreateSign(SortedDictionary<string, string> parameters)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kv in parameters)
                {
                    if (string.IsNullOrWhiteSpace(kv.Value) || kv.Key.Equals("sign", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sb.Append($"{kv.Key}={kv.Value}&");
                }
                sb.Append($"key={API_KEY}");
                string signStr = sb.ToString().TrimEnd('&');

                using (var md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signStr));
                    StringBuilder signBuilder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        signBuilder.Append(b.ToString("X2")); // 大写MD5
                    }
                    return signBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("V2签名生成失败：" + ex.Message);
            }
        }
        #endregion

        #region 工具方法2：V3接口SHA256-RSA签名（适配GET请求）
        private static string GetV3AuthorizationHeader(HttpMethod method, string url, string body = "")
        {
            try
            {
                var nonce = Guid.NewGuid().ToString().Replace("-", "");
                // 修正：确保时间戳是秒级（避免毫秒级导致签名错误）
                var timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                var uri = new Uri(url);
                // 修正：URL路径+查询参数需保留原始编码（避免Uri类自动解码导致签名不一致）
                var pathAndQuery = uri.PathAndQuery;

                // 严格按微信要求拼接签名串（末尾必须加\n）
                var signStr = $"{method.ToString().ToUpper()}\n{pathAndQuery}\n{timestamp}\n{nonce}\n{body}\n";

                // 原签名逻辑不变
                var privateKey = ReadPrivateKey(MCH_PRIVATE_KEY);
                using (var rsa = RSA.Create())
                {
                    rsa.ImportParameters(privateKey);
                    var signBytes = rsa.SignData(Encoding.UTF8.GetBytes(signStr), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    var sign = Convert.ToBase64String(signBytes);

                    return $"WECHATPAY2-SHA256-RSA2048 mchid=\"{MCH_ID}\",nonce_str=\"{nonce}\",timestamp=\"{timestamp}\",serial_no=\"{SERIAL_NO}\",signature=\"{sign}\"";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("V3签名生成失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 解析PKCS#8格式商户私钥（BouncyCastle 1.9.0终极版，解决所有编译错误）
        /// </summary>
        private static RSAParameters ReadPrivateKey(string privateKeyStr)
        {
            try
            {
                // 1. 预处理私钥：保留PEM头尾，清理无效字符
                string cleanKey = privateKeyStr
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Trim();

                // 2. 1.9.0版本标准PEM解析（无PemObject/GetInstance依赖）
                AsymmetricKeyParameter privateKey = null;
                using (var reader = new StringReader(cleanKey))
                {
                    // 明确指定OpenSsl下的PemReader，避免歧义
                    var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
                    object obj = pemReader.ReadObject();

                    // 区分PKCS#1（AsymmetricCipherKeyPair）和PKCS#8（AsymmetricKeyParameter）
                    if (obj is AsymmetricCipherKeyPair keyPair)
                    {
                        privateKey = keyPair.Private;
                    }
                    else if (obj is AsymmetricKeyParameter param)
                    {
                        privateKey = param;
                    }
                    else
                    {
                        // 兜底解析：手动提取Base64并解析PKCS#8私钥
                        string base64Key = cleanKey
                            .Replace("-----BEGIN PRIVATE KEY-----", "")
                            .Replace("-----END PRIVATE KEY-----", "")
                            .Replace("\n", "")
                            .Trim();
                        byte[] keyBytes = Convert.FromBase64String(base64Key);
                        privateKey = PrivateKeyFactory.CreateKey(keyBytes);
                    }
                }

                if (privateKey == null)
                {
                    throw new Exception("解析失败：未读取到有效的RSA私钥");
                }

                // 3. 1.9.0版本PKCS#8转RsaPrivateCrtKeyParameters（替代GetInstance）
                RsaPrivateCrtKeyParameters rsaParams = null;
                if (privateKey is RsaPrivateCrtKeyParameters)
                {
                    rsaParams = (RsaPrivateCrtKeyParameters)privateKey;
                }
                else
                {
                    // 1.9.0版本核心适配：用RsaPrivateKeyStructure替代GetInstance
                    PrivateKeyInfo pkcs8KeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
                    Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(pkcs8KeyInfo.ParsePrivateKey().GetDerEncoded());
                    RsaPrivateKeyStructure rsaPrivStruct = RsaPrivateKeyStructure.GetInstance(seq);

                    // 手动构造RsaPrivateCrtKeyParameters（1.9.0标准写法）
                    rsaParams = new RsaPrivateCrtKeyParameters(
                        rsaPrivStruct.Modulus,
                        rsaPrivStruct.PublicExponent,
                        rsaPrivStruct.PrivateExponent,
                        rsaPrivStruct.Prime1,
                        rsaPrivStruct.Prime2,
                        rsaPrivStruct.Exponent1,
                        rsaPrivStruct.Exponent2,
                        rsaPrivStruct.Coefficient);
                }

                if (rsaParams == null)
                {
                    throw new Exception("私钥解析失败：未转换为RSA私钥参数");
                }

                // 4. 转换为.NET原生RSAParameters（无兼容问题）
                return DotNetUtilities.ToRSAParameters(rsaParams);
            }
            catch (Exception ex)
            {
                throw new Exception($"BouncyCastle 1.9.0私钥解析失败：{ex.Message}\n私钥格式：PKCS#8（BEGIN PRIVATE KEY）");
            }
        }
        #endregion

        #region 工具方法3：拼接URL参数（适配GET请求）
        /// <summary>
        /// 将参数字典拼接为URL QueryString
        /// </summary>
        private static string BuildQueryString(Dictionary<string, string> parameters)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kv in parameters)
            {
                if (!string.IsNullOrWhiteSpace(kv.Value))
                {
                    query[kv.Key] = kv.Value;
                }
            }
            return query.ToString();
        }
        #endregion

        #region 功能1：V3接口申请账单（GET版本）
        private static async Task<string> ApplyWeChatBill(string billDate)
        {
            string billType = "ALL";
            try
            {
                // 1. 转换日期格式（V3要求yyyy-MM-dd，用户选择的是yyyyMMdd）
                var billDateV3 = DateTime.ParseExact(billDate, "yyyyMMdd", null).ToString("yyyy-MM-dd");

                // 2. 构造GET参数（拼接到URL）
                var parameters = new Dictionary<string, string>
                {
                    { "mchid", MCH_ID },
                    { "bill_date", billDateV3 },
                    { "bill_type", billType }
                };

                // 3. 拼接完整URL
                string queryString = BuildQueryString(parameters);
                string fullUrl = $"{APPLY_BILL_URL}?{queryString}";

                // 4. 构造V3 GET请求（添加签名头，body为空）
                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Add("Authorization", GetV3AuthorizationHeader(HttpMethod.Get, fullUrl, "")); // GET请求body为空
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var userAgentProduct = new ProductInfoHeaderValue("ReconciliationPlatform", "1.0");
                _httpClient.DefaultRequestHeaders.UserAgent.Add(userAgentProduct);

                // 5. 调用接口
                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return $"V3申请账单失败（{response.StatusCode}）：{responseContent}";
                }
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();

                // 6. 解析下载地址
                dynamic applyObj = JsonConvert.DeserializeObject(result);
                return applyObj.download_url ?? null; // 成功返回下载地址，失败返回null
            }
            catch (Exception ex)
            {
                return $"V3申请账单失败：{ex.Message}";
            }
        }
        #endregion

        #region 功能2：下载账单（GET版本，兼容V3下载地址+V2直接下载）
        private static async Task<string> DownloadWeChatBill(string billDate, string billType = "ALL", string downloadUrl = null)
        {
            try
            {
                // 场景1：使用V3申请的下载地址（GET请求+V3签名）
                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                    request.Headers.Add("Authorization", GetV3AuthorizationHeader(HttpMethod.Get, downloadUrl, "")); // GET body为空
                    request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                    request.Headers.Add("hash_type", "SHA1");
                    request.Headers.ConnectionClose = true; // 关闭长连接，避免流截断
                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead); // 确保内容完整读取

                    response.EnsureSuccessStatusCode();

                    return await ProcessBillResponse(response);
                }

                // 场景2：V2接口直接下载（GET请求+MD5签名，参数拼URL）
                var parameters = new SortedDictionary<string, string>
                {
                    { "appid", APPID },
                    { "mch_id", MCH_ID },
                    { "bill_date", billDate },
                    { "bill_type", billType },
                    { "nonce_str", Guid.NewGuid().ToString().Replace("-", "") },
                    { "sign_type", "MD5" }
                };
                // 生成签名并添加到参数
                parameters.Add("sign", CreateSign(parameters));

                // 拼接V2 GET请求的URL
                string queryString = BuildQueryString(parameters.ToDictionary(kv => kv.Key, kv => kv.Value));
                string fullV2Url = $"{DOWNLOAD_BILL_URL}?{queryString}";

                // 发送V2 GET请求
                var v2Response = await _httpClient.GetAsync(fullV2Url);
                v2Response.EnsureSuccessStatusCode();
                string content = await ProcessBillResponse(v2Response);

                // 校验V2返回是否为错误XML
                if (content.Contains("<xml>"))
                {
                    return $"V2下载失败：{content}";
                }
                return content;
            }
            catch (Exception ex)
            {
                return $"下载账单失败：{ex.Message}";
            }
        }

        /// <summary>
        /// 统一处理账单响应（解压+编码转换）- 使用二进制流方式
        /// </summary>
        private static async Task<string> ProcessBillResponse(HttpResponseMessage response)
        {
            try
            {
                // 禁用HttpClient自动编码干预，保留原始字节
                if (response.Content.Headers.ContentType != null)
                {
                    response.Content.Headers.ContentType.CharSet = null;
                }

                // 步骤1：读取完整的响应字节流
                using (var memoryStream = new MemoryStream())
                {
                    await response.Content.CopyToAsync(memoryStream);
                    byte[] responseBytes = memoryStream.ToArray();

                    if (responseBytes.Length == 0)
                        throw new Exception("响应字节流为空");

                    byte[] finalBytes = responseBytes;

                    // 步骤2：如果是GZIP压缩，使用二进制流解压
                    if (response.Content.Headers.ContentEncoding.Any(enc =>
                        enc.Equals("gzip", StringComparison.OrdinalIgnoreCase)))
                    {
                        using (var compressedStream = new MemoryStream(responseBytes))
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        using (var outputMemoryStream = new MemoryStream())
                        {
                            // 使用二进制流完全复制解压数据
                            byte[] buffer = new byte[4096]; // 4KB缓冲区
                            int bytesRead;

                            while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await outputMemoryStream.WriteAsync(buffer, 0, bytesRead);
                            }

                            finalBytes = outputMemoryStream.ToArray();
                        }
                    }

                    // 步骤3：使用GBK编码将二进制数据转换为字符串（微信账单固定GBK）
                    string billContent = Encoding.GetEncoding("utf-8").GetString(finalBytes);

                    // 清理不可见字符（可选：解决部分特殊字符乱码）
                    //billContent = billContent.Replace("\0", "").Trim();

                    return billContent;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"账单响应处理失败：{ex.Message}");
            }
        }
        #endregion

        #region CSV字符串转DataTable - 使用二进制流处理
        /// <summary>
        /// 将CSV格式的字符串转换为DataTable - 改进的二进制流处理
        /// </summary>
        private DataTable CsvToDataTable(string csvContent)
        {
            try
            {
                // 将CSV内容转换为字节数组，然后用二进制流处理
                byte[] csvBytes = Encoding.GetEncoding("GBK").GetBytes(csvContent);

                using (var memoryStream = new MemoryStream(csvBytes))
                using (var streamReader = new StreamReader(memoryStream, Encoding.GetEncoding("GBK")))
                {
                    var dataTable = new DataTable();
                    string line;
                    bool isFirstLine = true;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        // 处理表头（第一行为列名）
                        if (isFirstLine)
                        {
                            string[] headers = ParseCsvLine(line);
                            foreach (string header in headers)
                            {
                                // 替换可能导致DataTable列名冲突的字符
                                string cleanHeader = header.Trim().Replace("\r", "");
                                if (!string.IsNullOrEmpty(cleanHeader))
                                {
                                    // 确保列名唯一
                                    string columnName = cleanHeader;
                                    int counter = 1;
                                    while (dataTable.Columns.Contains(columnName))
                                    {
                                        columnName = $"{cleanHeader}_{counter++}";
                                    }
                                    dataTable.Columns.Add(columnName, typeof(string));
                                }
                            }
                            isFirstLine = false;
                        }
                        else
                        {
                            // 处理数据行
                            string lineTrimmed = line.Trim();
                            if (string.IsNullOrEmpty(lineTrimmed)) continue;

                            string[] values = ParseCsvLine(lineTrimmed);
                            if (values.Length > 0)
                            {
                                DataRow row = dataTable.NewRow();

                                // 确保值的数量不超过列数
                                int valueCount = Math.Min(values.Length, dataTable.Columns.Count);
                                for (int j = 0; j < valueCount; j++)
                                {
                                    // 移除回车符，处理中文字符
                                    string value = values[j].Replace("\r", "").Trim();

                                    // 使用GBK编码确保中文字符正确显示
                                    byte[] valueBytes = Encoding.GetEncoding("GBK").GetBytes(value);
                                    string processedValue = Encoding.GetEncoding("GBK").GetString(valueBytes);

                                    row[j] = processedValue;
                                }

                                dataTable.Rows.Add(row);
                            }
                        }
                    }

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV转DataTable失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 解析CSV行，处理包含逗号的字段（用双引号包围）
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 双引号转义
                        currentField += '"';
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        // 切换引号状态
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            fields.Add(currentField);
            return fields.ToArray();
        }
        #endregion

        #region 核心按钮点击事件（完整流程）
        private async void button1_Click(object sender, EventArgs e)
        {
            WXZD(dateTimePicker1.Text.ToString());
        }
        #endregion

        public static async void WXZD(string Datetime)
        {

            DBHelper db = new DBHelper();

            StringBuilder sd = new StringBuilder();
            sd.Append("SELECT config_value FROM DZPT.dbo.Config WHERE config_name='WX_Config'");
            DataSet op = db.select_sb(sd.ToString());

            XDocument xDoc = XDocument.Parse(op.Tables[0].Rows[0][0].ToString());
            APPLY_BILL_URL = xDoc.Root.Element("WxPayApplyBillUrl")?.Value;
            DOWNLOAD_BILL_URL = xDoc.Root.Element("WxPayDownloadBillUrl")?.Value;


            try
            {
                // 1. 选择账单日期
                string billDate = Datetime.Replace("-", "");
                string billDate1 = Datetime;

                // 3. 先尝试V3接口申请账单
                string downloadUrl = await ApplyWeChatBill(billDate);
                string billContent = string.Empty;

                if (!string.IsNullOrEmpty(downloadUrl) && !downloadUrl.Contains("失败"))
                {
                    // V3申请成功，使用下载地址下载
                    billContent = await DownloadWeChatBill(billDate, "ALL", downloadUrl);
                }
                else
                {
                    // V3申请失败，降级使用V2接口直接下载
                    MessageBox.Show("V3接口申请账单失败，尝试使用V2接口下载...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    billContent = await DownloadWeChatBill(billDate, "ALL");
                }

                // 4. 校验账单内容并加载到DataGridView
                if (string.IsNullOrEmpty(billContent) || billContent.Contains("失败"))
                {
                    MessageBox.Show($"账单下载失败：{billContent}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //去掉表头
                billContent = billContent.Substring(billContent.IndexOf("`"));
                //去掉汇总
                billContent = billContent.Substring(0, billContent.IndexOf("总交易单数"));
                //去掉特殊符号
                billContent = billContent.Replace("`", "").Replace("\n", "");
                //按换行符分割
                string[] strayy = billContent.Split('\r');

                if (strayy.Length > 0)
                {
                    int n = db.insert_sb(" DELETE FROM [DZPT].[dbo].[WxTradeBill] WHERE CONVERT(DATE,交易时间)='" + billDate1 + "' ");

                    for (int i = 0; i < strayy.Length; i++)
                    {
                        //按逗号分割
                        string[] item = strayy[i].Split(',');
                        if (item.Length > 26)
                        {
                            StringBuilder sb = new StringBuilder();

                            sb.Append(" INSERT INTO [DZPT].[dbo].[WxTradeBill] ([交易时间],[公众账号ID],[商户号],[特约商户号],[设备号],[微信订单号],[商户订单号],[用户标识], ");
                            sb.Append(" [交易类型],[交易状态],[付款银行],[货币种类],[应结订单金额],[代金券金额],[微信退款单号],[商户退款单号],[退款金额],[充值券退款金额], ");
                            sb.Append(" [退款类型],[退款状态],[商品名称],[商户数据包],[手续费],[费率],[订单金额],[申请退款金额],[费率备注]) ");
                            sb.Append("  VALUES('" + item[0].ToString() + "','" + item[1].ToString() + "','" + item[2].ToString() + "','" + item[3].ToString() + "','" + item[4].ToString() + "','" + item[5].ToString() + "','" + item[6].ToString() + "','" + item[7].ToString() + "','" + item[8].ToString() + "', ");
                            sb.Append("  '" + item[9].ToString() + "','" + item[10].ToString() + "','" + item[11].ToString() + "','" + item[12].ToString() + "','" + item[13].ToString() + "','" + item[14].ToString() + "','" + item[15].ToString() + "','" + item[16].ToString() + "','" + item[17].ToString() + "', ");
                            sb.Append("  '" + item[18].ToString() + "','" + item[19].ToString() + "','" + item[20].ToString() + "','" + item[21].ToString() + "','" + item[22].ToString() + "','" + item[23].ToString() + "','" + item[24].ToString() + "','" + item[25].ToString() + "','" + item[26].ToString() + "') ");
                            int m = db.insert_sb(sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载账单时发生异常：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region 资源释放
        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
