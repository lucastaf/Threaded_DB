using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DBRequestHandler.Handlers;

namespace DBRequestHandler.Server
{
    public class ServerProcess
    {
        private readonly string _pipeName;
        public ServerProcess(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start() // Inicializa un listener para o servidor
        {
            // Inicializando váriaveis
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut);
            StreamReader streamReader = new StreamReader(pipeServer);
            StreamWriter streamWriter = new StreamWriter(pipeServer);

            Console.WriteLine("Esperando conexão...");
            pipeServer.WaitForConnection();

            string line = streamReader.ReadLine();
            Console.WriteLine($"Received from client: {line}");

            streamWriter.WriteLine("Hello, Client!");
        }
    }
}
