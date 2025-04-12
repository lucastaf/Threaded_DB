namespace DBRequestHandler.Database.Models
{
    public class Registro
    {       
        public Registro(int id, string nome) { 
            Id = id;
            Nome = nome;
        }

        public int Id;
        public string Nome;
    }
}
