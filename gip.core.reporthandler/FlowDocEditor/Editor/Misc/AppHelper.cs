//using Microsoft.VisualBasic;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.Net;
//using System.Web;
//using System.IO;
//using System.Text.RegularExpressions;


//public class AppHelper
//{
//    public struct Margins
//    {
//        public Margins(Thickness t)
//        {
//            Left = Convert.ToInt32(Conversion.Fix(t.Left));
//            Right = Convert.ToInt32(Conversion.Fix(t.Right));
//            Top = Convert.ToInt32(Conversion.Fix(t.Top));
//            Bottom = Convert.ToInt32(Conversion.Fix(t.Bottom));
//        }
//        public int Left;
//        public int Right;
//        public int Top;
//        public int Bottom;
//    }
//    public struct blogInfo
//    {
//        public string title;
//        public string description;
//    }

//    public interface IgetCatList
//    {
//        [CookComputing.XmlRpc.XmlRpcMethod("metaWeblog.newPost")]
//        string NewPage(int blogId, string strUserName, string strPassword, AppHelper.blogInfo content, int publish);
//    }

//    [Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = false)]
//    public static void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMargins)
//    {
//    }

//    [Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = false)]
//    public static bool DwmIsCompositionEnabled()
//    {
//    }

//    public static bool IsGlassEnabled
//    {
//        get { return DwmIsCompositionEnabled(); }
//    }

//    public static bool ExtendGlassFrame(Window window, Thickness margin)
//    {
//        if (!DwmIsCompositionEnabled())
//        {
//            return false;
//        }
//        IntPtr hwnd = new Interop.WindowInteropHelper(window).Handle;
//        Margins margins = new Margins(margin);
//        SolidColorBrush background = new SolidColorBrush(Colors.Red);
//        if (hwnd == IntPtr.Zero)
//        {
//            throw new InvalidOperationException("The Window must be shown before extending glass.");
//        }
//        background.Opacity = 0.5;
//        window.Background = Brushes.Transparent;
//        Interop.HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;
//        DwmExtendFrameIntoClientArea(hwnd, ref margins);
//        return true;
//    }

//    public static void DisableGlassFrame(Window window)
//    {
//        IntPtr hwnd = new Interop.WindowInteropHelper(window).Handle;
//        if (hwnd == IntPtr.Zero)
//        {
//            throw new InvalidOperationException("The Window must be shown before extending glass.");
//        }
//        Interop.HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.White;
//    }
//}

//class FTPItem : ListBoxItem
//{
//    public string Name;
//    public string FileName;
//    public string FullName;
//    public bool IsFile = false;
//}

//namespace Utilities.FTP
//{

//    #region "FTP client class"
//    /// <summary>
//    /// A wrapper class for .NET 2.0 FTP
//    /// </summary>
//    /// <remarks>
//    /// This class does not hold open an FTP connection but 
//    /// instead is stateless: for each FTP request it 
//    /// connects, performs the request and disconnects.
//    /// </remarks>
//    public class FTPclient
//    {

//        #region "CONSTRUCTORS"
//        /// <summary>
//        /// Blank constructor
//        /// </summary>
//        /// <remarks>Hostname, username and password must be set manually</remarks>
//        public FTPclient()
//        {
//        }

//        /// <summary>
//        /// Constructor just taking the hostname
//        /// </summary>
//        /// <param name="Hostname">in either ftp://ftp.host.com or ftp.host.com form</param>
//        /// <remarks></remarks>
//        public FTPclient(string Hostname)
//        {
//            _hostname = Hostname;
//        }

//        /// <summary>
//        /// Constructor taking hostname, username and password
//        /// </summary>
//        /// <param name="Hostname">in either ftp://ftp.host.com or ftp.host.com form</param>
//        /// <param name="Username">Leave blank to use 'anonymous' but set password to your email</param>
//        /// <param name="Password"></param>
//        /// <remarks></remarks>
//        public FTPclient(string Hostname, string Username, string Password)
//        {
//            _hostname = Hostname;
//            _username = Username;
//            _password = Password;
//        }
//        #endregion

