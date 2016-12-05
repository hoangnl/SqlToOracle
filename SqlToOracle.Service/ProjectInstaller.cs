using System.ComponentModel;
using Microsoft.Win32;

namespace SqlToOracle.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {

        private readonly System.ServiceProcess.ServiceProcessInstaller _syncProcessInstaller;
        private readonly System.ServiceProcess.ServiceInstaller _syncServiceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();
            this._syncProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this._syncServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // EcustomsProcessInstaller
            // 
            this._syncProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this._syncProcessInstaller.Password = null;
            this._syncProcessInstaller.Username = null;
            // 
            // EcustomsServiceInstaller
            // 
            this._syncServiceInstaller.DisplayName = "SqlToOracle";
            this._syncServiceInstaller.ServiceName = "Sync data NSW from Sql to Oracle";
            this._syncServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            //this.EcustomsServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this._syncProcessInstaller,
            this._syncServiceInstaller});

            RegistryKey ckey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + this._syncServiceInstaller.ServiceName, true);
            // Good to always do error checking!
            if (ckey != null)
            {
                // Ok now lets make sure the "Type" value is there, 
                //and then do our bitwise operation on it.
                if (ckey.GetValue("Type") != null)
                {
                    ckey.SetValue("Type", ((int)ckey.GetValue("Type") | 256));
                }
            }
        }
    }
}
