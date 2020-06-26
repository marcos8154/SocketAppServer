using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAppServerClient
{
    public class RequestBody
    {
        [JsonIgnore]
        private JsonSerializerSettings SerializerSettings { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }
        public List<RequestParameter> Parameters { get; set; }

        public string InTo { get; set; }

        private RequestBody()
        {
            var globalConf = Client.GetConfiguration();
            if (globalConf != null)
                SerializerSettings = globalConf.SerializerSettings;
        }

        private RequestBody(JsonSerializerSettings settings)
        {
            SerializerSettings = settings;
        }

        public static RequestBody Create(string controller, string action)
        {
            RequestBody rb = new RequestBody();
            rb.Controller = controller;
            rb.Action = action;

            return rb;
        }

        public static RequestBody Create(string controller, string action,
            JsonSerializerSettings serializerSettings)
        {
            RequestBody rb = new RequestBody(serializerSettings);
            rb.Controller = controller;
            rb.Action = action;
            return rb;
        }

        public RequestBody SaveOnMemoryStorage(string storageId)
        {
            InTo = storageId;
            return this;
        }

        public RequestBody AddParameter(string name, object value)
        {
            if (Parameters == null)
                Parameters = new List<RequestParameter>();

            Parameters.Add(new RequestParameter(name, value, SerializerSettings));
            return this;
        }
    }
}
