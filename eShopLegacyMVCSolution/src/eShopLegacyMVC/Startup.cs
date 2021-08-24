// This Startup file is based on ASP.NET Core new project templates and is included
// as a starting point for DI registration and HTTP request processing pipeline configuration.
// This file will need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core startup files, see https://docs.microsoft.com/aspnet/core/fundamentals/startup

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using eShopLegacyMVC.Modules;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace eShopLegacyMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(ConfigureMvcOptions)
                // Newtonsoft.Json is added for compatibility reasons
                // The recommended approach is to use System.Text.Json for serialization
                // Visit the following link for more guidance about moving away from Newtonsoft.Json to System.Text.Json
                // https://docs.microsoft.com/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to
                .AddNewtonsoftJson(options =>
                {
                    options.UseMemberCasing();
                });

        }

        public void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
        {
            // Register your own things directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory
            // for you.
            builder.RegisterModule(new ApplicationModule(configuration.GetValue<bool>("UseMockData")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            app.Use(async (context, next) =>
            {
                LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();

                LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo(context);

                log.Debug("BeginRequest");

                await next.Invoke();
            });
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Catalog}/{action=Index}/{id?}");
            });
        }

        private void ConfigureMvcOptions(MvcOptions mvcOptions)
        { 
        }

        public class ActivityIdHelper
        {
            public override string ToString()
            {
                if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                {
                    Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                }

                return Trace.CorrelationManager.ActivityId.ToString();
            }
        }

        public class WebRequestInfo
        {
            private readonly HttpContext context;

            public WebRequestInfo(HttpContext context)
            {
                this.context = context;
            }


            public override string ToString()
            {
                return context.Request?.GetDisplayUrl() + ", " + context.Request?.Headers[HeaderNames.UserAgent];
            }
        }
    }
}
