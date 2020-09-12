using System;
using System.Net;
using System.Runtime.InteropServices;


namespace DrComDotnet
{
    //小工具
    static class Utils
    {
        
        // 将Bytes按16进制输出
        static public void printBytesHex(byte[] bytes,string name = "Hex", int logLevel = 2)
        {
            if(logLevel < 2)
                return ;
    
            Console.Write("[{0} {1,2:D}] ",name,bytes.Length);
            foreach(byte i in bytes)
            {
                Console.Write("{0,2:X2} ",i);
            }
            Console.WriteLine();
        }
        
        // 传参时, logLevel+1 表示更倾向输出,-1表示更不倾向输出
        static public void log(string info, int logLevel = 2)
        {
            if(logLevel >= 2)
                Console.WriteLine(info);
        }
    
    //也是醉了
    #if IS_WINDOWS
        [DllImport("msvcrt.dll")]
        public extern static int system(string command);

        public static bool connectWifi(string ssid, bool isDebug = false)
        {
            string cmd = $"netsh wlan connect name=\"{ssid}\"";
            if(isDebug)
            {
                Console.WriteLine("执行: ",cmd);
            }
            int result = system(cmd);
            return result == 0;
        }
    #else
        public static bool connectWifi(string ssid, bool isDebug = false)
        {
            if(isDebug)
                throw new ApplicationException("Only for Windows!");
            log("Connet Wifi only available for Windows");
            return false;
        }
        public static int system(string command)
        {
            throw new ApplicationException("msvcrt.dll only available for Windows");
        }
    #endif

    // 匹配地址,NET48需要现写
    #if NETFRAMEWORK      
        public static IPEndPoint ParseIPEndpoint(string endPoint)
        {
        
            string[] ep = endPoint.Split(':');
            if(ep.Length != 2) 
                throw new FormatException("Invalid ipendpoint format");
            IPAddress ip = IPAddress.Parse(ep[0]);
            int port     = Convert.ToInt32(ep[1]);
            return new IPEndPoint(ip, port);
       
        }
    #else
        public static IPEndPoint ParseIPEndpoint(string endPoint)
            => IPEndPoint.Parse(endPoint);
    #endif

        //将bytes进行连接的一个类
        public class BytesLinker
        {
            public  byte[] bytes       {get; private set; }
            public  int    bytesLength {get; private set; }
            public  int    offset      {get; private set; }      //偏移量，第一个未填充的字符的下标

            //初始化
            public BytesLinker(int bytesLength)
            {
                this.bytes       = new byte[bytesLength];
                this.bytesLength = bytesLength;
                offset           = 0;
            }
            public void AddBytes(byte[] src)
            {
                //判断是否溢出
                if(offset + src.Length > bytesLength)
                {
                    throw new ApplicationException($"offset={offset},bytesLength={bytesLength},src.Length={src.Length}");
                }

                //连接并偏移
                src.CopyTo(bytes, offset);
                offset += src.Length;
            }
            //连接并检验offset
            public void AddBytes(byte[] src, int assertOffset)
            {
                AddBytes(src);
                if(offset != assertOffset)
                {
                    Console.WriteLine($"错误,packet长度与预期偏移不符合! 预期:{assertOffset} 实际:{offset}");
                    throw new ApplicationException();
                }
            }
            //添加一个byte
            public void AddByte(byte src)
            {
                //判断是否溢出
                if(offset + 1 > bytesLength)
                {
                    throw new ApplicationException($"offset={offset},bytesLength={bytesLength},src.Length={1}");
                }
                //连接并偏移
                bytes[offset] = src;
                offset++;
            }
            
            //重载切片
            public byte[] this[Range r]
            {
                get { return bytes[r]; }
            }
        }

    }



    //JSON反序列化类
    public class JsonOptionsModel
    {
        public int        version    {get; set;}
        public Debug      debug      {get; set;}
        public User       user       {get; set;}
        public AuthServer authServer {get; set;}
        public Misc       misc       {get; set;}
        
        //具体的实现
        public class Debug 
        {
            public bool   enabled     {get; set;}
            public int    sendTimeout {get; set;}
            public int    recvTimeout {get; set;}
            public string bindAddress {get; set;}
            public int    logLevel    {get; set;}
            public int    wifiDelay   {get; set;}

        }
        public class User
        {
            public string name      {get; set;}
            public string password  {get; set;}
            public string mac       {get; set;}
            public string ip        {get; set;}
            public string dns       {get; set;}
            public string hostName  {get; set;}
            public bool   randomMac {get; set;}
        }
        public class AuthServer
        {
            public string host  {get; set;}
            public string ip    {get; set;}
            public int    port  {get; set;}
            public bool useDNS  {get; set;}
        }
        public class Misc 
        {
            public bool   autoConnectWifi {get; set;}
            public int    wifiDelay       {get; set;}
            public string authWifi        {get; set;}
            public bool   showWelcome     {get; set;}
            public string welcomeUrl      {get; set;}
        }

        public JsonOptionsModel()
        {
            debug = new Debug {
                enabled     = false,
                bindAddress = "0.0.0.0:61440",
                logLevel    = 1,
                recvTimeout = 3000,
                sendTimeout = 3000,
                wifiDelay   = 3000
            };
            user  = new User {
                name = "XXXXX",
                password = "XXXXX",
                ip = "DHCP",
                dns = "10.10.10.10",
                hostName = "NORMAL-1XPC3",
                randomMac = true,
                mac = "00-00-00-00-00-00"
            };
            authServer  = new AuthServer {
                host = "auth.jlu.edu.cn",
                ip   = "10.100.61.3",
                port = 61440,
                useDNS = true
            };
            misc  = new Misc {
                autoConnectWifi = false,
                wifiDelay       = 5000,
                authWifi        = "JLU.PC",
                showWelcome     = false,
                welcomeUrl      = "http://login.jlu.edu.cn/notice.php"
            };
        }        
    }

}