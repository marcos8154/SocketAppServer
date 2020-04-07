using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.DomainModelsManagement
{
    internal class DomainModelsManager : IDomainModelsManager
    {
        private List<ModelRegister> registeredModels = null;

        public DomainModelsManager()
        {
            registeredModels = new List<ModelRegister>();
        }

        public ModelRegister GetModelRegister(string typeName)
        {
            return registeredModels.FirstOrDefault(m => m.ModeName.Equals(typeName));
        }

        public void RegisterAllModels(Assembly assembly, string namespaceName)
        {
            Type[] models = GetTypesInNamespace(assembly, namespaceName);
            for (int i = 0; i < models.Length; i++)
                RegisterModelType(models[i]);
        }

        public void RegisterModelType(Type modelType)
        {
            registeredModels.Add(new ModelRegister(modelType.FullName, modelType));
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }
    }
}
