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
                if(offset != assertOffset)
                {
                    Console.WriteLine($"错误,packet长度与预期偏移不符合! 预期:{assertOffset} 实际:{offset}");
                    throw new Exception();
                }
            }

            //添加一个byte
            public void AddByte(byte src)
            {
                //判断是否溢出
                if(offset + 1 > bytesLength)
                {
                    throw new BytesLinkOverflowException($"offset={offset},bytesLength={bytesLength},src.Length={1}");
                }
                //连接并偏移
                bytes[offset] = src;
                offset++;
            }
            
            //重载切片，用于练习
            public byte[] this[Range r]
            {
                get { return bytes[r]; }
            }
        }
    }

    //设置
    class Settings
    {
        public string    userName    { get; set; }
        public string    passWord    { get; set; }
        public string    hostName    { get; set; }
        public byte[]    macAddress  { get; set; }
        public IPAddress serverIP    { get; set; }
        public IPAddress userIP      { get; set; }  //可能没用
        public IPAddress userDNS     { get; set; }  //可能没用
        public IPAddress primaryDNS  { get; set; }
        public IPAddress handShakeIP { get; set; }  //handShake(challenge)返回的IP
        
        public string   serverHost      = "auth.jlu.edu.cn";
        public string   defaultServerIp = "10.100.61.3";
        public byte[]   salt;

        public void loadFromJsonFile(string filePath)
        {
            //先画个大饼
        }

        // TODO: 
        public bool check()
        {
            return userName.Length   <= 36
                && macAddress.Length == 6
                && hostName.Length   <= 32
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

            //设置默认值
            primaryDNS = IPAddress.Parse("10.10.10.10");
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
            Debug.Assert(recv[2] == packet[2] && recv[3] == packet[3]);

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

        public byte[] packetBuild(int packetLength)
        {
            //起个别名，方便阅读。getBytes = Encoding.Default.GetBytes
            Func<string,byte[]> getBytes = Encoding.Default.GetBytes;

            // 获取其他参数 username, password, mac,并转换成byte[]
            byte[] salt       = settings.salt;
            byte[] userName   = getBytes(settings.userName);
            byte[] passWord   = getBytes(settings.passWord);
            byte[] macAddress = settings.macAddress;
            byte[] hostName   = getBytes(settings.hostName);
            byte[] primaryDNS = settings.primaryDNS.GetAddressBytes();

            //接下来了才是重点,伙计!
            //按照模板(https://github.com/drcoms/jlu-drcom-client/blob/master/jlu-drcom-java/jlu-drcom-protocol.md)构建packet.由于长度不固定,代码必须一点点写,所以非常难看
            //这里使用了一个自己定义的类用来方便的连接字符串。如果有内置类当然就是白忙活了
            //还有,由于有些参数必须在连接一部分后才能计算,所以分两次连接
            var packet = new Utils.BytesLinker(packetLength);

            //一个packet中有很多参数(以t开头进行区分),一一计算拼接
            //前4个固定的packet参数。
            const byte tCode= 0x03;
            const byte tType= 0x01;
            const byte tEof = 0x00;
            byte       tUsrLen = (byte) (userName.Length + 20);

            //其他几个固定参数
            const byte tControlCheck = 0x20;
            const byte tAdapterNum   = 0x05;
            const byte tIPDog        = 0x01;

            //计算md5a
            MD5 md5Builder = new MD5CryptoServiceProvider();
            byte[] tMd5a = md5Builder.ComputeHash(
                new byte[]{tCode,tType}
                    .Concat(salt)
                    .Concat(passWord)
                    .ToArray()
            );
            Utils.printBytesHex(tMd5a,"tMd5a");
            
            //计算md5b
            byte[] tMd5b = md5Builder.ComputeHash(
                new byte[]{0x01}
                    .Concat(passWord)
                    .Concat(salt)
                    .Concat(new byte[4] {0x00,0x00,0x00,0x00})
                    .ToArray()
            );

            // 计算xor = md5a[0..6] ^ mac
            // 由于移位运算符仅针对 int、uint、long 和 ulong 类型定义。如果左侧操作数是其他整数类型，则其值将转换为 int 类型
            // WTF.
            byte[] tXor = tMd5a[0..6].Zip(macAddress, (a,b) => (byte) (a ^ b)).ToArray();
            Utils.printBytesHex(tXor,"tXor");

            // 计算uname 用户名左对齐末尾补 0 凑 36 长度
            byte[] tUname = new byte[36];  //TODO 手动填0
            userName.CopyTo(tUname,0);

            //生成IP部分
            const byte tIPNum  = 0x01; //对应numOfIP
            byte[] tIP1        = settings.handShakeIP.GetAddressBytes();
            byte[] tIP2        = new byte[4] {0x00,0x00,0x00,0x00};
            byte[] tIP3        = new byte[4] {0x00,0x00,0x00,0x00};
            byte[] tIP4        = new byte[4] {0x00,0x00,0x00,0x00};
 
            //第一次连接
            packet.AddBytes( new byte[] {
                tCode, tType, tEof, tUsrLen  });
            packet.AddBytes(tMd5a,  20);
            packet.AddBytes(tUname, 56);
            packet.AddBytes( new byte[] {
                tControlCheck, tAdapterNum   });
            packet.AddBytes(tXor,   64);
            packet.AddBytes(tMd5b,  80);
            packet.AddByte (tIPNum);
            packet.AddBytes(tIP1);
            packet.AddBytes(tIP2);
            packet.AddBytes(tIP3);
            packet.AddBytes(tIP4,   97);

            //继续计算
            //计算md5c
            byte[] tMd5c = md5Builder.ComputeHash(
                packet[0..98]
                .Concat(new byte[] {0x14,0x07,0x00,0x0b})
                .ToArray()
            )[0..8]; //TODO: 使用引用的方式减小内存占用 类似于 ref packet[0..98]

            //对齐hostname
            byte[] tHostName = new byte[32]; //TODO: 手动补0
            hostName.CopyTo(tHostName, 0);

            //构建DHCP,两个DNS
            byte[] tPrimaryDNS   = primaryDNS;
            byte[] tSecondaryDNS = {0x00, 0x00, 0x00, 0x00};
            byte[] tDHCP         = {0x00, 0x00, 0x00, 0x00};

            //未知的固定部分
            //前面是操作系统版本之类的,以后记得改写
            byte[] tUnknownFixed = new byte[] {
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0xf0, 0x23, 0x00, 0x00, 0x02, 0x00,
                0x00, 0x00, 0x44, 0x72, 0x43, 0x4f, 0x4d, 0x00, 0xcf, 0x07,
                0x6a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            //str有很多版本,以后抓包看看
            byte[] tUnknownStr  = getBytes("1c210c99585fd22ad03d35c956911aeec1eb449b");

            //第二次连接
            packet.AddBytes(tMd5c);
            packet.AddBytes(new byte[] { tIPDog, 0x00, 0x00, 0x00, 0x00 }, 110);
            packet.AddBytes(tHostName, 142);
            packet.AddBytes(tPrimaryDNS);
            packet.AddBytes(tDHCP);
            packet.AddBytes(tSecondaryDNS, 154);
            packet.AddBytes(tUnknownFixed, 246);
            packet.AddBytes(tUnknownStr,   286);

            return packet.bytes;

        }
        public void login()
        {
            //计算packet长度
            //t 表示意义不明的临时变量.协议描述中为 x / 4 * 4,等于x - x % 4
            int t0 = (settings.passWord.Length>16)? 16 : settings.passWord.Length - 1;
            int packetLength = 334 + t0 - t0 % 4;

            //构建packet
            byte[] packet = packetBuild(packetLength);
            Utils.printBytesHex(packet);

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
            settings.hostName   = "LENNOVE";
            settings.macAddress = new byte[]{0x01, 0x03, 0x05, 0x01, 0x03, 0x05};
            Debug.Assert(settings.check());

            //初始化socket(UDP报文形式的SOCKET)
            Socket      socket     = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp); 
            IPAddress   bindIP     = IPAddress.Parse("0.0.0.0");
            IPEndPoint  bindIpPort = new IPEndPoint(bindIP, 61440);
            socket.Bind(bindIpPort);
            socket.SendTimeout     = 3000;
            
            //握手
            Handshaker handshaker        = new Handshaker(socket,settings);
            var (salt,handShakeClinetIP) = handshaker.handShake();
            settings.handShakeIP         = handShakeClinetIP;
            settings.salt                = salt;

            //登录
            Logger logger = new Logger(socket,settings);
            logger.login();


            //清理
            socket.Close();
        }
    }
}
