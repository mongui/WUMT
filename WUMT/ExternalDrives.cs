using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WUMT
{
    public class ExternalDrives
    {
        private List<DriveInfo> GetRemovableDrives()
        {
            return DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToList();
        }

        public String[] GetRemovableDriveNames()
        {
            List<DriveInfo> extDrvs = GetRemovableDrives();
            String[] returnedNames = new String[extDrvs.Count];

            for (int i = 0; i < extDrvs.Count; i++)
	        {
                returnedNames[i] = extDrvs[i].Name;
            }

            return returnedNames;
        }
    }
}
