using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WUMT.WinVer;

namespace WUMT
{
    /// <summary>
    /// Restore and "Copy from an external HD" pages of the wizard.
    /// They share the Restore1..4 tab pages; the controls are created here.
    /// </summary>
    public partial class WUMT
    {
        private Restore restore;
        private bool restoreUsersLoaded;
        private bool restoreFoldersLoaded;

        // Restore 1: source.
        private Label r1lInfo;
        private ComboBox r1cSource;
        private Button r1bExamine;
        private TextBox r1tInfo;

        // Restore 2: users and options.
        private DataGridView r2gUsers;
        private CheckBox r2cDesktop;
        private CheckBox r2cMyDocs;
        private CheckBox r2cNetworkDrives;
        private CheckBox r2cPrinters;
        private CheckBox r2cOverwrite;

        // Restore 3: other folders.
        private Label r3lInfo;
        private CheckedListBox r3lFolders;
        private Button r3bAddFolder;
        private RadioButton r3rOriginal;
        private RadioButton r3rAlternative;
        private TextBox r3tAlternative;
        private Button r3bAlternative;

        // Restore 4: summary and progress.
        private Label r4lInfo;
        private ProgressBar r4progressBar;
        private TextBox r4tStatus;

        private const int ColCopy = 0, ColOldUser = 1, ColNewUser = 2;

        private void InitializeRestorePages()
        {
            AnchorStyles topWide = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            AnchorStyles fill = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Restore 1.
            r1lInfo = NewLabel(Restore1, 17, 15, 620, 34, topWide);
            Label r1lSource = NewLabel(Restore1, 17, 62, 63, 13, AnchorStyles.Top | AnchorStyles.Left);
            r1lSource.Text = "Source:";

            r1cSource = new ComboBox();
            r1cSource.SetBounds(86, 59, 521, 21);
            r1cSource.Anchor = topWide;
            r1cSource.SelectedIndexChanged += delegate { ShowRestoreSourceInfo(); };
            Restore1.Controls.Add(r1cSource);

            r1bExamine = new Button();
            r1bExamine.Text = "...";
            r1bExamine.SetBounds(613, 57, 25, 23);
            r1bExamine.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            r1bExamine.Click += r1bExamine_Click;
            Restore1.Controls.Add(r1bExamine);

            r1tInfo = NewStatusBox(Restore1, 17, 95, 619, 215, fill);

            // Restore 2.
            Label r2lInfo = NewLabel(Restore2, 17, 15, 620, 20, topWide);
            r2lInfo.Text = "Select which users to copy and to which user account of this computer.";

            r2gUsers = new DataGridView();
            r2gUsers.SetBounds(17, 40, 619, 150);
            r2gUsers.Anchor = fill;
            r2gUsers.AllowUserToAddRows = false;
            r2gUsers.AllowUserToDeleteRows = false;
            r2gUsers.RowHeadersVisible = false;
            r2gUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            r2gUsers.EditMode = DataGridViewEditMode.EditOnEnter;
            r2gUsers.BackgroundColor = SystemColors.Window;

            DataGridViewCheckBoxColumn colCopy = new DataGridViewCheckBoxColumn();
            colCopy.HeaderText = "Copy";
            colCopy.FillWeight = 15;
            DataGridViewTextBoxColumn colOld = new DataGridViewTextBoxColumn();
            colOld.HeaderText = "Old user";
            colOld.ReadOnly = true;
            colOld.FillWeight = 40;
            DataGridViewComboBoxColumn colNew = new DataGridViewComboBoxColumn();
            colNew.HeaderText = "Copy to user of this computer";
            colNew.FillWeight = 45;
            colNew.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            r2gUsers.Columns.AddRange(new DataGridViewColumn[] { colCopy, colOld, colNew });
            Restore2.Controls.Add(r2gUsers);

            AnchorStyles bottomLeft = AnchorStyles.Bottom | AnchorStyles.Left;
            r2cDesktop = NewCheckBox(Restore2, "Desktop", 20, 200, bottomLeft);
            r2cMyDocs = NewCheckBox(Restore2, "My Documents", 20, 223, bottomLeft);
            r2cNetworkDrives = NewCheckBox(Restore2, "Network drives", 200, 200, bottomLeft);
            r2cPrinters = NewCheckBox(Restore2, "Network printers", 200, 223, bottomLeft);
            r2cOverwrite = NewCheckBox(Restore2, "Overwrite files that already exist in this computer", 20, 260, bottomLeft);
            r2cOverwrite.Checked = false;

            // Restore 3.
            r3lInfo = NewLabel(Restore3, 17, 15, 620, 20, topWide);

            r3lFolders = new CheckedListBox();
            r3lFolders.SetBounds(17, 40, 619, 160);
            r3lFolders.Anchor = fill;
            r3lFolders.CheckOnClick = true;
            r3lFolders.IntegralHeight = false;
            Restore3.Controls.Add(r3lFolders);

            r3bAddFolder = new Button();
            r3bAddFolder.Text = "Add folder...";
            r3bAddFolder.SetBounds(17, 206, 110, 23);
            r3bAddFolder.Anchor = bottomLeft;
            r3bAddFolder.Click += r3bAddFolder_Click;
            Restore3.Controls.Add(r3bAddFolder);

            r3rOriginal = new RadioButton();
            r3rOriginal.Text = "Copy them to their original location";
            r3rOriginal.SetBounds(17, 240, 400, 20);
            r3rOriginal.Anchor = bottomLeft;
            r3rOriginal.Checked = true;
            Restore3.Controls.Add(r3rOriginal);

            r3rAlternative = new RadioButton();
            r3rAlternative.Text = "Copy them into this folder:";
            r3rAlternative.SetBounds(17, 266, 160, 20);
            r3rAlternative.Anchor = bottomLeft;
            Restore3.Controls.Add(r3rAlternative);

            r3tAlternative = new TextBox();
            r3tAlternative.SetBounds(180, 265, 427, 20);
            r3tAlternative.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            r3tAlternative.TextChanged += delegate { r3rAlternative.Checked = true; };
            Restore3.Controls.Add(r3tAlternative);

            r3bAlternative = new Button();
            r3bAlternative.Text = "...";
            r3bAlternative.SetBounds(613, 263, 25, 23);
            r3bAlternative.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            r3bAlternative.Click += delegate
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    r3tAlternative.Text = folderBrowserDialog.SelectedPath;
            };
            Restore3.Controls.Add(r3bAlternative);

