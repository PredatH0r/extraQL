namespace ExtraQL
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.label1 = new System.Windows.Forms.Label();
      this.txtLauncherExe = new System.Windows.Forms.TextBox();
      this.btnLauncherExe = new System.Windows.Forms.Button();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.lblRealm = new System.Windows.Forms.Label();
      this.btnInstallHook = new System.Windows.Forms.Button();
      this.panelAdvanced = new System.Windows.Forms.Panel();
      this.grpAdvanced = new System.Windows.Forms.GroupBox();
      this.cbDisableScripts = new System.Windows.Forms.CheckBox();
      this.linkFocusForum = new System.Windows.Forms.LinkLabel();
      this.linkFocusLogin = new System.Windows.Forms.LinkLabel();
      this.comboRealm = new System.Windows.Forms.ComboBox();
      this.panelTop = new System.Windows.Forms.Panel();
      this.cbFocus = new System.Windows.Forms.CheckBox();
      this.linkAbout = new System.Windows.Forms.LinkLabel();
      this.cbAdvanced = new System.Windows.Forms.CheckBox();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.lblPassword = new System.Windows.Forms.Label();
      this.txtEmail = new System.Windows.Forms.TextBox();
      this.lblEmail = new System.Windows.Forms.Label();
      this.btnQuit = new System.Windows.Forms.Button();
      this.btnStartFocus = new System.Windows.Forms.Button();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.panelFocus = new System.Windows.Forms.Panel();
      this.grpFocus = new System.Windows.Forms.GroupBox();
      this.panelAdvanced.SuspendLayout();
      this.grpAdvanced.SuspendLayout();
      this.panelTop.SuspendLayout();
      this.panelFocus.SuspendLayout();
      this.grpFocus.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(134, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "File path to Launcher.exe:";
      // 
      // txtLauncherExe
      // 
      this.txtLauncherExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtLauncherExe.Location = new System.Drawing.Point(10, 37);
      this.txtLauncherExe.Name = "txtLauncherExe";
      this.txtLauncherExe.Size = new System.Drawing.Size(357, 21);
      this.txtLauncherExe.TabIndex = 1;
      // 
      // btnLauncherExe
      // 
      this.btnLauncherExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLauncherExe.Location = new System.Drawing.Point(376, 37);
      this.btnLauncherExe.Name = "btnLauncherExe";
      this.btnLauncherExe.Size = new System.Drawing.Size(23, 21);
      this.btnLauncherExe.TabIndex = 2;
      this.btnLauncherExe.Text = "…";
      this.btnLauncherExe.UseVisualStyleBackColor = true;
      this.btnLauncherExe.Click += new System.EventHandler(this.btnLauncherExe_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.DefaultExt = "exe";
      this.openFileDialog1.FileName = "openFileDialog1";
      this.openFileDialog1.Filter = "EXE Files|*.exe";
      this.openFileDialog1.RestoreDirectory = true;
      // 
      // timer1
      // 
      this.timer1.Interval = 500;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // lblRealm
      // 
      this.lblRealm.AutoSize = true;
      this.lblRealm.Location = new System.Drawing.Point(10, 28);
      this.lblRealm.Name = "lblRealm";
      this.lblRealm.Size = new System.Drawing.Size(40, 13);
      this.lblRealm.TabIndex = 3;
      this.lblRealm.Text = "Realm:";
      // 
      // btnInstallHook
      // 
      this.btnInstallHook.Location = new System.Drawing.Point(10, 64);
      this.btnInstallHook.Name = "btnInstallHook";
      this.btnInstallHook.Size = new System.Drawing.Size(172, 23);
      this.btnInstallHook.TabIndex = 5;
      this.btnInstallHook.Text = "Re-install hook.js";
      this.btnInstallHook.UseVisualStyleBackColor = true;
      this.btnInstallHook.Click += new System.EventHandler(this.btnInstallHook_Click);
      // 
      // panelAdvanced
      // 
      this.panelAdvanced.Controls.Add(this.grpAdvanced);
      this.panelAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelAdvanced.Location = new System.Drawing.Point(0, 254);
      this.panelAdvanced.Name = "panelAdvanced";
      this.panelAdvanced.Size = new System.Drawing.Size(414, 304);
      this.panelAdvanced.TabIndex = 14;
      // 
      // grpAdvanced
      // 
      this.grpAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAdvanced.Controls.Add(this.txtLauncherExe);
      this.grpAdvanced.Controls.Add(this.txtLog);
      this.grpAdvanced.Controls.Add(this.label1);
      this.grpAdvanced.Controls.Add(this.label2);
      this.grpAdvanced.Controls.Add(this.cbDisableScripts);
      this.grpAdvanced.Controls.Add(this.btnLauncherExe);
      this.grpAdvanced.Controls.Add(this.btnInstallHook);
      this.grpAdvanced.Location = new System.Drawing.Point(3, 17);
      this.grpAdvanced.Name = "grpAdvanced";
      this.grpAdvanced.Size = new System.Drawing.Size(407, 281);
      this.grpAdvanced.TabIndex = 23;
      this.grpAdvanced.TabStop = false;
      this.grpAdvanced.Text = "Advanced";
      // 
      // cbDisableScripts
      // 
      this.cbDisableScripts.AutoSize = true;
      this.cbDisableScripts.Location = new System.Drawing.Point(227, 68);
      this.cbDisableScripts.Name = "cbDisableScripts";
      this.cbDisableScripts.Size = new System.Drawing.Size(114, 17);
      this.cbDisableScripts.TabIndex = 22;
      this.cbDisableScripts.Text = "disable userscripts";
      this.cbDisableScripts.UseVisualStyleBackColor = true;
      this.cbDisableScripts.CheckedChanged += new System.EventHandler(this.cbDisableScripts_CheckedChanged);
      // 
      // linkFocusForum
      // 
      this.linkFocusForum.LinkArea = new System.Windows.Forms.LinkArea(0, 37);
      this.linkFocusForum.Location = new System.Drawing.Point(227, 58);
      this.linkFocusForum.Name = "linkFocusForum";
      this.linkFocusForum.Size = new System.Drawing.Size(111, 18);
      this.linkFocusForum.TabIndex = 23;
      this.linkFocusForum.TabStop = true;
      this.linkFocusForum.Text = "Open Focus Forum ";
      this.linkFocusForum.UseCompatibleTextRendering = true;
      this.linkFocusForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkFocusForum_LinkClicked);
      // 
      // linkFocusLogin
      // 
      this.linkFocusLogin.LinkArea = new System.Windows.Forms.LinkArea(0, 40);
      this.linkFocusLogin.Location = new System.Drawing.Point(13, 58);
      this.linkFocusLogin.Name = "linkFocusLogin";
      this.linkFocusLogin.Size = new System.Drawing.Size(136, 13);
      this.linkFocusLogin.TabIndex = 22;
      this.linkFocusLogin.TabStop = true;
      this.linkFocusLogin.Text = "Login to Focus website";
      this.linkFocusLogin.UseCompatibleTextRendering = true;
      this.linkFocusLogin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkFocusLogin_LinkClicked);
      // 
      // comboRealm
      // 
      this.comboRealm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboRealm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboRealm.FormattingEnabled = true;
      this.comboRealm.Location = new System.Drawing.Point(59, 25);
      this.comboRealm.Name = "comboRealm";
      this.comboRealm.Size = new System.Drawing.Size(340, 21);
      this.comboRealm.TabIndex = 6;
      // 
      // panelTop
      // 
      this.panelTop.Controls.Add(this.cbFocus);
      this.panelTop.Controls.Add(this.linkAbout);
      this.panelTop.Controls.Add(this.cbAdvanced);
      this.panelTop.Controls.Add(this.txtPassword);
      this.panelTop.Controls.Add(this.lblPassword);
      this.panelTop.Controls.Add(this.txtEmail);
      this.panelTop.Controls.Add(this.lblEmail);
      this.panelTop.Controls.Add(this.btnQuit);
      this.panelTop.Controls.Add(this.btnStartFocus);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(414, 148);
      this.panelTop.TabIndex = 15;
      // 
      // cbFocus
      // 
      this.cbFocus.AutoSize = true;
      this.cbFocus.Checked = true;
      this.cbFocus.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbFocus.Location = new System.Drawing.Point(13, 120);
      this.cbFocus.Name = "cbFocus";
      this.cbFocus.Size = new System.Drawing.Size(111, 17);
      this.cbFocus.TabIndex = 22;
      this.cbFocus.Text = "QL Focus member";
      this.cbFocus.UseVisualStyleBackColor = true;
      this.cbFocus.CheckedChanged += new System.EventHandler(this.cbFocus_CheckedChanged);
      // 
      // linkAbout
      // 
      this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkAbout.AutoSize = true;
      this.linkAbout.LinkArea = new System.Windows.Forms.LinkArea(13, 22);
      this.linkAbout.Location = new System.Drawing.Point(275, 121);
      this.linkAbout.Name = "linkAbout";
      this.linkAbout.Size = new System.Drawing.Size(124, 18);
      this.linkAbout.TabIndex = 21;
      this.linkAbout.TabStop = true;
      this.linkAbout.Text = "developed by PredatH0r";
      this.linkAbout.UseCompatibleTextRendering = true;
      this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
      // 
      // cbAdvanced
      // 
      this.cbAdvanced.AutoSize = true;
      this.cbAdvanced.Checked = true;
      this.cbAdvanced.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAdvanced.Location = new System.Drawing.Point(136, 120);
      this.cbAdvanced.Name = "cbAdvanced";
      this.cbAdvanced.Size = new System.Drawing.Size(73, 17);
      this.cbAdvanced.TabIndex = 20;
      this.cbAdvanced.Text = "advanced";
      this.cbAdvanced.UseVisualStyleBackColor = true;
      this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
      // 
      // txtPassword
      // 
      this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPassword.Location = new System.Drawing.Point(227, 33);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.PasswordChar = '☺';
      this.txtPassword.Size = new System.Drawing.Size(175, 21);
      this.txtPassword.TabIndex = 17;
      this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
      // 
      // lblPassword
      // 
      this.lblPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(227, 17);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(106, 13);
      this.lblPassword.TabIndex = 16;
      this.lblPassword.Text = "Password (optional):";
      // 
      // txtEmail
      // 
      this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtEmail.Location = new System.Drawing.Point(13, 33);
      this.txtEmail.Name = "txtEmail";
      this.txtEmail.Size = new System.Drawing.Size(196, 21);
      this.txtEmail.TabIndex = 15;
      this.txtEmail.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEmail_KeyDown);
      // 
      // lblEmail
      // 
      this.lblEmail.AutoSize = true;
      this.lblEmail.Location = new System.Drawing.Point(13, 17);
      this.lblEmail.Name = "lblEmail";
      this.lblEmail.Size = new System.Drawing.Size(84, 13);
      this.lblEmail.TabIndex = 14;
      this.lblEmail.Text = "Email (optional):";
      // 
      // btnQuit
      // 
      this.btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnQuit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnQuit.Location = new System.Drawing.Point(227, 67);
      this.btnQuit.Name = "btnQuit";
      this.btnQuit.Size = new System.Drawing.Size(175, 37);
      this.btnQuit.TabIndex = 19;
      this.btnQuit.Text = "Exit";
      this.btnQuit.UseVisualStyleBackColor = true;
      this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
      // 
      // btnStartFocus
      // 
      this.btnStartFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStartFocus.Location = new System.Drawing.Point(13, 67);
      this.btnStartFocus.Name = "btnStartFocus";
      this.btnStartFocus.Size = new System.Drawing.Size(196, 37);
      this.btnStartFocus.TabIndex = 18;
      this.btnStartFocus.Text = "Start Quake Live Launcher";
      this.btnStartFocus.UseVisualStyleBackColor = true;
      this.btnStartFocus.Click += new System.EventHandler(this.btnStartLauncher_Click);
      // 
      // txtLog
      // 
      this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtLog.Location = new System.Drawing.Point(10, 125);
      this.txtLog.Multiline = true;
      this.txtLog.Name = "txtLog";
      this.txtLog.ReadOnly = true;
      this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtLog.Size = new System.Drawing.Size(389, 150);
      this.txtLog.TabIndex = 21;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 109);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(28, 13);
      this.label2.TabIndex = 20;
      this.label2.Text = "Log:";
      // 
      // panelFocus
      // 
      this.panelFocus.Controls.Add(this.grpFocus);
      this.panelFocus.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelFocus.Location = new System.Drawing.Point(0, 148);
      this.panelFocus.Name = "panelFocus";
      this.panelFocus.Size = new System.Drawing.Size(414, 106);
      this.panelFocus.TabIndex = 17;
      // 
      // grpFocus
      // 
      this.grpFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpFocus.Controls.Add(this.comboRealm);
      this.grpFocus.Controls.Add(this.lblRealm);
      this.grpFocus.Controls.Add(this.linkFocusForum);
      this.grpFocus.Controls.Add(this.linkFocusLogin);
      this.grpFocus.Location = new System.Drawing.Point(3, 18);
      this.grpFocus.Name = "grpFocus";
      this.grpFocus.Size = new System.Drawing.Size(408, 81);
      this.grpFocus.TabIndex = 0;
      this.grpFocus.TabStop = false;
      this.grpFocus.Text = "QL Focus members (beta testers)";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(414, 558);
      this.Controls.Add(this.panelAdvanced);
      this.Controls.Add(this.panelFocus);
      this.Controls.Add(this.panelTop);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimumSize = new System.Drawing.Size(430, 192);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "extraQL";
      this.panelAdvanced.ResumeLayout(false);
      this.grpAdvanced.ResumeLayout(false);
      this.grpAdvanced.PerformLayout();
      this.panelTop.ResumeLayout(false);
      this.panelTop.PerformLayout();
      this.panelFocus.ResumeLayout(false);
      this.grpFocus.ResumeLayout(false);
      this.grpFocus.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtLauncherExe;
    private System.Windows.Forms.Button btnLauncherExe;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.Label lblRealm;
    private System.Windows.Forms.Button btnInstallHook;
    private System.Windows.Forms.Panel panelAdvanced;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.CheckBox cbAdvanced;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtEmail;
    private System.Windows.Forms.Label lblEmail;
    private System.Windows.Forms.Button btnQuit;
    private System.Windows.Forms.Button btnStartFocus;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox comboRealm;
    private System.Windows.Forms.LinkLabel linkAbout;
    private System.Windows.Forms.LinkLabel linkFocusLogin;
    private System.Windows.Forms.LinkLabel linkFocusForum;
    private System.Windows.Forms.CheckBox cbDisableScripts;
    private System.Windows.Forms.GroupBox grpAdvanced;
    private System.Windows.Forms.CheckBox cbFocus;
    private System.Windows.Forms.Panel panelFocus;
    private System.Windows.Forms.GroupBox grpFocus;
  }
}

