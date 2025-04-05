using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
/*
 * Alunos: Lucas Bittencourt Rauch
 * Andre Mello
 * 
 * 
 * Explicação:
 * Fizemos um pequenos código capaz de parsear algumas instruções "SQL"
 * As instruções apenas inserem, deletam, o pegam nomes em uma tabela
 * Uso:
 * Insert-> INSERT <<Nome_Do_Registro>>
 * Delete-> DELETE <<ID>>
 * Select-> SELECT <<ID>>
 */
partial class Program
{
    // Semáforo para controlar o acesso ao servidor
    static Semaphore semaphore = new Semaphore(1, 1, "Global\\DatabaseSemaphore");

    static void Main(string[] args)
    {
        Thread serverThread = new Thread(() => Server("Databse.txt"));
        serverThread.Start();

        List<Thread> threads = new List<Thread>();
        List<string> commands = new List<string>
        {
            "TRUNCATE",
            "INSERT 'Joao da silva'",
            "INSERT 'Maria da Silva'",
            "SELECT 1",
            "SELECT 2",
            "DELETE 1",
            "SELECT 1"
        };

        foreach(var command in commands)
        {
            threads.Add(new Thread(() => SendRequest(command)));
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("Todos os clientes terminaram.");
    }
}

struct Registro
{
    public int id;
    public string name;
}




