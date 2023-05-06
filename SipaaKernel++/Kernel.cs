#define SKDEV
//using SipaaGL2;
//using SipaaGL2.Extensions;
using SipaaKernel.Core.Encryption;
using Cosmos.Core;
using Cosmos.Core.Multiboot;
using IL2CPU.API.Attribs;
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
using System.IO;

namespace SipaaKernel
{
    public class Kernel : Sys.Kernel
    {
        [ManifestResourceStream(ResourceName = "SipaaKernel.Resources.wp.bmp")]
        public static byte[] wallp;

        public static Graphics wallpbmp;

        public virtual void Start()
        {
            try
            {
                mStarted = true;

                if (string.Empty == null)
                {
                    throw new Exception("Compiler didn't initialize System.String.Empty!");
                }

                Bootstrap.Init();
                OnBoot();

                wallpbmp = Image.FromBitmap(wallp);

                ProcessManager.Init();
                ProcessManager.StartProcess(new MemoryService());
                ProcessManager.StartProcess(new FSManager());
                //ProcessManager.StartProcess(new WindowManager());
                EnvSelection();

                Cosmos.HAL.Global.EnableInterrupts();

                while (!mStopped)
                {
                    ProcessManager.Yield();
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.SKBugCheck(e);
            }
        }
        void EnvSelection()
        {
            Console.Clear();
            Console.WriteLine("Please select your prefered environment");
            Console.WriteLine("1) GUI");
            Console.WriteLine("2) Console");
            #if SKDEV
            Console.WriteLine("3) Test Environment");
            #endif
            Console.WriteLine();
            Console.Write("Choice : ");

            switch (int.Parse(Console.ReadLine()))
            {
                case 1:
                    ProcessManager.StartProcess(new WindowManager());
                    break;
                case 2:
                    ProcessManager.StartProcess(new ShellService());
                    break;
                    #if SKDEV
                case 3:
                    Console.Write("User name : ");
                    var usrname = Console.ReadLine();
                    Console.Write("Password: ");
                    var pass = Console.ReadLine();

                    CreateUser(usrname, pass);

                    var u = (User)ReadUserFile(@"0:\Users\" + usrname + ".ini");

                    Console.WriteLine("usrname=" + u.Name);
                    Console.WriteLine("pass=" + u.Password);

                    break;
                    #endif
            }
        }
        protected override void BeforeRun() { }
        protected override void Run() { }

        void CreateUser(string usrname, string pass)
        {
            String file = $"[SKUser]\nName={usrname}\nPassword={pass}";

            if (!Directory.Exists(@"0:\Users"))
                Directory.CreateDirectory(@"0:\Users");

            File.WriteAllText($@"0:\Users\{usrname}.ini", file);
        }

        User? ReadUserFile(string usrfile)
        {
            User u = new();
            string[] lines = File.ReadAllLines(usrfile);

            // Parse INI file
            if (lines[0] != "[SKUser]")
                return null;

            foreach (string line in lines)
            {
                string[] split = line.Split('=');

                string name = split[0];
                string value = split[1];

                if (name.StartsWith("Name"))
                    u.Name = value;
                
                if (name.StartsWith("Password"))
                    u.Password = value;
            }

            return u;
        }

        struct User
        {
            public string Password;
            public string Name;
        }
    }
}
