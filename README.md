# Windows Userdata Migration Tool

**Windows Userdata Migration Tool (WUMT)** is a wizard for Windows that moves user data from an old computer to a new one.

It has three modes:

| Mode | What it does |
|---|---|
| **Backup** | Saves the data of the computer to a USB drive, an external disk or any folder. |
| **Restore** | Brings a WUMT backup back on the new computer. |
| **Copy from an external HD** | Copies the data straight from the old computer's hard disk connected to the new one, with no intermediate backup. |

## Requirements

- Windows XP or later (tested on Windows 10).
- .NET Framework 3.5, or .NET Framework 4 when 3.5 is not installed (Windows 10/11 include it).
- No installation needed: just run `WUMT.exe`.
- Run it **as administrator** to work with every user of the computer, including users that are not logged in, and to read the settings stored on an old disk. Without administrator rights only the current user is available.

#### Windows XP users

Windows XP doesn't include .NET 3.5 before SP2, so some computers may not have it. [This launcher](https://ayra.ch/apps/default.asp?show=79) bundles the libraries needed to run WUMT, but .NET can only be distributed with its official installer, so using it could break the .NET Framework license. Use it at your own risk.

## What it moves

| Data | Details |
|---|---|
| **Desktop** and **My Documents** of each user | Their real location according to the registry, even if the user moved them. |
| **Other folders** | Any folder of the fixed disks, with or without subfolders. |
| **Network drives** of each user | Drive letter and network path (`Z:` → `\\server\share`). |
| **Network printers** of each user | `\\server\printer`. |
| **Local printers** | Listed so they can be installed again by hand (they need their drivers). |

## Features

- Step-by-step wizard with a summary and the total size before starting.
- **Backup**
  - USB drives are offered as destination automatically.
  - Optional **compression** of every file (GZip).
  - Each backup goes to a new folder named after the date and time, so older backups are never overwritten.
- **Restore** and **Copy from an external HD**
  - Backups and old Windows disks are found automatically.
  - Each old user can be copied to any user of the new computer (for example "Ann" → "ann.smith").
  - Other folders go back to their original location or into a folder you choose.
  - Existing files are kept unless you choose to overwrite them.
  - Network drives and printers are connected right away for the current user, and at the next logon for other users.
- **Safe copy**
  - Background copy with a progress bar and a **Cancel** button.
  - Paths longer than 260 characters.
  - File dates are kept, even when compressing.
  - Windows junctions such as "My Music" inside Documents are skipped.
  - A file that can't be copied doesn't stop the copy: it is listed on screen and saved to a log.
  - A folder is never copied into itself.

## Backup layout

```
E:\
└── 2026.10.24-13.45.00\
    ├── Backup.ini        ← backup information (users, folders, drives, printers)
    ├── Errors.log        ← only if something failed
    ├── Users\
    │   └── Ann\
    │       ├── Desktop\
    │       └── Documents\
    └── C\                ← other folders, under their drive letter
        └── Projects\
```

Compressed backups add `.gz` to every file (standard GZip, readable with 7-Zip or WinRAR).

## Getting started

1. Download `WUMT-x.y.z.zip` from the [latest release](https://github.com/mongui/WUMT/releases/latest) and unzip it.
2. Run `WUMT.exe`.
3. Choose **Backup**, **Restore** or **Copy from an external HD** and follow the steps.

The zip also includes `LEEME.md`, a full guide in Spanish.

## How to build

```
git clone https://github.com/mongui/WUMT.git
```

Open `WUMT.sln` with Visual Studio and build the **Release** configuration. The program goes to `WUMT\bin\Release\WUMT.exe`.

## TODO

- Add more code comments.
- Add more backup options (browser data, Outlook files, desktop settings…).

## License

Apache License 2.0. See [LICENSE](LICENSE).
