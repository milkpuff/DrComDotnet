# DrComDotnet - JLU DrCom Clinet written in C# .Net

[![GitHub version](https://img.shields.io/github/v/release/leviolet/DrComDotnet?include_prereleases&style=flat-square)](https://github.com/leviolet/DrComDotnet/releases/)
![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/leviolet/DrComDotnet?style=flat-square)
[![GitHub Releases Downloads](https://img.shields.io/github/downloads/leviolet/DrComDotnet/total?style=flat-square&color=blue)](https://github.com/leviolet/DrComDotnet/releases/latest)
![GitHub](https://img.shields.io/github/license/leviolet/DrComDotnet?color=blue&style=flat-square)

一个用C# .Net实现的 JLU DrCom 认证客户端.  
本项目离不开:  
- zhjc1124 的 newclinet.py
- YouthLin的 jlu-drcom-protocol 协议分析, checksum与ror算法

本项目有幸成为 [jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client) 的子模块之一


## 使用方法 1

1. 确保你的系统是 Windows 10/8/8.1 的较新版本,它们应当预装了.NET Framework 4 (如果你使用的的是 Windows 7/XP 也可以尝试一下安装.NET Framework 4)
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

带有❌的项目表示目前无效

```JSONC
{
   "version": 1,                //配置文件版本,也许将来会升级?
   "debug" : {                  //调试选项
        "enabled"    : true,       //是否启用
        "sendTimeout": 3000,       //socket发送超时时长
        "recvTimeout": 3000,       //socket接收超时时长
        "bindAddress": "0.0.0.0:61440", //socket绑定地址和端口
        "logLevel"   : 2           //输出信息 0:静默 1:正常 2:详细。不放心就写的再大点或小点都可以
   },
   "user"  : {                  //用户信息
        "name"     : "XXXXXX",     //校园网登录 用户名,可能会被命令行参数覆盖
        "password" : "XXXXXX",     //校园网登录 密码  ,可能会被命令行参数覆盖
        "mac"      : "CC-11-22-33-44-AA",  //网卡地址,填"random"表示使用随机地址
        "hostName" : "Default",    //用户主机名称,长度小于32. 填Default(不区分大小写)表示使用本机主机名
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
        "autoConnectWifi": true,   // 自动连接认证Wifi(不稳定)
        "wifiDelay"      : 5000,   // 自动连接WIFI后的延迟时间。(太短了会在WIFI连通之前发送消息导致连接失败)
        "authWifi"       : "JLU.PC",// 认证WIFI名
        "showWelcome"    : true,    // 在登录后访问欢迎页面
        "welcomeUrl"     : "http://login.jlu.edu.cn/notice.php" // 欢迎页面地址
   }
}
```

## 源码使用

没有使用Visual Studio. 只需安装.NET SDK之后在drcomDotnet文件夹中直接运行 

```Shell 
$ dotnet run --framework net48 -- [用户名] [密码]
```


## 路线图

v1.0 "TariTari" 路线图:

- [ ] 尝试休眠重连功能
- [ ] 打磨文档
- [ ] 简单的图形界面配置
- [x] 静默模式
- [x] 打开欢迎页面选项
- [x] 自动连接WIFI
- [x] 增加JSON配置文件
- [x] 针对 .NET 4 进行编译

## 其他

本项目的校验(checksum)与jlu-drcom-protocol一致,其他部分与newclinet.py一致


### 相关项目

yang-er学长的(jlu-drcom-csharp)[https://github.com/yang-er/jlu-drcom-csharp]  
同样使用C#开发,图形界面,功能丰富,学长认证,你值得拥有  

本项目的意义?  
其实本来想用C语言写,结果太麻烦了,又懒得学Java,就用的C#。一开始写的时候还不知道已经有同类型项目了,写了几个星期才搞出来。总不能让几个星期努力打水漂吧,就继续维护一段时间。还非常厚脸皮的给了[jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client)项目提了几个Pull Request,被打了几次脸,也出了不少乌龙(人家是第一次用Github协作功能啊,求原谅)。后来才发现有学长写了(jlu-drcom-csharp)[https://github.com/yang-er/jlu-drcom-csharp] (没提交成jlu-drcom-client子模块所以没看见), 自己还占了人家名字, 于是又交了个Pull Request改了回来。(给学长们添麻烦了,对不起)。所以吧,我就不会像jlu-drcom-csharp那样搞图形界面了,把命令行和配置文件写好就行,再加点实用小功能作为特色。
