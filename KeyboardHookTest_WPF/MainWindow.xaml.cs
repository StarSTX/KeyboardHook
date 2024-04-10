using KeyboardHook;
using System.Windows;
using static KeyboardHook.MouseHook;

namespace KeyboardHookTest_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KeyboardHook.KeyboardHook _listener;
        private MouseHook _Mouselistener;

        public MainWindow()
        {
            InitializeComponent();
            _listener = new KeyboardHook.KeyboardHook();
            _listener.OnKeyPressedEvent += Listener_OnKeyPressed;
            _listener.AddKeyboardHandler(Listener_OnKeyPressed1);
            _listener.HookKeyboard();

            _Mouselistener = new MouseHook();
            _Mouselistener.AddKeyboardHandler(Listener_OnMouse);
            _Mouselistener.HookMouse();
        }

        private int a = 0;

        /// <summary>
        /// Performed when a key on the keyboard is pressed
        /// </summary>
        private void Listener_OnKeyPressed(object? sender, KeyPressedArgs e)
        {
            this.textBox_DisplayKeyboardInput.Text += e.KeyPressed.ToString() + "_";
            a++;
            if (a > 10)
            {
                this.textBox_DisplayKeyboardInput.Text += "\n";
                a = 0;
            }
        }

        /// <summary>
        /// Performed when a key on the keyboard is pressed
        /// </summary>
        private void Listener_OnKeyPressed1(IntPtr? wParam, KeyboardHookStruct e)
        {
            Text1.Text = e.Key.ToString() + wParam;
            Text2.Text = e.VkCode.ToString();
            Text3.Text = "Time:" + e.Time;
        }

        private void Listener_OnMouse(IntPtr wParam, MouseHookStruct? mouseMsg)
        {
            if (wParam != WM_MOUSEMOVE)
            {
                a++;
                if (a > 7)
                {
                    this.textBox_DisplayKeyboardInput.Text += "\n";
                    a = 0;
                }
            }

            switch (wParam)
            {
                case WM_MOUSEMOVE:
                    // 鼠标移动
                    Text11.Text = wParam + mouseMsg?.pt?.x.ToString();
                    Text12.Text = mouseMsg?.pt?.y.ToString();
                    Text13.Text = "hWnd:" + mouseMsg?.hWnd + "_wHitTestCode:" + mouseMsg?.wHitTestCode + "_dwExtraInfo:" + mouseMsg?.dwExtraInfo;
                    break;

                case WM_LBUTTONDOWN:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标左键按下_";
                    break;

                case WM_LBUTTONUP:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标左键抬起_";
                    break;

                case WM_LBUTTONDBLCLK:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标左键双击_";
                    break;

                case WM_RBUTTONDOWN:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标右键按下_";
                    break;

                case WM_RBUTTONUP:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标右键抬起_";
                    break;

                case WM_RBUTTONDBLCLK:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标右键双击_";
                    break;

                case WM_MBUTTONDOWN:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标中键按下_";
                    break;

                case WM_MBUTTONUP:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标中键抬起_";
                    break;

                case WM_MBUTTONDBLCLK:
                    this.textBox_DisplayKeyboardInput.Text += "鼠标中键双击_";
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
            _Mouselistener.UnHookMouse();
        }

        private bool aaa = true;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (aaa)
                _Mouselistener.RemoveKeyboardHandler(Listener_OnMouse);
            else
                _Mouselistener.AddKeyboardHandler(Listener_OnMouse);
            aaa = !aaa;
        }
    }
}