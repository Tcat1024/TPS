using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ConsoleApplication1
{
    [ServiceContract]
    public interface IService
    {
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int get();
    }
}
