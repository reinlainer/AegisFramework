using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using Aegis.Threading;



namespace Aegis
{
    public static class Framework
    {
        public static readonly Assembly ExecutingAssembly;
        public static readonly Version AegisVersion;
        public static readonly Version ExecutingVersion;


        public static event Func<string[], bool> Initialized;
        public static event Action<CloseReason> Finalizing;
        public static event Action Finalized, Running;

        private static EventWaitHandle _stopRunningEvent;
        private static EventWaitHandle _releaseEvent;





        static Framework()
        {
            ExecutingAssembly = Assembly.GetEntryAssembly();

            AegisVersion = new Version(
                Assembly.GetExecutingAssembly().GetName().Version.Major,
                Assembly.GetExecutingAssembly().GetName().Version.Minor,
                Assembly.GetExecutingAssembly().GetName().Version.Build,
                Assembly.GetExecutingAssembly().GetName().Version.Revision);

            ExecutingVersion = new Version(
                ExecutingAssembly.GetName().Version.Major,
                ExecutingAssembly.GetName().Version.Minor,
                ExecutingAssembly.GetName().Version.Build,
                ExecutingAssembly.GetName().Version.Revision);
        }


        public static void SetWorkingDirectory(string path = null)
        {
            if (path != null)
                System.IO.Directory.SetCurrentDirectory(path);
            else
            {
                path = System.IO.Path.GetDirectoryName(ExecutingAssembly.Location);
                System.IO.Directory.SetCurrentDirectory(path);
            }
        }


        public static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();


            //  프레임워크 초기화
            {
                SpinWorker.Initialize();


                _stopRunningEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
                _releaseEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
                Logger.Info(LogMask.Aegis, "Aegis Framework({0})", AegisVersion.ToString(3));
            }



            AegisTask.Run(() =>
            {
                //  컨텐츠 초기화 (UI 모드)
                if (Environment.UserInteractive)
                {
                    AegisTask.SafeAction(() =>
                    {
                        if (Initialized == null ||
                            Initialized.Invoke(args) == true)
                            Running?.Invoke();
                    });

                    AegisTask.SafeAction(() => { Finalizing?.Invoke(CloseReason.Close); });
                }
                //  컨텐츠 초기화 (서비스 모드)
                else
                {
                    ServiceMain.Instance.EventStart = () =>
                    {
                        AegisTask.SafeAction(() =>
                        {
                            if (Initialized == null ||
                                Initialized?.Invoke(System.Environment.GetCommandLineArgs()) == true)
                            {
                                //  Running이 설정된 경우에는 Running이 반환되기를 대기하고, 반환된 후 종료처리 진행
                                if (Running != null)
                                {
                                    (new Thread(() =>
                                    {
                                        AegisTask.SafeAction(() => { Running.Invoke(); });
                                        ServiceMain.Instance.Stop();
                                    })).Start();
                                }
                            }
                        });
                    };
                    ServiceMain.Instance.EventStop = () =>
                    {
                        AegisTask.SafeAction(() => { Finalizing?.Invoke(CloseReason.ServiceStop); });
                    };
                    ServiceMain.Instance.EventShutDown = () =>
                    {
                        AegisTask.SafeAction(() => { Finalizing?.Invoke(CloseReason.ShutDown); });
                    };
                    ServiceMain.Instance.Run();
                }


                _stopRunningEvent.Set();
            });


            AegisTask.Run(() =>
            {
                WaitForRunning();


                //  프레임워크 종료
                Calculate.IntervalTimer.DisposeAll();
                NamedThread.DisposeAll();
                NamedObjectManager.Clear();
                SpinWorker.Release();


                Logger.Info(LogMask.Aegis, "Aegis Framework finalized.");
                Logger.RemoveAll();


                AegisTask.SafeAction(() => { Finalized?.Invoke(); });
                Initialized = null;
                Finalizing = null;
                Finalized = null;
                Running = null;

                _releaseEvent.Set();
            });
        }


        public static bool WaitForRunning(int millisecondTimeout = Timeout.Infinite)
        {
            if (_stopRunningEvent == null)
                return false;

            return _stopRunningEvent.WaitOne(millisecondTimeout);
        }


        public static void Release()
        {
            if (_stopRunningEvent == null)
                return;

            _stopRunningEvent.Set();
            _releaseEvent.WaitOne();
        }


        public static void RequestAdditionalTime(int milliseconds)
        {
            ServiceMain.Instance.RequestAdditionalTime(milliseconds);
        }
    }
}
