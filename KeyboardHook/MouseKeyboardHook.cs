using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardHook
{
    public class MouseKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;    //截获整个系统的键盘事件。
        public const int WH_MOUSE_LL = 14; //可以截获整个系统所有模块的鼠标事件。

        public MouseKeyboardHook()
        {
        }

        public delegate void MouseKeyboardHandler();

        private event MouseKeyboardHandler? MouseKeyboardHandlerEvent;

        private HookProc? _mouseHookDelegate;
        private HookProc? _keyboardHookDelegate;
        private IntPtr _mousehookID = IntPtr.Zero;
        private IntPtr _keyboardhookID = IntPtr.Zero;
        private volatile bool isStart = false; // 当前状态,是否已经启动
        private readonly object lockObject = new object();

        #region Win32的Api

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion Win32的Api

        public void HookStart()
        {
            if (isStart)
                return;
            lock (lockObject)
            {
                if (isStart)
                    return;
                if (MouseKeyboardHandlerEvent == null)
                    return;

                _mouseHookDelegate = new HookProc(HookCallback);
                _keyboardHookDelegate = new HookProc(HookCallback1);
                _mousehookID = SetHook(_mouseHookDelegate);
                _keyboardhookID = SetHook1(_keyboardHookDelegate);
                if (_mousehookID == IntPtr.Zero)
                    UnHook();
                else
                    isStart = true;
            }
        }

        public void UnHook()
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
                UnhookWindowsHookEx(_mousehookID);//#Unhook函数拆除之前设置的钩子
                UnhookWindowsHookEx(_keyboardhookID);//#Unhook函数拆除之前设置的钩子
                MouseKeyboardHandlerEvent = null; // 清除所有事件
                isStart = false;
            }
        }

        private IntPtr SetHook(HookProc proc)
        {
            using (Process? curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    WH_KEYBOARD_LL,   //#要监视的事件
                    proc,          //#指向钩子过程的指针
                    Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), //#要钩取的线程所属的DLL句柄，此函数返回
                    0              //#要钩取的线程ID，为0则表示全局钩子
                    );
            }
        }

        private IntPtr SetHook1(HookProc proc)
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

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件
            if (nCode >= 0 && (MouseKeyboardHandlerEvent != null))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (MouseKeyboardHandlerEvent != null)
                {
                    MouseKeyboardHandlerEvent();
                }
            }
            return CallNextHookEx(_mousehookID, nCode, wParam, lParam);
        }

        private IntPtr HookCallback1(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件
            if (nCode >= 0 && (MouseKeyboardHandlerEvent != null))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (MouseKeyboardHandlerEvent != null)
                {
                    MouseKeyboardHandlerEvent();
                }
            }
            return CallNextHookEx(_keyboardhookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 添加按键的回调函数
        /// </summary>
        /// <param name="handler">需要添加的触发操作</param>
        public void AddHandler(MouseKeyboardHandler handler)
        {
            MouseKeyboardHandlerEvent += handler;
        }

        /// <summary>
        /// 添加按键的回调函数
        /// </summary>
        /// <param name="handler">需要删除的触发操作</param>
        public void RemoveHandler(MouseKeyboardHandler handler)
        {
            if (MouseKeyboardHandlerEvent != null)
                MouseKeyboardHandlerEvent -= handler;
        }
    }
}