//        #region "Directory functions"
//        /// <summary>
//        /// Return a simple directory listing
//        /// </summary>
//        /// <param name="directory">Directory to list, e.g. /pub</param>
//        /// <returns>A list of filenames and directories as a List(of String)</returns>
//        /// <remarks>For a detailed directory listing, use ListDirectoryDetail</remarks>
//        public List<string> ListDirectory(string directory = "")
//        {
//            //return a simple list of filenames in directory
//            System.Net.FtpWebRequest ftp = GetRequest(GetDirectory(directory));
//            //Set request to do simple list
//            ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectory;

//            string str = GetStringResponse(ftp);
//            //replace CRLF to CR, remove last instance
//            str = str.Replace(Constants.vbCrLf, Constants.vbCr).TrimEnd(Strings.Chr(13));
//            //split the string into a list
//            List<string> result = new List<string>();
//            result.AddRange(str.Split(Strings.Chr(13)));
//            return result;
//        }

//        /// <summary>
//        /// Return a detailed directory listing
//        /// </summary>
//        /// <param name="directory">Directory to list, e.g. /pub/etc</param>
//        /// <returns>An FTPDirectory object</returns>
//        public FTPdirectory ListDirectoryDetail(string directory = "")
//        {
//            System.Net.FtpWebRequest ftp = GetRequest(GetDirectory(directory));
//            //Set request to do simple list
//            ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails;

//            string str = GetStringResponse(ftp);
//            //replace CRLF to CR, remove last instance
//            str = str.Replace(Constants.vbCrLf, Constants.vbCr).TrimEnd(Strings.Chr(13));
//            //split the string into a list
//            return new FTPdirectory(str, _lastDirectory);
//        }

//        #endregion

//        #region "Upload: File transfer TO ftp server"
//        /// <summary>
//        /// Copy a local file to the FTP server
//        /// </summary>
//        /// <param name="localFilename">Full path of the local file</param>
//        /// <param name="targetFilename">Target filename, if required</param>
//        /// <returns></returns>
//        /// <remarks>If the target filename is blank, the source filename is used
//        /// (assumes current directory). Otherwise use a filename to specify a name
//        /// or a full path and filename if required.</remarks>
//        public bool Upload(string localFilename, string targetFilename = "")
//        {
//            //1. check source
//            if (!File.Exists(localFilename))
//            {
//                throw new ApplicationException("File " + localFilename + " not found");
//            }
//            //copy to FI
//            FileInfo fi = new FileInfo(localFilename);
//            return Upload(fi, targetFilename);
//        }

//        /// <summary>
//        /// Upload a local file to the FTP server
//        /// </summary>
//        /// <param name="fi">Source file</param>
//        /// <param name="targetFilename">Target filename (optional)</param>
//        /// <returns></returns>
//        public bool Upload(FileInfo fi, string targetFilename = "")
//        {
//            //copy the file specified to target file: target file can be full path or just filename (uses current dir)

//            //1. check target
//            string target = null;
//            if (string.IsNullOrEmpty(targetFilename.Trim()))
//            {
//                //Blank target: use source filename & current dir
//                target = this.CurrentDirectory + fi.Name;
//            }
//            else if (targetFilename.Contains("/"))
//            {
//                //If contains / treat as a full path
//                target = AdjustDir(targetFilename);
//            }
//            else
//            {
//                //otherwise treat as filename only, use current directory
//                target = CurrentDirectory + targetFilename;
//            }

//            string URI = Hostname + target;
//            //perform copy
//            System.Net.FtpWebRequest ftp = GetRequest(URI);

//            //Set request to upload a file in binary
//            ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
//            ftp.UseBinary = true;

