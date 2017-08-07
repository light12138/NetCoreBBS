using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using IdentityServer4;

namespace IdentityService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            #region 连接数据库的用户
            var connectionString = @"server=.;database=IdentityServer4;trusted_connection=yes";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // configure identity server with in-memory users, but EF stores for clients and resources
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddTestUsers(config.GetUsers())
                .AddConfigurationStore(builder =>
                    builder.UseSqlServer(connectionString, options =>
                        options.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(builder =>
                    builder.UseSqlServer(connectionString, options =>
                        options.MigrationsAssembly(migrationsAssembly)));
            #endregion

            #region 测试用户
            //services.AddMvc();
            //services.AddIdentityServer()
            //    .AddTemporarySigningCredential()
            //    .AddInMemoryIdentityResources(config.GetIdentityResources())
            //    .AddInMemoryClients(config.GetClients())
            //    .AddTestUsers(config.GetUsers()); 
            #endregion

            #region 默认配置
            ////RSA：证书长度2048以上，否则抛异常
            ////配置AccessToken的加密证书
            //var rsa = new RSACryptoServiceProvider();
            ////从配置文件获取加密证书
            //rsa.ImportCspBlob(Convert.FromBase64String(Configuration["SigningCredential"]));
            ////配置IdentityServer4
            //services.AddSingleton<IClientStore, MyClientStore>();   //注入IClientStore的实现，可用于运行时校验Client
            //services.AddSingleton<IScopeStore, MyScopeStore>();    //注入IScopeStore的实现，可用于运行时校验Scope
            ////注入IPersistedGrantStore的实现，用于存储AuthorizationCode和RefreshToken等等，默认实现是存储在内存中，
            ////如果服务重启那么这些数据就会被清空了，因此可实现IPersistedGrantStore将这些数据写入到数据库或者NoSql(Redis)中
            //services.AddSingleton<IPersistedGrantStore, MyPersistedGrantStore>();
            //services.AddIdentityServer()
            //    .AddSigningCredential(new RsaSecurityKey(rsa));
            ////.AddTemporarySigningCredential()   //生成临时的加密证书，每次重启服务都会重新生成
            ////.AddInMemoryScopes(Config.GetScopes())    //将Scopes设置到内存中
            ////.AddInMemoryClients(Config.GetClients())    //将Clients设置到内存中 
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseIdentityServer();

           // app.UseIdentityServer();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });




            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
