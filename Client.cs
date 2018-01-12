using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using System.Data.Sql;


namespace DST_CLIENT
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            timer1.Start();
            string activeDir = DtsFilePath;
            System.IO.Directory.CreateDirectory(DtsFilePath);
        }
        public static string Company = ConfigurationManager.AppSettings["Company"].ToString();
        public static string ShopCode = ConfigurationManager.AppSettings["SHOPCODE"].ToString();
        public static string Describe = ConfigurationManager.AppSettings["Describe"].ToString();
        public static string Version = ConfigurationManager.AppSettings["Version"].ToString();
        public static string ExeHour = ConfigurationManager.AppSettings["ExecHour"].ToString();
        public static string ExeFirst = ConfigurationManager.AppSettings["ExecHour"].ToString();
        public static string DtsFilePathC = ConfigurationManager.AppSettings["DtsFilePathC"].ToString();
        public static string DtsFilePathS = ConfigurationManager.AppSettings["DtsFilePathS"].ToString();
        public static string DtsFilePathP = ConfigurationManager.AppSettings["DtsFilePathP"].ToString();
        public static string DtsFilePath = ConfigurationManager.AppSettings["DtsFilePath"].ToString();
        public static string txt = "";
        public static string lastExechout = "";
        public static string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

        ThreadStart ts;
        Thread thread;
        bool b_IsRun;

        public void Autoupload()
        {
            timer2.Start();
            fetchData();
            CompressZip();
            DeletePath();
            timer2.Stop();
        }

        public void Autodowm()
        {
            //FormWindowState bffag = this.WindowState;
            //this.WindowState = FormWindowState.Minimized;
            download();
            btn_upload.Enabled = true;
            btn_Dowm.Enabled = true;
            //if (bffag == FormWindowState.Normal)
            //{
            //    this.Visible = true;
            //    this.WindowState = FormWindowState.Normal;
            //}
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {

            btn_upload.Enabled = false;
            btn_Dowm.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            b_IsRun = true;
            ts = delegate { Autoupload(); };
            thread = new Thread(ts);
            thread.Start();
            btn_upload.Enabled = true;
            btn_Dowm.Enabled = true;
        }

        private void btn_Dowm_Click(object sender, EventArgs e)
        {
            btn_upload.Enabled = false;
            btn_Dowm.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            b_IsRun = true;
            ts = delegate { Autodowm(); };
            thread = new Thread(ts);
            thread.Start();
        }

        private void download()
        {
            timer2.Start();
            FildPublicDowm(true);
            FildCompanyDowm(true);
            FildShopDowm(true);
            DeletePathD();
            timer2.Stop();
        }

        public void DeletePath()
        {
            string pathS = DtsFilePathS + ShopCode;
            try
            {
                Directory.Delete(pathS, true);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void DeletePathD()
        {

            string pathP = DtsFilePathP + "/DOWNLOAD/";
            string pathC = DtsFilePathC + "/DOWNLOAD/";
            string pathS = DtsFilePathS + ShopCode + "/DOWNLOAD/";
            try
            {
                Directory.Delete(pathC, true);
                Directory.Delete(pathP, true);
                Directory.Delete(pathS, true);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            //timer1.Enabled = false;
            Application.Exit();

        }

        private void MoveCurorLast()
        {
            //让文本框获取焦点 
            this.rTB.Focus();
            //设置光标的位置到文本尾 
            this.rTB.Select(this.rTB.TextLength, 0);
            //滚动到控件光标处 
            this.rTB.ScrollToCaret();
        }

        #region 拿到新數據
        public void fetchData()
        {
            string strSql = "SELECT row_number() over (order by CODE,SHOP_UPD_ON) as ROWNUM ," +
                          " * FROM DOC_MEMBER " +
                           " WHERE MEMBER_ID=-1 " +
                            " ORDER BY CODE,SHOP_UPD_ON ";
            DataTable DataMbr = ExportTotxt.QueryData(strSql);
            WriteTxt(DataMbr, "DOC_MEMBER");


            strSql = "SELECT row_number() over (order by MEMBER_CODE,SHOP_UPD_ON) as ROWNUM ," +
                         " * FROM DOC_MEMBER_EXPAND " +
                          " WHERE MEMBER_ID=-1 " +
                           " ORDER BY MEMBER_CODE,SHOP_UPD_ON ";
            DataTable DataMbrExp = ExportTotxt.QueryData(strSql);
            WriteTxt(DataMbrExp, "DOC_MEMBER_EXPAND");

            strSql = "SELECT row_number() over (order by BIL_CODE,SHOP_UPD_ON) as ROWNUM ," +
                         " * FROM BIL_RETAIL " +
                          " WHERE RETAIL_ID=-1 " +
                           " ORDER BY BIL_CODE,SHOP_UPD_ON ";
            DataTable Data = ExportTotxt.QueryData(strSql);
            WriteTxt(Data, "BIL_RETAIL");


            strSql = " SELECT row_number() over (order by BIL_RETAIL_CODE,LINE_NUM,SHOP_UPD_ON) as ROWNUM , " +
                    " * FROM BIL_RETAIL_DTL " +
                    " WHERE RETAIL_DTL_ID = -1 " +
                    " ORDER BY BIL_RETAIL_CODE,LINE_NUM,SHOP_UPD_ON ";
            DataTable DataDtl = ExportTotxt.QueryData(strSql);
            WriteTxt(DataDtl, "BIL_RETAIL_DTL");


            strSql = " SELECT row_number() over (order by BIL_RETAIL_CODE,SHOP_UPD_ON) as ROWNUM , " +
                    " * FROM BIL_RETAIL_PAYMENT " +
                    " WHERE RETAIL_PAYMENT_ID = -1 " +
                    " ORDER BY BIL_RETAIL_CODE ";
            DataTable DataPayment = ExportTotxt.QueryData(strSql);
            WriteTxt(DataPayment, "BIL_RETAIL_PAYMENT");

            strSql = " SELECT row_number() over (order by BIL_RETAIL_CODE,SHOP_UPD_ON) as ROWNUM , " +
                    " * FROM BIL_RETAIL_CHARGES " +
                    " WHERE RETAIL_CHARGES_ID = -1 " +
                    " ORDER BY BIL_RETAIL_CODE,SHOP_UPD_ON ";
            DataTable DataChares = ExportTotxt.QueryData(strSql);
            WriteTxt(DataChares, "BIL_RETAIL_CHARGES");

            strSql = " SELECT row_number() over (order by BIL_RETAIL_CODE,SHOP_UPD_ON) as ROWNUM , " +
                    " HEAD_REMARK,FOOT_REMARK,INTERNAL_REMARK,BIL_RETAIL_CODE  FROM BIL_RETAIL_EXPAND " +
                    " WHERE RETAIL_ID = -1 " +
                    " ORDER BY BIL_RETAIL_CODE,SHOP_UPD_ON ";
            DataTable DataRetailExpand = ExportTotxt.QueryData(strSql);
            WriteTxt(DataRetailExpand, "BIL_RETAIL_EXPAND");

            strSql = " SELECT BIL_RETAIL_CODE,USER_SIGNATURE" +
                    "  FROM BIL_RETAIL_SIGNATURE " +
                    " WHERE RETAIL_ID = -1 " +
                    " ORDER BY BIL_RETAIL_CODE ";
            DataTable DataSign = ExportTotxt.QueryData(strSql);
            SIGNATURE(DataSign, "BIL_RETAIL_SIGNATURE");

            strSql = " SELECT row_number() over (order by BIL_TREATED_CODE) as ROWNUM , " +
                    " * FROM BIL_TREATED_COLLECT_WAGE " +
                    " WHERE TREATED_COLLECT_WAGE_ID = -1 " +
                    " ORDER BY BIL_TREATED_CODE ";
            DataTable DataTreateCollWage = ExportTotxt.QueryData(strSql);
            WriteTxt(DataTreateCollWage, "BIL_TREATED_COLLECT_WAGE");

            strSql = " SELECT row_number() over (order by BIL_TREATED_CODE) as ROWNUM , " +
                    " * FROM BIL_TREATED_SPLIT " +
                    " WHERE TREATED_SPLIT_ID = -1 " +
                    " ORDER BY BIL_TREATED_CODE ";
            DataTable DataTreateWageSplit = ExportTotxt.QueryData(strSql);
            WriteTxt(DataTreateWageSplit, "BIL_TREATED_SPLIT");

            strSql = " SELECT row_number() over (order by BIL_TREATED_CODE) as ROWNUM , " +
                    " * FROM BIL_TREATED_WAGE " +
                    " WHERE TREATED_ID = -1 " +
                    " ORDER BY BIL_TREATED_CODE ";
            DataTable DataTreateWage = ExportTotxt.QueryData(strSql);
            WriteTxt(DataTreateWage, "BIL_TREATED_WAGE");

            strSql = "SELECT row_number() over (order by BIL_CODE,SHOP_UPD_ON) as ROWNUM ," +
                          " * FROM BIL_TREATED " +
                           " WHERE TREATED_ID=-1 " +
                            " ORDER BY BIL_CODE,SHOP_UPD_ON ";
            DataTable DataTreated = ExportTotxt.QueryData(strSql);
            WriteTxt(DataTreated, "BIL_TREATED");


            strSql = " SELECT row_number() over (order by BIL_TREATED_CODE,LINE_NUM,SHOP_UPD_ON) as ROWNUM , " +
                    " * FROM BIL_TREATED_COLLECT " +
                    " WHERE TREATED_COLLECT_ID = -1 " +
                    " ORDER BY BIL_TREATED_CODE, LINE_NUM, SHOP_UPD_ON ";
            DataTable DataCollect = ExportTotxt.QueryData(strSql);
            WriteTxt(DataCollect, "BIL_TREATED_COLLECT");

            strSql = " SELECT  BIL_TREATED_CODE,USER_SIGNATURE " +
                    "  FROM BIL_TREATED_SIGNATURE " +
                    " WHERE TREATED_ID = -1 " +
                    " ORDER BY BIL_TREATED_CODE ";
            DataTable DataTreatedSign = ExportTotxt.QueryData(strSql);
            SIGNATURE(DataTreatedSign, "BIL_TREATED_SIGNATURE");

            strSql = "update DOC_MEMBER_EXPAND set MEMBER_ID=0 where MEMBER_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update DOC_MEMBER set MEMBER_ID=0 where MEMBER_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_RETAIL_SIGNATURE set RETAIL_ID=0 where RETAIL_ID=-1";// ADD BY MARK 20170803
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_RETAIL_CHARGES set RETAIL_CHARGES_ID=0 where RETAIL_CHARGES_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_RETAIL_PAYMENT set RETAIL_PAYMENT_ID=0 where RETAIL_PAYMENT_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_RETAIL_DTL set RETAIL_DTL_ID=0 where RETAIL_DTL_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_RETAIL set RETAIL_ID=0 where RETAIL_ID=-1";
            ExportTotxt.ExceSql_int(strSql);

            strSql = "update BIL_TREATED_SIGNATURE set TREATED_ID=0 where TREATED_ID=-1";// ADD BY MARK 20170803
            ExportTotxt.ExceSql_int(strSql);

            strSql = "update BIL_TREATED_COLLECT_WAGE set TREATED_COLLECT_WAGE_ID=0 where TREATED_COLLECT_WAGE_ID=-1";
            ExportTotxt.ExceSql_int(strSql);

            strSql = "update BIL_TREATED_SPLIT set TREATED_SPLIT_ID=0 where TREATED_SPLIT_ID=-1";
            ExportTotxt.ExceSql_int(strSql);

            strSql = "update BIL_TREATED_WAGE set TREATED_ID=0 where TREATED_ID=-1";
            ExportTotxt.ExceSql_int(strSql);


            strSql = "update BIL_TREATED_COLLECT set TREATED_COLLECT_ID=0 where TREATED_COLLECT_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update BIL_TREATED set TREATED_ID=0 where TREATED_ID=-1";
            ExportTotxt.ExceSql_int(strSql);
            strSql = "update ACC_MEMBER_TREATMENT set MEMBER_TREATMENT_ID=0 where MEMBER_TREATMENT_ID IN(-2,-1)";
            ExportTotxt.ExceSql_int(strSql);

        }
        #endregion

        public void WriteTxt(DataTable tb, String Tablename)
        {
            string activeDir = DtsFilePathS + ShopCode + "";
            string newPath = System.IO.Path.Combine(activeDir, "UPLOAD");
            System.IO.Directory.CreateDirectory(newPath);
            if (tb == null || tb.Rows.Count < 1)
            {
                return;
            }
            if (tb.Rows.Count >= 1)
            {
                string line = "";
                StreamWriter sr = File.CreateText(DtsFilePathS + ShopCode + "/UPLOAD/" + Tablename + ".txt");
                int Colid = 0;
                foreach (DataColumn c in tb.Columns)
                {
                    int count = tb.Columns.Count;
                    if (Colid < count - 1)
                    {
                        line += c.ColumnName + "^";
                        Colid++;
                    }
                    else
                    {
                        line += c.ColumnName + "\r\n";
                    }
                }
                for (int i = 0; i < tb.Rows.Count; i++)
                {
                    for (int j = 0; j < tb.Columns.Count; j++)
                    {
                        if (j < tb.Columns.Count - 1)
                        {
                            line += tb.Rows[i][j].ToString().Trim().Replace("^", "") + "^";
                        }
                        if (j == tb.Columns.Count - 1)
                        {
                            line += tb.Rows[i][j].ToString().Trim().Replace("^", "") + "\r\n";
                        }
                    }
                }
                sr.WriteLine(line);
                sr.Close();
                rTB.Text += string.Format("{0}上傳成功.\n", Tablename);

            }
        }

        public void SIGNATURE(DataTable tb, String tabname)
        {
            string activeDir = DtsFilePathS + ShopCode + "";
            string newPath = System.IO.Path.Combine(activeDir, "UPLOAD");
            System.IO.Directory.CreateDirectory(newPath);
            if (tb == null || tb.Rows.Count < 1)
            {
                return;
            }
            if (tb.Rows.Count >= 1)
            {
                string line = "";
                string Tablename = "";
                for (int i = 0; i < tb.Rows.Count; i++)
                {
                    if (tabname == "BIL_RETAIL_SIGNATURE")
                        Tablename = tb.Rows[i]["BIL_RETAIL_CODE"].ToString().Trim();
                    else
                        Tablename = tb.Rows[i]["BIL_TREATED_CODE"].ToString().Trim();
                    line = tb.Rows[i]["USER_SIGNATURE"].ToString();
                    StreamWriter sr = File.CreateText(DtsFilePathS + ShopCode + "/UPLOAD/" + Tablename + ".txt");
                    sr.WriteLine(line);
                    sr.Close();
                    rTB.Text += string.Format("{0} {1}上傳成功.\n", tabname, Tablename);
                }

            }

        }

        public void FildLoad(bool load, string temp)
        {
            if (load)
            {
                string FtpServerIP = ConfigurationManager.AppSettings["FTPIP"].ToString();
                string FtpUserID = ConfigurationManager.AppSettings["FTPID"].ToString();
                string FtpPassword = ConfigurationManager.AppSettings["FTPPW"].ToString();

                string errorinfo = "已經存在";
                FtpUpDown ftpUpDown = new FtpUpDown(FtpServerIP, FtpUserID, FtpPassword);

                //if (temp == "IsNotsignture")
                //{
                string zipname = "";
                DirectoryInfo theFolder = new DirectoryInfo(DtsFilePathS + ShopCode + "/UPLOAD");
                FileInfo[] fileInfo = theFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)  //遍曆文件
                {
                    if (NextFile.Extension == ".zip")
                    {
                        zipname = NextFile.Name;
                        string filename = DtsFilePathS + ShopCode + "/UPLOAD/" + zipname;
                        if (ftpUpDown.Upload(filename, ShopCode) == false)
                        {
                            ftpUpDown.Upload(filename, ShopCode);
                        }
                    }
                }
                MoveCurorLast();
                #region 記錄存入recieve.txt
                string activeDir = DtsFilePathS + ShopCode + "";
                string newPath = System.IO.Path.Combine(activeDir, "txt");
                System.IO.Directory.CreateDirectory(newPath);
                ftpUpDown.Download_recieve(DtsFilePathS + ShopCode + "/txt", "recieve.txt", ShopCode, out errorinfo);//先下載記錄，再加入去

                string path = DtsFilePathS + ShopCode + " /txt/recieve.txt";
                StreamWriter sw = new StreamWriter(path, true);
                string write = DateTime.Now.ToString() + "," + "\t" + zipname + "," + "\t" + "N";
                sw.WriteLine(write);
                sw.Close();

                if (ftpUpDown.Upload(DtsFilePathS + ShopCode + " /txt/recieve.txt", ShopCode) == true)
                {//上傳recieve.txt
                    string deletepath = DtsFilePathS + ShopCode + "/UPLOAD/";
                    Directory.Delete(deletepath, true);  //上傳完刪目錄
                    rTB.Text += string.Format("{0}上傳成功,詳細記錄在recieve.txt\r\n", zipname);

                }
                else
                {
                    ftpUpDown.Upload(DtsFilePathS + ShopCode + " /txt/recieve.txt", ShopCode);
                    string deletepath = DtsFilePathS + ShopCode + "/UPLOAD/";
                    Directory.Delete(deletepath, true);  //上傳完刪目錄
                    rTB.Text += string.Format("{0}上傳成功,詳細記錄在recieve.txt\r\n", zipname);

                }
                #endregion

            }
        }

        public void FildShopDowm(bool dowm)
        {
            if (dowm)
            {
                string activeDir = DtsFilePathS + ShopCode + "";
                string newPath = System.IO.Path.Combine(activeDir, "DOWNLOAD");
                System.IO.Directory.CreateDirectory(newPath);

                string FtpServerIP = ConfigurationManager.AppSettings["FTPIP"].ToString();
                string FtpUserID = ConfigurationManager.AppSettings["FTPID"].ToString();
                string FtpPassword = ConfigurationManager.AppSettings["FTPPW"].ToString();

                string errorinfo = "已經存在";
                FtpUpDown ftpUpDown = new FtpUpDown(FtpServerIP, FtpUserID, FtpPassword);

                MoveCurorLast();

                string path = "SHOP/" + ShopCode + "/DOWNLOAD/";
                string[] str = ftpUpDown.GetFileList(path);//獲取zip列表
                if (str == null || str.Length < 1)
                {
                    rTB.Text += string.Format("FTP上沒有新數據下載到本地.\n");

                    return;
                }
                else if (str.Length > 0)
                {
                    foreach (string list in str)
                    {
                        if (list == "SendList.txt")
                        {
                            ftpUpDown.Download(DtsFilePathS + ShopCode + "/DOWNLOAD", list, ShopCode, out errorinfo);
                            string rootpath = DtsFilePathS + ShopCode + "/DOWNLOAD/" + "SendList.txt";
                            string[] data = File.ReadAllLines(rootpath);//txt文本路徑
                            for (int i = 0; i < data.Length; i++)
                            {
                                string SendList = data[i].Replace("\t", "");
                                string[] SendListArray = SendList.Split(',');
                                for (int j = 0; j < SendListArray.Length; j++)
                                {
                                    if (SendListArray[j].TrimStart() == "N")
                                    {
                                        bool dowmload = false;
                                        dowmload = ftpUpDown.Download(DtsFilePathS + ShopCode + "/DOWNLOAD", SendListArray[j - 1].TrimStart(), ShopCode, out errorinfo);
                                        if (dowmload)
                                        {
                                            ZipClass ZipClass = new ZipClass();
                                            string activeDirunzip = DtsFilePathS + ShopCode + "/DOWNLOAD/" + SendListArray[j - 1].ToString().Trim().Substring(0, SendListArray[j - 1].ToString().Trim().IndexOf(".")) + "/";
                                            ZipClass.UnZip(DtsFilePathS + ShopCode + "/DOWNLOAD/" + SendListArray[j - 1].ToString().Trim(), activeDirunzip);
                                            rTB.Text += string.Format("{0}下载在本地成功\n", SendListArray[j - 1].ToString().Trim());
                                            UnCompressZip(activeDirunzip);
                                            SendListArray[j] = "Y";
                                        }
                                    }
                                }
                                SendList = string.Join(",\t", SendListArray);
                                data[i] = SendList;
                            }
                            File.WriteAllLines(rootpath, data);

                            FileUpDownload.FtpServerIP = ConfigurationManager.AppSettings["FTPIP"].ToString();
                            FileUpDownload.FtpUserID = ConfigurationManager.AppSettings["FTPID"].ToString();
                            FileUpDownload.FtpPassword = ConfigurationManager.AppSettings["FTPPW"].ToString();
                            FileUpDownload.FtpUploadFileS(DtsFilePathS + "/DOWNLOAD" + ShopCode + "/SendList.txt");
                        }
                    }
                }
            }
        }

        public void FildCompanyDowm(bool dowm)
        {
            if (dowm)
            {
                string activeDir = DtsFilePathC + "";
                string newPath = System.IO.Path.Combine(activeDir, "DOWNLOAD");
                System.IO.Directory.CreateDirectory(newPath);

                string FtpServerIP = ConfigurationManager.AppSettings["FTPIP"].ToString();
                string FtpUserID = ConfigurationManager.AppSettings["FTPID"].ToString();
                string FtpPassword = ConfigurationManager.AppSettings["FTPPW"].ToString();

                string errorinfo = "已經存在";
                FtpUpDown ftpUpDown = new FtpUpDown(FtpServerIP, FtpUserID, FtpPassword);

                string path = "COMPANY/DOWNLOAD/";
                string[] str = ftpUpDown.GetFileList(path);

                if (str == null || str.Length < 1)
                {
                    return;
                }
                if (str.Length > 0)
                {
                    //foreach (string list in str)
                    //{
                    //    if (list == "SendList.txt")
                    //    {
                    if (File.Exists(DtsFilePathC + "SendList.txt"))
                    {
                    }
                    else
                    {
                        StreamWriter sw = File.CreateText(DtsFilePathC + "SendList.txt");
                        sw.Close();
                    }

                    List<string> localzip = new List<string>();
                    string ppath = DtsFilePathC + "SendList.txt";
                    string[] localData = File.ReadAllLines(ppath);
                    for (int i = 0; i < localData.Length; i++)
                    {
                        string localList = localData[i].Replace("\t", "");
                        string[] localListArray = localList.Split(',');
                        for (int j = 0; j < localListArray.Length; j++)
                        {
                            if (localListArray[j].TrimStart().Contains(".zip"))
                            {
                                localzip.Add(localListArray[j]);
                            }
                        }
                    }

                    string[] sevlist = ftpUpDown.GetFileList("COMPANY\\DOWNLOAD");
                    int m = sevlist.Length;
                    ArrayList Al = new ArrayList(sevlist);
                    Al.RemoveAt(m - 1);
                    sevlist = (string[])Al.ToArray(typeof(string));
                    for (int A = 0; A < sevlist.Length; A++)
                    {
                        bool exists = (localzip).Contains(sevlist[A].ToString());
                        if (exists)
                        {
                        }
                        else
                        {
                            bool dowmload = false;
                            dowmload = ftpUpDown.DownloadC(DtsFilePathC + "/DOWNLOAD", sevlist[A].TrimStart(), out errorinfo);
                            if (dowmload)
                            {
                                ZipClass ZipClass = new ZipClass();
                                string activeDirunzip = DtsFilePathC + "/DOWNLOAD/" + sevlist[A].ToString().Trim().Substring(0, sevlist[A].ToString().Trim().IndexOf(".")) + "/";
                                ZipClass.UnZip(DtsFilePathC + "/DOWNLOAD/" + sevlist[A].ToString().Trim(), activeDirunzip);

                                rTB.Text += string.Format("{0}下载在本地成功\n", sevlist[A].ToString().Trim());
                                UnCompressZip(activeDirunzip);
                            }
                            StreamWriter sw = new StreamWriter(ppath, true);
                            string write = DateTime.Now.ToString() + "," + "\t" + sevlist[A] + "," + "\t" + "N" + "" + "\r\n";
                            sw.WriteLine(write);
                            sw.Close();
                        }
                    }
                }
            }
        }

        public void FildPublicDowm(bool dowm)
        {
            if (dowm)
            {
                string activeDir = DtsFilePathP + "";
                string newPath = System.IO.Path.Combine(activeDir, "DOWNLOAD");
                System.IO.Directory.CreateDirectory(newPath);

                string FtpServerIP = ConfigurationManager.AppSettings["FTPIP"].ToString();
                string FtpUserID = ConfigurationManager.AppSettings["FTPID"].ToString();
                string FtpPassword = ConfigurationManager.AppSettings["FTPPW"].ToString();

                MoveCurorLast();
                string errorinfo = "已經存在";
                FtpUpDown ftpUpDown = new FtpUpDown(FtpServerIP, FtpUserID, FtpPassword);

                string path = "PUBLIC/DOWNLOAD/";
                string[] str = ftpUpDown.GetFileList(path);//獲取zip列表

                if (str == null || str.Length < 1)
                {
                    return;
                }
                if (str.Length > 0)
                {
                    if (File.Exists(DtsFilePathP + "SendList.txt"))
                    {
                    }
                    else
                    {
                        StreamWriter sw = File.CreateText(DtsFilePathP + "SendList.txt");
                        sw.Close();
                    }

                    List<string> localzip = new List<string>();
                    string ppath = DtsFilePathP + "SendList.txt";
                    string[] localData = File.ReadAllLines(ppath);
                    for (int i = 0; i < localData.Length; i++)
                    {
                        string localList = localData[i].Replace("\t", "");
                        string[] localListArray = localList.Split(',');
                        for (int j = 0; j < localListArray.Length; j++)
                        {
                            if (localListArray[j].TrimStart().Contains(".zip"))
                            {
                                localzip.Add(localListArray[j]);
                            }
                        }
                    }

                    string[] sevlist = ftpUpDown.GetFileList("PUBLIC\\DOWNLOAD");
                    int m = sevlist.Length;
                    ArrayList Al = new ArrayList(sevlist);
                    Al.RemoveAt(m - 1);
                    sevlist = (string[])Al.ToArray(typeof(string));
                    for (int A = 0; A < sevlist.Length; A++)
                    {
                        bool exists = (localzip).Contains(sevlist[A].ToString());
                        if (exists)
                        {
                        }
                        else
                        {
                            bool dowmload = false;
                            dowmload = ftpUpDown.DownloadP(DtsFilePathP + "/DOWNLOAD", sevlist[A].TrimStart(), out errorinfo);
                            if (dowmload)
                            {
                                ZipClass ZipClass = new ZipClass();
                                string activeDirunzip = DtsFilePathP + "/DOWNLOAD/" + sevlist[A].ToString().Trim().Substring(0, sevlist[A].ToString().Trim().IndexOf(".")) + "/";
                                ZipClass.UnZip(DtsFilePathP + "/DOWNLOAD/" + sevlist[A].ToString().Trim(), activeDirunzip);
                                rTB.Text += string.Format("{0}下载在本地成功\n", sevlist[A].ToString().Trim());
                                UnCompressZip(activeDirunzip);
                            }
                            StreamWriter sw = new StreamWriter(ppath, true);
                            string write = DateTime.Now.ToString() + "," + "\t" + sevlist[A] + "," + "\t" + "N" + "" + "\r\n";
                            sw.WriteLine(write);
                            sw.Close();
                        }
                    }
                }
            }
        }

        public void CompressZip()
        {
            bool zip = false;
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string activeDir = DtsFilePathS + ShopCode + "";
            string newPath = System.IO.Path.Combine(activeDir, "Upload");
            System.IO.Directory.CreateDirectory(newPath);
            DirectoryInfo theFolder = new DirectoryInfo(newPath);
            FileInfo[] fileInfo = theFolder.GetFiles();
            foreach (FileInfo NextFile in fileInfo)  //遍曆zip文件
            {
                if (NextFile.Extension == ".txt")
                {
                    zip = true;
                }
            }
            if (zip)
            {
                ZipClass ZipClass = new ZipClass();
                ZipClass.ZipFileFromDirectory(DtsFilePathS + ShopCode + "/Upload", DtsFilePathS + ShopCode + "/Upload/" + now + ShopCode + ".zip", 1);
                FildLoad(true, "IsNotsignture");
            }
            else
            {
                rTB.Text = "沒有新的數據需要上傳.\n";

            }
        }

        public void UnCompressZip(string activeDir)
        {

            DirectoryInfo theFolder2 = new DirectoryInfo(activeDir);
            FileInfo[] fileInfo2 = theFolder2.GetFiles();
            foreach (FileInfo NextFile in fileInfo2)  //遍曆txt文件
            {

                if (NextFile.Name == "DOC_MEMBER.txt")
                {
                    bool db = false;
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    string sqldb = "select top 1 * from " + Tablesname;
                    DataTable qurrydb = ExportTotxt.QueryData(sqldb);
                    if (qurrydb == null || qurrydb.Rows.Count < 1)
                    {
                        db = true;
                    }

                    if (db)
                    {
                        GetDOC_MEMBER(activeDir, Tablesname, StrName);
                    }
                    else
                    {
                        GetTableMember(activeDir, Tablesname, StrName);
                    }
                }
                else if (NextFile.Name == "DOC_MEMBER_EXPAND.txt")
                {
                    bool db = false;
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    string sqldb = "select top 1 * from " + Tablesname;
                    DataTable qurrydb = ExportTotxt.QueryData(sqldb);
                    if (qurrydb == null || qurrydb.Rows.Count < 1)
                    {
                        db = true;
                    }

                    if (db)
                    {
                        GetDOC_Memberexpand(activeDir, Tablesname, StrName);
                    }
                    else
                    {
                        GetTableMember(activeDir, Tablesname, StrName);
                    }
                }
                else if (NextFile.Name == "ACC_MEMBER_TREATMENT.txt")
                {
                    bool db = false;
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    string sqldb = "select top 1 * from " + Tablesname;
                    DataTable qurrydb = ExportTotxt.QueryData(sqldb);
                    if (qurrydb == null || qurrydb.Rows.Count < 1)
                    {
                        db = true;
                    }

                    if (db)
                    {
                        GetAccTreatment(activeDir, Tablesname, StrName);
                    }
                    else
                    {
                        GetTableMember(activeDir, Tablesname, StrName);
                    }
                }
                else if (NextFile.Name == "LOG_STOCK_GOODS_LOT_BATCH.txt" || NextFile.Name == "ACC_STOCK_GOODS_LOT.txt" || NextFile.Name == "ACC_STOCK_GOODS_LOT_BATCH.txt" || NextFile.Name == "ACC_STOCK_GOODS.txt")
                {
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    GetTableACC(activeDir, Tablesname, StrName);
                }
                else if (
                     NextFile.Name == "DOC_GROUP_USER.txt"
                    || NextFile.Name == "DOC_ORG_ADDRESS.txt"
                    || NextFile.Name == "DOC_ORG_PAYMENT.txt"
                    || NextFile.Name == "DOC_ORG_STAFF.txt"
                    || NextFile.Name == "DOC_STAFF_POSITION.txt"
                    || NextFile.Name == "DOC_WAGE_BOOK_AREA.txt"
                    || NextFile.Name == "BIL_RETAIL_SHOP_SPLIT_WAGE.txt"
                    )
                {
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    GetTableArrKey(activeDir, Tablesname, StrName);
                }

                else if (
                    // NextFile.Name == "DOC_GOODS.txt"
                    // NextFile.Name == "DOC_GOODS_EXPAND.txt"
                    //|| NextFile.Name == "DOC_GOODS_PRICE.txt"
                    //|| 
                    NextFile.Name == "PUB_GOODS_CATE.txt"
                    || NextFile.Name == "PUB_ADDRESS.txt"
                    || NextFile.Name == "DOC_STAFF.txt"
                    || NextFile.Name == "DOC_WAGE_BOOK_DETAIL.txt"
                    || NextFile.Name == "DOC_STAFF_POSITION .txt"
                    || NextFile.Name == "DOC_COMPANY.txt"
                    || NextFile.Name == "DOC_LOCATION.txt"
                    || NextFile.Name == "DOC_WAGE_BOOK_AREA.txt"
                    || NextFile.Name == "DOC_GROUP_USER.txt"
                    || NextFile.Name == "DOC_WAGE_BOOK_CATE_DETAI.txt"
                    || NextFile.Name == "DOC_POS_LICENCE.txt"
                    || NextFile.Name == "DOC_BAL_CENTER.txt"
                    || NextFile.Name.Substring(0, 3) == "PUB"
                    || NextFile.Name.Substring(0, 3) == "SYS"
                    )
                {
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    GetTableByReplace(activeDir, Tablesname, StrName);
                }


                else if (NextFile.Extension == ".txt" && NextFile.Name != "SendList.txt" && NextFile.Name != "ServerToShop.txt")
                {
                    string StrName = NextFile.Name;
                    string Tablesname = StrName.Replace(".txt", "");
                    GetTable(activeDir, Tablesname, StrName);
                }
            }

            try
            {
                Directory.Delete(activeDir, true);  //上傳完刪目錄
            }
            catch (Exception ex)
            {
                return;
            }

        }

        #region insert datatable
        protected string GetTablePriKey(string strTableName)
        {
            string strKey = "";
            if (strTableName == "ACC_BC_GOODS_BATCH_COST") { strKey = "BC_GOODS_BATCH_COST_ID"; }
            else if (strTableName == "ACC_BC_GOODS_COST") { strKey = "BC_GOODS_COST_ID"; }
            else if (strTableName == "ACC_COUPON") { strKey = "COUPON_ID"; }
            else if (strTableName == "ACC_DAY_ORG_TRADE_DTL") { strKey = "DAY_ORG_TRADE_DTL_ID"; }
            else if (strTableName == "ACC_MEMBER_TREATMENT") { strKey = "MEMBER_TREATMENT_ID"; }
            else if (strTableName == "ACC_MEMBER_TREATMENT_BOM") { strKey = "MEMBER_TREATMENT_ID"; }
            else if (strTableName == "ACC_PERIOD_ORG_TRADE") { strKey = "PERIOD_ID"; }
            else if (strTableName == "ACC_PERIOD_ORG_TRADE_DTL") { strKey = "PERIOD_ORG_TRADE_DTL_ID"; }
            else if (strTableName == "ACC_STOCK_GOODS") { strKey = "GOODS_ID"; }
            else if (strTableName == "ACC_STOCK_GOODS_COLOR") { strKey = "STOCK_GOODS_COLOR_ID"; }
            else if (strTableName == "ACC_STOCK_GOODS_LOT") { strKey = "STOCK_GOODS_LOT_ID"; }
            else if (strTableName == "ACC_STOCK_GOODS_LOT_BATCH") { strKey = "STOCK_GOODS_LOT_BATCH_ID"; }
            else if (strTableName == "ACC_SV_CARD") { strKey = "SV_CARD_ID"; }
            else if (strTableName == "ACC_SV_CARD_LOG") { strKey = "SV_CARD_LOG_ID"; }
            else if (strTableName == "BIL_ADJUST") { strKey = "ADJUST_ID"; }
            else if (strTableName == "BIL_ADJUST_ACC_BATCH") { strKey = "ADJUST_ID"; }
            else if (strTableName == "BIL_ADJUST_DTL") { strKey = "ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_ADJUST_DTL_EXPAND") { strKey = "ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_ADJUST_DTL_MTX") { strKey = "ADJUST_DTL_MTX_ID"; }
            else if (strTableName == "BIL_ADJUST_EXPAND") { strKey = "ADJUST_ID"; }
            else if (strTableName == "BIL_ADJUST_LOG") { strKey = "ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_APPLIED") { strKey = "APPLIED_ID"; }
            else if (strTableName == "BIL_APPLIED_DTL") { strKey = "APPLIED_DTL_ID"; }
            else if (strTableName == "BIL_APPLIED_EXPAND") { strKey = "APPLIED_ID"; }
            else if (strTableName == "BIL_APPLIED_LOG") { strKey = "APPLIED_LOG_ID"; }
            else if (strTableName == "BIL_BONUS_ADJUST") { strKey = "BONUS_ADJUST_ID"; }
            else if (strTableName == "BIL_BONUS_ADJUST_DTL") { strKey = "BONUS_ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_BONUS_ADJUST_EXPAND") { strKey = "BONUS_ADJUST_ID"; }
            else if (strTableName == "BIL_BONUS_ADJUST_LOG") { strKey = "BONUS_ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_BONUS_RULE") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_CONSUME") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_EXPAND") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_GIVING") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_GOODS") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_GOODS_TYPE") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_LOG") { strKey = "BONUS_RULE_LOG_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_MEMBER_TYPE") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BONUS_RULE_SHOP") { strKey = "BONUS_RULE_ID"; }
            else if (strTableName == "BIL_BOOKING") { strKey = "BOOKING_ID"; }
            else if (strTableName == "BIL_BOOKING_COLLECT") { strKey = "BOOKING_COLLECT_ID"; }
            else if (strTableName == "BIL_BOOKING_COLLECT_WAGE") { strKey = "BOOKING_COLLECT_WAGE_ID"; }
            else if (strTableName == "BIL_BOOKING_EXPAND") { strKey = "BOOKING_ID"; }
            else if (strTableName == "BIL_BOOKING_LOG") { strKey = "BOOKING_LOG_ID"; }
            else if (strTableName == "BIL_BOOKING_RESOURCE") { strKey = "BOOKING_ID"; }
            else if (strTableName == "BIL_BOOKING_STAFF") { strKey = "BOOKING_SPLIT_ID"; }
            else if (strTableName == "BIL_BOOKING_WAGE") { strKey = "BOOKING_ID"; }
            else if (strTableName == "BIL_CLOCK_ADJUST") { strKey = "CLOCK_ADJUST_ID"; }
            else if (strTableName == "BIL_CLOCK_ADJUST_DETAIL") { strKey = "CLOCK_ADJUST_DETAIL_ID"; }
            else if (strTableName == "BIL_CLOCK_ADJUST_EXPAND") { strKey = "CLOCK_ADJUST_ID"; }
            else if (strTableName == "BIL_CLOCK_ADJUST_LOG") { strKey = "CLOCK_ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_COMPOSE") { strKey = "COMPOSE_ID"; }
            else if (strTableName == "BIL_COMPOSE_DTL") { strKey = "COMPOSE_DTL_ID"; }
            else if (strTableName == "BIL_COMPOSE_EXPAND") { strKey = "COMPOSE_ID"; }
            else if (strTableName == "BIL_COMPOSE_EXPENSE") { strKey = "COMPOSE_EXPENSE_ID"; }
            else if (strTableName == "BIL_COMPOSE_LOG") { strKey = "COMPOSE_LOG_ID"; }
            else if (strTableName == "BIL_COMPOSE_MATERIAL") { strKey = "COMPOSE_MATERIAL_ID"; }
            else if (strTableName == "BIL_COUPON_APPLIED") { strKey = "COUPON_APPLIED_ID"; }
            else if (strTableName == "BIL_COUPON_APPLIED_DTL") { strKey = "COUPON_APPLIED_DTL_ID"; }
            else if (strTableName == "BIL_COUPON_APPLIED_EXPAND") { strKey = "COUPON_APPLIED_ID"; }
            else if (strTableName == "BIL_COUPON_APPLIED_LOG") { strKey = "COUPON_APPLIED_LOG_ID"; }
            else if (strTableName == "BIL_COUPON_APPLIED_SN") { strKey = "COUPON_APPLIED_SN_ID"; }
            else if (strTableName == "BIL_COUPON_SET") { strKey = "COUPON_SET_ID"; }
            else if (strTableName == "BIL_COUPON_SET_EXPAND") { strKey = "COUPON_SET_ID"; }
            else if (strTableName == "BIL_COUPON_SET_GOODS") { strKey = "COUPON_SET_ID"; }
            else if (strTableName == "BIL_COUPON_SET_LOG") { strKey = "COUPON_SET_LOG_ID"; }
            else if (strTableName == "BIL_COUPON_SET_SHOP") { strKey = "COUPON_SET_ID"; }
            else if (strTableName == "BIL_DISC_QUOTA") { strKey = "DISC_QUOTA_ID"; }
            else if (strTableName == "BIL_DISC_QUOTA_DTL") { strKey = "DISC_QUOTA_DTL_ID"; }
            else if (strTableName == "BIL_DISC_QUOTA_LOG") { strKey = "DISC_QUOTA_LOG_ID"; }
            else if (strTableName == "BIL_HOLD") { strKey = "HOLD_ID"; }
            else if (strTableName == "BIL_HOLD_DTL") { strKey = "HOLD_DTL_ID"; }
            else if (strTableName == "BIL_HOLD_EXPAND") { strKey = "HOLD_ID"; }
            else if (strTableName == "BIL_HOLD_LOG") { strKey = "HOLD_LOG_ID"; }
            else if (strTableName == "BIL_INVOICE") { strKey = "INVOICE_ID"; }
            else if (strTableName == "BIL_INVOICE_DTL") { strKey = "INVOICE_DTL_ID"; }
            else if (strTableName == "BIL_INVOICE_DTL_EXPAND") { strKey = "INVOICE_DTL_ID"; }
            else if (strTableName == "BIL_INVOICE_EXPAND") { strKey = "INVOICE_ID"; }
            else if (strTableName == "BIL_INVOICE_EXPENSE") { strKey = "INVOICE_EXPENSE_ID"; }
            else if (strTableName == "BIL_INVOICE_LOG") { strKey = "INVOICE_LOG_ID"; }
            else if (strTableName == "BIL_MEMBER_CONSIGN") { strKey = "MEMBER_CONSIGN_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION") { strKey = "MIXPROMOTION_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_DTL") { strKey = "MIXPROMOTION_DTL_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_DTL_BUY") { strKey = "MIXPROMOTION_DTL_BUY_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_DTL_GIFT") { strKey = "MIXPROMOTION_DTL_GIFT_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_EXPAND") { strKey = "MIXPROMOTION_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_GIFT") { strKey = "MIXPROMOTION_GIFT_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_LOG") { strKey = "MIXPROMOTION_LOG_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_SHOP") { strKey = "MIXPROMOTION_ID"; }
            else if (strTableName == "BIL_MIXPROMOTION_VIPTYPE") { strKey = "MIXPROMOTION_ID"; }
            else if (strTableName == "BIL_ORDER") { strKey = "ORDER_ID"; }
            else if (strTableName == "BIL_ORDER_DTL") { strKey = "ORDER_DTL_ID"; }
            else if (strTableName == "BIL_ORDER_DTL_SOURCE") { strKey = "ORDER_DTL_SOURCE_ID"; }
            else if (strTableName == "BIL_ORDER_EXPAND") { strKey = "ORDER_ID"; }
            else if (strTableName == "BIL_ORDER_LOG") { strKey = "ORDER_LOG_ID"; }
            else if (strTableName == "BIL_PRICE_ADJUST") { strKey = "PRICE_ADJUST_ID"; }
            else if (strTableName == "BIL_PRICE_ADJUST_DTL") { strKey = "PRICE_ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_PRICE_ADJUST_EXPAND") { strKey = "PRICE_ADJUST_ID"; }
            else if (strTableName == "BIL_PRICE_ADJUST_LOG") { strKey = "PRICE_ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_PROMOTION") { strKey = "PROMOTION_ID"; }
            else if (strTableName == "BIL_PROMOTION_DTL") { strKey = "PROMOTION_DTL_ID"; }
            else if (strTableName == "BIL_PROMOTION_DTL_EXPAND") { strKey = "PROMOTION_DTL_EXPAND_ID"; }
            else if (strTableName == "BIL_PROMOTION_EXPAND") { strKey = "PROMOTION_ID"; }
            else if (strTableName == "BIL_PROMOTION_LOG") { strKey = "PROMOTION_LOG_ID"; }
            //else if(strTableName == "BIL_RETAIL_CHARGES") { strKey = "ID"; }
            else if (strTableName == "BIL_RETAIL") { strKey = "RETAIL_ID"; }
            else if (strTableName == "BIL_RETAIL_BONUS") { strKey = "RETAIL_BONUS_ID"; }
            else if (strTableName == "BIL_RETAIL_CHARGES") { strKey = "RETAIL_CHARGES_ID"; }
            else if (strTableName == "BIL_RETAIL_COLLECT") { strKey = "RETAIL_COLLECT_ID"; }
            else if (strTableName == "BIL_RETAIL_DTL") { strKey = "RETAIL_DTL_ID"; }
            else if (strTableName == "BIL_RETAIL_DTL_EXPAND") { strKey = "RETAIL_DTL_ID"; }
            else if (strTableName == "BIL_RETAIL_DTL_WAGE") { strKey = "RETAIL_DTL_WAGE_ID"; }
            else if (strTableName == "BIL_RETAIL_EXPAND") { strKey = "RETAIL_ID"; }
            else if (strTableName == "BIL_RETAIL_LOG") { strKey = "RETAIL_LOG_ID"; }
            else if (strTableName == "BIL_RETAIL_PAYMENT") { strKey = "RETAIL_PAYMENT_ID"; }
            else if (strTableName == "BIL_RETAIL_PROMOTION") { strKey = "RETAIL_PROMOTION_ID"; }
            else if (strTableName == "BIL_RETAIL_SHIFT") { strKey = "RETAIL_SHIFT_ID"; }
            else if (strTableName == "BIL_RETAIL_SHIFT_EXPAND") { strKey = "RETAIL_SHIFT_ID"; }
            else if (strTableName == "BIL_RETAIL_SHIFT_PAYMENT") { strKey = "RETAIL_SHIFT_ID"; }
            else if (strTableName == "BIL_RETAIL_SHOP_SPLIT") { strKey = "RETAIL_ID"; }
            else if (strTableName == "BIL_RETAIL_SHOP_SPLIT_WAGE") { strKey = "RETAIL_ID,SHOP_ID,WAGE_TYPE_ID"; }
            else if (strTableName == "BIL_RETAIL_STAFF_SPLIT") { strKey = "RETAIL_STAFF_SPLIT_ID"; }
            else if (strTableName == "BIL_RETAIL_STAFF_SPLIT_DTL") { strKey = "RETAIL_STAFF_SPLIT_DTL_ID"; }
            else if (strTableName == "BIL_RETAIL_SUBTOTAL") { strKey = "RETAIL_SUBTOTAL_ID"; }
            else if (strTableName == "BIL_RETAIL_SUBTOTAL_WAGE") { strKey = "RETAIL_SUBTOTAL_WAGE_ID"; }
            else if (strTableName == "BIL_RETAIL_SUM_WAGE") { strKey = "RETAIL_SUM_WAGE_ID"; }
            else if (strTableName == "BIL_TREATED_SIGNATURE") { strKey = "TREATED_ID"; }
            else if (strTableName == "BIL_RETAIL_SIGNATURE") { strKey = "RETAIL_ID"; }
            else if (strTableName == "BIL_RETURN") { strKey = "RETURN_ID"; }
            else if (strTableName == "BIL_RETURN_DTL") { strKey = "RETURN_DTL_ID"; }
            else if (strTableName == "BIL_RETURN_EXPAND") { strKey = "RETURN_ID"; }
            else if (strTableName == "BIL_RETURN_LOG") { strKey = "RETURN_LOG_ID"; }
            else if (strTableName == "BIL_SHOP_APPLIED") { strKey = "SHOP_APPLIED_ID"; }
            else if (strTableName == "BIL_SHOP_APPLIED_DTL") { strKey = "SHOP_APPLIED_DTL_ID"; }
            else if (strTableName == "BIL_SHOP_APPLIED_DTL_SOURCE") { strKey = "SHOP_APPLIED_DTL_SOURCE_ID"; }
            else if (strTableName == "BIL_SHOP_APPLIED_EXPAND") { strKey = "SHOP_APPLIED_ID"; }
            else if (strTableName == "BIL_SHOP_APPLIED_LOG") { strKey = "SHOP_APPLIED_LOG_ID"; }
            else if (strTableName == "BIL_STOCK") { strKey = "STOCK_ID"; }
            else if (strTableName == "BIL_STOCK_ACC_BATCH") { strKey = "STOCK_DTL_ID"; }
            else if (strTableName == "BIL_STOCK_DTL") { strKey = "STOCK_DTL_ID"; }
            else if (strTableName == "BIL_STOCK_DTL_EXPAND") { strKey = "STOCK_DTL_ID"; }
            else if (strTableName == "BIL_STOCK_EXPAND") { strKey = "STOCK_ID"; }
            else if (strTableName == "BIL_STOCK_LOG") { strKey = "STOCK_LOG_ID"; }
            else if (strTableName == "BIL_TAKE") { strKey = "TAKE_ID"; }
            else if (strTableName == "BIL_TAKE_DTL") { strKey = "TAKE_DTL_ID"; }
            else if (strTableName == "BIL_TAKE_EXPAND") { strKey = "TAKE_ID"; }
            else if (strTableName == "BIL_TAKE_LOG") { strKey = "TAKE_LOG_ID"; }
            else if (strTableName == "BIL_TAKE_NFD_BARCODE") { strKey = "TAKE_NFD_BARCODE_ID"; }
            else if (strTableName == "BIL_TAKE_NFD_ITEM") { strKey = "TAKE_NFD_ITEM_ID"; }
            else if (strTableName == "BIL_TAKE_NOT_FOUND") { strKey = "TAKE_NOT_FOUND_ID"; }
            else if (strTableName == "BIL_TAKEPLAN") { strKey = "TAKEPLAN_ID"; }
            else if (strTableName == "BIL_TAKEPLAN_DTL") { strKey = "TAKEPLAN_DTL_ID"; }
            else if (strTableName == "BIL_TAKEPLAN_DTL_EXPAND") { strKey = "TAKEPLAN_DTL_ID"; }
            else if (strTableName == "BIL_TAKEPLAN_EXPAND") { strKey = "TAKEPLAN_ID"; }
            else if (strTableName == "BIL_TAKEPLAN_LOG") { strKey = "TAKEPLAN_LOG_ID"; }
            else if (strTableName == "BIL_TRANSFER") { strKey = "TRANSFER_ID"; }
            else if (strTableName == "BIL_TRANSFER_DTL") { strKey = "TRANSFER_DTL_ID"; }
            else if (strTableName == "BIL_TRANSFER_DTL_EXPAND") { strKey = "TRANSFER_DTL_ID"; }
            else if (strTableName == "BIL_TRANSFER_EXPAND") { strKey = "TRANSFER_ID"; }
            else if (strTableName == "BIL_TRANSFER_LOG") { strKey = "TRANSFER_LOG_ID"; }
            else if (strTableName == "BIL_TREATED") { strKey = "TREATED_ID"; }
            else if (strTableName == "BIL_TREATED_COLLECT") { strKey = "TREATED_COLLECT_ID"; }
            else if (strTableName == "BIL_TREATED_COLLECT_BOM") { strKey = "TREATED_COLLECT_ID"; }
            else if (strTableName == "BIL_TREATED_COLLECT_WAGE") { strKey = "TREATED_COLLECT_WAGE_ID"; }
            else if (strTableName == "BIL_TREATED_EXPAND") { strKey = "TREATED_ID"; }
            else if (strTableName == "BIL_TREATED_LOG") { strKey = "TREATED_LOG_ID"; }
            else if (strTableName == "BIL_TREATED_QUESTIONNAIRE") { strKey = "TREATED_ID"; }
            else if (strTableName == "BIL_TREATED_QUESTIONNAIRE_DTL") { strKey = "TREATED_QUESTIONNAIRE_DTL_ID"; }
            else if (strTableName == "BIL_TREATED_SPLIT") { strKey = "TREATED_SPLIT_ID"; }
            else if (strTableName == "BIL_TREATED_WAGE") { strKey = "TREATED_ID"; }
            else if (strTableName == "BIL_VIPSTATUS_ADJUST") { strKey = "VIPSTATUS_ADJUST_ID"; }
            else if (strTableName == "BIL_VIPSTATUS_ADJUST_DTL") { strKey = "VIPSTATUS_ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_VIPSTATUS_ADJUST_EXPAND") { strKey = "VIPSTATUS_ADJUST_ID"; }
            else if (strTableName == "BIL_VIPSTATUS_ADJUST_LOG") { strKey = "VIPSTATUS_ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST") { strKey = "VIPTYPE_ADJUST_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_DTL") { strKey = "VIPTYPE_ADJUST_DTL_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_DTL_SELL") { strKey = "VIPTYPE_ADJUST_DTL_SELL_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_EXPAND") { strKey = "VIPTYPE_ADJUST_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_LOG") { strKey = "VIPTYPE_ADJUST_LOG_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_SUED") { strKey = "VIPTYPE_ADJUST_SUED_ID"; }
            else if (strTableName == "BIL_VIPTYPE_ADJUST_SUED_GTYPE") { strKey = "VIPTYPE_ADJUST_SUED_ID"; }
            else if (strTableName == "DEFER_MONTH_END") { strKey = "ID"; }
            else if (strTableName == "DEPOSIT_MONTH_END") { strKey = "ID"; }
            else if (strTableName == "DOC_BAL_CENTER") { strKey = "BAL_CENTER_ID"; }
            else if (strTableName == "DOC_COMPANY") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_COMPANY_INSTEAD") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_CUSTOMER") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_GOODS") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_BARCODE") { strKey = "GOODS_BARCODE_ID"; }
            else if (strTableName == "DOC_GOODS_BOM") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_COLOR") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_EXPAND") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_IMAGE") { strKey = "GOODS_IMAGE_ID"; }
            else if (strTableName == "DOC_GOODS_ONLINE_EXPAND") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_PRICE") { strKey = "GOODS_ID,CURR_ID"; }
            else if (strTableName == "DOC_GOODS_PRICE_GROUP") { strKey = "GOODS_PRICE_GROUP_ID"; }
            else if (strTableName == "DOC_GOODS_SUB_TREATMENT") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_SUPPLIER_IN_PRICE") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GOODS_TREAT_CONSUM") { strKey = "GOODS_TREAT_CONSUM_ID"; }
            else if (strTableName == "DOC_GOODS_TREAT_EQUIPMENT") { strKey = "GOODS_TREAT_EQUIPMENT_ID"; }
            else if (strTableName == "DOC_GOODS_TREAT_PROCESS") { strKey = "GOODS_TREAT_PROCESS_ID"; }
            else if (strTableName == "DOC_GOODS_USAGE") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_GROUP_USER") { strKey = "STAFF_ID,GROUP_ID"; }
            else if (strTableName == "DOC_ITEM_GROUP") { strKey = "ITEM_GROUP_ID"; }
            else if (strTableName == "DOC_ITEM_GROUP_DTL") { strKey = "ITEM_GROUP_ID"; }
            else if (strTableName == "DOC_LOCATION") { strKey = "LOCATION_ID"; }
            else if (strTableName == "DOC_MEMBER") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_ADDRESS") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_BONUS_LOG") { strKey = "MEMBER_BONUS_LOG_ID"; }
            else if (strTableName == "DOC_MEMBER_DISC_QUOTA") { strKey = "MEMBER_DISC_QUOTA_ID"; }
            else if (strTableName == "DOC_MEMBER_EXPAND") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_FOLDER") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_FOLLOW") { strKey = "MEMBER_FOLLOW_ID"; }
            else if (strTableName == "DOC_MEMBER_PHOTO") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_SELL") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_MEMBER_SKIN_CONCERNS") { strKey = "MEMBER_SKIN_CONCERNS_ID"; }
            else if (strTableName == "DOC_MEMBER_SKIN_STATUS") { strKey = "MEMBER_SKIN_STATUS_ID"; }
            else if (strTableName == "DOC_MIX_MATCH") { strKey = "MIX_MATCH_ID"; }
            else if (strTableName == "DOC_MIX_MATCH_BUY") { strKey = "MIX_MATCH_BUY_ID"; }
            else if (strTableName == "DOC_MIX_MATCH_GET") { strKey = "MIX_MATCH_GET_ID"; }
            else if (strTableName == "DOC_MSN") { strKey = "MSN_ID"; }
            else if (strTableName == "DOC_MSN_ATTACHMENT") { strKey = "MSN_ATTACHMENT_ID"; }
            else if (strTableName == "DOC_MSN_TO") { strKey = "MSN_TO_ID"; }
            else if (strTableName == "DOC_ORG") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_ORG_ADDRESS") { strKey = "ORG_ID,ADDRESS_ID"; }
            else if (strTableName == "DOC_ORG_CONTACT") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_ORG_CUSTOMER") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_ORG_D_SELL_TARGET") { strKey = "ORG_D_SELL_TARGET_ID"; }
            else if (strTableName == "DOC_ORG_GOODS_IN_PRICE") { strKey = "GOODS_ID"; }
            else if (strTableName == "DOC_ORG_GOODS_PARAM") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_ORG_GOODS_PRICE") { strKey = "ORG_GOODS_PRICE_ID"; }
            else if (strTableName == "DOC_ORG_GOODS_RELATION") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_ORG_LOGO") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_ORG_M_SELL_TARGET") { strKey = "ORG_M_SELL_TARGET_ID"; }
            else if (strTableName == "DOC_ORG_MEMBER_RELATION") { strKey = "MEMBER_ID"; }
            else if (strTableName == "DOC_ORG_PAYMENT") { strKey = "ORG_ID,POS_PAYMENT_ID"; }
            else if (strTableName == "DOC_ORG_STAFF") { strKey = "STAFF_ID,ORG_ID"; }
            else if (strTableName == "DOC_ORG_SUPERVISOR") { strKey = "ORG_SUPERVISOR_ID"; }
            else if (strTableName == "DOC_ORG_SUPPLIER") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_ORG_WAGE_BOOK") { strKey = "ORG_ID"; }
            else if (strTableName == "DOC_PARTNER") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_PARTNER_ADDRESS") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_PARTNER_CONTACT") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_POS_LICENCE") { strKey = "POS_LICENCE_ID"; }
            else if (strTableName == "DOC_QUESTION") { strKey = "QUESTION_ID"; }
            else if (strTableName == "DOC_QUESTIONNAIRE") { strKey = "QUESTIONNAIRE_ID"; }
            else if (strTableName == "DOC_QUESTIONNAIRE_QUESTION") { strKey = "QUESTIONNAIRE_QUESTION_ID"; }
            else if (strTableName == "DOC_REPLENISHMENT_SETTING") { strKey = "REPLENISHMENT_SETTING_ID"; }
            else if (strTableName == "DOC_RESOURCE") { strKey = "RESOURCE_ID"; }
            else if (strTableName == "DOC_STAFF") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_ADDRESS") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_CLOCK") { strKey = "STAFF_CLOCK_ID"; }
            else if (strTableName == "DOC_STAFF_CLOCK_PHOTO") { strKey = "STAFF_CLOCK_ID"; }
            else if (strTableName == "DOC_STAFF_CLOCK_REMARKS") { strKey = "STAFF_CLOCK_REMARKS_ID"; }
            else if (strTableName == "DOC_STAFF_ELEC_SIGNATURE") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_EXPAND") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_M_SELL_TARGET") { strKey = "STAFF_M_SELL_TARGET_ID"; }
            else if (strTableName == "DOC_STAFF_PHOTO") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_POSITION") { strKey = "STAFF_ID,POSITION_ID"; }
            else if (strTableName == "DOC_STAFF_QUALIFICATION") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_STAFF_WAGE_TYPE") { strKey = "STAFF_ID"; }
            else if (strTableName == "DOC_SUB_TREATMENT_RULE") { strKey = "SUB_TREATMENT_RULE_ID"; }
            else if (strTableName == "DOC_SUPPLIER") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_SUPPLIER_GOODS") { strKey = "PARTNER_ID"; }
            else if (strTableName == "DOC_WAGE_BOOK") { strKey = "WAGE_BOOK_ID"; }
            else if (strTableName == "DOC_WAGE_BOOK_AREA") { strKey = "WAGE_TYPE_ID,WAGE_AREA_ID"; }
            else if (strTableName == "DOC_WAGE_BOOK_CATE_DETAIL") { strKey = "WAGE_BOOK_CATE_DETAIL_ID"; }
            else if (strTableName == "DOC_WAGE_BOOK_DETAIL") { strKey = "WAGE_BOOK_DETAIL_ID"; }
            else if (strTableName == "DOC_WAREHOUSE") { strKey = "ORG_ID"; }
            else if (strTableName == "LOG_STOCK_GOODS_LOT_BATCH") { strKey = "STOCK_LOG_ID"; }
            else if (strTableName == "LOG_SYS_OPERATE") { strKey = "LOG_SYS_OPERATE_ID"; }
            else if (strTableName == "LOG_WAGE_BOOK_CATE_DETAIL") { strKey = "LOG_WAGE_BOOK_CATE_DETAIL_ID"; }
            else if (strTableName == "LOG_WAGE_BOOK_DETAIL") { strKey = "LOG_WAGE_BOOK_DETAIL_ID"; }
            else if (strTableName == "PI_BIL_CHECK_2") { strKey = "TB"; }
            else if (strTableName == "PUB_ADDRESS") { strKey = "ADDRESS_ID"; }
            else if (strTableName == "PUB_ADDRESS_IMAGE") { strKey = "ADDRESS_IMAGE_ID"; }
            else if (strTableName == "PUB_BARCODE_TYPE") { strKey = "BARCODE_TYPE_ID"; }
            else if (strTableName == "PUB_BIL_CODE_GEN_RULE") { strKey = "GEN_RULE_ID"; }
            else if (strTableName == "PUB_BOOKING_STATUS_COLOR") { strKey = "STATUS_COLOR_ID"; }
            else if (strTableName == "PUB_BRAND") { strKey = "BRAND_ID"; }
            else if (strTableName == "PUB_BRAND_IMAGE") { strKey = "BRAND_IMAGE_ID"; }
            else if (strTableName == "PUB_COLOR") { strKey = "COLOR_ID"; }
            else if (strTableName == "PUB_CONTACT") { strKey = "CONTACT_ID"; }
            else if (strTableName == "PUB_CURR_EX_RATE") { strKey = "CURR_ID"; }
            else if (strTableName == "PUB_CURR_MON_RATE") { strKey = "CURR_MON_RATE_ID"; }
            else if (strTableName == "PUB_GOODS_CATE") { strKey = "GOODS_CATE_ID,PARENT_CATE_ID"; }
            else if (strTableName == "PUB_GOODS_CATE_ONLINE") { strKey = "CATE_ID"; }
            else if (strTableName == "PUB_GOODS_CATE_TYPE_ONLINE") { strKey = "CATE_TYPE_ID"; }
            else if (strTableName == "PUB_GOODS_CODE_RULE") { strKey = "GOODS_CODE_RULE_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE") { strKey = "MEMBER_TYPE_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE_DISC") { strKey = "MEMBER_TYPE_DISC_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE_DISC_BRAND") { strKey = "MEMBER_TYPE_DISC_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE_DISC_GTYPE") { strKey = "MEMBER_TYPE_DISC_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE_SUED") { strKey = "MEMBER_TYPE_SUED_ID"; }
            else if (strTableName == "PUB_MEMBER_TYPE_SUED_GTYPE") { strKey = "MEMBER_TYPE_SUED_ID"; }
            else if (strTableName == "PUB_POS_PAYMENT") { strKey = "POS_PAYMENT_ID"; }
            else if (strTableName == "PUB_REGION") { strKey = "REGION_ID"; }
            else if (strTableName == "PUB_RETAIL_CHARGES") { strKey = "CHARGES_ID"; }
            else if (strTableName == "PUB_SUB_BRAND") { strKey = "SUB_BRAND_ID"; }
            else if (strTableName == "PUB_SV_CARD_TYPE") { strKey = "SV_CARD_TYPE_ID"; }
            else if (strTableName == "PUB_WAGE_TYPE") { strKey = "WAGE_TYPE_ID"; }
            else if (strTableName == "SAL_MONTH_END") { strKey = "END_YEAR"; }
            else if (strTableName == "staff_info") { strKey = "staff_code"; }
            else if (strTableName == "staff_sign") { strKey = "sh_code"; }
            else if (strTableName == "STK_ADJ_0AMT_20131231") { strKey = "WH_CODE"; }
            else if (strTableName == "STK_ADJ_IN_AMT_20131231") { strKey = "WH_CODE"; }
            else if (strTableName == "STK_AGING_MONTH_END") { strKey = "END_YEAR"; }
            else if (strTableName == "STK_BAL_MONTH_END") { strKey = "END_YEAR"; }
            else if (strTableName == "STK_BAL_MONTH_END_V2") { strKey = "END_YEAR"; }
            else if (strTableName == "SYS_BIL_STATUS") { strKey = "BIL_STATUS_ID"; }
            else if (strTableName == "SYS_BIL_TYPE") { strKey = "BIL_TYPE_ID"; }
            else if (strTableName == "SYS_BONUS_FACTOR_TYPE") { strKey = "BONUS_FACTOR_TYPE_ID"; }
            else if (strTableName == "SYS_CLOCK_ADJUST_TYPE") { strKey = "CLOCK_ADJUST_TYPE_ID"; }
            else if (strTableName == "SYS_CONVERT_DATA") { strKey = "CONVERT_DATA_ID"; }
            else if (strTableName == "SYS_COUPON_TYPE") { strKey = "COUPON_TYPE_ID"; }
            else if (strTableName == "SYS_DATA_RESOURCE") { strKey = "DATA_RESOURCE_ID"; }
            else if (strTableName == "SYS_DATA_RESOURCE_TYPE") { strKey = "DATA_RESOURCE_TYPE_ID"; }
            else if (strTableName == "SYS_DB_VERSION") { strKey = "DB_VERSION_ID"; }
            else if (strTableName == "SYS_EXCHANGE_MODE") { strKey = "EXCHANGE_MODE_ID"; }
            else if (strTableName == "SYS_EXPIRED_CALC_MODE") { strKey = "EXPIRED_CALC_MODE_ID"; }
            else if (strTableName == "SYS_FUNCTION") { strKey = "FUNCTION_ID"; }
            else if (strTableName == "SYS_FUNCTION_OPERATE") { strKey = "FUNCTION_ID"; }
            else if (strTableName == "SYS_FUNCTION_QUERY") { strKey = "FUNCTION_QUERY_ID"; }
            else if (strTableName == "SYS_GBBIG_CODE") { strKey = "ID"; }
            else if (strTableName == "SYS_GOODS_LIMIT_TYPE") { strKey = "GOODS_LIMIT_TYPE_ID"; }
            else if (strTableName == "SYS_GOODS_TYPE") { strKey = "GOODS_TYPE_ID"; }
            else if (strTableName == "SYS_GROUP") { strKey = "GROUP_ID"; }
            else if (strTableName == "SYS_GROUP_FUNCTION_OPERATE") { strKey = "GROUP_ID"; }
            else if (strTableName == "SYS_GROUP_SPEC_PERM") { strKey = "GROUP_ID"; }
            else if (strTableName == "SYS_LACK_STATUS") { strKey = "LACK_STATUS_ID"; }
            else if (strTableName == "SYS_MEMBER_UPGRADE") { strKey = "MEMBER_UPGRADE_ID"; }
            else if (strTableName == "SYS_MIX_MATCH_GET_TYPE") { strKey = "MIX_MATCH_GET_TYPE_ID"; }
            else if (strTableName == "SYS_MIX_MATCH_TYPE") { strKey = "MIX_MATCH_TYPE_ID"; }
            else if (strTableName == "SYS_MIX_RULE") { strKey = "MIX_RULE_ID"; }
            else if (strTableName == "SYS_MIX_TYPE") { strKey = "MIX_TYPE_ID"; }
            else if (strTableName == "SYS_MODULE") { strKey = "MODULE_ID"; }
            else if (strTableName == "SYS_MONTH_END_STATUS") { strKey = "ME_STATUS_ID"; }
            else if (strTableName == "SYS_OPERATE") { strKey = "OPERATE_ID"; }
            else if (strTableName == "SYS_ORG_TYPE") { strKey = "ORG_TYPE_ID"; }
            else if (strTableName == "SYS_PARAM_SETTING") { strKey = "PARAM_SETTING_ID"; }
            else if (strTableName == "SYS_PARAM_SETTING_LOGO") { strKey = "PARAM_SETTING_ID"; }
            else if (strTableName == "SYS_PARAM_SETTING_POS") { strKey = "PARAM_SETTING_ID"; }
            else if (strTableName == "SYS_PERIOD") { strKey = "PERIOD_ID"; }
            else if (strTableName == "SYS_PERIOD_TYPE") { strKey = "PERIOD_TYPE_ID"; }
            else if (strTableName == "SYS_POS_LICENCE_TYPE") { strKey = "POS_LICENCE_TYPE_ID"; }
            else if (strTableName == "SYS_PRICE_TYPE") { strKey = "PRICE_TYPE_ID"; }
            else if (strTableName == "SYS_PROMOTION_RULE") { strKey = "PROMOTION_RULE_ID"; }
            else if (strTableName == "SYS_PROMOTION_TYPE") { strKey = "GOODS_TYPE_ID"; }
            else if (strTableName == "SYS_REF_DATA") { strKey = "REF_DATA_ID"; }
            else if (strTableName == "SYS_REF_DATA_TYPE") { strKey = "REF_DATA_TYPE_ID"; }
            else if (strTableName == "SYS_REPORT") { strKey = "REPORT_ID"; }
            else if (strTableName == "SYS_REPORT_GROUP") { strKey = "REPORT_ID"; }
            else if (strTableName == "SYS_REPORT_VARIABLE") { strKey = "REPORT_VARIABLE_ID"; }
            else if (strTableName == "SYS_RESET_RULE") { strKey = "RESET_RULE_ID"; }
            else if (strTableName == "SYS_RULE_DATA") { strKey = "RULE_DATA_ID"; }
            else if (strTableName == "SYS_RUNNING_NO") { strKey = "RUNNING_NO_ID"; }
            else if (strTableName == "SYS_SELL_TYPE") { strKey = "SELL_TYPE_ID"; }
            else if (strTableName == "SYS_SINGLE_REPORT") { strKey = "REPORT_ID"; }
            else if (strTableName == "SYS_SINGLE_REPORT_GROUP") { strKey = "REPORT_ID"; }
            else if (strTableName == "SYS_SPEC_PERM") { strKey = "SPEC_PERM_ID"; }
            else if (strTableName == "SYS_STOPPED_TYPE") { strKey = "STOPPED_TYPE_ID"; }
            else if (strTableName == "SYS_SV_CARD_PROPERTY") { strKey = "PROPERTY_ID"; }
            else if (strTableName == "SYS_TRADE_TYPE") { strKey = "TRADE_TYPE_ID"; }
            else if (strTableName == "SYS_TREATMENT_EXPIRED_TYPE") { strKey = "TREATMENT_EXPIRED_TYPE_ID"; }
            else if (strTableName == "SYS_TREATMENT_EXPIRY_RULE") { strKey = "TRT_EXP_RULE_ID"; }
            else if (strTableName == "SYS_TREATMENT_MANAGE_TYPE") { strKey = "TREATMENT_MANAGE_TYPE_ID"; }
            else if (strTableName == "SYS_TRT_EXP") { strKey = "TRT_EXP_ID"; }
            else if (strTableName == "SYS_TRT_EXP_DATE") { strKey = "TRT_EXP_DATE_ID"; }
            else if (strTableName == "SYS_TRT_EXP_DATE_DTL") { strKey = "TRT_EXP_DATE_DTL_ID"; }
            else if (strTableName == "SYS_TRT_EXP_DTL") { strKey = "TRT_EXP_DTL_ID"; }
            else if (strTableName == "SYS_TRT_EXP_SPEC_ITEM") { strKey = "TRT_EXP_SPEC_ITEM_ID"; }
            else if (strTableName == "SYS_TRT_EXP_SPEC_ITEM_DTL") { strKey = "TRT_EXP_SPEC_ITEM_DTL_ID"; }
            else if (strTableName == "SYS_TRT_EXP_SPEC_ITEM_GOODS") { strKey = "TRT_EXP_SPEC_ITEM_GOODS_ID"; }
            else if (strTableName == "SYS_TRT_UDF_EXPDATE_COMPANY") { strKey = "TRT_UDF_EXPDATE_COMPANY_ID"; }
            else if (strTableName == "sysdiagrams") { strKey = "name"; }
            else if (strTableName == "TBG_BIL_STOCK_LOG") { strKey = "STOCK_LOG_ID"; }
            else if (strTableName == "TBG_CHANGE_CATE") { strKey = "GOODS_ID"; }
            else if (strTableName == "TBG_CHANGE_DEPOSIT_SHOP") { strKey = "BIL_CODE"; }
            else if (strTableName == "TBG_CLEAR_BIL_RETAIL_LOG") { strKey = "RETAIL_ID"; }
            else if (strTableName == "TBG_CLEAR_BIL_STOCK_LOG") { strKey = "STOCK_ID"; }
            else if (strTableName == "TBG_CLEAR_BIL_TREATED_LOG") { strKey = "TREATED_ID"; }
            else if (strTableName == "TBG_Commission_History") { strKey = "ID"; }
            else if (strTableName == "TBG_DM_Right") { strKey = "STAFF_ID"; }
            else if (strTableName == "TBG_ItemDeposit_History") { strKey = "ID"; }
            else if (strTableName == "TBG_ItemSaleType_History") { strKey = "ID"; }
            else if (strTableName == "TBG_ItemSuspension_HISTORY") { strKey = "ID"; }
            else if (strTableName == "TBG_MONTH_END") { strKey = "ME_ID"; }
            else if (strTableName == "TBG_MONTH_END_LOG") { strKey = "ME_LOG_ID"; }
            else if (strTableName == "TBG_SIGNIN_LOG") { strKey = "ID"; }
            else { strKey = ""; }

            return strKey;

        }
        #endregion

        protected void GetTableByReplace(string activeDir, string strTableName, string txtName)
        {
            string line = "";
            string strCols = "";
            string strVals = "";

            string strTableKey = GetTablePriKey(strTableName.TrimEnd());
            string strTableKeyVal = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');

            int insertsuccess = 0;
            int insertfaile = 0;
            int updsuccess = 0;

            int nLastColumns = ColumnName.Length;

            //add by mark -->

            string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;

            string strSQLDelete = "truncate table " + strTableName;
            try
            {
                comm.CommandText = strSQLDelete;
                comm.ExecuteNonQuery().ToString();

            }
            catch (Exception)
            {
                connStr = "";
            }

            // add by mark 

            for (int j = 0; j < nLastColumns && j < ColumnName.Length; j++)
            {
                if (j < nLastColumns - 1 && j < ColumnName.Length - 1)
                {
                    strCols += ColumnName[j] + ",";
                }
                if (j == nLastColumns - 1 || j == ColumnName.Length - 1)
                {
                    strCols += ColumnName[j];
                }
            }

            for (int i = 1; i < s.Length; i++)
            {
                line = s[i];
                string strUpd = "";
                string[] Columnqty = s[i].Split('^');

                if (Columnqty.Length == nLastColumns)
                {
                    for (int j = 0; j < nLastColumns && j < Columnqty.Length; j++)
                    {
                        if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                        {
                            strVals += "'" + Columnqty[j].Replace("'", "‘") + "',";
                        }
                        if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                        {
                            strVals += "'" + Columnqty[j].Replace("'", "‘") + "'";
                        }

                        if (ColumnName[j] == strTableKey)
                        {
                            strTableKeyVal = Columnqty[j].Replace("'", "‘");
                        }

                        if (strTableKey != ColumnName[j] && ColumnName[j] != "STAMP")
                        {
                            if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                            {
                                strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "',";
                            }
                            if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                            {
                                strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "'";
                            }
                        }
                    }

                    strCols = strCols.Replace(",STAMP", "");
                    strCols = strCols.Replace(",REPORT_FILE", "");
                    strVals = strVals.Replace(",'System.Byte[]'", "");
                    strVals = strVals.Replace(",''", ",null");

                    //string StrSqlChk = "";

                    if (strUpd.Length > 1)
                    {
                        if (strUpd.Substring(strUpd.Length - 1, 1) == ",")
                        {
                            strUpd = strUpd.Remove(strUpd.Length - 1, 1);
                        }
                    }

                    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                        + strVals + "";

                    //modify by mark -->
                    try
                    {
                        comm.CommandText = strSQL;
                        int num = int.Parse(comm.ExecuteNonQuery().ToString());
                        strVals = "";
                        if (num > 0)
                        {
                            insertsuccess++;
                        }
                        else
                        {
                            insertfaile++;
                        }
                    }
                    catch (Exception)
                    {
                        insertfaile++;
                    }
                    //modify by mark <--

                }
            }
            conn.Close();
            MoveCurorLast();
            rTB.Text += string.Format("{0}資料更新成功 {1}條\n", strTableName, insertsuccess);

            DeleteUploadedData(strTableName);
        }

        protected void DeleteUploadedData(string strTableName)
        {
            string strSql = "";
            if (strTableName == "DOC_MEMBER_EXPAND")
                strSql = "delete DOC_MEMBER_EXPAND where MEMBER_ID = 0";
            else if (strTableName == "DOC_MEMBER")
                strSql = "delete DOC_MEMBER where MEMBER_ID = 0";
            else if (strTableName == "BIL_RETAIL_SIGNATURE") //add by mark 20170803
                strSql = "delete BIL_RETAIL_SIGNATURE where RETAIL_ID=0";
            else if (strTableName == "BIL_RETAIL_CHARGES")
                strSql = "delete BIL_RETAIL_CHARGES where RETAIL_CHARGES_ID=0";
            else if (strTableName == "BIL_RETAIL_PAYMENT")
                strSql = "delete BIL_RETAIL_PAYMENT where RETAIL_PAYMENT_ID=0";
            else if (strTableName == "BIL_RETAIL_DTL")
                strSql = "delete BIL_RETAIL_DTL where RETAIL_DTL_ID=0";
            else if (strTableName == "BIL_RETAIL")
                strSql = "delete BIL_RETAIL where RETAIL_ID=0";

            else if (strTableName == "BIL_TREATED_SIGNATURE") //add by mark 20170803
                strSql = "delete BIL_TREATED_SIGNATURE where TREATED_ID=0";
            else if (strTableName == "BIL_TREATED_COLLECT_WAGE")
                strSql = "delete BIL_TREATED_COLLECT_WAGE where TREATED_COLLECT_WAGE_ID=0";
            else if (strTableName == "BIL_TREATED_SPLIT")
                strSql = "delete BIL_TREATED_SPLIT where TREATED_SPLIT_ID=0";
            else if (strTableName == "BIL_TREATED_WAGE")
                strSql = "delete BIL_TREATED_WAGE where TREATED_ID=0";

            else if (strTableName == "BIL_TREATED_COLLECT")
                strSql = "delete BIL_TREATED_COLLECT where TREATED_COLLECT_ID=0";
            else if (strTableName == "BIL_TREATED")
                strSql = "delete BIL_TREATED where TREATED_ID=0";
            else
            {
                strSql = "";
            }
            if (strSql.Length > 0)
                ExportTotxt.ExceSql_int(strSql);
        }

        protected void GetTable(string activeDir, string strTableName, string txtName)
        {
            string line = "";
            string strCols = "";
            string strVals = "";

            string strTableKey = GetTablePriKey(strTableName.TrimEnd());
            string strTableKeyVal = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');

            //if (txtName.Contains("BIL_RETAIL_SIGNATURE") || txtName.Contains("BIL_TREATED_SIGNATURE"))
            //{
            //    strTableName = strTableName.Substring(0, strTableName.IndexOf("!"));
            //}

            int insertsuccess = 0;
            int insertfaile = 0;
            int updsuccess = 0;
            int updfaile = 0;

            bool db = true;

            int nLastColumns = ColumnName.Length;
            string sqldb = "select top 1 * from " + strTableName;
            DataTable qurrydb = ExportTotxt.QueryData(sqldb);
            if (qurrydb == null || qurrydb.Rows.Count < 1)
            {
                db = false;
            }
            else
            {
                nLastColumns = qurrydb.Columns.Count;
                if (qurrydb.Rows.Count < 1)
                    db = false;
            }



            for (int j = 0; j < nLastColumns && j < ColumnName.Length; j++)
            {
                if (j < nLastColumns - 1 && j < ColumnName.Length - 1)
                {
                    strCols += ColumnName[j] + ",";
                }
                if (j == nLastColumns - 1 || j == ColumnName.Length - 1)
                {
                    strCols += ColumnName[j];
                }
            }

            //add by mark -->
            string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            // add by mark 

            for (int i = 1; i < s.Length; i++)
            {
                line = s[i];
                string strUpd = "";
                string[] Columnqty = s[i].Split('^');

                if (Columnqty.Length == nLastColumns)
                {
                    for (int j = 0; j < nLastColumns && j < Columnqty.Length; j++)
                    {
                        if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                        {
                            strVals += "'" + Columnqty[j].Replace("'", "‘") + "',";
                        }
                        if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                        {
                            strVals += "'" + Columnqty[j].Replace("'", "‘") + "'";
                        }

                        if (ColumnName[j] == strTableKey)
                        {
                            strTableKeyVal = Columnqty[j].Replace("'", "‘");
                        }

                        if (strTableKey != ColumnName[j] && ColumnName[j] != "STAMP")
                        {
                            if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                            {
                                strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "',";
                            }
                            if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                            {
                                strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "'";
                            }
                        }
                    }

                    strCols = strCols.Replace(",STAMP", "");
                    strCols = strCols.Replace(",REPORT_FILE", "");
                    strVals = strVals.Replace(",'System.Byte[]'", "");
                    strVals = strVals.Replace(",''", ",null");

                    string StrSqlChk = "";

                    if (strUpd.Length > 1)
                    {
                        if (strUpd.Substring(strUpd.Length - 1, 1) == ",")
                        {
                            strUpd = strUpd.Remove(strUpd.Length - 1, 1);
                        }
                    }


                    if (db == false)
                    {
                        string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                            + strVals + "";
                        try
                        {
                            comm.CommandText = strSQL;
                            int num2 = int.Parse(comm.ExecuteNonQuery().ToString());
                            insertsuccess += num2;
                            strVals = "";
                        }
                        catch (Exception)
                        {
                            insertfaile++;
                        }
                        //int num = ExportTotxt.ExceSql_int(strSQL);
                        //strVals = "";

                        //if (num > 0)
                        //{
                        //    insertsuccess++;
                        //}
                        //else
                        //{
                        //    insertfaile++;
                        //}
                    }
                    else
                    {
                        //modify by mark -->
                        string StrSqlDelete = "delete " + strTableName + "  where " + strTableKey + "='" + strTableKeyVal + "'";
                        string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT " + strVals + "";
                        try
                        {
                            comm.CommandText = StrSqlDelete;
                            int num1 = int.Parse(comm.ExecuteNonQuery().ToString());
                            updsuccess += num1;


                            comm.CommandText = strSQL;
                            int num2 = int.Parse(comm.ExecuteNonQuery().ToString());
                            if (num1 <= 0)
                                insertsuccess += num2;

                            strVals = "";
                        }
                        catch (Exception)
                        {
                            insertfaile++;
                        }
                        //modify by mark <--
                        //StrSqlChk = "select * from " + strTableName + "  where " + strTableKey + "='" + strTableKeyVal + "'";
                        //DataTable dtex = ExportTotxt.QueryData(StrSqlChk);
                        //if (dtex != null && dtex.Rows.Count > 0)
                        //{
                        //    string strSQL = " update " + strTableName + " set " + strUpd + " where " + strTableKey + "='" + strTableKeyVal + "'";
                        //    int num = ExportTotxt.ExceSql_int(strSQL);
                        //    strVals = "";

                        //    if (num > 0)
                        //    {
                        //        updsuccess++;
                        //    }
                        //    else
                        //    {
                        //        updfaile++;
                        //    }

                        //}
                        //else
                        //{
                        //    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                        //        + strVals + "";

                        //    if (strTableKey.Length > 0)
                        //    {
                        //        strSQL += " where not exists(select 1 from " + strTableName + "  where " + strTableKey + "='" + strTableKeyVal + "')";
                        //    }

                        //    int num = ExportTotxt.ExceSql_int(strSQL);
                        //    strVals = "";

                        //    if (num > 0)
                        //    {
                        //        insertsuccess++;
                        //    }
                        //    else
                        //    {
                        //        insertfaile++;
                        //    }
                        //}
                    }
                }
            }
            conn.Close();
            //rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);
            MoveCurorLast();
            rTB.Text += string.Format("{0}資料更新成功 {1}條\n", strTableName, insertsuccess);

            DeleteUploadedData(strTableName);

        }

        protected void GetTableArrKey(string activeDir, string strTableName, string txtName)
        {
            string line = "";
            string strCols = "";
            string strVals = "";

            string strTableKey = GetTablePriKey(strTableName.TrimEnd());
            string strTableKeyVal = "";
            string[] s = File.ReadAllLines(activeDir + txtName);

            string[] ArrKey = strTableKey.Split(',');

            string[] ColumnName = s[0].Split('^');


            //add by mark -->
            string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            // add by mark 

            if (txtName.Contains("BIL_RETAIL_SIGNATURE") || txtName.Contains("BIL_TREATED_SIGNATURE"))
            {
                strTableName = strTableName.Substring(0, strTableName.IndexOf("!"));
            }

            int insertsuccess = 0;
            int insertfaile = 0;
            int updsuccess = 0;
            int updfaile = 0;


            bool db = true;

            int nLastColumns = ColumnName.Length;
            string sqldb = "select top 1 * from " + strTableName;
            DataTable qurrydb = ExportTotxt.QueryData(sqldb);
            if (qurrydb == null || qurrydb.Rows.Count < 1)
            {
                db = false;
            }
            else
            {
                nLastColumns = qurrydb.Columns.Count;
                if (qurrydb.Rows.Count < 1)
                    db = false;
            }

            for (int j = 0; j < nLastColumns && j < ColumnName.Length; j++)
            {
                if (j < nLastColumns - 1 && j < ColumnName.Length - 1)
                {
                    strCols += ColumnName[j] + ",";
                }
                if (j == nLastColumns - 1 || j == ColumnName.Length - 1)
                {
                    strCols += ColumnName[j];
                }
            }


            for (int i = 1; i < s.Length; i++)
            {
                line = s[i];
                string strUpd = "";
                strTableKeyVal = "";
                string[] Columnqty = s[i].Split('^');
                for (int j = 0; j < nLastColumns && j < Columnqty.Length; j++)
                {

                    if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "',";
                    }
                    if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "'";
                    }

                    for (int k = 0; k < ArrKey.Length; k++)
                    {
                        if (ColumnName[j] == ArrKey[k])
                        {

                            strTableKeyVal += " AND " + ArrKey[k] + "='" + Columnqty[j].Replace("'", "‘") + "'";
                        }
                    }


                }

                strCols = strCols.Replace(",STAMP", "");
                strCols = strCols.Replace(",REPORT_FILE", "");
                strVals = strVals.Replace(",'System.Byte[]'", "");
                strVals = strVals.Replace(",''", ",null");



                if (db == false)
                {
                    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                        + strVals + "";

                    //modify by mark -->
                    try
                    {
                        comm.CommandText = strSQL;
                        int num2 = int.Parse(comm.ExecuteNonQuery().ToString());
                        insertsuccess += num2;
                        strVals = "";
                    }
                    catch (Exception)
                    {
                        insertfaile++;
                    }
                    //modify by mark <-

                }
                else
                {



                    //modify by mark -->
                    string StrSqlDelete = "delete " + strTableName + "  where 1=1 " + strTableKeyVal;
                    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT " + strVals + "";
                    try
                    {
                        comm.CommandText = StrSqlDelete;
                        int num1 = int.Parse(comm.ExecuteNonQuery().ToString());
                        updsuccess += num1;

                        comm.CommandText = strSQL;
                        int num2 = int.Parse(comm.ExecuteNonQuery().ToString());
                        if (num1 <= 0)
                            insertsuccess += num2;

                        strVals = "";
                    }
                    catch (Exception)
                    {
                        insertfaile++;
                    }
                    //modify by mark <--

                    //StrSqlChk = "select * from " + strTableName + "  where 1=1 " + strTableKeyVal ;
                    //DataTable dtex = ExportTotxt.QueryData(StrSqlChk);
                    //if (dtex != null && dtex.Rows.Count > 0)
                    //{
                    //    string strSQL = " update " + strTableName + " set " + strUpd + "  where 1=1 " + strTableKeyVal;
                    //    int num = ExportTotxt.ExceSql_int(strSQL);
                    //    strVals = "";

                    //    if (num > 0)
                    //    {
                    //        updsuccess++;
                    //    }
                    //    else
                    //    {
                    //        updfaile++;
                    //    }

                    //}
                    //else
                    //{
                    //    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                    //        + strVals + "";

                    //    if (strTableKey.Length > 0)
                    //    {
                    //        strSQL += " where not exists(select 1 from " + strTableName + "  where   1=1 " + strTableKeyVal +")";
                    //    }
                    //    int num = ExportTotxt.ExceSql_int(strSQL);
                    //    strVals = "";

                    //    if (num > 0)
                    //    {
                    //        insertsuccess++;
                    //    }
                    //    else
                    //    {
                    //        insertfaile++;
                    //    }
                    //}
                }
            }
            conn.Close();
            //rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);
            MoveCurorLast();
            rTB.Text += string.Format("{0}資料更新成功 {1}條\n", strTableName, insertsuccess);


            DeleteUploadedData(strTableName);

        }

        protected void GetTableMember(string activeDir, string strTableName, string txtName)
        {
            string line = "";
            string strCols = "";
            string strVals = "";

            string strTableKey = GetTablePriKey(strTableName);
            string strTableKeyVal = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');

            int nTotalColumns = 0;


            int insertsuccess = 0;
            int insertfaile = 0;
            int updsuccess = 0;
            int updfaile = 0;




            nTotalColumns = ColumnName.Length;

            for (int j = 0; j < nTotalColumns && j < ColumnName.Length; j++)
            {
                if (j < nTotalColumns - 1 && j < ColumnName.Length - 1)
                {
                    strCols += ColumnName[j] + ",";
                }
                if (j == nTotalColumns - 1 || j == ColumnName.Length - 1)
                {
                    strCols += ColumnName[j];
                }
            }

            //add by mark -->

            string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            // add by mark 

            for (int i = 1; i < s.Length; i++)
            {
                line = s[i];
                string strUpd = "";
                string[] Columnqty = s[i].Split('^');
                for (int j = 0; j < nTotalColumns && j < Columnqty.Length; j++)
                {
                    if (j < nTotalColumns - 1 && j < Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "',";
                    }

                    if (j == nTotalColumns - 1 || j == Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "'";
                    }

                    if (ColumnName[j] == strTableKey)
                    {
                        strTableKeyVal = Columnqty[j].Replace("'", "‘");
                    }

                    if (strTableKey != ColumnName[j] && ColumnName[j] != "STAMP")
                    {
                        if (j < Columnqty.Length - 1 && j < nTotalColumns - 1)
                        {
                            strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "',";
                        }
                        if (j == Columnqty.Length - 1 || j == nTotalColumns - 1)
                        {
                            strUpd += ColumnName[j] + "='" + Columnqty[j].Replace("'", "‘") + "'";
                        }
                    }


                }

                strCols = strCols.Replace(",STAMP", "");
                strCols = strCols.Replace(",REPORT_FILE", "");
                strVals = strVals.Replace(",'System.Byte[]'", "");
                strVals = strVals.Replace(",''", ",null");

                string StrSqlChk = "";

                StrSqlChk = "select * from " + strTableName + "  where " + strTableKey + "='" + strTableKeyVal + "'";

                if (strVals.Contains(Company))
                {

                    //modify by mark -->
                    string StrSqlDelete = "delete " + strTableName + "  where " + strTableKey + "='" + strTableKeyVal + "'";
                    string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT " + strVals + "";
                    try
                    {

                        comm.CommandText = StrSqlDelete;
                        int num1 = int.Parse(comm.ExecuteNonQuery().ToString());
                        updsuccess += num1;


                        comm.CommandText = strSQL;
                        int num2 = int.Parse(comm.ExecuteNonQuery().ToString());
                        if (num1 <= 0)
                            insertsuccess += num2;

                        strVals = "";
                    }
                    catch (Exception)
                    {
                        insertfaile++;
                    }
                    //modify by mark <--

                }
                strVals = "";
            }
            conn.Close();
            MoveCurorLast();
            rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);

            rTB.Text += string.Format("{0}資料更新成功 {1}條\n", strTableName, updsuccess);


            DeleteUploadedData(strTableName);

        }

        protected void GetTableACC(string activeDir, string strTableName, string txtName)
        {
            string line = "";
            string strCols = "";
            string strVals = "";

            string strTableKey = GetTablePriKey(strTableName);
            string strTableKeyVal = "";
            string[] s = File.ReadAllLines(activeDir + txtName);

            string[] ColumnName = s[0].Split('^');

            int nLastColumns = ColumnName.Length;


            //add by mark -->

            string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;

            string strSQLDelete = "truncate table " + strTableName;
            try
            {
                comm.CommandText = strSQLDelete;
                comm.ExecuteNonQuery().ToString();

            }
            catch (Exception)
            {
                connStr = "";
            }

            // add by mark 
            string org = ExportTotxt.ExceSql(string.Format("SELECT ORG_ID FROM DOC_ORG WHERE CODE='{0}'", ShopCode));
            int orgId = 0;
            if (string.IsNullOrEmpty(org))
            {
                orgId = 0;
            }
            else
            {
                orgId = int.Parse(org);
            }

            int insertsuccess = 0;
            int insertfaile = 0;

            for (int j = 0; j < nLastColumns && j < ColumnName.Length; j++)
            {
                if (j < nLastColumns - 1 && j < ColumnName.Length - 1)
                {
                    strCols += ColumnName[j] + ",";
                }
                if (j == nLastColumns - 1 || j == ColumnName.Length - 1)
                {
                    strCols += ColumnName[j];
                }
            }

            for (int i = 1; i < s.Length; i++)
            {
                line = s[i];
                string[] Columnqty = s[i].Split('^');
                for (int j = 0; j < Columnqty.Length && j < nLastColumns; j++)
                {
                    if (j < nLastColumns - 1 && j < Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "',";
                    }
                    if (j == nLastColumns - 1 || j == Columnqty.Length - 1)
                    {
                        strVals += "'" + Columnqty[j].Replace("'", "‘") + "'";
                    }

                    if (ColumnName[j] == strTableKey)
                    {
                        strTableKeyVal = Columnqty[j].Replace("'", "‘");
                    }
                }

                strCols = strCols.Replace(",STAMP", "");
                strCols = strCols.Replace(",REPORT_FILE", "");
                strVals = strVals.Replace(",'System.Byte[]'", "");
                strVals = strVals.Replace(",''", ",null");




                string strSQL = "insert into " + strTableName + "(" + strCols + ") SELECT "
                    + strVals + "";
                try
                {
                    comm.CommandText = strSQL;
                    int num = int.Parse(comm.ExecuteNonQuery().ToString());
                    strVals = "";
                    if (num > 0)
                    {
                        insertsuccess++;
                    }
                    else
                    {
                        insertfaile++;
                    }
                }
                catch (Exception)
                {
                    insertfaile++;
                }

            }
            conn.Close();

            MoveCurorLast();
            rTB.Text += string.Format("{0}資料更新成功 {1}條\n", strTableName, insertsuccess);


            string strSql = "";

            if (strTableName == "ACC_MEMBER_TREATMENT")
                strSql = "delete ACC_MEMBER_TREATMENT where MEMBER_TREATMENT_ID=0";
            else
            {
                strSql = "";
            }
            if (strSql.Length > 0)
                ExportTotxt.ExceSql_int(strSql);


        }

        protected void GetDOC_MEMBER(string activeDir, string strTableName, string txtName)
        {

            string line = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');

            //add by mark -->
            string strtimestart = "";

            strtimestart = DateTime.Now.ToString();

            int nColMEMBER_ID = 0;
            int nColSHOP_ID = 1;
            int nColIS_GEN_CODE = 2;
            int nColMEMBER_TYPE_ID = 10;
            int nColIS_STAFF_MEMBER = 13;
            int nColIS_RECEIVE_EMAIL = 14;
            int nColMEMBER_STATUS_ID = 15;
            int nColIS_END_SERVICE = 16;
            int nColIS_ACTIVE = 17;
            int nColLAST_UPBY = 21;
            int nColREVISE = 22;
            int nColCREATE_BY = 19;

            //date col
            int nColJOIN_DATE = 6;
            int nColMEMBER_TYPE_VALID_DATE = 11;
            int nColMEMBER_TYPE_UPDATE_TIME = 12;
            int nColCREATE_DATE = 18;
            int nColLAST_UPDATE = 20;

            //string col
            int nColCODE = 3;
            int nColNAME = 4;
            int nColENAME = 5;

            for (int j = 0; j < ColumnName.Length; j++)
            {
                if (ColumnName[j].ToString().ToUpper() == "") { }

                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_ID") { nColMEMBER_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SHOP_ID") { nColSHOP_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_GEN_CODE") { nColIS_GEN_CODE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_TYPE_ID") { nColMEMBER_TYPE_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_STAFF_MEMBER") { nColIS_STAFF_MEMBER = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_RECEIVE_EMAIL") { nColIS_RECEIVE_EMAIL = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_STATUS_ID") { nColMEMBER_STATUS_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_END_SERVICE") { nColIS_END_SERVICE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_ACTIVE") { nColIS_ACTIVE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "LAST_UPBY") { nColLAST_UPBY = j; }
                else if (ColumnName[j].ToString().ToUpper() == "REVISE") { nColREVISE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "CREATE_BY") { nColCREATE_BY = j; }

                else if (ColumnName[j].ToString().ToUpper() == "JOIN_DATE") { nColJOIN_DATE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_TYPE_VALID_DATE") { nColMEMBER_TYPE_VALID_DATE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_TYPE_UPDATE_TIME") { nColMEMBER_TYPE_UPDATE_TIME = j; }
                else if (ColumnName[j].ToString().ToUpper() == "CREATE_DATE") { nColCREATE_DATE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "LAST_UPDATE") { nColLAST_UPDATE = j; }

                else if (ColumnName[j].ToString().ToUpper() == "CODE") { nColCODE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "NAME") { nColNAME = j; }
                else if (ColumnName[j].ToString().ToUpper() == "ENAME") { nColENAME = j; }
            }
            //add by mark <--


            int insertsuccess = 0;
            int nLastColumns = ColumnName.Length;

            DataTable dt = GetTableSchema(strTableName, s[0]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = strTableName;
                bulkCopy.BatchSize = dt.Rows.Count;
                bulkCopy.BulkCopyTimeout = 360000;
                conn.Open();


                for (int i = 1; i < s.Length; i++)
                {
                    line = s[i];
                    DataRow dr = dt.NewRow();

                    line = s[i];

                    string[] Columnqty = s[i].Split('^');
                    dr[ColumnName[0]] = Convert.ToInt32(Columnqty[0]);



                    if (Columnqty.Length == ColumnName.Length)
                    {
                        //add by mark -->
                        if (Columnqty[nColMEMBER_ID] != "") { dr["MEMBER_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_ID]); }
                        if (Columnqty[nColSHOP_ID] != "") { dr["SHOP_ID"] = Convert.ToInt32(Columnqty[nColSHOP_ID]); }
                        if (Columnqty[nColIS_GEN_CODE] != "") { dr["IS_GEN_CODE"] = Convert.ToInt32(Columnqty[nColIS_GEN_CODE]); }
                        if (Columnqty[nColMEMBER_TYPE_ID] != "") { dr["MEMBER_TYPE_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_TYPE_ID]); }
                        if (Columnqty[nColIS_STAFF_MEMBER] != "") { dr["IS_STAFF_MEMBER"] = Convert.ToInt32(Columnqty[nColIS_STAFF_MEMBER]); }
                        if (Columnqty[nColIS_RECEIVE_EMAIL] != "") { dr["IS_RECEIVE_EMAIL"] = Convert.ToInt32(Columnqty[nColIS_RECEIVE_EMAIL]); }
                        if (Columnqty[nColMEMBER_STATUS_ID] != "") { dr["MEMBER_STATUS_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_STATUS_ID]); }
                        if (Columnqty[nColIS_END_SERVICE] != "") { dr["IS_END_SERVICE"] = Convert.ToInt32(Columnqty[nColIS_END_SERVICE]); }
                        if (Columnqty[nColIS_ACTIVE] != "") { dr["IS_ACTIVE"] = Convert.ToInt32(Columnqty[nColIS_ACTIVE]); }
                        if (Columnqty[nColLAST_UPBY] != "") { dr["LAST_UPBY"] = Convert.ToInt32(Columnqty[nColLAST_UPBY]); }
                        if (Columnqty[nColREVISE] != "") { dr["REVISE"] = Convert.ToInt32(Columnqty[nColREVISE]); }
                        if (Columnqty[nColCREATE_BY] != "") { dr["CREATE_BY"] = Convert.ToInt32(Columnqty[nColCREATE_BY]); }

                        if (Columnqty[nColJOIN_DATE] != "") { dr["JOIN_DATE"] = Columnqty[nColJOIN_DATE].ToString(); }
                        if (Columnqty[nColMEMBER_TYPE_VALID_DATE] != "") { dr["MEMBER_TYPE_VALID_DATE"] = Columnqty[nColMEMBER_TYPE_VALID_DATE].ToString(); }
                        if (Columnqty[nColMEMBER_TYPE_UPDATE_TIME] != "") { dr["MEMBER_TYPE_UPDATE_TIME"] = Columnqty[nColMEMBER_TYPE_UPDATE_TIME].ToString(); }
                        if (Columnqty[nColCREATE_DATE] != "") { dr["CREATE_DATE"] = Columnqty[nColCREATE_DATE].ToString(); }

                        if (Columnqty[nColCODE] != "") { dr["CODE"] = Columnqty[nColCODE].ToString(); }
                        if (Columnqty[nColNAME] != "") { dr["NAME"] = Columnqty[nColNAME].ToString(); }
                        if (Columnqty[nColENAME] != "") { dr["ENAME"] = Columnqty[nColENAME].ToString(); }
                        //add by mark <--

                        insertsuccess++;
                        dt.Rows.Add(dr);
                    }
                }
                if (dt != null && dt.Rows.Count != 0)
                {
                    bulkCopy.WriteToServer(dt);
                }
                MoveCurorLast();
                rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);

            }
        }

        protected void GetDOC_Memberexpand(string activeDir, string strTableName, string txtName)
        {

            string line = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');

            //add by mark -->
            string strtimestart = "";
            string strtimeend = "";
            strtimestart = DateTime.Now.ToString();

            //int col
            int nColMEMBER_ID = 0;
            int nColMEMBER_SOURCE_ID = 3;
            int nColGENDER_ID = 9;
            int nColIS_RECEIVE_PHONE = 24;
            int nColIS_ISSUED_CARD = 25;
            int nColCOMPLAINTS_NUMBER = 30;


            //string col
            int nColID_NUMBER = 4;
            int nColPHONE = 13;
            int nColMOBILE = 14;
            int nColMEMBER_CODE = 32;

            for (int j = 0; j < ColumnName.Length; j++)
            {
                if (ColumnName[j].ToString().ToUpper() == "") { }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_ID") { nColMEMBER_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_SOURCE_ID") { nColMEMBER_SOURCE_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "GENDER_ID") { nColGENDER_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_RECEIVE_PHONE") { nColIS_RECEIVE_PHONE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_ISSUED_CARD") { nColIS_ISSUED_CARD = j; }
                else if (ColumnName[j].ToString().ToUpper() == "COMPLAINTS_NUMBER") { nColCOMPLAINTS_NUMBER = j; }
                else if (ColumnName[j].ToString().ToUpper() == "ID_NUMBER") { nColID_NUMBER = j; }
                else if (ColumnName[j].ToString().ToUpper() == "PHONE") { nColPHONE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MOBILE") { nColMOBILE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_CODE") { nColMEMBER_CODE = j; }
            }
            //add by mark <--

            int insertsuccess = 0;
            int nLastColumns = ColumnName.Length;

            DataTable dt = GetTableSchema(strTableName, s[0]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = strTableName;
                bulkCopy.BatchSize = dt.Rows.Count;
                bulkCopy.BulkCopyTimeout = 360000;
                conn.Open();


                for (int i = 1; i < s.Length; i++)
                {
                    line = s[i];
                    DataRow dr = dt.NewRow();

                    line = s[i];

                    string[] Columnqty = s[i].Split('^');

                    if (nLastColumns == Columnqty.Length)
                    {


                        if (Columnqty[nColMEMBER_ID] != "") { dr["MEMBER_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_ID]); }
                        if (Columnqty[nColMEMBER_SOURCE_ID] != "") { dr["MEMBER_SOURCE_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_SOURCE_ID]); }
                        if (Columnqty[nColGENDER_ID] != "") { dr["GENDER_ID"] = Convert.ToInt32(Columnqty[nColGENDER_ID]); }
                        if (Columnqty[nColIS_RECEIVE_PHONE] != "") { dr["IS_RECEIVE_PHONE"] = Convert.ToInt32(Columnqty[nColIS_RECEIVE_PHONE]); }
                        if (Columnqty[nColIS_ISSUED_CARD] != "") { dr["IS_ISSUED_CARD"] = Convert.ToInt32(Columnqty[nColIS_ISSUED_CARD]); }
                        if (Columnqty[nColCOMPLAINTS_NUMBER] != "") { dr["COMPLAINTS_NUMBER"] = Convert.ToInt32(Columnqty[nColCOMPLAINTS_NUMBER]); }

                        if (Columnqty[nColID_NUMBER] != "") { dr["ID_NUMBER"] = Columnqty[nColID_NUMBER].ToString(); }
                        if (Columnqty[nColPHONE] != "") { dr["PHONE"] = Columnqty[nColPHONE].ToString(); }
                        if (Columnqty[nColMOBILE] != "") { dr["MOBILE"] = Columnqty[nColMOBILE].ToString(); }
                        if (Columnqty[nColMEMBER_CODE] != "") { dr["MEMBER_CODE"] = Columnqty[nColMEMBER_CODE].ToString(); }

                        insertsuccess++;
                        dt.Rows.Add(dr);
                    }
                }
                if (dt != null && dt.Rows.Count != 0)
                {
                    bulkCopy.WriteToServer(dt);
                }
                MoveCurorLast();
                rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);

                // Console.WriteLine(string.Format("插入{0}条记录共花费{1}毫秒，{2}分钟", totalRow, sw.ElapsedMilliseconds, GetMinute(sw.ElapsedMilliseconds)));
            }
        }

        protected void GetAccTreatment(string activeDir, string strTableName, string txtName)
        {

            string line = "";
            string[] s = File.ReadAllLines(activeDir + txtName);
            string[] ColumnName = s[0].Split('^');


            //add by mark -->
            string strtimestart = "";
            string strtimeend = "";
            strtimestart = DateTime.Now.ToString();

            //int col
            int nColMEMBER_TREATMENT_ID = 0;
            int nColMEMBER_ID = 1;
            int nColSHOP_ID = 2;
            int nColGOODS_ID = 3;
            int nColCURR_ID = 4;
            int nColSELL_QTY = 5;
            int nColSELL_SUBTOTAL_AMT = 6;
            int nColSELL_NET_AMT = 7;
            int nColTOTAL_TIMES = 8;
            int nColAVG_PRICE = 9;
            int nColNET_AVG_PRICE = 10;
            int nColAVAILABLE = 11;
            int nColAVAILABLE_TIMES = 12;
            int nColAVAILABLE_AMT = 13;
            int nColAVAILABLE_NET_AMT = 14;
            int nColBAL_TIMES = 15;
            int nColBAL_AMT = 16;
            int nColBAL_NET_AMT = 17;
            int nColUSED_TIMES = 18;
            int nColUSED_AMT = 19;
            int nColUSED_NET_AMT = 20;
            int nColRETURNED_TIMES = 21;
            int nColRETURNED_AMT = 22;
            int nColRETURNED_NET_AMT = 23;
            int nColUPGRADED_TIMES = 24;
            int nColUPGRADED_AMT = 25;
            int nColUPGRADED_NET_AMT = 26;
            int nColTRAN_INCOME_TIMES = 27;
            int nColTRAN_INCOME_AMT = 28;
            int nColTRAN_INCOME_NET_AMT = 29;
            int nColSOURCE_BIL_TYPE_ID = 30;
            int nColSOURCE_BIL_ID = 31;
            int nColSOURCE_BIL_DTL_ID = 33;

            //date
            int nColSOURCE_BIL_DATE = 38;
            int nColVALID_DATE = 34;

            //string
            int nColTREATMENT_CARD_NO = 37;
            int nColIS_RTN_TREATMENT = 39;
            int nColCREDIT_METHOD = 40;
            int nColHOLD_TIMES = 41;
            int nColHOLD_AMT = 42;
            int nColHOLD_NET_AMT = 43;
            int nColVALUE_BY_INV = 44;
            int nColVALUE_BY_RTN = 45;
            int nColSOURCE_BIL_CODE = 32;
            int nColMEMBER_TREATMENT_CODE = 46;
            int nColMEMBER_CODE = 47;

            for (int j = 0; j < ColumnName.Length; j++)
            {
                if (ColumnName[j].ToString().ToUpper() == "") { }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_TREATMENT_ID") { nColMEMBER_TREATMENT_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_ID") { nColMEMBER_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SHOP_ID") { nColSHOP_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "GOODS_ID") { nColGOODS_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "CURR_ID") { nColCURR_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SELL_QTY") { nColSELL_QTY = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SELL_SUBTOTAL_AMT") { nColSELL_SUBTOTAL_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SELL_NET_AMT") { nColSELL_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "TOTAL_TIMES") { nColTOTAL_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "AVG_PRICE") { nColAVG_PRICE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "NET_AVG_PRICE") { nColNET_AVG_PRICE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "AVAILABLE") { nColAVAILABLE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "AVAILABLE_TIMES") { nColAVAILABLE_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "AVAILABLE_AMT") { nColAVAILABLE_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "AVAILABLE_NET_AMT") { nColAVAILABLE_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "BAL_TIMES") { nColBAL_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "BAL_AMT") { nColBAL_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "BAL_NET_AMT") { nColBAL_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "USED_TIMES") { nColUSED_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "USED_AMT") { nColUSED_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "USED_NET_AMT") { nColUSED_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "RETURNED_TIMES") { nColRETURNED_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "RETURNED_AMT") { nColRETURNED_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "RETURNED_NET_AMT") { nColRETURNED_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "UPGRADED_TIMES") { nColUPGRADED_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "UPGRADED_AMT") { nColUPGRADED_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "UPGRADED_NET_AMT") { nColUPGRADED_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "TRAN_INCOME_TIMES") { nColTRAN_INCOME_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "TRAN_INCOME_AMT") { nColTRAN_INCOME_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "TRAN_INCOME_NET_AMT") { nColTRAN_INCOME_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SOURCE_BIL_TYPE_ID") { nColSOURCE_BIL_TYPE_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SOURCE_BIL_ID") { nColSOURCE_BIL_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SOURCE_BIL_DTL_ID") { nColSOURCE_BIL_DTL_ID = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SOURCE_BIL_DATE") { nColSOURCE_BIL_DATE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "VALID_DATE") { nColVALID_DATE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "TREATMENT_CARD_NO") { nColTREATMENT_CARD_NO = j; }
                else if (ColumnName[j].ToString().ToUpper() == "IS_RTN_TREATMENT") { nColIS_RTN_TREATMENT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "CREDIT_METHOD") { nColCREDIT_METHOD = j; }
                else if (ColumnName[j].ToString().ToUpper() == "HOLD_TIMES") { nColHOLD_TIMES = j; }
                else if (ColumnName[j].ToString().ToUpper() == "HOLD_AMT") { nColHOLD_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "HOLD_NET_AMT") { nColHOLD_NET_AMT = j; }
                else if (ColumnName[j].ToString().ToUpper() == "VALUE_BY_INV") { nColVALUE_BY_INV = j; }
                else if (ColumnName[j].ToString().ToUpper() == "VALUE_BY_RTN") { nColVALUE_BY_RTN = j; }
                else if (ColumnName[j].ToString().ToUpper() == "SOURCE_BIL_CODE") { nColSOURCE_BIL_CODE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_TREATMENT_CODE") { nColMEMBER_TREATMENT_CODE = j; }
                else if (ColumnName[j].ToString().ToUpper() == "MEMBER_CODE") { nColMEMBER_CODE = j; }

            }
            //add by mark <--




            int insertsuccess = 0;
            int nLastColumns = ColumnName.Length;

            DataTable dt = GetTableSchema(strTableName, s[0]);

            int nmod = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = strTableName;
                bulkCopy.BatchSize = dt.Rows.Count;
                bulkCopy.BulkCopyTimeout = 360000;
                conn.Open();


                for (int i = 1; i < s.Length; i++)
                {
                    line = s[i];
                    DataRow dr = dt.NewRow();

                    line = s[i];

                    string[] Columnqty = s[i].Split('^');

                    if (nLastColumns == Columnqty.Length)
                    {

                        if (Columnqty[nColMEMBER_TREATMENT_ID] != "") { dr["MEMBER_TREATMENT_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_TREATMENT_ID]); }
                        if (Columnqty[nColMEMBER_ID] != "") { dr["MEMBER_ID"] = Convert.ToInt32(Columnqty[nColMEMBER_ID]); }
                        if (Columnqty[nColSHOP_ID] != "") { dr["SHOP_ID"] = Convert.ToInt32(Columnqty[nColSHOP_ID]); }
                        if (Columnqty[nColGOODS_ID] != "") { dr["GOODS_ID"] = Convert.ToInt32(Columnqty[nColGOODS_ID]); }
                        if (Columnqty[nColCURR_ID] != "") { dr["CURR_ID"] = Convert.ToInt32(Columnqty[nColCURR_ID]); }
                        if (Columnqty[nColSELL_QTY] != "") { dr["SELL_QTY"] = Convert.ToInt32(Columnqty[nColSELL_QTY]); }
                        if (Columnqty[nColSELL_SUBTOTAL_AMT] != "") { dr["SELL_SUBTOTAL_AMT"] = Convert.ToDecimal(Columnqty[nColSELL_SUBTOTAL_AMT]); }
                        if (Columnqty[nColSELL_NET_AMT] != "") { dr["SELL_NET_AMT"] = Convert.ToDecimal(Columnqty[nColSELL_NET_AMT]); }
                        if (Columnqty[nColTOTAL_TIMES] != "") { dr["TOTAL_TIMES"] = Convert.ToInt32(Columnqty[nColTOTAL_TIMES]); }
                        if (Columnqty[nColAVG_PRICE] != "") { dr["AVG_PRICE"] = Convert.ToDecimal(Columnqty[nColAVG_PRICE]); }
                        if (Columnqty[nColNET_AVG_PRICE] != "") { dr["NET_AVG_PRICE"] = Convert.ToDecimal(Columnqty[nColNET_AVG_PRICE]); }
                        if (Columnqty[nColAVAILABLE] != "") { dr["AVAILABLE"] = Convert.ToDecimal(Columnqty[nColAVAILABLE]); }
                        if (Columnqty[nColAVAILABLE_TIMES] != "") { dr["AVAILABLE_TIMES"] = Convert.ToInt32(Columnqty[nColAVAILABLE_TIMES]); }
                        if (Columnqty[nColAVAILABLE_AMT] != "") { dr["AVAILABLE_AMT"] = Convert.ToDecimal(Columnqty[nColAVAILABLE_AMT]); }
                        if (Columnqty[nColAVAILABLE_NET_AMT] != "") { dr["AVAILABLE_NET_AMT"] = Convert.ToDecimal(Columnqty[nColAVAILABLE_NET_AMT]); }
                        if (Columnqty[nColBAL_TIMES] != "") { dr["BAL_TIMES"] = Convert.ToInt32(Columnqty[nColBAL_TIMES]); }
                        if (Columnqty[nColBAL_AMT] != "") { dr["BAL_AMT"] = Convert.ToDecimal(Columnqty[nColBAL_AMT]); }
                        if (Columnqty[nColBAL_NET_AMT] != "") { dr["BAL_NET_AMT"] = Convert.ToDecimal(Columnqty[nColBAL_NET_AMT]); }
                        if (Columnqty[nColUSED_TIMES] != "") { dr["USED_TIMES"] = Convert.ToInt32(Columnqty[nColUSED_TIMES]); }
                        if (Columnqty[nColUSED_AMT] != "") { dr["USED_AMT"] = Convert.ToDecimal(Columnqty[nColUSED_AMT]); }
                        if (Columnqty[nColUSED_NET_AMT] != "") { dr["USED_NET_AMT"] = Convert.ToDecimal(Columnqty[nColUSED_NET_AMT]); }
                        if (Columnqty[nColRETURNED_TIMES] != "") { dr["RETURNED_TIMES"] = Convert.ToInt32(Columnqty[nColRETURNED_TIMES]); }
                        if (Columnqty[nColRETURNED_AMT] != "") { dr["RETURNED_AMT"] = Convert.ToDecimal(Columnqty[nColRETURNED_AMT]); }
                        if (Columnqty[nColRETURNED_NET_AMT] != "") { dr["RETURNED_NET_AMT"] = Convert.ToDecimal(Columnqty[nColRETURNED_NET_AMT]); }
                        if (Columnqty[nColUPGRADED_TIMES] != "") { dr["UPGRADED_TIMES"] = Convert.ToInt32(Columnqty[nColUPGRADED_TIMES]); }
                        if (Columnqty[nColUPGRADED_AMT] != "") { dr["UPGRADED_AMT"] = Convert.ToDecimal(Columnqty[nColUPGRADED_AMT]); }
                        if (Columnqty[nColUPGRADED_NET_AMT] != "") { dr["UPGRADED_NET_AMT"] = Convert.ToDecimal(Columnqty[nColUPGRADED_NET_AMT]); }
                        if (Columnqty[nColTRAN_INCOME_TIMES] != "") { dr["TRAN_INCOME_TIMES"] = Convert.ToInt32(Columnqty[nColTRAN_INCOME_TIMES]); }
                        if (Columnqty[nColTRAN_INCOME_AMT] != "") { dr["TRAN_INCOME_AMT"] = Convert.ToDecimal(Columnqty[nColTRAN_INCOME_AMT]); }
                        if (Columnqty[nColTRAN_INCOME_NET_AMT] != "") { dr["TRAN_INCOME_NET_AMT"] = Convert.ToDecimal(Columnqty[nColTRAN_INCOME_NET_AMT]); }
                        if (Columnqty[nColSOURCE_BIL_TYPE_ID] != "") { dr["SOURCE_BIL_TYPE_ID"] = Convert.ToInt32(Columnqty[nColSOURCE_BIL_TYPE_ID]); }
                        if (Columnqty[nColSOURCE_BIL_ID] != "") { dr["SOURCE_BIL_ID"] = Convert.ToInt32(Columnqty[nColSOURCE_BIL_ID]); }
                        if (Columnqty[nColSOURCE_BIL_DTL_ID] != "") { dr["SOURCE_BIL_DTL_ID"] = Convert.ToInt32(Columnqty[nColSOURCE_BIL_DTL_ID]); }

                        if (Columnqty[nColSOURCE_BIL_DATE] != "") { dr["SOURCE_BIL_DATE"] = Columnqty[nColSOURCE_BIL_DATE].ToString(); }
                        if (Columnqty[nColVALID_DATE] != "") { dr["VALID_DATE"] = Columnqty[nColVALID_DATE].ToString(); }

                        if (Columnqty[nColTREATMENT_CARD_NO] != "") { dr["TREATMENT_CARD_NO"] = Columnqty[nColTREATMENT_CARD_NO].ToString(); }
                        if (Columnqty[nColIS_RTN_TREATMENT] != "") { dr["IS_RTN_TREATMENT"] = Columnqty[nColIS_RTN_TREATMENT].ToString(); }
                        if (Columnqty[nColCREDIT_METHOD] != "") { dr["CREDIT_METHOD"] = Columnqty[nColCREDIT_METHOD].ToString(); }
                        if (Columnqty[nColHOLD_TIMES] != "") { dr["HOLD_TIMES"] = Columnqty[nColHOLD_TIMES].ToString(); }
                        if (Columnqty[nColHOLD_AMT] != "") { dr["HOLD_AMT"] = Columnqty[nColHOLD_AMT].ToString(); }
                        if (Columnqty[nColHOLD_NET_AMT] != "") { dr["HOLD_NET_AMT"] = Columnqty[nColHOLD_NET_AMT].ToString(); }
                        if (Columnqty[nColVALUE_BY_INV] != "") { dr["VALUE_BY_INV"] = Columnqty[nColVALUE_BY_INV].ToString(); }
                        if (Columnqty[nColVALUE_BY_RTN] != "") { dr["VALUE_BY_RTN"] = Columnqty[nColVALUE_BY_RTN].ToString(); }
                        if (Columnqty[nColSOURCE_BIL_CODE] != "") { dr["SOURCE_BIL_CODE"] = Columnqty[nColSOURCE_BIL_CODE].ToString(); }



                        insertsuccess++;
                        nmod++;
                        dt.Rows.Add(dr);

                        if (nmod == 100000)
                        {
                            bulkCopy.WriteToServer(dt);
                            nmod = 0;
                            dt.Rows.Clear();
                        }
                    }
                }
                if (dt != null && dt.Rows.Count != 0)
                {
                    bulkCopy.WriteToServer(dt);
                }
                MoveCurorLast();
                rTB.Text += string.Format("{0}資料插入成功 {1}條\n", strTableName, insertsuccess);

                // Console.WriteLine(string.Format("插入{0}条记录共花费{1}毫秒，{2}分钟", totalRow, sw.ElapsedMilliseconds, GetMinute(sw.ElapsedMilliseconds)));
            }
        }

        static DataTable GetTableSchema(string strtablename, string strcolus)
        {
            DataTable dt = new DataTable();
            if (strtablename == "DOC_ORG")
            {
                dt.Columns.AddRange(new DataColumn[] {   
                new DataColumn("ORG_ID",typeof(Int32)),  
                new DataColumn("PARENT_ORG_ID",typeof(Int32)),  
                new DataColumn("ORG_TYPE_ID",typeof(Int32)),  
                new DataColumn("CODE",typeof(string)),  
                new DataColumn("NAME",typeof(string))});
            }
            else if (strtablename == "DOC_MEMBER")
            {
                dt.Columns.AddRange(new DataColumn[] {   
                 new DataColumn("MEMBER_ID",typeof(Int32)),  
                 new DataColumn("SHOP_ID",typeof(Int32)),
                 new DataColumn("IS_GEN_CODE",typeof(Int32)),
                 new DataColumn("CODE",typeof(String)),
                 new DataColumn("NAME",typeof(String)),
                 new DataColumn("ENAME",typeof(String)),
                 new DataColumn("JOIN_DATE",typeof(DateTime)),
                 new DataColumn("MEMBER_CARD",typeof(String)),
                 new DataColumn("PASSWORD",typeof(String)),
                 new DataColumn("MAGNETIC_CODE",typeof(String)),
                 new DataColumn("MEMBER_TYPE_ID",typeof(Int32)),
                 new DataColumn("MEMBER_TYPE_VALID_DATE",typeof(DateTime)),
                 new DataColumn("MEMBER_TYPE_UPDATE_TIME",typeof(DateTime)),
                 new DataColumn("IS_STAFF_MEMBER",typeof(Int32)),
                 new DataColumn("IS_RECEIVE_EMAIL",typeof(Int32)),
                 new DataColumn("MEMBER_STATUS_ID",typeof(Int32)),
                 new DataColumn("IS_END_SERVICE",typeof(Int32)),
                 new DataColumn("IS_ACTIVE",typeof(Int32)),
                 new DataColumn("CREATE_DATE",typeof(DateTime)),
                 new DataColumn("CREATE_BY",typeof(Int32)),
                 new DataColumn("LAST_UPDATE",typeof(DateTime)),
                 new DataColumn("LAST_UPBY",typeof(Int32)),
                 new DataColumn("REVISE",typeof(Int32)),
                 new DataColumn("GUID",typeof(String)),
                 new DataColumn("STAMP",typeof(String)),
                 new DataColumn("STAFF_ID",typeof(Int32)),
                 new DataColumn("SIMPLE_ADDRESS_ID",typeof(Int32)),
                 new DataColumn("SIMPLE_ADDRESS_CONTENT",typeof(String)),
                 new DataColumn("CAN_RENEW_EXPIRED",typeof(Int32)),
                 new DataColumn("RENEW_SELL_DATE",typeof(DateTime))});
                //new DataColumn("SHOP_UPD_ON",typeof(DateTime)),
                //new DataColumn("SHOP_UPD_BY",typeof(Int32)),
                //new DataColumn("HQ_UPD_ON",typeof(DateTime)),
                //new DataColumn("HQ_UPD_BY",typeof(Int32)),
                //new DataColumn("ISUPLOADED",typeof(Int32))});

            }
            else if (strtablename == "DOC_MEMBER_EXPAND")
            {
                dt.Columns.AddRange(new DataColumn[] {   
                 new DataColumn("MEMBER_ID",typeof(Int32)),  
                 new DataColumn("INTRO_STAFF_ID",typeof(Int32)),
                 new DataColumn("INTRO_MEMBER_ID",typeof(Int32)),
                 new DataColumn("MEMBER_SOURCE_ID",typeof(Int32)),
                 new DataColumn("ID_NUMBER",typeof(String)),
                 new DataColumn("BIRTHDAY_YEAR",typeof(Int32)),
                 new DataColumn("BIRTHDAY_MONTH",typeof(Int32)),
                 new DataColumn("BIRTHDAY_DAY",typeof(Int32)),
                 new DataColumn("AGE_LEVEL_ID",typeof(Int32)),
                 new DataColumn("GENDER_ID",typeof(Int32)),
                 new DataColumn("MARITAL_STATUS_ID",typeof(Int32)),
                 new DataColumn("PROFESSION_ID",typeof(Int32)),
                 new DataColumn("HEIGHT",typeof(String)),
                 new DataColumn("PHONE",typeof(String)),
                 new DataColumn("MOBILE",typeof(String)),
                 new DataColumn("EMAIL",typeof(String)),
                 new DataColumn("MSN",typeof(String)),
                 new DataColumn("QQ",typeof(String)),
                 new DataColumn("END_SERVICE_REASON_ID",typeof(Int32)),
                 new DataColumn("END_SERVICE_DATE",typeof(DateTime)),
                 new DataColumn("REMARK",typeof(String)),
                 new DataColumn("INTERNAL_REMARK",typeof(String)),
                 new DataColumn("GUID",typeof(String)),
                 new DataColumn("STAMP",typeof(String)),
                 new DataColumn("IS_RECEIVE_PHONE",typeof(Int32)),
                 new DataColumn("IS_ISSUED_CARD",typeof(Int32)),
                 new DataColumn("ISSUE_DATE",typeof(DateTime)),
                 new DataColumn("GETOUT_DATE",typeof(DateTime)),
                 new DataColumn("GETOUT_SHOP_ID",typeof(Int32)),
                 new DataColumn("END_REASON_DESCRIPTION",typeof(String)),
                 new DataColumn("COMPLAINTS_NUMBER",typeof(Int32)),
                 new DataColumn("INTERNAL_REF_ID",typeof(Int32)),
                 new DataColumn("MEMBER_CODE",typeof(String))});
            }
            else if (strtablename == "ACC_MEMBER_TREATMENT")
            {
                dt.Columns.AddRange(new DataColumn[] {   
                new DataColumn("MEMBER_TREATMENT_ID",typeof(Int32)),
                new DataColumn("MEMBER_ID",typeof(Int32)),
                new DataColumn("SHOP_ID",typeof(Int32)),
                new DataColumn("GOODS_ID",typeof(Int32)),
                new DataColumn("CURR_ID",typeof(Int32)),
                new DataColumn("SELL_QTY",typeof(Int32)),
                new DataColumn("SELL_SUBTOTAL_AMT",typeof(Decimal)),
                new DataColumn("SELL_NET_AMT",typeof(Decimal)),
                new DataColumn("TOTAL_TIMES",typeof(Int32)),
                new DataColumn("AVG_PRICE",typeof(Decimal)),
                new DataColumn("NET_AVG_PRICE",typeof(Decimal)),
                new DataColumn("AVAILABLE",typeof(Decimal)),
                new DataColumn("AVAILABLE_TIMES",typeof(Int32)),
                new DataColumn("AVAILABLE_AMT",typeof(Decimal)),
                new DataColumn("AVAILABLE_NET_AMT",typeof(Decimal)),
                new DataColumn("BAL_TIMES",typeof(Int32)),
                new DataColumn("BAL_AMT",typeof(Decimal)),
                new DataColumn("BAL_NET_AMT",typeof(Decimal)),
                new DataColumn("USED_TIMES",typeof(Int32)),
                new DataColumn("USED_AMT",typeof(Decimal)),
                new DataColumn("USED_NET_AMT",typeof(Decimal)),
                new DataColumn("RETURNED_TIMES",typeof(Int32)),
                new DataColumn("RETURNED_AMT",typeof(Decimal)),
                new DataColumn("RETURNED_NET_AMT",typeof(Decimal)),
                new DataColumn("UPGRADED_TIMES",typeof(Int32)),
                new DataColumn("UPGRADED_AMT",typeof(Decimal)),
                new DataColumn("UPGRADED_NET_AMT",typeof(Decimal)),
                new DataColumn("TRAN_INCOME_TIMES",typeof(Int32)),
                new DataColumn("TRAN_INCOME_AMT",typeof(Decimal)),
                new DataColumn("TRAN_INCOME_NET_AMT",typeof(Decimal)),
                new DataColumn("SOURCE_BIL_TYPE_ID",typeof(Int32)),
                new DataColumn("SOURCE_BIL_ID",typeof(Int32)),
                new DataColumn("SOURCE_BIL_CODE",typeof(String)),
                new DataColumn("SOURCE_BIL_DTL_ID",typeof(Int32)),
                new DataColumn("VALID_DATE",typeof(DateTime)),
                new DataColumn("GUID",typeof(String)),
                new DataColumn("STAMP",typeof(String)),
                new DataColumn("TREATMENT_CARD_NO",typeof(Int32)),
                new DataColumn("SOURCE_BIL_DATE",typeof(DateTime)),
                new DataColumn("IS_RTN_TREATMENT",typeof(Int32)),
                new DataColumn("CREDIT_METHOD",typeof(Int32)),
                new DataColumn("HOLD_TIMES",typeof(Int32)),
                new DataColumn("HOLD_AMT",typeof(Decimal)),
                new DataColumn("HOLD_NET_AMT",typeof(Decimal)),
                new DataColumn("VALUE_BY_INV",typeof(Decimal)),
                new DataColumn("VALUE_BY_RTN",typeof(Decimal)),
                new DataColumn("MEMBER_CODE",typeof(String)),
                new DataColumn("MEMBER_TREATMENT_CODE",typeof(String)),});

            }
            return dt;
        }

        private void SqlBulkCopyByDatatable(string connectionString, string TableName, DataTable dt)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                {

                    try
                    {
                        sqlbulkcopy.DestinationTableName = TableName;
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        sqlbulkcopy.WriteToServer(dt);
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                return;
            }
        }

        private void rbn_manual_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_manual.Checked)
            {
                btn_upload.Show();
                btn_Dowm.Show();
            }
        }

        private void rbn_auto_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            string[] arrHour = ExeHour.Split(',');


            if (arrHour.Length < 1)
            {
                return;
            }

            string strNowhour = DateTime.Now.Hour.ToString();
            for (int j = 0; j < arrHour.Length; j++)
            {
                string strhour = arrHour[j].ToString();
                if (strNowhour == strhour & lastExechout != strhour)
                {
                    lastExechout = strhour;
                    RunDTS();

                }
            }
        }


        private void RunDTS()
        {
            timer1.Stop();
            txt = "";
            this.btn_upload.PerformClick();
            this.btn_Dowm.PerformClick();
            timer1.Start();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            this.Text = Describe + " 【" + ShopCode + "】 " + Version;

            string startup = Application.ExecutablePath;
            int pp = startup.LastIndexOf("\\");
            startup = startup.Substring(0, pp);
            string icon = startup + "\\sale.ico";
            notifyIcon1.Icon = new Icon(icon);

            string path = Application.StartupPath;
            SettingHel.SetAutoRun(path + @"\DST CLIENT.exe", true);


            if (ExeFirst == "Y")
            {
                this.btn_Dowm.PerformClick();
                this.btn_upload.PerformClick();
            }
        }

        private void Client_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(60000);
            rTB.Text += txt;
        }
        
    }
}
