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

                StreamWriter streamWriter = new StreamWriter(pipeClient) { AutoFlush = true };
                StreamReader streamReader = new StreamReader(pipeClient);

                streamWriter.WriteLine(request);

                string? response = streamReader.ReadLine();
                Console.Write(response + "\n");
            }
        }
    }

}
