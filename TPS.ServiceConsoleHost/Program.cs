using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using TPS.Service;

namespace TPS.ServiceConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceManager myManager = new ServiceManager();
                myManager.ServiceStart += new EventHandler(showstart);
                ServiceHostBuilder.ServiceLoadSeccuss += new EventHandler(showadd);
                myManager.GetServices();
                myManager.GetHosts();
                myManager.StartAll();
                Console.WriteLine(getMessagePre()+"All the Service has started.");
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
            Console.ReadLine();
        }
        private static void showadd(object sender,EventArgs e)
        {
            Console.WriteLine(getMessagePre() + sender.ToString() + " Service has been added");
        }
        private static void showstart(object sender,EventArgs e)
        {
            Console.WriteLine(getMessagePre() + sender.ToString() + " Service has been started");
        }
        private static string getMessagePre()
        {
            return "TIME: "+DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " ";
        }
    }
}
