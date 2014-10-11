using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace TPS.Service
{
    public class ServiceHostBuilder
    {
        public static event EventHandler ServiceLoadSeccuss;
        public static List<ServiceHostObject> BuildAll(string baseaddress,IList<ServiceObject> services)
        {
            List<ServiceHostObject> myHost = new List<ServiceHostObject>();
            foreach(var item in services)
            {
                myHost.Add(SingleHostBuild(baseaddress,item));
            }
            return myHost;
        }
        public static List<ServiceHostObject> BuildAll(string baseaddress, Dictionary<string,ServiceObject> services)
        {
            List<ServiceHostObject> myHost = new List<ServiceHostObject>();
            foreach (var item in services)
            {
                myHost.Add(SingleHostBuild(baseaddress, item.Value));
            }
            return myHost;
        }
        public static ServiceHostObject SingleHostBuild(string baseaddress, ServiceObject service)
        {
            Uri serviceaddress = new Uri(baseaddress + @"/" + service.behaviorType.FullName);
            var bind = Getbind(service);
            ServiceHost host = BuildHost(service.behaviorType,service, bind, serviceaddress);
            if (service.callbackType != null)
                AddCallBackEndPoint(host, service.contractType, service.callbackType, bind, serviceaddress);
            else
            {
                AddEndPoint(host, service.contractType, bind, serviceaddress);
                AddWebGetEndPoint(host, service.contractType, new Uri(serviceaddress,@"/Test"));
            }
            AddMetaBehavior(host);
            
            if(ServiceLoadSeccuss!=null)
                ServiceLoadSeccuss(service.behaviorType.FullName, new EventArgs());
            return new ServiceHostObject(service.behaviorType.FullName,host);
        }
        private static System.ServiceModel.Channels.Binding Getbind(ServiceObject service)
        {
            if(service.callbackType!=null)
                return(new WSDualHttpBinding());
            else
                return (new WSHttpContextBinding()); 
        }
        private static ServiceHost BuildHost(Type behaviortype, ServiceObject service, System.ServiceModel.Channels.Binding bind, Uri address)
        {
            if (service.instanceMode == InstanceContextMode.Single)
                return BuildSingleObjectHost(behaviortype, bind, address);
            else
                return BuildBaseHost(behaviortype, bind, address);
        }
        private static ServiceHost BuildBaseHost(Type behaviortype, System.ServiceModel.Channels.Binding bind, Uri address)
        {
            ServiceHost Result = new ServiceHost(behaviortype, address);
            return Result;
        }
        private static ServiceHost BuildSingleObjectHost(Type behaviortype, System.ServiceModel.Channels.Binding bind, Uri address)
        {
            ServiceHost Result = new ServiceHost(Activator.CreateInstance(behaviortype), address);
            return Result;
        }
        private static void AddMetaBehavior(ServiceHost target)
        {
            var metabehavior = new ServiceMetadataBehavior();
            metabehavior.HttpGetEnabled = true;
            target.Description.Behaviors.Add(metabehavior);
        }
        private static void AddEndPoint(ServiceHost target, Type contracttype, System.ServiceModel.Channels.Binding bind, Uri address)
        {
            var con = ContractDescription.GetContract(contracttype);
            var endpoint = new ServiceEndpoint(con, bind, new EndpointAddress(address));
            target.AddServiceEndpoint(endpoint);
        }
        private static void AddWebGetEndPoint(ServiceHost target, Type contracttype, Uri address)
        {
            var con = ContractDescription.GetContract(contracttype);
            var endpoint = new ServiceEndpoint(con, new WebHttpBinding(), new EndpointAddress(address));
            endpoint.Behaviors.Add(new WebHttpBehavior());
            target.AddServiceEndpoint(endpoint);
        }
        private static void AddCallBackEndPoint(ServiceHost target, Type contracttype, Type callbackcontract, System.ServiceModel.Channels.Binding bind, Uri address)
        {
            var con = ContractDescription.GetContract(contracttype);
            con.CallbackContractType = callbackcontract;
            var endpoint = new ServiceEndpoint(con, bind, new EndpointAddress(address));
            target.AddServiceEndpoint(endpoint);
        }
    }
}
