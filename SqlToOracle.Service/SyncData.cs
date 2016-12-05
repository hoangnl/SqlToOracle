using System;
using System.ServiceProcess;
using System.Timers;
using SqlToOracle.Core;
using SqlToOracle.Core.Common;

namespace SqlToOracle.Service
{
    public partial class SyncData : ServiceBase
    {
        private Timer _timer;

        private static int _scheduleTime = 3600000;

        public SyncData()
        {
            InitializeComponent();
        }

        private void SetScheduleTime()
        {
            _scheduleTime = Utility.GetScheduleTime();
        }

        private void RunSchedule()
        {
            try
            {
                SetScheduleTime();
                //_intervalTimer = new Timer(Run, null, _scheduleTime, _scheduleTime);
                _timer = new Timer(_scheduleTime); // Sets a 10 second interval
                _timer.Elapsed += Run; // Specifies The Event Handler
                _timer.Enabled = true; // Enables the control
                _timer.AutoReset = false; // makes it repeat
                _timer.Start();
                Logger.Info("Cấu hình timer thành công ");
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi khởi tạo timer: " + ex);
                //throw;
            }
        }

        protected void Run(object sender)
        {
            try
            {
                var obj = new Synchronize();
                obj.Run();
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR Sync data from Sql to Oracle ||Exception: " + ex);
            }
            finally
            {
                _timer.Start();
            }
        }

        protected void Run(object sender, ElapsedEventArgs e)
        {
            try
            {
                var obj = new Synchronize();
                obj.Run();
                Logger.Info("Config timer successfully " + _scheduleTime.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR Sync data from Sql to Oracle ||Exception: " + ex);
            }
            finally
            {
                _timer.Start();
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var obj = new Synchronize();
                obj.Run();
                RunSchedule();
                Logger.Info("Service sync data from Sql to Oracle successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR Sync data from Sql to Oracle ||Exception: " + ex);
                //throw;
            }

        }

        protected override void OnStop()
        {
            //_intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //_intervalTimer.Dispose();
            //_intervalTimer = null;

            _timer.Dispose();
            _timer = null;
            var message = "Sync data stop successfully at " + DateTime.Now.ToLongTimeString();
            Logger.Info(message);
        }

        protected override void OnContinue()
        {
            var message = "Sync data continue successfully at " + DateTime.Now.ToLongTimeString();
            Logger.Info(message);
        }

        protected override void OnPause()
        {
            var message = "Sync data pause successfully at " + DateTime.Now.ToLongTimeString();
            Logger.Info(message);
        }
    }
}
