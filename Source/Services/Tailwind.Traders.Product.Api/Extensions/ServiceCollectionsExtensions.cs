﻿using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Tailwind.Traders.Product.Api.Infrastructure;
using Tailwind.Traders.Product.Api.Mappers;
using Tailwind.Traders.Product.Api.Security;

namespace Tailwind.Traders.Product.Api.Extensions
{
    public static class ServiceCollectionsExtensions
    {
        public static IServiceCollection AddProductsContext(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<ProductContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionString"], sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            return service;
        }

        public static IServiceCollection AddModulesProducts(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddTransient<IContextSeed<ProductContext>, ProductContextSeed>()
                .AddTransient<IProcessFile, ProccessCsv>()
                .AddTransient<ClassMap, ProductBrandMap>()
                .AddTransient<ClassMap, ProductFeatureMap>()
                .AddTransient<ClassMap, ProductItemMap>()
                .AddTransient<ClassMap, ProductTypeMap>()
                .AddTransient<ClassMap, ProductTagMap>()
                .AddTransient<MapperDtos>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<HttpClientAuthorizationDelegatingHandler>()
                .AddHttpClient(configuration["ProductVisitsUrl"])
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            service.Configure<AppSettings>(configuration);

            return service;
        }

        public static IServiceCollection AddSecurity(this IServiceCollection service)
        {
            service.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            return service;
        }
    }
}
