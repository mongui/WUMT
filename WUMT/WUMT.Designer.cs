namespace WUMT
{
    partial class WUMT
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WUMT));
            this.panelTitle = new System.Windows.Forms.Panel();
            this.logoTitle = new System.Windows.Forms.PictureBox();
            this.lTitle = new System.Windows.Forms.Label();
            this.pTitle = new System.Windows.Forms.PictureBox();
            this.lBackupRestore = new System.Windows.Forms.Label();
            this.bNext = new System.Windows.Forms.Button();
            this.bBack = new System.Windows.Forms.Button();
            this.Panels = new System.Windows.Forms.TabControl();
            this.IntroSelect = new System.Windows.Forms.TabPage();
            this.l2IntroSelect = new System.Windows.Forms.Label();
            this.lChooseOp = new System.Windows.Forms.Label();
            this.bBackup = new System.Windows.Forms.Button();
            this.bRestore = new System.Windows.Forms.Button();
            this.bCopy = new System.Windows.Forms.Button();
            this.Backup1 = new System.Windows.Forms.TabPage();
            this.b1gDestination = new System.Windows.Forms.GroupBox();
            this.b1tDestination = new System.Windows.Forms.ComboBox();
            this.b1lRecommendation = new System.Windows.Forms.Label();
            this.b1cCompress = new System.Windows.Forms.CheckBox();
            this.b1bExamine = new System.Windows.Forms.Button();
            this.b1lDestination = new System.Windows.Forms.Label();
            this.b1lSelDest = new System.Windows.Forms.Label();
            this.Restore1 = new System.Windows.Forms.TabPage();
            this.Backup2 = new System.Windows.Forms.TabPage();
            this.b2gUsers = new System.Windows.Forms.GroupBox();
            this.b2cPrinters = new System.Windows.Forms.CheckBox();
            this.b2cNetworkDrives = new System.Windows.Forms.CheckBox();
            this.b2cMyDocs = new System.Windows.Forms.CheckBox();
            this.b2cDesktop = new System.Windows.Forms.CheckBox();
            this.b2Folders = new System.Windows.Forms.Label();
            this.b2lSelectAccounts = new System.Windows.Forms.Label();
            this.b2AvailableUsers = new System.Windows.Forms.Label();
            this.b2lbUsers = new System.Windows.Forms.ListBox();
            this.Restore2 = new System.Windows.Forms.TabPage();
            this.Backup3 = new System.Windows.Forms.TabPage();
            this.b3gFolders = new System.Windows.Forms.GroupBox();
            this.c3cSubfolders = new System.Windows.Forms.CheckBox();
            this.b3tvOtherFolders = new System.Windows.Forms.TreeView();
            this.b3OtherFolders = new System.Windows.Forms.Label();
            this.Restore3 = new System.Windows.Forms.TabPage();
            this.Backup4 = new System.Windows.Forms.TabPage();
            this.b4tStatus = new System.Windows.Forms.TextBox();
            this.b4progressBar = new System.Windows.Forms.ProgressBar();
            this.b4l2Collected = new System.Windows.Forms.Label();
            this.b4l1Ready = new System.Windows.Forms.Label();
            this.Restore4 = new System.Windows.Forms.TabPage();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panelTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoTitle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTitle)).BeginInit();
            this.Panels.SuspendLayout();
            this.IntroSelect.SuspendLayout();
            this.Backup1.SuspendLayout();
            this.b1gDestination.SuspendLayout();
            this.Backup2.SuspendLayout();
            this.b2gUsers.SuspendLayout();
            this.Backup3.SuspendLayout();
            this.b3gFolders.SuspendLayout();
            this.Backup4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTitle
            // 
            this.panelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTitle.BackColor = System.Drawing.Color.White;
            this.panelTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTitle.Controls.Add(this.logoTitle);
            this.panelTitle.Controls.Add(this.lTitle);
            this.panelTitle.Controls.Add(this.pTitle);
            this.panelTitle.Location = new System.Drawing.Point(-2, -2);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(690, 102);
            this.panelTitle.TabIndex = 4;
            // 
            // logoTitle
            // 
            this.logoTitle.BackColor = System.Drawing.Color.Transparent;
            this.logoTitle.Image = ((System.Drawing.Image)(resources.GetObject("logoTitle.Image")));
            this.logoTitle.Location = new System.Drawing.Point(21, 7);
            this.logoTitle.Name = "logoTitle";
            this.logoTitle.Size = new System.Drawing.Size(83, 81);
            this.logoTitle.TabIndex = 2;
            this.logoTitle.TabStop = false;
            // 
            // lTitle
            // 
            this.lTitle.AutoSize = true;
            this.lTitle.BackColor = System.Drawing.Color.Transparent;
            this.lTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTitle.Location = new System.Drawing.Point(97, 29);
            this.lTitle.Name = "lTitle";
            this.lTitle.Size = new System.Drawing.Size(536, 39);
            this.lTitle.TabIndex = 0;
            this.lTitle.Text = "Windows Userdata Migration Tool";
            // 
            // pTitle
            // 
            this.pTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pTitle.Image = global::WUMT.Properties.Resources.top;
            this.pTitle.Location = new System.Drawing.Point(-6, 94);
            this.pTitle.Name = "pTitle";
            this.pTitle.Size = new System.Drawing.Size(700, 10);
            this.pTitle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pTitle.TabIndex = 1;
            this.pTitle.TabStop = false;
            // 
            // lBackupRestore
            // 
            this.lBackupRestore.AutoSize = true;
            this.lBackupRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.lBackupRestore.Location = new System.Drawing.Point(30, 123);
            this.lBackupRestore.Name = "lBackupRestore";
            this.lBackupRestore.Size = new System.Drawing.Size(133, 31);
            this.lBackupRestore.TabIndex = 9;
            this.lBackupRestore.Text = "Welcome";
            // 
            // bNext
            // 
            this.bNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bNext.Location = new System.Drawing.Point(584, 503);
            this.bNext.Name = "bNext";
            this.bNext.Size = new System.Drawing.Size(86, 28);
            this.bNext.TabIndex = 10;
            this.bNext.Text = "&Next >";
            this.bNext.UseVisualStyleBackColor = true;
            this.bNext.Click += new System.EventHandler(this.bNext_Click);
            // 
            // bBack
            // 
            this.bBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bBack.Location = new System.Drawing.Point(492, 503);
            this.bBack.Name = "bBack";
            this.bBack.Size = new System.Drawing.Size(86, 28);
            this.bBack.TabIndex = 11;
            this.bBack.Text = "< &Back";
            this.bBack.UseVisualStyleBackColor = true;
            this.bBack.Click += new System.EventHandler(this.bBack_Click);
            // 
            // Panels
            // 
            this.Panels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panels.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.Panels.Controls.Add(this.IntroSelect);
            this.Panels.Controls.Add(this.Backup1);
            this.Panels.Controls.Add(this.Restore1);
            this.Panels.Controls.Add(this.Backup2);
            this.Panels.Controls.Add(this.Restore2);
            this.Panels.Controls.Add(this.Backup3);
            this.Panels.Controls.Add(this.Restore3);
            this.Panels.Controls.Add(this.Backup4);
            this.Panels.Controls.Add(this.Restore4);
            this.Panels.Cursor = System.Windows.Forms.Cursors.Default;
            this.Panels.ItemSize = new System.Drawing.Size(0, 1);
            this.Panels.Location = new System.Drawing.Point(12, 160);
            this.Panels.Name = "Panels";
            this.Panels.SelectedIndex = 0;
            this.Panels.Size = new System.Drawing.Size(662, 332);
            this.Panels.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.Panels.TabIndex = 12;
            // 
            // IntroSelect
            // 
            this.IntroSelect.Controls.Add(this.l2IntroSelect);
            this.IntroSelect.Controls.Add(this.lChooseOp);
            this.IntroSelect.Controls.Add(this.bBackup);
            this.IntroSelect.Controls.Add(this.bRestore);
            this.IntroSelect.Controls.Add(this.bCopy);
            this.IntroSelect.Location = new System.Drawing.Point(4, 5);
            this.IntroSelect.Name = "IntroSelect";
            this.IntroSelect.Padding = new System.Windows.Forms.Padding(3);
            this.IntroSelect.Size = new System.Drawing.Size(654, 323);
            this.IntroSelect.TabIndex = 0;
            this.IntroSelect.Text = "IntroSelect";
            this.IntroSelect.UseVisualStyleBackColor = true;
            // 
            // l2IntroSelect
            // 
            this.l2IntroSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.l2IntroSelect.Location = new System.Drawing.Point(18, 11);
            this.l2IntroSelect.Name = "l2IntroSelect";
            this.l2IntroSelect.Size = new System.Drawing.Size(620, 58);
            this.l2IntroSelect.TabIndex = 9;
            this.l2IntroSelect.Text = resources.GetString("l2IntroSelect.Text");
            // 
            // lChooseOp
            // 
            this.lChooseOp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lChooseOp.AutoSize = true;
            this.lChooseOp.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lChooseOp.Location = new System.Drawing.Point(16, 81);
            this.lChooseOp.Name = "lChooseOp";
            this.lChooseOp.Size = new System.Drawing.Size(170, 25);
            this.lChooseOp.TabIndex = 8;
            this.lChooseOp.Text = "Choose a task:";
            // 
            // bBackup
            // 
            this.bBackup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bBackup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bBackup.Location = new System.Drawing.Point(51, 121);
            this.bBackup.Name = "bBackup";
            this.bBackup.Size = new System.Drawing.Size(561, 57);
            this.bBackup.TabIndex = 10;
            this.bBackup.Text = "      Backup";
            this.bBackup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bBackup.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bBackup.UseVisualStyleBackColor = true;
            this.bBackup.Click += new System.EventHandler(this.bBackup_Click);
            // 
            // bRestore
            // 
            this.bRestore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bRestore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bRestore.Location = new System.Drawing.Point(51, 188);
            this.bRestore.Name = "bRestore";
            this.bRestore.Size = new System.Drawing.Size(561, 57);
            this.bRestore.TabIndex = 11;
            this.bRestore.Text = "      Restore";
            this.bRestore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bRestore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bRestore.UseVisualStyleBackColor = true;
            this.bRestore.Click += new System.EventHandler(this.bRestore_Click);
            this.bRestore.Enabled = false; // TODO: restore is not implemented yet.
            // 
            // bCopy
            // 
            this.bCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bCopy.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bCopy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bCopy.Location = new System.Drawing.Point(51, 255);
            this.bCopy.Name = "bCopy";
            this.bCopy.Size = new System.Drawing.Size(561, 57);
            this.bCopy.TabIndex = 12;
            this.bCopy.Text = "      Copy from an external HD";
            this.bCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bCopy.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bCopy.UseVisualStyleBackColor = true;
            this.bCopy.Enabled = false; // TODO: not implemented yet.
            // 
            // Backup1
            // 
            this.Backup1.Controls.Add(this.b1gDestination);
            this.Backup1.Location = new System.Drawing.Point(4, 5);
            this.Backup1.Name = "Backup1";
            this.Backup1.Padding = new System.Windows.Forms.Padding(3);
            this.Backup1.Size = new System.Drawing.Size(654, 323);
            this.Backup1.TabIndex = 1;
            this.Backup1.Text = "Backup1";
            this.Backup1.UseVisualStyleBackColor = true;
            // 
            // b1gDestination
            // 
            this.b1gDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b1gDestination.Controls.Add(this.b1tDestination);
            this.b1gDestination.Controls.Add(this.b1lRecommendation);
            this.b1gDestination.Controls.Add(this.b1cCompress);
            this.b1gDestination.Controls.Add(this.b1bExamine);
            this.b1gDestination.Controls.Add(this.b1lDestination);
            this.b1gDestination.Controls.Add(this.b1lSelDest);
            this.b1gDestination.Location = new System.Drawing.Point(0, 0);
            this.b1gDestination.Name = "b1gDestination";
            this.b1gDestination.Size = new System.Drawing.Size(654, 281);
            this.b1gDestination.TabIndex = 0;
            this.b1gDestination.TabStop = false;
            this.b1gDestination.Text = "Destination";
            // 
            // b1tDestination
            // 
            this.b1tDestination.FormattingEnabled = true;
            this.b1tDestination.Location = new System.Drawing.Point(86, 87);
            this.b1tDestination.Name = "b1tDestination";
            this.b1tDestination.Size = new System.Drawing.Size(521, 21);
            this.b1tDestination.TabIndex = 6;
            // 
            // b1lRecommendation
            // 
            this.b1lRecommendation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b1lRecommendation.Location = new System.Drawing.Point(17, 159);
            this.b1lRecommendation.Name = "b1lRecommendation";
            this.b1lRecommendation.Size = new System.Drawing.Size(620, 22);
            this.b1lRecommendation.TabIndex = 4;
            this.b1lRecommendation.Text = "It is highly recommended that is preferably located on a removable drive.";
            // 
            // b1cCompress
            // 
            this.b1cCompress.AutoSize = true;
            this.b1cCompress.Enabled = false;
            this.b1cCompress.Location = new System.Drawing.Point(86, 117);
            this.b1cCompress.Name = "b1cCompress";
            this.b1cCompress.Size = new System.Drawing.Size(233, 17);
            this.b1cCompress.TabIndex = 5;
            this.b1cCompress.Text = "Compress all files when backing up. (slower)";
            this.b1cCompress.UseVisualStyleBackColor = true;
            // 
            // b1bExamine
            // 
            this.b1bExamine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.b1bExamine.Location = new System.Drawing.Point(613, 85);
            this.b1bExamine.Name = "b1bExamine";
            this.b1bExamine.Size = new System.Drawing.Size(24, 23);
            this.b1bExamine.TabIndex = 3;
            this.b1bExamine.Text = "...";
            this.b1bExamine.UseVisualStyleBackColor = true;
            this.b1bExamine.Click += new System.EventHandler(this.b1bExamine_Click);
            // 
            // b1lDestination
            // 
            this.b1lDestination.AutoSize = true;
            this.b1lDestination.Location = new System.Drawing.Point(17, 90);
            this.b1lDestination.Name = "b1lDestination";
            this.b1lDestination.Size = new System.Drawing.Size(41, 13);
            this.b1lDestination.TabIndex = 1;
            this.b1lDestination.Text = "Target:";
            // 
            // b1lSelDest
            // 
            this.b1lSelDest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b1lSelDest.Location = new System.Drawing.Point(17, 25);
            this.b1lSelDest.Name = "b1lSelDest";
            this.b1lSelDest.Size = new System.Drawing.Size(620, 34);
            this.b1lSelDest.TabIndex = 0;
            this.b1lSelDest.Text = "Select a target place where the data will be backed up. In this will create a fol" +
    "der in which files from this computer are to be copied.";
            // 
            // Restore1
            // 
            this.Restore1.Location = new System.Drawing.Point(4, 5);
            this.Restore1.Name = "Restore1";
            this.Restore1.Size = new System.Drawing.Size(654, 323);
            this.Restore1.TabIndex = 2;
            this.Restore1.Text = "Restore1";
            this.Restore1.UseVisualStyleBackColor = true;
            // 
            // Backup2
            // 
            this.Backup2.Controls.Add(this.b2gUsers);
            this.Backup2.Location = new System.Drawing.Point(4, 5);
            this.Backup2.Name = "Backup2";
            this.Backup2.Size = new System.Drawing.Size(654, 323);
            this.Backup2.TabIndex = 3;
            this.Backup2.Text = "Backup2";
            this.Backup2.UseVisualStyleBackColor = true;
            // 
            // b2gUsers
            // 
            this.b2gUsers.Controls.Add(this.b2cPrinters);
            this.b2gUsers.Controls.Add(this.b2cNetworkDrives);
            this.b2gUsers.Controls.Add(this.b2cMyDocs);
            this.b2gUsers.Controls.Add(this.b2cDesktop);
            this.b2gUsers.Controls.Add(this.b2Folders);
            this.b2gUsers.Controls.Add(this.b2lSelectAccounts);
            this.b2gUsers.Controls.Add(this.b2AvailableUsers);
            this.b2gUsers.Controls.Add(this.b2lbUsers);
            this.b2gUsers.Location = new System.Drawing.Point(0, 0);
            this.b2gUsers.Name = "b2gUsers";
            this.b2gUsers.Size = new System.Drawing.Size(654, 323);
            this.b2gUsers.TabIndex = 0;
            this.b2gUsers.TabStop = false;
            this.b2gUsers.Text = "Users";
            // 
            // b2cPrinters
            // 
            this.b2cPrinters.AutoSize = true;
            this.b2cPrinters.Checked = true;
            this.b2cPrinters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.b2cPrinters.Location = new System.Drawing.Point(20, 283);
            this.b2cPrinters.Name = "b2cPrinters";
            this.b2cPrinters.Size = new System.Drawing.Size(61, 17);
            this.b2cPrinters.TabIndex = 8;
            this.b2cPrinters.Text = "Printers";
            this.b2cPrinters.UseVisualStyleBackColor = true;
            // 
            // b2cNetworkDrives
            // 
            this.b2cNetworkDrives.AutoSize = true;
            this.b2cNetworkDrives.Checked = true;
            this.b2cNetworkDrives.CheckState = System.Windows.Forms.CheckState.Checked;
            this.b2cNetworkDrives.Location = new System.Drawing.Point(20, 259);
            this.b2cNetworkDrives.Name = "b2cNetworkDrives";
            this.b2cNetworkDrives.Size = new System.Drawing.Size(97, 17);
            this.b2cNetworkDrives.TabIndex = 7;
            this.b2cNetworkDrives.Text = "Network drives";
            this.b2cNetworkDrives.UseVisualStyleBackColor = true;
            // 
            // b2cMyDocs
            // 
            this.b2cMyDocs.AutoSize = true;
            this.b2cMyDocs.Checked = true;
            this.b2cMyDocs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.b2cMyDocs.Location = new System.Drawing.Point(20, 235);
            this.b2cMyDocs.Name = "b2cMyDocs";
            this.b2cMyDocs.Size = new System.Drawing.Size(97, 17);
            this.b2cMyDocs.TabIndex = 6;
            this.b2cMyDocs.Text = "My Documents";
            this.b2cMyDocs.UseVisualStyleBackColor = true;
            // 
            // b2cDesktop
            // 
            this.b2cDesktop.AutoSize = true;
            this.b2cDesktop.Checked = true;
            this.b2cDesktop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.b2cDesktop.Location = new System.Drawing.Point(20, 211);
            this.b2cDesktop.Name = "b2cDesktop";
            this.b2cDesktop.Size = new System.Drawing.Size(66, 17);
            this.b2cDesktop.TabIndex = 5;
            this.b2cDesktop.Text = "Desktop";
            this.b2cDesktop.UseVisualStyleBackColor = true;
            // 
            // b2Folders
            // 
            this.b2Folders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b2Folders.Location = new System.Drawing.Point(20, 188);
            this.b2Folders.Name = "b2Folders";
            this.b2Folders.Size = new System.Drawing.Size(612, 19);
            this.b2Folders.TabIndex = 4;
            this.b2Folders.Text = "Select which folders will be saved:";
            // 
            // b2lSelectAccounts
            // 
            this.b2lSelectAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b2lSelectAccounts.Location = new System.Drawing.Point(20, 20);
            this.b2lSelectAccounts.Name = "b2lSelectAccounts";
            this.b2lSelectAccounts.Size = new System.Drawing.Size(612, 34);
            this.b2lSelectAccounts.TabIndex = 3;
            this.b2lSelectAccounts.Text = "Select which user accounts will be saved. If you want to back up others than your" +
    " user account data, you must be sure to have administrator level rights.\r\n";
            // 
            // b2AvailableUsers
            // 
            this.b2AvailableUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b2AvailableUsers.Location = new System.Drawing.Point(17, 59);
            this.b2AvailableUsers.Name = "b2AvailableUsers";
            this.b2AvailableUsers.Size = new System.Drawing.Size(612, 16);
            this.b2AvailableUsers.TabIndex = 2;
            this.b2AvailableUsers.Text = "Available user accounts:";
            // 
            // b2lbUsers
            // 
            this.b2lbUsers.FormattingEnabled = true;
            this.b2lbUsers.Location = new System.Drawing.Point(20, 81);
            this.b2lbUsers.Name = "b2lbUsers";
            this.b2lbUsers.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.b2lbUsers.Size = new System.Drawing.Size(201, 82);
            this.b2lbUsers.TabIndex = 1;
            // 
            // Restore2
            // 
            this.Restore2.Location = new System.Drawing.Point(4, 5);
            this.Restore2.Name = "Restore2";
            this.Restore2.Size = new System.Drawing.Size(654, 323);
            this.Restore2.TabIndex = 4;
            this.Restore2.Text = "Restore2";
            this.Restore2.UseVisualStyleBackColor = true;
            // 
            // Backup3
            // 
            this.Backup3.Controls.Add(this.b3gFolders);
            this.Backup3.Location = new System.Drawing.Point(4, 5);
            this.Backup3.Name = "Backup3";
            this.Backup3.Size = new System.Drawing.Size(654, 323);
            this.Backup3.TabIndex = 5;
            this.Backup3.Text = "Backup3";
            this.Backup3.UseVisualStyleBackColor = true;
            // 
            // b3gFolders
            // 
            this.b3gFolders.Controls.Add(this.c3cSubfolders);
            this.b3gFolders.Controls.Add(this.b3tvOtherFolders);
            this.b3gFolders.Controls.Add(this.b3OtherFolders);
            this.b3gFolders.Location = new System.Drawing.Point(0, 0);
            this.b3gFolders.Name = "b3gFolders";
            this.b3gFolders.Size = new System.Drawing.Size(654, 323);
            this.b3gFolders.TabIndex = 0;
            this.b3gFolders.TabStop = false;
            this.b3gFolders.Text = "Other folders";
            // 
            // c3cSubfolders
            // 
            this.c3cSubfolders.AutoSize = true;
            this.c3cSubfolders.Checked = true;
            this.c3cSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.c3cSubfolders.Location = new System.Drawing.Point(20, 300);
            this.c3cSubfolders.Name = "c3cSubfolders";
            this.c3cSubfolders.Size = new System.Drawing.Size(126, 17);
            this.c3cSubfolders.TabIndex = 2;
            this.c3cSubfolders.Text = "Also copy subfolders.";
            this.c3cSubfolders.UseVisualStyleBackColor = true;
            // 
            // b3tvOtherFolders
            // 
            this.b3tvOtherFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b3tvOtherFolders.CheckBoxes = true;
            this.b3tvOtherFolders.Location = new System.Drawing.Point(17, 44);
            this.b3tvOtherFolders.Name = "b3tvOtherFolders";
            this.b3tvOtherFolders.Size = new System.Drawing.Size(618, 250);
            this.b3tvOtherFolders.TabIndex = 1;
            this.b3tvOtherFolders.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.b3tvOtherFolders_BeforeExpand);
            // 
            // b3OtherFolders
            // 
            this.b3OtherFolders.AutoSize = true;
            this.b3OtherFolders.Location = new System.Drawing.Point(17, 21);
            this.b3OtherFolders.Name = "b3OtherFolders";
            this.b3OtherFolders.Size = new System.Drawing.Size(187, 13);
            this.b3OtherFolders.TabIndex = 0;
            this.b3OtherFolders.Text = "Do you want to save any other folder?";
            // 
            // Restore3
            // 
            this.Restore3.Location = new System.Drawing.Point(4, 5);
            this.Restore3.Name = "Restore3";
            this.Restore3.Size = new System.Drawing.Size(654, 323);
            this.Restore3.TabIndex = 6;
            this.Restore3.Text = "Restore3";
            this.Restore3.UseVisualStyleBackColor = true;
            // 
            // Backup4
            // 
            this.Backup4.Controls.Add(this.b4tStatus);
            this.Backup4.Controls.Add(this.b4progressBar);
            this.Backup4.Controls.Add(this.b4l2Collected);
            this.Backup4.Controls.Add(this.b4l1Ready);
            this.Backup4.Location = new System.Drawing.Point(4, 5);
            this.Backup4.Name = "Backup4";
            this.Backup4.Size = new System.Drawing.Size(654, 323);
            this.Backup4.TabIndex = 7;
            this.Backup4.Text = "Backup4";
            this.Backup4.UseVisualStyleBackColor = true;
            // 
            // b4tStatus
            // 
            this.b4tStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b4tStatus.Location = new System.Drawing.Point(17, 134);
            this.b4tStatus.Multiline = true;
            this.b4tStatus.Name = "b4tStatus";
            this.b4tStatus.ReadOnly = true;
            this.b4tStatus.Size = new System.Drawing.Size(619, 178);
            this.b4tStatus.TabIndex = 12;
            // 
            // b4progressBar
            // 
            this.b4progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b4progressBar.Location = new System.Drawing.Point(17, 104);
            this.b4progressBar.Name = "b4progressBar";
            this.b4progressBar.Size = new System.Drawing.Size(619, 23);
            this.b4progressBar.TabIndex = 11;
            // 
            // b4l2Collected
            // 
            this.b4l2Collected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b4l2Collected.Location = new System.Drawing.Point(17, 62);
            this.b4l2Collected.Name = "b4l2Collected";
            this.b4l2Collected.Size = new System.Drawing.Size(619, 39);
            this.b4l2Collected.TabIndex = 10;
            this.b4l2Collected.Text = "The application has collected all the information needed to make the backup. Plea" +
    "se, before start make sure that the selected destination drive (%DST_DRIVE%) has" +
    " enough space to save all the data.";
            // 
            // b4l1Ready
            // 
            this.b4l1Ready.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.b4l1Ready.AutoSize = true;
            this.b4l1Ready.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b4l1Ready.Location = new System.Drawing.Point(15, 22);
            this.b4l1Ready.Name = "b4l1Ready";
            this.b4l1Ready.Size = new System.Drawing.Size(167, 25);
            this.b4l1Ready.TabIndex = 9;
            this.b4l1Ready.Text = "Ready to start.";
            // 
            // Restore4
            // 
            this.Restore4.Location = new System.Drawing.Point(4, 5);
            this.Restore4.Name = "Restore4";
            this.Restore4.Size = new System.Drawing.Size(654, 323);
            this.Restore4.TabIndex = 8;
            this.Restore4.Text = "Restore4";
            this.Restore4.UseVisualStyleBackColor = true;
            // 
            // WUMT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 543);
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.lBackupRestore);
            this.Controls.Add(this.Panels);
            this.Controls.Add(this.bNext);
            this.Controls.Add(this.bBack);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::WUMT.Properties.Resources.BackupRestoreIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WUMT";
            this.Text = "Windows Userdata Migration Tool 1.0";
            this.Load += new System.EventHandler(this.WUMT_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WUMT_FormClosing);
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoTitle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTitle)).EndInit();
            this.Panels.ResumeLayout(false);
            this.IntroSelect.ResumeLayout(false);
            this.IntroSelect.PerformLayout();
            this.Backup1.ResumeLayout(false);
            this.b1gDestination.ResumeLayout(false);
            this.b1gDestination.PerformLayout();
            this.Backup2.ResumeLayout(false);
            this.b2gUsers.ResumeLayout(false);
            this.b2gUsers.PerformLayout();
            this.Backup3.ResumeLayout(false);
            this.b3gFolders.ResumeLayout(false);
            this.b3gFolders.PerformLayout();
            this.Backup4.ResumeLayout(false);
            this.Backup4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label lTitle;
        private System.Windows.Forms.PictureBox pTitle;
        private System.Windows.Forms.Label lBackupRestore;
        private System.Windows.Forms.Button bNext;
        private System.Windows.Forms.Button bBack;
        private System.Windows.Forms.TabControl Panels;
        private System.Windows.Forms.TabPage IntroSelect;
        private System.Windows.Forms.Label l2IntroSelect;
        private System.Windows.Forms.Label lChooseOp;
        private System.Windows.Forms.Button bBackup;
        private System.Windows.Forms.Button bRestore;
        private System.Windows.Forms.TabPage Backup1;
        private System.Windows.Forms.TabPage Restore1;
        private System.Windows.Forms.GroupBox b1gDestination;
        private System.Windows.Forms.Label b1lSelDest;
        private System.Windows.Forms.Button b1bExamine;
        private System.Windows.Forms.Label b1lDestination;
        private System.Windows.Forms.TabPage Backup2;
        private System.Windows.Forms.TabPage Restore2;
        private System.Windows.Forms.TabPage Backup3;
        private System.Windows.Forms.TabPage Restore3;
        private System.Windows.Forms.TabPage Backup4;
        private System.Windows.Forms.TabPage Restore4;
        private System.Windows.Forms.Label b1lRecommendation;
        private System.Windows.Forms.CheckBox b1cCompress;
        private System.Windows.Forms.GroupBox b2gUsers;
        private System.Windows.Forms.Label b2lSelectAccounts;
        private System.Windows.Forms.Label b2AvailableUsers;
        private System.Windows.Forms.ListBox b2lbUsers;
        private System.Windows.Forms.CheckBox b2cPrinters;
        private System.Windows.Forms.CheckBox b2cNetworkDrives;
        private System.Windows.Forms.CheckBox b2cMyDocs;
        private System.Windows.Forms.CheckBox b2cDesktop;
        private System.Windows.Forms.Label b2Folders;
        private System.Windows.Forms.ComboBox b1tDestination;
        private System.Windows.Forms.GroupBox b3gFolders;
        private System.Windows.Forms.Label b3OtherFolders;
        private System.Windows.Forms.TreeView b3tvOtherFolders;
        private System.Windows.Forms.ProgressBar b4progressBar;
        private System.Windows.Forms.Label b4l2Collected;
        private System.Windows.Forms.Label b4l1Ready;
        private System.Windows.Forms.TextBox b4tStatus;
        private System.Windows.Forms.CheckBox c3cSubfolders;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.PictureBox logoTitle;
        private System.Windows.Forms.Button bCopy;

    }
}