            // Restore 4.
            r4lInfo = NewLabel(Restore4, 17, 15, 620, 40, topWide);
            r4progressBar = new ProgressBar();
            r4progressBar.SetBounds(17, 62, 619, 23);
            r4progressBar.Anchor = topWide;
            Restore4.Controls.Add(r4progressBar);
            r4tStatus = NewStatusBox(Restore4, 17, 95, 619, 215, fill);
        }

        private static Label NewLabel(Control parent, int x, int y, int w, int h, AnchorStyles anchor)
        {
            Label label = new Label();
            label.SetBounds(x, y, w, h);
            label.Anchor = anchor;
            parent.Controls.Add(label);
            return label;
        }

        private static CheckBox NewCheckBox(Control parent, string text, int x, int y, AnchorStyles anchor)
        {
            CheckBox check = new CheckBox();
            check.Text = text;
            check.AutoSize = true;
            check.Location = new Point(x, y);
            check.Anchor = anchor;
            check.Checked = true;
            parent.Controls.Add(check);
            return check;
        }

        private static TextBox NewStatusBox(Control parent, int x, int y, int w, int h, AnchorStyles anchor)
        {
            TextBox box = new TextBox();
            box.SetBounds(x, y, w, h);
            box.Anchor = anchor;
            box.Multiline = true;
            box.ReadOnly = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.BackColor = SystemColors.Window;
            parent.Controls.Add(box);
            return box;
        }

        /* ****************************************************************************** */
        /* *                               PAGES                                        * */
        /* ****************************************************************************** */

