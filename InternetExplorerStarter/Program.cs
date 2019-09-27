using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace InternetExplorerStarter
{

    public class Program
    {
        private static OptionSet options;
        public static bool Exit;

        static void Main(string[] args)
        {
            List<string> urls = new List<string>();
            string task_name = "Internet Explorer Starter";
            bool identify = false, show_help = false, maximize = false, show_version = false, relative_mode = false, keep_running = false;
            bool kiosk = false, fullscreen = false, hide_addressbar = false, disable_addressbar = false;
            int screenId = 1, offset_x = 0, offset_y = 0, window_h = 0, window_w = 0, refresh = 0;

            Thread TrayThread;

            options = new OptionSet()
            {
                { "u|url=", "The {url} to open.", v => urls.Add(v) },
                { "i|identify", "Identify screens by drawing screen number on each screen", v => identify = v != null },
                { "s|screen=", "Place IE window on screen {x}", (int v) => screenId = v },
                { "x=", "Place IE window on screen {x} position", (int v) => offset_x = v },
                { "y=", "Place IE window on screen {y} position", (int v) => offset_y = v },
                { "width=", "Window width", (int v) => window_w = v },
                { "height=", "Window height", (int v) => window_h = v },
                { "r|relative", "Position window relative to given screen number", v => relative_mode = v != null },
                { "m|maximize",  "Maximize window", v => maximize = v != null },
                { "k|kiosk", "Open in kiosk mode", v => kiosk = v != null },
                { "f|fullscreen", "Open in fullscreen mode", v => fullscreen = v != null },
                { "a|addressbar", "Hide address bar (this also hides tabs)", v => hide_addressbar = v != null },
                { "d|disable_addressbar", "Disable addressbar", v => disable_addressbar = v != null },
                { "n|name", "Name for task", v => task_name = v },
                { "e|keeprunning", "Ensures IE is always running", v => keep_running =v != null },
                { "refresh=", "refresh every x seconds (this will activate keep running)", (int v) => refresh = v },
                { "version", "Show application version", v => show_version = v != null },
                { "h|help",  "show this message and exit", v => show_help = v != null },
            };

            // Parse program arguments
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Use --help for more information.");
                return;
            }

            Console.Title = task_name;

            if (show_version)
            {
                Console.WriteLine("InternetExplorerStarter version: {0}", Application.ProductVersion);
                return;
            }

            if (show_help) /* Print help and exit! */
            {
                ShowHelp();
                return;
            }

            if (identify) /* Identify screen by drawing screen number and exit! */
            {
                var identifyThread = new Thread(() =>
                {
                    Identify();
                });
                identifyThread.SetApartmentState(ApartmentState.STA);
                identifyThread.Start();
                identifyThread.Join();
                return;
            }

            InternetExplorer IE = new InternetExplorer();

            keep_running = keep_running || refresh > 0;

            if (keep_running)
            {
                // place icon in systemtray
                TrayThread = new Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new SystemTray());
                });
                TrayThread.SetApartmentState(ApartmentState.STA);
                TrayThread.Start();

                // Hide console window
                WinAPI.ShowWindow(WinAPI.GetConsoleWindow(), WinAPI.ShowWindowCommands.Hide);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!Exit)
            {
                if (IE.IsRunning)
                {
                    if (refresh > 0 && sw.ElapsedMilliseconds > (refresh * 1000))
                    {
                        try
                        {
                            IE.Refresh();
                        }
                        catch
                        {
                            Console.WriteLine("Refresh failed.");
                        }
                        sw.Restart();
                    }

                    Thread.Sleep(10);
                    continue;
                }
                else
                {
                    Console.WriteLine("Reset IE to startup state..");
                    IE.Reset();
                }

                Console.WriteLine("Internet Explorer Version: {0}", IE.Version);
                Console.WriteLine("ProcessId: {0}", IE.GetPid);


                /* Open URL's in IE */
                bool first = true;
                foreach (string url in urls)
                {
                    if (first) // open first url in main tab
                    {
                        IE.OpenUrl(url);
                        first = false;
                    }
                    else // open all others urls as new tab
                        IE.OpenNewTab(url);
                }

                int screen_x = 0;
                int screen_y = 0;

                if (screenId > 0)
                {
                    Screen[] screens = Screen.AllScreens;
                    Screen screen;
                    if (screenId > screens.Length)
                    {
                        Console.WriteLine("Unable to select screen number {0}", screenId);
                        Console.WriteLine("Use --identify to draw corresponding number on each screen");
                        screen = screens.FirstOrDefault();
                    }
                    else
                        screen = screens[screenId - 1];

                    screen_x = screen.WorkingArea.X;
                    screen_y = screen.WorkingArea.Y;

                    // Set Screen position
                    if (relative_mode)
                    {
                        screen_x = (Math.Abs(screen_x) + offset_x) * (screen_x < 0 ? -1 : 1);
                        screen_y = (Math.Abs(screen_y) + offset_y) * (screen_y < 0 ? -1 : 1);
                    }
                    else
                    {
                        screen_x = screen_x != 0 ? screen_x : offset_x;
                        screen_y = screen_y != 0 ? screen_y : offset_y;
                    }

                    // set screen size
                    window_w = window_w != 0 ? window_w : screen.WorkingArea.Width;
                    window_h = window_h != 0 ? window_h : screen.WorkingArea.Height;
                }

                IE.Show();
                IE.SetForeground();

                Console.WriteLine("Moving window to: (X: {0}, Y: {1}) (W: {2}, H: {3})", screen_x, screen_y, window_w, window_h);
                WinAPI.MoveWindow(IE.GetHWND, screen_x, screen_y, window_w, window_h, true);

                if (maximize) IE.Maximize();
                IE.SetFullscreen(fullscreen);
                IE.SetKioskMode(kiosk);
                IE.HideAddressbar(hide_addressbar);
                if (disable_addressbar) IE.DisableAddressbar();

                // Exit and keep IE running
                if (!keep_running)
                    return;
            }

            // Close
            if (IE != null)
                IE.Exit();
        }



        public static void Identify()
        {
            int nr = 1;
            var drawObject = new DrawWindow();
            Console.WriteLine("Screen info:");
            foreach (Screen screen in Screen.AllScreens)
            {
                int w = screen.Bounds.Width;
                int h = screen.Bounds.Height;
                int x = screen.Bounds.X;
                int y = screen.Bounds.Y;
                drawObject.Create(nr, x, y, w, h);
                // Show info
                Console.WriteLine();
                Console.WriteLine("* Screen {0}", nr);
                Console.WriteLine("\tName:\t\t {0}", screen.DeviceName);
                Console.WriteLine("\tX:\t\t {0}", screen.Bounds.X);
                Console.WriteLine("\tY:\t\t {0}", screen.Bounds.Y);
                Console.WriteLine("\tW:\t\t {0}", screen.Bounds.Width);
                Console.WriteLine("\tH:\t\t {0}", screen.Bounds.Height);
                nr++;
            }

            drawObject.Show();
            Thread.Sleep(5000);
            drawObject.Close();
        }

        public static void ShowHelp()
        {
            string exe = AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine("Internet Explorer Starter");
            Console.WriteLine("Version: {0}", Application.ProductVersion);
            Console.WriteLine("Get latest release from: https://github.com/Sidhy/InternetExplorerStarter/releases");
            Console.WriteLine("Usage: {0} [OPTIONS]\n", exe);
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("\n\nExamples:");
            Console.WriteLine("{0} --screen=2 -u https://www.google.com --url=https://www.github.com -u https://www.bing.com", exe );
            Console.WriteLine("{0} --identify", exe);
        }
    }
}
