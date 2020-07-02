# 

## 介绍



## 截图

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/MainWindow_Light.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/MainWindow_Dark.png)

## 特性

- 使用.Net Core 3.1 + WPF + SQLite+EFCore 开发
- 使用WinUI风格的界面

## 更新日志

### 2020-05-10

开始项目

由《网页内容变动提醒》进行拆除

### 2020-05-11

完成主界面的框架

完成分类树形图的逻辑和显示

基本完成分类设置的匹配条件设置

### 2020-05-12

基本完成了分类树状图的新增和删除

完成分类的重命名

基本完成对文件的枚举分类工具方法

完成文件浏览界面基本功能，包括项目的名称显示、根目录设置

基本完成使用列表视图和图标视图来显示文件。

 ### 2020-05-13

基本完成使用图标视图时的内存分页（图标视图应该没有虚拟化，很卡）

新增树状视图

基本完成对图片缩略图的显示

### 2020-05-14

将图标和缩略图封装为组件，并为三个视图都应用了该组件

新增列表视图支持滚轮缩放

修改了分页按钮样式

新增项目的新建

新增右键打开目录菜单

列表视图新增组合框，用于跳转到指定目录的文件位置

新增Ctrl+滚轮实现图标视图的缩放功能

重写了缩略图策略，改为在枚举文件时就获取缩略图，并使用数据库BLOB保存缩略图

过滤方式新增正则表达式相关、文件大小限制、文件修改时间限制

### 2020-05-26

将控件库从MaterialDesign改为ModernWpf，基本完成主要修改

### 2020-05-27

完全清除了MaterialDesign库

新增对于没有缩略图的文件，根据文件类型提供不同的图标，对于文件夹提供文件夹图标

新增了单独的项目设置页

支持了图标视图根据文件夹跳转

修改了一些逻辑

为处理中Ring的覆盖层加了动画

### 2020-05-28

新增了导出为快捷方式或副本的功能

修复了一些BUG，优化了一些功能

### 2020-05-28

支持了导入和导出数据库（项目）的功能

支持了一键删除所有项目

### 2020-06-01

修复了项目设置界面的布局错误

新增可以使用按钮+对话框选择根目录

修改文件浏览界面的左侧分类列表为Expander

新增错误提示框

支持了对单个类进行刷新

### 2020-07-02

为了增加标签模式，大幅度修改了代码，对某些类和界面进行了抽象，新增了一些父类，对文件浏览块进行了封装

修改Class，从树状结构修改为顺序结构