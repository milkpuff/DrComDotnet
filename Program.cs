�?/*
DrComDotnet - JLU DrCom Clinet written in C#
coding:   UTF-8
csharp:   8
dotnet:   Dotnet Core 3
version:  0.0.2
codename: Still a Flower Bud (仍是花蕾)

Inspired by newclinet.py(zhjc1124) and jlu-drcom-protocol(YouthLin).
*/

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using System.Numerics;

namespace DrComDotnet
{
    using uint8 = System.Byte;

    //小工�?
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

            //初始�?
            public BytesLinker(int bytesLength)
            {
                this.bytes       = new byte[bytesLength];
                this.bytesLength = bytesLength;
                offset           = 0;
            }

            //定义溢出异常,没太有必�?,只是用来学习
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

                //连接并偏�?
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

            //添加�?个byte
            public void AddByte(byte src)
            {
                //判断是否溢出
                if(offset + 1 > bytesLength)
                {
                    throw new BytesLinkOverflowException($"offset={offset},bytesLength={bytesLength},src.Length={1}");
                }
                //连接并偏�?
                bytes[offset] = src;
                offset++;
            }
            
            //重载切片，用于练�?
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
        public IPEndPoint serverIPEndPoint { get; set; }
        
        public string   serverHost      = "auth.jlu.edu.cn";
        public string   defaultServerIp = "10.100.61.3";
        public byte[]   salt;

