    //JSON反序列化类
namespace configer
{
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
            public string bindIP      {get; set;}
            public int    logLevel    {get; set;}
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
            public bool autoConnectWifi {get; set;}
            public string authWifi      {get; set;}
        }

        public JsonOptionsModel()
        {
            debug = new Debug {
                enabled     = false,
                bindIP      = "0.0.0.0",
                logLevel    = 1,
                recvTimeout = 3000,
                sendTimeout = 3000
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
                authWifi = "JLU.PC"
            };
        }
    }
}