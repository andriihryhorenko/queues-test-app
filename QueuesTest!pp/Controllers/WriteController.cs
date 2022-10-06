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
    public class WriteController : ControllerBase
    {

        private static Random random = new Random();
        private IConnectionMultiplexer _rdb;
        private IAofStorage _aof;

        private IDatabase _rdbDb;
        private IDatabase _aofDb;

        public WriteController(IConnectionMultiplexer rdb, IAofStorage aof)
        {
            _rdb = rdb;
            _aof = aof;

            _rdbDb = _rdb.GetDatabase();
            _aofDb = _aof.GetDatabase();
        }


        [HttpGet]
        [Route("add-beanstalk")]
        public async Task<ActionResult> Add(int count = 10)
        {
            //var current = 0;
            //var step = 100;
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();
         
            var users = GenerateRandomUsers(count);

            using (var client = new BeanstalkConnection("beanstalkd", 11300))
            {
                main.Start();
                await client.Use("default");
                foreach(var user in users)
                {
                    await client.Put(JsonConvert.SerializeObject(user));
                }
                main.Stop();

            }

            log.AppendLine($"Full execution: {main.Elapsed}");
            return Ok(new { Log = log.ToString() });


        }


        [HttpGet]
        [Route("add-rdb")]
        public async Task<ActionResult> AddRdb(int count = 10)
        {
            //var current = 0;
            //var step = 100;
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();

            var users = GenerateRandomUsers(count);

            main.Start();
            foreach (var user in users)
            {
                _rdbDb.ListRightPush("users", JsonConvert.SerializeObject(user));
                //await client.Put(JsonConvert.SerializeObject(user));
            }
            main.Stop();

            

            log.AppendLine($"Full execution: {main.Elapsed}");
            return Ok(new { Log = log.ToString() });


        }


        [HttpGet]
        [Route("add-aof")]
        public async Task<ActionResult> AddAof(int count = 10)
        {
            //var current = 0;
            //var step = 100;
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();

            var users = GenerateRandomUsers(count);

            main.Start();
            foreach (var user in users)
            {
                _aofDb.ListRightPush("users", JsonConvert.SerializeObject(user));
                //await client.Put(JsonConvert.SerializeObject(user));
            }
            main.Stop();



            log.AppendLine($"Full execution: {main.Elapsed}");
            return Ok(new { Log = log.ToString() });


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