        private void ShowRestorePage()
        {
            bool copyMode = mode == WizardMode.Copy;

            if (step == 2)
            {
                r1lInfo.Text = copyMode
                    ? "Select the drive of the old computer's hard disk, connected to this computer. Run WUMT as administrator to also copy moved folders, network drives and printers."
                    : "Select the folder of the backup to restore (the one that contains " + Backup.IniName + ").";

                if (r1cSource.Items.Count == 0 || r1cSource.Tag == null || (WizardMode)r1cSource.Tag != mode)
                {
                    r1cSource.Items.Clear();
                    r1cSource.Text = "";
                    foreach (string source in copyMode ? Restore.FindWindowsDisks() : Restore.FindBackups())
                        r1cSource.Items.Add(source);
                    r1cSource.Tag = mode;
                    if (r1cSource.Items.Count > 0)
                        r1cSource.SelectedIndex = 0;
                    else
                        r1tInfo.Text = copyMode ? "No old Windows disk found. Connect it and press Back, then Next, to search again, or choose its drive with \"...\"."
                                                : "No backup was found. Choose its folder with the \"...\" button.";
                }
            }
            else if (step == 4)
            {
                if (!restoreUsersLoaded)
                    LoadRestoreUsers();
            }
            else if (step == 6)
            {
                r3lInfo.Text = copyMode
                    ? "Add other folders of the old disk to copy:"
                    : "Select which other folders of the backup will be restored:";
                r3bAddFolder.Visible = copyMode;

                if (!restoreFoldersLoaded)
                {
                    r3lFolders.Items.Clear();
                    foreach (MigrationFolder folder in restore.ExtraFolders)
                        r3lFolders.Items.Add(folder, true);
                    restoreFoldersLoaded = true;
                }
            }
            else if (step == 8)
            {
                r4lInfo.Text = (copyMode ? "Ready to copy from " : "Ready to restore from ") + restore.SourcePath
                    + ". Press Start to begin.";
                r4progressBar.Value = 0;
                bNext.Text = "Start";
                bNext.Enabled = PrepareRestore();
            }
        }

