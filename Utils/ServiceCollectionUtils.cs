//namespace ReadVideo.Server.Utils
//{
//    public static class ServiceCollectionExtensions
//    {
//        public static void Decorate<TInterface, TDecorator>(this IServiceCollection services,
//            Func<TInterface, IServiceProvider, TDecorator> decoratorFactory)
//            where TDecorator : TInterface
//        {
//            var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));

//            if (wrappedDescriptor == null)
//            {
//                throw new InvalidOperationException($"Service type {typeof(TInterface).Name} has not been registered.");
//            }

//            services.Remove(wrappedDescriptor);

//            services.AddSingleton<TInterface>(serviceProvider =>
//            {
//                var originalService = (TInterface)serviceProvider.CreateInstance(wrappedDescriptor.ImplementationType);
//                return decoratorFactory(originalService, serviceProvider);
//            });
//        }
//    }
//}
