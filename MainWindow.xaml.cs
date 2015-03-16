using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThreadTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void GoProgressHandle(ProgressBar proBar, int val);
        delegate void ShowThreadIdHandle(Label lb, int id);
        delegate void AsyncDelegate(ProgressBar proBar);

        System.ComponentModel.BackgroundWorker bgWorker = new System.ComponentModel.BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            this.bgWorker.DoWork += bgWorker_DoWork; // 不可以注册多次
        }

        private void bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ShowThreadId(this.lbBgWorker);
 	        AsyncProgressBar(this.proBgWorker);
        }
        
        private void ThreadpoolPro(object state)
        {
            ShowThreadId(this.lbThreadpool);
            AsyncProgressBar(this.proThreadpool);
        }
        
        private void ThreadPro(object obj)
        {
            ShowThreadId(this.lbThread);
            AsyncProgressBar((ProgressBar)obj);
        }

        private void DelegatePro(ProgressBar proBar)
        {
            ShowThreadId(this.lbDelegate);
            AsyncProgressBar(proBar);
        }

        private void TaskPro()
        {
            ShowThreadId(this.lbTask);
            AsyncProgressBar(this.proTask);
        }

        private void AsyncProgressBar(ProgressBar proBar)
        {
            // do some thing asynchrouse
            int val = 0;
            while (true)
            {
                // 线程内不能直接访问 UI 对象，需要使用 Invoke 或者 BeginInvoke
                object[] args = new object[2];
                args[0] = proBar;
                args[1] = val++;
                Dispatcher.BeginInvoke(new GoProgressHandle(GoProgress), args);
                if (val >= 100)
                    val = 0;

                System.Threading.Thread.Sleep(100);
            }
        }

        private void GoProgress(ProgressBar proBar, int val)
        {
            proBar.Value = val;
        }

        private void ShowThreadId(Label lb)
        {
            object[] args = new object[2];
            args[0] = lb;
            args[1] = System.AppDomain.GetCurrentThreadId();
            Dispatcher.BeginInvoke(new ShowThreadIdHandle(ShowThreadId_Invoke), args);
        }

        private void ShowThreadId_Invoke(Label lb, int id)
        {
            lb.Content = string.Format("线程 Id：{0}", id);
        }

        private void btThread_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ThreadPro));
            t.Start(this.proThread);
        }

        private void btThreadpool_Click(object sender, RoutedEventArgs e)
        {
            // 排队任务，线程池有空线程时进入线程函数
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ThreadpoolPro));
        }

        private void btBgWorker_Click(object sender, RoutedEventArgs e)
        {
            if (this.bgWorker.IsBusy)
                return;

            this.bgWorker.RunWorkerAsync();
            // bgWorker 可以在线程函数中直接调用 ReportProgress 当然要先注册事件响应函数
            // this.bgWorker.ProgressChanged += bgWorker_ProgressChanged;
            // bgWorker.ReportProgress
        }

        private void btDelegate_Click(object sender, RoutedEventArgs e)
        {
            AsyncDelegate dele = new AsyncDelegate(DelegatePro);
            dele.BeginInvoke(this.proDelegate, null, null);
        }

        private void btTask_Click(object sender, RoutedEventArgs e)
        {
            Task tk = new Task(new Action(TaskPro));
            tk.Start();
        }

       

    }
}
