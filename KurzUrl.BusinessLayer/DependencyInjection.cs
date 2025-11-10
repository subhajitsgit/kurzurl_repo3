using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Implementation;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KurzUrl.BusinessLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            services.AddScoped<IKurzUrl, Implementation.KurzUrl>();
            return services;
        }
    }
}
