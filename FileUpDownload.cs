using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

public class FileUpDownload
{
    #region 變量屬性
    /// <summary>  
    /// Ftp伺服器ip  
    /// </summary>  
    public static string FtpServerIP = string.Empty;
    /// <summary>  
    /// Ftp 指定用戶名  
    /// </summary>  
    public static string FtpUserID = string.Empty;
    /// <summary>  
    /// Ftp 指定使用者密碼  
    /// </summary>  
    public static string FtpPassword = string.Empty;

    #endregion

    #region 從FTP伺服器下載文件，指定本地路徑和本地文件名
    /// <summary>  
    /// 從FTP伺服器下載檔案，遠端檔案名和本地檔案名 
    /// </summary>  
    /// <param name="remoteFileName">遠程檔案名</param>  
    /// <param name="localFileName">保存本地的檔案名（包含路徑）</param>  
    /// <param name="ifCredential">是否啓用身份驗證（false：表示允許用戶匿名下載）</param>  
    /// <param name="updateProgress">報告進度的處理(第一個參數：總大小，第二個參數：當前進度)</param>  
    /// <returns>是否下載成功</returns>  
    public static bool FtpDownload(string remoteFileName, string localFileName, bool ifCredential, String ShopCode, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP, ftpsize;
        Stream ftpStream = null;
        FtpWebResponse response = null;
        FileStream outputStream = null;
        try
        {
            //string activeDir = @"..\" + ShopCode + "";
            //string newPath = System.IO.Path.Combine(activeDir, "Upload");
            //System.IO.Directory.CreateDirectory(newPath);

            if (!File.Exists(localFileName))
            {
                File.Create(localFileName);
            }
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp下載目標伺服器位址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/" + ShopCode + "/DOWNLOAD/" + remoteFileName);
            ftpsize = (FtpWebRequest)FtpWebRequest.Create(uri);
            ftpsize.UseBinary = true;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.UseBinary = true;
            reqFTP.KeepAlive = false;
            if (ifCredential)//使用用戶身份認證  
            {
                ftpsize.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            }
            ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
            FtpWebResponse re = (FtpWebResponse)ftpsize.GetResponse();
            long totalBytes = re.ContentLength;
            re.Close();

            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            response = (FtpWebResponse)reqFTP.GetResponse();
            ftpStream = response.GetResponseStream();
            ftpStream.Close();
            //outputStream.Close();
            response.Close();
            return true;
        }
        catch (Exception)
        {
            return false;
            throw;
        }
        finally
        {
            if (ftpStream != null)
            {
                ftpStream.Close();
            }
            //if (outputStream != null)
            //{
            //    outputStream.Close();
            //}
            if (response != null)
            {
                response.Close();
            }
        }
    }
    /// <summary>  
    /// 從FTP伺服器下載文件，指定本地路徑和本地文件名（支持斷點下載）  
    /// </summary>  
    /// <param name="remoteFileName">遠程文件名</param>  
    /// <param name="localFileName">保存本地的文件名（包含路徑）</param>  
    /// <param name="ifCredential">是否啓用身份驗證（false：表示允許用戶匿名下載）</param>  
    /// <param name="size">已下載文件流大小</param>  
    /// <param name="updateProgress">報告進度的處理(第一個參數：總大小，第二個參數：當前進度)</param>  
    /// <returns>是否下載成功</returns>  
    public static bool FtpBrokenDownload(string remoteFileName, string localFileName, bool ifCredential, long size, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP, ftpsize;
        Stream ftpStream = null;
        FtpWebResponse response = null;
        FileStream outputStream = null;
        try
        {

            outputStream = new FileStream(localFileName, FileMode.Append);
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp下載目標伺服器地址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/" + remoteFileName);
            ftpsize = (FtpWebRequest)FtpWebRequest.Create(uri);
            ftpsize.UseBinary = true;
            ftpsize.ContentOffset = size;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.UseBinary = true;
            reqFTP.KeepAlive = false;
            reqFTP.ContentOffset = size;
            if (ifCredential)//使用用戶身份認證  
            {
                ftpsize.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            }
            ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
            FtpWebResponse re = (FtpWebResponse)ftpsize.GetResponse();
            long totalBytes = re.ContentLength;
            re.Close();

            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            response = (FtpWebResponse)reqFTP.GetResponse();
            ftpStream = response.GetResponseStream();

            //更新進度    
            if (updateProgress != null)
            {
                updateProgress((int)totalBytes, 0);//更新進度條     
            }
            long totalDownloadedByte = 0;
            int bufferSize = 2048;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            while (readCount > 0)
            {
                totalDownloadedByte = readCount + totalDownloadedByte;
                outputStream.Write(buffer, 0, readCount);
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)totalBytes, (int)totalDownloadedByte);//更新進度條     
                }
                readCount = ftpStream.Read(buffer, 0, bufferSize);
            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            return true;
        }
        catch (Exception)
        {
            return false;
            throw;
        }
        finally
        {
            if (ftpStream != null)
            {
                ftpStream.Close();
            }
            if (outputStream != null)
            {
                outputStream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
        }
    }

    /// <summary>  
    /// 從FTP伺服器下載文件，指定本地路徑和本地文件名  
    /// </summary>  
    /// <param name="remoteFileName">遠程文件名</param>  
    /// <param name="localFileName">保存本地的文件名（包含路徑）</param>  
    /// <param name="ifCredential">是否啓用身份驗證（false：表示允許用戶匿名下載）</param>  
    /// <param name="updateProgress">報告進度的處理(第一個參數：總大小，第二個參數：當前進度)</param>  
    /// <param name="brokenOpen">是否斷點下載：true 會在localFileName 找是否存在已經下載的文件，並計算文件流大小</param>  
    /// <returns>是否下載成功</returns>  
    public static bool FtpDownload(string remoteFileName, string localFileName, bool ifCredential, bool brokenOpen, string shopcode, Action<int, int> updateProgress = null)
    {
        if (brokenOpen)
        {
            try
            {
                long size = 0;
                if (File.Exists(localFileName))
                {
                    using (FileStream outputStream = new FileStream(localFileName, FileMode.Open))
                    {
                        size = outputStream.Length;
                    }
                }
                return FtpBrokenDownload(remoteFileName, localFileName, ifCredential, size, updateProgress);
            }
            catch
            {
                throw;
            }
        }
        else
        {
            return FtpDownload(remoteFileName, localFileName, ifCredential, shopcode, updateProgress);
        }
    }
    #endregion

    #region 上傳文件到FTP伺服器
    /// <summary>  
    /// 上傳文件到FTP伺服器  
    /// </summary>  
    /// <param name="localFullPath">本地帶有完整路徑的文件名</param>  
    /// <param name="updateProgress">報告進度的處理(第一個參數：總大小，第二個參數：當前進度)</param>  
    /// <returns>是否下載成功</returns>  
    public static bool FtpUploadFile(string localFullPathName, string shopcode, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP;
        Stream stream = null;
        FtpWebResponse response = null;
        FileStream fs = null;
        try
        {
            FileInfo finfo = new FileInfo(localFullPathName);
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp上傳目標伺服器地址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/" + shopcode + "/UPLOAD/" + finfo.Name);
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用戶，密碼  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;//向伺服器發出下載請求命令  
            reqFTP.ContentLength = finfo.Length;//為request指定上傳文件的大小  
            response = reqFTP.GetResponse() as FtpWebResponse;
            reqFTP.ContentLength = finfo.Length;
            int buffLength = 1024;
            byte[] buff = new byte[buffLength];
            int contentLen;
            fs = finfo.OpenRead();
            stream = reqFTP.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            int allbye = (int)finfo.Length;
            //更新進度    
            if (updateProgress != null)
            {
                updateProgress((int)allbye, 0);//更新進度條     
            }
            int startbye = 0;
            while (contentLen != 0)
            {
                startbye = contentLen + startbye;
                stream.Write(buff, 0, contentLen);
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, (int)startbye);//更新進度條     
                }
                contentLen = fs.Read(buff, 0, buffLength);
            }
            stream.Close();
            fs.Close();
            response.Close();
            return true;

        }
        catch (Exception)
        {
            return false;
            throw;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
        }
    }

    /// <summary>  
    /// 上傳文件到FTP伺服器(斷點續傳)  
    /// </summary>  
    /// <param name="localFullPath">本地文件全路徑名稱：C:\Users\JianKunKing\Desktop\IronPython腳本測試工具</param>  
    /// <param name="remoteFilepath">遠程文件所在文件夾路徑</param>  
    /// <param name="updateProgress">報告進度的處理(第一個參數：總大小，第二個參數：當前進度)</param>  
    /// <returns></returns>         
    public static bool FtpUploadBroken(string localFullPath, string remoteFilepath, Action<int, int> updateProgress = null)
    {
        if (remoteFilepath == null)
        {
            remoteFilepath = "";
        }
        string newFileName = string.Empty;
        bool success = true;
        FileInfo fileInf = new FileInfo(localFullPath);
        long allbye = (long)fileInf.Length;
        if (fileInf.Name.IndexOf("#") == -1)
        {
            newFileName = RemoveSpaces(fileInf.Name);
        }
        else
        {
            newFileName = fileInf.Name.Replace("#", "＃");
            newFileName = RemoveSpaces(newFileName);
        }
        long startfilesize = GetFileSize(newFileName, remoteFilepath);
        if (startfilesize >= allbye)
        {
            return false;
        }
        long startbye = startfilesize;
        //更新進度    
        if (updateProgress != null)
        {
            updateProgress((int)allbye, (int)startfilesize);//更新進度條     
        }

        string uri;
        if (remoteFilepath.Length == 0)
        {
            uri = "ftp://" + FtpServerIP + "/" + newFileName;
        }
        else
        {
            uri = "ftp://" + FtpServerIP + "/" + remoteFilepath + "/" + newFileName;
        }
        FtpWebRequest reqFTP;
        // 根據uri創建FtpWebRequest對象   
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
        // ftp用戶名和密碼   
        reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
        // 默認為true，連接不會被關閉   
        // 在一個命令之後被執行   
        reqFTP.KeepAlive = false;
        // 指定執行什麽命令   
        reqFTP.Method = WebRequestMethods.Ftp.AppendFile;
        // 指定數據傳輸類型   
        reqFTP.UseBinary = true;
        // 上傳文件時通知伺服器文件的大小   
        reqFTP.ContentLength = fileInf.Length;
        int buffLength = 2048;// 緩衝大小設置為2kb   
        byte[] buff = new byte[buffLength];
        // 打開一個文件流 (System.IO.FileStream) 去讀上傳的文件   
        FileStream fs = fileInf.OpenRead();
        Stream strm = null;
        try
        {
            // 把上傳的文件寫入流   
            strm = reqFTP.GetRequestStream();
            // 每次讀文件流的2kb     
            fs.Seek(startfilesize, 0);
            int contentLen = fs.Read(buff, 0, buffLength);
            // 流內容沒有結束   
            while (contentLen != 0)
            {
                // 把內容從file stream 寫入 upload stream   
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
                startbye += contentLen;
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, (int)startbye);//更新進度條     
                }
            }
            // 關閉兩個流   
            strm.Close();
            fs.Close();
        }
        catch
        {
            success = false;
            throw;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
            if (strm != null)
            {
                strm.Close();
            }
        }
        return success;
    }

    /// <summary>  
    /// 去除空格  
    /// </summary>  
    /// <param name="str"></param>  
    /// <returns></returns>  
    private static string RemoveSpaces(string str)
    {
        string a = "";
        CharEnumerator CEnumerator = str.GetEnumerator();
        while (CEnumerator.MoveNext())
        {
            byte[] array = new byte[1];
            array = System.Text.Encoding.ASCII.GetBytes(CEnumerator.Current.ToString());
            int asciicode = (short)(array[0]);
            if (asciicode != 32)
            {
                a += CEnumerator.Current.ToString();
            }
        }
        string sdate = System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()
            + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString() + System.DateTime.Now.Millisecond.ToString();
        return a.Split('.')[a.Split('.').Length - 2] + "." + a.Split('.')[a.Split('.').Length - 1];
    }
    /// <summary>  
    /// 獲取已上傳文件大小  
    /// </summary>  
    /// <param name="filename">文件名稱</param>  
    /// <param name="path">伺服器文件路徑</param>  
    /// <returns></returns>  
    public static long GetFileSize(string filename, string remoteFilepath)
    {
        long filesize = 0;
        try
        {
            FtpWebRequest reqFTP;
            FileInfo fi = new FileInfo(filename);
            string uri;
            if (remoteFilepath.Length == 0)
            {
                uri = "ftp://" + FtpServerIP + "/" + fi.Name;
            }
            else
            {
                uri = "ftp://" + FtpServerIP + "/" + remoteFilepath + "/" + fi.Name;
            }
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用戶，密碼  
            reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            filesize = response.ContentLength;
            return filesize;
        }
        catch
        {
            return 0;
        }
    }

    public static bool FtpUploadFileC(string localFullPathName, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP;
        Stream stream = null;
        FtpWebResponse response = null;
        FileStream fs = null;
        try
        {
            FileInfo finfo = new FileInfo(localFullPathName);
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp上傳目標伺服器地址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/COMPANY/DOWNLOAD/" + finfo.Name);
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用戶，密碼  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;//向伺服器發出下載請求命令  
            reqFTP.ContentLength = finfo.Length;//為request指定上傳文件的大小  
            response = reqFTP.GetResponse() as FtpWebResponse;
            reqFTP.ContentLength = finfo.Length;
            int buffLength = 1024;
            byte[] buff = new byte[buffLength];
            int contentLen;
            fs = finfo.OpenRead();
            stream = reqFTP.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            int allbye = (int)finfo.Length;
            //更新進度    
            if (updateProgress != null)
            {
                updateProgress((int)allbye, 0);//更新進度條     
            }
            int startbye = 0;
            while (contentLen != 0)
            {
                startbye = contentLen + startbye;
                stream.Write(buff, 0, contentLen);
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, (int)startbye);//更新進度條     
                }
                contentLen = fs.Read(buff, 0, buffLength);
            }
            stream.Close();
            fs.Close();
            response.Close();
            return true;

        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.Black;
            return false;
            throw;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
        }
    }

    public static bool FtpUploadFileS(string localFullPathName, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP;
        Stream stream = null;
        FtpWebResponse response = null;
        FileStream fs = null;
        try
        {
            FileInfo finfo = new FileInfo(localFullPathName);
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp上傳目標伺服器地址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/SHOP/DOWNLOAD/" + finfo.Name);
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用戶，密碼  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;//向伺服器發出下載請求命令  
            reqFTP.ContentLength = finfo.Length;//為request指定上傳文件的大小  
            response = reqFTP.GetResponse() as FtpWebResponse;
            reqFTP.ContentLength = finfo.Length;
            int buffLength = 1024;
            byte[] buff = new byte[buffLength];
            int contentLen;
            fs = finfo.OpenRead();
            stream = reqFTP.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            int allbye = (int)finfo.Length;
            //更新進度    
            if (updateProgress != null)
            {
                updateProgress((int)allbye, 0);//更新進度條     
            }
            int startbye = 0;
            while (contentLen != 0)
            {
                startbye = contentLen + startbye;
                stream.Write(buff, 0, contentLen);
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, (int)startbye);//更新進度條     
                }
                contentLen = fs.Read(buff, 0, buffLength);
            }
            stream.Close();
            fs.Close();
            response.Close();
            return true;

        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.Black;
            return false;
            throw;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
        }
    }

    public static bool FtpUploadFileP(string localFullPathName, Action<int, int> updateProgress = null)
    {
        FtpWebRequest reqFTP;
        Stream stream = null;
        FtpWebResponse response = null;
        FileStream fs = null;
        try
        {
            FileInfo finfo = new FileInfo(localFullPathName);
            if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
            {
                throw new Exception("ftp上傳目標伺服器地址未設置！");
            }
            Uri uri = new Uri("ftp://" + FtpServerIP + "/PUBLIC/DOWNLOAD/" + finfo.Name);
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用戶，密碼  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;//向伺服器發出下載請求命令  
            reqFTP.ContentLength = finfo.Length;//為request指定上傳文件的大小  
            response = reqFTP.GetResponse() as FtpWebResponse;
            reqFTP.ContentLength = finfo.Length;
            int buffLength = 1024;
            byte[] buff = new byte[buffLength];
            int contentLen;
            fs = finfo.OpenRead();
            stream = reqFTP.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            int allbye = (int)finfo.Length;
            //更新進度    
            if (updateProgress != null)
            {
                updateProgress((int)allbye, 0);//更新進度條     
            }
            int startbye = 0;
            while (contentLen != 0)
            {
                startbye = contentLen + startbye;
                stream.Write(buff, 0, contentLen);
                //更新進度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, (int)startbye);//更新進度條     
                }
                contentLen = fs.Read(buff, 0, buffLength);
            }
            stream.Close();
            fs.Close();
            response.Close();
            return true;

        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.Black;
            return false;
            throw;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
        }
    }

    public static bool fileDelete(string ftpPath, string ftpName)
    {
        bool success = false;
        FtpWebRequest ftpWebRequest = null;
        FtpWebResponse ftpWebResponse = null;
        Stream ftpResponseStream = null;
        StreamReader streamReader = null;
        try
        {
            string uri = "ftp://" + FtpServerIP + ftpPath + ftpName;
            ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            ftpWebRequest.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            ftpWebRequest.KeepAlive = false;
            ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
            long size = ftpWebResponse.ContentLength;
            ftpResponseStream = ftpWebResponse.GetResponseStream();
            streamReader = new StreamReader(ftpResponseStream);
            string result = String.Empty;
            result = streamReader.ReadToEnd();

            success = true;
        }
        catch (Exception)
        {
            success = false;
        }
        finally
        {
            if (streamReader != null)
            {
                streamReader.Close();
            }
            if (ftpResponseStream != null)
            {
                ftpResponseStream.Close();
            }
            if (ftpWebResponse != null)
            {
                ftpWebResponse.Close();
            }
        }
        return success;
    }

    #endregion
}

