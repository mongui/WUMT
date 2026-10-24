using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WUMT.Properties;
using WUMT.WinVer;

namespace WUMT
{
    public partial class WUMT : Form
    {
        // Placeholder text for nodes whose subfolders haven't been loaded yet.
        private const string DummyNode = "...";

        // Tab page setter.
        private int step = 0;
        private Backup backup;

        private BackgroundWorker bw = new BackgroundWorker();
        private Timer progressTimer = new Timer();
        private bool backupRunning = false;
        private bool backupFinished = false;
        private bool closeWhenDone = false;
        private string collectedTemplate;

        public WUMT()
        {
            InitializeComponent();

            collectedTemplate = b4l2Collected.Text;

            bw.WorkerSupportsCancellation = true;
            bw.DoWork += (sender, args) =>
            {
                backup.StartBackup();
            };
            bw.RunWorkerCompleted += BackupCompleted;

            progressTimer.Interval = 500;
            progressTimer.Tick += (sender, args) =>
            {
                if (backup != null)
                    b4progressBar.Value = backup.CopiedPercentage;
            };
        }

        private void WUMT_Load(object sender, EventArgs e)
        {
            int iconSize = bBackup.Height / 5 * 4;
            bBackup.Image = new Bitmap(Resources.backup, new Size(iconSize, iconSize));
            bRestore.Image = new Bitmap(Resources.restore, new Size(iconSize, iconSize));
            bCopy.Image = new Bitmap(Resources.copy, new Size(iconSize, iconSize));

            LoadExternalDrives();

            SetTabIndex();
        }

        private void WUMT_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backupRunning)
            {
                if (MessageBox.Show("A backup is running. Do you want to cancel it and exit?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    backup.Cancel();
                    closeWhenDone = true; // Closes when the worker finishes.
                }
                e.Cancel = true;
            }
        }

