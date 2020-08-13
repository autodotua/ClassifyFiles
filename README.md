# 文件分类浏览器

![image](Image/DotNetCore.svg)
![image](Image/CSharp.svg)
![image](Image/WPF.svg)
![image](Image/ModernWpfUI.svg)

## 介绍

文件分类浏览器，可以根据文件名、路径名、扩展名、文件信息等各种文件的元数据对海量文件进行自动分类并显示，也可以作为基于标签的文件管理和浏览器，进行手动分类

## 截图

![image](Image/Screenshot_1.png)

![image](Image/Screenshot_2.png)

![image](Image/Screenshot_3.png)

![image](Image/Screenshot_4.png)

![image](Image/Screenshot_5.png)

![image](Image/Screenshot_6.png)

## 特性

- 使用.Net Core 3.1 + WPF + SQLite+EFCore 开发
- 使用WinUI（[ModernWpfUI](https://github.com/Kinnara/ModernWpf)）风格的界面

## 如何构建

打开解决方案，设置```Classify.WPF```为启动项目，按F5一般来说可以直接运行。

由于二进制文件ffmpeg过大，所以被我gitIgnore了。如果需要生成视频缩略图功能，请手动添加至ClassifyFiles.WPFCore的exe目录下。

如果出现依赖错误，请重新定位```Classify```和```Classify.WPF```的dll引用，dll位于项目的Libs文件夹。

## 如何使用

[查看](Help)

## 更新计划和待解决BUG



## 更新日志

[日志](ChangeLog.md)