//            //Notify FTP of the expected size
//            ftp.ContentLength = fi.Length;

//            //create byte array to store: ensure at least 1 byte!
//            const int BufferSize = 2048;
//            byte[] content = new byte[BufferSize];
//            int dataRead = 0;

//            //open file for reading 
//            using (FileStream fs = fi.OpenRead())
//            {
//                try
//                {
//                    //open request to send
//                    using (Stream rs = ftp.GetRequestStream())
//                    {
//                        do
//                        {
//                            dataRead = fs.Read(content, 0, BufferSize);
//                            rs.Write(content, 0, dataRead);
//                        } while (!(dataRead < BufferSize));
//                        rs.Close();
//                    }

//                }
//                catch (Exception ex)
//                {
//                }
//                finally
//                {
//                    //ensure file closed
//                    fs.Close();
//                }

//            }

//            ftp = null;
//            return true;

//        }
//        #endregion

//        #region "Download: File transfer FROM ftp server"
//        /// <summary>
//        /// Copy a file from FTP server to local
//        /// </summary>
//        /// <param name="sourceFilename">Target filename, if required</param>
//        /// <param name="localFilename">Full path of the local file</param>
//        /// <returns></returns>
//        /// <remarks>Target can be blank (use same filename), or just a filename
//        /// (assumes current directory) or a full path and filename</remarks>
//        public bool Download(string sourceFilename, string localFilename, bool PermitOverwrite = false)
//        {
//            //2. determine target file
//            FileInfo fi = new FileInfo(localFilename);
//            return this.Download(sourceFilename, fi, PermitOverwrite);
//        }

//        //Version taking an FtpFileInfo
//        public bool Download(FTPfileInfo file, string localFilename, bool PermitOverwrite = false)
//        {
//            return this.Download(file.FullName, localFilename, PermitOverwrite);
//        }

//        //Another version taking FtpFileInfo and FileInfo
//        public bool Download(FTPfileInfo file, FileInfo localFI, bool PermitOverwrite = false)
//        {
//            return this.Download(file.FullName, localFI, PermitOverwrite);
//        }

//        //Version taking string/FileInfo
//        public bool Download(string sourceFilename, FileInfo targetFI, bool PermitOverwrite = false)
//        {
//            //1. check target
//            if (targetFI.Exists & !(PermitOverwrite))
//                throw new ApplicationException("Target file already exists");

//            //2. check source
//            string target = null;
//            if (string.IsNullOrEmpty(sourceFilename.Trim()))
//            {
//                throw new ApplicationException("File not specified");
//            }
//            else if (sourceFilename.Contains("/"))
//            {
//                //treat as a full path
//                target = AdjustDir(sourceFilename);
//            }
//            else
//            {
//                //treat as filename only, use current directory
//                target = CurrentDirectory + sourceFilename;
//            }

//            string URI = Hostname + target;

//            //3. perform copy
//            System.Net.FtpWebRequest ftp = GetRequest(URI);

//            //Set request to download a file in binary mode
//            ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
//            ftp.UseBinary = true;

//            //open request and get response stream
//            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
//            {
//                using (Stream responseStream = response.GetResponseStream())
//                {
//                    //loop to read & write to file
//                    using (FileStream fs = targetFI.OpenWrite())
//                    {
//                        try
//                        {
//                            byte[] buffer = new byte[2048];
//                            int read = 0;
//                            do
//                            {
//                                read = responseStream.Read(buffer, 0, buffer.Length);
//                                fs.Write(buffer, 0, read);
//                            } while (!(read == 0));
//                            responseStream.Close();
//                            fs.Flush();
//                            fs.Close();
//                        }
//                        catch (Exception ex)
//                        {
//                            //catch error and delete file only partially downloaded
//                            fs.Close();
//                            //delete target file as it's incomplete
//                            targetFI.Delete();
//                            throw;
//                        }
//                    }
//                    responseStream.Close();
//                }
//                response.Close();
//            }

