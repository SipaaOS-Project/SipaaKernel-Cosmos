using Cosmos.HAL.Drivers.PCI.Video;
using Cosmos.System;
using PrismGraphics;
using PrismGraphics.Extentions;
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

            d.Clear();

            d.DrawFilledRectangle(10, 10, 100, 100, 7, new(0xFFFFFF));
            d.DrawFilledRectangle(0, 0, 200, 200, 0, new(140, 255, 0, 255));
            if (!isVBE)
                d.SetCursor(MouseManager.X, MouseManager.Y, true);
            else
                d.DrawImage((int)MouseManager.X, (int)MouseManager.Y, cursorGraphics);
            d.Update();

            return true;
        }
    }
}
