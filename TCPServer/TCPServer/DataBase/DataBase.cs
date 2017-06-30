using System;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text;

namespace startOnline.DataBase
{
    public class DataBase
    {
        private static DataBase m_operator;
        public static DataBase Operator { get {
                                                if (m_operator == null)
                                                { m_operator = new DataBase(); }
                                                return DataBase.m_operator;
                                               }
                                        }
        private MySql.Data.MySqlClient.MySqlConnection conn_arcloud;
        public DataBase()
        {
            CreateDBConnect(ref conn_arcloud, "Photon", "y33ona", "mtv");
        }

        #region Function
        public void CreateDBConnect(ref MySql.Data.MySqlClient.MySqlConnection conn, string uid, string pwd, string dataBaseName)
        {
            string myConnectionString;
            myConnectionString = "server=localhost;uid=" + uid + ";" + "pwd=" + pwd + ";database=" + dataBaseName + ";";
            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
                conn.Open();
                //MessageBox.Show("Connect Database Success");
                
                //Log.Log.ToTxt("Connect Database Success");
                DisplayMessageBox("Connect Database Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Connect Database fail");
                //Log.Log.ToTxt("Connect Database fail");
                DisplayMessageBox("Connect Database fail");
                switch (ex.Number)
                {
                    case 0:
                        //MessageBox.Show("Cannot connect to server.  Contact administrator");
                        //Log.Log.ToTxt("Cannot connect to server.  Contact administrator");
                        DisplayMessageBox("Cannot connect to server.  Contact administrator");
                        break;
                    case 1045:
                        //MessageBox.Show("Invalid username/password, please try again");
                        //Log.Log.ToTxt("Invalid username/password, please try again");
                        DisplayMessageBox("Invalid username/password, please try again");

                        break;
                }
            }
        }
        /// <summary>
        /// 使用 serialId 取得 QRCode
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        public string GetQRCode(string serialId)
        {
            try
            {
                #region 如果連線斷掉，則重新連線
                if (conn_arcloud.State != ConnectionState.Open)
                    CreateDBConnect(ref conn_arcloud, "Photon", "y33ona", "mtv");
                #endregion
                string result = "";
                string sql = @"select qrcode from mtv.product where serialid = '" + serialId + @"'";
                //MessageBox.Show(sql);
                MySqlCommand cmd1 = new MySqlCommand(sql, conn_arcloud);
                MySqlDataReader rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    result = rdr[0].ToString();
                    //MessageBox.Show(rdr[0].ToString());
                    break;
                }
                rdr.Close();
                if (result.Length == 0)
                    return null;
                return result;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                Log.Log.ToTxt(e.Message);
            }
            try
            {
                //重新連線
                CreateDBConnect(ref conn_arcloud, "Photon", "y33ona", "mtv");
                return this.GetQRCode(serialId);
            }
            catch (Exception e)
            {
                Log.Log.ToTxt(e.Message);
            }
            return null;
        }
        /// <summary>
        /// 使用 QRCode 取得公司資訊
        /// </summary>
        /// <param name="qrcode"></param>
        /// <returns></returns>
        public List<object> GetCompanyInfo(string qrcode)
        {
            try
            {
                #region 如果連線斷掉，則重新連線
                if (conn_arcloud.State != ConnectionState.Open)
                    CreateDBConnect(ref conn_arcloud, "Photon", "y33ona", "mtv");
                #endregion
                string userid = "";
                #region 先用 Qrcode，從 product 取 userid
                string sql = @"select userid from mtv.product where qrcode = '" + qrcode + @"'";
                //MessageBox.Show(sql);
                MySqlCommand cmd1 = new MySqlCommand(sql, conn_arcloud);
                MySqlDataReader rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    userid = rdr[0].ToString();
                    //MessageBox.Show("欄位 "+rdr[0].ToString());
                    break;
                }
                rdr.Close();
                #endregion
                #region 用 userid 取 公司資訊
                if (userid.Length == 0)
                    return null;
                //如果有取到 userId
                sql = @"select * from mtv.customer where userid = '" + userid + @"'";
                //MessageBox.Show(sql);
                cmd1 = new MySqlCommand(sql, conn_arcloud);
                rdr = cmd1.ExecuteReader();
                List<object> result = new List<object>();
                while (rdr.Read())
                {
                    userid = rdr[0].ToString();
                    for (byte i = 0; i < rdr.FieldCount; i++)
                    {
                        result.Add(rdr[i]);
                    }
                    break;
                }
                rdr.Close();
                return result;
                #endregion
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //Log.Log.ToTxt(e.Message);
            }
            return null;
        }

        public bool Including(string sql) //直接給sql
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn_arcloud);
            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                int r = Convert.ToInt32(result);
                if (r == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 註冊新機器
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="currect_qrcode"></param>
        /// <param name="companyName"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="product_tv"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public bool Registered(out byte errorCode,out string currect_qrcode, string companyName, string phone, string address, bool product_tv,string serialNumber)
        {
            try
            {
                errorCode = 0;
                currect_qrcode = "";

                #region 檢查公司名子是否存在
                string sql3 = @"select count(customer.name) from customer where customer.name = '" + companyName + "'";
                MySqlCommand cmd3 = new MySqlCommand(sql3, conn_arcloud);
                MySqlDataReader rdr3 = cmd3.ExecuteReader();
                string count = "";
                while (rdr3.Read())
                {
                    count = rdr3[0].ToString();
                    //MessageBox.Show(rdr[0].ToString());
                    break;
                }
                rdr3.Close();
                //DisplayMessageBox(" rdr 3 = " + count);
                if (count != "0")
                    return false;
                #endregion

                #region 取得可用的 userId
                string sql = @"select count(customer.userid) from customer";
                string result = ""; //userid 的數量
                                    //MessageBox.Show(sql);
                MySqlCommand cmd1 = new MySqlCommand(sql, conn_arcloud);
                MySqlDataReader rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    result = rdr[0].ToString();
                    //MessageBox.Show(rdr[0].ToString());
                    break;
                }
                rdr.Close();

                int new_userid = int.Parse(result);
                new_userid += 1;
                #endregion
                //DisplayMessageBox("result = " + new_userid);
                #region 檢查 product_tv
                byte hasProduct_tv = 0;
                if (product_tv)
                    hasProduct_tv = 1;
                #endregion

                #region 寫入 customer 資料表
                string sql2 = @"insert into customer(customer.userid,customer.name,customer.phone,customer.address,customer.product_tv) 
                        values(" + new_userid + ",'" + companyName + "','" + phone + "','" + address + "'," + product_tv + ")";
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn_arcloud);
                cmd2.ExecuteNonQuery();
                #endregion

                #region 寫入 product 資料表
                //執行到這邊代表寫入 customer 資料表成功
                //利用 userid 和 sn 去 product資料表註冊
                string qrcode = generateStringID();                
                string sql4 = "insert into product(product.userid,product.qrcode,product.serialid)values(" + new_userid + ", '" + qrcode + "', '" + serialNumber + "'); ";
                MySqlCommand cmd4 = new MySqlCommand(sql4, conn_arcloud);
                cmd4.ExecuteNonQuery();
                currect_qrcode = qrcode;
                return true;
                #endregion
            }
            catch (Exception e)
            {
                Log.Log.ToTxt(e.Message);
                errorCode = 1;
                currect_qrcode = "";
                return false;
            }
        }
        /// <summary>
        /// 使用公司名，取得 customer表所有資訊 和 product表中的 QRCode
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public List<object> Query(string companyName)
        {
            try
            {
                List<object> result = new List<object>();
                #region 如果連線斷掉，則重新連線
                if (conn_arcloud.State != ConnectionState.Open)
                    CreateDBConnect(ref conn_arcloud, "Photon", "y33ona", "mtv");
                #endregion
                string userid="";
                #region 取得 customer 表裡的資訊
                string sql = @"SELECT * FROM customer WHERE customer.name LIKE '"+ companyName + @"' LIMIT 1";

                Log.Log.ToTxt("sql 1 :" + sql);
                //MessageBox.Show(sql);
                MySqlCommand cmd1 = new MySqlCommand(sql, conn_arcloud);
                MySqlDataReader rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    Log.Log.ToTxt("Read 1");
                    userid = rdr[0].ToString();
                    for (byte i = 0; i < rdr.FieldCount; i++)
                    {
                        result.Add(rdr[i]);
                    }
                    //result.Add(rdr[1]);
                    //result.Add(rdr[2]);
                    //result.Add(rdr[3]);
                    //result.Add(rdr[4]);
                    //result.Add(rdr[5]);
                    //result.Add(rdr[6]);
                    //result.Add(rdr[7]);
                    //MessageBox.Show("欄位 "+rdr[0].ToString());
                    break;
                }
                rdr.Close();
                Log.Log.ToTxt("1 :" + result.Count);
                #endregion
                #region 取得 product 表裡的資訊
                sql = @"select * from mtv.product where userid = '" + userid + @"'";
                //MessageBox.Show(sql);
                cmd1 = new MySqlCommand(sql, conn_arcloud);
                rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    result.Add(rdr[1]); //QRCode
                    result.Add(rdr[2]); //SN

                    //Log.Log.ToTxt("usewrid = "+userid+","+rdr[0] + "," + rdr[1] + "," + rdr[2]);
                    break;
                }
                rdr.Close();
                #endregion
                Log.Log.ToTxt("2 :" + result.Count);
                result = ConvertDateTimeToString(result);
                Log.Log.ToTxt("3 :" + result.Count);
                return result;
            }
            catch (Exception e)
            {
                Log.Log.ToTxt(e.Message);
                return null;
            }

        }
        /// <summary>
        /// 將 Guid 縮短
        /// </summary>
        /// <returns></returns>
        private string generateStringID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
        /// <summary>
        /// 將傳入 List中的 所有 DateTime 轉為 tick 並回傳
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<object> ConvertDateTimeToString(List<object> list)
        {
            List<object> result = new List<object>();
            for (byte i = 0; i < list.Count; i++)
            {
                if (list[i] is System.DateTime)
                {
                    //string format = @"MM\/dd\/yyyy HH:mm";
                    //DateTime dateTime = (DateTime)list[i];
                    //list[i] = dateTime.ToString(format);
                    list[i] = ((System.DateTime)list[i]).Ticks;
                }
                result.Add(list[i]);
            }
            return result;
        }
        private void DisplayMessageBox(string context)
        {
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show(context);
            })).Start();
        }
        #endregion
    }
}
