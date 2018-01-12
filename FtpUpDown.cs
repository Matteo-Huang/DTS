using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class FtpUpDown
{

    string ftpServerIP;

    string ftpUserID;

    string ftpPassword;

    FtpWebRequest reqFTP;

    private void Connect(String path)//連接ftp
    {

        // 根據uri創建FtpWebRequest對象
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
        // 指定數據傳輸類型
        reqFTP.UseBinary = true;
        // ftp用戶名和密碼
        reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
    }

    public FtpUpDown(string ftpServerIP, string ftpUserID, string ftpPassword)
    {
        this.ftpServerIP = ftpServerIP;
        this.ftpUserID = ftpUserID;
        this.ftpPassword = ftpPassword;
    }

    private string[] GetFileList(string path, string WRMethods)//上面的代碼示例了如何從ftp服務器上獲得文件列表
    {
        string[] downloadFiles;
        StringBuilder result = new StringBuilder();
        try
        {
            Connect(path);
            reqFTP.Method = WRMethods;
            WebResponse response = reqFTP.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);//中文文件名
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Append(line);
                result.Append("\n");
                line = reader.ReadLine();
            }

            // to remove the trailing '\n' 
            result.Remove(result.ToString().LastIndexOf('\n'), 1);
            reader.Close();
            response.Close();
            return result.ToString().Split('\n');

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            downloadFiles = null;
            return downloadFiles;
        }
    }
    public string[] GetFileList(string path)//ftp服務器上獲得文件列表
    {

        return GetFileList("ftp://" + ftpServerIP + "/" + path, WebRequestMethods.Ftp.ListDirectory);
    }

    public string[] GetTxt(string path)//
    {
        StreamReader sr = new StreamReader(path);
        string st = string.Empty;
        while (!sr.EndOfStream)
        {
            st = sr.ReadLine();
        }

        string[] sArray = st.Split(' ');
        return sArray;
    }


    public string[] GetFileList()//ftp服務器上獲得文件列表
    {

        return GetFileList("ftp://" + ftpServerIP + "/", WebRequestMethods.Ftp.ListDirectory);
    }

    public bool Upload(string filename, string ShopCode) //ftp服務器上載文件的功能
    {
        bool falg = true;
        FileInfo fileInf = new FileInfo(filename);
        string uri = "ftp://" + ftpServerIP + "/SHOP/" + ShopCode + "/UPLOAD/" + fileInf.Name;
        Connect(uri);
        // 在一個命令之後被執行
        reqFTP.KeepAlive = false;
        // 指定執行什麽命令
        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
        // 上傳文件時通知服務器文件的大小
        reqFTP.ContentLength = fileInf.Length;
        // 緩衝大小設置為kb 
        int buffLength = 204800;
        byte[] buff = new byte[buffLength];
        int contentLen;
        // 打開一個文件流(System.IO.FileStream) 去讀上傳的文件
        FileStream fs = fileInf.OpenRead();
        try
        {
            // 把上傳的文件寫入流
            Stream strm = reqFTP.GetRequestStream();
            // 每次讀文件流的kb 
            contentLen = fs.Read(buff, 0, buffLength);
            // 流內容沒有結束
            while (contentLen != 0)
            {
                // 把內容從file stream 寫入upload stream 
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            // 關閉兩個流
            strm.Close();
            fs.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Upload Error");
            falg = false;
            return falg;
        }
        return falg;
    }

    public bool UploadTxt(string filename, string ShopCode) //ftp服務器上載文件的功能
    {
        bool falg = true;
        FileInfo fileInf = new FileInfo(filename);
        string uri = "ftp://" + ftpServerIP + "/SHOP/UpSendList/" + fileInf.Name;
        Connect(uri);//連接          
        // 在一個命令之後被執行
        reqFTP.KeepAlive = false;
        // 指定執行什麽命令
        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
        // 上傳文件時通知服務器文件的大小
        reqFTP.ContentLength = fileInf.Length;
        // 緩衝大小設置為kb 
        int buffLength = 204800;
        byte[] buff = new byte[buffLength];
        int contentLen;
        // 打開一個文件流(System.IO.FileStream) 去讀上傳的文件
        FileStream fs = fileInf.OpenRead();
        try
        {
            // 把上傳的文件寫入流
            Stream strm = reqFTP.GetRequestStream();
            // 每次讀文件流的kb 
            contentLen = fs.Read(buff, 0, buffLength);
            // 流內容沒有結束
            while (contentLen != 0)
            {
                // 把內容從file stream 寫入upload stream 
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            // 關閉兩個流
            strm.Close();
            fs.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Upload Error");
            falg = false;
            return falg;
        }
        return falg;
    }

    private FtpWebRequest GetRequest(string URI, string username, string password)
    {
        //根據服務器信息FtpWebRequest創建類的對象
        FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
        //提供身份驗證信息
        result.Credentials = new System.Net.NetworkCredential(username, password);
        //設置請求完成之後是否保持到FTP服務器的控制連接，默認值為true
        result.KeepAlive = false;
        return result;
    }

    public void UploadFile3(string filename,

        string ShopCode)
    {

        string hostname = this.ftpServerIP;
        string username = this.ftpUserID;
        string password = this.ftpPassword;

        //1. check target
        string target;

        target = Guid.NewGuid().ToString();  //使用臨時文件名

        FileInfo fileInf = new FileInfo(filename);
        string URI = "FTP://" + ftpServerIP + "/" + ShopCode + "/UPLOAD/" + fileInf.Name;
        ///WebClient webcl = new WebClient();
        System.Net.FtpWebRequest ftp = GetRequest(URI, username, password);

        //設置FTP命令 設置所要執行的FTP命令，
        //ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails;//假設此處為顯示指定路徑下的文件列表
        ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
        //指定文件傳輸的數據類型
        ftp.UseBinary = true;
        ftp.UsePassive = true;

        //告訴ftp文件大小
        ftp.ContentLength = fileInf.Length;
        //緩衝大小設置為2KB
        const int BufferSize = 2048;
        byte[] content = new byte[BufferSize - 1 + 1];
        int dataRead;

        //打開一個文件流 (System.IO.FileStream) 去讀上傳的文件
        using (FileStream fs = fileInf.OpenRead())
        {
            try
            {
                //把上傳的文件寫入流
                using (Stream rs = ftp.GetRequestStream())
                {
                    do
                    {
                        //每次讀文件流的2KB
                        dataRead = fs.Read(content, 0, BufferSize);
                        rs.Write(content, 0, dataRead);
                    } while (dataRead > 0);
                    rs.Close();
                }

            }
            catch (Exception ex) { }
            finally
            {
                fs.Close();
            }

        }

        ftp = null;
        //設置FTP命令
        ftp = GetRequest(URI, username, password);
        ftp.Method = System.Net.WebRequestMethods.Ftp.Rename; //改名
        ftp.RenameTo = fileInf.Name;
        try
        {
            ftp.GetResponse();
        }
        catch (Exception ex)
        {
            ftp = GetRequest(URI, username, password);
            ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile; //刪除
            ftp.GetResponse();
            throw ex;
        }
        finally
        {
            //fileinfo.Delete();
        }

        // 可以記錄一個日志  "上傳" + fileinfo.FullName + "上傳到" + "FTP://" + hostname + "/" + targetDir + "/" + fileinfo.Name + "成功." );
        ftp = null;

        #region
        /*****
             *FtpWebResponse
             * ****/
        //FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();
        #endregion
    }


    //public void Upload2(string filename, string ShopCode) //ftp服務器上載文件的功能
    //{

    //    FileInfo fileInf = new FileInfo(filename);
    //    string uri = "ftp://" + ftpServerIP + "/" + ShopCode + "/DOWNLOAD/" + fileInf.Name;
    //    Connect(uri);//連接          
    //    // 在一個命令之後被執行
    //    reqFTP.KeepAlive = false;
    //    // 指定執行什麽命令
    //    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
    //    // 上傳文件時通知服務器文件的大小
    //    reqFTP.ContentLength = fileInf.Length;
    //    // 緩衝大小設置為kb 
    //    int buffLength = 204800;
    //    byte[] buff = new byte[buffLength];
    //    int contentLen;
    //    // 打開一個文件流(System.IO.FileStream) 去讀上傳的文件
    //    FileStream fs = fileInf.OpenRead();
    //    try
    //    {
    //        // 把上傳的文件寫入流
    //        Stream strm = reqFTP.GetRequestStream();
    //        // 每次讀文件流的kb 
    //        contentLen = fs.Read(buff, 0, buffLength);
    //        // 流內容沒有結束
    //        while (contentLen != 0)
    //        {
    //            // 把內容從file stream 寫入upload stream 
    //            strm.Write(buff, 0, contentLen);
    //            contentLen = fs.Read(buff, 0, buffLength);
    //        }
    //        // 關閉兩個流
    //        strm.Close();
    //        fs.Close();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message, "Upload Error");
    //    }

    //}

    public bool Download(string filePath, string fileName, string ShopCode, out string errorinfo)
    {
        try
        {
            String onlyFileName = Path.GetFileName(fileName);
            string newFileName = filePath + "/" + onlyFileName;
            //if (File.Exists(newFileName))
            //{
            //    errorinfo = string.Format("本地文件{0}已存在,無法下載", newFileName);
            //    return false;
            //}
            string url = "ftp://" + ftpServerIP + "/SHOP/" + ShopCode + "/DOWNLOAD/" + fileName;
            Connect(url);//連接  
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

            Stream ftpStream = response.GetResponseStream();
            long cl = response.ContentLength;
            int bufferSize = 20480000;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            FileStream outputStream = new FileStream(newFileName, FileMode.Create);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);

            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            errorinfo = "";
            return true;
        }
        catch (Exception ex)
        {
            errorinfo = string.Format("因{0},無法下載", ex.Message);
            return false;
        }
    }

    public bool DownloadC(string filePath, string fileName, out string errorinfo)
    {
        try
        {
            String onlyFileName = Path.GetFileName(fileName);
            string newFileName = filePath + "/" + onlyFileName;
            //if (File.Exists(newFileName))
            //{
            //    errorinfo = string.Format("本地文件{0}已存在,無法下載", newFileName);
            //    return false;
            //}
            string url = "ftp://" + ftpServerIP + "/COMPANY/DOWNLOAD/" + fileName;
            Connect(url);//連接  
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

            Stream ftpStream = response.GetResponseStream();
            long cl = response.ContentLength;
            int bufferSize = 20480000;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            FileStream outputStream = new FileStream(newFileName, FileMode.Create);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);

            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            errorinfo = "";
            return true;
        }
        catch (Exception ex)
        {
            errorinfo = string.Format("因{0},無法下載", ex.Message);
            return false;
        }
    }

    public bool DownloadP(string filePath, string fileName, out string errorinfo)
    {
        try
        {
            String onlyFileName = Path.GetFileName(fileName);
            string newFileName = filePath + "/" + onlyFileName;
            //if (File.Exists(newFileName))
            //{
            //    errorinfo = string.Format("本地文件{0}已存在,無法下載", newFileName);
            //    return false;
            //}
            string url = "ftp://" + ftpServerIP + "/PUBLIC/DOWNLOAD/" + fileName;
            Connect(url);//連接  
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

            Stream ftpStream = response.GetResponseStream();
            long cl = response.ContentLength;
            int bufferSize = 20480000;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            FileStream outputStream = new FileStream(newFileName, FileMode.Create);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);

            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            errorinfo = "";
            return true;
        }
        catch (Exception ex)
        {
            errorinfo = string.Format("因{0},無法下載", ex.Message);
            return false;
        }
    }

    public bool Download_recieve(string filePath, string fileName, string ShopCode, out string errorinfo)
    {
        try
        {
            String onlyFileName = Path.GetFileName(fileName);
            string newFileName = filePath + "/" + onlyFileName;
            //if (File.Exists(newFileName))
            //{
            //    errorinfo = string.Format("本地文件{0}已存在,無法下載", newFileName);
            //    return false;
            //}
            string url = "ftp://" + ftpServerIP + "/SHOP/" + ShopCode + "/UPLOAD/" + fileName;
            Connect(url);//連接  
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

            Stream ftpStream = response.GetResponseStream();
            long cl = response.ContentLength;
            int bufferSize = 20480000;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            FileStream outputStream = new FileStream(newFileName, FileMode.Create);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);

            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            errorinfo = "";
            return true;
        }
        catch (Exception ex)
        {
            errorinfo = string.Format("因{0},無法下載", ex.Message);
            return false;
        }
    }
    //刪除文件
    public void DeleteFileName(string fileName)
    {
        try
        {
            FileInfo fileInf = new FileInfo(fileName);

            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            Connect(uri);//連接          
            // 默認為true，連接不會被關閉
            // 在一個命令之後被執行
            reqFTP.KeepAlive = false;
            // 指定執行什麽命令
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "刪除錯誤");
        }
    }

    //創建目錄
    public void MakeDir(string dirName)
    {
        try
        {
            string uri = "ftp://" + ftpServerIP + "/" + dirName;
            Connect(uri);//連接       
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    //刪除目錄
    public void delDir(string dirName)
    {
        try
        {
            string uri = "ftp://" + ftpServerIP + "/" + dirName;
            Connect(uri);//連接       
            reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    //獲得文件大小

    public long GetFileSize(string filename)
    {
        long fileSize = 0;
        try
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            Connect(uri);//連接       
            reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            fileSize = response.ContentLength;
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return fileSize;

    }

    //文件改名
    public void Rename(string currentFilename, string newFilename)
    {
        try
        {
            FileInfo fileInf = new FileInfo(currentFilename);
            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            Connect(uri);//連接
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = newFilename;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            //Stream ftpStream = response.GetResponseStream();

            //ftpStream.Close();
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    //獲得文件
    public string[] GetFilesDetailList()
    {
        return GetFileList("ftp://" + ftpServerIP + "/", WebRequestMethods.Ftp.ListDirectoryDetails);
    }
    //獲得文件
    public string[] GetFilesDetailList(string path)
    {
        return GetFileList("ftp://" + ftpServerIP + "/" + path, WebRequestMethods.Ftp.ListDirectoryDetails);
    }
}


