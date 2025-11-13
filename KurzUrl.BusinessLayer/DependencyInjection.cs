using KurzUrl.BusinessLayer.Interface;
using KurzUrl.BusinessLayer.Implementation;
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
            services.AddScoped<IQRGenerator, Implementation.QRGenerator>();
            services.AddScoped<IUserLinksService, Implementation.UserLinksService>();
            services.AddScoped<IQRService, Implementation.QRService>();
            return services;
        }
    }
}