//            return true;
//        }
//        #endregion

//        #region "Other functions: Delete rename etc."
//        /// <summary>
//        /// Delete remote file
//        /// </summary>
//        /// <param name="filename">filename or full path</param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        public bool FtpDelete(string filename)
//        {
//            //Determine if file or full path
//            string URI = this.Hostname + GetFullPath(filename);

//            System.Net.FtpWebRequest ftp = GetRequest(URI);
//            //Set request to delete
//            ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
//            try
//            {
//                //get response but ignore it
//                string str = GetStringResponse(ftp);
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        /// Determine if file exists on remote FTP site
//        /// </summary>
//        /// <param name="filename">Filename (for current dir) or full path</param>
//        /// <returns></returns>
//        /// <remarks>Note this only works for files</remarks>
//        public bool FtpFileExists(string filename)
//        {
//            //Try to obtain filesize: if we get error msg containing "550"
//            //the file does not exist
//            try
//            {
//                long size = GetFileSize(filename);
//                return true;

//            }
//            catch (Exception ex)
//            {
//                //only handle expected not-found exception
//                if (ex is System.Net.WebException)
//                {
//                    //file does not exist/no rights error = 550
//                    if (ex.Message.Contains("550"))
//                    {
//                        //clear 
//                        return false;
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Determine size of remote file
//        /// </summary>
//        /// <param name="filename"></param>
//        /// <returns></returns>
//        /// <remarks>Throws an exception if file does not exist</remarks>
//        public long GetFileSize(string filename)
//        {
//            string path = null;
//            if (filename.Contains("/"))
//            {
//                path = AdjustDir(filename);
//            }
//            else
//            {
//                path = this.CurrentDirectory + filename;
//            }
//            string URI = this.Hostname + path;
//            System.Net.FtpWebRequest ftp = GetRequest(URI);
//            //Try to get info on file/dir?
//            ftp.Method = System.Net.WebRequestMethods.Ftp.GetFileSize;
//            string tmp = this.GetStringResponse(ftp);
//            return GetSize(ftp);
//        }

//        public bool FtpRename(string sourceFilename, string newName)
//        {
//            //Does file exist?
//            string source = GetFullPath(sourceFilename);
//            if (!FtpFileExists(source))
//            {
//                throw new FileNotFoundException("File " + source + " not found");
//            }

//            //build target name, ensure it does not exist
//            string target = GetFullPath(newName);
//            if (target == source)
//            {
//                throw new ApplicationException("Source and target are the same");
//            }
//            else if (FtpFileExists(target))
//            {
//                throw new ApplicationException("Target file " + target + " already exists");
//            }

//            //perform rename
//            string URI = this.Hostname + source;

//            System.Net.FtpWebRequest ftp = GetRequest(URI);
//            //Set request to delete
//            ftp.Method = System.Net.WebRequestMethods.Ftp.Rename;
//            ftp.RenameTo = target;
//            try
//            {
//                //get response but ignore it
//                string str = GetStringResponse(ftp);
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }

//        public bool FtpCreateDirectory(string dirpath)
//        {
//            //perform create
//            string URI = this.Hostname + AdjustDir(dirpath);
//            System.Net.FtpWebRequest ftp = GetRequest(URI);
//            //Set request to MkDir
//            ftp.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory;
//            try
//            {
//                //get response but ignore it
//                string str = GetStringResponse(ftp);
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }

//        public bool FtpDeleteDirectory(string dirpath)
//        {
//            //perform remove
//            string URI = this.Hostname + AdjustDir(dirpath);
//            System.Net.FtpWebRequest ftp = GetRequest(URI);
//            //Set request to RmDir
//            ftp.Method = System.Net.WebRequestMethods.Ftp.RemoveDirectory;
//            try
//            {
//                //get response but ignore it
//                string str = GetStringResponse(ftp);
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }
//        #endregion

