using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;



namespace Aegis
{
    internal class ServiceMain : ServiceBase
    {
        public static readonly ServiceMain Instance = new ServiceMain();
        public Action EventStart, EventStop, EventShutDown = null;





        private ServiceMain()
        {
            CanStop = true;
            CanShutdown = true;
            CanPauseAndContinue = false;
            CanHandlePowerEvent = false;
        }


        protected override void OnStart(string[] args)
        {
            ServiceStart();
        }


        protected override void OnStop()
        {
            ServiceStop();
        }


        protected override void OnShutdown()
        {
            EventShutDown?.Invoke();
        }


        public void ServiceStart()
        {
            EventStart?.Invoke();
        }


        public void ServiceStop()
        {
            lock (this)
            {
                EventStop?.Invoke();

                EventStart = null;
                EventStop = null;
                EventShutDown = null;
            }
        }


        public void Run()
        {
            ServiceBase.Run(this);
        }
    }
}
