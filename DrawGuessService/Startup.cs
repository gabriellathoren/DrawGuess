using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DrawGuessService
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            var db = new DrawGuessContext(new DbContextOptionsBuilder<DrawGuessContext>()
                .UseSqlServer(Constants.SqlAzureConnectionString).Options);
            services.AddScoped<ICustomerRepository, SqlCustomerRepository>(_ => new SqlCustomerRepository(db));
            services.AddScoped<IOrderRepository, SqlOrderRepository>(_ => new SqlOrderRepository(db));
            services.AddScoped<IProductRepository, SqlProductRepository>(_ => new SqlProductRepository(db));
            services.AddMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
