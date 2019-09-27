using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using InternetExplorerStarter;
using Microsoft.Win32;

namespace InternetExplorerStarter
{
    public class InternetExplorer
    {
        private SHDocVw.InternetExplorer internetExplorer;

        public enum BrowserNavConstants
        {
            /// <summary>
            /// Open the resource or file in a new window.
            /// </summary>
            navOpenInNewWindow = 0x1,

            /// <summary>
            /// Do not add the resource or file to the history list. The new page replaces the current page in the list.
            /// </summary>
            navNoHistory = 0x2,

            /// <summary>
            /// Do not consult the Internet cache; retrieve the resource from the origin server (implies BINDF_PRAGMA_NO_CACHE and BINDF_RESYNCHRONIZE).
            /// </summary>
            navNoReadFromCache = 0x4,

            /// <summary>
            /// Do not add the downloaded resource to the Internet cache. See BINDF_NOWRITECACHE.
            /// </summary>
            navNoWriteToCache = 0x8,

            /// <summary>
            /// If the navigation fails, the autosearch functionality attempts to navigate common root domains (.com, .edu, and so on). If this also fails, the URL is passed to a search engine.
            /// </summary>
            navAllowAutosearch = 0x10,

            /// <summary>
            /// Causes the current Explorer Bar to navigate to the given item, if possible.
            /// </summary>
            navBrowserBar = 0x20,

            /// <summary>
            /// Microsoft Internet Explorer 6 for Microsoft Windows XP Service Pack 2 (SP2) and later. If the navigation fails when a hyperlink is being followed, this constant specifies that the resource should then be bound to the moniker using the BINDF_HYPERLINK flag.
            /// </summary>
            navHyperlink = 0x40,

            /// <summary>
            /// Internet Explorer 6 for Windows XP SP2 and later. Force the URL into the restricted zone.
            /// </summary>
            navEnforceRestricted = 0x80,

            /// <summary>
            /// Internet Explorer 6 for Windows XP SP2 and later. Use the default Popup Manager to block pop-up windows.
            /// </summary>
            navNewWindowsManaged = 0x0100,

            /// <summary>
            /// Internet Explorer 6 for Windows XP SP2 and later. Block files that normally trigger a file download dialog box.
            /// </summary>
            navUntrustedForDownload = 0x0200,

            /// <summary>
            /// Internet Explorer 6 for Windows XP SP2 and later. Prompt for the installation of Microsoft ActiveX controls.
            /// </summary>
            navTrustedForActiveX = 0x0400,

            /// <summary>
            /// Windows Internet Explorer 7. Open the resource or file in a new tab. Allow the destination window to come to the foreground, if necessary.
            /// </summary>
            navOpenInNewTab = 0x0800,

            /// <summary>
            /// Internet Explorer 7. Open the resource or file in a new background tab; the currently active window and/or tab remains open on top.
            /// </summary>
            navOpenInBackgroundTab = 0x1000,

            /// <summary>
            /// Internet Explorer 7. Maintain state for dynamic navigation based on the filter string entered in the search band text box (wordwheel). Restore the wordwheel text when the navigation completes.
            /// </summary>
            navKeepWordWheelText = 0x2000
        }
        private static object missing = Type.Missing;
        public IntPtr GetHWND { get { return new IntPtr(internetExplorer.HWND); } }
        public uint GetPid { get
            {
                WinAPI.GetWindowThreadProcessId(GetHWND, out uint pid);
                return pid;
            }
        }

        public Version Version;
        public Thread RefreshThread;
        public Process Process { get; private set; }

        private bool isLoading;

