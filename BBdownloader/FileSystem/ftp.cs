using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BBdownloader.FileSystem
{
    public class Ftp: IFileSystem
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;
        private readonly int pause = 500;

        private string _path;

        /* Construct Object */
        public Ftp(string hostIP, string userName, string password)
        {
            host = hostIP;
            user = userName;
            pass = password;
        }
   
        public string GetFullPath()
        {
            return this._path;
        }

        
        /* Rename File */
        public void rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                //ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                /* Rename the File */
                ftpRequest.RenameTo = newFileName;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        public string[] ReadFile(string remoteFile)
        {
            if (!this.DirectoryExists(remoteFile))
                return new string[] {""};

            var contents = new List<string>();

            string currentPath = Path.Combine(this._path, remoteFile);

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            ftpRequest.Credentials = new NetworkCredential(user, pass);         
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;

            try
            {              
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Thread.Sleep(pause);

                ftpStream = ftpResponse.GetResponseStream();
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead;

                using (var stream = new MemoryStream())
                { 
                    try
                    {
                        while ((bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize)) != 0)
                        {                            
                            stream.Write(byteBuffer, 0, bytesRead);                            
                        }
                        stream.Flush();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                    stream.Position = 0;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!String.IsNullOrEmpty(line))
                                contents.Add(line);
                        }                                
                    }

                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                ftpStream.Close();
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;

            }
            return contents.ToArray();
        }
        
        public bool WriteFile(string remoteFile, string[] contents)
        {
            string currentPath = Path.Combine(this._path, remoteFile);

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;

            try
            {
                ftpStream = ftpRequest.GetRequestStream();
                Thread.Sleep(pause);
                byte[] byteBuffer = new byte[bufferSize];

                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var line in contents)
                    {
                        writer.Write(line);
                    }
                    writer.Flush();

                    stream.Position = 0;
                    try
                    {
                        int bytesSent;

                        while ((bytesSent = stream.Read(byteBuffer, 0, bufferSize)) != 0)
                        {
                            ftpStream.Write(byteBuffer, 0, bytesSent);
                        }
                        ftpStream.Flush();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                ftpStream.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            return true;
        }

        public bool WriteFile(string remoteFile, string contents)
        {
            string[] array = contents.Split('\n');
            this.WriteFile(remoteFile, array);
            return true;
        }

        public bool WriteFileRaw(string remoteFile, byte[] byteArray)
        {
            string currentPath = Path.Combine(this._path, remoteFile);

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;

            try
            {
                ftpStream = ftpRequest.GetRequestStream();
                Thread.Sleep(pause);
                byte[] byteBuffer = new byte[bufferSize];

                using (var stream = new MemoryStream(byteArray))
                using (var writer = new StreamWriter(stream))
                {            
                    /*       
                    foreach (var line in byteArray)
                    {
                        writer.Write(line);
                    }
                    writer.Flush();
                    */
                    stream.Position = 0;
                    try
                    {
                        int bytesSent;

                        while ((bytesSent = stream.Read(byteBuffer, 0, bufferSize)) != 0)
                        {
                            ftpStream.Write(byteBuffer, 0, bytesSent);
                        }
                        ftpStream.Flush();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                ftpStream.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            return true;
        }

        /* Delete File */
        public bool DeleteFile(string deleteFile)
        {
            if (!this.FileExists(deleteFile))
                return false;

            string currentPath = Path.Combine(this._path, deleteFile);

            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            try
            {
                /* Create an FTP Request */
                             
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Thread.Sleep(pause);
                /* Resource Cleanup */
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            return true;
        }

        public bool DeleteDirectory(string deleteDirectory)
        {
            if (!this.DirectoryExists(deleteDirectory))
                return false;

            var files = from f in this.ListFiles(deleteDirectory)
                        select f;

            foreach (string f in files)
            {
                if (!String.IsNullOrEmpty(f))
                    DeleteFile(Path.Combine(deleteDirectory, f));
            }

            string currentPath = Path.Combine(this._path, deleteDirectory);

            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            try
            {
                /* Create an FTP Request */

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Thread.Sleep(pause);
                
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            finally
            {
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            return true;
        }


        public string[] ListDirectories(string directory)
        {
            return ListFiles(directory);
        }


        /* List Directory Contents File/Folder Name Only */
        public string[] ListFiles(string directory)
        {
            string[] directoryList = new List<string>().ToArray();

            if (!this.DirectoryExists(directory))
                return new string[] { "" };

            /* Create an FTP Request */
            string currentPath = Path.Combine(this._path, directory);
                
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + currentPath);

            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            

            try {
                
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Thread.Sleep(pause);

                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                string directoryRaw = null;

                using (var ftpReader = new StreamReader(ftpStream))
                {
                    
                    /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */

                    while (ftpReader.Peek() != -1)
                    {
                        string file = Path.GetFileName(ftpReader.ReadLine());

                        if (!file.Equals("..") && !file.Equals(".") && !String.IsNullOrEmpty(file))
                            directoryRaw += file + "|";
                    }
                }
                /* Store the Raw Response */
                
                directoryRaw = directoryRaw.Trim('|');
                try 
                { 
                    directoryList = directoryRaw.Split("|".ToCharArray()); 
                }
                catch 
                { 
                    //Console.WriteLine("Directory Empty"); 
                }
            }
            catch (Exception ex) 
            {
                //Console.WriteLine("Directory Empty");
            }
            finally
            {
                ftpStream.Close();
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            return directoryList;
        }

        /* Create a New Directory on the FTP Server */
        public bool CreateDirectory(string newDirectory)
        {
            try
            {
                string currentPath = Path.Combine(this._path, newDirectory);

                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Thread.Sleep(pause);
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return true;
        }

        /* Get the Date/Time a File was Created */
        public string getFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { fileInfo = ftpReader.ReadToEnd(); }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Created Date Time */
                return fileInfo;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* Get the Size of a File */
        public float FileSize(string fileName)
        {
            string currentPath = Path.Combine(this._path, fileName);
            float size = 0;
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + currentPath);
                //ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                size = ftpResponse.ContentLength;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString()); 
            }
            finally
            {
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
            }
            
            return size;
        }


        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] directoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;

                
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        public void SetPath(string path)
        {
            _path = path;

            if (!this.DirectoryExists(""))
                this.CreateDirectory("");
        }

        public bool DirectoryExists(string path)
        {
            bool directoryExists = false;

            string currentPath = Path.Combine(this._path, path);

            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);
            ftpRequest.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            
            try
            {
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                directoryExists = true;                
            }
            catch (WebException ex)
            {
                directoryExists = false;
            }
            finally
            {
                ftpResponse.Close();
                ftpResponse = null;
                ftpRequest = null;
            }

            return directoryExists;
        }

        public bool FileExists(string path)
        {
            bool fileExists = false;

            string currentPath = Path.Combine(this._path, path);

            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentPath);            
            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            ftpRequest.Credentials = new NetworkCredential(user, pass);

            try
            {
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                fileExists = true;
                
            }
            catch (WebException ex)
            {
                fileExists = false;
            }
            finally
            {
                if (ftpResponse != null)
                    ftpResponse.Close();
                ftpResponse = null;            
                ftpRequest = null;
            }

            return fileExists;
        }

        public byte[] ReadFileRaw(string path)
        {
            throw new NotImplementedException();
        }
    }
}
