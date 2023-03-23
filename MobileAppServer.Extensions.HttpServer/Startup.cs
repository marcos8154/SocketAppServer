
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Swashbuckle.Application;
using System.Net.Http.Formatting;
using System.Web.Http;

[assembly: OwinStartup(typeof(SocketAppServer.Extensions.HttpServer.Startup))]
namespace SocketAppServer.Extensions.HttpServer
{
    public class Startup
    {
        internal static string API_TITLE = "Web Api";
        internal static string API_VERSION = "v1";
        public void Configuration(IAppBuilder app)
        {
            //configura a web api para auto-hospedagem
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
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

            config
              .EnableSwagger(c => c.SingleApiVersion(API_VERSION, API_TITLE))
              .EnableSwaggerUi();

        }
    }
}
