using Microsoft.AspNetCore.Mvc;
using MongoService.DAL;
using MongoService.Models;
using MongoService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IUserRepository userRepository;
        private Dal dal;
        public UserController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
            dal = new Dal();
        }

        [HttpPost("create")]
        public async Task<User> PostUser(string name, string password, string email)
        {
            User user = new User(name, password, email, "[{}]");
            await userRepository.CreateUser(user);
            return await userRepository.GetUser(name);
/*            User user = new User(name, password, email, "[{}]");
            try
            {
                await dal.CreateUser(user);
            }
            catch (Exception)
            {

                throw;
            }
            return await dal.GetUser(name);*/
        }

        [HttpGet("getuser")]
        public async Task<User> GetUser(string name)
        {
            return await userRepository.GetUser(name);
        }

        [HttpGet("deleteuser")]
        public async Task DeleteUser(string name)
        {
            await userRepository.DeleteUser(name);
        }

        [HttpGet("updateuser")]
        public async Task<User> UpdateUser(User user)
        {
            return await userRepository.UpdateUser(user);
        }
    }
}
