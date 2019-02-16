using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    8/28/2018
 * 
 * FixedThreadPool
 * 
 * a thread pool with a fixed number of threads
 * -specify the number of threads in the constructor
 * -provides promise-future-like interface
 * -no limit for function's return type
 * -C++ is GOOD
 * 
 */

namespace WebApi.Core
{
    sealed class FixedThreadPool
    {

        //constructor↓

        public FixedThreadPool(int threadNum)
        {
            workers = new Thread[threadNum];
            for (int i = 0; i < threadNum; i++)
            {
                workers[i] = new Thread(WorkerFunc);
                workers[i].Start();
            }
        }

        //interface↓

        public Task Async(Action func)
        {
            return
                AsyncImpl(new TaskCompletionSource<object>(),
                    (TaskCompletionSource<object> promise) =>
                    {
                        func.Invoke();
                        promise.SetResult(null);
                    });
        }

        public Task<TResult> Async<TResult>(Func<TResult> func)
        {
            return
                AsyncImpl(new TaskCompletionSource<TResult>(),
                    (TaskCompletionSource<TResult> promise) =>
                    {
                        promise.SetResult((TResult)func.Invoke());
                    });
        }

        public void Stop()
        {
            EnterDoExit(cvLock, () =>
            {
                stopFlag = true;
                Monitor.PulseAll(cvLock);
            });
            foreach (var worker in workers)
                if (worker.IsAlive)
                    worker.Join();
        }

        //implementation↓

        private Task<TResult> AsyncImpl<TResult>(TaskCompletionSource<TResult> promise, Action<TaskCompletionSource<TResult>> setResultFunc)
        {
            EnterDoExit(cvLock, () =>
            {
                works.Enqueue(() =>
                {
                    try
                    {
                        setResultFunc(promise);
                    }
                    catch (Exception ex)
                    {
                        promise.SetException(ex);
                    }
                });
                Monitor.Pulse(cvLock);
            });
            return promise.Task;
        }

        private void WorkerFunc()
        {
            while (true)
            {
                Action work = null;
                EnterDoExit(cvLock, () =>
                {
                    while (!stopFlag && works.Count == 0)
                        Monitor.Wait(cvLock);
                    works.TryDequeue(out work);
                });
                if (stopFlag)
                    return;
                work();
            }
        }

        static private void EnterDoExit(object mut, Action func)
        {
            Monitor.Enter(mut);
            func();
            Monitor.Exit(mut);
        }

        private Queue<Action> works = new Queue<Action>();
        private Thread[] workers;
        private object cvLock = new object();
        private volatile bool stopFlag = false;
    }
}
