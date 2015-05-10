#Windows Userdata Migration Tool

**Windows Userdata Migration Tool (WUMT)** is a wizard style application designed to make work easier the long and boring process of backing up data from old computers and restoring it to new ones.

**Note:** the program is far from being finished so I don't recommend using it until most of the features are done (see TODO).


##Getting Started
There's no need to install any other external software than .NET runtime libraries that come with Windows. The idea is to use this application with an external data storage like a USB drive where user's data would be backed up.

Now, follow these three easy steps:

- Download latest version of WUMT from [here](https://github.com/mongui/WUMT/archive/master.zip).
- Decompress it.
- Execute WUMT.exe which is located inside the bin/release folder and follow the instructions.

####What does it backup/restore?

- Desktop, My Documents folders.
- Other folders selected by user.
- Printers.
- Network drives.
- Windows Serial Number.

####Windows XP users:

This version of Windows doesn't come with .NET 3.5 out of the box until SP2 so it is possible that some computers don't have it. To avoid this inconvenience I found a launcher that compiles all the necessary libraries to execute WUMT [here](https://ayra.ch/apps/default.asp?show=79) (direct download link [here](https://ayra.ch/apps/bin/FULL.NET.zip)), but as long as .NET can only be distributed as its official installer, using this tool could be a violation of the End User License Agreement of the .NET framework, so use it under your responsibility.


##How to build

Clone a copy of the main WUMT git repo by running:

```git clone git://github.com/WUMT/WUMT.git```

Then, open the solution with Visual Studio and Compile/Run it.


##TODO

- Code the 'Restore' part.
- Add code comments.
- Add more backup options.
- Add direct data copy from an old computer hard drive connected to a new computer.
