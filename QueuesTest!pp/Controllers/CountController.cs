using Beanstalk.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QueuesTest_pp.Model;
using QueuesTest_pp.OtherStorage;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text;

namespace QueuesTest_pp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountController : ControllerBase
    {

        private static Random random = new Random();
        private IConnectionMultiplexer _rdb;
        private IAofStorage _aof;

        private IDatabase _rdbDb;
        private IDatabase _aofDb;

        public CountController(IConnectionMultiplexer rdb, IAofStorage aof)
        {
            _rdb = rdb;
            _aof = aof;

            _rdbDb = _rdb.GetDatabase();
            _aofDb = _aof.GetDatabase();
        }




        [HttpGet]
        [Route("count-beanstalk")]
        public async Task<ActionResult> Count()
        {

            using (var client = new BeanstalkConnection("beanstalkd", 11300))
            {
                var stats = await client.StatsTube("default");
                return Ok(stats);
       

            }


        }

        [HttpGet]
        [Route("count-rdb")]
        public async Task<ActionResult> CountRdb()
        {
            var count = _rdbDb.ListLength("users");
            return Ok(count);


        }


        [HttpGet]
        [Route("count-aof")]
        public async Task<ActionResult> CountAof()
        {

            var count = _aofDb.ListLength("users");
            return Ok(count);


        }



        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }


        private List<User> GenerateRandomUsers(int numberOfUsers)
        {
            var users = new List<User>();
            for (var i = 0; i < numberOfUsers; i++)
            {
                users.Add(new Model.User()
                {
                    UserName = RandomString(8),
                    UniqueName = Guid.NewGuid().ToString(),
                    BirthDate = RandomDay(),
                    CreateTime = DateTime.Now,
                });
            }
            return users;
        }

        private User GenerateSingleUser()
        {
            return new Model.User()
            {
                UserName = RandomString(8),
                UniqueName = Guid.NewGuid().ToString(),
                BirthDate = RandomDay(),
                CreateTime = DateTime.Now,
            };
        }
    }
}