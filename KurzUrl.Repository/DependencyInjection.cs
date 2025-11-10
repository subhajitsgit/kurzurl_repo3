using KurzUrl.Repository.Implementation;
using KurzUrl.Repository.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace KurzUrl.Repository
{
    public static class DependencyInjection
    {
        //test
        public static IServiceCollection AddRepositoryLayer(this IServiceCollection services)
        {
            services.AddTransient<IKurzUrlRepo, KurzUrlRepo>();
            return services;
        }
    }
}
