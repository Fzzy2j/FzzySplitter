using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.ASL;
using LiveSplit.UI;

namespace FzzyTools.UI.Components
{
    public partial class FzzySettings : UserControl
    {
        //public CompositeHook Hook { get; set; }
        public bool TasToolsEnabled { get; set; }
        public bool AutoLoadNcs { get; set; }

        public bool SpeedmodEnabled { get; set; }

        //public bool TASAimbot { get; set; }
        public bool AutoLoad18HourSave { get; set; }

        private Dictionary<string, bool> state;
        public ASLSettings aslsettings;

        public FzzySettings()
        {
            InitializeComponent();

            //Hook = new CompositeHook();

            TasToolsEnabled = false;
            AutoLoadNcs = false;
            SpeedmodEnabled = false;
            //TASAimbot = false;
            AutoLoad18HourSave = false;

            state = new Dictionary<string, bool>();
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement) node;

            if (!element.IsEmpty)
            {
                TasToolsEnabled = SettingsHelper.ParseBool(element["tasToolsEnabled"]);
                AutoLoadNcs = SettingsHelper.ParseBool(element["autoLoadNCS"]);
                SpeedmodEnabled = SettingsHelper.ParseBool(element["speedmod"]);
                AutoLoad18HourSave = SettingsHelper.ParseBool(element["btSave"]);
                //TASAimbot = SettingsHelper.ParseBool(element["tasAimbot"]);

                ParseSettingsFromXml(element);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var node = document.CreateElement("Settings");

            AppendSettingsToXml(document, node);
            SettingsHelper.CreateSetting(document, node, "tasToolsEnabled", TasToolsEnabled);
            SettingsHelper.CreateSetting(document, node, "autoLoadNCS", AutoLoadNcs);
            SettingsHelper.CreateSetting(document, node, "speedmod", SpeedmodEnabled);
            SettingsHelper.CreateSetting(document, node, "btSave", AutoLoad18HourSave);
            //SettingsHelper.CreateSetting(document, node, "tasAimbot", TASAimbot);

            return node;
        }

        public void InitASLSettings(ASLSettings settings)
        {
            this.aslsettings = settings;

            this.settingsTree.BeginUpdate();
            this.settingsTree.Nodes.Clear();

            var values = new Dictionary<string, bool>();

            // Store temporary for easier lookup of parent nodes
            var flat = new Dictionary<string, TreeNode>();

            foreach (var setting in settings.OrderedSettings)
            {
                var value = setting.Value;
                if (state.ContainsKey(setting.Id))
                    value = state[setting.Id];

                var node = new TreeNode(setting.Label)
                {
                    Tag = setting,
                    Checked = value,
                    ContextMenuStrip = this.treeContextMenu2,
                    ToolTipText = setting.ToolTip
                };
                setting.Value = value;

                if (setting.Parent == null)
                {
                    this.settingsTree.Nodes.Add(node);
                }
                else if (flat.ContainsKey(setting.Parent))
                {
                    flat[setting.Parent].Nodes.Add(node);
                    flat[setting.Parent].ContextMenuStrip = this.treeContextMenu;
                }

                flat.Add(setting.Id, node);
                values.Add(setting.Id, value);
            }

            // Gray out deactivated nodes after all have been added
            foreach (var item in flat.Where(item => !item.Value.Checked))
            {
                UpdateGrayedOut(item.Value);
            }

            // Only if a script was actually loaded, update current state with current ASL settings
            // (which may be empty if the successfully loaded script has no settings, but shouldn't
            // be empty because the script failed to load, which can happen frequently when working
            // on ASL scripts)
            state = values;

            settingsTree.ExpandAll();
            settingsTree.EndUpdate();

            // Scroll up to the top
            if (this.settingsTree.Nodes.Count > 0)
                this.settingsTree.Nodes[0].EnsureVisible();

            UpdateCustomSettingsVisibility();
        }

        private void UpdateCustomSettingsVisibility()
        {
            bool show = this.settingsTree.GetNodeCount(false) > 0;
            this.settingsTree.Visible = show;
        }

