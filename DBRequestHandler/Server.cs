using System.IO.Pipes;
using DBRequestHandler.Context;
using DBRequestHandler.Handlers;

namespace DBRequestHandler
{
    internal class Server
    {
        private readonly CustomThreadPool pool = new CustomThreadPool(10);

        bool running;
        Thread? runningThread;

        private readonly string _pipeName;
        private readonly SQLHandler _sqlHandler;
        private readonly SemaphoreSlim _semaphore;
        public Server(string pipeName, string databasePath, int maxThreadsNumber, string rowSeparator, string columnSeparator)
        {
            _pipeName = pipeName;
            _semaphore = new SemaphoreSlim(maxThreadsNumber, maxThreadsNumber);
            _sqlHandler = new SQLHandler(databasePath, rowSeparator, columnSeparator);
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

                // Read client request
                StreamReader streamReader = new StreamReader(pipeStream);
                string request = streamReader.ReadLine(); 
                while (request == null)
                {
                    request = streamReader.ReadLine();
                }

                // Parse client requested query
                Dictionary<string, string> parsedInstruction = _sqlHandler.ParseInstruction(request);
                if (parsedInstruction.TryGetValue("error", out string? value))
                {
                    using (StreamWriter streamWriter = new StreamWriter(pipeStream) { AutoFlush = true })
                    {
                        streamWriter.WriteLine(value);
                    }

                    pipeStream.Dispose(); // Important: close the stream after sending the error
                    return; // Exit early so the invalid request isn’t processed further
                }
                else
                {
                    // If valid input, pass it to be processed
                    pool.QueueUserWorkItem(ProcessClientRequest, new RequestContext
                    {
                        PipeStream = pipeStream,
                        ParsedInstruction = parsedInstruction
                    });
                    //ThreadPool.QueueUserWorkItem(ProcessClientRequest, new RequestContext
                    //{
                    //    PipeStream = pipeStream,
                    //    ParsedInstruction = parsedInstruction
                    //});
                }

            }
            catch (Exception e)
            {//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
            }
        }
                
        private void ProcessClientRequest(object stateInfo)
        {
            var context = (RequestContext)stateInfo;
            NamedPipeServerStream pipeStream = context.PipeStream;
            Dictionary<string, string> parsedQuery = context.ParsedInstruction;

            StreamReader streamReader = new StreamReader(pipeStream);
            StreamWriter streamWriter = new StreamWriter(pipeStream) { AutoFlush = true };

            // Verifica se ouve um erro durante o parse
            string error;
            if (parsedQuery.TryGetValue("error", out error))
            {
                throw new Exception(error);
            }

            // TODO: Melhorar isso, deixar mais enxuto
            string response = "";
            switch (parsedQuery["command"])
            {
                case "SELECT":
                    _semaphore.Wait();
                    response = _sqlHandler.Select(parsedQuery);
                    _semaphore.Release();
                    break;

                case "INSERT":
                    _semaphore.Wait();
                    response = _sqlHandler.Insert(parsedQuery);
                    _semaphore.Release();
                    break;

                case "UPDATE":
                    _semaphore.Wait();
                    response += _sqlHandler.Update(parsedQuery);
                    _semaphore.Release();
                    break;

                case "DELETE":
                    _semaphore.Wait();
                    response = _sqlHandler.Delete(parsedQuery);
                    _semaphore.Release();
                    break;

                case "TRUNCATE":
                    _semaphore.Wait();
                    response = _sqlHandler.Truncate();
                    _semaphore.Release();
                    break;
            }

            streamWriter.WriteLine($"{response}");

            pipeStream.Close();
            pipeStream.Dispose();
        }
    }
}
