using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoService.DAL;
using MongoService.Models;
using MongoService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MongoService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IUserRepository userRepository;
        public UserController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
        }

        [HttpGet("create")]
        public async Task<User> CreateUser(string name, string password, string email)
        {
            User user = new User(name, password, email, "[{}]");
            return await userRepository.CreateUser(user);
        }

        [HttpGet("getuser")]
        public async Task<User> GetUser(string name, string password)
        {
            return await userRepository.GetUser(name, password);
        }

        [HttpDelete("deleteuser")]
        public async Task DeleteUser(string name)
        {
            await userRepository.DeleteUser(name);
        }

        [HttpPut("updateuser")]
        public async Task<User> UpdateUser(User user)
        {
            return await userRepository.UpdateUser(user);
        }

        [HttpGet("forgetme")]
        public async Task<string> ForgetMe(string name, string password)
        {
            User newUser = await userRepository.GetUser(name, password);
            newUser.Pwd = password;
            await userRepository.ForgetMe(newUser);
            return $"Deleted {newUser.Name}";
        }
    }
}
