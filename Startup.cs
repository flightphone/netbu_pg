using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace netbu
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // установка конфигурации подключения, Аутентификация
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Home/Login");
                    //options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);


                });

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });
            //Задача по таймеру    
            //services.AddHostedService<TimedHostedService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            //17/10/2020 выводим ошибки в прод. для диагностики
            
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage();
            }
            else
            
            app.UseExceptionHandler(errorApp =>
               {
                   errorApp.Run(async context =>
                   {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html";
                        
                        var err =
                            context.Features.Get<IExceptionHandlerFeature>();
                        
                        string html = @"<!DOCTYPE html><html lang=""ru""><head><meta charset=""utf-8""><title>Ошибка</title></head><body>" +
                        "Error: " + err.Error.Message + "<br/>" + err.Error.StackTrace + "</bode></html>";
                        
                        if (env.IsDevelopment ()) {
                            await context.Response.WriteAsync(html);
                        }
                        
                        string mes = $"{err.Error.Message}\r\n{err.Error.StackTrace} - {DateTime.Now}\r\n\r\n";
                        await File.AppendAllTextAsync("netbu_error.log", mes);
                    });
               });
            
            //Аутентификация
            app.UseAuthentication();

            app.UseCors(builder => 
                //builder.SetIsOriginAllowed(s => (s == "http://localhost:8080" )).AllowCredentials().AllowAnyHeader().AllowAnyMethod()
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()
            );
            //app.UseDefaultFiles ();
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