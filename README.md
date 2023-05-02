# AdbUninstaller简介
ADBUninstalller是一个允许用户在非Root环境下对Android手机程序进行批量安装和卸载的小工具
注意：若要使用此程序对手机软件进行操作，需要你的手机接入电脑并开启USB调试模式
# 程序介绍
此例图是基于Android5.1的oppo a37m

<img width="809" alt="屏幕截图 2023-05-02 014824" src="https://user-images.githubusercontent.com/132123702/235501220-88ac92d6-0048-4cfa-814a-1f5d020359f0.png">

用户在启动程序时，程序会自动检索手机是否接入电脑，若用户电脑内拥有adb环境也可在cmd中键入adb devices来验证手机连接状况。当与电脑成功连接，则进入程序后程序会自动检索您手机中安装的应用程序。
## 面板介绍
- 连接状态会在左下角显示
- 左侧面板展示了手机中应用程序列表及其包名
- 右侧上部分为程序执行日志面板，您在程序中的安装卸载过程都会显示在该面板
- 右侧中间的输入框是用以搜索包名，键入你所需要搜索的字段会列出所有包含该字段的包名
- 右侧下部分展示操作按钮
## 操作方式
- 卸载操作：进入程序后，程序自动列出手机中所安装的应用程序列表，选择你需要卸载的应用程序点击卸载按钮即可
- 安装操作：点击安装应用按钮，选择你电脑中的apk安装包（可多选）确定安装即可，安装操作需要在你的手机上一一确认安装，手机上取消安装会导致安装失败
## 程序问题
至0.1.1版本测试，发现Android10版本下设备都可正常使用，其他移动设备没有测试过。但实测荣耀华为设备扫描程序列表会出现问题。有大佬知道问题所在欢迎指正
# 跟作者一起喝咖啡
![image](https://user-images.githubusercontent.com/132123702/235288535-38e245a9-17c4-4ca3-93fc-f55db69c8654.jpg)
