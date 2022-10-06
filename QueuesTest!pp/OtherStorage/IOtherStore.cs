using StackExchange.Redis;

namespace QueuesTest_pp.OtherStorage
{
    public interface IAofStorage
    {
        IDatabase GetDatabase();
    }

    public class OtherStorage : IAofStorage
    {
        private IConnectionMultiplexer _muxer;

        public OtherStorage(ConfigurationOptions options)
        {
            _muxer = ConnectionMultiplexer.Connect(options);
        }

        public IDatabase GetDatabase()
        {
            return _muxer.GetDatabase();
        }

       
    }
}
