using System;
using System.IO;
using System.Net;
using System.Text;


namespace Seedunk.Net.Utils
{

    /**
      var ftp =    new FTPClient("192.168.100.250", "21", "", "ccd", "123456", false);
            string dir = "/" + DateTime.Now.ToString("yyyyMMdd") + "/自定义的目录/";
            ftp.FtpCheckDirectoryExist(dir);
            ftp.UploadFilePath("C:\\Users\\Administrator\\Desktop\\test.jpg", dir + "文件名2.jpg");

     */
    public class FTPClient
    {
        private string ftpServerIP;
        private string ftpRemotePath;
        private string ftpUserID;
        private string ftpPassword;
        public string ftpURI;
        private string ftpServerPort;
        private bool ftpMode;
        /// <summary>
        /// 连接FTP
        /// </summary>
        /// <param name="FtpServerIP">FTP连接地址</param>
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>
        /// <param name="FtpUserID">用户名</param>
        /// <param name="FtpPassword">密码</param>
        public FTPClient(string FtpServerIP, string FtpServerPort, string FtpRemotePath, string FtpUserID, string FtpPassword, bool FtpMode)
        {
            ftpServerIP = FtpServerIP;
            ftpRemotePath = FtpRemotePath;
            ftpUserID = FtpUserID;
            ftpPassword = FtpPassword;
            ftpServerPort = FtpServerPort;
            ftpMode = FtpMode;
            ftpURI = "ftp://" + ftpServerIP + ":" + ftpServerPort + "/";
            if (!string.IsNullOrEmpty(ftpRemotePath))
            {
                ftpURI = ftpURI + ftpRemotePath + "/";
            }
        }

        /// <summary>  
        /// 上传  
        /// </summary>   
        public void UploadFile(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + fileInf.Name));
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = ftpMode;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInf.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("上传  Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 上传文件到远程ftp
        /// </summary>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <returns></returns>
        public bool UploadFilePath(string path, string FtpPath)
        {


            FileInfo f = new FileInfo(path);
            FtpWebRequest reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + FtpPath));
            reqFtp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFtp.KeepAlive = false;
            reqFtp.UsePassive = ftpMode;
            reqFtp.UseBinary = true;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;


            FileStream fs = f.OpenRead();
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("上传  Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public string DownloadFile(string fileName, string filePath)
        {
            FtpWebRequest reqFTP;
            try
            {
                FileStream outputStream = new FileStream(filePath + "\\" + fileName.Trim(), FileMode.Create);
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + fileName.Trim()));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                return fileName + " 下载失败 --> " + ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public string DeleteFile(string fileName)
        {
            try
            {
                string uri = ftpURI + fileName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string result = string.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                return "删除失败 --> " + ex.Message + "  文件名:" + fileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderName"></param>
        public void RemoveDirectory(string folderName)
        {
            try
            {
                string uri = ftpURI + folderName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;

                string result = string.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("FtpHelper  Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <returns>名称数组</returns>
        public string[] GetFilesDetailList()
        {
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI));
                ftp.UseBinary = true;
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftp.KeepAlive = false;
                ftp.UsePassive = ftpMode;//表示连接类型为主动模式
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                //while (reader.Read() > 0)
                //{
                //}
                string line = reader.ReadLine();
                //line = reader.ReadLine();
                //line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                throw new Exception("文件列表获取  Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <returns></returns>
        public string[] GetFileList(string mask)
        {
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.ToLower().IndexOf(mask) > 0)
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    //if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                    //{
                    //    string mask_ = mask.Substring(0, mask.IndexOf("*"));
                    //    if (line.Substring(0, mask_.Length) == mask_)
                    //    {
                    //        result.Append(line);
                    //        result.Append("\n");
                    //    }
                    //}
                    //else
                    //{
                    //    result.Append(line);
                    //    result.Append("\n");
                    //}
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                throw new Exception("FtpHelper  Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetDirectoryList()
        {
            string[] drectory = GetFilesDetailList();
            string m = string.Empty;
            foreach (string str in drectory)
            {
                int dirPos = str.IndexOf("<DIR>");
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                {
                    /*判断 Unix 风格*/
                    string dir = str.Substring(54).Trim();
                    if (dir != "." && dir != "..")
                    {
                        m += dir + "\n";
                    }
                }
            }
            char[] n = new char[] { '\n' };
            return m.Split(n);
        }


        //目录解析
        public string FtpParseDirectory(string destFilePath)
        {
            return destFilePath.Substring(0, destFilePath.LastIndexOf("/"));
        }
        //判断文件的目录是否存,不存则创建  
        public void FtpCheckDirectoryExist(string destFilePath)
        {
            string fullDir = FtpParseDirectory(destFilePath);   //目录解析
            string[] dirs = fullDir.Split('/');
            string curDir = "/";
            for (int i = 0; i < dirs.Length; i++)
            {
                string dir = dirs[i];
                try
                {
                    curDir += dir + "/";    //一层一层创建
                    MakeDir(curDir);        //创建文件夹
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// </summary>
        /// <param name="RemoteDirectoryName">指定的目录名</param>
        public bool DirectoryExist(string RemoteDirectoryName)
        {
            string[] dirList = GetDirectoryList();
            foreach (string str in dirList)
            {
                if (str.Trim() == RemoteDirectoryName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当前目录下指定的文件是否存在
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public bool FileExist(string RemoteFileName)
        {
            string[] fileList = GetFileList("*.*");
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirName"></param>
        public void MakeDir(string dirName)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("FtpHelper MakeDir Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 获取指定文件大小
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public long GetFileSize(string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = ftpMode;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("FtpHelper GetFileSize Error --> " + ex.Message);
            }
            return fileSize;
        }

        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void ReName(string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = ftpMode;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("FtpHelper ReName Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void MovieFile(string currentFilename, string newDirectory)
        {
            ReName(currentFilename, newDirectory);
        }

        /// <summary>
        /// 切换当前目录
        /// </summary>
        /// <param name="DirectoryName"></param>
        /// <param name="IsRoot">true 绝对路径   false 相对路径</param>
        public void GotoDirectory(string DirectoryName, bool IsRoot)
        {
            if (IsRoot)
            {
                ftpRemotePath = DirectoryName;
            }
            else
            {
                ftpRemotePath += DirectoryName + "/";
            }
            //ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "/";
            ftpURI = "ftp://" + ftpServerIP + ":" + ftpServerPort + "/";
            if (!string.IsNullOrEmpty(ftpRemotePath))
            {
                ftpURI += ftpRemotePath;
            }
        }
    }
}
