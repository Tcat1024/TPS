using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Diagnostics;

namespace TPS.Service
{
    public class ServiceContainer
    {
        public Dictionary<string, ServiceObject> Services = new Dictionary<string, ServiceObject>();
        public Dictionary<string,AppDomainObject> Appdomains = new Dictionary<string,AppDomainObject>();
        public void GetService()
        {
            this.Clear();
            DirectoryInfo BaseAddress = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = BaseAddress.GetFiles();
            foreach (var file in files)
            {
                if (file.Extension.ToLower() == ".dll")
                {
                    this.LoadServiceAssembly(file.FullName);
                }
            }
            this.CheckServices(this.Services);
            this.CheckAppDomains(this.Appdomains);
        }
        private void Clear()
        {
            Services.Clear();
            if(Appdomains.Count>0)
            {
                foreach(var item in Appdomains)
                {
                    AppDomain.Unload(item.Value.appdomain);
                }
                Appdomains.Clear();
            }
        }
        public void LoadServiceAssembly(string name)
        {
            AppDomain tempDomain = AppDomain.CreateDomain(name);
            AssemblyLoader myLoader = (AssemblyLoader)tempDomain.CreateInstanceAndUnwrap(typeof(AssemblyLoader).Assembly.FullName,typeof(AssemblyLoader).FullName);
            myLoader.Services = this.Services;
            int result= myLoader.LoadServiceAssembly(name);
            this.Services = myLoader.Services;
            if (result == 0)
                AppDomain.Unload(tempDomain);
            else
            {
                var temp = new AppDomainObject() { appdomain = tempDomain, usecount = result };
                Appdomains.Add(name, temp);
            }
        }
        private class AssemblyLoader:MarshalByRefObject
        {
            public Dictionary<string, ServiceObject> Services;
            public int LoadServiceAssembly(string name)
            {
                Assembly tempAssembly =  Assembly.LoadFrom(name);
                Type[] types = tempAssembly.GetTypes();
                int result = 0;
                foreach(var type in types)
                {
                    if (type.IsClass)
                    {
                        ServiceBehaviorAttribute[] behaviorattrs = (ServiceBehaviorAttribute[])type.GetCustomAttributes(typeof(ServiceBehaviorAttribute), true);
                        ContractTypeAttribute[] contractattrs = (ContractTypeAttribute[])type.GetCustomAttributes(typeof(ContractTypeAttribute), true);
                        if (behaviorattrs.Length == 1&&contractattrs.Length==1)
                        {
                            result++;
                            string keyname = contractattrs[0].ContractType.FullName+"-"+type.FullName;
                            if (Services.ContainsKey(keyname))
                                throw new Exception("重复的服务:\r\n" + "\tContract:\r\n" + "\t\tAssembly:" + contractattrs[0].ContractType.Assembly.FullName + "\r\n" + "\t\tName:" + contractattrs[0].ContractType.FullName + "\r\n" + "\tBehavior:\r\n" + "\t\tAssembly:" + type.Assembly.FullName + "\r\n" + "\t\tName:" + type.FullName + "\r\n");
                            else
                                Services.Add(keyname, new ServiceObject() { instanceMode = behaviorattrs[0].InstanceContextMode, behaviorAssembly = name, behaviorType = type });
                            ServiceContractAttribute[] attrs = (ServiceContractAttribute[])contractattrs[0].ContractType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
                            if (attrs.Length == 1)
                            {
                                var temp = Services[keyname];
                                temp.callbackType = attrs[0].CallbackContract;
                                temp.contractAssembly = name;
                                temp.contractType = type;
                            }
                        }
                    }                   
                }
                return result;
            }
        }
        public void CheckServices(Dictionary<string, ServiceObject> services)
        {
            foreach(var service in services)
            {
                var temp =  service.Value;
                if (temp.behaviorType == null)
                {
                    services.Remove(service.Key);
                }
                else if (temp.contractType == null)
                {
                    Appdomains[temp.behaviorAssembly].usecount--;
                    services.Remove(service.Key);
                }
            }
        }
        public void CheckAppDomains(Dictionary<string,AppDomainObject> appdomains)
        {
            foreach (var appdomain in appdomains)
            {
                var temp = appdomain.Value;
                if (temp.usecount == 0)
                {
                    appdomains.Remove(appdomain.Key);
                    AppDomain.Unload(temp.appdomain);
                }
            }
        }
    }
    
}
