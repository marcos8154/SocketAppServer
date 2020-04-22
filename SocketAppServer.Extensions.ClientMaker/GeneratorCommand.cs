using SocketAppServer.CoreServices.CLIHost;
using System;
using System.Collections.Generic;
using System.Text;
using SocketAppServerClient;
using System.IO;
using SocketAppServer.ManagedServices;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.ControllerManagement;
using System.Reflection;
using SocketAppServer.ServerObjects;

namespace SocketAppServer.Extensions.ClientMaker
{
    public class GeneratorCommand : ICLIClient
    {
        private string targetDirectory;
        private string targetNamespace;

        private List<string> customNamespacesUsing = new List<string>();

        private void FillParameters()
        {
            Console.WriteLine("Target directory path:");
            targetDirectory = Console.ReadLine();
            if (!Directory.Exists(targetDirectory))
                throw new Exception("Invalid path");

            Console.WriteLine("Namespace:");
            targetNamespace = Console.ReadLine();
            if (string.IsNullOrEmpty(targetNamespace))
                throw new Exception("Namespace cannot be empty");

            FillCustomUsingNamespaces();
        }

        private void FillCustomUsingNamespaces()
        {
            Console.WriteLine("Add custom 'using' namespace (name only):  [type empty ENTER to continue/ignore]");
            string custom = Console.ReadLine();
            if (string.IsNullOrEmpty(custom))
                return;
            customNamespacesUsing.Add(custom);
            FillCustomUsingNamespaces();
        }

        public void Activate()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*******************");
            Console.WriteLine("*   ClientMaker   *");
            Console.WriteLine("*******************");
            Console.WriteLine(@"This extension will generate the source files (.cs) of 
classes to access your server's controllers and actions.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"WARNING: supported methods must have the annotation / attribute [ServerAction],
and also cannot return ActionResult. Instead, return void or your return object directly");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Answer the following parameters: \n");

            FillParameters();
            IServiceManager manager = ServiceManager.GetInstance();
            ILoggingService logging = manager.GetService<ILoggingService>();
            IDomainModelsManager models = manager.GetService<IDomainModelsManager>();
            IControllerManager controllers = manager.GetService<IControllerManager>();

            foreach (ControllerRegister controller in controllers.GetRegisteredControllers())
                if (controller.Type.Name != "ServerInfoController")
                    WriteControllerClient(controller);
        }

        private bool IsValidMethod(MethodInfo method, string controllerName)
        {
            bool valid = (method.GetCustomAttribute<ServerAction>() != null &&
                 method.ReturnType != (typeof(ActionResult)));

            if (!valid)
            {
                if (!method.Name.StartsWith("ToString") &&
                    !method.Name.StartsWith("Equals") &&
                    !method.Name.StartsWith("GetHashCode") &&
                    !method.Name.StartsWith("GetType"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"    Method '{method.Name}' in '{controllerName}' It is not supported.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            return valid;
        }

        private string GetMethodParameters(MethodInfo info)
        {
            string result = "";
            foreach (ParameterInfo parameter in info.GetParameters())
                if (parameter.ParameterType != typeof(SocketRequest))
                    result += $"{GetTypeDescription(parameter.ParameterType)} {parameter.Name}, ";
            if (result.EndsWith(", "))
                result = result.Substring(0, result.Length - 2);
            return result;
        }

        private string GetTypeDescription(Type returnType)
        {
            if (returnType.Name == "Void")
                return "void";

            if (returnType == typeof(List<>) ||
                returnType.Name.Contains("List"))
            {
                var type = returnType.GenericTypeArguments[0];
                return $"List<{type.Name}>";
            }
            else
                return returnType.Name;
        }

        private string GetMethodBody(string controllerName, MethodInfo method)
        {
            string body = $@"            Client client = new Client();
            RequestBody rb = RequestBody.Create(""{controllerName}"", ""{method.Name}"")";
            foreach (var parameter in method.GetParameters())
                if (parameter.ParameterType != typeof(SocketRequest))
                    body += $@"
                .AddParameter(""{parameter.Name}"", {parameter.Name})";
            body += $@";
            client.SendRequest(rb);";

            if (method.ReturnType.Name == "Void")
            {
                body += @"
            client.GetResult();";
                return body;
            }

            body += $@"
            {GetTypeDescription(method.ReturnType)} result = client.GetResult<{GetTypeDescription(method.ReturnType)}> ();
            return result;";
            return body;
        }

        private void WriteControllerClient(ControllerRegister controller)
        {
            Console.WriteLine($"Writing '{controller.Name}'...");
            string customUsing = "";
            foreach (string customNamespace in customNamespacesUsing)
                customUsing += $"using {customNamespace};\n";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($@"using System;
using System.Collections.Generic;
using System.Text;
using SocketAppServerClient;

{customUsing}

namespace {targetNamespace} 
{{
    public class {controller.Name}
    {{");

            Type cType = controller.Type;
            foreach (var method in cType.GetMethods())
            {
                if (!IsValidMethod(method, controller.Name))
                    continue;

                string parameters = GetMethodParameters(method);

                string methodDeclaration = $@"        public {GetTypeDescription(method.ReturnType)} {method.Name} ({parameters})
        {{
{GetMethodBody(controller.Name, method)}
        }}

";

                sb.AppendLine(methodDeclaration);
                Console.WriteLine($"    Action '{method.Name}' included.");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText($@"{targetDirectory}\{controller.Name}.cs", sb.ToString());
        }
    }
}
