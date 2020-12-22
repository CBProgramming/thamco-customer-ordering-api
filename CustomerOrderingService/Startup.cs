using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using Polly;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using IdentityModel.Client;
using OrderData;
using Order.Repository;
using StaffProduct.Facade;
using CustomerAccount.Facade;
using Invoicing.Facade;
using Review.Facade;

namespace CustomerOrderingService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication()
                .AddJwtBearer("CustomerAuth", options =>
                {
                    options.Authority = Configuration.GetValue<string>("CustomerAuthServerUrl");
                    options.Audience = "customer_ordering_api";
                })
                .AddJwtBearer("StaffAuth", options =>
                {
                    options.Authority = Configuration.GetValue<string>("StaffAuthServerUrl");
                    options.Audience = "customer_ordering_api";
                });

            services.AddAuthorization(OptionsBuilderConfigurationExtensions =>
            {
                OptionsBuilderConfigurationExtensions.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("CustomerAuth")
                .Build();

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOrAccountAPI", policy =>
                policy.AddAuthenticationSchemes("CustomerAuth")
                .RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "role" && c.Value == "Customer")
                || context.User.HasClaim(c => c.Type == "client_id" && c.Value == "customer_account_api"))
                .Build());

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOrStaffWebApp", policy =>
                policy.AddAuthenticationSchemes("CustomerAuth","StaffAuth")
                .RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "role" && c.Value == "Customer")
                || context.User.HasClaim(c => c.Type == "role" && c.Value == "ManageCustomerAccounts"))
                .Build());

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOnly", policy =>
                    policy.AddAuthenticationSchemes("CustomerAuth")
                    .RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "role" && c.Value == "Customer"))
                .Build());

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerProductAPI", policy =>
                    policy.AddAuthenticationSchemes("CustomerAuth")
                    .RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "client_id" && c.Value == "customer_product_api"))
                .Build());
        });
            
            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<OrderDb>(options => options.UseSqlServer(
                 Configuration.GetConnectionString("CustomerOrdering"), optionsBuilder =>
                 {
                     optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null);
                 }
                ));
            services.AddHttpClient("StaffProductAPI", client =>
                {
                    client.BaseAddress = new Uri("http://localhost:49268");
                })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
            services.AddScoped<IOrderRepository, OrderRepository>();

            if (Env.IsDevelopment())
            {
                services.AddScoped<IStaffProductFacade, FakeStaffProductFacade>();
                services.AddScoped<ICustomerAccountFacade, FakeCustomerFacade>();
                services.AddScoped<IInvoiceFacade, FakeInvoiceFacade>();
                services.AddScoped<IReviewFacade, FakeReviewFacade>();
            }
            else
            {
                services.AddScoped<IStaffProductFacade, StaffProductFacade>();
                services.AddScoped<ICustomerAccountFacade, CustomerFacade>();
                services.AddScoped<IInvoiceFacade, InvoiceFacade>();
                services.AddScoped<IReviewFacade, ReviewFacade>();
            }

            

            services.AddHttpClient("CustomerAccountAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("CustomerAccountUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
            
            services.AddHttpClient("InvoiceAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("InvoiceUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
            
            services.AddHttpClient("StaffProductAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("StaffProductUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
            
            services.AddHttpClient("ReviewAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("ReviewUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, OrderDb db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
