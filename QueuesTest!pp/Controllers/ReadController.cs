using Beanstalk.Core;
using Microsoft.AspNetCore.Mvc;
using QueuesTest_pp.OtherStorage;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text;

namespace QueuesTest_pp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadController : ControllerBase
    {

        private IConnectionMultiplexer _rdb;
        private IAofStorage _aof;

        private IDatabase _rdbDb;
        private IDatabase _aofDb;

        public ReadController(IConnectionMultiplexer rdb, IAofStorage aof)
        {
            _rdb = rdb;
            _aof = aof;

            _rdbDb = _rdb.GetDatabase();
            _aofDb = _aof.GetDatabase();
        }


        [HttpGet]
        [Route("read-beanstalk")]
        public async Task<ActionResult> Read(int count = 10)
        {
            //var current = 0;
            //var step = 100;
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();



            var i = 0;
            using (var client = new BeanstalkConnection("beanstalkd", 11300))
            {
                main.Start();
                await client.Watch("mytube");
                while (i<count)
                {
                    var job = await client.Reserve(TimeSpan.FromSeconds(10));
                  
                    if (job == null)
                    {
                        break;
                    }
                    i++;
                    await client.Delete(job.Id);
                }

                main.Stop();

            }

            log.AppendLine($"Full execution: {main.Elapsed}, Count {i}");
            return Ok(new { Log = log.ToString() });


        }


        [HttpGet]
        [Route("read-rdb")]
        public async Task<ActionResult> ReadRdb(int count = 10)
        {
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();


            var i = 0;
  
            main.Start();
               
            while (i < count)
            {
                var job = _rdbDb.ListRightPop("users");

                if (!job.HasValue)
                {
                    break;
                }
                i++;
            }

            main.Stop();

            log.AppendLine($"Full execution: {main.Elapsed}, Count {i}");
            return Ok(new { Log = log.ToString() });


        }


        [HttpGet]
        [Route("read-aof")]
        public async Task<ActionResult> ReadAof(int count = 10)
        {
            var log = new StringBuilder();
            Stopwatch main = new Stopwatch();


            var i = 0;

            main.Start();

            while (i < count)
            {
                var job = _aofDb.ListRightPop("users");

                if (!job.HasValue)
                {
                    break;
                }
                i++;
            }

            main.Stop();

            log.AppendLine($"Full execution: {main.Elapsed}, Count {i}");
            return Ok(new { Log = log.ToString() });



        }
    }
}