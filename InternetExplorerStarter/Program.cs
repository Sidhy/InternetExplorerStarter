using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Security.Principal;
using System.ComponentModel;

namespace InternetExplorerStarter
{

    public class Program
    {
        private static OptionSet options;
        public static bool Exit;
        public static string Name;

        static void Main(string[] args)
        {
            List<string> urls = new List<string>();
            string task_name = "Internet Explorer Starter";
            bool identify = false, show_help = false, maximize = false, show_version = false, keep_running = false, topmost = false;
            bool kiosk = false, hide_addressbar = false, disable_addressbar = false, install_association = false, fullscreen = false;
            string file = string.Empty;
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
                { "m|maximize",  "Maximize window", v => maximize = v != null },
                { "k|kiosk", "Open in kiosk mode", v => kiosk = v != null },
                { "f|fullscreen", "Set window fullscreen", v => fullscreen = v != null },
                { "a|addressbar", "Hide address bar (this also hides tabs)", v => hide_addressbar = v != null },
                { "d|disable_addressbar", "Disable addressbar", v => disable_addressbar = v != null },
                { "n|name", "Name for task", v => task_name = v },
                { "t|topmost", "Set window always on top", v => topmost = v != null },
                { "e|keeprunning", "Ensures IE is always running", v => keep_running =v != null },
                { "r|refresh=", "refresh every x seconds (this will activate keep running)", (int v) => refresh = v },
                { "file=", "Open ies file", v => file = v },
                { "install", "Install/Reinstall file association for .ies files", v => install_association = v !=null },
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

            if (install_association)
            {
                Console.WriteLine("Installing file association");

                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        Verb = "runas",
                        FileName = Application.ExecutablePath,
                        Arguments = "--install"
                    };
                    try
                    {
                        Console.WriteLine("Trying to elevate to install file association");
                        Process.Start(processInfo);
                    }
                    catch (Win32Exception)
                    {
                        Console.WriteLine("Failed to elevate");
                    }
                    return;
                }
                else
                {
                    InstallFileAssociation(".ies", "InternetExplorerStarter", Application.ExecutablePath, "Launch IE Starter");
                }
                return;
            }

            if (identify) /* Identify screen by drawing screen number and exit! */
            {
                Identify();
                return;
            }

            #region Read Associated/Process File
            // if set load file from path
            if (!string.IsNullOrEmpty(file))
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine("ERROR: File ({0}) does not exist!", file);
                    return;
                }

                try
                {
                    foreach (var line in File.ReadAllLines(file))
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;

                        var index = line.IndexOf("=");
                        var cmd = line.Substring(0, index).Trim();
                        var value = line.Substring(index + 1, line.Length - index - 1).Trim();

                        if (string.IsNullOrWhiteSpace(cmd) || string.IsNullOrWhiteSpace(value))
                        {
                            Console.WriteLine("Unable to process {0}", line);
                            continue;
                        }

                        switch (cmd)
                        {
                            case "identify":
                                Identify();
                                return;
                            case "url":
                                urls.Add(value);
                                break;
                            case "name":
                                task_name = value;
                                break;
                            case "screen":
                                screenId = ParseStringToNumber(value);
                                break;
                            case "x":
                                offset_x = ParseStringToNumber(value);
                                break;
                            case "y":
                                offset_y = ParseStringToNumber(value);
                                break;
                            case "width":
                                window_w = ParseStringToNumber(value);
                                break;
                            case "height":
                                window_h = ParseStringToNumber(value);
                                break;
                            case "maximize":
                                maximize = ParseBooleanString(value);
                                break;
                            case "fullscreen":
                                fullscreen = ParseBooleanString(value);
                                break;
                            case "topmost":
                                topmost = ParseBooleanString(value);
                                break;
                            case "kiosk":
                                kiosk = ParseBooleanString(value);
                                break;
                            case "hide_addressbar":
                                hide_addressbar = ParseBooleanString(value);
                                break;
                            case "disable_addressbar":
                                disable_addressbar = ParseBooleanString(value);
                                break;
                            case "keeprunning":
                                keep_running = ParseBooleanString(value);
                                break;
                            case "refresh":
                                refresh = ParseStringToNumber(value);
                                break;
                            default:
                                Console.WriteLine("Unable to parse {0}", line);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: Reading file ({0})", file);
                    Console.WriteLine(ex);
                }
            }
            #endregion

            Console.Title = task_name;
            Name = task_name;

            InternetExplorer IE = new InternetExplorer();

            keep_running = keep_running || refresh > 0;
            if (keep_running)
            {
                // place icon in systemtray
                TrayThread = new Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var tray = new SystemTray();
                    tray.trayIcon.Text = task_name;
                    Application.Run(tray);
                });
                TrayThread.SetApartmentState(ApartmentState.STA);
                TrayThread.Start();

                // Hide console window
                WinAPI.ShowWindow(WinAPI.GetConsoleWindow(), WinAPI.ShowWindowCommands.Hide);

                // Disable Close button on Console window
                WinAPI.DeleteMenu(WinAPI.GetSystemMenu(WinAPI.GetConsoleWindow(), false), 0xF060, 0x0);
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

                    // Set Screen Position
                    screen_x += offset_x;
                    screen_y += offset_y;

                    // set screen size
                    window_w = window_w != 0 ? window_w : screen.WorkingArea.Width;
                    window_h = window_h != 0 ? window_h : screen.WorkingArea.Height;
                }


                IE.Show();
                IE.SetForeground();

                Console.WriteLine("Moving window to: (X: {0}, Y: {1}) (W: {2}, H: {3})", screen_x, screen_y, window_w, window_h);
                WinAPI.MoveWindow(IE.GetHWND, screen_x, screen_y, window_w, window_h, true);

                if (maximize) IE.Maximize();
                if (fullscreen) IE.SetFullscreen();
                if (kiosk) IE.SetKioskMode();
                IE.HideAddressbar(hide_addressbar);
                if (disable_addressbar) IE.DisableAddressbar();

                if (topmost) IE.SetTopMost();

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
            var identifyThread = new Thread(() =>
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
                    nr++; // increment screen number
                }

                drawObject.Show();
                Thread.Sleep(3000);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                drawObject.Close();
            });
            identifyThread.SetApartmentState(ApartmentState.STA);
            identifyThread.Start();
            identifyThread.Join();

            
        }

        private static bool ParseBooleanString(string boolean)
        {
            boolean = boolean.ToLower();
            return boolean.Equals("true");
        }

        private static int ParseStringToNumber(string number)
        {
            try
            {
                return Convert.ToInt32(number);
            }
            catch
            { }

            return 0;
        }

        public static void InstallFileAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        { 

            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RegistryKey CurrentUser;

            BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
            BaseKey.SetValue("", KeyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
            OpenMethod.SetValue("", FileDescription);
            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"--file=%1\"");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"--file=%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            // Delete the key instead of trying to change it
            CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
            CurrentUser.DeleteSubKey("UserChoice", false);
            CurrentUser.Close();

            // Tell explorer the file association has been changed
            WinAPI.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
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
