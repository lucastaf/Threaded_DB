using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DBRequestHandler.Client
{
    public class ClientProcess
    {
        private readonly NamedPipeClientStream pipeClient;
        private readonly StreamWriter streamWriter;
        private readonly StreamReader streamReader;

        public ClientProcess(string pipeName)
        {
            pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            streamWriter = new StreamWriter(pipeClient);
            streamReader = new StreamReader(pipeClient);
        }

        public void SendRequest(string request)
        {
            // Conectando ao cliente
            Console.WriteLine("Tentando se conectar ao cliente...");
            pipeClient.Connect();
            Console.WriteLine("Conectado ao cliente!");

            // Cliente enviando ao servidor
            Console.WriteLine($"Escrevendo '{request}' para o servidor");
            streamWriter.AutoFlush = true;
            streamWriter.Write(request);

            // Cliente recebendo resposta do servidor
            string response = streamReader.ReadLine();
            Console.WriteLine($"Resposta do servidor: {response}");
            
            // Fechando conexão após envio
            streamWriter.Close();
            pipeClient.Close();
        }
    }
}
