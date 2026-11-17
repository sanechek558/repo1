using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace mini_CRM
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client1 = new Client(1, "Иван Иванов", "ivanivanov@ivan.com", DateTime.Now);
            Console.WriteLine(client1);

            var order1 = new Order(1, client1.Id, "Создать сайт", 1000.00m, DateOnly.FromDateTime(DateTime.Now.AddDays(7)));
            Console.WriteLine(order1);

            var client1_copy = new Client(1, "Иван Иванов", "ivanivanov@ivan.com", client1.CreatedAt);
            Console.Write("Равны ли клиенты? ");
            Console.WriteLine(client1_copy == client1);

            string clientFile = "clients.json";
            var ClientRepo = new ClientRepository(clientFile);
            Console.WriteLine("Список клиентов: ");
            PrintClients(ClientRepo.GetAll);

            ClientRepo.Add("ivan", "ivan@bk.ru");
            PrintClients(ClientRepo.GetAll);
            
            await ClientRepo.SaveAsync();

        }
        public static void PrintClients(List<Client> clients)
        {
            if (!clients.Any())
            {
                Console.WriteLine("Список пуст");
                return;
            }
            foreach (var c in clients)
            {
                Console.WriteLine(c);
            }
        }
    }
    public record Client(int Id, string Name, string Email, DateTime CreatedAt);
    public record Order(int Id, int ClientId, string Description, decimal Amount, DateOnly DueDate );
    public class BaseRepository<T>
    {
        protected List<T> _items;
        protected readonly string _filepath;
        protected int _nextId = 1;

        protected BaseRepository(string filepath)
        {
            _filepath = filepath;
            _items = new List<T>();
            Load();
        }
        private void Load()
        {
            if (!File.Exists(_filepath)) return; 
            string json = File.ReadAllText(_filepath);
            _items = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        public virtual async Task SaveAsync()
        {
            string json = JsonConvert.SerializeObject(_items, Newtonsoft.Json.Formatting.Indented);
            await File.WriteAllTextAsync(_filepath, json);
        }
        public List<T> GetAll => _items;
    }
    public class ClientRepository : BaseRepository<Client>
    {
        public ClientRepository(string filepath) : base(filepath)
        {
            if(_items.Any())
            {
                _nextId = _items.Cast<Client>().Max(c => c.Id)+1;
            }
            
        }
        public Client Add(string Name, string Email)
        {
            var client = new Client(_nextId++, Name, Email, DateTime.Now);
            _items.Add(client);
            return client;
        }
        public Client? GetById(int Id)
        {
            return _items.Cast<Client>().FirstOrDefault(c => c.Id == Id);
        }
    }
}
