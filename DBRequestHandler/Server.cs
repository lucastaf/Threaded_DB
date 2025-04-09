using System.IO;
using System.IO.Pipes;
using System.Threading;
using DBRequestHandler.Handlers;

namespace DBRequestHandler
{
    internal class Server
    {
        bool running;
        Thread? runningThread;

        private readonly SemaphoreSlim _semaphore;
        private readonly string _pipeName;
        private readonly SQLHandler _sqlHandler;
        public Server(string pipeName, string databasePath, int maxThreadsNumber, string rowSeparator, string columnSeparator)
        {
            _pipeName = pipeName;
            _sqlHandler = new SQLHandler(databasePath, rowSeparator, columnSeparator);
            _semaphore = new SemaphoreSlim(maxThreadsNumber, maxThreadsNumber);
        }

        public void Start()
        {
            running = true;
            runningThread = new Thread(ServerLoop);
            runningThread.Start();
        }
        public void Stop()
        {
            running = false;
        }

        private void ServerLoop()
        {
            while (running)
            {
                ProcessNextClient();
            }
        }

        private void ProcessNextClient()
        {
            try
            {
                // Wait input from client
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 254);
                pipeStream.WaitForConnection();

                // Read input and parse it
                //StreamReader streamReader = new StreamReader(pipeStream);
                //string request = streamReader.ReadLine();
                //string response = _sqlHandler.ParseInstruction(request);

                // If valid input, pass it to be processed
                ThreadPool.QueueUserWorkItem(ProcessClientRequest, pipeStream);

            }
            catch (Exception e)
            {//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
            }
        }
                
        private void ProcessClientRequest(object stateInfo)
        {
            _semaphore.Wait();

            NamedPipeServerStream pipeStream = (NamedPipeServerStream)stateInfo;

            StreamReader streamReader = new StreamReader(pipeStream);
            StreamWriter streamWriter = new StreamWriter(pipeStream) { AutoFlush = true };

            string line = streamReader.ReadLine();

            string response = _sqlHandler.ParseInstruction(line);
            streamWriter.WriteLine($"{response}");

            pipeStream.Close();
            pipeStream.Dispose();
            _semaphore.Release();
        }
    }
}
