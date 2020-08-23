using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;

namespace DrComDotnet
{
    using uint8 = System.Byte;

    //小工具
    static class Utils
    {
        static public void printBytesHex(byte[] bytes,string name = "Hex")
        {
            Console.Write("[{0} {1,2:D}] ",name,bytes.Length);
            foreach(byte i in bytes)
            {
                Console.Write("{0,2:X2} ",i);
            }
            Console.WriteLine();
        }

        //将bytes进行连接
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

            //定义溢出异常,没太有必要,只是用来学习
            public class BytesLinkOverflowException: ApplicationException
            {   
                public BytesLinkOverflowException(string message): base(message)
                {
                }
            }

            public void AddBytes(byte[] src)
            {
                //判断是否溢出
                if(offset + src.Length > bytesLength)
                {
                    throw new BytesLinkOverflowException($"offset={offset},bytesLength={bytesLength},src.Length={src.Length}");
                }

                //连接并偏移
                src.CopyTo(bytes, offset);
                offset += src.Length;
            }

            //连接并检验offset
            public void AddBytes(byte[] src, int assertOffset)
            {
                AddBytes(src);
                Debug.Assert(offset == assertOffset);
            }
        }
    }

    //设置
    class Settings
    {
        public string    userName    { get; set; }
        public string    passWord    { get; set; }
        public byte[]    macAddress  { get; set; }
        public IPAddress serverIP    { get; set; }
        public IPAddress userIP      { get; set; }
        public IPAddress userDNS     { get; set; }
        
        public string   serverHost      = "auth.jlu.edu.cn";
        public string   defaultServerIp = "10.100.61.3";

        // TODO: 
        public bool check()
        {
            return userName.Length <= 36
                && macAddress.Length == 6
            ;
        }

        public Settings()
        {
            //尝试用DNS获取认证服务器IP
            try
            {
                serverIP = Dns.GetHostAddresses(serverHost)[0];
            }
            catch(System.Net.Sockets.SocketException socketException)
            {  
                Console.WriteLine($"Can not find serverIP via DNS, using {defaultServerIp}");
                serverIP = IPAddress.Parse(defaultServerIp);
                Debug.WriteLine(socketException);
            }
        }
    }

    // 握手,即协议分析中的Challenge
    // args socket,setting
    // usage: new-> handShake
    class Handshaker
    {

        private uint8 challenge_times = 0x02;
        private Socket socket;
        private Settings settings;

        // 用于packetBuild
        Random randomBuilder = new Random();

        // 构建握手需要的包
        private byte[] packetBuild(uint8 challenge_times)
        {
            // 四部分组成 packet(20B) = begin(1B) + times(1B) + rand(2B) + end(17B)
            byte[] begin = new byte[] {0x01};
            byte[] rand  = new byte[2];
            randomBuilder.NextBytes(rand);
            byte[] times = {challenge_times};
            byte[] end   = new byte[] { 0x6a,
                0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00
            };

            //连接
            byte[] packet = begin.Concat(rand).Concat(times).Concat(end).ToArray();

            //确保长度为20
            Debug.Assert(packet.Length == 20, $"Incorrect Packet Length: {packet.Length}");

            return packet;
        }

        // 握手,返回salt和客户端ip
        public Tuple<byte[], IPAddress> handShake()
        {
            //构建握手包
            byte[] packet = packetBuild(challenge_times);
            Utils.printBytesHex(packet,"packet");

            //发送
            socket.SendTo(
                packet,
                0,
                20,
                SocketFlags.None,
                new IPEndPoint(settings.serverIP,61440)
            );

            //接收服务器返回消息
            byte[] recv = new byte[76];
            socket.Receive(recv);
            Utils.printBytesHex(recv,"handshake recv");

            //切出salt和客户端 IP 地址
            byte[] salt          = recv[4..8];
            byte[] clinetIPBytes = recv[20..24];
            IPAddress clinetIP = new IPAddress(clinetIPBytes);

            //输出测试
            Utils.printBytesHex(salt,"salt");
            Utils.printBytesHex(clinetIPBytes,"clinetIPBytes");

            //校验随机位
            Debug.Assert(recv[2..3] == packet[2..3]);

            return new Tuple<byte[], IPAddress>(salt,clinetIP);
        }

        //初始化
        public Handshaker(Socket socket, Settings settings)
        {
            //赋值
            this.socket   = socket;
            this.settings = settings;
        }
    }

    //登录器
    class Logger
    {
        public Settings settings;
        public Socket   socket;
        public byte[]   salt;       // 盐

        public byte[] packetBuild(int packetLength, byte[] salt)
        {
            //起个别名，方便阅读。getBytes = Encoding.Default.GetBytes
            Func<string,byte[]> getBytes = Encoding.Default.GetBytes;
            // 获取其他参数 username, password, mac,并转换成byte[]
            byte[] userName   = getBytes(settings.userName);
            byte[] passWord   = getBytes(settings.passWord);
            byte[] macAddress = settings.macAddress;

            //接下来了才是重点,伙计!
            //按照模板(https://github.com/drcoms/jlu-drcom-client/blob/master/jlu-drcom-java/jlu-drcom-protocol.md)构建packet.由于长度不固定,代码必须一点点写,所以非常难看
            //这里使用了一个自己定义的类用来方便的连接字符串。如果有内置类当然就是白忙活了
            var packet = new Utils.BytesLinker(packetLength);

            //一个packet中有很多参数(以t开头进行区分),一一计算拼接
            //前4个固定的packet参数
            const byte tCode= 0x03;
            const byte tType= 0x01;
            const byte tEof = 0x00;
            byte       tUsrLen = (byte) (userName.Length + 20);

            //其他几个固定参数
            const byte tControlCheck = 0x20;
            const byte tAdapterNum    = 0x05;

            //计算md5a
            MD5 md5Builder = new MD5CryptoServiceProvider();
            byte[] tMd5a = md5Builder.ComputeHash(
                new byte[]{0x03,0x01}
                    .Concat(salt)
                    .Concat(passWord)
                    .ToArray()
            );
            Utils.printBytesHex(tMd5a,"tMd5a");
            
            //计算xor = md5[0..6] ^ mac
            //太累了,改天吧
            
            
            //计算uname 用户名左对齐末尾补 0 凑 36 长度
            byte[] tUname = new byte[36];
            userName.CopyTo(tUname,0);

            packet.AddBytes(new byte[] {
                tCode, tType, tEof, tUsrLen
            });
            packet.AddBytes(tMd5a,  21);
            packet.AddBytes(tUname, 56);
            packet.AddBytes(new byte[] {
                tControlCheck, tAdapterNum
            });

            return packet.bytes;

        }
        public void login()
        {
            //计算packet长度
            //t 表示意义不明的临时变量.协议描述中为 x / 4 * 4,等于x - x % 4
            int t0 = (settings.passWord.Length>16)? 16 : settings.passWord.Length - 1;
            int packetLength = 334 + t0 - t0 % 4;

            //构建packet
            byte[] packet = packetBuild(packetLength,salt);

            //进行通信
        }

        public Logger(Socket socketArg, Settings settingsArg)
        {
            //赋值
            socket   = socketArg;
            settings = settingsArg;
        }
    }

    //KeepAliver
    class KeepAliver
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //流程 握手->登录->KeepAlive

            //初始化设置
            Settings settings   = new Settings();
            settings.userName   = "XXXXX";
            settings.passWord   = "XXXXX";
            settings.macAddress = new byte[]{0x01, 0x03, 0x05, 0x01, 0x03, 0x05};
            Debug.Assert(settings.check());

            //初始化socket(UDP报文形式的SOCKET)
            Socket      socket   = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp); 
            IPAddress   bindIP     = IPAddress.Parse("0.0.0.0");
            IPEndPoint  bindIpPort = new IPEndPoint(bindIP, 61440);
            socket.Bind(bindIpPort);
            socket.SendTimeout = 3000;
            
            //握手
            Handshaker handshaker = new Handshaker(socket,settings);
            var (salt,clinetIP) = handshaker.handShake();



            //清理
            socket.Close();
        }
    }
}
