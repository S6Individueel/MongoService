using Microsoft.AspNetCore.Mvc;
using MongoService.DAL;
using MongoService.Models;
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
        private Dal dal;
        public UserController()
        {
            dal = new Dal();
        }

        [HttpGet("create")]
        public async Task<IEnumerable<User>> CreateUser()
        {
            User user = new User("Match", "Flix", "app@gmail.com", "[{}]");
            try
            {
                await dal.CreateTask(user);
            }
            catch (Exception)
            {

                throw;
            }
            return dal.GetAllTasks();
        }
    }
}
