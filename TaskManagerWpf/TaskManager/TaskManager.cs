using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager
{
    public class TaskManager
    {
        public int CountProcess { get; private set; } = 0;
        private Process[] processList;

        enum ActionValue : int
        {
            Delete = -1,
            Nothing = 0,
            Add = 1
        }
        enum ProcessValue : int
        {
            NullOrEmpty = 0 ,
            Erorr = -1,
            Success = 1
        }

        public string PerfomancePC()
        {
            PerformanceCounter ramCounter =
                new PerformanceCounter("Memory", "Available MBytes");
            float memoryPC = ramCounter.NextValue();

            //CPU PerfomancePC
            PerformanceCounter cpuCounter = 
                new PerformanceCounter("Processor", "% Processor Time", "_Total");

            CounterSample cs1 = cpuCounter.NextSample();
            Thread.Sleep(100);
            CounterSample cs2 = cpuCounter.NextSample();

            float finalCpu = CounterSample.Calculate(cs1, cs2);

            return string.Format("Free RAM: {0}  MB; CPU:  {1}%", memoryPC, (int)finalCpu);


            //Process p = /*get the desired process here*/;
            //Process p = Process.GetProcessesByName("calc")[0];
            //PerformanceCounter ramCounter = 
            //    new PerformanceCounter("Process", "Working Set", p.ProcessName);

            //PerformanceCounter cpuCounter = 
            //    new PerformanceCounter("Process", "% Processor Time", p.ProcessName);

            //double ram = ramCounter.NextValue();
            //double cpu = cpuCounter.NextValue();
            //return string.Format("RAM: {0}  MB; CPU:  {1}%", (ram / 1024 / 1024), (cpu));
        }

        public ProcessInfo[] DiscoverTaskManager()
        {
            processList = Process.GetProcesses();

            if (processList.Length >0)
            {
                CountProcess = processList.Length;
                ProcessInfo[] pr = new ProcessInfo[processList.Length];

                for (int i = 0; i < processList.Length; i++)
                {
                    pr[i] = new ProcessInfo(
                        string.Format("{0}.exe", processList[i].ProcessName),
                        processList[i].Id,
                        processList[i].Threads.Count,
                        processList[i].HandleCount);
                }
                return pr;
            }
            return null; 
        }
        private ProcessInfo[] ProcessExists(Process[] whereFind, Process[] compareWith)
        {
            List<Process> prList = new List<Process>();

            //поиск новых процессов
            foreach (Process p in whereFind)
            {
                if (Array.FindIndex(compareWith,
                    x => x.Id == p.Id) == -1)
                {
                    prList.Add(p);
                }
            }

            if (prList.Count != 0)
            {
                processList = Process.GetProcesses();
                CountProcess = processList.Length;

                //создание массива процессов либо удаленных либо новых
                ProcessInfo[] pr = new ProcessInfo[prList.Count];
                for (int i = 0; i < prList.Count; i++)
                {
                    pr[i] = new ProcessInfo(
                        prList[i].ProcessName,
                        prList[i].Id,
                        prList[i].Threads.Count,
                        prList[i].HandleCount);
                }
                return pr;
            }
            return null;
        }

        public ProcessInfo[] UpdateTaskManager(out int action)
        {
            Process[] newProcessList = Process.GetProcesses();

            if (newProcessList.Length > CountProcess)//добавились новые процессы
            {
                action = (int)ActionValue.Add;
                return ProcessExists(newProcessList, processList);
            }
            else if (newProcessList.Length < CountProcess)//удалились процессы
            {
                action = (int)ActionValue.Delete;
                return ProcessExists(processList, newProcessList);
            }
            // все осталось также
            action = (int)ActionValue.Nothing;
            return null;
        }

        public int LaunchProsecc(string nameProcess)
        {
            if (!string.IsNullOrEmpty(nameProcess))
            {
                try
                {
                    var process = new Process();
                    process.StartInfo.Arguments = string.Empty;
                    process.StartInfo.FileName = nameProcess;

                    process.Start();

                    return (int)ProcessValue.Success;                   
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    //возмоно неправильное имя процесса
                    return (int)ProcessValue.Erorr; 
                }            

            }
            return (int)ProcessValue.NullOrEmpty;
        }
        public int KillProsecc(int processId)
        {
            if(processId != -1)
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    process.Kill();

                    return (int)ProcessValue.Success;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    //отказано в доступе
                    return (int)ProcessValue.Erorr;
                }

            }
            return (int)ProcessValue.NullOrEmpty;
        }

    }
}
