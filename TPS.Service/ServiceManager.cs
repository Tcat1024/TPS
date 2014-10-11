using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace TPS.Service
{
    public class ServiceManager
    {
        public string IPAddress = "localhost";
        public string PortNumber = "8999";
        public string baseAddress;
        public ServiceContainer myServices = new ServiceContainer();
        public List<ServiceHostObject> myHosts;
        public event EventHandler ServiceStart;
        public event EventHandler ServiceStop;
        public event EventHandler ServiceAdded;
        public event EventHandler ServiceRemoved;
        public ServiceManager()
        {
            baseAddress = @"http://" + IPAddress + ":" + PortNumber;
        }
        public ServiceManager(string ipaddress,string portnumber):this()
        {
            IPAddress = ipaddress;
            PortNumber = portnumber;
        }
        public void GetServices()
        {
            myServices.GetService();
        }
        public void GetHosts()
        {
            if (myHosts != null)
                this.ClearHosts();
            myHosts = ServiceHostBuilder.BuildAll(this.baseAddress, myServices.Services);
        }
        public void AddHost(ServiceObject service)
        {
            if (myHosts == null)
                myHosts = new List<ServiceHostObject>();
            var temp = ServiceHostBuilder.SingleHostBuild(this.baseAddress, service);
            myHosts.Add(temp);
            if(ServiceAdded!=null)
                ServiceAdded(temp.serviceName,new EventArgs());
        }
        public void RemoveHost(ServiceHostObject hostObj)
        {
            Stop(hostObj);
            myHosts.Remove(hostObj);
            if(ServiceRemoved!=null)
                ServiceRemoved(hostObj.serviceName, new EventArgs());
        }
        public void StartAll()
        {
            if (myHosts != null)
            {
                foreach (var item in myHosts)
                {
                    Start(item);
                }
            }
        }
        public void StopAll()
        {
            if (myHosts != null)
            {
                foreach (var item in myHosts)
                {
                    Stop(item);
                }
            }
        }
        public void Start(ServiceHostObject hostObj)
        {
            if (hostObj.host.State != CommunicationState.Opened)
            {
                hostObj.host.Open();
                if(ServiceStart!=null)
                    ServiceStart(hostObj.serviceName, new EventArgs());
            }
        }
        public void Stop(ServiceHostObject hostObj)
        {
            if (hostObj.host.State != CommunicationState.Closed)
            {
                hostObj.host.Close();
                if(ServiceStop!=null)
                    ServiceStop(hostObj, new EventArgs());
            }
        }
        public void ClearHosts()
        {
            if(myHosts!=null)
            {
                foreach(var item in myHosts)
                {
                    Stop(item);
                }
                myHosts.Clear();
            } 
        }
    }
}
