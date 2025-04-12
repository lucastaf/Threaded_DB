using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using DBRequestHandler.Database.Models;
using Microsoft.Win32;

namespace DBRequestHandler.Handlers
{
    public class SQLHandler
    {
        private readonly string _databasePath;
        private readonly string _rowSeparator;
        private readonly string _columnSeparator; 
        public SQLHandler(string databasePath, string rowSeparator, string columnSeparator)
        {
            _databasePath = databasePath;
            _rowSeparator = rowSeparator;
            _columnSeparator = columnSeparator;

            if (!File.Exists(databasePath))
            {
                File.Create(databasePath).Close();
                Console.WriteLine($"Created database file at {databasePath}");
            }
        }

        private string[] getDatabaseRows()
        {
            return File.ReadAllText(_databasePath).Split(_rowSeparator);
        }

        private int getLine(int id)
        {
            string fileText = File.ReadAllText(_databasePath);

            string[] strings = fileText.Split("\r\n");
            //Implementado busca binaria
            int left = 0;
            int right = strings.Length - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string[] line = strings[mid].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string rawId = line[0].Trim().TrimEnd(','); // remove vírgula se houver
                int lineID = int.Parse(rawId);
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

        public string Select(Dictionary<string, string> parsedInstruction)
        {
            string[] databaseRows = getDatabaseRows();
            if (parsedInstruction.ContainsKey("allRows"))
            {
                return String.Join(" ", databaseRows);
            }
            
            if (parsedInstruction.ContainsKey("id"))
            {
                int rowId = getLine(int.Parse(parsedInstruction["id"]));
                if (rowId != -1)
                {
                    return databaseRows[rowId];
                }
            }

            if (parsedInstruction.ContainsKey("nome"))
            {
                foreach (string row in databaseRows)
                {
                    if (row.Contains(parsedInstruction["nome"])) 
                       {
                        return row;
                    }   
                }
            }

            return $"Registro {parsedInstruction.ToString()} não encontrado";
        }

        public string Insert(Dictionary<string, string> parsedInstruction)
        {
            Registro newRegistro = new Registro(int.Parse(parsedInstruction["id"]), parsedInstruction["nome"]);

            string[] lines = getDatabaseRows();
            string lastLine = lines.Length > 0 ? lines[^1] : "";

            if (newRegistro.Id == null)
            {
                newRegistro.Id = lastLine != "" ? int.Parse(lastLine.Split(' ')[0]) + 1 : 0;
            }

            using (StreamWriter w = File.AppendText(_databasePath))
            {
                string formattedNewRegistro = (lines.Length != 0 ? "\r\n" : "") + _columnSeparator + _columnSeparator;
                w.Write(formattedNewRegistro);
            }

            return $"INSERIDO REGISTRO -- {newRegistro.Id}, {newRegistro.Nome}";
        }

        public string Update(Dictionary<string, string> parsedString)
        {
            string[] databaseRows = getDatabaseRows();
            bool updated = false;

            for (int i = 0; i < databaseRows.Length; i++)
            {
                string[] columns = databaseRows[i].Split(_columnSeparator);
                string rowId = columns[0];

                if (rowId == parsedString["id"])
                {
                    // Atualiza a linha com os novos dados
                    databaseRows[i] = rowId + _columnSeparator + parsedString["nome"];
                    updated = true;
                    break; // Sai do loop após atualizar
                }
            }

            File.WriteAllText(_databasePath, string.Join("\r\n", databaseRows));

            return updated ? "Registro atualizado com sucesso." : "Registro não encontrado.";
        }

        public string Delete(Dictionary<string, string> parsedInstruction)
        {
            string fileText = File.ReadAllText(_databasePath);
            List<string> lines = fileText.Split("\r\n").ToList();
            int index = getLine(int.Parse(parsedInstruction["id"]));
            if (index == -1) return "ID NAO ENCONTRADO";

            string deleted = lines[index];
            lines.RemoveAt(index);
            File.WriteAllText(_databasePath, string.Join("\r\n", lines));
            return $"DELETADO REGISTRO -- {deleted}";
        }

        public string Truncate()
        {
            File.WriteAllText(_databasePath, "");
            return "LIMPOU A TABELA";
        }

        public Dictionary<string, string> ParseInstruction(string instruction)
        {
            string[] instructionParts = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = instructionParts[0].ToUpper();
            Dictionary<string, string> parsedInstruction = new Dictionary<string, string>()
            {
                {"command", command}  
            };

            switch (command)
            {
                case "SELECT":
                    // SELECT *
                    // SELECT id=5
                    // SELECT nome='andre'
                    if (instructionParts[1] == "*")
                    {
                        parsedInstruction.Add("allRows", "*");
                    } 
                    else
                    {
                        string[] condition = instructionParts[1].Split("=");
                        parsedInstruction.Add(condition[0], condition[1]);
                    }

                    break;

                case "INSERT":
                    // Example: INSERT id=7 nome='João'
                    // Get id and name
                    string[] fisrtAttr = instructionParts[1].Split("=");
                    string[] secondAttr = instructionParts[2].Split("=");
                    
                    parsedInstruction.Add(fisrtAttr[0], fisrtAttr[1]);
                    parsedInstruction.Add(secondAttr[0], secondAttr[1]);

                    break;

                case "UPDATE":
                    // UPDATE nome='andre' WHERE id=5
                    string nome = instructionParts[1].Split("=")[2];
                    string id = instructionParts[3].Split("=")[2];

                    parsedInstruction.Add("nome", nome);
                    parsedInstruction.Add("id", id);

                    break;

                case "DELETE":
                    // DELETE id=7 nome='João'
                    // Get id, or name, or id and name
                    for (int i = 1; i < instructionParts.Length; i++)
                    {
                        string[] attr = instructionParts[i].Split("=");
                        parsedInstruction.Add(attr[0], attr[1]);
                    }
                    break;

                case "TRUNCATE":
                    break;

            default:
                parsedInstruction.Add("error", "Error: Comando não suportado ou inválido");
                break;
            }

            return parsedInstruction;
        }   
    }
}
