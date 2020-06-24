using Newtonsoft.Json;
using SocketAppServer.CoreServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerUtils
{
    public static class JsonExt
    {
        public static void ApplyCustomSettings(this JsonSerializer serializer)
        {
            if (AppServerConfigurator.SerializerSettings == null)
                return;

            var settings = AppServerConfigurator.SerializerSettings;
            serializer.ContractResolver = settings.ContractResolver;
            serializer.SerializationBinder = settings.SerializationBinder;
            serializer.CheckAdditionalContent = settings.CheckAdditionalContent;
            serializer.Culture = settings.Culture;
            serializer.ConstructorHandling = settings.ConstructorHandling;
            serializer.Context = settings.Context;
            serializer.DateFormatHandling = settings.DateFormatHandling;
            serializer.DateFormatString = settings.DateFormatString;
            serializer.DateParseHandling = settings.DateParseHandling;
            serializer.DateTimeZoneHandling = settings.DateTimeZoneHandling;
            serializer.DefaultValueHandling = settings.DefaultValueHandling;
            serializer.EqualityComparer = settings.EqualityComparer;
            serializer.FloatFormatHandling = settings.FloatFormatHandling;
            serializer.FloatParseHandling = settings.FloatParseHandling;
            serializer.Formatting = settings.Formatting;
            serializer.MaxDepth = settings.MaxDepth;
            serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            serializer.MissingMemberHandling = settings.MissingMemberHandling;
            serializer.NullValueHandling = settings.NullValueHandling;
            serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
            serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
            serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            serializer.SerializationBinder = settings.SerializationBinder;
            serializer.StringEscapeHandling = settings.StringEscapeHandling;
            serializer.TraceWriter = settings.TraceWriter;
            serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            serializer.TypeNameHandling = settings.TypeNameHandling;
        }
    }
}
