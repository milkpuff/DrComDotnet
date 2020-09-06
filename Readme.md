# DrComDotnet - JLU DrCom Clinet written in C# .Net

[![GitHub version](https://img.shields.io/github/v/release/leviolet/DrComDotnet?include_prereleases&style=flat-square)](https://github.com/leviolet/DrComDotnet/releases/)
![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/leviolet/DrComDotnet?style=flat-square)
[![GitHub Releases Downloads](https://img.shields.io/github/downloads/leviolet/DrComDotnet/total?style=flat-square&color=blue)](https://github.com/leviolet/DrComDotnet/releases/latest)
![GitHub](https://img.shields.io/github/license/leviolet/DrComDotnet?color=blue&style=flat-square)

一个用C# .Net实现的 JLU DrCom 认证客户端.  
本项目离不开:  
- zhjc1124 的 newclinet.py
- YouthLin的 jlu-drcom-protocol 协议分析, checksum与ror算法

本项目有幸成为 [jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client) 的子模块(jlu-drcom-csharp)之一


## 使用方法 1

1. 确保你的系统是Windows 10的较新版本
2. 下载[最新版本](https://github.com/leviolet/DrComDotnet/releases/latest)中含有"**NET48**"字样的文件
3. 解压到你喜欢的地方,在options.json中进行你喜欢的设置,包括用户名密码等等
4. 运行DrComDotnet.exe

## 使用方法 2

1. 安装 .NET Core 3.1 Desktop Runtime: [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.7-windows-x64-installer) [x86](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.7-windows-x86-installer)
2. 下载[最新版本](https://github.com/leviolet/DrComDotnet/releases/latest)
3. 解压到你喜欢的地方,在options.json中进行你喜欢的设置
4. 在终端中运行:  
 ```Shell 
 $ drcomdotnet [用户名] [密码]
 ```

## 引用项目

| 项目                                                                                    | 作者                                    | 许可     |
| --------------------------------------------------------------------------------------- | --------------------------------------- | -------- |
| [jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client)                          | [drcoms](YouthLindrcoms)                | AGPL 3.0 |
| [jlu-drcom-java](https://github.com/drcoms/jlu-drcom-client/tree/master/jlu-drcom-java) | [YouthLin](https://github.com/YouthLin) | AGPL 3.0 |

## 许可

AGPL 3.0

## option.json设计格式

有❌的项目表示目前不可用

```JSONC
{
   "version": 1,                //配置文件版本,也许将来会升级?
   "debug" : {                  //调试选项
       "enabled"    : true,       //是否启用
       "sendTimeout": 3000,       //socket发送超时时长
       "recvTimeout": 3000,       //socket接收超时时长
       "bindIP"     : "0.0.0.0",    //socket绑定地址
       "bindPort"   : 61440         //socket绑定端口 ❌
       "logLevel"   : 2           //输出信息 0:静默 1:正常 2:详细 ❌
   },
   "user"  : {                  //用户信息
       "name"     : "XXXXXX",     //校园网登录 用户名,可能会被命令行参数覆盖
       "password" : "XXXXXX",     //校园网登录 密码  ,可能会被命令行参数覆盖
       "mac"      : "00-11-22-33-44-55",  //网卡地址,填"random"表示使用随机地址
       "hostName" : "LENOV-ABCD", //用户主机名称,长度小于32.填Default表示使用本机主机名
       "ip"       : "DHCP",       //用户IP地址,不填或填DHCP表示使用握手返回的IP地址
       "dns"      : "10.10.10.10",//用户首选DNS
       "randomMac": false         //是否使用随机DNS,填true会覆盖mac项中的固定mac
   },
   "authServer" : {             //认证服务器信息
       "host"  : "auth.jlu.edu.cn", // 网址,会调用DNS进行查询,获得IP地址
       "ip"    : "10.100.61.3",     // IP地址,若使用网址未查到则会使用这个
       "port"  : 61440,             // 端口
       "useDNS": true               // 是否使用DNS查询IP ❌
   },
   "misc" : {                  // 杂项
       "autoConnectWifi": false,  // 自动连接authWIFI
       "authWifi"       : "JLU.PC"// 认证WIFI名
   }
}
```

## 源码使用

没有使用Visual Studio. 只需安装.Net SDK之后在coreVer或frameworkVer中直接运行 

 ```Shell 
 $ dotnet run
 ```

## 路线

- [x] 自动连接WIFI
- [ ] 图形界面
- [x] 增加JSON配置文件
- [x] 针对 .Net Framework 4 进行编译