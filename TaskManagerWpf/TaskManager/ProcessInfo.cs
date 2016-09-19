using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    public class ProcessInfo
    {
        public string Name { private set; get; }
        public int Id { private set; get; }
        public int Threads { private set; get; }
        public int Handles { private set; get; }
        public ProcessInfo(string name, int id, int threads, int handles)
        {
            Name = name;
            Id = id;
            Threads = threads;
            Handles = handles;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}", Name, Id, Handles, Threads);
        }
    }
}
