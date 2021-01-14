using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using LiveSplit.ASL;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using FzzyTools.UI.Components;
using LiveSplit.Options;
using LiveSplit.ComponentUtil;

namespace LiveSplit.UI.Components
{
    public partial class FzzySettings : UserControl
    {
        public bool TASToolsEnabled { get; set; }
        public bool AutoLoadNCS { get; set; }
        public bool Speedmod { get; set; }
        public bool TASAimbot { get; set; }

        private Dictionary<string, bool> _state;
        public ASLSettings aslsettings;

        public FzzySettings()
        {
            InitializeComponent();

            TASToolsEnabled = false;
            AutoLoadNCS = false;
            Speedmod = false;
            TASAimbot = false;

            _state = new Dictionary<string, bool>();
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;

            if (!element.IsEmpty)
            {
                TASToolsEnabled = SettingsHelper.ParseBool(element["tasToolsEnabled"]);
                AutoLoadNCS = SettingsHelper.ParseBool(element["autoLoadNCS"]);
                Speedmod = SettingsHelper.ParseBool(element["speedmod"]);
                TASAimbot = SettingsHelper.ParseBool(element["tasAimbot"]);

                ParseSettingsFromXml(element);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var node = document.CreateElement("Settings");

            AppendSettingsToXml(document, node);
            SettingsHelper.CreateSetting(document, node, "tasToolsEnabled", TASToolsEnabled);
            SettingsHelper.CreateSetting(document, node, "autoLoadNCS", AutoLoadNCS);
            SettingsHelper.CreateSetting(document, node, "speedmod", Speedmod);
            SettingsHelper.CreateSetting(document, node, "tasAimbot", TASAimbot);

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

            foreach (ASLSetting setting in settings.OrderedSettings)
            {
                var value = setting.Value;
                if (_state.ContainsKey(setting.Id))
                    value = _state[setting.Id];

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
            foreach (var item in flat)
            {
                if (!item.Value.Checked)
                {
                    UpdateGrayedOut(item.Value);
                }
            }

            // Only if a script was actually loaded, update current state with current ASL settings
            // (which may be empty if the successfully loaded script has no settings, but shouldn't
            // be empty because the script failed to load, which can happen frequently when working
            // on ASL scripts)
            _state = values;

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
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void AppendSettingsToXml(XmlDocument document, XmlNode parent)
        {
            XmlElement asl_parent = document.CreateElement("CustomSettings");

            foreach (var setting in _state)
            {
                XmlElement element = SettingsHelper.ToElement(document, "Setting", setting.Value);
                XmlAttribute id = SettingsHelper.ToAttribute(document, "id", setting.Key);
                // In case there are other setting types in the future
                XmlAttribute type = SettingsHelper.ToAttribute(document, "type", "bool");

                element.Attributes.Append(id);
                element.Attributes.Append(type);
                asl_parent.AppendChild(element);
            }

            parent.AppendChild(asl_parent);
        }
        private void ParseSettingsFromXml(XmlElement data)
        {
            XmlElement custom_settings_node = data["CustomSettings"];

            if (custom_settings_node != null && custom_settings_node.HasChildNodes)
            {
                foreach (XmlElement element in custom_settings_node.ChildNodes)
                {
                    if (element.Name != "Setting")
                        continue;

                    string id = element.Attributes["id"].Value;
                    string type = element.Attributes["type"].Value;

                    if (id != null && type == "bool")
                    {
                        bool value = SettingsHelper.ParseBool(element);
                        _state[id] = value;
                    }
                }
            }

            // Update tree with loaded state (in case the tree is already populated)
            UpdateNodesCheckedState(_state);
        }
        private void UpdateNodesCheckedState(Dictionary<string, bool> setting_values, TreeNodeCollection nodes = null)
        {
            if (setting_values == null)
                return;

            UpdateNodesCheckedState(setting =>
            {
                string id = setting.Id;

                if (setting_values.ContainsKey(id))
                    return setting_values[id];

                return setting.Value;
            }, nodes);
        }
        private void UpdateNodesCheckedState(Func<ASLSetting, bool> func, TreeNodeCollection nodes = null)
        {
            if (nodes == null)
                nodes = this.settingsTree.Nodes;

            UpdateNodesInTree(node =>
            {
                var setting = (ASLSetting)node.Tag;
                bool check = func(setting);

                if (node.Checked != check)
                    node.Checked = check;

                return true;
            }, nodes);
        }
        private void UpdateNodesInTree(Func<TreeNode, bool> func, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                bool include_child_nodes = func(node);
                if (include_child_nodes)
                    UpdateNodesInTree(func, node.Nodes);
            }
        }

        private void UpdateNodeCheckedState(Func<ASLSetting, bool> func, TreeNode node)
        {
            var setting = (ASLSetting)node.Tag;
            bool check = func(setting);

            if (node.Checked != check)
                node.Checked = check;
        }

        private void FzzySettings_Load(object sender, EventArgs e)
        {
            tasTools.DataBindings.Clear();
            autoLoadNCS.DataBindings.Clear();
            speedmod.DataBindings.Clear();
            tasAimbot.DataBindings.Clear();

            tasTools.DataBindings.Add("Checked", this, "TASToolsEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
            autoLoadNCS.DataBindings.Add("Checked", this, "AutoLoadNCS", false, DataSourceUpdateMode.OnPropertyChanged);
            speedmod.DataBindings.Add("Checked", this, "Speedmod", false, DataSourceUpdateMode.OnPropertyChanged);
            tasAimbot.DataBindings.Add("Checked", this, "TASAimbot", false, DataSourceUpdateMode.OnPropertyChanged);
        }
        // Custom Setting checked/unchecked (only after initially building the tree)
        private void settingsTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Update value in the ASLSetting object, which also changes it in the ASL script
            ASLSetting setting = (ASLSetting)e.Node.Tag;
            setting.Value = e.Node.Checked;
            _state[setting.Id] = setting.Value;

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

            string titanfallInstallDirectory = FzzyComponent.GetTitanfallInstallDirectory();

            if (titanfallInstallDirectory.Length == 0)
            {
                MessageBox.Show("Couldn't find Titanfall 2 install location!");
                return;
            }

            string settingscfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\local\\settings.cfg");
            string settingscontent = File.ReadAllText(settingscfg);
            for (int i = 1; i <= 9; i++)
            {
                if (settingscontent.Contains("\"load fastany" + i + "\"")) continue;
                File.AppendAllText(settingscfg, "\nbind \"F" + i + "\" \"load fastany" + i + "\"");
            }
            File.AppendAllText(settingscfg, "\nbind \"F12\" \"exec autosplitter.cfg\"");

            using (WebClient webClient = new WebClient())
            {
                installMenuModButton.Enabled = false;
                menumod = Path.Combine(titanfallInstallDirectory, "vpk\\installmenumod.exe");
                webClient.DownloadFileAsync(new Uri(FzzyComponent.MENU_MOD_INSTALLER_LINK), menumod);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(MenuModDownloadCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(MenuModProgressChanged);
                installMenuModProgress.Visible = true;
            }

            InstallFastanySaves();
        }

        private static string fastanySavesInstaller = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\profile\\savegames\\installsaves.exe");
        private static string speedmodSavesInstaller = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\profile\\savegames\\installspeedmodsaves.exe");

        public static bool AreFastanySavesInstalled()
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

        public static bool AreSavesInstalled(string[] saves)
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
        public static void InstallFastanySaves()
        {
            if (AreFastanySavesInstalled()) return;
            using (WebClient webClient = new WebClient())
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
        public static void InstallSpeedmodSaves()
        {
            if (AreSpeedmodSavesInstalled()) return;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileAsync(new Uri(FzzyComponent.SPEEDMOD_SAVES_INSTALLER_LINK), speedmodSavesInstaller);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(SpeedmodSavesDownloadCompleted);
            }
        }

        private static void SpeedmodSavesDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetParent(speedmodSavesInstaller).FullName,
                FileName = speedmodSavesInstaller
            };
            Process.Start(startInfo);
        }

        private void MenuModProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            installMenuModProgress.Value = e.ProgressPercentage;
        }

        private void MenuModDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            installMenuModProgress.Visible = false;
            installMenuModButton.Enabled = true;
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetParent(menumod).FullName,
                FileName = menumod
            };
            Process.Start(startInfo);
        }

        private void speedmod_CheckedChanged(object sender, EventArgs e)
        {
            InstallSpeedmodSaves();
        }
    }
    class FzzyTreeView : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203) // identified double click
            {
                var local_pos = PointToClient(Cursor.Position);
                var hit_test_info = HitTest(local_pos);

                if (hit_test_info.Location == TreeViewHitTestLocations.StateImage)
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
