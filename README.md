# InternetExplorerStarter
Application to position IE Window on multiple monitors.  

# Usage

```
Internet Explorer Starter
Version: 1.0.5.0
Get latest release from: https://github.com/Sidhy/InternetExplorerStarter/releases
Usage: InternetExplorerStarter.exe [OPTIONS]

Options:
  -u, --url=url              The url to open.
  -i, --identify             Identify screens by drawing screen number on
                               each screen
  -s, --screen=x             Place IE window on screen x
  -x=x                       Place IE window on screen x position
  -y=y                       Place IE window on screen y position
      --width=VALUE          Window width
      --height=VALUE         Window height
  -r, --relative             Position window relative to given screen number
  -m, --maximize             Maximize window
  -k, --kiosk                Open in kiosk mode
  -f, --fullscreen           Open in fullscreen mode
  -a, --addressbar           Hide address bar (this also hides tabs)
  -d, --disable_addressbar   Disable addressbar
  -n, --name                 Name for task
  -e, --keeprunning          Ensures IE is always running
      --refresh=VALUE        refresh every x seconds (this will activate keep
                               running)
      --version              Show application version
  -h, --help                 show this message and exit


Examples:
InternetExplorerStarter.exe --screen=2 -u https://www.google.com --url=https://www.github.com -u https://www.bing.com
InternetExplorerStarter.exe --identify
```

# Thirdparty tools used 
Fody  
Costura.Fody  
NDesk.Options  

