﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreBBS.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using NetCoreBBS.Middleware;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.WebEncoders;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using NetCoreBBS.Entities;
using NetCoreBBS.Infrastructure.Repositorys;
using NetCoreBBS.Interfaces;
using System.Security.Claims;
using NetCoreBBS.MyAttributes;
using Microsoft.AspNetCore.Authorization;

namespace NetCoreBBS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 连接sql
            services.AddDbContext<DataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")));

            /*
               services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
                });
                              services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
                });
             */


            //添加身份认证
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password = new PasswordOptions()
                {
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
            }).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IRepository<TopicNode>, Repository<TopicNode>>();
            services.AddSingleton<ITopicRepository, TopicRepository>();
            services.AddSingleton<ITopicReplyRepository, TopicReplyRepository>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<UserServices>();
            services.AddMemoryCache();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Admin",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("Admin", "Allowed");
                    });
                options.AddPolicy(
                    "Wss",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("Wss", "Allowed");
                    });

                options.AddPolicy(
                    "Two",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("Two");
                    });

                //自定义授权 --claim等于wss
                options.AddPolicy(
                   "AsWss",
                   authBuilder =>
                   {
                       authBuilder.Requirements.Add(new SameAsRequirement("wss"));
                   });
                // ClaimTypes.DateOfBirth
            });

            services.AddSingleton<IAuthorizationHandler, SameAsHandler>();


            //文字被编码 https://github.com/aspnet/HttpAbstractions/issues/315
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            env.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();

            app.UseRequestIPMiddleware();

            InitializeNetCoreBBSDatabase(app.ApplicationServices);
            app.UseDeveloperExceptionPage();



          

            app.UseStaticFiles();
            app.UseIdentity();
            app.UseStatusCodePages();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeNetCoreBBSDatabase(IServiceProvider serviceProvider)
        {
            var sql = Configuration.GetConnectionString("SqlConnection");
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<DataContext>();
                db.Database.Migrate();
                if (db.TopicNodes.Count() == 0)
                {
                    //为空时 提前注入三个板块供参考
                    db.TopicNodes.AddRange(GetTopicNodes());
                    db.SaveChanges();
                }
            }
        }

        IEnumerable<TopicNode> GetTopicNodes()
        {
            return new List<TopicNode>()
            {
                new TopicNode() { Name=".NET Core", NodeName="netcore", ParentId=0, Order=1, CreateOn=DateTime.Now, },
                new TopicNode() { Name=".NET Core", NodeName="netcore", ParentId=1, Order=1, CreateOn=DateTime.Now, },
                new TopicNode() { Name="ASP.NET Core", NodeName="aspnetcore", ParentId=1, Order=1, CreateOn=DateTime.Now, }
            };
        }
    }
}