        public bool IsRunning {
            get {
                try
                {
                    if (!isLoading)
                    {
                        IntPtr xtest = new IntPtr(internetExplorer.HWND);
                    }

                    return (Process != null && !Process.HasExited && Process.MainWindowHandle != IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return false;
            }
        }

        public InternetExplorer()
        {
        }

        private void init()
        {
            this.internetExplorer = new SHDocVw.InternetExplorer();
            this.Version = Version.Parse(Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Internet Explorer").GetValue("svcVersion").ToString());
            internetExplorer.DocumentComplete += InternetExplorer_DocumentComplete;
            Process = Process.GetProcessById((int)GetPid);
        }

        private void InternetExplorer_DocumentComplete(object pDisp, ref object URL)
        {
            isLoading = false;
        }

        /// <summary>
        /// Open url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="frame"></param>
        public void OpenUrl(string url, string frame = "")
        {
            isLoading = true;
            internetExplorer.Navigate2(url, ref missing, frame, ref missing, ref missing);
        }

        /// <summary>
        /// Open Url as new tab
        /// </summary>
        /// <param name="url"></param>
        /// <param name="frame"></param>
        public void OpenNewTab(string url, string frame = "")
        {
            internetExplorer.Navigate2(url, BrowserNavConstants.navOpenInNewTab, frame, ref missing, ref missing);
        }

        /// <summary>
        /// Show Internet Explorer
        /// </summary>
        public void Show()
        {
            internetExplorer.Visible = true;
        }

        /// <summary>
        /// Hide Internet Explorer
        /// </summary>
        public void Hide()
        {
            internetExplorer.Visible = false;
        }

        /// <summary>
        /// Maximize Internet Explorer window
        /// </summary>
        public void Maximize()
        {
            WinAPI.ShowWindow(GetHWND, WinAPI.ShowWindowCommands.ShowMaximized);
        }

        /// <summary>
        /// Enable Kiosk mode
        /// </summary>
        public void SetKioskMode(bool enabled)
        {
            internetExplorer.TheaterMode = enabled;
        }

        /// <summary>
        /// Enable fullscreen mode
        /// </summary>
        public void SetFullscreen(bool enabled)
        {
            internetExplorer.FullScreen = enabled;
        }

        /// <summary>
        /// Hide Address bar
        /// </summary>
        public void HideAddressbar(bool enabled)
        {
            internetExplorer.AddressBar = !enabled;
        }

        /// <summary>
        /// Disable Address bar using winapi
        /// 
        /// Only tested with IE 11.239.18362.0
        /// </summary>
        public void DisableAddressbar()
        {
            if (Version.Major != 11)
                Console.WriteLine("WARNING: Only confirmed to be working with IE version 11");
            try
            {
                IntPtr MainWindowHandle = new IntPtr(internetExplorer.HWND);
                IntPtr Child;
                bool success = false;
                Child = WinAPI.FindWindowEx(MainWindowHandle, IntPtr.Zero, "WorkerW", IntPtr.Zero);
                if (Child != IntPtr.Zero)
                {
                    Child = WinAPI.FindWindowEx(Child, IntPtr.Zero, "ReBarWindow32", IntPtr.Zero);
                    if (Child != IntPtr.Zero)
                    {
                        Child = WinAPI.FindWindowEx(Child, IntPtr.Zero, "Address Band Root", IntPtr.Zero);
                        if (Child != IntPtr.Zero)
                        {
                            Child = WinAPI.FindWindowEx(Child, IntPtr.Zero, "Edit", IntPtr.Zero);
                            if (Child != IntPtr.Zero)
                            {
                                Console.WriteLine("Disabling address bar: {0:X}", Child);
                                WinAPI.EnableWindow(Child, false);
                                success = true;
                            }
                        }
                    }
                }

                if (!success)
                    Console.WriteLine("WARNING: Failed to disable addressbar!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Refresh page every x seconds
        /// </summary>
        public void Refresh(bool wait = true)
        {
            internetExplorer.Refresh();
        }

        /// <summary>
        /// Set Internet Explorer at foreground
        /// </summary>
        public void SetForeground()
        {
            WinAPI.SetForegroundWindow(GetHWND);
        }

        /// <summary>
        /// Reset to startup state
        /// </summary>
        public void Reset()
        {
            //Init
            init();
        }

        public void Exit()
        {
            try
            {
                if (IsRunning)
                    internetExplorer.Quit();
            }
            catch { } // Do nothing on fail!
        }
    }
}
