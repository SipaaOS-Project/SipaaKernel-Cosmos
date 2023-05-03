//using SipaaGL2;
//using SipaaGL2.Extensions;
using PrismGraphics;
using PrismGraphics.Extentions;
using PrismGraphics.Extentions.VMWare;
using SipaaKernel.Core;
using SipaaKernel.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Sys = Cosmos.System;

namespace SipaaKernel
{
    public class Kernel : Sys.Kernel
    {
        //VBEGraphics g;
        SVGAIICanvas d;

        protected override void BeforeRun()
        {
            Console.WriteLine("SipaaKernel++ (Copyright The SipaaKernel Project)");
            ProcessManager.Init();
            ProcessManager.StartProcess(new WindowManager());
        }

        protected override void Run()
        {
            ProcessManager.Yield();
        }
    }
}
