using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;
using Microsoft.Win32;
using TaskManager;

namespace TaskManagerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskManager.TaskManager TM;
        private int sleepTimeUpdate = 200;
        private int sleepTimeCPU = 300;
        private List<int> listProcId;

        enum ActionValue :int
        {
            Delete = -1,
            Add = 1
        }
        enum ProcessValue : int
        {
            NullOrEmpty = 0,
            Erorr = -1,
            Success = 1
        }
        public MainWindow()
        {
            InitializeComponent();
            TM = new TaskManager.TaskManager();
            listProcId = new List<int>();
        }

        private void WindowLoad(object sender, RoutedEventArgs e)
        {
            Task processTask = new Task(UpdateProcess);
            processTask.Start();

            Task ramCPUTask = new Task(UpdateUIMemoryCPU);
            ramCPUTask.Start();
        }

        private void SaveProcess(object sender, RoutedEventArgs e)
        {
            Task saveTask = new Task(Save);
            saveTask.Start();
        }

        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "txt file (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                lock (ProcessListView)
                {
                    StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName);
                    streamWriter.WriteLine("Process Name, Id, Threads, Handles");

                    for (int i = 0; i < ProcessListView.Items.Count; i++)
                    {
                        streamWriter.WriteLine(ProcessListView.Items[i].ToString());
                    }

                    streamWriter.Close();
                }
            }
        }

        private void StopProcess(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ProcessListView.SelectedIndex;

            if (selectedIndex != -1)
            {
                int actionValue = TM.KillProsecc(listProcId[selectedIndex]);

                if (actionValue == (int)ProcessValue.Success)
                {
                    MessageBox.Show("Process was stoped");
                }
                else if (actionValue == (int)ProcessValue.Erorr)
                {
                    MessageBox.Show(string.Format(
                       "Can not stop process. \nMaybe you do not have roots"));
                }
            }

        }
        private void LaunchProcess(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbNameProc.Text))
            {
                //.exe$ - строка заканчивается на .exe
                //[^...] - строка содержит может содержать символы кроме тех которые указаны в скобках 
                if (Regex.IsMatch(tbNameProc.Text,
                    // @"([^ ,,,№,!,@,#,%,^,(,),<,>,/,~,?,&,|,+,-,/,*]).exe$"))
                    @"([A-Za-z0-9]).exe$"))
                {
                    int actionValue = TM.LaunchProsecc(tbNameProc.Text);

                    if (actionValue == (int)ProcessValue.Success)
                    {
                        tbNameProc.Text = "";
                    }
                    else if (actionValue == (int)ProcessValue.Erorr)
                    {
                        MessageBox.Show(string.Format(
                           "Can not launch process: {0}\nMaybe invalid name of process", tbNameProc.Text));
                    }
                }
                else
                {
                    MessageBox.Show("Invalid name of process \nExample: calc.exe");
                }
            }
            else
            {
                MessageBox.Show("Please enter the name process");
            }
        }

        private bool firstClickWas = false;
        private void ClickTextBox(object sender, RoutedEventArgs e)
        {
            if (!firstClickWas)
            {
                firstClickWas = true;
                tbNameProc.Text = "";
            }
        }
        private void UpdateUIListView(ProcessInfo[] processInfos, int action)
        {
            foreach (var process in processInfos)
            {
                ProcessListView.Dispatcher.Invoke(DispatcherPriority.Background, new
                    Action(() =>
                    {
                        if (action == (int)ActionValue.Add) //добавились процессы
                        {
                            ProcessListView.Items.Add(process);
                            listProcId.Add(process.Id);
                        } 
                        else if (action == (int)ActionValue.Delete) //удалились процессы 
                        {
                            int keyId = listProcId.FindIndex(x => x == process.Id);

                            listProcId.RemoveAt(keyId);
                            ProcessListView.Items.RemoveAt(keyId);

                            ProcessListView.Items.Refresh();
                        }
                    }));
            }

            tbProcRun.Dispatcher.Invoke(DispatcherPriority.Background, new
            Action(() =>
            {
                tbProcRun.Text = String.Format("Process: {0}", TM.CountProcess);
            }));
        }

        private void UpdateUIMemoryCPU()
        {
            while(true)
            {
                Thread.Sleep(sleepTimeCPU);

                tbMemoryCpu.Dispatcher.Invoke(DispatcherPriority.Background, new
                Action(() =>
                {
                    tbMemoryCpu.Text = TM.PerfomancePC();
                }));          
            }
        }
        private void UpdateProcess()
        {
            while (true)
            {
                Thread.Sleep(sleepTimeUpdate);

                if (ProcessListView.Items.Count > 0)
                {
                    int action;
                    ProcessInfo[] processes = TM.UpdateTaskManager(out action);

                    if (processes != null)
                    {
                        UpdateUIListView(processes, action);
                    }
                }
                else //1 запуск программы когда ListView пуст
                {
                    ProcessInfo[] processes = TM.DiscoverTaskManager();

                    if (processes != null)
                    {
                        UpdateUIListView(processes, (int)ActionValue.Add); 
                    }
                }
            }
        }

    }
}
