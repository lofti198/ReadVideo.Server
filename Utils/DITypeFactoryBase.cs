using System.Collections.Concurrent;

namespace ReadVideo.Server.Utils
{
    public class DITypeFactoryBase<TKey, TService>
        where TService : notnull
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<TKey, TService> _instances = new ConcurrentDictionary<TKey, TService>();
        private readonly Func<IServiceProvider, TKey, TService> _createInstanceFunc;

        public DITypeFactoryBase(
            IServiceProvider serviceProvider,
            Func<IServiceProvider, TKey, TService> createInstanceFunc)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _createInstanceFunc = createInstanceFunc ?? throw new ArgumentNullException(nameof(createInstanceFunc));
        }

        public TService GetOrCreate(TKey key)
        {
            return _instances.GetOrAdd(key, k => _createInstanceFunc(_serviceProvider, k));
        }
    }
}
