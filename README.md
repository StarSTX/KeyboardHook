# 一个用于 .NET 8 的全局键盘钩子实现
- 应该也适用于 **.NET 7** 和 **.NET 6** 
我的新项目需要在 **.NET 8** 中使用全局键盘钩子，但是我在网上找了一圈没找到可以在 .NET 上到手即用的方式，基本都是基于 .NET Framework 的全局键盘钩子，当你在 .NET 8 中使用时你会发现有很多东西被舍弃了，比如：之前借助 **System.Windows.Input** 或 **System.Windows.Forms** 命名空间内的代码可以方便快捷的实现的键盘钩子，但是这些方式经过我(小菜鸟)的测试发现是不可用的。本项目没有使用这两个命名空间，参考了很多文章与手册，并单独实现了一些必须的功能。
所以这个项目结构是这样的：
```
/KeyboardHook/ : 实现键盘钩子需要的一些定义与静态方法;
/KeyboardHookTest_WPF/ : 使用WPF简单测试键盘钩子;
```
## 使用过程中如果出现问题我会及时在下面更新并推送新的修改，也欢迎朋友们积极提意见或提出你们碰到问题

## 参考
- https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.input.key?view=windowsdesktop-7.0
- https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes
- https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.input.keyinterop?view=windowsdesktop-7.0
- http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/
- https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application?answertab=modifieddesc#tab-top
- https://blog.csdn.net/qq_43851684/article/details/113096306
