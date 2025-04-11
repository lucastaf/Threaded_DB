using System.IO.Pipes;
using System.Collections.Concurrent;
using System.Threading;

namespace DBRequestHandler
{
    internal class Client
    {
        private readonly string _pipeName;
        private readonly ConcurrentQueue<string> _pendingInstructions = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _executionSemaphore;
        private readonly object _lock = new object();

        public Client(string pipeName)
        {
            _pipeName = pipeName;
            _executionSemaphore = new SemaphoreSlim(1, int.MaxValue); // Controla a execu��o de uma instru��o por vez
        }

        public void QueueInstruction(string instruction)
        {
            // Adiciona a instru��o na fila
            _pendingInstructions.Enqueue(instruction);
            Console.WriteLine($"Instru��o enfileirada: {instruction}");
        }

        public void ExecuteAllInstructions()
        {
            Console.WriteLine($"Executando {_pendingInstructions.Count} instru��es enfileiradas em ordem...");

            // Enquanto houver instru��es na fila
            while (_pendingInstructions.TryDequeue(out string? instruction))
            {
                // Aguarda o sem�foro para executar a pr�xima instru��o
                _executionSemaphore.Wait();

                // Executa a instru��o em uma nova thread
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        Console.WriteLine($"Executando instru��o: {instruction}");
                        SendRequest(instruction);
                    }
                    finally
                    {
                        // Libera o sem�foro ap�s a execu��o
                        _executionSemaphore.Release();
                    }
                });
            }

            // Aguarda todas as threads conclu�rem
            lock (_lock)
            {
                while (_executionSemaphore.CurrentCount < _executionSemaphore.Release())
                {
                    Thread.Sleep(10); // Pequeno atraso para evitar uso excessivo de CPU
                }
            }

                //Console.WriteLine("Todas as instru��es foram conclu�das.");
        }

        private void SendRequest(string request)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
            {
                pipeClient.Connect();

                StreamWriter streamWriter = new StreamWriter(pipeClient) { AutoFlush = true };
                StreamReader streamReader = new StreamReader(pipeClient);

                streamWriter.WriteLine(request);

                string? response = streamReader.ReadLine();
                Console.Write(response + "\n");
            }
        }
    }
}
