using MongoDB.Bson;
using MongoDB.Driver;
using MongoService.Models;
using MongoService.Repositories.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;
namespace MongoService.DAL
{
    public class Dal : IDisposable, IUserRepository
    {
        //private MongoServer mongoServer = null;
        private bool disposed = false;

        // To do: update the connection string with the DNS name
        // or IP address of your server. 
        //For example, "mongodb://testlinux.cloudapp.net
        private string userName = "matchflixmongo";
        private string host = "matchflixmongo.mongo.cosmos.azure.com";
        private string password = "JSlXbKbcLAKkAL2G2NAGEEoHxo8KxajMloBHkE1vgRZtwGQegxZZQYj82IQTPZIqxz6iqCk0eiWSu89OJYKCBQ==";

        // This sample uses a database named "Tasks" and a 
        //collection named "TasksList".  The database and collection 
        //will be automatically created if they don't already exist.
        private string dbName = "User";
        private string collectionName = "UserList";

        // Default constructor.        
        public Dal()
        {
        }

        // Gets all Task items from the MongoDB server.        
        public List<User> GetAllUsers()
        {
            try
            {
                var collection = GetUserCollection();
                return collection.Find(new BsonDocument()).ToList();
            }
            catch (MongoConnectionException)
            {
                return new List<User>();
            }
        }
        public async Task<User> GetUser(string name, string password)
        {
            try
            {
                var collection = GetUserCollection();

                var filter = new BsonDocument("Name", name);
                User newUser = null;
                try
                {
                    await collection.Find(filter)
                         .ForEachAsync(u => newUser = new User(u._id, u.Name, u.Pwd, u.Email, u.Shows));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Not Found." + ex.Message);
                    return await Task.FromResult(new User());
                }

                if (newUser == null)
                {
                    return await Task.FromResult(new User());

                }
                else if (BC.Verify(password, newUser.Pwd))
                {
                    return newUser;
                }
                return await Task.FromResult(new User());
            }
            catch (MongoConnectionException)
            {
                return await Task.FromResult(new User());
            }
        }
        public async Task<User> UpdateUser(User user)
        {
            try
            {
                var collection = GetUserCollection();

                var filter = new BsonDocument("Name", user.Name);
                var update = new BsonDocument { { "Name", user.Name }, {"Pwd", user.Pwd }, {"Email", user.Email } };

                await collection.FindOneAndUpdateAsync(filter, update);
                return await GetUser(user.Name, user.Pwd);
            }
            catch (MongoConnectionException)
            {
                return await Task.FromResult(new User());
            }
        }
        // Creates a Task and inserts it into the collection in MongoDB.
        public async Task<User> CreateUser(User user)
        {
            var collection = GetUserCollectionForEdit();

            user.Pwd = BC.HashPassword(user.Pwd);
            User existingUser = await GetUser(user.Name, user.Pwd);
            if (existingUser.Name == null)
            {
                try
                {
                    await collection.InsertOneAsync(user);
                    return await GetUser(user.Name, user.Pwd);
                }
                catch (MongoCommandException ex)
                {
                    string msg = ex.Message;
                    return new User();
                }
            } 
            else if (existingUser.Name.Equals(user.Name))
            {
                return new User();
            }
            return new User();
        }

        public async Task DeleteUser(string name)
        {
            var collection = GetUserCollectionForEdit();
            var filter = new BsonDocument("Name", name);
            try
            {
                await collection.FindOneAndDeleteAsync(filter);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        public async Task ForgetMe(User user)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_exchange",                     //EXCHANGE creation
                                        type: "topic");

                var json = JsonConvert.SerializeObject(user);                    //MESSAGE creation
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "topic_exchange",                        //MESSAGE publishing
                                     routingKey: "user.forget",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent Update message", body);
            }
        }
        private IMongoCollection<User> GetUserCollection()
        {
            MongoClient client = new MongoClient(SetSettings());
            var database = client.GetDatabase(dbName);
            var todoTaskCollection = database.GetCollection<User>(collectionName);
            return todoTaskCollection;
        }

        private IMongoCollection<User> GetUserCollectionForEdit()
        {
            MongoClient client = new MongoClient(SetSettings());
            var database = client.GetDatabase(dbName);
            var todoTaskCollection = database.GetCollection<User>(collectionName);
            return todoTaskCollection;
        }

        # region IDisposable

        public MongoClientSettings SetSettings()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
            settings.RetryWrites = false;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            return settings;
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }
            }

            this.disposed = true;
        }

        #endregion
    }
}
