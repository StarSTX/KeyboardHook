using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardHook
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;    //安装一个监视低级键盘输入事件的钩子过程
        private const int WM_KEYDOWN = 0x0100;    //键盘被按下,按下一个非系统键时将消息发送给具有键盘焦点窗口（不与ALT键连用）
        private const int WM_KEYUP = 0x101;      // 键盘被松开
        private const int WM_SYSKEYDOWN = 0x0104; // 键盘被按下，这个是系统键被按下，例如Alt、Ctrl等键
        public const int WM_SYSKEYUP = 0x105;   // 键盘被松开，这个是系统键被松开，例如Alt、Ctrl等键

        private volatile bool isStart = false; // 当前状态,是否已经启动
        private readonly object lockObject = new object();

        #region Win32的Api

        /// <summary>
        ///
        /// </summary><param name="nCode"></param> <param name="wParam">wParam为按键的状态，详情参考钩子类的常数部分</param> <param name="lParam"></param>
        /// <returns></returns>
        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        #endregion Win32的Api

        /// <summary>
        /// 键盘回调委托
        /// </summary>
        /// <param name="wParam">wParam为按键的状态，详情参考钩子类的常数部分</param>
        /// <param name="keyboardHookStruct">被按下的按键的相关信息</param>
        //private delegate void KeyboardHandler(Int32 wParam, KeyboardHookStruct keyboardHookStruct);
        //public delegate void KeyboardHandler(object? sender, KeyboardHookStruct keyboardHookStruct);
        public delegate void KeyboardHandler(IntPtr? wParam, KeyboardHookStruct keyboardHookStruct);

        /// <summary>
        /// Keyboard key press event,键盘回调事件
        /// </summary>
        public event EventHandler<KeyPressedArgs>? OnKeyPressedEvent;

        /// <summary>
        /// The second type of Keyboard key press event,第二种键盘回调事件
        /// </summary>
        private event KeyboardHandler? KeyboardHandlerEvent;

        private KeyboardProc? _keyboardHookDelegate;
        private IntPtr _hookID = IntPtr.Zero;

        public KeyboardHook()
        {
            //_keyboardHookDelegate = HookCallback;
        }

        #region 单例模式

        private static volatile KeyboardHook? keyboardListener;
        private static readonly object createLock = new object();

        public static KeyboardHook GetKeyboardHook()
        {
            if (keyboardListener == null)
            {
                lock (createLock)
                {
                    if (keyboardListener == null)
                    {
                        keyboardListener = new KeyboardHook();
                    }
                }
            }
            return keyboardListener;
        }

        #endregion 单例模式

        /// <summary>
        /// Keyboard Hook
        /// </summary>
        public void HookKeyboard()
        {
            if (isStart)
            {
                return;
            }
            lock (lockObject)
            {
                if (isStart)
                {
                    return;
                }
                _keyboardHookDelegate = new KeyboardProc(HookCallback);
                _hookID = SetHook(_keyboardHookDelegate);
                isStart = true;
            }
        }

        /// <summary>
        /// Release keyboard hook
        /// </summary>
        public void UnHookKeyboard()
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
                UnhookWindowsHookEx(_hookID);  //#Unhook函数拆除之前设置的钩子
                OnKeyPressedEvent = null; // 清除所有事件
                KeyboardHandlerEvent = null; // 清除所有事件
                isStart = false;
            }
        }

        private IntPtr SetHook(KeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    WH_KEYBOARD_LL,   //#要监视的事件
                    proc,             //#指向钩子过程的指针
                    GetModuleHandle(curModule?.ModuleName), //#要钩取的线程所属的DLL句柄，此函数返回
                    0                                      //#要钩取的线程ID，为0则表示全局钩子
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
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (OnKeyPressedEvent != null)
                {
                    OnKeyPressedEvent(this, new KeyPressedArgs(KeyboardKey.KeyFromVirtualKey(vkCode)));
                }
                if (KeyboardHandlerEvent != null)
                {
                    //KeyboardHookStruct KeyDataFromHook = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                    KeyboardHookStruct KeyDataFromHook = new KeyboardHookStruct()
                    {
                        VkCode = vkCode,
                        Time = DateTime.Now,
                    };
                    KeyboardHandlerEvent(wParam, KeyDataFromHook);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 添加按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void AddKeyboardHandler(KeyboardHandler handler)
        {
            KeyboardHandlerEvent += handler;
        }

        /// <summary>
        /// 删除指定按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveKeyboardHandler(KeyboardHandler handler)
        {
            if (KeyboardHandlerEvent != null)
            {
                KeyboardHandlerEvent -= handler;
            }
        }
    }

    /// <summary>
    /// Keyboard key press EventArgs
    /// </summary>
    public class KeyPressedArgs : EventArgs
    {
        public KeyboardKey.Key KeyPressed { get; private set; }

        public KeyPressedArgs(KeyboardKey.Key key)
        {
            KeyPressed = key;
        }
    }

    /// <summary>
    /// 声明键盘钩子的封送结构类型 , 存储被按下的按键的相关信息。详情参考虚拟按键
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class KeyboardHookStruct
    {
        private int vkCode;

        public int VkCode //表示一个在1到254间的虚似键盘码
        {
            get { return vkCode; }
            set
            {
                vkCode = value;
                Key = KeyboardKey.KeyFromVirtualKey(vkCode);
            }
        }

        public KeyboardKey.Key Key; //WPF Key
        public DateTime Time; //触发事件的时间
    }

    //[StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型
    //public class KeyboardHookStruct

    //{
    //    public int vkCode; //表示一个在1到254间的虚似键盘码
    //    public int scanCode; //表示硬件扫描码
    //    public int flags;
    //    public int time;
    //    public int dwExtraInfo;
    //}
}