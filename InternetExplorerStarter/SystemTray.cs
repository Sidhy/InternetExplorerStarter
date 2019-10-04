using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternetExplorerStarter
{
    public partial class SystemTray : Form
    {
        ContextMenu trayContextMenu;
        MenuItem trayClose, trayShowHideConsole, trayName, traySpacer;

        public SystemTray()
        {
            InitializeComponent();
            Resize += SystemTray_Resize;
            BuildContextMenu();
        }

        private void BuildContextMenu()
        {
            trayContextMenu = new ContextMenu();

            traySpacer = new MenuItem();
            traySpacer.Text = "-";

            // Initialize trayClose
            trayName = new MenuItem();
            trayName.Text = Program.Name;
            trayName.Enabled = false;
            trayContextMenu.MenuItems.Add(trayName);

            trayContextMenu.MenuItems.Add(traySpacer);

            trayShowHideConsole = new MenuItem();
            trayShowHideConsole.Text = "&Show Console";
            trayShowHideConsole.Click += TrayShowHideConsole_Click;
            trayContextMenu.MenuItems.Add(trayShowHideConsole);

            trayClose = new MenuItem();
            trayClose.Text = "E&xit";
            trayClose.Click += TrayClose_Click;
            trayContextMenu.MenuItems.Add(trayClose);

            trayIcon.ContextMenu = trayContextMenu;
        }

        private void TrayShowHideConsole_Click(object sender, EventArgs e)
        {
            if (trayShowHideConsole.Text.Equals("&Show Console"))
            {
                WinAPI.ShowWindow(WinAPI.GetConsoleWindow(), WinAPI.ShowWindowCommands.Show);
                trayShowHideConsole.Text = "&Hide Console";
            }
            else
            { 
                WinAPI.ShowWindow(WinAPI.GetConsoleWindow(), WinAPI.ShowWindowCommands.Hide);
                trayShowHideConsole.Text = "&Show Console";
            }
        }

        private void TrayClose_Click(object sender, EventArgs e)
        {
            Program.Exit = true;
            this.Close();
        }

        private void SystemTray_Load(object sender, EventArgs e)
        {
        }

        private void SystemTray_Resize(object sender, EventArgs e)
        {
            Hide();
            trayIcon.Visible = true;
        }

        
    }
}
