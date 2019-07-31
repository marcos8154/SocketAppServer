using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace MobileAppServer.ServerObjects
{
    internal class ServerInfoController : IController
    {
        public ActionResult Reboot()
        {
            Server.GlobalInstance.SendReboot();
            return ActionResult.Json(true, 600, "Ok");
        }

        public ActionResult FullServerInfo()
        {
            Thread.Sleep(2000);
            ServerInfo info = new ServerInfo();

            DirectoryInfo dInfo = new DirectoryInfo(@".\Mappings\");
            foreach (FileInfo fi in dInfo.GetFiles())
            {
                if (!fi.Extension.Equals(".xml"))
                    continue;

                ControllerInfo controllerInfo = new ControllerInfo();
                controllerInfo.ControllerName = fi.Name.Replace(".xml", string.Empty);

                info.ServerControllers.Add(GetControllerInfo(fi.Name));

            }

            return ActionResult.Json(info);
        }

        public ActionResult DownloadFile(string path)
        {
            return ActionResult.File(path);
        }

        private ControllerInfo GetControllerInfo(string mappingName)
        {
            StreamReader stream = null;

            try
            {
                stream = new StreamReader(
                    Directory.GetCurrentDirectory() + $@"\Mappings\{mappingName}", Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.Load(stream);
                stream.Close();

                ControllerInfo info = new ControllerInfo();
                info.ControllerName = mappingName.Replace(".xml", string.Empty);
                info.ControllerActions = ListActions(mappingName);

                XmlNode node = RequestProccess.FindNode(xml.ChildNodes, "ControllerMapping");
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.Equals("class"))
                        info.ControllerClass = attr.Value;
                }


                return info;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<string> ListActions(string controllerMappingFileName)
        {
            StreamReader stream = null;

            List<string> result = new List<string>();
            try
            {
                stream = new StreamReader(
                    Directory.GetCurrentDirectory() + $@"\Mappings\{controllerMappingFileName}", Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.Load(stream);
                stream.Close();

                XmlNode node = RequestProccess.FindNode(xml.ChildNodes, "ControllerMapping");
                foreach (XmlNode chNode in node.ChildNodes)
                    if (chNode.Name.Equals("RequestMapping"))
                        foreach (XmlAttribute a in chNode.Attributes)
                            result.Add(a.Value);

                return result;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
    }
}
