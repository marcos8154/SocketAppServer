
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using System.Net.Http.Formatting;
using System.Web.Http;

[assembly: OwinStartup(typeof(MobileAppServer.Extensions.HttpServer.Startup))]
namespace MobileAppServer.Extensions.HttpServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //configura a web api para auto-hospedagem
            var config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{controllerName}/{actionName}",
                defaults: new { controllerName = RouteParameter.Optional, actionName = RouteParameter.Optional }
           );

            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);
        }
    }
}
