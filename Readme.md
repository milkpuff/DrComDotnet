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

## 路线图

- 增加JSON配置文件
- 自动连接wifi
- 针对.Net Framework 4.6进行编译(大部分Windows 10预装)
- 图形界面

## 引用项目

[jlu-drcom-client](https://github.com/drcoms/jlu-drcom-client) AGPL许可

## 许可

GNU AGPL v3