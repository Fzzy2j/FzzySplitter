namespace FzzyTools.UI.Components
{
    partial class FzzySettings
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
            this.tasTools = new System.Windows.Forms.CheckBox();
            this.treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmiExpandTree = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiCollapseTree = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiCollapseTreeToSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmiExpandBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiCollapseBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmiCheckBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiUncheckBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiResetBranchToDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cmiResetSettingToDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextMenu2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmiExpandTree2 = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiCollapseTree2 = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiCollapseTreeToSelection2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.resetSettingToDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installMenuModButton = new System.Windows.Forms.Button();
            this.installMenuModProgress = new System.Windows.Forms.ProgressBar();
            this.autoLoadNCS = new System.Windows.Forms.CheckBox();
            this.speedmod = new System.Windows.Forms.CheckBox();
            this.btSave = new System.Windows.Forms.CheckBox();
            this.uninstallMenuModButton = new System.Windows.Forms.Button();
            this.settingsTree = new FzzyTreeView();
            this.treeContextMenu.SuspendLayout();
            this.treeContextMenu2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tasTools
            // 
            this.tasTools.AutoSize = true;
            this.tasTools.Location = new System.Drawing.Point(10, 19);
            this.tasTools.Name = "tasTools";
            this.tasTools.Size = new System.Drawing.Size(112, 17);
            this.tasTools.TabIndex = 1;
            this.tasTools.Text = "Enable TAS Tools";
            this.tasTools.UseVisualStyleBackColor = true;
            // 
            // treeContextMenu
            // 
            this.treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiExpandTree,
            this.cmiCollapseTree,
            this.cmiCollapseTreeToSelection,
            this.toolStripSeparator1,
            this.cmiExpandBranch,
            this.cmiCollapseBranch,
            this.toolStripSeparator2,
            this.cmiCheckBranch,
            this.cmiUncheckBranch,
            this.cmiResetBranchToDefault,
            this.toolStripSeparator3,
            this.cmiResetSettingToDefault});
            this.treeContextMenu.Name = "treeContextMenu";
            this.treeContextMenu.Size = new System.Drawing.Size(209, 220);
            // 
            // cmiExpandTree
            // 
            this.cmiExpandTree.Name = "cmiExpandTree";
            this.cmiExpandTree.Size = new System.Drawing.Size(208, 22);
            this.cmiExpandTree.Text = "Expand Tree";
            this.cmiExpandTree.Click += new System.EventHandler(this.cmiExpandTree_Click);
            // 
            // cmiCollapseTree
            // 
            this.cmiCollapseTree.Name = "cmiCollapseTree";
            this.cmiCollapseTree.Size = new System.Drawing.Size(208, 22);
            this.cmiCollapseTree.Text = "Collapse Tree";
            this.cmiCollapseTree.Click += new System.EventHandler(this.cmiCollapseTree_Click);
            // 
            // cmiCollapseTreeToSelection
            // 
            this.cmiCollapseTreeToSelection.Name = "cmiCollapseTreeToSelection";
            this.cmiCollapseTreeToSelection.Size = new System.Drawing.Size(208, 22);
            this.cmiCollapseTreeToSelection.Text = "Collapse Tree to Selection";
            this.cmiCollapseTreeToSelection.Click += new System.EventHandler(this.cmiCollapseTreeToSelection_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(205, 6);
            // 
            // cmiExpandBranch
            // 
            this.cmiExpandBranch.Name = "cmiExpandBranch";
            this.cmiExpandBranch.Size = new System.Drawing.Size(208, 22);
            this.cmiExpandBranch.Text = "Expand Branch";
            this.cmiExpandBranch.Click += new System.EventHandler(this.cmiExpandBranch_Click);
            // 
            // cmiCollapseBranch
            // 
            this.cmiCollapseBranch.Name = "cmiCollapseBranch";
            this.cmiCollapseBranch.Size = new System.Drawing.Size(208, 22);
            this.cmiCollapseBranch.Text = "Collapse Branch";
            this.cmiCollapseBranch.Click += new System.EventHandler(this.cmiCollapseBranch_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(205, 6);
            // 
            // cmiCheckBranch
            // 
            this.cmiCheckBranch.Name = "cmiCheckBranch";
            this.cmiCheckBranch.Size = new System.Drawing.Size(208, 22);
            this.cmiCheckBranch.Text = "Check Branch";
            this.cmiCheckBranch.Click += new System.EventHandler(this.cmiCheckBranch_Click);
            // 
            // cmiUncheckBranch
            // 
            this.cmiUncheckBranch.Name = "cmiUncheckBranch";
            this.cmiUncheckBranch.Size = new System.Drawing.Size(208, 22);
            this.cmiUncheckBranch.Text = "Uncheck Branch";
            this.cmiUncheckBranch.Click += new System.EventHandler(this.cmiUncheckBranch_Click);
            // 
            // cmiResetBranchToDefault
            // 
            this.cmiResetBranchToDefault.Name = "cmiResetBranchToDefault";
            this.cmiResetBranchToDefault.Size = new System.Drawing.Size(208, 22);
            this.cmiResetBranchToDefault.Text = "Reset Branch to Default";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(205, 6);
            // 
            // cmiResetSettingToDefault
            // 
            this.cmiResetSettingToDefault.Name = "cmiResetSettingToDefault";
            this.cmiResetSettingToDefault.Size = new System.Drawing.Size(208, 22);
            this.cmiResetSettingToDefault.Text = "Reset Setting to Default";
            // 
            // treeContextMenu2
            // 
            this.treeContextMenu2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiExpandTree2,
            this.cmiCollapseTree2,
            this.cmiCollapseTreeToSelection2,
            this.toolStripSeparator4,
            this.resetSettingToDefaultToolStripMenuItem});
            this.treeContextMenu2.Name = "treeContextMenu";
            this.treeContextMenu2.Size = new System.Drawing.Size(209, 98);
            // 
            // cmiExpandTree2
            // 
            this.cmiExpandTree2.Name = "cmiExpandTree2";
            this.cmiExpandTree2.Size = new System.Drawing.Size(208, 22);
            this.cmiExpandTree2.Text = "Expand Tree";
            this.cmiExpandTree2.Click += new System.EventHandler(this.cmiExpandTree_Click);
            // 
            // cmiCollapseTree2
            // 
            this.cmiCollapseTree2.Name = "cmiCollapseTree2";
            this.cmiCollapseTree2.Size = new System.Drawing.Size(208, 22);
            this.cmiCollapseTree2.Text = "Collapse Tree";
            this.cmiCollapseTree2.Click += new System.EventHandler(this.cmiCollapseTree_Click);
            // 
            // cmiCollapseTreeToSelection2
            // 
            this.cmiCollapseTreeToSelection2.Name = "cmiCollapseTreeToSelection2";
            this.cmiCollapseTreeToSelection2.Size = new System.Drawing.Size(208, 22);
            this.cmiCollapseTreeToSelection2.Text = "Collapse Tree to Selection";
            this.cmiCollapseTreeToSelection2.Click += new System.EventHandler(this.cmiCollapseTreeToSelection_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(205, 6);
            // 
            // resetSettingToDefaultToolStripMenuItem
            // 
            this.resetSettingToDefaultToolStripMenuItem.Name = "resetSettingToDefaultToolStripMenuItem";
            this.resetSettingToDefaultToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.resetSettingToDefaultToolStripMenuItem.Text = "Reset Setting to Default";
            // 
            // installMenuModButton
            // 
            this.installMenuModButton.Location = new System.Drawing.Point(334, 10);
            this.installMenuModButton.Name = "installMenuModButton";
            this.installMenuModButton.Size = new System.Drawing.Size(121, 23);
            this.installMenuModButton.TabIndex = 18;
            this.installMenuModButton.Text = "Install Menu Mod";
            this.installMenuModButton.UseVisualStyleBackColor = true;
            this.installMenuModButton.Click += new System.EventHandler(this.installMenuModButton_Click);
            // 
            // installMenuModProgress
            // 
            this.installMenuModProgress.Location = new System.Drawing.Point(211, 10);
            this.installMenuModProgress.Name = "installMenuModProgress";
            this.installMenuModProgress.Size = new System.Drawing.Size(117, 23);
            this.installMenuModProgress.TabIndex = 19;
            this.installMenuModProgress.Visible = false;
            // 
            // autoLoadNCS
            // 
            this.autoLoadNCS.AutoSize = true;
            this.autoLoadNCS.Location = new System.Drawing.Point(10, 43);
            this.autoLoadNCS.Name = "autoLoadNCS";
            this.autoLoadNCS.Size = new System.Drawing.Size(100, 17);
            this.autoLoadNCS.TabIndex = 20;
            this.autoLoadNCS.Text = "Auto Load NCS";
            this.autoLoadNCS.UseVisualStyleBackColor = true;
            // 
            // speedmod
            // 
            this.speedmod.AutoSize = true;
            this.speedmod.Location = new System.Drawing.Point(128, 19);
            this.speedmod.Name = "speedmod";
            this.speedmod.Size = new System.Drawing.Size(77, 17);
            this.speedmod.TabIndex = 21;
            this.speedmod.Text = "Speedmod";
            this.speedmod.UseVisualStyleBackColor = true;
            this.speedmod.CheckedChanged += new System.EventHandler(this.speedmod_CheckedChanged);
            // 
            // btSave
            // 
            this.btSave.AutoSize = true;
            this.btSave.Location = new System.Drawing.Point(112, 43);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(144, 17);
            this.btSave.TabIndex = 22;
            this.btSave.Text = "Auto Load 18 Hour Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.CheckedChanged += new System.EventHandler(this.btSave_CheckedChanged);
            // 
            // uninstallMenuModButton
            // 
            this.uninstallMenuModButton.Location = new System.Drawing.Point(334, 40);
            this.uninstallMenuModButton.Name = "uninstallMenuModButton";
            this.uninstallMenuModButton.Size = new System.Drawing.Size(121, 23);
            this.uninstallMenuModButton.TabIndex = 23;
            this.uninstallMenuModButton.Text = "Uninstall Menu Mod";
            this.uninstallMenuModButton.UseVisualStyleBackColor = true;
            this.uninstallMenuModButton.Click += new System.EventHandler(this.uninstallMenuModButton_Click);
            // 
            // settingsTree
            // 
            this.settingsTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsTree.CheckBoxes = true;
            this.settingsTree.Location = new System.Drawing.Point(10, 69);
            this.settingsTree.Name = "settingsTree";
            this.settingsTree.ShowNodeToolTips = true;
            this.settingsTree.Size = new System.Drawing.Size(445, 411);
            this.settingsTree.TabIndex = 15;
            this.settingsTree.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.settingsTree_BeforeCheck);
            this.settingsTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.settingsTree_AfterCheck);
            this.settingsTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.settingsTree_NodeMouseClick);
            // 
            // FzzySettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uninstallMenuModButton);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.speedmod);
            this.Controls.Add(this.autoLoadNCS);
            this.Controls.Add(this.installMenuModProgress);
            this.Controls.Add(this.installMenuModButton);
            this.Controls.Add(this.settingsTree);
            this.Controls.Add(this.tasTools);
            this.Name = "FzzySettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(465, 490);
            this.Load += new System.EventHandler(this.FzzySettings_Load);
            this.treeContextMenu.ResumeLayout(false);
            this.treeContextMenu2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox tasTools;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cmiExpandTree;
        private System.Windows.Forms.ToolStripMenuItem cmiCollapseTree;
        private System.Windows.Forms.ToolStripMenuItem cmiCollapseTreeToSelection;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem cmiExpandBranch;
        private System.Windows.Forms.ToolStripMenuItem cmiCollapseBranch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem cmiCheckBranch;
        private System.Windows.Forms.ToolStripMenuItem cmiUncheckBranch;
        private System.Windows.Forms.ToolStripMenuItem cmiResetBranchToDefault;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cmiResetSettingToDefault;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu2;
        private System.Windows.Forms.ToolStripMenuItem cmiExpandTree2;
        private System.Windows.Forms.ToolStripMenuItem cmiCollapseTree2;
        private System.Windows.Forms.ToolStripMenuItem cmiCollapseTreeToSelection2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem resetSettingToDefaultToolStripMenuItem;
        private FzzyTreeView settingsTree;
        private System.Windows.Forms.Button installMenuModButton;
        private System.Windows.Forms.ProgressBar installMenuModProgress;
        private System.Windows.Forms.CheckBox autoLoadNCS;
        private System.Windows.Forms.CheckBox speedmod;
        private System.Windows.Forms.CheckBox btSave;
        private System.Windows.Forms.Button uninstallMenuModButton;
    }
}
