using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;

namespace DuTestClient
{
    public partial class Form1 : Form
    {
        private Callback myc = new Callback();
        private delegate void de(string a);
        private DuplexChannelFactory<testIService.IService> factory;
        private testIService.IService myChannel;
        public Form1()
        {
            InitializeComponent();
            this.myc.newdata += this.writeMessage;
        }
        private void writeMessage(string data)
        {
            de d = new de(write);
            this.Invoke(d, data);
        }
        private void write(string data)
        {
            this.memoEdit1.Text += data;
            this.memoEdit1.Text += "\r\n";
            this.memoEdit1.Select(this.memoEdit1.Text.Length, 0);
            this.memoEdit1.ScrollToCaret();
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (factory == null)
            {
                var binding = new WSDualHttpBinding();
                var address = new EndpointAddress("http://localhost:8999/testService.serviceclass");
                var context = new InstanceContext(myc);
                factory = new DuplexChannelFactory<testIService.IService>(context, binding, address);
            }
            if (myChannel != null)
                myChannel.Unsubscribe();
            myChannel = factory.CreateChannel();
            myChannel.Subscribe();
            MessageBox.Show("success");
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (myChannel != null)
                myChannel.Unsubscribe();
        }
        
    }
    public class Callback : testIService.IServiceCallBack
    {
        public event Action<string> newdata;
        public void set(string input)
        {
            newdata(input);
        }
    }
}
