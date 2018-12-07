using System;
using KnowledgeGraph.Database.Persistence;
using KnowledgeGraph.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeGraph
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
            services.Configure<Neo4jSettings>(
                options =>
                {
                    options.ConnectionString = Configuration.GetSection("Neo4j:ConnectionString").Value;
                    options.Container = Configuration.GetSection("Neo4j:Container").Value;
                    options.IsDockerized = Configuration["DOTNET_RUNNING_IN_CONTAINER"] != null;
                    options.UserId = Configuration.GetSection("Neo4j:UserId").Value;
                    options.Password = Configuration.GetSection("Neo4j:Password").Value;
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine(options.ConnectionString);
                    Console.WriteLine("-------------------------------------------------------");
                }
            );
            services.AddSingleton<IGraphFunctions, GraphFunctions>();
            services.AddSingleton<QueueBuilder>();
            services.AddSingleton<QueueHandler>();
            services.AddSingleton<GraphDbConnection>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, QueueHandler queueHandler,QueueBuilder queueBuilder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseMvc();
        }
    }
}