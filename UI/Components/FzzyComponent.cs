using FzzyTools.UI.Components;
using LiveSplit.ASL;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.Options;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TitanfallAuto_splitter.UI.Components;
using TitanfallAutosplitter.UI.Components;

namespace LiveSplit.UI.Components
{
    class FzzyComponent : LogicComponent
    {

        public const string MENU_MOD_INSTALLER_LINK = "https://github.com/Fzzy2j/FzzySplitter/releases/download/v1.0/Enhanced.Menu.exe";
        public const string SAVES_INSTALLER_LINK = "https://github.com/Fzzy2j/FzzySplitter/releases/download/v1.0/installsaves.exe";

        public FzzySettings Settings { get; set; }

        public Timer updateTimer;

        public LiveSplitState state;
        public TimerModel timer;

        private NCSAutoLoader _ncsAutoLoader;
        private Speedmod _speedmod;

        private FzzySplitter _splitter;

        private AutoStrafer _autoStrafer;
        private ZLurchMacro _zLurchMacro;
        private AutoJumper _autoJumper;

        public readonly Keyboard board = new Keyboard();
        public static Process process;

        public Dictionary<string, MemoryValue> values = new Dictionary<string, MemoryValue>();

        private ASLSettings aslSettings;

        public bool isLoading;
        public bool wasLoading;

        public FzzyComponent(LiveSplitState state)
        {

            Settings = new FzzySettings();
            this.state = state;
            aslSettings = new ASLSettings();
            aslSettings.AddSetting("flagSplit", false, "Start timer on flag pickup, split on flag capture, and pause when not holding flag", null);
            aslSettings.AddSetting("levelChangeSplit", true, "Split on level change", null);
            aslSettings.AddSetting("helmetSplit", false, "Split on helmet pickup", null);
            aslSettings.AddSetting("endSplit", true, "Split at the end of escape (end of run)", null);
            aslSettings.AddSetting("removeLoads", true, "Remove Loads", null);

            aslSettings.AddSetting("subSplits", false, "Subsplits", null);

            aslSettings.AddSetting("btSplits", true, "BT-7274", "subSplits");
            aslSettings.AddSetting("btBattery1", true, "Split on first battery", "btSplits");
            aslSettings.AddSetting("btBattery2", true, "Split on second battery", "btSplits");

            aslSettings.AddSetting("bnrSplits", true, "Blood and Rust", "subSplits");
            aslSettings.AddSetting("bnrButton1", true, "Split on first button", "bnrSplits");
            aslSettings.AddSetting("bnrDoor", true, "Split at door", "bnrSplits");
            aslSettings.AddSetting("bnrButton2", true, "Split on second button", "bnrSplits");
            aslSettings.AddSetting("bnrEmbark", true, "Split on embark BT", "bnrSplits");

            aslSettings.AddSetting("ita3Splits", true, "Into the Abyss 3 Embark", "subSplits");

            aslSettings.AddSetting("enc1Splits", true, "Effect and Cause 1 Cutscene", "subSplits");

            aslSettings.AddSetting("enc2Splits", true, "Effect and Cause 2", "subSplits");
            aslSettings.AddSetting("enc2Button1", true, "Split on button 1", "enc2Splits");
            aslSettings.AddSetting("enc2Button2", true, "Split on button 2", "enc2Splits");
            aslSettings.AddSetting("enc2Dialogue", true, "Split during second dialogue", "enc2Splits");
            aslSettings.AddSetting("enc2Hellroom", true, "Split at hellroom entrance", "enc2Splits");

            aslSettings.AddSetting("b2Splits", true, "The Beacon 2", "subSplits");
            aslSettings.AddSetting("b2Warp", true, "Split on death warp", "b2Splits");
            aslSettings.AddSetting("b2Button1", true, "Split on first button", "b2Splits");
            aslSettings.AddSetting("b2Trigger", true, "Split when you touch heatsink trigger", "b2Splits");

            aslSettings.AddSetting("b3Splits", true, "The Beacon 3", "subSplits");
            aslSettings.AddSetting("b3Module1", true, "Split on retrieve module", "b3Splits");
            aslSettings.AddSetting("b3Module2", true, "Split on insert module", "b3Splits");

            aslSettings.AddSetting("tbfSplits", true, "Trial by Fire Elevator", "subSplits");

            aslSettings.AddSetting("arkSplits", true, "The Ark", "subSplits");
            aslSettings.AddSetting("arkElevator", true, "Split when the elevator starts going up", "arkSplits");
            aslSettings.AddSetting("arkKnife", true, "Split when you start data knife cutscene", "arkSplits");

            aslSettings.AddSetting("foldSplits", true, "The Fold Weapon", "subSplits");
            aslSettings.AddSetting("foldDataCore", true, "Split when you insert the data core", "foldSplits");
            aslSettings.AddSetting("foldEscape", true, "Split when escape starts", "foldSplits");

            aslSettings.AddSetting("ilSettings", false, "IL Settings", null);
            aslSettings.AddSetting("BnRpause", false, "Blood and Rust IL pause", "ilSettings");
            aslSettings.AddSetting("enc3pause", false, "Effect and Cause 3 IL pause", "ilSettings");
            aslSettings.AddSetting("loadReset", false, "Reset after load screens", "ilSettings");
            Settings.InitASLSettings(aslSettings);

            values["radioSpeaking"] = new MemoryValue("int", new DeepPointer("client.dll", 0x2A98128));
            values["dialogue"] = new MemoryValue("int", new DeepPointer("client.dll", 0x2A9612C, new int[] { }));
            values["inCutscene"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B58, new int[] { }));
            values["flag"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B60));

