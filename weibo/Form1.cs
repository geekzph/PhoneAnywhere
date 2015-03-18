using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web;
using System.Threading;
using System.Runtime.InteropServices;
using WindowsFormsApplication1;
using ThunderAgentLib;
using Microsoft.Win32;
using System.Net.NetworkInformation;
namespace weibo
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "LockWorkStation", CharSet = CharSet.Ansi)]
        private static extern int LockWorkStation();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
            ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int DoFlag, int rea);

        [DllImport("wininet.dll")]
        private extern static bool InternetCheckConnection(string lpszUrl, int dwFlags, int dwReserved); 

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName,
      string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent,
      IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd,
      int Msg, int wParam, int lParam);

        const int WM_GETTEXT = 0x000D;
        const int WM_SETTEXT = 0x000C;
        const int WM_CLICK = 0x00F5;
        const int WM_SYSKEYDOWN = 0X104;
        const int WM_KEYDOWN = 0X100;
        const int WM_KEYUP = 0X101;
        const int WM_SYSCHAR = 0X106;
        const int WM_SYSKEYUP = 0X105;
        const int WM_CHAR = 0X102;

        string updateurl = "http://api.t.sina.com.cn/statuses/update.xml?";
        string uploadurl = "http://api.t.sina.com.cn/statuses/upload.xml?";

        private static bool DoExitWin(int DoFlag)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(DoFlag, 0);
            return ok;
        }

        public Form1()
        {
            InitializeComponent();
            string[] strArr = new string[7] { "public_timeline", "friends_timeline", "user_timeline", "mentions", "comments_timeline", "comments_by_me","count"};
            
        }

        static bool ipsuccess = false;
        public static void neta()//测试网络状态
        {
            ////测试网络状态
            //int i = 0;
            //while (i < 60)
            //{
            //    Ping pingSender = new Ping();
            //    PingOptions options = new PingOptions();
            //    options.DontFragment = true;
            //    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            //    byte[] buffer = Encoding.ASCII.GetBytes(data);
            //    int timeout = 9000;
            //    PingReply reply = pingSender.Send("119.75.217.56", timeout, buffer, options);
            //    if (reply.Status == IPStatus.Success)
            //    {
            //        ipsuccess = true;
            //        break;
            //    }
            //    else
            //    {
            //        //MessageBox.Show("网络错误jping");
            //        //break;
            //        i++;
            //        Thread.Sleep(5000);
            //    }

            //}
            bool state=false;
            while (!state)
            {
                //int I = 1;
                state = InternetCheckConnection("http://www.baidu.com",1,0);
            }
        }
        void shutdownpc()//关机
        {
            //关机
            DoExitWin(EWX_FORCE | EWX_POWEROFF);
        }
        void restart()//重启
        {
            //重启
            DoExitWin(EWX_FORCE | EWX_REBOOT);
        }
        void logoutpc()
        {
            DoExitWin(EWX_FORCE | EWX_LOGOFF);
        }//注销

        void GetModel(string strUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Timeout = 15000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //response.
                response.Close();
            }
            catch
            { }
        }//get请求

        void scshot()//截屏
        {
            //获得当前屏幕的分辨率
            Screen scr = Screen.PrimaryScreen;
            Rectangle rc = scr.Bounds;
            int iWidth = rc.Width;
            int iHeight = rc.Height;
            //创建一个和屏幕一样大的Bitmap
            Image myImage = new Bitmap(iWidth, iHeight);
            //从一个继承自Image类的对象中创建Graphics对象
            Graphics g = Graphics.FromImage(myImage);
            //抓屏并拷贝到myimage里
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
            //保存为文件
            //picpath = picpath + @"\1.jpeg";
            myImage.Save(@"c:\1.jpeg");
            //Bitmap bmpOld = new Bitmap(@"c:\windows\1.jpeg");
            //double d1;
            //if (bmpOld.Height > bmpOld.Width)
            //    d1 = (double)(250 / (double)bmpOld.Width);
            //else
            //    d1 = (double)(250 / (double)bmpOld.Height);
            ////产生缩图
            //Bitmap bmpThumb = new Bitmap(bmpOld, (int)(bmpOld.Width * d1), (int)(bmpOld.Height * d1));

            //bmpThumb.Save(picpath);
            //bmpThumb.Dispose();
            //bmpOld.Dispose();
        }

        string geturl(string url)
        {
            string strRet = "";

            try
            {

                // Creates an HttpWebRequest with the specified URL. 
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // Sends the HttpWebRequest and waits for the response.			
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                // Gets the stream associated with the response.
                Stream receiveStream = myHttpWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("gb2312");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);
                Char[] read = new Char[256];
                // Reads 256 characters at a time.    
                int count = readStream.Read(read, 0, 256);
                Console.WriteLine("HTML...\r\n");
                //read option
                while (count > 0)
                {
                    String str = new String(read, 0, count);
                    strRet = strRet + str;
                    count = readStream.Read(read, 0, 256);
                }
                // Releases the resources of the response.
                myHttpWebResponse.Close();
                // Releases the resources of the Stream.
                readStream.Close();
            }
            catch
            {
               
            }
            return strRet;


        }//获取搜索页面源代码

        string siteurl(string url)
        {
            string strRet = "";

            try
            {

                // Creates an HttpWebRequest with the specified URL. 
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // Sends the HttpWebRequest and waits for the response.			
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                // Gets the stream associated with the response.
                Stream receiveStream = myHttpWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("gb2312");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);
                Char[] read = new Char[256];
                // Reads 256 characters at a time.    
                int count = readStream.Read(read, 0, 256);
                Console.WriteLine("HTML...\r\n");
                //read option
                int i = 0;
                while (i < 20)
                {
                    String str = new String(read, 0, count);
                    strRet = strRet + str;
                    count = readStream.Read(read, 0, 256);
                    i++;
                }
                // Releases the resources of the response.
                myHttpWebResponse.Close();
                // Releases the resources of the Stream.
                readStream.Close();
            }
            catch
            {
                //GetModel(getpath + "/ControlPage.aspx?method=download&info=未找到合适的下载链接&opcode=" + opcode);
            }
            return strRet;


        }//获取下载地址页面源代码

        void DownLoadByThunder(string s, int min, int max, string gs,int flagOauth,int xx)
        {
            string moviename = "http://movie.gougou.com/search1?search=" + s;
            //发送请求
            string strRet = geturl(moviename);
            int i = 0;
            int pos = strRet.IndexOf("http://down.gougou.com");
            string[,] dlurl = new string[20, 3];
            string dlpage = "";
            //分析下载页面
            try
            {
                while (i < 18)
                {
                    int pos2 = strRet.IndexOf("http://down.gougou.com", pos + 10);
                    string downloadsite = strRet.Substring(pos2, 72);
                    Console.WriteLine(downloadsite);
                    pos2 = strRet.IndexOf("<td", pos2);
                    pos2 = strRet.IndexOf("<td", pos2 + 4);
                    pos2 = strRet.IndexOf("<td", pos2 + 4);
                    int pos2end = strRet.IndexOf("</td>", pos2);
                    string danwei = strRet.Substring(pos2end - 1, 1);
                    pos2end = pos2end - pos2 - 4;
                    Console.WriteLine(danwei);
                    double ss;
                    string size = strRet.Substring(pos2 + 4, pos2end);
                    if (danwei == "G")
                    {
                        size = size.Remove(size.Length - 1, 1);
                        ss = double.Parse(size);
                        ss = ss * 1024;
                    }
                    else
                    {
                        size = size.Remove(size.Length - 1, 1);
                        ss = double.Parse(size);
                    }
                    pos2 = strRet.IndexOf("<td", pos2 + 4);
                    pos2end = strRet.IndexOf("</td>", pos2);
                    pos2end = pos2end - pos2 - 4;
                    string geshi = strRet.Substring(pos2 + 4, pos2end);
                    pos2 = strRet.IndexOf("<tr", pos2 + 4);
                    pos2 = strRet.IndexOf("<tr", pos2 + 4);
                    pos = pos2 + 10;
                    dlurl[i, 0] = downloadsite;
                    dlurl[i, 1] = ss.ToString();
                    dlurl[i, 2] = geshi;
                    i++;
                }

                //寻找合适的下载页面
                //Console.WriteLine("");
                for (int m = 0; m < 18; m++)
                {
                    if (double.Parse(dlurl[m, 1]) > min && double.Parse(dlurl[m, 1]) < max)
                        if (dlurl[m, 2] == gs || gs == "任意")
                        {
                            dlpage = dlurl[m, 0];
                            break;
                        }
                        else
                            continue;
                    else
                        continue;
                }


                //if (dlpage == "")
                //    GetModel(getpath + "/ControlPage.aspx?method=download&info=未找到合适的下载链接&opcode=" + opcode);
                //打开下载页面
                string finalstr = siteurl(dlpage);
                int posf = finalstr.IndexOf("g_downUrl");
                int endpos = finalstr.IndexOf(")", posf + 10);
                //int lth = endpos - posf - 17;
                //提取下载地址
                int start = finalstr.IndexOf("(", posf);
                int lth = endpos - start - 3;
                finalstr = finalstr.Substring(start + 2, lth);
                if (finalstr != "")
                {
                    //========================================================================
                    Encoding encode = System.Text.Encoding.GetEncoding("gb2312");
                    Encoding encode2 = System.Text.Encoding.GetEncoding("utf-8");
                    //创建对象
                    //Agent agent = new Agent();
                    //IAgent2 agent=new IAgent2();
                    ThunderAgentLib.AgentClass agent = new ThunderAgentLib.AgentClass();
                    string downloadurl1 = HttpUtility.UrlDecode(finalstr, encode);
                    string downloadurl2 = HttpUtility.UrlEncode(downloadurl1, encode);
                    //解码下载地址
                    if (finalstr != downloadurl2)
                        finalstr = HttpUtility.UrlDecode(finalstr, encode2);
                    agent.AddTask(finalstr);
                    Thread.Sleep(3000);
                    agent.CommitTasks2(1);
                    //整个窗口的类名
                    string lpszParentClass = "#32770";
                    //窗口标题
                    string lpszParentWindow = "NewTask2";
                    string bt = "BT Task";
                    IntPtr ParenthWnd = new IntPtr(0);
                    Thread.Sleep(7000);
                    //查找窗口，发送回车键信息

                    while (i < 20)
                    {
                        ParenthWnd = FindWindow(lpszParentClass, lpszParentWindow);
                        if (!ParenthWnd.Equals(IntPtr.Zero))
                        {
                            SendMessage(ParenthWnd, WM_SYSKEYDOWN, 0x0d, 0);
                            break;
                        }
                        Thread.Sleep(1000);
                        i++;
                    }
                    string[] spgs = new string[13];
                    spgs[0] = "rm";
                    spgs[1] = "rmvb";
                    spgs[2] = "3gp";
                    spgs[3] = "wmv";
                    spgs[4] = "avi";
                    spgs[5] = "mp4";
                    spgs[6] = "mp5";
                    spgs[7] = "mkv";
                    spgs[8] = "asf";
                    spgs[9] = "swf";
                    spgs[10] = "mpg";
                    finalstr.ToLower();


                    while (i < 40)
                    {
                        int mc = 0;
                        int flag = 0;
                        while (mc < 11)
                        {
                            if ((finalstr.IndexOf(spgs[mc], finalstr.Length - 5) != -1))
                            {
                                flag = 1;
                                break;
                            }
                            mc++;

                        }
                        if (finalstr.IndexOf(".torrent") != -1)
                            flag = 0;
                        if (flag == 1)
                            break;
                        ParenthWnd = FindWindow(lpszParentClass, bt);
                        if (!ParenthWnd.Equals(IntPtr.Zero))
                        {
                            SendMessage(ParenthWnd, WM_SYSKEYDOWN, 0x0d, 0);
                            break;
                        }
                        i++;
                        Thread.Sleep(1500);
                    }
                    if (flagOauth == 1)
                        oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode("正在下载")+xx);
                    else
                        oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode("正在下载")+xx);
                }
                else
                {
                    oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode("未找到合适下载链接")+xx);
                }

            }
            catch
            {
                //MessageBox.Show("未找到合适下载链接。");
                if (flagOauth == 1)
                    oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode("未找到合适下载链接")+xx);
                if(flagOauth==0)
                    oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode("未找到合适下载链接")+xx);
            }

        }//使用迅雷下载
        public string GetPage(string posturl, string postData, string username, string password)
        {
            string userpwd = username + ":" + password;
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.ASCII;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            

            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                //
                System.Net.CredentialCache myCache = new System.Net.CredentialCache();
                myCache.Add(new Uri(posturl), "Basic", new System.Net.NetworkCredential(username, password));
                request.Credentials = myCache;
                request.Headers.Add("Authorization", "Basic " +
                Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(userpwd)));
                //
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        } //发送post请求

        public string SendImage(string posturl, byte[] postData, string username, string password)
        {
            //
            
            //
            string userpwd = username + ":" + password;
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.ASCII;
            //byte[] data = encoding.GetBytes(postData);
            // 准备请求...


            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                //
                System.Net.CredentialCache myCache = new System.Net.CredentialCache();
                myCache.Add(new Uri(posturl), "Basic", new System.Net.NetworkCredential(username, password));
                request.Credentials = myCache;
                request.Headers.Add("Authorization", "Basic " +
                Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(userpwd)));
                //
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "multipart/form-data";
                request.ContentLength = postData.Length;
                outstream = request.GetRequestStream();
                outstream.Write(postData, 0, postData.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        } //发送post请求

        static bool IsThreadStart = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            menuStrip1.Visible = false;
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
                RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
                try 
                { 
                    name.Text = key1.GetValue("name").ToString(); 
                }
                catch { }
                try 
                { 
                    pwd.Text = key1.GetValue("pwd").ToString();
                    if (pwd.Text != "")
                        checkBox1.Checked = true;
                }
                catch { }
                
                try
                {
                    if (key1.GetValue("auto").ToString() == "yes"&&IsThreadStart==false)
                    {
                        RegistryKey run = Registry.CurrentUser;
                        RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                        try
                        {
                            if (autorun.GetValue("PhoneAnywhere").ToString() != "")
                                neta();

                        }
                        catch
                        { }
                        LoginWeibo1(name.Text.ToString(), pwd.Text.ToString());
                        LoginWeibo2(name.Text.ToString(), pwd.Text.ToString());

                        mythread = new Thread(new ThreadStart(DoWork));
                        mythread.Start();
                        IsThreadStart =true;
                        panel1.Hide();
                        menuStrip1.Visible = true;
                        label3.Text = readins(oauth1.oAuthWebRequest(oAuthSina.Method.GET, "http://api.t.sina.com.cn/statuses/user_timeline.xml", String.Empty), "name") + ",已登录";
                        label4.Text = "手机登录新浪微博，即可控制";
                        label5.Text = "操作说明：\n\r关机：关机 gj\n\r锁定：锁定 sd\n\r重启：重启 cq\n\r注销：注销 zx\n\r截屏：截屏 jp\n\r拍照：拍照 pz\n\r迅雷下载：电影 电影名称 最小 最大 格式";
                        
                    }
                }
                catch
                { }
            }
            catch
            
            {
                MessageBox.Show("登录失败");
            }

        }

       

        private string readid()
        {
            string id="";
            return id;
        }

        private string readins(string inxml,string name)
        {
            //string retstr = "";
            try
            {
                string sname = "<" + name + ">";
                string ename = "</" + name + ">";
                int startpos = inxml.IndexOf(sname);
                int endpos = inxml.IndexOf(ename, startpos);
                int strlength = endpos - startpos - sname.Length;
                return inxml.Substring(startpos + sname.Length, strlength);
            }
            catch
            {
                return "error";
            }
        }


        private string read_count(string inxml, string name)
        {
            string retstr = "";
            try
            {
                string sname = "<" + name;
                string ename = "</" + name + ">";
                int startpos = inxml.IndexOf(sname);
                int endpos = inxml.IndexOf(ename, startpos);
                int strlength = endpos - startpos - sname.Length-16;
                return inxml.Substring(startpos + sname.Length+16, strlength);
            }
            catch
            {
                return retstr;
            }
        }
        private string readpin(string inhtml)
        {
            try
            {
                int startpos = inhtml.IndexOf(@"<span class=""fb"">");
                int endpos = inhtml.IndexOf(@"</span>", startpos);
                int strlength = endpos - startpos - 17;
                return inhtml.Substring(startpos + 17, strlength);
            }
            catch
            {
                
                return "error";
            }
        }
        public string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        string since_id = "";
       
        private void DoWork()
        {
            
            int i = 1;
            int flag=1;
            int m = 1;
            while (true)
            {
                try
                {
                    string basic = "http://api.t.sina.com.cn/";
                    string url = "";
                    string command = "";
                    string complete = "命令已完成";
                    string remain_time = "";
                    
                    if (since_id != "")
                        url = basic + "statuses/user_timeline.xml?since_id=" + since_id;
                    else
                        url = basic + "statuses/user_timeline.xml";
                    string retstr = "";
                    if (i % 2 == 1)
                    {
                        retstr=oauth1.oAuthWebRequest(oAuthSina.Method.GET, url, String.Empty);
                        flag = 1;
                    }
                    else
                    {
                        retstr=oauth2.oAuthWebRequest(oAuthSina.Method.GET, url, String.Empty);
                        flag = 0;
                    }
                    since_id = readins(retstr, "id");
                    command = readins(retstr, "text");
                    try
                    {
                        if (command == "sd" || command == "锁定")
                        {
                            LockWorkStation();
                            if (flag == 1)
                                oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete+"：已锁定"+m));
                            if (flag == 0)
                                oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已锁定"+m));
                        }
                        else if (command == "gj" || command == "关机")
                        {
                            shutdownpc();
                            if (flag == 1)
                                oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已关机"+m));
                            if (flag == 0)
                                oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已关机"+m));
                        }
                        else if (command == "zx" || command == "注销")
                        {
                            logoutpc();
                            if (flag == 1)
                                oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已注销"+m));
                            if (flag == 0)
                                oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已注销"+m));
                        }
                        else if (command == "cq" || command == "重启")
                        {
                            restart();
                            if (flag == 1)
                                oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已重启" + m));
                            if (flag == 0)
                                oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：已重启" + m));
                        }
                        else if (command == "jp" || command == "截屏")
                        {
                            scshot();
                            if (flag == 1)
                            {
                                oauth1.oAuthWebRequestWithPic(oAuthSina.Method.POST, uploadurl, "status=" + HttpUtility.UrlEncode("已截屏"+m), "c:\\1.jpeg");
                                //oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "4"));
                            }
                            if (flag == 0)
                            {
                                oauth2.oAuthWebRequestWithPic(oAuthSina.Method.POST, uploadurl, "status=" + HttpUtility.UrlEncode("已截屏"+m), "c:\\1.jpeg");
                                //oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "4"));
                            }
                        }
                        else if (command == "pz" || command == "拍照")
                        {
                            campic c = new campic();
                            c.Show();
                            if (flag == 1)
                            {
                                oauth1.oAuthWebRequestWithPic(oAuthSina.Method.POST, uploadurl, "status=" + HttpUtility.UrlEncode("已拍照"+m), "c:\\a.jpg");
                                //oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "5"));
                            }
                            if (flag == 0)
                            {
                                oauth2.oAuthWebRequestWithPic(oAuthSina.Method.POST, uploadurl, "status=" + HttpUtility.UrlEncode("已拍照"+m), "c:\\a.jpg");
                                //oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "5"));
                            }
                        }
                        else if ((command[0] == '电' && command[1] == '影') || (command[0] == 'd' && command[1] == 'y'))
                        {
                            string[] sarray = command.Split(' ');
                            string moviename = sarray[1];
                            int min = int.Parse(sarray[2]);
                            int max = int.Parse(sarray[3]);
                            string gs = sarray[4];
                            gs = gs.ToUpper();
                            DownLoadByThunder(moviename, min, max, gs,flag,m);
                            //if (flag == 1)
                            //    oauth1.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：正在下载"+m));
                            //if (flag == 0)
                            //    oauth2.oAuthWebRequest(oAuthSina.Method.POST, updateurl, "status=" + HttpUtility.UrlEncode(complete + "：正在下载"+m));
                        }
                    }
                    catch { }
                    Thread.Sleep(6000);
                    if(flag==1)
                        remain_time = read_count(oauth1.oAuthWebRequest(oAuthSina.Method.GET, "http://api.t.sina.com.cn/account/rate_limit_status.xml", String.Empty), "remaining-hits");
                    if(flag==0)
                        remain_time = read_count(oauth2.oAuthWebRequest(oAuthSina.Method.GET, "http://api.t.sina.com.cn/account/rate_limit_status.xml", String.Empty), "remaining-hits");
                    if (int.Parse(remain_time) < 15)
                        i++;
                    m++;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            }
            

        }


        //oAuthSina oauth = new oAuthSina();
        oAuthSina oauth1 = new oAuthSina();
        oAuthSina oauth2 = new oAuthSina();
        int LoginWeibo1(string username,string password)
        {
            oauth1._appKey = "2265546054";
            oauth1._appSecret = "f075616f65ea34d0d4bbff0960ae39e6";
            oauth1.RequestTokenGet();
            //string authlink = oauth1.AuthorizationGet();

            
            //string password = "woshishen";
            string content = "userId=" + username + "&passwd=" + password + "&oauth_callback=none" + "&action=submit" + "&from=" + "null" + "&oauth_token=" + oauth1.token;
            string restr = GetPage("http://api.t.sina.com.cn/oauth/authorize?oauth_token=" + oauth1.token, content);
            string pin = readpin(restr);
            //if (pin == "error")
            //{
               // return 0;
            //}
            //else
            //{
                //oauth1.Verifier = pin;
                oauth1.AccessTokenGet();
                return 1;
            //}
        }

        int LoginWeibo2(string username,string password)
        {
            oauth2._appKey = "649187664";
            oauth2._appSecret = "fbb5f21ccea15b45f43b6faa09a8ebd2";
            oauth2.RequestTokenGet();
            string authlink = oauth2.AuthorizationGet();

           
            string content = "userId=" + username + "&passwd=" + password + "&oauth_callback=none" + "&action=submit" + "&from=" + "null" + "&oauth_token=" + oauth2.token;
            string restr = GetPage("http://api.t.sina.com.cn/oauth/authorize?oauth_token=" + oauth2.token, content);
            string pin = readpin(restr);
            //if (pin == "error")
            //{
                return 0;
            //}
            //else
            //{
                oauth2.Verifier = pin;
                oauth2.AccessTokenGet();
                return 1;
            //}
        }

        Thread mythread;
        private void RunsOnWorkerThread()
        {

            MethodInvoker mi = new MethodInvoker(ViewLabel);
            mi.BeginInvoke(null, null);

        }
        void ViewLabel()
        {
            label6.Text = "登录中……";
        }
        private void button12_Click(object sender, EventArgs e)
        {
            RegistryKey run = Registry.CurrentUser;
            RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            try
            {
                if (autorun.GetValue("PhoneAnywhere").ToString() != "")
                    neta();

            }
            catch
            { }
            
            label6.Text = "登录中……";
            Application.DoEvents();
            
            
            RegistryKey key = Registry.LocalMachine;
            RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
            RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
            key1.SetValue("name", name.Text);
            if (checkBox1.Checked == true)
            {
                key1.SetValue("pwd", pwd.Text);
                //key1.SetValue("c1", "1");
            }
            //if (checkBox2.Checked == true)
            //{
            //    RegistryKey run = Registry.CurrentUser;
            //    RegistryKey autorun = run.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            //    autorun.SetValue("PhoneAnywhere", Application.ExecutablePath);

            //    key1.SetValue("c2", "1");
            //    key1.SetValue("auto", "yes");
            //}
            int l1=LoginWeibo1(name.Text.ToString(),pwd.Text.ToString());
            //int l2=LoginWeibo2(name.Text.ToString(),pwd.Text.ToString());
            if(l1==1&&IsThreadStart==false)
            {
            
                string tt = readins(oauth1.oAuthWebRequest(oAuthSina.Method.GET, "http://api.t.sina.com.cn/statuses/user_timeline.xml", String.Empty), "name") + ",已登录";

                label3.Text = tt;
                mythread = new Thread(new ThreadStart(DoWork));
                mythread.Start();
                IsThreadStart = true;
                panel1.Hide();
                menuStrip1.Visible = true;
                label4.Text = "手机登录新浪微博，即可控制";
                label5.Text = "操作说明：\n\r关机：关机 gj\n\r锁定：锁定 sd\n\r重启：重启 cq\n\r注销：注销 zx\n\r截屏：截屏 jp\n\r拍照：拍照 pz\n\r迅雷下载：电影 电影名称 最小 最大 格式";
                
            }
            else
            {
                MessageBox.Show("用户名或密码错误！错误代码：101\n\r请重新打开本软件再登录！");
            }
        }

        private void button12_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { mythread.Abort(); }
            catch
            { }
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mythread.Abort();
            }
            catch
            { }
            Application.Exit();
        }

        

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
                RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
                key1.SetValue("pwd", "");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            mythread.Abort();
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
           
            RegistryKey key = Registry.LocalMachine;
            RegistryKey soft = key.OpenSubKey("SOFTWARE", true);
            RegistryKey key1 = soft.CreateSubKey("PhoneAnywhere");
            key1.SetValue("pwd", "");
            key1.SetValue("c1", "0");
            key1.SetValue("name", "");
            key1.SetValue("c2", "");
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate(); 
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = false; 
        }

        private void button12_MouseClick(object sender, MouseEventArgs e)
        {
            button12.Text = "登录中……";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://hi.baidu.com/geekgeek/blog");
        }

        private void 基本设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings st = new settings();
            st.Show();
            st.panel1.Visible = false;
            st.panel2.Visible = true;
        }

        private void 账号设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings st = new settings();
            st.Show();
            st.panel1.Visible = true;
            st.panel2.Visible = false;
            
        }

        private void 退出ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mythread.Abort();
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void 开发者博客ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://hi.baidu.com/geekgeek/blog");
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本软件纯属个人兴趣，难免存在缺陷，谢谢您的支持！");
        }

       

      

        

       

       
    }
}
