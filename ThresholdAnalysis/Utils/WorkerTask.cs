using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.Utils
{
    class WorkerTask
    {
        private readonly BlockingCollection<ITask> _queue;
        private bool _shutDown;

        public WorkerTask(BlockingCollection<ITask> queue)
        {
            _queue = queue;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public  void ShutDown() 
        {
            _shutDown = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsShutDown() 
        {
            return _shutDown;
        }

        public void Execute()
        {
            while (IsShutDown() == false || _queue.Count > 0)
            {
                try
                {
                    var task = _queue.Take();
                    if (task != null)
                        Execute(task);
                }
                catch(InvalidOperationException )
                {
                    //This can happen when the _queue is marked complete and the thread is trying to take.
                    //Eat this and move on.
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
        private static void Execute(ITask t)
        {
            t.Execute();
        }

    }
}