//        #region "private supporting fns"
//        //Get the basic FtpWebRequest object with the
//        //common settings and security
//        private FtpWebRequest GetRequest(string URI)
//        {
//            //create request
//            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
//            //Set the login details
//            result.Credentials = GetCredentials();
//            //Do not keep alive (stateless mode)
//            result.KeepAlive = false;
//            return result;
//        }


//        /// <summary>
//        /// Get the credentials from username/password
//        /// </summary>
//        private Net.ICredentials GetCredentials()
//        {
//            return new System.Net.NetworkCredential(Username, Password);
//        }

//        /// <summary>
//        /// returns a full path using CurrentDirectory for a relative file reference
//        /// </summary>
//        private string GetFullPath(string file)
//        {
//            if (file.Contains("/"))
//            {
//                return AdjustDir(file);
//            }
//            else
//            {
//                return this.CurrentDirectory + file;
//            }
//        }

//        /// <summary>
//        /// Amend an FTP path so that it always starts with /
//        /// </summary>
//        /// <param name="path">Path to adjust</param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        private string AdjustDir(string path)
//        {
//            return Convert.ToString((path.StartsWith("/") ? "" : "/")) + path;
//        }

//        private string GetDirectory(string directory = "")
//        {
//            string URI = null;
//            if (string.IsNullOrEmpty(directory))
//            {
//                //build from current
//                URI = Hostname + this.CurrentDirectory;
//                _lastDirectory = this.CurrentDirectory;
//            }
//            else
//            {
//                if (!directory.StartsWith("/"))
//                    throw new ApplicationException("Directory should start with /");
//                URI = this.Hostname + directory;
//                _lastDirectory = directory;
//            }
//            return URI;
//        }

//        //stores last retrieved/set directory

//        private string _lastDirectory = "";
//        /// <summary>
//        /// Obtains a response stream as a string
//        /// </summary>
//        /// <param name="ftp">current FTP request</param>
//        /// <returns>String containing response</returns>
//        /// <remarks>FTP servers typically return strings with CR and
//        /// not CRLF. Use respons.Replace(vbCR, vbCRLF) to convert
//        /// to an MSDOS string</remarks>
//        private string GetStringResponse(FtpWebRequest ftp)
//        {
//            //Get the result, streaming to a string
//            string result = "";
//            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
//            {
//                long size = response.ContentLength;
//                using (Stream datastream = response.GetResponseStream())
//                {
//                    using (StreamReader sr = new StreamReader(datastream))
//                    {
//                        result = sr.ReadToEnd();
//                        sr.Close();
//                    }
//                    datastream.Close();
//                }
//                response.Close();
//            }
//            return result;
//        }

//        /// <summary>
//        /// Gets the size of an FTP request
//        /// </summary>
//        /// <param name="ftp"></param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        private long GetSize(FtpWebRequest ftp)
//        {
//            long size = 0;
//            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
//            {
//                size = response.ContentLength;
//                response.Close();
//            }
//            return size;
//        }
//        #endregion

//        #region "Properties"
//        private string _hostname;
//        /// <summary>
//        /// Hostname
//        /// </summary>
//        /// <value></value>
//        /// <remarks>Hostname can be in either the full URL format
//        /// ftp://ftp.myhost.com or just ftp.myhost.com
//        /// </remarks>
//        public string Hostname
//        {
//            get
//            {
//                if (_hostname.StartsWith("ftp://"))
//                {
//                    return _hostname;
//                }
//                else
//                {
//                    return "ftp://" + _hostname;
//                }
//            }
//            set { _hostname = value; }
//        }
//        private string _username;
//        /// <summary>
//        /// Username property
//        /// </summary>
//        /// <value></value>
//        /// <remarks>Can be left blank, in which case 'anonymous' is returned</remarks>
//        public string Username
//        {
//            get { return (string.IsNullOrEmpty(_username) ? "anonymous" : _username); }
//            set { _username = value; }
//        }
//        private string _password;
//        public string Password
//        {
//            get { return _password; }
//            set { _password = value; }
//        }

