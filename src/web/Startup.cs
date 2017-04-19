﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using web.Data;

namespace web
{
    public class Startup
    {
        private IHostingEnvironment _env { get; set; }
        private IConfigurationRoot _config;
        private bool _useSqlServer = false;

        private DateTime _uptime = DateTime.Now;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appoptions.json", optional: true, reloadOnChange: true) //IOptionsSnapshot to live reload
                .AddEnvironmentVariables(); //override any config files / user secrets          

            _config = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                    .Configure<Configuration.Settings>(_config)
                    .Configure<Configuration.Options>(_config);

            //framework service (Mvc,EF...)                                              
            var dbList = _config.GetSection("DbList").Get<IEnumerable<Configuration.Settings.Db>>();
            if (dbList != null && dbList.Any())
            {
                // check Sql
                var dbSql = dbList.FirstOrDefault(_ => _.Type == Configuration.Settings.Db.Types.SqlServer);
                if (dbSql != null)
                {
                    _useSqlServer = true;
                    // EntityFramework
                    services.AddDbContext<Data.AppDbContext>(_ => _.UseSqlServer(dbSql.Connection));
                    // Identity
                    /*
                    services.AddIdentity<Models.Identity.User, IdentityRole>()
                        .AddEntityFrameworkStores<Data.AppDbContext>()
                        .AddDefaultTokenProviders();
                    */
                }

                //Db main repository            
                Type dbType = typeof(Data.Memory<>);
                var mainType = dbList.First().Type;
                switch (mainType)
                {
                    case Configuration.Settings.Db.Types.FileSystem:
                        dbType = typeof(Data.FileSystem<>);
                        break;
                    case Configuration.Settings.Db.Types.Mongo:
                        dbType = typeof(Data.Mongo<>);
                        break;
                    case Configuration.Settings.Db.Types.SqlServer:
                        dbType = typeof(Data.SqlServer<>);
                        break;
                }
                services.AddTransient(typeof(Data.IRepository<>), dbType);

            }

            services.AddMvc();

            //app service
            services.AddTransient<IMessage, EmailMessage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400");
                }
            });

            //Data
            // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859                
            if (_useSqlServer)
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var _db = serviceScope.ServiceProvider.GetService<Data.AppDbContext>();
                        _db.Database.EnsureCreated();
                        //Migrate()
                        //Seed
                    }
                }
                catch { }

            //Autentication

            //Mvc              
            app.UseMvc();

            app.Map("/info", _ => _.Run(async (context) =>
             {
                 var msg = @"
..####...#####...#####............####....####...#####...######.
.##..##..##..##..##..##..........##..##..##..##..##..##..##.....
.######..#####...#####...######..##......##..##..#####...####...
.##..##..##......##..............##..##..##..##..##..##..##.....
.##..##..##......##...............####....####...##..##..######.
................................................................
";
                 msg +=
                     "\n" +
                     $"Uptime: {_uptime}\n" +
                     $"ApplicationName: {_env.ApplicationName}\n" +
                     $"Environment: {_env.EnvironmentName}\n" +
                     $"MachineName: {Environment.MachineName}\n" +
                     $"ProcessorCount: {Environment.ProcessorCount}\n";

                 await context.Response.WriteAsync(msg);
             }));


        }
    }
}
