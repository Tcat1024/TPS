using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace TPS.Service
{
    public class ServiceObject:MarshalByRefObject
    {
        public Type callbackType;
        public string contractAssembly;
        public Type contractType;
        public InstanceContextMode instanceMode;
        public string behaviorAssembly;
        public Type behaviorType;
    }
    public class AppDomainObject
    {
        public AppDomain appdomain;
        public int usecount = 0;
    }
    public class ServiceHostObject
    {
        public ServiceHost host;
        public string serviceName;
        public ServiceHostObject(string name,ServiceHost ho)
        {
            this.serviceName = name;
            this.host = ho;
        }
    }
    public class ContractTypeAttribute:Attribute
    {
        public Type ContractType;
        public ContractTypeAttribute(Type con)
        {
            this.ContractType = con;
        }
    }
}
