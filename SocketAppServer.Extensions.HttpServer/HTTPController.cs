using MobileAppServerClient;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Http;

namespace MobileAppServer.Extensions.HttpServer
{
    public class HTTPController : ApiController
    {
        [HttpGet]
        [Route("api/http/{controllerName}/{actionName}")]
        public IHttpActionResult HttpGet(string controllerName, string actionName, 
            [FromBody] RequestParameter[] parameters)
        {
            try
            {
                var rb = RequestBody.Create(controllerName, actionName);
                parameters.ToList().ForEach(p => rb.AddParameter(p.Name, p.Value));

                Client client = new Client();
                client.Encoding = ServerObjects.Server.GlobalInstance.ServerEncoding;
                client.SendRequest(rb);
                var result = client.GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult HttpPost([FromBody]RequestBody rb)
        {
            try
            {
                Client client = new Client();
                client.Encoding = ServerObjects.Server.GlobalInstance.ServerEncoding;
                client.SendRequest(rb);
                var result = client.GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
