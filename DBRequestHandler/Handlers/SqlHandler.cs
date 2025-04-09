using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DBRequestHandler.Database.Models;

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

        private string Insert(Registro registro)
        {
            string fileText = File.ReadAllText(_databasePath);
            string[] lines = fileText.Split("\r\n");

            string lastLine = lines.Length > 0 ? lines[^1] : "";

            if (registro.Id == null)
            {
                registro.Id = lastLine != "" ? int.Parse(lastLine.Split(' ')[0]) + 1 : 0;
            }

            using (StreamWriter w = File.AppendText(_databasePath))
            {
                string newRegistro = (fileText != "" ? "\r\n" : "") + registro.Id + ", '" + registro.Nome + "'";
                w.Write(newRegistro);
            }

            return $"INSERIDO REGISTRO -- {registro.Id}, {registro.Nome}";
        }

        private string Delete(int id)
        {
            string fileText = File.ReadAllText(_databasePath);
            List<string> lines = fileText.Split("\r\n").ToList();
            int index = getLine(id);
            if (index == -1) return "ID NAO ENCONTRADO";

            string deleted = lines[index];
            lines.RemoveAt(index);
            File.WriteAllText(_databasePath, string.Join("\r\n", lines));
            return $"DELETADO REGISTRO -- {deleted}";
        }

        private string Delete(string nome)
        {
            string fileText = File.ReadAllText(_databasePath);
            List<string> lines = fileText.Split("\r\n").ToList();

            int index = lines.FindIndex(line => line.Contains($"'{nome}'", StringComparison.OrdinalIgnoreCase));
            if (index == -1) return "NOME NAO ENCONTRADO";

            string deleted = lines[index];
            lines.RemoveAt(index);
            File.WriteAllText(_databasePath, string.Join("\r\n", lines));
            return $"DELETADO REGISTRO -- {deleted}";
        }

        private string Select(int registroId)
        {
            string fileText = File.ReadAllText(_databasePath);
            string[] lines = fileText.Split("\r\n");
            int selectedRegistroIndex = getLine(registroId);
            if (selectedRegistroIndex == -1)
            {
                return "ID NAO ENCONTRADO";
            }
            else
            {
                return lines[selectedRegistroIndex];
            }
        }

        private string SelectAll()
        {
            string[] lines = File.ReadAllText(_databasePath).Split("\r\n");
            return String.Join(" ", lines);
        }

        private string Truncate()
        {
            File.WriteAllText(_databasePath, "");
            return "LIMPOU A TABELA";
        }


        public string ParseInstruction(string instruction)
        {
            string[] instructionParts = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = instructionParts[0].ToUpper();

            switch (command)
            {
                case "INSERT":
                    Registro registro = new Registro();

                    foreach (var part in instructionParts.Skip(1))
                    {
                        if (part.StartsWith("id=", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(part.Substring(3), out int id))
                            {
                                registro.Id = id;
                            }
                            else
                            {
                                return "INSERT INVÁLIDO: id mal formatado";
                            }
                        }
                        else if (part.StartsWith("nome=", StringComparison.OrdinalIgnoreCase))
                        {
                            var match = Regex.Match(part, "nome='([^']+)'", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                registro.Nome = match.Groups[1].Value;
                            }
                            else
                            {
                                return "INSERT INVÁLIDO: nome mal formatado";
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(registro.Nome))
                        return "INSERT INVÁLIDO: faltando nome";

                    if (registro.Id == 0)
                        return "INSERT INVÁLIDO: faltando id";

                    return Insert(registro);

                case "DELETE":
                    if (instructionParts.Length < 2)
                        return "DELETE INVÁLIDO: parâmetro ausente";

                    string param = instructionParts[1];

                    if (param.StartsWith("id=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(param.Substring(3), out int id))
                        {
                            return Delete(id);
                        }
                        else
                        {
                            return "DELETE INVÁLIDO: id mal formatado";
                        }
                    }
                    else if (param.StartsWith("nome=", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = Regex.Match(param, "nome='([^']+)'", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            return Delete(match.Groups[1].Value);
                        }
                        else
                        {
                            return "DELETE INVÁLIDO: nome mal formatado";
                        }
                    }
                    else
                    {
                        return "DELETE INVÁLIDO: parâmetro não reconhecido";
                    }

                case "SELECT":
                    if (instructionParts.Length < 2)
                        return "SELECT INVÁLIDO. Formato esperado: SELECT <id>";

                    string selectFields = instructionParts[1].ToLower();
                    string[] fields = selectFields.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    if (selectFields == "*")
                    {
                        return SelectAll();
                    }

                    if (int.TryParse(instructionParts[1], out int registroId))
                    {
                        try
                        {
                            return Select(registroId);
                        }
                        catch (Exception ex)
                        {
                            return $"SELECT INVÁLIDO. Erro: {ex.Message}";
                        }
                    }

                    return "SELECT INVÁLIDO. Id no formato errado";

                case "TRUNCATE":
                    return Truncate();

                default:
                    return "INVALID INSTRUCTION";
            }
        }
    }
}
