using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Numerics;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;


namespace DrComDotnet
{
    //小工具
    static class Utils
    {
        // 切片,模仿Range
        static public byte[] slice(byte[] src, int begin, int end)
        {
            return src.Skip(begin).Take(end - begin).ToArray();
        }

        // 将Bytes按16进制输出
        static public void printBytesHex(byte[] bytes,string name = "Hex")
        {
            Console.Write("[{0} {1,2:D}] ",name,bytes.Length);
            foreach(byte i in bytes)
            {
                Console.Write("{0,2:X2} ",i);
            }
            Console.WriteLine();
        }

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
    }

}