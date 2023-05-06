using Cosmos.Core;
using Cosmos.System;
using PrismGraphics;
using PrismGraphics.Animation;
using PrismGraphics.Extentions;
using PrismGraphics.Fonts;
using SipaaKernel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaKernel.Services
{
    internal class WindowManager : Process
    {
        public override string Name { get; set; } = "Window Manager";
        public override string Description { get; set; } = "Manage windows & display";
        public override ProcessType Type { get; set; } = ProcessType.Service;
        public override bool IsCritical { get; set; } = true;

        public ushort ScreenWidth { get => Display.Width; set => Display.Width = value; }
        public ushort ScreenHeight { get => Display.Height; set => Display.Height = value; }
        public Display Display { get; set; }

        public static WindowManager Instance { get; private set; }

        private Logger logger;
        private bool isVBE = false;
        private Graphics cursorGraphics;
        AnimationController A = new(25f, 270f, new(0, 0, 0, 0, 750), AnimationMode.Ease);
        AnimationController P = new(0f, 360f, new(0, 0, 0, 0, 500), AnimationMode.Linear);

        int X { get => ScreenWidth / 2; }
        int Y { get => ScreenHeight / 2; }

        public override bool Start()
        {
            try
            {
                logger = new();
                logger.LoggerSource = "SK:WM";

                Instance = this;
                
                logger.Log("Initializing mouse manager...", Logger.LogType.Info);
                MouseManager.ScreenWidth = 1280;
                MouseManager.ScreenHeight = 720;
                logger.Log("Mouse manager is now initialized", Logger.LogType.Success);

                logger.Log("Initializing PrismGraphics...", Logger.LogType.Info);
                Display = Display.GetDisplay(1280, 720);

                cursorGraphics = new(1, 1);
                cursorGraphics.Clear(Color.White);

                isVBE = !Display.DefineCursor(cursorGraphics);

                if (isVBE)
                    logger.Log("Got a VBECanvas from PrismGraphics", Logger.LogType.Info);
                else
                    logger.Log("Got a SVGAIICanvas from PrismGraphics", Logger.LogType.Info);

                logger.Log("PrismGraphics is now initialized", Logger.LogType.Success);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool Stop()
        {
            try
            {
                Instance = null;

                MouseManager.ScreenWidth = 0;
                MouseManager.ScreenHeight = 0;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool Update()
        {
            var d = Display;

            d.DrawImage(0, 0, Kernel.wallpbmp, false);
            d.DrawString(10, 10, $"{GCImplementation.GetUsedRAM() / 1024 / 1024}/{CPU.GetAmountOfRAM()}", Font.Fallback, Color.White);
           
            if (!isVBE)
                d.SetCursor(MouseManager.X, MouseManager.Y, true);
            else
                d.DrawImage((int)MouseManager.X, (int)MouseManager.Y, cursorGraphics);
            //d.Blur(20, 400, 300, 60, 60);
            d.Update();

            return true;
        }
    }
}
