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
            _executionSemaphore = new SemaphoreSlim(1, int.MaxValue); // Controla a execução de uma instrução por vez
        }

        public void QueueInstruction(string instruction)
        {
            // Adiciona a instrução na fila
            _pendingInstructions.Enqueue(instruction);
            Console.WriteLine($"Instrução enfileirada: {instruction}");
        }

        public void ExecuteAllInstructions()
        {
            Console.WriteLine($"Executando {_pendingInstructions.Count} instruções enfileiradas em ordem...");

            // Enquanto houver instruções na fila
            while (_pendingInstructions.TryDequeue(out string? instruction))
            {
                // Aguarda o semáforo para executar a próxima instrução
                _executionSemaphore.Wait();

                // Executa a instrução em uma nova thread
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        Console.WriteLine($"Executando instrução: {instruction}");
                        SendRequest(instruction);
                    }
                    finally
                    {
                        // Libera o semáforo após a execução
                        _executionSemaphore.Release();
                    }
                });
            }

            // Aguarda todas as threads concluírem
            lock (_lock)
            {
                while (_executionSemaphore.CurrentCount < _executionSemaphore.Release())
                {
                    Thread.Sleep(10); // Pequeno atraso para evitar uso excessivo de CPU
                }
            }

                //Console.WriteLine("Todas as instruções foram concluídas.");
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
