using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.utils
{
    public class TaskProcessor
    {
        private readonly BlockingCollection<ITask> _requestQueue;
        private Task[] _tasks;
        private WorkerTask[] _workers;
        private bool _shutDown;
        private readonly int _numThreads;
        private readonly int _capacity;

        public TaskProcessor(int maxThreads, int capacity)
        {
            _numThreads = maxThreads;
            _capacity = capacity;
            _requestQueue = CreateBlockingQueue(_capacity);
        }

        private static BlockingCollection<ITask> CreateBlockingQueue(int capacity)
        {
            return (capacity > 0) ? new BlockingCollection<ITask>(capacity) : new BlockingCollection<ITask>();
        }

        public int GetCapacity()
        {
            return _capacity;
        }

        public int GetPendingCount()
        {
            return _requestQueue.Count;
        }

        public void Start()
        {
            _workers = new WorkerTask[_numThreads];
            _tasks = new Task[_numThreads];
            for (int i = 0; i < _numThreads; i++)
            {
                _workers[i] = new WorkerTask(_requestQueue);
                int i1 = i;
                _tasks[i] = new Task(() => _workers[i1].Execute());
                _tasks[i].Start();
            }
        }

        public void Enqueue(ITask task)
        {
            _requestQueue.Add(task);
        }


        public bool IsShutDown()
        {
            return _shutDown;
        }

        public void ShutDown()
        {
            try
            {
                _shutDown = true;
                _requestQueue.CompleteAdding();
                for (int i = 0; i < _workers.Count(); i++)
                {
                    WorkerTask t = _workers[i];
                    t.ShutDown();
                }
                Task.WaitAll(_tasks);
            }
            catch (Exception t)
            {
                Console.WriteLine(t.StackTrace);
            }
        }

    }
}
