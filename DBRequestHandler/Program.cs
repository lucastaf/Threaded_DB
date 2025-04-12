using DBRequestHandler;

string pipeName = "database";
string databasePath = Path.Combine(Directory.GetCurrentDirectory(), "Database\\database.txt");
int maxThreadsNumber = 254;
string rowSeparator = "\r\n";
string columnSeparator = ", ";

// Configurações do ThreadPool
ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount);
ThreadPool.SetMaxThreads(maxThreadsNumber, maxThreadsNumber);

// Cria uma instância do cliente
Client client = new Client(pipeName);

// Cria uma instância do servidor e inicia ele
Server server = new Server(pipeName, databasePath, maxThreadsNumber, rowSeparator, columnSeparator);
server.Start();

Console.WriteLine("Servidor iniciado. Digite 'help' para listar possiveis instruções.");
Console.WriteLine("Qualquer comando SQL inserido será enfileirado para execução posterior.");

// Mantém o programa em execução
while (true)
{
    // Solicita a consulta a ser usada no banco de dados
    Console.Write("\nConsulta: ");
    string? inputQuery = Console.ReadLine();
    Console.WriteLine();

    // Continua solicitando enquanto a consulta for inválida (nula ou em branco)
    while (string.IsNullOrWhiteSpace(inputQuery))
    {
        Console.Write("Entrada inválida. Tente novamente: ");
        inputQuery = Console.ReadLine();
    }

    // Se a entrada for 'quit', encerra a aplicação em 5 segundos
    if (inputQuery.ToLower() == "quit")
    {
        Console.Clear();
        Console.WriteLine("Obrigado por utilizar nosso programa!");
        server.Stop(); // Para o servidor de forma graciosa
        Thread.Sleep(5000);
        Environment.Exit(0);
    }

    // Se a entrada for 'clear', limpa a tela e continua o loop
    if (inputQuery.ToLower() == "clear")
    {
        Console.Clear();
        continue;
    }

    if (inputQuery.ToLower() == "help")
    {
        Console.WriteLine("Comandos disponíveis (Banco):");
        Console.WriteLine("Select id=<id> - Seleciona um registro do banco");
        Console.WriteLine("Insert nome=<nome> id=<id> - Inseri um registro no banco");
        Console.WriteLine("Update nome=<nome> WHERE id=<id> - Atuliza um registro no banco");
        Console.WriteLine("Delete <id> - Remove um registro do banco");
        Console.WriteLine("Truncate - Limpa todos os registros banco");

        Console.WriteLine(" ");
        Console.WriteLine("Comandos disponíveis (Outros):");
        Console.WriteLine("'clear' - Limpa a tela");
        Console.WriteLine("'quit' - Encerra o programa");
        Console.WriteLine("'help' - Lista os comandos disponíveis");
        continue;
    }

    // Enfileira a instrução para execução posterior
    client.SendRequest(inputQuery);
}