            values["menuText"] = new MemoryValue("string20", new DeepPointer("client.dll", 0x22BC680));
            values["thing"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC26B04));
            values["angle"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x7B666C));
            values["velocity"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2A9EEB8, new int[] { 0x884 }));
            values["bnrbutton1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x13B1E914));
            values["bnrbutton2"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9D48));
            values["enc2button1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9C98));
            values["enc2button2"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9E08));
            values["b2button"] = new MemoryValue("int", new DeepPointer("server.dll", 0x1506C00));
            values["arkElevator"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x139A4D38));
            values["viper"] = new MemoryValue("int", new DeepPointer("client.dll", 0xC0916C));
            values["embarkCount"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E18D8));

            values["f12Bind"] = new MemoryValue("string30", new DeepPointer("engine.dll", 0x1396CC30, new int[] { 0x0 }));
            values["speedmodLoading"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x23E8738, new int[] { }));
            values["speedmodLevel"] = new MemoryValue("string20", new DeepPointer("engine.dll", 0x13977A70, new int[] { }));
            values["lurchMax"] = new MemoryValue("float", new DeepPointer("client.dll", 0x11B0308, new int[] { }));
            values["slideStepVelocityReduction"] = new MemoryValue("int", new DeepPointer("client.dll", 0x11B0D28, new int[] { }));
            values["repelEnable"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x11B287C, new int[] { }));
            values["slideBoostCooldown"] = new MemoryValue("float", new DeepPointer("client.dll", 0x11B3AD8, new int[] { }));

            values["x"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2172FF8, new int[] { 0xDEC }));
            values["y"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2173B48, new int[] { 0x2A0 }));
            values["z"] = new MemoryValue("float", new DeepPointer("client.dll", 0x216F9C0, new int[] { 0xF4 }));
            values["level"] = new MemoryValue("string20", new DeepPointer("engine.dll", 0x13536498, new int[] { 0x2C }));
            values["b3Door"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9D18, new int[] { }));
            values["gauntletDialogue"] = new MemoryValue("int", new DeepPointer("client.dll", 0x02A9F500, new int[] { 0x10, 0x50, 0xCF48, 0x20, 0x4C0, 0x568, 0x7E8, 0x900, 0x10, 0x4B90 }));
            values["arkDialogue"] = new MemoryValue("int", new DeepPointer("client.dll", 0x23E7C18, new int[] { }));
            values["isB1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0xF8DCC1C, new int[] { }));
            values["rodeo"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E0FE4, new int[] { }));
            values["btSpeak1"] = new MemoryValue("int", new DeepPointer("client.dll", 0x02A9F080, new int[] { 0xC0, 0x4C0, 0x568, 0x2A8, 0xC0, 0x10, 0x48 }));
            values["btSpeak2"] = new MemoryValue("int", new DeepPointer("client.dll", 0x02A9F080, new int[] { 0xC0, 0x3B8, 0x180, 0x520, 0xB8, 0x648, 0x10, 0xD8, 0x10, 0x4C }));
            values["onWall"] = new MemoryValue("int", new DeepPointer("server.dll", 0x1211270, new int[] { }));
            values["inCutscene"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B58, new int[] { }));
            values["clFrames"] = new MemoryValue("int", new DeepPointer("materialsystem_dx11.dll", 0x1A9F4A8, new int[] { 0x58C }));
            values["b3Fight"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC28754, new int[] { }));
            values["airAcceleration"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x13084248, new int[] { 0x2564 }));
            values["airSpeed"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x13084248, new int[] { 0xEA8, 0x1008, 0x1038, 0x390, 0x48, 0x18, 0xA30, 0x10, 0x2218 }));
            values["maxHealth"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x13084248, new int[] { 0xA90, 0x18, 0xED8, 0x48, 0xA40, 0x10, 0xCB0, 0x4D0 }));
            values["currentHealth"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x13084248, new int[] { 0xA90, 0x18, 0xED8, 0x48, 0xA40, 0x10, 0xCB0, 0x4D4 }));
            values["yaw"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00E69EA0, new int[] { 0x1E94 }));
            values["holdingW"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C7D8, new int[] { }));
            values["holdingA"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C678, new int[] { }));
            values["holdingS"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C798, new int[] { }));
            values["holdingD"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C6A8, new int[] { }));
            values["holdingZ"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C80C, new int[] { }));
            values["timescale"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x1315A2C8, new int[] { }));
            values["holdingShift"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396CAB8, new int[] { }));
            values["lean"] = new MemoryValue("float", new DeepPointer("client.dll", 0x216FC68, new int[] { }));
            values["velX"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C2C, new int[] { }));
            values["velY"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C30, new int[] { }));
            values["velZ"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C34, new int[] { }));

            _ncsAutoLoader = new NCSAutoLoader(this);
            _speedmod = new Speedmod(this);

            _autoStrafer = new AutoStrafer(this);
            _zLurchMacro = new ZLurchMacro(this);
            _autoJumper = new AutoJumper(this);
            _splitter = new FzzySplitter(this);

            updateTimer = new Timer() { Interval = 15 };
            updateTimer.Tick += (sender, args) =>
            {
                UpdateScript();
            };
            updateTimer.Enabled = true;
        }

        private void UpdateScript()
        {
            process = Process.GetProcessesByName("Titanfall2").OrderByDescending(x => x.StartTime).FirstOrDefault(x => !x.HasExited);
            if (process == null) return;

            if (timer == null) timer = new TimerModel() { CurrentState = state };

            try
            {
                wasLoading = isLoading;
                isLoading = values["clFrames"].Current <= 0 || values["thing"].Current == 0;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
                if (Settings.TASToolsEnabled)
                {
                    _autoStrafer.Tick();
                    _zLurchMacro.Tick();
                    _autoJumper.Tick();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
                if (Settings.AutoLoadNCS && !Settings.Speedmod) _ncsAutoLoader.Tick();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
               _speedmod.Tick();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
                _splitter.Tick();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            foreach (MemoryValue value in values.Values)
            {
                value.EndTick();
            }
        }

        public static string GetTitanfallInstallDirectory()
        {

            string titanfallInstallDirectory = "";

            string originInstall = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Respawn\\Titanfall2", "Install Dir", null);
            if (originInstall == null)
            {
                string steamInstall = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
                if (steamInstall == null)
                {
                    return null;
                }

                string steamTitanfallDefault = Path.Combine(steamInstall.Replace("/", "\\"), "steamapps\\common\\Titanfall2");
                if (Directory.Exists(steamTitanfallDefault))
                {
                    titanfallInstallDirectory = steamTitanfallDefault;
                }
                else
                {
                    string config = Path.Combine(steamInstall, "config\\config.vdf");
                    string[] lines = File.ReadAllLines(config);
                    foreach (string line in lines)
                    {
                        if (line.Contains("BaseInstallFolder"))
                        {
                            int lastQuote = line.LastIndexOf('"');
                            int secondToLastQuote = line.Substring(0, lastQuote).LastIndexOf('"');
                            string gameDirectory = line.Substring(secondToLastQuote + 1, lastQuote - secondToLastQuote - 1);
                            string externalTitanfallDirectory = Path.Combine(gameDirectory, "Titanfall2");
                            if (Directory.Exists(externalTitanfallDirectory))
                            {
                                titanfallInstallDirectory = externalTitanfallDirectory;
                            }
                        }
                    }
                }
            }
            else
            {
                titanfallInstallDirectory = originInstall;
            }
            if (titanfallInstallDirectory.Length == 0) return null;

            return titanfallInstallDirectory;
        }

        public override void Dispose()
        {
            updateTimer.Dispose();
            //_speedmod.DisableSpeedmod();
        }

        public override string ComponentName => "FzzyTools";

        public override XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public override void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }
    }
}
