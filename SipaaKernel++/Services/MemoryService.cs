using Cosmos.Core;
using Cosmos.Core.Memory;
using SipaaKernel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SipaaKernel.Services
{
    public class MemoryService : Process
    {
        Logger g;

        public override bool Start()
        {
            g = new();
            g.LoggerSource = "SK:MemoryService";
            return true;
        }

        public override bool Stop()
        {
            g.Log("Memory Service has been stopped. You may see memory leaks.", Logger.LogType.Warning);
            return true;
        }

        int t = 0;

        public override bool Update()
        {
            if (t == 5)
            {
                t = 0;
                Heap.Collect();
            }
            t++;
            return true;
        }
    }
}