        private void UpdateGrayedOut(TreeNode node)
        {
            // Only change color of childnodes if this node isn't already grayed out
            if (node.ForeColor != SystemColors.GrayText)
            {
                UpdateNodesInTree(n =>
                {
                    n.ForeColor = node.Checked ? SystemColors.WindowText : SystemColors.GrayText;
                    return n.Checked || !node.Checked;
                }, node.Nodes);
            }
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button) sender, this);
        }

        private void AppendSettingsToXml(XmlDocument document, XmlNode parent)
        {
            var aslParent = document.CreateElement("CustomSettings");

            foreach (var setting in state)
            {
                var element = SettingsHelper.ToElement(document, "Setting", setting.Value);
                var id = SettingsHelper.ToAttribute(document, "id", setting.Key);
                // In case there are other setting types in the future
                var type = SettingsHelper.ToAttribute(document, "type", "bool");

                element.Attributes.Append(id);
                element.Attributes.Append(type);
                aslParent.AppendChild(element);
            }

            parent.AppendChild(aslParent);
        }

        private void ParseSettingsFromXml(XmlElement data)
        {
            var custom_settings_node = data["CustomSettings"];

            if (custom_settings_node != null && custom_settings_node.HasChildNodes)
            {
                foreach (XmlElement element in custom_settings_node.ChildNodes)
                {
                    if (element.Name != "Setting")
                        continue;

                    var id = element.Attributes["id"].Value;
                    var type = element.Attributes["type"].Value;

                    if (id == null || type != "bool") continue;
                    var value = SettingsHelper.ParseBool(element);
                    state[id] = value;
                }
            }

            // Update tree with loaded state (in case the tree is already populated)
            UpdateNodesCheckedState(state);
        }

        private void UpdateNodesCheckedState(IReadOnlyDictionary<string, bool> settingValues,
            TreeNodeCollection nodes = null)
        {
            if (settingValues == null)
                return;

            UpdateNodesCheckedState(setting =>
            {
                var id = setting.Id;

                return settingValues.ContainsKey(id) ? settingValues[id] : setting.Value;
            }, nodes);
        }

        private void UpdateNodesCheckedState(Func<ASLSetting, bool> func, TreeNodeCollection nodes = null)
        {
            if (nodes == null)
                nodes = this.settingsTree.Nodes;

            UpdateNodesInTree(node =>
            {
                var setting = (ASLSetting) node.Tag;
                bool check = func(setting);

                if (node.Checked != check)
                    node.Checked = check;

                return true;
            }, nodes);
        }

        private static void UpdateNodesInTree(Func<TreeNode, bool> func, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                var include_child_nodes = func(node);
                if (include_child_nodes)
                    UpdateNodesInTree(func, node.Nodes);
            }
        }

        private static void UpdateNodeCheckedState(Func<ASLSetting, bool> func, TreeNode node)
        {
            var setting = (ASLSetting) node.Tag;
            bool check = func(setting);

            if (node.Checked != check)
                node.Checked = check;
        }

        private void FzzySettings_Load(object sender, EventArgs e)
        {
            tasTools.DataBindings.Clear();
            autoLoadNCS.DataBindings.Clear();
            speedmod.DataBindings.Clear();
            btSave.DataBindings.Clear();

            tasTools.DataBindings.Add("Checked", this, "TASToolsEnabled", false,
                DataSourceUpdateMode.OnPropertyChanged);
            autoLoadNCS.DataBindings.Add("Checked", this, "AutoLoadNCS", false, DataSourceUpdateMode.OnPropertyChanged);
            speedmod.DataBindings.Add("Checked", this, "SpeedmodEnabled", false,
                DataSourceUpdateMode.OnPropertyChanged);
            btSave.DataBindings.Add("Checked", this, "AutoLoad18HourSave", false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        // Custom Setting checked/unchecked (only after initially building the tree)
        private void settingsTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Update value in the ASLSetting object, which also changes it in the ASL script
            var setting = (ASLSetting) e.Node.Tag;
            setting.Value = e.Node.Checked;
            state[setting.Id] = setting.Value;

            UpdateGrayedOut(e.Node);
        }

        private void settingsTree_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = e.Node.ForeColor == SystemColors.GrayText;
        }


        // Custom Settings Context Menu Events

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Select clicked node (not only with left-click) for use with context menu
            this.settingsTree.SelectedNode = e.Node;
        }

        private void cmiCheckBranch_Click(object sender, EventArgs e)
        {
            UpdateNodesCheckedState(s => true, this.settingsTree.SelectedNode.Nodes);
            UpdateNodeCheckedState(s => true, this.settingsTree.SelectedNode);
        }

        private void cmiUncheckBranch_Click(object sender, EventArgs e)
        {
            UpdateNodesCheckedState(s => false, this.settingsTree.SelectedNode.Nodes);
            UpdateNodeCheckedState(s => false, this.settingsTree.SelectedNode);
        }

        private void cmiResetBranchToDefault_Click(object sender, EventArgs e)
        {
            UpdateNodesCheckedState(s => s.DefaultValue, this.settingsTree.SelectedNode.Nodes);
            UpdateNodeCheckedState(s => s.DefaultValue, this.settingsTree.SelectedNode);
        }

        private void cmiExpandBranch_Click(object sender, EventArgs e)
        {
            this.settingsTree.SelectedNode.ExpandAll();
            this.settingsTree.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseBranch_Click(object sender, EventArgs e)
        {
            this.settingsTree.SelectedNode.Collapse();
            this.settingsTree.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseTreeToSelection_Click(object sender, EventArgs e)
        {
            TreeNode selected = this.settingsTree.SelectedNode;
            this.settingsTree.CollapseAll();
            this.settingsTree.SelectedNode = selected;
            selected.EnsureVisible();
        }

        private void cmiExpandTree_Click(object sender, EventArgs e)
        {
            this.settingsTree.ExpandAll();
            this.settingsTree.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseTree_Click(object sender, EventArgs e)
        {
            this.settingsTree.CollapseAll();
        }

        private void cmiResetSettingToDefault_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(s => s.DefaultValue, this.settingsTree.SelectedNode);
        }

        private string menumod = Path.Combine(Path.GetTempPath(), "menumod.exe");

        private void installMenuModButton_Click(object sender, EventArgs e)
        {
            if (FzzyComponent.process != null)
            {
                MessageBox.Show("Please close Titanfall 2 to install enhanced menu");
                return;
            }

            var titanfallInstallDirectory = FzzyComponent.GetTitanfallInstallDirectory(this);

            if (string.IsNullOrEmpty(titanfallInstallDirectory))
            {
                MessageBox.Show("Couldn't find Titanfall 2 install location!");
                return;
            }

            var settingscfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Respawn\\Titanfall2\\local\\settings.cfg");
            var settingscontent = File.ReadAllText(settingscfg);
            for (var i = 1; i <= 9; i++)
            {
                if (settingscontent.Contains("\"load fastany" + i + "\"")) continue;
                File.AppendAllText(settingscfg, "\nbind \"F" + i + "\" \"load fastany" + i + "\"");
            }

            using (var webClient = new WebClient())
            {
                installMenuModButton.Enabled = false;
                uninstallMenuModButton.Enabled = false;
                install = true;
                menumod = Path.Combine(titanfallInstallDirectory, "vpk\\installmenumod.exe");
                webClient.DownloadFileAsync(new Uri(FzzyComponent.MENU_MOD_INSTALLER_LINK), menumod);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(MenuModDownloadCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(MenuModProgressChanged);
                installMenuModProgress.Visible = true;
            }

            InstallFastanySaves();
        }

        private void uninstallMenuModButton_Click(object sender, EventArgs e)
        {
            if (FzzyComponent.process != null)
            {
                MessageBox.Show("Please close Titanfall 2 to uninstall enhanced menu");
                return;
            }

            var titanfallInstallDirectory = FzzyComponent.GetTitanfallInstallDirectory(this);

            if (string.IsNullOrEmpty(titanfallInstallDirectory))
            {
                MessageBox.Show("Couldn't find Titanfall 2 install location!");
                return;
            }

            using (WebClient webClient = new WebClient())
            {
                installMenuModButton.Enabled = false;
                menumod = Path.Combine(titanfallInstallDirectory, "vpk\\installmenumod.exe");
                install = false;
                webClient.DownloadFileAsync(new Uri(FzzyComponent.MENU_MOD_UNINSTALLER_LINK), menumod);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(MenuModDownloadCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(MenuModProgressChanged);
                installMenuModProgress.Visible = true;
            }
        }

        private static string fastanySavesInstaller =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Respawn\\Titanfall2\\profile\\savegames\\installsaves.exe");

        private static bool AreFastanySavesInstalled()
        {
            var saves = new string[]
            {
                "fastany1",
                "fastany2",
                "fastany3",
                "fastany4",
                "fastany5",
                "fastany6",
                "fastany7",
                "fastany8",
                "fastany9",
                "fastany3SATCHEL",
                "fastany4FRAG",
                "fastany4SATCHEL",
                "fasthelms2",
                "fasthelms5",
            };
            return AreSavesInstalled(saves);
        }

        public static bool AreSpeedmodSavesInstalled()
        {
            var saves = new string[]
            {
                "speedmod1",
                "speedmod2",
                "speedmod3",
                "speedmod4",
                "speedmod5",
                "speedmod6",
                "speedmod7",
                "speedmod8",
                "speedmod9",
                "speedmod10",
                "speedmod11",
                "speedmod12"
            };
            return AreSavesInstalled(saves);
        }

        private static bool AreSavesInstalled(string[] saves)
        {
            var savesDirectory = Directory.GetParent(fastanySavesInstaller);
            var files = Directory.GetFiles(savesDirectory.FullName);
            foreach (var check in saves)
            {
                bool exists = false;
                foreach (var file in files)
                {
                    if (Path.GetFileNameWithoutExtension(file) == check)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists) return false;
            }

            return true;
        }

        private static void InstallFastanySaves()
        {
            if (AreFastanySavesInstalled()) return;
            using (var webClient = new WebClient())
            {
                webClient.DownloadFileAsync(new Uri(FzzyComponent.FASTANY_SAVES_INSTALLER_LINK), fastanySavesInstaller);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(FastanySavesDownloadCompleted);
            }
        }

        private static void FastanySavesDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetParent(fastanySavesInstaller).FullName,
                FileName = fastanySavesInstaller
            };
            Process.Start(startInfo);
        }

        private void MenuModProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            installMenuModProgress.Value = e.ProgressPercentage;
        }

        private bool install = true;

        private void MenuModDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            installMenuModProgress.Visible = false;
            installMenuModButton.Enabled = true;
            uninstallMenuModButton.Enabled = true;
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetParent(menumod).FullName,
                FileName = menumod
            };
            Process.Start(startInfo);
            string word = "uninstalled from";
            if (install) word = "installed to";
            MessageBox.Show("Menu Mod " + word + ":\n" + Directory.GetParent(menumod).FullName);
        }

        private void btSave_CheckedChanged(object sender, EventArgs e)
        {
            if (!btSave.Checked) return;
            if (!AreSavesInstalled(new string[] {"fastany1"}))
            {
                using (WebClient webClient = new WebClient())
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "Respawn\\Titanfall2\\profile\\savegames\\fastany1.sav");
                    webClient.DownloadFileAsync(new Uri(FzzyComponent.FASTANY1_SAVE_LINK), path);
                }
            }

            var settingscfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Respawn\\Titanfall2\\local\\settings.cfg");
            var settingscontent = File.ReadAllText(settingscfg);
            if (settingscontent.Contains("load fastany1")) return;
            if (FzzyComponent.process != null)
            {
                FzzyComponent.AddToSettingsOnClose("load fastany1");
                MessageBox.Show("18 Hour Cutscene Bind Added!\nRestart your game for it to take effect.");
            }
            else
            {
                File.AppendAllText(settingscfg, "\nbind \"F1\" \"load fastany1\"");
            }
        }

        private void speedmod_CheckedChanged(object sender, EventArgs e)
        {
            if (speedmod.Checked) Speedmod.InstallSpeedmod(this);
        }
    }

    class FzzyTreeView : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203) // identified double click
            {
                var localPos = PointToClient(Cursor.Position);
                var hitTestInfo = HitTest(localPos);

                if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                {
                    m.Msg = 0x201; // if checkbox was clicked, turn into single click
                }

                base.WndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}