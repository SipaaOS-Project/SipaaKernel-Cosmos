using System;
using SipaaKernel.Core;

namespace SipaaKernel.Services;

public class ShellService : Process
{
        public override string Name { get; set; } = "Shell Service";
        public override string Description { get; set; } = "Manage shell";
        public override ProcessType Type { get; set; } = ProcessType.Service;
        public override bool IsCritical { get; set; } = true;

        public override bool Start()
        {
            Console.Clear();
            Console.WriteLine("<S> SipaaKernel++");
            Console.WriteLine("Copyright (c) The SipaaKernel Project");
            Console.WriteLine();
            return true;
        }

        public override bool Stop()
        {
            return true;
        }

        public override bool Update()
        {
            Console.Write("> ");
            var i = Console.ReadLine();
            if (i.StartsWith("gui"))
            {
                ProcessManager.StopProcess(this);
                ProcessManager.StartProcess(new WindowManager());
            }
            return true;
        }
}