using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace 对账平台
{
    public class DBHelper
    {
        // 数据库连接字符串（配置到App.config）
        private static readonly string _connStr = System.Configuration.ConfigurationManager.AppSettings["DB_ConnectionString"]
            ?? "Data Source=.;Initial Catalog=ReconciliationDB;Integrated Security=True;Encrypt=False";

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public static IDbConnection GetConnection()
        {
            var conn = new Microsoft.Data.SqlClient.SqlConnection(_connStr);
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        /// <summary>
        /// 批量插入账单数据
        /// </summary>
        public static int BatchInsertWxTradeBill(List<WxTradeBillModel> billList)
        {
            if (billList == null || billList.Count == 0)
                return 0;

            using (var conn = GetConnection())
            {
                // 批量插入SQL（适配Dapper批量操作）
                var sql = @"INSERT INTO WxTradeBill (
                                BillDate, TransactionId, OutTradeNo, TransactionTime,
                                TradeType, TradeStatus, TotalAmount, PayerAmount,
                                RefundAmount, PayerRefundAmount, ProductName, MerchantName,
                                MerchantId, PayerOpenId
                            ) VALUES (
                                @BillDate, @TransactionId, @OutTradeNo, @TransactionTime,
                                @TradeType, @TradeStatus, @TotalAmount, @PayerAmount,
                                @RefundAmount, @PayerRefundAmount, @ProductName, @MerchantName,
                                @MerchantId, @PayerOpenId
                            )";
                // 执行批量插入
                return conn.Execute(sql, billList);
            }
        }

        public int insert_sb(string sql)
        {

            using (var conn = GetConnection())
            {
                int sl = conn.Execute(sql);
                conn.Close();
                return sl;
            }
        }

        public DataSet select_sb(string sql)
        {
            DataSet ds;
            string strCon = _connStr;
            string strCom = sql;
            SqlDataAdapter myCommand = new SqlDataAdapter(strCom, strCon);
            ds = new DataSet();
            myCommand.Fill(ds);
            return ds;
        }

        /// <summary>
        /// 检查订单是否已存在（避免重复入库）
        /// </summary>
        public static bool IsOrderExists(string outTradeNo, string billDate)
        {
            using (var conn = GetConnection())
            {
                var sql = "SELECT COUNT(1) FROM WxTradeBill WHERE OutTradeNo = @OutTradeNo AND BillDate = @BillDate";
                return conn.ExecuteScalar<int>(sql, new { OutTradeNo = outTradeNo, BillDate = billDate }) > 0;
            }
        }
    }

    /// <summary>
    /// 微信交易账单实体模型（与数据库字段对应）
    /// </summary>
    public class WxTradeBillModel
    {
        public string BillDate { get; set; }          // 账单日期
        public string TransactionId { get; set; }    // 微信支付订单号
        public string OutTradeNo { get; set; }       // 商户订单号
        public DateTime? TransactionTime { get; set; }// 交易时间
        public string TradeType { get; set; }        // 交易类型
        public string TradeStatus { get; set; }      // 交易状态
        public decimal? TotalAmount { get; set; }    // 总金额
        public decimal? PayerAmount { get; set; }    // 微信支付金额
        public decimal? RefundAmount { get; set; }   // 退款金额
        public decimal? PayerRefundAmount { get; set; }// 微信退款金额
        public string ProductName { get; set; }      // 商品名称
        public string MerchantName { get; set; }     // 商户名称
        public string MerchantId { get; set; }       // 商户号
        public string PayerOpenId { get; set; }      // 付款方OpenID
    }
}