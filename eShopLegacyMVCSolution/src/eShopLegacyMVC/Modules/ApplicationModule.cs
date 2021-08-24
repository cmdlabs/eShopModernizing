using Autofac;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Modules
{
    public class ApplicationModule : Module
    {
        private bool useMockData;
        private readonly string connectionString;

        public ApplicationModule(bool useMockData, string connectionString)
        {
            this.useMockData = useMockData;
            this.connectionString = connectionString;
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
            }

            builder.RegisterType<CatalogDBContext>()
                .WithParameter("connectionString", connectionString)
                .InstancePerLifetimeScope();

            builder.RegisterType<CatalogDBInitializer>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();
        }
    }
}