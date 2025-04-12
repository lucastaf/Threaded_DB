public class CustomThreadPool
{
    private readonly Queue<(WaitCallback, object?)> _taskQueue = new();
    private readonly List<Thread> _workers = new();
    private readonly AutoResetEvent _taskAvailable = new(false);
    private readonly object _lock = new();

    private bool _running = true;

    public CustomThreadPool(int numberOfThreads)
    {
        for (int i = 0; i < numberOfThreads; i++)
        {
            Thread worker = new Thread(WorkerLoop) { IsBackground = true };
            _workers.Add(worker);
            worker.Start();
        }
    }

    public void QueueUserWorkItem(WaitCallback callback, object? state)
    {
        lock (_lock)
        {
            _taskQueue.Enqueue((callback, state));
        }
        _taskAvailable.Set(); // Signal that a task is available
    }

    private void WorkerLoop()
    {
        while (_running)
        {
            (WaitCallback callback, object? state) task;

            lock (_lock)
            {
                if (_taskQueue.Count == 0)
                {
                    Monitor.Exit(_lock);
                    _taskAvailable.WaitOne();
                    Monitor.Enter(_lock);
                }

                if (_taskQueue.Count == 0)
                    continue;

                task = _taskQueue.Dequeue();
            }

            task.callback(task.state); // Call the method with the state
        }
    }

    public void Stop()
    {
        _running = false;
        _taskAvailable.Set(); // Wake up all threads
    }
}