        private void r1bExamine_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                r1cSource.Text = folderBrowserDialog.SelectedPath;
                ShowRestoreSourceInfo();
            }
        }

        private Restore LoadRestoreSource(bool showErrors)
        {
            string source = r1cSource.Text.Trim();
            try
            {
                if (source.Length == 0 || !Directory.Exists(source))
                    throw new DirectoryNotFoundException("Select an existing folder or drive.");

                if (mode == WizardMode.Copy)
                {
                    Restore r = Restore.FromDisk(Path.GetPathRoot(Path.GetFullPath(source)));
                    if (r.SourceUsers.Count == 0)
                        throw new InvalidOperationException("No Windows users were found in " + r.SourcePath + ".");
                    return r;
                }

                return Restore.FromBackup(source);
            }
            catch (Exception ex)
            {
                if (showErrors)
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    r1tInfo.Text = ex.Message;
                return null;
            }
        }

        private void ShowRestoreSourceInfo()
        {
            Restore r = LoadRestoreSource(false);
            if (r == null)
                return;

            StringBuilder info = new StringBuilder();
            info.Append(r.Description + "\r\n\r\nUsers:\r\n");
            foreach (MigrationUser user in r.SourceUsers)
            {
                List<string> parts = new List<string>();
                if (user.DesktopSource != null) parts.Add("Desktop");
                if (user.PersonalSource != null) parts.Add("Documents");
                if (user.NetworkDrives.Count > 0) parts.Add(user.NetworkDrives.Count + " network drives");
                if (user.Printers.Count > 0) parts.Add(user.Printers.Count + " printers");
                info.Append("  " + user.Name + (parts.Count > 0 ? " (" + String.Join(", ", parts.ToArray()) + ")" : "") + "\r\n");
            }

            if (r.ExtraFolders.Count > 0)
            {
                info.Append("\r\nOther folders:\r\n");
                foreach (MigrationFolder folder in r.ExtraFolders)
                    info.Append("  " + folder.OriginalPath + "\r\n");
            }

            r1tInfo.Text = info.ToString();
        }

        private bool ValidateRestoreSource()
        {
            Restore r = LoadRestoreSource(true);
            if (r == null)
                return false;

            // Only reload the next pages if the source changed.
            if (restore == null || restore.SourcePath != r.SourcePath || restore.Kind != r.Kind)
            {
                restore = r;
                restoreUsersLoaded = false;
                restoreFoldersLoaded = false;
            }
            return true;
        }

        private void LoadRestoreUsers()
        {
            CommonWinData data = new CommonWinData();
            string currentUser = data.GetCurrentUserName();
            List<string> localUsers = new List<string>(data.IsUserAdministrator()
                ? data.GetWindowsUserAccounts()
                : new string[] { currentUser });
            if (!localUsers.Contains(currentUser))
                localUsers.Add(currentUser);

            DataGridViewComboBoxColumn colNew = (DataGridViewComboBoxColumn)r2gUsers.Columns[ColNewUser];
            colNew.Items.Clear();
            foreach (string user in localUsers)
                colNew.Items.Add(user);

            r2gUsers.Rows.Clear();
            foreach (MigrationUser user in restore.SourceUsers)
            {
                // Same user name if it exists here; otherwise, the current user.
                string target = localUsers.Find(delegate(string u) { return String.Equals(u, user.Name, StringComparison.OrdinalIgnoreCase); });
                bool sameName = target != null;
                if (target == null)
                    target = currentUser;

                // Selected by default: users that exist here, or the only one.
                bool copy = sameName || restore.SourceUsers.Count == 1;
                r2gUsers.Rows.Add(copy, user.Name, target);
            }

            restoreUsersLoaded = true;
        }

        private bool ValidateRestoreUsers()
        {
            r2gUsers.EndEdit();
            restore.UserMapping.Clear();

            foreach (DataGridViewRow row in r2gUsers.Rows)
            {
                if (Convert.ToBoolean(row.Cells[ColCopy].Value))
                {
                    string local = row.Cells[ColNewUser].Value as string;
                    if (String.IsNullOrEmpty(local))
                    {
                        MessageBox.Show("Choose the user of this computer for " + row.Cells[ColOldUser].Value + ".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    restore.UserMapping[(string)row.Cells[ColOldUser].Value] = local;
                }
            }

            restore.RestoreDesktop = r2cDesktop.Checked;
            restore.RestorePersonal = r2cMyDocs.Checked;
            restore.RestoreNetworkDrives = r2cNetworkDrives.Checked;
            restore.RestorePrinters = r2cPrinters.Checked;
            restore.Overwrite = r2cOverwrite.Checked;
            return true;
        }

        private void r3bAddFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = restore.SourcePath;
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;

            string path = folderBrowserDialog.SelectedPath;
            if (!Backup.IsSameOrInside(path, restore.SourcePath, true))
            {
                MessageBox.Show("Choose a folder of the disk " + restore.SourcePath + ".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            r3lFolders.Items.Add(restore.AddDiskFolder(path), true);
        }

        private bool ValidateRestoreFolders()
        {
            restore.SelectedFolders.Clear();
            foreach (object item in r3lFolders.CheckedItems)
                restore.SelectedFolders.Add((MigrationFolder)item);

            restore.AlternativeRoot = null;
            if (r3rAlternative.Checked && restore.SelectedFolders.Count > 0)
            {
                string alt = r3tAlternative.Text.Trim();
                bool valid;
                try
                {
                    valid = alt.Length > 0 && Path.IsPathRooted(alt) && Directory.Exists(Path.GetPathRoot(alt));
                }
                catch (ArgumentException)
                {
                    valid = false;
                }

                if (!valid)
                {
                    MessageBox.Show("Choose a valid folder to copy the other folders into.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                restore.AlternativeRoot = alt;
            }

            return true;
        }

        private bool PrepareRestore()
        {
            try
            {
                StringBuilder status = new StringBuilder();
                foreach (string line in restore.GetPlan())
                    status.Append(line + "\r\n");
                r4tStatus.Text = status.ToString();
                return true;
            }
            catch (Exception ex)
            {
                r4tStatus.Text = "Error preparing the copy: " + ex.Message + "\r\n";
                return false;
            }
        }
    }
}