//        /// <summary>
//        /// The CurrentDirectory value
//        /// </summary>
//        /// <remarks>Defaults to the root '/'</remarks>
//        private string _currentDirectory = "/";
//        public string CurrentDirectory
//        {
//            //return directory, ensure it ends with /
//            get { return _currentDirectory + Convert.ToString((_currentDirectory.EndsWith("/") ? "" : "/")); }
//            set
//            {
//                if (!value.StartsWith("/"))
//                    throw new ApplicationException("Directory should start with /");
//                _currentDirectory = value;
//            }
//        }


//        #endregion

//    }
//    #endregion

//    #region "FTP file info class"
//    /// <summary>
//    /// Represents a file or directory entry from an FTP listing
//    /// </summary>
//    /// <remarks>
//    /// This class is used to parse the results from a detailed
//    /// directory list from FTP. It supports most formats of
//    /// </remarks>
//    public class FTPfileInfo
//    {
//        //Stores extended info about FTP file

//        #region "Properties"
//        public string FullName
//        {
//            get { return Path + Filename; }
//        }
//        public string Filename
//        {
//            get { return _filename; }
//        }
//        public string Path
//        {
//            get { return _path; }
//        }
//        public DirectoryEntryTypes FileType
//        {
//            get { return _fileType; }
//        }
//        public long Size
//        {
//            get { return _size; }
//        }
//        public System.DateTime FileDateTime
//        {
//            get { return _fileDateTime; }
//        }
//        public string Permission
//        {
//            get { return _permission; }
//        }
//        public string Extension
//        {
//            get
//            {
//                int i = this.Filename.LastIndexOf(".");
//                if (i >= 0 & i < (this.Filename.Length - 1))
//                {
//                    return this.Filename.Substring(i + 1);
//                }
//                else
//                {
//                    return "";
//                }
//            }
//        }
//        public string NameOnly
//        {
//            get
//            {
//                int i = this.Filename.LastIndexOf(".");
//                if (i > 0)
//                {
//                    return this.Filename.Substring(0, i);
//                }
//                else
//                {
//                    return this.Filename;
//                }
//            }
//        }
//        private string _filename;
//        private string _path;
//        private DirectoryEntryTypes _fileType;
//        private long _size;
//        private System.DateTime _fileDateTime;

//        private string _permission;
//        #endregion

//        /// <summary>
//        /// Identifies entry as either File or Directory
//        /// </summary>
//        public enum DirectoryEntryTypes
//        {
//            File,
//            Directory
//        }

//        /// <summary>
//        /// Constructor taking a directory listing line and path
//        /// </summary>
//        /// <param name="line">The line returned from the detailed directory list</param>
//        /// <param name="path">Path of the directory</param>
//        /// <remarks></remarks>
//        public FTPfileInfo(string line, string path)
//        {
//            //parse line
//            Match m = GetMatchingRegex(line);
//            if (m == null)
//            {
//                //failed
//                throw new ApplicationException("Unable to parse line: " + line);
//            }
//            else
//            {
//                _filename = m.Groups["name"].Value;
//                _path = path;
//                _size = Convert.ToInt64(m.Groups["size"].Value);
//                _permission = m.Groups["permission"].Value;
//                string _dir = m.Groups["dir"].Value;
//                if ((!string.IsNullOrEmpty(_dir) & _dir != "-"))
//                {
//                    _fileType = DirectoryEntryTypes.Directory;
//                }
//                else
//                {
//                    _fileType = DirectoryEntryTypes.File;
//                }

//                try
//                {
//                    _fileDateTime = System.DateTime.Parse(m.Groups["timestamp"].Value);
//                }
//                catch (Exception ex)
//                {
//                    _fileDateTime = null;
//                }

