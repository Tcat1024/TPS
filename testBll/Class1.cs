using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testBll
{
    public class MainBll
    {
        public event Action<string> newData;
        private Thread newT;
        private void Start_Thread()
        {
            int count = 0;
            var pre = DateTime.Now;
            var now = DateTime.Now;
            while (true)
            {
                now = DateTime.Now;
                if (newData != null)
                {
                    foreach(Action<string> item in newData.GetInvocationList())
                    {
                        item.BeginInvoke("Count is " + count.ToString() + ", Thread in Server is " + Thread.CurrentThread.ManagedThreadId+", Cost time is "+(now-pre).TotalMilliseconds.ToString()+"ms",null,null);
                    }
                }
                count++;
                Thread.Sleep(1000);
                pre = now;
            }
        }
        public void Start()
        {
            newT = new Thread(this.Start_Thread);
            newT.IsBackground = true;
            newT.Start();
        }
        public void Stop()
        {
            if (newT != null && newT.ThreadState != ThreadState.Aborted)
                newT.Abort();
        }
    }
}
