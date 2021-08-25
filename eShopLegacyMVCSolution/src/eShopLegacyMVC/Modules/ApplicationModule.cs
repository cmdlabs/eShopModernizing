using Autofac;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eShopLegacyMVC.Modules
{
    public class ApplicationModule : Module
    {
        private bool useMockData;

        public ApplicationModule(bool useMockData)
        {
            this.useMockData = useMockData;
        }
        protected override void Load(ContainerBuilder builder)
        {
            if (this.useMockData)
            {
                builder.RegisterType<CatalogServiceMock>()
                    .As<ICatalogService>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<CatalogService>()
                    .As<ICatalogService>()
                    .InstancePerLifetimeScope();

                // TODO: More conventionally, would move the below to services.AddDbContext in Startup.cs
                builder.RegisterType<CatalogDBContext>()
                .InstancePerLifetimeScope();

                builder.Register(c =>
                {
                    var configuration = c.Resolve<IConfiguration>();
                    var optionsBuilder = new DbContextOptionsBuilder<CatalogDBContext>();
                    return optionsBuilder.UseSqlServer(configuration.GetConnectionString("CatalogDBContext")).Options;
                }).AsSelf().InstancePerLifetimeScope();
            }
        }
    }
}