        public void loadFromJsonFile(string filePath)
        {
            //先画个大�?
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

            //设置默认�?
            primaryDNS = IPAddress.Parse("10.10.10.10");
            serverIPEndPoint = new IPEndPoint(serverIP, 61440);
        }
    }

    // 握手,即协议分析中的Challenge
    // args socket,setting
    // usage: new -> handShake
    class Handshaker
    {

        private uint8 challenge_times = 0x02;
        private Socket socket;
        private Settings settings;

        // 用于packetBuild
        Random randomBuilder = new Random();

        // 构建握手�?要的�?
        private byte[] packetBuild(uint8 challenge_times)
        {
            // 四部分组�? packet(20B) = begin(1B) + times(1B) + rand(2B) + end(17B)
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

            //确保长度�?20
            Debug.Assert(packet.Length == 20, $"Incorrect Packet Length: {packet.Length}");

            return packet;
        }

        // 握手,返回salt和客户端ip
        public Tuple<byte[], IPAddress> handShake()
        {
            //构建握手�?
            byte[] packet = packetBuild(challenge_times);
            Utils.printBytesHex(packet,"packet");

            //发�??
            socket.SendTo(
                packet,
                0,
                20,
                SocketFlags.None,
                settings.serverIPEndPoint
            );

            //接收服务器返回消�?
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

            //校验随机�?
            Debug.Assert(recv[2] == packet[2] && recv[3] == packet[3]);

            return new Tuple<byte[], IPAddress>(salt,clinetIP);
        }

        //初始�?
        public Handshaker(Socket socket, Settings settings)
        {
            //赋�??
            this.socket   = socket;
            this.settings = settings;
        }
    }

    //登录�?
    class Logger
    {
        public Settings settings;
        public Socket   socket;
        
        //packetBuild的辅助函�?,用来计算协议中的ror
        public byte[] packetBuildCalculateRor(byte[] md5a, byte[] password) 
        {
            byte[] ret = new byte[password.Length];
            byte t;
            for (int i = 0; i < password.Length; i++) 
            {
                t      = (byte) ( md5a[i] ^ password[i] );
                ret[i] = (byte) ( (t << 3) & 0xFF + (t >> 5) );
                //& 0xFF: C#不能直接对byte位运�?,�?要先拓宽为int,�?以用& 0xFF来只保留�?8�?
            }
            return ret;
        }

        public byte[] packetBuildCalculateChecksum(byte[] packetPiece)
        {
            // TODO
            byte[] data = packetPiece
                .Concat(new byte[]{ 0x01, 0x26, 0x07, 0x11, 0x00, 0x00 })
                .Concat(settings.macAddress)
                .ToArray();
            // 1234 = 0x_00_00_04_d2
            byte[] sum = new byte[]{0x00, 0x00, 0x04, 0xd2};
            int len = data.Length;
            int i = 0;
            //0123_4567_8901_23
            for (; i + 3 < len; i = i + 4) {
                //abcd ^ 3210
                //abcd ^ 7654
                //abcd ^ 1098
                sum[0] ^= data[i + 3];
                sum[1] ^= data[i + 2];
                sum[2] ^= data[i + 1];
                sum[3] ^= data[i];
            }
            if (i < len) {
                //剩下_23
                //i=12,len=14
                byte[] tmp = new byte[4];
                for (int j = 3; j >= 0 && i < len; j--) {
                    //j=3 tmp = 0 0 0 2  i=12  13
                    //j=2 tmp = 0 0 3 2  i=13  14
                    tmp[j] = data[i++];
                }
                for (int j = 0; j < 4; j++) {
                    sum[j] ^= tmp[j];
                }
            }
            BigInteger bigInteger =  new BigInteger(sum);
            bigInteger   = bigInteger * (new BigInteger(1968));
            bigInteger   = bigInteger & (new BigInteger(0xff_ff_ff_ffL));
            byte[] bytes = bigInteger.ToByteArray();
            len          = bytes.Length;
            i = 0;
            byte[] ret   = new byte[4];
            for (int j   = len - 1; j >= 0 && i < 4; j--) {
                ret[i++] = bytes[j];
            }
            return ret;
        } 

        //构建请求�?
        public byte[] packetBuild(int packetLength)
        {
            //起个别名，方便阅读�?�getBytes = Encoding.Default.GetBytes
            Func<string,byte[]> getBytes = Encoding.Default.GetBytes;

            // 获取其他参数 username, password, mac,并转换成byte[]
            byte[] salt       = settings.salt;
            byte[] userName   = getBytes(settings.userName);
            byte[] passWord   = getBytes(settings.passWord);
            byte[] macAddress = settings.macAddress;
            byte[] hostName   = getBytes(settings.hostName);
            byte[] primaryDNS = settings.primaryDNS.GetAddressBytes();

            //接下来了才是重点,伙计!
            //按照模板(https://github.com/drcoms/jlu-drcom-client/blob/master/jlu-drcom-java/jlu-drcom-protocol.md)构建packet.由于长度不固�?,代码必须�?点点�?,�?以非常难�?
            //这里使用了一个自己定义的类用来方便的拼接字符串�?�如果有内置类当然就是白忙活�?
            //还有,由于有些参数必须在拼接一部分后才能计�?,�?以分三次拼接
            var packet = new Utils.BytesLinker(packetLength + 32); //由于�?0的奇怪算�?

            //�?个packet中有很多参数(以t�?头进行区�?),�?�?计算拼接
            //�?4个固定的packet参数�?
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
            // 由于移位运算符仅针对 int、uint、long �? ulong 类型定义。如果左侧操作数是其他整数类型，则其值将转换�? int 类型
            // WTF.
            byte[] tXor = tMd5a[0..6].Zip(macAddress, (a,b) => (byte) (a ^ b)).ToArray();
            Utils.printBytesHex(tXor,"tXor");

            // 计算uname 用户名左对齐末尾�? 0 �? 36 长度
            byte[] tUname = new byte[36];  //TODO 手动�?0
            userName.CopyTo(tUname,0);

            //生成IP部分
            const byte tIPNum  = 0x01; //对应numOfIP
            byte[] tIP1        = settings.handShakeIP.GetAddressBytes();
            byte[] tIP2        = new byte[4] {0x00,0x00,0x00,0x00};
            byte[] tIP3        = new byte[4] {0x00,0x00,0x00,0x00};
            byte[] tIP4        = new byte[4] {0x00,0x00,0x00,0x00};
 
            //第一次拼�?
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
            )[0..8]; //TODO: 使用引用的方式减小内存占�? 类似�? ref packet[0..98]

            //对齐hostname
            byte[] tHostName = new byte[32]; //TODO: 手动�?0
            hostName.CopyTo(tHostName, 0);

            //构建DHCP,两个DNS
            byte[] tPrimaryDNS   = primaryDNS;
            byte[] tSecondaryDNS = {0x00, 0x00, 0x00, 0x00};
            byte[] tDHCP         = {0x00, 0x00, 0x00, 0x00};

            //操作系统版本之类�?,先固定着,以后记得改写
            byte[] tOSInfo = new byte[] {
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0xf0, 0x23, 0x00, 0x00, 0x02, 0x00,
                0x00, 0x00 };
            
            byte[] tDrComCheck = new byte[] { 
                0x44, 0x72, 0x43, 0x4f, 0x4d, 0x00, 0xcf, 0x07, 0x6a
            };

            //固定长度的零字节,tFixed对应协议分析中的 zero[24] �? 6a 00 00
            byte[] tZero55 = new byte[55];
            byte[] tFixed  = new byte[27] {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x6a, 0x00, 0x00
            };

            //str有很多版�?,以后抓包看看
            byte[] tUnknownStr  = getBytes("1c210c99585fd22ad03d35c956911aeec1eb449b");

            //计算ror �? passlen
            int passLen   = (passWord.Length>16)? 16 : passWord.Length;
            byte tPassLen = (uint8) passLen;
            byte[] tRor = packetBuildCalculateRor(tMd5a,passWord);

            //第二次拼�?
            packet.AddBytes(tMd5c);
            packet.AddBytes(new byte[] { tIPDog, 0x00, 0x00, 0x00, 0x00 }, 110);
            packet.AddBytes(tHostName,     142);
            packet.AddBytes(tPrimaryDNS);
            packet.AddBytes(tDHCP);
            packet.AddBytes(tSecondaryDNS, 154);
            packet.AddBytes(tOSInfo);
            packet.AddBytes(tDrComCheck);
            packet.AddBytes(tZero55,       246);
            packet.AddBytes(tUnknownStr,   286);
            packet.AddBytes(tFixed,        313);
            packet.AddByte (tPassLen);
            packet.AddBytes(tRor,          314 + passLen);
            //现在�?2020年八�?25日凌�?0�?,由于宿舍停电,未经调试,紧�?�保存现�?
            
            //计算checksum
            byte[] tCheckSum  = packetBuildCalculateChecksum( packet.bytes[0..(315+passLen)] )[0..4];
            Utils.printBytesHex(tCheckSum);
            byte[] tBeforeCheckSum = new byte[] {
                0x02, 0x0c, 
            };
            byte[] tAfterCheckSum = new byte[] {
                0x00, 0x00
            };
            Utils.printBytesHex(tCheckSum,"tCheckSum");

            //计算tMac
            ref byte[] tMac = ref macAddress;

            // tZeroCount Protocol�?
            // var zeroCount = (4 - passLen % 4) % 4;
            // byte[] tZeroCount = new byte[zeroCount];

            // tZeroCount newclinet.py�?
            var zeroCount = passLen / 4 == 4? 0 : passLen / 4; // Weird...
            byte[] tZeroCount = new byte[zeroCount];

            // tRand
            byte[] tRand = new byte[2];
            Random random = new Random();
            random.NextBytes(tRand);

            //第三次拼�?
            packet.AddBytes(tBeforeCheckSum);
            packet.AddBytes(tCheckSum);
            packet.AddBytes(tAfterCheckSum);
            packet.AddBytes(tMac,          passLen + 328);
            packet.AddBytes(tZeroCount);
            packet.AddBytes(tRand);

            //�?验并返回
            //Debug.Assert(packet.offset == packet.bytesLength);

            return packet.bytes;
        }
        public void login()
        {
            //计算packet长度
            //t 表示意义不明的临时变�?.协议描述中为 x / 4 * 4,等于x - x % 4
            int t0 = (settings.passWord.Length > 16)? 16 : settings.passWord.Length;
            int t1 = t0 - 1;
            int packetLength = 334 + t1 - t1 % 4;

            //构建packet
            byte[] packet = packetBuild(packetLength);
            Utils.printBytesHex(packet,"Packet");

            //进行通信
            //发�??
            socket.SendTo(
                packet,
                0,
                packetLength + 32,
                SocketFlags.None,
                settings.serverIPEndPoint
            );
            //接收
            byte[] recv = new byte[128];
            socket.Receive(recv);
            Utils.printBytesHex(recv,"recv");

            //判断是否成功
            byte[] status = recv[0..6];
            if(status[0] == 0x04)
            {
                Console.WriteLine("登录成功!");
            }
            else if(status[0] == 0x05)
            {
                Console.WriteLine($"登录失败!");
                Utils.printBytesHex(status, "错误信息");
                //TODO: 判断具体错误
                throw new Exception();
            }

        }

        public Logger(Socket socketArg, Settings settingsArg)
        {
            //赋�??
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
            Console.WriteLine($"{args[0]},{args[1]}");

            //初始化设�?
            Settings settings   = new Settings();
            settings.userName   = args[0];
            settings.passWord   = args[1];
            settings.hostName   = "LENNOVE";
            settings.macAddress = new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
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
