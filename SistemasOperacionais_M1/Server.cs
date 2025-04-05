using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


partial class Program
{
    static void Server(string dataBaseFile)
    {
        SQLHandler requisitionHandler = new SQLHandler(dataBaseFile);
        while (true)
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("database", PipeDirection.In))
            {
                Console.WriteLine("Servidor aguardando conexão...");

                pipeServer.WaitForConnection();
                Console.WriteLine("Cliente conectado.");

                using (StreamReader reader = new StreamReader(pipeServer))
                {
                    string message = reader.ReadLine();
                    Console.WriteLine("Mensagem recebida - {0}", message);
                    Console.WriteLine(requisitionHandler.ParseInstruction(message));
                }
            }
        }

    }
}
class SQLHandler
{
    private string dataBaseFile;
    public SQLHandler(string dataBaseFile)
    {
        this.dataBaseFile = dataBaseFile;
        string path = Directory.GetCurrentDirectory() + "\\" + dataBaseFile;
        Console.WriteLine("Database directory: {0}", path);
        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }
    }

    private int getLine(int id)
    {
        string fileText = File.ReadAllText(dataBaseFile);
        string[] strings = fileText.Split("\r\n");
        //Implementado busca binaria
        int left = 0;
        int right = strings.Length - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            string[] line = strings[mid].Split(' ');
            int lineID = int.Parse(line[0]);
            if (lineID == id)
            {
                return mid;
            }
            else if (lineID < id)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return -1;

    }



    private string Insert(Registro registro)
    {
        string fileText = File.ReadAllText(dataBaseFile);
        string[] lines = fileText.Split("\r\n");
        string lastLine = lines[lines.Length - 1];
        int ID = lastLine == "" ? 0 : ID = int.Parse(lastLine.Split(' ')[0]) + 1;
        using (StreamWriter w = File.AppendText(dataBaseFile))
        {
            string content = (fileText != "" ? "\r\n" : "") + ID.ToString() + " '" + registro.name + "'";
            w.Write(content);
        }
        ;
        return "INSERIDO REGISTRO -- " + registro.name;
    }

    private string Delete(Registro registro)
    {
        string fileText = File.ReadAllText(dataBaseFile);
        List<string> lines = new List<string>(fileText.Split("\r\n"));
        int lineIndex = getLine(registro.id);
        if (lineIndex == -1)
        {
            return "ID NAO ENCONTRADO";
        }
        else
        {
            lines.RemoveAt(lineIndex);
            string content = String.Join("\r\n", lines);
            File.WriteAllText(dataBaseFile, content);
            return "DELETADO REGISTRO -- " + registro.id;
        }
    }

    private string Select(Registro registro)
    {
        string fileText = File.ReadAllText(dataBaseFile);
        string[] lines = fileText.Split("\r\n");
        int lineIndex = getLine(registro.id);
        if (lineIndex == -1)
        {
            return "ID NAO ENCONTRADO";
        }
        else
        {
            return lines[lineIndex];
        }
    }

    private string Truncate()
    {
        File.WriteAllText(dataBaseFile, "");
        return "LIMPOU A TABELA";
    }


    public string ParseInstruction(string instruction)
    {
        string[] instructionParts = instruction.Split(' ');
        switch (instructionParts[0].ToUpper())
        {
            case "INSERT":
                Registro registro = new Registro();

                var match = Regex.Match(instruction, "'([^']+)'");
                if (match.Success)
                {
                    registro.name = match.Groups[1].Value;
                    return this.Insert(registro);
                }
                else
                {
                    return "INSERT INVALIDO";
                }

            case "DELETE":
                if (int.TryParse(instructionParts[1], out int id))
                {
                    registro = new Registro();
                    registro.id = id;
                    return this.Delete(registro);
                }
                else
                {
                    return "DELETE INVALIDO";
                }
            case "SELECT":
                if (int.TryParse(instructionParts[1], out id))
                {
                    registro = new Registro();
                    registro.id = id;
                    return this.Select(registro);
                }
                else
                {
                    return "SELECT INVALIDO";
                }
            case "TRUNCATE":
                return this.Truncate();

            default:
                return "INVALID INSTRUCTION";
        }
    }

}