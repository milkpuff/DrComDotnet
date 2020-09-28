# DrcomDotnet : 3rd-Part JLU DrCom Clinet

[![GitHub version](https://img.shields.io/github/v/release/leviolet/DrComDotnet?include_prereleases&style=flat-square)](https://github.com/leviolet/DrComDotnet/releases/)
![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/leviolet/DrComDotnet?style=flat-square)
[![GitHub Releases Downloads](https://img.shields.io/github/downloads/leviolet/DrComDotnet/total?style=flat-square&color=blue)](https://github.com/leviolet/DrComDotnet/releases/latest)
![GitHub](https://img.shields.io/github/license/leviolet/DrComDotnet?color=blue&style=flat-square)

**[下载](https://github.com/leviolet/DrComDotnet/releases/latest) | [使用方法](#使用方法) | [常见问题](#常见问题) |  [源码相关](#源码相关) | [引用与许可](#引用与许可)**

一个用 .NET 实现的 JLU DrCom 第三方认证客户端.  
本项目离不开 zhjc1124 的 newclinet.py 和 YouthLin 的 jlu-drcom-protocol 协议分析以及 checksum 算法

本项目有幸~~十分厚脸皮地~~成为了 [jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client) 的子模块


## 使用方法

### 快速使用(Windows)

1. 如果你的操作系统是 Windows 10/8/8.1 ,它们应当预装了 .NET Framework 4  
如果你使用的的是 Windows 7/Vista/XP 也可以尝试一下安装 .NET Framework 4
2. 下载[最新版本](https://github.com/leviolet/DrComDotnet/releases/latest)中含有"**NET48**"字样的文件
3. 解压到你喜欢的地方,在options.json中进行你喜欢的设置(参照下面的 [配置文件格式](#配置文件) )
4. 运行DrComDotnet.exe

### 安装为服务(可选)

#### 可以提供的功能

- 断线重连
- 开机启动联网
- 不会弹出命令行窗口
- 在连接失败后自动重试两次
- 在休眠后会自动重新连接

#### 步骤

1. 下载安装 [nssm](https://nssm.cc/ci/nssm-2.24-101-g897c7ad.zip) 并将 nssm.exe 文件解压到程序路径.
2. 右键以管理员身份运行 installService.cmd
3. 可以运行 uninstallService.cmd 来卸载服务,也可在 ```开始菜单-Windows管理工具-服务``` 中找到 ```DrcomDontnetNssm``` 将其停用

### 其他操作系统

在Mac OS和Linux上,安装 .NET 运行环境,执行

```Shell
dotnet DrcomDotnet.dll
```

### 配置文件

配置文件为文件夹下的options.json,不区分大小写  
一般来讲,你只需要修改name(用户名),password(密码),和mac地址.  

```JSONC
{
   "version": 1,                //配置文件版本,也许将来会升级?
   "debug" : {                  //调试选项
        "enabled"    : true,       //是否启用
        "sendTimeout": 3000,       //socket发送超时时长
        "recvTimeout": 3000,       //socket接收超时时长
        "bindAddress": "0.0.0.0:61440", //socket绑定地址和端口
        "logLevel"   : 2           //输出信息 0:静默 1:正常 2:详细.不放心就写的再大点或小点都可以
   },
   "user"  : {                    //用户信息
        "name"     : "XXXXXX",             //(常用)用户名
        "password" : "XXXXXX",             //(常用)密码
        "mac"      : "CC-11-22-33-44-AA",  //(常用)网卡地址
        "hostName" : "Default",            //用户主机名称,长度小于32. 填Default表示使用本机主机名
        "ip"       : "DHCP",               //用户IP地址,不填或填DHCP表示使用握手返回的IP地址
        "dns"      : "10.10.10.10",        //用户首选DNS
        "randomMac": false                 //是否使用随机DNS,填true会覆盖mac项中的固定mac
   },
   "authServer" : {             //认证服务器信息
        "host"  : "auth.jlu.edu.cn", // 网址,会调用DNS进行查询,获得IP地址
        "ip"    : "10.100.61.3",     // IP地址,若使用网址未查到则会使用这个
        "port"  : 61440,             // 端口
        "useDNS": true               // 是否使用DNS查询IP
   },
   "misc" : {                  // 杂项
        "autoConnectWifi": false,   // (常用)自动连接认证Wifi     (仅Windows)
        "wifiDelay"      : 5000,   // 自动连接WIFI后的延迟时间.
        "authWifi"       : "JLU.PC",// 认证WIFI名
        "showWelcome"    : true,    // (常用)在登录后访问欢迎页面 (仅Windows,无法在服务状态下使用)
        "welcomeUrl"     : "http://login.jlu.edu.cn/notice.php" // 欢迎页面地址
   }
}
```

## 常见问题

**Q: 有没有带窗口界面的?**

**A:** 有.请看

- yang-er 学长的 [jlu-drcom-csharp](https://github.com/yang-er/jlu-drcom-csharp)
- YouthLin 学长的 [jlu-drcom-java](https://github.com/drcoms/jlu-drcom-client/tree/master/jlu-drcom-java)

图形界面,功能丰富,学长认证,你值得拥有!  
这个项目是不会做图形界面的.难度比较大,也没那么宏大的目标,现在这些都是上网搜索然后复制粘贴改改做出来的

**Q: 我想让它开机启动,静默运行.可以吗?**

**A:** 你可以试试nssm或srvany,直接将本程序包装为后台服务并设置开机启动.  参考上面的"安装为服务"  
本程序不会主动添加后台服务,主要是因为这会让本已混乱的代码更加混乱,而且程序自行安装服务是非常不受欢迎的行为.  

**Q: 和其他版本有什么区别啊?**

**A:** 本项目的校验(checksum)与jlu-drcom-protocol一致,其他部分与newclinet.py一致.  
连接方式基本上没什么区别,设计上比较追求简单纯粹的"命令行+配置文件+系统服务"

**Q: 用第三方客户端有没有可能出问题啊?**

**A:** **当然有风险**.所以如果不是想进行**学习和交流**,请使用原版.  
注意,本项目是一个编程练习项目,切勿用于实际环境.

**Q: 会不会对帐号安全造成影响啊?**

**A:** 源代码都放在上面,实在担心的话就自己看协议分析文件自己写一个呗.  
不过在配置文件中明文存放密码确实不是很安全.建议使用命令行输入密码,并时常修改.  
实际上,大部分第三方客户端都采用明文存储.甚至直接写在程序内部.  

**Q: 如何卸载?**  
**A:** 删除下载的文件夹即可,不会留下任何额外的文件  
如果你安装了服务,则需要在删除所有文件前先运行uninstallService.cmd

## 源码相关

### 认证流程

请参考:

- 协议分析文件: [jlu-drcom-protocol](https://github.com/drcoms/jlu-drcom-client/blob/master/jlu-drcom-java/jlu-drcom-protocol.md)
- Python版本: [newclient.py](https://github.com/drcoms/jlu-drcom-client/blob/master/newclient.py)

### 源码使用

安装 .NET SDK之后在drcomDotnet文件夹中直接运行:

```Shell
dotnet run --framework netcoreapp3.1 -- [用户名] [密码]
```

### 路线图

#### 当前

**暂无**

#### 已完成

<details>
<summary>v1.0 "TariTari"</summary>

- [x] 打磨文档
- [ ] ~~简单的图形界面配置~~
- [x] 提供安装服务的脚本
- [x] 其他系统测试
- [x] ~~断线重连(可通过服务实现)~~
- [x] 静默模式
- [x] 打开欢迎页面选项
- [x] 自动连接WIFI
- [x] JSON配置文件
- [x] 针对 .NET 4 进行编译

</details>


### 源码结构简介

#### 基本流程

在Main函数中体现

| 类型   | 简介                                       |
| ------ | ------------------------------------------ |
| 初始化 | 加载JSON文件, 生成Settings类, 初始化Socket |
| 认证   | 握手,登录,保持连接                         |
| 杂项   | 连接WIFI,打开登录窗口                      |

#### 初始化

(伪代码)

```CSharp
//加载settings,存放json里面的设置
Settings settings = new Settings();
settings.loadFromJsonFile(...);
settings.Init();

//初始化socket
Socket socket     = new Socket(...);
socket.Bind(...);
```

#### 认证

(伪代码)

```CSharp
//握手
Handshaker handshaker = new Handshaker(...);
handshaker.handShake();  //返回salt
//登录
Logger logger         = new Logger    (...,salt);
logger.login();          //返回tail16
//保持连接
KeepAliver keepAliver = new KeepAliver(...,md5a,tail16);
keepAliver.keepAlive();  //这里会无限循环
```

#### Handshaker, Logger, KeepAliver类

主要是 handShake, login, keepAlive函数  
具体过程为:

    准备包需要的参数 -> 连接包 -> 发送包 -> 接收回复 -> 提取需要的参数 -> 返回参数

如果构建包比较复杂,会把 "准备包需要的参数 -> 连接包" 这两个步骤单独写一个函数: packetBuild.

## 引用与许可

### 引用项目

| 项目                                                                                    | 作者                                      | 许可     |
| --------------------------------------------------------------------------------------- | ----------------------------------------- | -------- |
| [jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client)                          | [drcoms](https://github.com/drcoms/)      | AGPL-3.0 |
| [jlu-drcom-java](https://github.com/drcoms/jlu-drcom-client/tree/master/jlu-drcom-java) | [YouthLin](https://github.com/YouthLin)   | AGPL-3.0 |
| [IndexRange](https://github.com/bgrainger/IndexRange)                                   | [bgrainger](https://github.com/bgrainger) | MIT      |

### 许可

Copyright 2020 Leviolet.

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program. If not, see <https://www.gnu.org/licenses/>


![GNU AGPL](https://www.gnu.org/graphics/agplv3-155x51.png)