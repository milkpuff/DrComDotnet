using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;

namespace DrComDotnet
{
    using uint8 = System.Byte;

    //小工具
    static class Utils
    {
        static public void PrintBytesHex(string name, ref byte[] bytes)
        {
            Console.Write("[{0} {1,2:D}] ",name,bytes.Length);
            foreach(byte i in bytes)
            {
                Console.Write("{0,2:X2} ",i);
            }
            Console.WriteLine();
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
        public Settings()
        {
            macAddress = new byte[6];
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
        // packetBuilder
        Random randomBuilder = new Random();

        // 构建握手需要的包
        private byte[] packetBuilder(uint8 challenge_times)
        {
            // packet(20B) = begin(1B) + times(1B) + rand(2B) + end(17B)
            byte[] begin = new byte[] {0x01};
            
            byte[] rand  = new byte[2];
            randomBuilder.NextBytes(rand);

            byte[] times = {challenge_times};

            byte[] end   = new byte[] { 0x6a,
                0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00
            };
            byte[] packet = begin.Concat(rand).Concat(times).Concat(end).ToArray();
            Debug.Assert(packet.Length == 20, $"Incorrect Packet Length: {packet.Length}");
            return packet;
        }

        public void handShakeDev()
        {
            byte[] packet = packetBuilder(challenge_times);
            Utils.PrintBytesHex("packet",ref packet);
        }

        // 握手,返回salt和客户端ip
        public Tuple<byte[], IPAddress> handShake()
        {
            //构建握手包
            byte[] packet = packetBuilder(challenge_times);
            Utils.PrintBytesHex("packet",ref packet);
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
            Utils.PrintBytesHex("handshake recv", ref recv);
            //切出salt和客户端 IP 地址
            byte[] salt     = recv[4..8];
            byte[] clinetIPBytes = recv[20..24];
            IPAddress clinetIP = new IPAddress(clinetIPBytes);
            Utils.PrintBytesHex("salt", ref salt);
            Utils.PrintBytesHex("clinetIPBytes", ref clinetIPBytes);
            //校验随机位
            Debug.Assert(recv[2..3] == packet[2..3]);

            return new Tuple<byte[], IPAddress>(salt,clinetIP);
        }


        public Handshaker(Socket socket, Settings settings)
        {
            this.socket = socket;
            this.settings = settings;
        }
    }

    //登录器
    class Logger
    {

        public void login()
        {

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

            // UDP报文形式的SOCKET
            Socket   socket   = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp); 
            Settings settings = new Settings();
            settings.userName = "XXXXX";
            settings.passWord = "XXXXX";

            Settings settingsDev = new Settings();
            settingsDev.userName = "XXXXX";
            settingsDev.passWord = "XXXXX";

            IPAddress   bindIP     = IPAddress.Parse("0.0.0.0");
            IPEndPoint  bindIpPort = new IPEndPoint(bindIP, 61440);
            socket.Bind(bindIpPort);
            socket.SendTimeout = 3000;
            
            Handshaker handshaker = new Handshaker(socket,settings);
            handshaker.handShake();

            socket.Close();
        }
    }
}
