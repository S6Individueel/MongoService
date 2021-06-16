using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoService.Models
{
    public class User
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid _id { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Pwd")]
        public string Pwd { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        [BsonElement("Shows")]
        public string Shows { get; set; }

        public User()
        {

        }
        public User(string Name, string Pwd, string Email, string Shows)
        {
            this.Name = Name;
            this.Pwd = Pwd;
            this.Email = Email;
            this.Shows = Shows;
        }
        public User(Guid id,string Name, string Pwd, string Email, string Shows)
        {
            _id = id;
            this.Name = Name;
            this.Pwd = Pwd;
            this.Email = Email;
            this.Shows = Shows;
        }
    }
}
