using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;


/// <summary>  
/// 壓縮和解壓文件  
/// </summary>  
public class ZipClass
{
    /// <summary>  
    /// 所有文件緩存  
    /// </summary>  
    List<string> files = new List<string>();

    /// <summary>  
    /// 所有空目錄緩存  
    /// </summary>  
    List<string> paths = new List<string>();

    /// <summary>  
    /// 壓縮單個文件  
    /// </summary>  
    /// <param name="fileToZip">要壓縮的文件</param>  
    /// <param name="zipedFile">壓縮後的文件全名</param>  
    /// <param name="compressionLevel">壓縮程度，範圍0-9，數值越大，壓縮程序越高</param>  
    /// <param name="blockSize">分塊大小</param>  
    public void ZipFile(string fileToZip, string zipedFile, int compressionLevel, int blockSize)
    {
        if (!System.IO.File.Exists(fileToZip))//如果文件沒有找到，則報錯  
        {
            throw new FileNotFoundException("The specified file " + fileToZip + " could not be found. Zipping aborderd");
        }

        FileStream streamToZip = new FileStream(fileToZip, FileMode.Open, FileAccess.Read);
        FileStream zipFile = File.Create(zipedFile);
        ZipOutputStream zipStream = new ZipOutputStream(zipFile);
        ZipEntry zipEntry = new ZipEntry(fileToZip);
        zipStream.PutNextEntry(zipEntry);
        zipStream.SetLevel(compressionLevel);
        byte[] buffer = new byte[blockSize];
        int size = streamToZip.Read(buffer, 0, buffer.Length);
        zipStream.Write(buffer, 0, size);

        try
        {
            while (size < streamToZip.Length)
            {
                int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                zipStream.Write(buffer, 0, sizeRead);
                size += sizeRead;
            }
        }
        catch (Exception ex)
        {
            GC.Collect();
            throw ex;
        }

        zipStream.Finish();
        zipStream.Close();
        streamToZip.Close();
        GC.Collect();
    }

    /// <summary>  
    /// 壓縮目錄（包括子目錄及所有文件）  
    /// </summary>  
    /// <param name="rootPath">要壓縮的根目錄</param>  
    /// <param name="destinationPath">保存路徑</param>  
    /// <param name="compressLevel">壓縮程度，範圍0-9，數值越大，壓縮程序越高</param>  
    public void ZipFileFromDirectory(string rootPath, string destinationPath, int compressLevel)
    {
        GetAllDirectories(rootPath);

        string rootMark = rootPath + "\\";//得到當前路徑的位置，以備壓縮時將所壓縮內容轉變成相對路徑。  
        Crc32 crc = new Crc32();
        ZipOutputStream outPutStream = new ZipOutputStream(File.Create(destinationPath));
        outPutStream.SetLevel(compressLevel); // 0 - store only to 9 - means best compression  
        foreach (string file in files)
        {
            FileStream fileStream = File.OpenRead(file);//打開壓縮文件  
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            ZipEntry entry = new ZipEntry(file.Replace(rootMark, string.Empty));
            entry.DateTime = DateTime.Now;
            // set Size and the crc, because the information  
            // about the size and crc should be stored in the header  
            // if it is not set it is automatically written in the footer.  
            // (in this case size == crc == -1 in the header)  
            // Some ZIP programs have problems with zip files that don't store  
            // the size and crc in the header.  
            entry.Size = fileStream.Length;
            fileStream.Close();
            crc.Reset();
            crc.Update(buffer);
            entry.Crc = crc.Value;
            outPutStream.PutNextEntry(entry);
            outPutStream.Write(buffer, 0, buffer.Length);
        }

        this.files.Clear();

        foreach (string emptyPath in paths)
        {
            ZipEntry entry = new ZipEntry(emptyPath.Replace(rootMark, string.Empty) + "/");
            outPutStream.PutNextEntry(entry);
        }

        this.paths.Clear();
        outPutStream.Finish();
        outPutStream.Close();
        GC.Collect();
    }

    /// <summary>  
    /// 取得目錄下所有文件及文件夾，分別存入files及paths  
    /// </summary>  
    /// <param name="rootPath">根目錄</param>  
    private void GetAllDirectories(string rootPath)
    {

        string[] subPaths = Directory.GetDirectories(rootPath);//得到所有子目錄  
        foreach (string path in subPaths)
        {
            GetAllDirectories(path);//對每一個字目錄做與根目錄相同的操作：即找到子目錄並將當前目錄的文件名存入List  
        }
        string[] files = Directory.GetFiles(rootPath);
        foreach (string file in files)
        {
            this.files.Add(file);//將當前目錄中的所有文件全名存入文件List  
        }
        if (subPaths.Length == files.Length && files.Length == 0)//如果是空目錄  
        {
            this.paths.Add(rootPath);//記錄空目錄  
        }
    }

    /// <summary>  
    /// 解壓縮文件(壓縮文件中含有子目錄)  
    /// </summary>  
    /// <param name="zipfilepath">待解壓縮的文件路徑</param>  
    /// <param name="unzippath">解壓縮到指定目錄</param>  
    /// <returns>解壓後的文件列表</returns>  
    public List<string> UnZip(string zipfilepath, string unzippath)
    {
        //解壓出來的文件列表  
        List<string> unzipFiles = new List<string>();

        //檢查輸出目錄是否以“\\”結尾  
        if (unzippath.EndsWith("\\") == false || unzippath.EndsWith(":\\") == false)
        {
            unzippath += "\\";
        }

        ZipInputStream s = new ZipInputStream(File.OpenRead(zipfilepath));
        ZipEntry theEntry;
        while ((theEntry = s.GetNextEntry()) != null)
        {
            string directoryName = Path.GetDirectoryName(unzippath);
            string fileName = Path.GetFileName(theEntry.Name);

            //生成解壓目錄【用戶解壓到硬盤根目錄時，不需要創建】  
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (fileName != String.Empty)
            {
                //如果文件的壓縮後大小為0那麽說明這個文件是空的,因此不需要進行讀出寫入  
                if (theEntry.CompressedSize == 0)
                    break;
                //解壓文件到指定的目錄  
                directoryName = Path.GetDirectoryName(unzippath + theEntry.Name);
                //建立下面的目錄和子目錄  
                Directory.CreateDirectory(directoryName);

                //記錄導出的文件  
                unzipFiles.Add(unzippath + theEntry.Name);

                FileStream streamWriter = File.Create(unzippath + theEntry.Name);

                int size = 20480000;
                byte[] data = new byte[20480000];
                while (true)
                {
                    size = s.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        streamWriter.Write(data, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                streamWriter.Close();
            }
        }
        s.Close();
        GC.Collect();
        return unzipFiles;
    }
}