        private void bBack_Click(object sender, EventArgs e)
        {
            if (backupRunning)
            {
                if (MessageBox.Show("Do you want to cancel the backup?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    backup.Cancel();
                    bBack.Enabled = false;
                    b4tStatus.AppendText("Cancelling...\r\n");
                }
                return;
            }

            if (step == 1)
                step--;
            else
                step -= 2;

            SetTabIndex();
        }

        private void bNext_Click(object sender, EventArgs e)
        {
            if (backupFinished)
            {
                Close();
                return;
            }

            if (step == 1 && !ValidateDestination())
                return;

            if (step == 3 && b2lbUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select at least one user account.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (step == 7 /*|| step == start restore */)
            {
                StartBackup();
            }
            else
            {
                step += 2;
                SetTabIndex();
            }
        }

        private void SetTabIndex()
        {
            bNext.Text = "&Next >";
            bBack.Text = "< &Back";
            bBack.Visible = step != 0;
            bNext.Visible = step != 0;

            if (step == 0)
            {
                lBackupRestore.Text = "Welcome";
            }
            else if (step == 3)
            {
                if (b2lbUsers.Items.Count == 0)
                {
                    LoadUserList();
                }
            }
            else if (step == 5)
            {
                if (b3tvOtherFolders.Nodes.Count == 0)
                {
                    LoadFolderTreeDrives();
                }
            }
            else if (step == 7)
            {
                b4l2Collected.Text = collectedTemplate.Replace("%DST_DRIVE%", GetDestinationRoot());
                bNext.Text = "Start";
                bNext.Enabled = PrepareBackup();
            }

            Panels.SelectedIndex = step;
        }


        /* ****************************************************************************** */
        /* *                                 TABS                                       * */
        /* ****************************************************************************** */

        // IntroSelect.
        private void bBackup_Click(object sender, EventArgs e)
        {
            step++;

            lBackupRestore.Text = "Backup";
            SetTabIndex();
        }

        private void bRestore_Click(object sender, EventArgs e)
        {
            step += 2;

            lBackupRestore.Text = "Restore";
            SetTabIndex();
        }


        // Backup 1.
        private void LoadExternalDrives()
        {
            ExternalDrives exDrvs = new ExternalDrives();
            string[] removables = exDrvs.GetRemovableDriveNames();

            if (removables.Length > 0)
                foreach (string drv in removables)
                    b1tDestination.Items.Add(drv);
            else
                b1tDestination.Items.Add(@"C:\");

            b1tDestination.SelectedIndex = 0;
        }

        private void b1bExamine_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                b1tDestination.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private bool ValidateDestination()
        {
            string dest = b1tDestination.Text.Trim();
            string error = null;

            try
            {
                if (dest.Length == 0)
                    error = "Select a destination.";
                else if (!Path.IsPathRooted(dest) || Path.GetPathRoot(dest) == @"\")
                    error = "The destination must be a full path, like E:\\ or E:\\Backups.";
                else if (!Directory.Exists(Path.GetPathRoot(dest)))
                    error = "The drive " + Path.GetPathRoot(dest) + " is not available.";
            }
            catch (ArgumentException)
            {
                error = "The destination contains invalid characters.";
            }

            if (error != null)
            {
                MessageBox.Show(error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            b1tDestination.Text = dest;
            return true;
        }

        private string GetDestinationRoot()
        {
            try
            {
                return Path.GetPathRoot(b1tDestination.Text.Trim()).TrimEnd('\\');
            }
            catch (ArgumentException)
            {
                return b1tDestination.Text;
            }
        }

        // Backup 2.
        private void LoadUserList()
        {
            CommonWinData a = new CommonWinData();
            string currentUser = a.GetCurrentUserName();
            string[] usrs;

            if (a.IsUserAdministrator())
            {
                usrs = a.GetWindowsUserAccounts();
            }
            else
            {
                usrs = new string[] { currentUser };
                b2lbUsers.Enabled = false;
            }

            if (usrs.Length == 0)
                usrs = new string[] { currentUser };

            foreach (string usr in usrs)
            {
                b2lbUsers.Items.Add(usr);
            }

            int index = b2lbUsers.Items.IndexOf(currentUser);
            b2lbUsers.SetSelected(index >= 0 ? index : 0, true);
        }

        // Backup 3.
        private void LoadFolderTreeDrives()
        {
            b3tvOtherFolders.Nodes.Clear();

            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.DriveType == DriveType.Fixed && d.IsReady)
                    b3tvOtherFolders.Nodes.Add(CreateFolderNode(d.Name, d.RootDirectory.FullName));
            }
        }

        private static TreeNode CreateFolderNode(string text, string path)
        {
            TreeNode node = new TreeNode(text);
            node.Tag = path;
            node.Nodes.Add(DummyNode); // Shows the "+" until the subfolders are loaded.
            return node;
        }

        private void b3tvOtherFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Nodes.Count != 1 || node.Nodes[0].Tag != null)
                return;

            node.Nodes.Clear();

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo((string)node.Tag);
                foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                {
                    // Junctions ("Documents and Settings", "My Music"...) are not readable and duplicate other folders.
                    if ((dir.Attributes & FileAttributes.ReparsePoint) != 0)
                        continue;

                    TreeNode child = CreateFolderNode(dir.Name, dir.FullName);
                    child.Checked = node.Checked;
                    node.Nodes.Add(child);
                }
            }
            catch (Exception)
            { }
        }

        private List<string> GetCheckedNodes(TreeNodeCollection Nodes, bool SearchChildrenOfChecked)
        {
            List<string> nodeList = new List<string>();
            foreach (TreeNode node in Nodes)
            {
                if (node.Tag == null)
                    continue; // Placeholder node.

                if (node.Checked)
                    nodeList.Add((string)node.Tag);

                if (node.Nodes.Count > 0 && (SearchChildrenOfChecked || !node.Checked))
                    nodeList.AddRange(GetCheckedNodes(node.Nodes, SearchChildrenOfChecked));
            }

            return nodeList;
        }

        // Backup 4.
        private bool PrepareBackup()
        {
            backup = new Backup();
            b4tStatus.Clear();
            b4progressBar.Value = 0;

            try
            {
                backup.Destination = b1tDestination.Text.Trim();
                backup.Desktop = b2cDesktop.Checked;
                backup.Personal = b2cMyDocs.Checked;
                backup.NetworkDrives = b2cNetworkDrives.Checked;
                backup.Printers = b2cPrinters.Checked;
                backup.RecursiveCopy = c3cSubfolders.Checked;
                backup.ExtraFolders = GetCheckedNodes(b3tvOtherFolders.Nodes, !c3cSubfolders.Checked).ToArray();

                string[] selectedUsers = new string[b2lbUsers.SelectedItems.Count];
                b2lbUsers.SelectedItems.CopyTo(selectedUsers, 0);
                backup.Users = selectedUsers;

                backup.PrepareFolders();

                StringBuilder status = new StringBuilder();

                foreach (string warning in backup.Warnings)
                    status.Append("Warning: " + warning + "\r\n");

                if (backup.AllFolders().Length > 0)
                {
                    status.Append("The following folders " + (backup.RecursiveCopy ? "and subfolders " : "") + "will be backed up:\r\n");

                    foreach (string folder in backup.AllFolders())
                        status.Append(folder + "\r\n");
                }
                else
                {
                    status.Append("No folders selected.\r\n");
                }

                float totalSize = backup.GetTotalSize();
                status.Append("The total size of files to be copied is: " + String.Format("{0:0,0.0}", totalSize / 1024 / 1024) + "MB (" + String.Format("{0:0,0}", totalSize) + " bytes).\r\n");

                b4tStatus.Text = status.ToString();
                b4tStatus.DeselectAll();
                return true;
            }
            catch (Exception ex)
            {
                b4tStatus.Text = "Error preparing the backup: " + ex.Message + "\r\n";
                return false;
            }
        }

        private void StartBackup()
        {
            backupRunning = true;
            bNext.Enabled = false;
            bBack.Text = "Cancel";
            b4tStatus.AppendText("Copying to " + backup.BackupPath + "...\r\n");

            progressTimer.Start();
            bw.RunWorkerAsync();
        }

        private void BackupCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            backupRunning = false;
            progressTimer.Stop();

            if (args.Error != null)
            {
                b4tStatus.AppendText("The backup failed: " + args.Error.Message + "\r\n");
            }
            else
            {
                string[] errors = backup.Errors;
                if (errors.Length > 0)
                {
                    const int maxShown = 50;
                    b4tStatus.AppendText(errors.Length + " files or folders could not be copied:\r\n");
                    for (int i = 0; i < errors.Length && i < maxShown; i++)
                        b4tStatus.AppendText(errors[i] + "\r\n");
                    if (errors.Length > maxShown)
                        b4tStatus.AppendText("... (see Errors.log in the backup folder)\r\n");
                }

                if (backup.Cancelled)
                {
                    b4tStatus.AppendText("Cancelled.\r\n");
                }
                else
                {
                    b4progressBar.Value = 100;
                    b4tStatus.AppendText(errors.Length > 0 ? "Done, with errors.\r\n" : "Done.\r\n");
                }
            }

            backupFinished = true;
            bBack.Visible = false;
            bBack.Enabled = true;
            bNext.Enabled = true;
            bNext.Text = "&Finish";

            if (closeWhenDone)
                Close();
        }
    }
}
