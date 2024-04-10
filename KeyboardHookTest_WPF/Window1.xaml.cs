using KeyboardHook;
using System.Windows;

namespace KeyboardHookTest_WPF
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private MouseKeyboardHook hook;

        public Window1()
        {
            InitializeComponent();
            hook = new MouseKeyboardHook();
            hook.AddHandler(Test1);
            hook.HookStart();
        }

        private int a = 0;

        private void Test1()
        {
            test.Text = a.ToString();
            a++;
        }

        private bool aaa = true;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if(aaa)
            //    hook.RemoveHandler(Test1);
            //else
            //    hook.AddHandler(Test1);
            if (aaa)
                hook.UnHook(); //建议使用时要取消的话直接取消钩子，但是重新启用钩子需要重新添加事件响应
            else
            {
                hook.AddHandler(Test1);
                hook.HookStart();
            }

            aaa = !aaa;
        }
    }
}