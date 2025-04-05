using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


partial class Program
{

    static void SendRequest(string message)
    {

        // Aguarda acesso ao semáforo
        semaphore.WaitOne();

        try
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "database", PipeDirection.Out))
            {
                pipeClient.Connect();
                using (StreamWriter writer = new StreamWriter(pipeClient))
                {
                    writer.AutoFlush = true;
                    writer.WriteLine(message);
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
