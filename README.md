# InternetExplorerStarter
Application to position IE Window on multiple monitors.  

# Usage

```
Internet Explorer Starter
Version: 1.1.0.0
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
  -m, --maximize             Maximize window
  -k, --kiosk                Open in kiosk mode
  -f, --fullscreen           Set window fullscreen
  -a, --addressbar           Hide address bar (this also hides tabs)
  -d, --disable_addressbar   Disable addressbar
  -n, --name                 Name for task
  -t, --topmost              Set window always on top
  -e, --keeprunning          Ensures IE is always running
  -r, --refresh=VALUE        refresh every x seconds (this will activate keep
                               running)
      --file=VALUE           Open ies file
      --install              Install/Reinstall file association for .ies files
      --version              Show application version
  -h, --help                 show this message and exit


Examples:
InternetExplorerStarter.exe --screen=2 -u https://www.google.com --url=https://www.github.com -u https://www.bing.com
InternetExplorerStarter.exe --identify
```

# File Association (.ies)
It is now possible to run a single file using file association. All you need to do is run this program from command line with the --install argument

Example:
```
InternetExplorerStarter.exe --install
```


# IES File example

example.ies
```
name=Example
screen=2
x=0
y=0
width=0
height=0
maximize=true
kiosk=false
fullscreen=true
topmost=true
hide_addressbar=false
disable_addressbar=false
keeprunning=true
refresh=10
url=https://www.google.com
```


# Thirdparty tools used 
Fody  
Costura.Fody  
NDesk.Options  