//            }
//        }

//        private Match GetMatchingRegex(string line)
//        {
//            Regex rx = null;
//            Match m = null;
//            for (int i = 0; i <= _ParseFormats.Length - 1; i++)
//            {
//                rx = new Regex(_ParseFormats[i]);
//                m = rx.Match(line);
//                if (m.Success)
//                    return m;
//            }
//            return null;
//        }

//        #region "Regular expressions for parsing LIST results"
//        /// <summary>
//        /// List of REGEX formats for different FTP server listing formats
//        /// </summary>
//        /// <remarks>
//        /// The first three are various UNIX/LINUX formats, fourth is for MS FTP
//        /// in detailed mode and the last for MS FTP in 'DOS' mode.
//        /// I wish VB.NET had support for Const arrays like C# but there you go
//        /// </remarks>
//        private static string[] _ParseFormats = {
//            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
//            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
//            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
//            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
//            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})(\\s+)(?<size>(\\d+))(\\s+)(?<ctbit>(\\w+\\s\\w+))(\\s+)(?<size2>(\\d+))\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{2}:\\d{2})\\s+(?<name>.+)",
//            "(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)"
//            #endregion
//        };
//    }
//    #endregion

//    #region "FTP Directory class"
//    /// <summary>
//    /// Stores a list of files and directories from an FTP result
//    /// </summary>
//    /// <remarks></remarks>
//    public class FTPdirectory : List<FTPfileInfo>
//    {

//        public FTPdirectory()
//        {
//            //creates a blank directory listing
//        }

//        /// <summary>
//        /// Constructor: create list from a (detailed) directory string
//        /// </summary>
//        /// <param name="dir">directory listing string</param>
//        /// <param name="path"></param>
//        /// <remarks></remarks>
//        public FTPdirectory(string dir, string path)
//        {
//            foreach (string line in dir.Replace(Constants.vbLf, "").Split(Convert.ToChar(Constants.vbCr)))
//            {
//                //parse
//                if (!string.IsNullOrEmpty(line))
//                    this.Add(new FTPfileInfo(line, path));
//            }
//        }

//        /// <summary>
//        /// Filter out only files from directory listing
//        /// </summary>
//        /// <param name="ext">optional file extension filter</param>
//        /// <returns>FTPdirectory listing</returns>
//        public FTPdirectory GetFiles(string ext = "")
//        {
//            return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.File, ext);
//        }

//        /// <summary>
//        /// Returns a list of only subdirectories
//        /// </summary>
//        /// <returns>FTPDirectory list</returns>
//        /// <remarks></remarks>
//        public FTPdirectory GetDirectories()
//        {
//            return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.Directory);
//        }

//        //internal: share use function for GetDirectories/Files
//        private FTPdirectory GetFileOrDir(FTPfileInfo.DirectoryEntryTypes type, string ext = "")
//        {
//            FTPdirectory result = new FTPdirectory();
//            foreach (FTPfileInfo fi in this)
//            {
//                if (fi.FileType == type)
//                {
//                    if (string.IsNullOrEmpty(ext))
//                    {
//                        result.Add(fi);
//                    }
//                    else if (ext == fi.Extension)
//                    {
//                        result.Add(fi);
//                    }
//                }
//            }
//            return result;

//        }

//        public bool FileExists(string filename)
//        {
//            foreach (FTPfileInfo ftpfile in this)
//            {
//                if (ftpfile.Filename == filename)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }


//        private const char slash = "/";
//        public static string GetParentDirectory(string dir)
//        {
//            string tmp = dir.TrimEnd(slash);
//            int i = tmp.LastIndexOf(slash);
//            if (i > 0)
//            {
//                return tmp.Substring(0, i - 1);
//            }
//            else
//            {
//                throw new ApplicationException("No parent for root");
//            }
//        }
//    }
//    #endregion

//}
