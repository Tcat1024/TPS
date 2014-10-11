using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using testBll;
using System.Threading;
using System.Threading.Tasks;

namespace testService
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,InstanceContextMode = InstanceContextMode.Single)]
    [TPS.Service.ContractType(typeof(testIService.IService))]
    public class serviceclass : testIService.IService, IDisposable
    {
        public MainBll mybll;
        private Dictionary<testIService.IServiceCallBack, SingleWork> subscribers = new Dictionary<testIService.IServiceCallBack, SingleWork>();
        public serviceclass()
        {
            mybll = new MainBll();
            mybll.Start();
        }
        public bool Subscribe()
        {
            try
            {
                var temp = OperationContext.Current.GetCallbackChannel<testIService.IServiceCallBack>();
                lock (subscribers)
                {
                    if(!subscribers.ContainsKey(temp))
                    {
                        var worker = new SingleWork(temp, this.mybll, this);
                        subscribers.Add(temp,worker);
                        worker.Start();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Unsubscribe()
        {
            try
            {
                var temp = OperationContext.Current.GetCallbackChannel<testIService.IServiceCallBack>();
                lock (temp)
                {
                    if (subscribers.ContainsKey(temp))
                    {
                        subscribers[temp].Stop();
                        subscribers.Remove(temp);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Unsubscribe(testIService.IServiceCallBack target)
        {
            try
            {
                lock (target)
                {
                    if (subscribers.ContainsKey(target))
                    {
                        subscribers[target].Stop();
                        subscribers.Remove(target);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (this.mybll != null)
                this.mybll.Stop();
        }
    }
    /*
    public class serviceclass : testIService.IService,IDisposable
    {
        public MainBll mybll;
        private List<testIService.IServiceCallBack> subscribers = new List<testIService.IServiceCallBack>();
        public serviceclass()
        {
            mybll = new MainBll();
            mybll.Start();
            this.mybll.newData += Thread_Start;
        }
        public bool Subscribe()
        {
            try
            {
                lock (subscribers)
                {
                    subscribers.Add(OperationContext.Current.GetCallbackChannel<testIService.IServiceCallBack>());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Unsubscribe()
        {
            try
            {
                lock (subscribers)
                {
                    subscribers.Remove(OperationContext.Current.GetCallbackChannel<testIService.IServiceCallBack>());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Thread_Start(string input)
        {
            lock (subscribers)
            {
                foreach (var item in subscribers)
                {
                    if ((item as ICommunicationObject).State == CommunicationState.Opened)
                    {
                        item.set(input + ", Service set thread is " + Thread.CurrentThread.ManagedThreadId);
                    }
                    else
                    {
                        subscribers.Remove(item);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this.mybll != null)
                this.mybll.Stop();
        }
    }
    */
    public class SingleWork:IDisposable
    {
        testIService.IServiceCallBack subscribe;
        MainBll mybll;
        serviceclass Register;
        public void Work(string input)
        {
            lock (subscribe)
            {
                if ((subscribe as ICommunicationObject).State == CommunicationState.Opened)
                {
                    subscribe.set(input + ", Service set thread is " + Thread.CurrentThread.ManagedThreadId);
                }
                else
                {
                    Register.Unsubscribe(this.subscribe);
                    this.Register = null;
                }
            }
        }
        public SingleWork(testIService.IServiceCallBack se,MainBll mybll,serviceclass re)
        {
            this.subscribe = se;
            this.mybll = mybll;
            this.Register = re;
        }
        public void Start()
        {
            this.mybll.newData += this.Work;
        }
        public void Stop()
        {
            this.mybll.newData -= this.Work;
        }

        public void Dispose()
        {
        }
    }
}
