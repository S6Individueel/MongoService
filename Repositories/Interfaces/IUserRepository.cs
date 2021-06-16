using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> GetUser(string name, string password);
        public List<User> GetAllUsers();
        public Task CreateUser(User user);
        public Task DeleteUser(string name);
        public Task<User> UpdateUser(User user);
        public Task ForgetMe(User user);
    }
}
