using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardHook
{
    public class MouseHook
    {
        public const int WM_MOUSEMOVE = 0x200; // 鼠标移动
        public const int WM_LBUTTONDOWN = 0x201;// 鼠标左键按下
        public const int WM_RBUTTONDOWN = 0x204;// 鼠标右键按下
        public const int WM_MBUTTONDOWN = 0x207;// 鼠标中键按下
        public const int WM_LBUTTONUP = 0x202;// 鼠标左键抬起
        public const int WM_RBUTTONUP = 0x205;// 鼠标右键抬起
        public const int WM_MBUTTONUP = 0x208;// 鼠标中键抬起
        public const int WM_LBUTTONDBLCLK = 0x203;// 鼠标左键双击
        public const int WM_RBUTTONDBLCLK = 0x206;// 鼠标右键双击
        public const int WM_MBUTTONDBLCLK = 0x209;// 鼠标中键双击
        public const int WH_MOUSE_LL = 14; //可以截获整个系统所有模块的鼠标事件。

        private volatile bool isStart = false; // 当前状态,是否已经启动
        private readonly object lockObject = new object();

        private MouseProc? _mouseHookDelegate;
        private IntPtr _hookID = IntPtr.Zero;

        public MouseHook()
        {
        }

        #region Win32的Api

        /// <summary>
        /// 钩子回调函数
        /// </summary>
        /// <param name="nCode">如果代码小于零，则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。此参数可以是下列值之一。(来自官网手册)</param>
        /// <param name="wParam">记录了按下的按钮</param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion Win32的Api

        /// <summary>
        /// 全局的鼠标事件
        /// </summary>
        /// <param name="wParam"> 代表发生的鼠标的事件 </param>
        /// <param name="mouseMsg">钩子的结构体，存储着鼠标的位置及其他信息</param>
        ///public delegate void MyMouseEventHandler(Int32 wParam, MouseHookStruct mouseMsg);
        public delegate void MouseHandler(IntPtr wParam, MouseHookStruct? mouseHookStruct);

        private event MouseHandler? MouseHandlerEvent;

        public void HookMouse()
        {
            if (isStart)
                return;
            lock (lockObject)
            {
                if (isStart)
                    return;
                if (MouseHandlerEvent == null)
                    return;

                _mouseHookDelegate = new MouseProc(HookCallback);
                _hookID = SetHook(_mouseHookDelegate);

                if (_hookID == IntPtr.Zero)
                    UnHookMouse();
                else
                    isStart = true;
            }
        }

        public void UnHookMouse()
        {
            if (!isStart)
            {
                return;
            }
            lock (lockObject)
            {
                if (!isStart)
                {
                    return;
                }
                UnhookWindowsHookEx(_hookID);//#Unhook函数拆除之前设置的钩子
                MouseHandlerEvent = null; // 清除所有事件
                isStart = false;
            }
        }

        private IntPtr SetHook(MouseProc proc)
        {
            using (Process? curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    WH_MOUSE_LL,   //#要监视的事件
                    proc,          //#指向钩子过程的指针
                    Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), //#要钩取的线程所属的DLL句柄，此函数返回
                    0              //#要钩取的线程ID，为0则表示全局钩子
                    );
            }
        }

        /// <summary>
        /// 键盘的系统回调函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam">wParam为按键的状态，详情参考钩子类的常数部分</param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件
            if (nCode >= 0 && (MouseHandlerEvent != null))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (MouseHandlerEvent != null)
                {
                    MouseHookStruct? MyMouseHookStruct = (MouseHookStruct?)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                    MouseHandlerEvent(wParam, MyMouseHookStruct);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 添加按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void AddKeyboardHandler(MouseHandler handler)
        {
            MouseHandlerEvent += handler;
        }

        /// <summary>
        /// 删除指定按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveKeyboardHandler(MouseHandler handler)
        {
            if (MouseHandlerEvent != null)
            {
                MouseHandlerEvent -= handler;
            }
        }
    }

    /// <summary>
    /// 钩子结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
        public POINT? pt; // 鼠标位置
        public int hWnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }

    //声明一个Point的封送类型
    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
        public int x;
        public int y;
    }
}