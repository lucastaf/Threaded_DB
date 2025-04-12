using System.IO.Pipes;

namespace DBRequestHandler
{
    internal class Client
    {
        private readonly string _pipeName;

        public Client(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void SendRequest(string request)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
            {
                pipeClient.Connect();

                // Cliente enviando ao servidor
                StreamWriter streamWriter = new StreamWriter(pipeClient) { AutoFlush = true };
                streamWriter.WriteLine(request);

                // Cliente lendo a resposta do servidor
                StreamReader streamReader = new StreamReader(pipeClient);
                string response = streamReader.ReadLine();
                Console.WriteLine(response);
            }
        }
    }
}
