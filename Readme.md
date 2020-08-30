# DrComDotnet - JLU DrCom Clinet written in C# .Net

一个用C# .Net实现的 JLU DrCom 认证客户端

![GitHub version](https://img.shields.io/github/v/release/leviolet/DrComDotnet?include_prereleases&style=flat-square) ![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/leviolet/DrComDotnet?style=flat-square) ![GitHub Releases Downloads](https://img.shields.io/github/downloads/leviolet/DrComDotnet/total?style=flat-square&color=blue) ![GitHub](https://img.shields.io/github/license/leviolet/DrComDotnet?color=blue&style=flat-square)

## 使用方法

1. 安装 .NET Core 3.1 Desktop Runtime: [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.7-windows-x64-installer) [x86](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.7-windows-x86-installer)
2. 下载[最新版本](https://github.com/leviolet/DrComDotnet/releases/latest)
3. 解压到你喜欢的地方
4. 在终端中运行:

   $ drcomdotnet [用户名] [密码]

## 源码使用

没有使用Visual Studio.安装.Net Core sdk之后直接运行

   $ dotnet run

## 路线

- 增加JSON配置文件
- 自动连接WIFI
- 针对.Net Framework 4.6进行编译 (大部分Windows 10预装)
- 图形界面

## 引用项目

[jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client) AGPL许可

## 许可

GNU AGPL v3

## option.json设计格式

目前只有部分功能可用(注释中没有 x 的)

```JSON
{
   "version": 1,                //配置文件版本,也许将来会升级?
   "debug" : {                  //调试选项
       "enabled"    : true,       //是否启用 x
       "sendTimeout": 3000,       //socket发送超时时长
       "recvTimeout": 3000,       //socket接收超时时长
       "bindIP"   : "0.0.0.0",    //socket绑定地址
       "logLevel"   : 2           //输出信息 0:静默 1:正常 2:详细 x
   },
   "user"  : {                  //用户信息
       "name"     : "XXXXXX",     //校园网登录 用户名,可能会被命令行参数覆盖
       "password" : "XXXXXX",     //校园网登录 密码  ,可能会被命令行参数覆盖
       "mac"      : "00-11-22-33-44-55-66-77",  //网卡地址,填"random"表示使用随机地址
       "hostName" : "LENOV-ABCD", //用户主机名称,长度小于32
       "ip"       : "DHCP",       //用户IP地址,不填或填DHCP表示使用握手返回的IP地址
       "dns"      : "10.10.10.10",//用户首选DNS
       "randomMac": false         //是否使用随机DNS,填true会覆盖mac项中的固定mac
   },
   "authServer" : {             //认证服务器信息
       "host"  : "auth.jlu.edu.cn", // 网址,会调用DNS进行查询,获得IP地址
       "ip"    : "10.100.61.3",     // IP地址,若使用网址未查到则会使用这个
       "port"  : 61440,             // 端口
       "useDNS": true               // 是否使用DNS查询IP
   },
   "misc" : {                  // 杂项
       "autoConnectWifi": false,  // 自动连接authWIFI x
       "authWifi"       : "JLU.PC"// 认证WIFI名       x
   }
}
```