using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

namespace testIService
{
    [ServiceContract(CallbackContract = typeof(IServiceCallBack))]
    public interface IService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool Subscribe();
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool Unsubscribe();
    }
    public interface IServiceCallBack
    {
        [OperationContract(IsOneWay = true)]
        void set(string input);
    }
    
}
