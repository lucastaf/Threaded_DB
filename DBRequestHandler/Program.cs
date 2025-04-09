using DBRequestHandler;

string pipeName = "database";
string databasePath =  Path.Combine(Directory.GetCurrentDirectory(), "Database/database.txt");
int maxThreadsNumber = 254;
string rowSeparator = "\r\n";
string columnSeparator = ", ";

// Make a client instace
Client client = new Client(pipeName);

// Make a instance of a server and run it in another thread
Server server = new Server(pipeName, databasePath, maxThreadsNumber, rowSeparator, columnSeparator);
Thread serverThread = new Thread(server.Start);
serverThread.Start();

// Keep the program running
while (true)
{
    // Ask the query to use in the database
    Console.Write("\nQuery: "); 
    string? inputQuery = Console.ReadLine();
    Console.WriteLine();

    // Keep asking while the query is invalid (null or whitespace)
    while (string.IsNullOrWhiteSpace(inputQuery)) 
    {
        Console.Write("Entrada inválida. Tente novamente: ");
        inputQuery = Console.ReadLine();
    }

    // If the input is 'clear', then just clean the screen and continue the loop
    if (inputQuery.ToLower() == "clear")
    {
        Console.Clear();
        continue;
    }

    // If the input is 'quit', ends the applications in 5 seconds
    if (inputQuery.ToLower() == "quit")
    {
        Console.Clear();
        Console.WriteLine("Obrigado por utilizar nosso programa!");
        Thread.Sleep(5000);
        Environment.Exit(0);
    }

    // Send requested query to the server
    client.SendRequest(inputQuery);
}