using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using SipaaKernel.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaKernel.Services
{
    public class FSManager : Process
    {
        public override string Name { get; set; } = "File System Manager";
        public override string Description { get; set; } = "Manage file system";
        public override ProcessType Type { get; set; } = ProcessType.Service;
        public override bool IsCritical { get; set; } = true;
        
        public CosmosVFS VirtualFileSystem { get; private set; }

        public static FSManager Instance { get; private set; }

        private Logger l;

        public override bool Start()
        {
            try
            {
                l = new();
                l.LoggerSource = "SK:FS";
                Instance = this;
                VirtualFileSystem = new();

                VFSManager.RegisterVFS(VirtualFileSystem, false);

                if (!File.Exists("0:\\sksafeshutdown"))
                {
                    l.Log("SipaaKernel didn't shutdown correctly last time. Please use the good shutdown places (like the dedicated command)", Logger.LogType.Warning);
                }
                else
                {
                    File.Delete("0:\\sksafeshutdown");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool Update() { return true; }

        public override bool Stop() { return true; }
    }
}
