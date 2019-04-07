using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HttpServerMember
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
              .UseUrls("http://*:30003")
              .UseStartup<Startup>();
        }    
    }
}
