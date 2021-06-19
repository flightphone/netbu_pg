using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Net;
using netbu.Models;
using WpfBu.Models;

namespace netbu
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("global.json")
                .Build();

            AppConfig = config;
            isPostgres = (AppConfig["isP"] == "postgres");
            string proxy = AppConfig["proxy"];
            string proxyport = AppConfig["proxyport"];
            string proxylogin = AppConfig["proxylogin"];
            string proxypassword = AppConfig["proxypassword"];
            if (!string.IsNullOrEmpty(AppConfig["proxy"]))
            {
                WebProxy wp = new WebProxy(proxy, int.Parse(proxyport));
                if (string.IsNullOrEmpty(proxylogin))
                    wp.UseDefaultCredentials = true;
                else
                    wp.Credentials = new NetworkCredential(proxylogin, proxypassword);
                WebRequest.DefaultWebProxy = wp;
                //GlobalProxySelection.Select = wp;
            }
            StartMethod();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>();
        }
      
        private static void StartMethod()
        {
            
            MainObj.IsPostgres = isPostgres;
            DBClient.CnStr = (isPostgres) ? AppConfig["cns"] : AppConfig["mscns"];
            //Для React
            MainObj.ConnectionString = DBClient.CnStr;
            
            


            //Мапим диск
            /*
            try
            {
                System.Diagnostics.Process batch = new System.Diagnostics.Process();
                batch.StartInfo.FileName = @"wwwroot\Run\disk.cmd";
                batch.Start();
            }
            catch
            {; }
            */
            //Кодировки
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        }

       

        public static System.Diagnostics.Process nodesv;

        public static IConfiguration AppConfig { get; set; }

        public static bool isPostgres { get; set; }
        public static bool FlagDadataUpdate = false;

        public static bool isNodeStart = false;
    }
}
