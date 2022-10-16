using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.ASL;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using Microsoft.Win32;
using Syringe;

namespace FzzyTools.UI.Components
{
    public class FzzyComponent : LogicComponent
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ConsoleCommand
        {
            [CustomMarshalAs(CustomUnmanagedType.LPStr)]
            public string Cmd;
        };

        public static void RunGameCommand(string cmd)
        {
            if (process == null) return;

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Components\\TitanfallInjection.dll"))
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileAsync(new Uri(INJECTION_DLL_LINK),
                        Directory.GetCurrentDirectory() + "\\Components\\TitanfallInjection.dll");
                }

                return;
            }

            var syringe = new Injector(process);
            syringe.InjectLibrary(Directory.GetCurrentDirectory() + "\\Components\\TitanfallInjection.dll");

            var consolecmd = new ConsoleCommand();
            consolecmd.Cmd = cmd;
            syringe.CallExport("TitanfallInjection.dll", "FzzyConsoleCommand", consolecmd);
        }

        public const string FASTANY_SAVES_INSTALLER_LINK =
            "https://github.com/Fzzy2j/FzzySplitter/releases/download/v1.0/installsaves.exe";

        public const string FASTANY1_SAVE_LINK =
            "https://github.com/Fzzy2j/FzzySplitter/releases/download/v1.0/fastany1.sav";

        public const string INJECTION_DLL_LINK =
            "https://github.com/Fzzy2j/FzzySplitter/releases/download/v1.0/TitanfallInjection.dll";

        public FzzySettings Settings { get; }

        public Timer updateTimer;

        public LiveSplitState state;
        public static LiveSplitState s;
        public TimerModel timer;

        private NCSAutoLoader ncsAutoLoader;

        private FzzySplitter splitter;

        public static Process process;

        public Dictionary<string, MemoryValue> values = new Dictionary<string, MemoryValue>();

        private ASLSettings aslSettings;

        public bool isLoading;
        public bool wasLoading;

        public FzzyComponent(LiveSplitState state)
        {
            Settings = new FzzySettings();
            this.state = state;
            s = state;
            aslSettings = new ASLSettings();
            aslSettings.AddSetting("flagSplit", false,
                "Start timer on flag pickup, split on flag capture, and pause when not holding flag", null);
            aslSettings.AddSetting("frontierWaveSplit", false, "Split on wave change in frontier defense", null);
            aslSettings.AddSetting("levelChangeSplit", true, "Split on level change", null);

            aslSettings.AddSetting("endSplit", true, "Split at the end of escape (end of run)", null);
            aslSettings.AddSetting("removeLoads", true, "Remove Loads", null);

            aslSettings.AddSetting("subSplits", false, "Subsplits", null);

            aslSettings.AddSetting("btSplits", true, "BT-7274", "subSplits");
            aslSettings.AddSetting("btBattery1", true, "Split on first battery", "btSplits");
            aslSettings.AddSetting("btBattery2", true, "Split on second battery", "btSplits");

            aslSettings.AddSetting("bnrSplits", true, "Blood & Rust", "subSplits");
            aslSettings.AddSetting("bnrButton1", true, "Split on first button", "bnrSplits");
            aslSettings.AddSetting("bnrDoor", true, "Split at door", "bnrSplits");
            aslSettings.AddSetting("bnrButton2", true, "Split on second button", "bnrSplits");
            aslSettings.AddSetting("bnrEmbark", true, "Split on embark BT", "bnrSplits");

            aslSettings.AddSetting("ita3Splits", true, "Into the Abyss 3 Embark", "subSplits");

            aslSettings.AddSetting("enc1Splits", true, "Effect & Cause 1 Cutscene", "subSplits");

            aslSettings.AddSetting("enc2Splits", true, "Effect & Cause 2", "subSplits");
            aslSettings.AddSetting("enc2Button1", true, "Split on button 1", "enc2Splits");
            aslSettings.AddSetting("enc2Button2", true, "Split on button 2", "enc2Splits");
            aslSettings.AddSetting("enc2Dialogue", true, "Split during second dialogue", "enc2Splits");
            aslSettings.AddSetting("enc2Hellroom", true, "Split at hellroom entrance", "enc2Splits");
            aslSettings.AddSetting("enc2Vent", true, "Split at the bottom of the end vents", "enc2Splits");

            aslSettings.AddSetting("b2Splits", true, "The Beacon 2", "subSplits");
            aslSettings.AddSetting("b2Warp", true, "Split on death warp", "b2Splits");
            aslSettings.AddSetting("b2Button1", true, "Split on first button", "b2Splits");
            aslSettings.AddSetting("b2Trigger", true, "Split when you touch heatsink trigger", "b2Splits");

            aslSettings.AddSetting("b3Splits", true, "The Beacon 3", "subSplits");
            aslSettings.AddSetting("b3Module1", true, "Split on retrieve module", "b3Splits");
            aslSettings.AddSetting("b3Module2", true, "Split on insert module", "b3Splits");
            aslSettings.AddSetting("b3SecureBeacon", true, "Split on secure beacon objective", "b3Splits");

            aslSettings.AddSetting("tbfSplits", true, "Trial by Fire", "subSplits");
            aslSettings.AddSetting("tbfPilotWithMe", false, "Split on \"Pilot you're with me\" dialog", "tbfSplits");
            aslSettings.AddSetting("tbfElevator", true, "Split on Elevator", "tbfSplits");

            aslSettings.AddSetting("arkSplits", true, "The Ark", "subSplits");
            aslSettings.AddSetting("arkElevator", true, "Split when the elevator starts going up", "arkSplits");
            aslSettings.AddSetting("arkKnife", true, "Split when you start data knife cutscene", "arkSplits");

            aslSettings.AddSetting("foldSplits", true, "The Fold Weapon", "subSplits");
            aslSettings.AddSetting("foldDataCore", true, "Split when you insert the data core", "foldSplits");
            aslSettings.AddSetting("foldEscape", true, "Split when escape starts", "foldSplits");

            aslSettings.AddSetting("miscSettings", false, "Misc. Settings", null);
            aslSettings.AddSetting("BnRpause", false, "Blood and Rust IL pause", "miscSettings");
            aslSettings.AddSetting("enc3pause", false, "Effect & Cause 3 IL pause", "miscSettings");
            aslSettings.AddSetting("loadReset", false, "Reset after load screens", "miscSettings");
            aslSettings.AddSetting("cheatsTimerLink", false, "Tie sv_cheats with if the timer is started or not",
                "miscSettings");
            aslSettings.AddSetting("tickTimer", false, "Tick-based Timer", "miscSettings");
            aslSettings.AddSetting("levelTimer", false, "Level-based Timer", "tickTimer");

            aslSettings.AddSetting("helmetSplit", false, "Helmet splits", null);

            // the ordering of these is a bit wack because the sp_unlocks_level_x convar order of bits storing helmets is a bit arbitrary
            // "levelHelmetX" is the order of how they're stored in the bits, the setting name "Helmet X" is ordered after when you collect each helmet in the run
            aslSettings.AddSetting("gauntletHelmetSplit", true, "Gauntlet", "helmetSplit");
            aslSettings.AddSetting("btHelmetSplit", true, "BT-7274", "helmetSplit");
            aslSettings.AddSetting("btHelmet1", true, "Helmet 1", "btHelmetSplit");
            aslSettings.AddSetting("btHelmet2", true, "Helmet 2", "btHelmetSplit");

            aslSettings.AddSetting("bnrHelmetSplit", true, "Blood & Rust", "helmetSplit");
            aslSettings.AddSetting("bnrHelmet2", true, "Helmet 1", "bnrHelmetSplit");
            aslSettings.AddSetting("bnrHelmet1", true, "Helmet 2", "bnrHelmetSplit");
            aslSettings.AddSetting("bnrHelmet6", true, "Helmet 3", "bnrHelmetSplit");
            aslSettings.AddSetting("bnrHelmet5", true, "Helmet 4", "bnrHelmetSplit");
            aslSettings.AddSetting("bnrHelmet4", true, "Helmet 5", "bnrHelmetSplit");
            aslSettings.AddSetting("bnrHelmet3", true, "Helmet 6", "bnrHelmetSplit");

            aslSettings.AddSetting("ita1HelmetSplit", true, "Into the Abyss 1", "helmetSplit");
            aslSettings.AddSetting("ita1Helmet2", true, "Helmet 1", "ita1HelmetSplit");
            aslSettings.AddSetting("ita1Helmet1", true, "Helmet 2", "ita1HelmetSplit");
            aslSettings.AddSetting("ita1Helmet3", true, "Helmet 3", "ita1HelmetSplit");
            aslSettings.AddSetting("ita1Helmet4", true, "Helmet 4", "ita1HelmetSplit");

            aslSettings.AddSetting("ita2HelmetSplit", false, "Into the Abyss 2", "helmetSplit");
            aslSettings.AddSetting("ita2Helmet1", false, "Helmet 1", "ita2HelmetSplit");
            aslSettings.AddSetting("ita2Helmet2", false, "Helmet 2", "ita2HelmetSplit");
            aslSettings.AddSetting("ita2Helmet3", false, "Helmet 3", "ita2HelmetSplit");

            aslSettings.AddSetting("ita3HelmetSplit", true, "Into the Abyss 3", "helmetSplit");
            aslSettings.AddSetting("ita3Helmet1", true, "Helmet 1", "ita3HelmetSplit");
            aslSettings.AddSetting("ita3Helmet2", true, "Helmet 2", "ita3HelmetSplit");

            aslSettings.AddSetting("enc1HelmetSplit", false, "Effect & Cause 1/3", "helmetSplit");
            aslSettings.AddSetting("enc1Helmet2", false, "Helmet 1", "enc1HelmetSplit");
            aslSettings.AddSetting("enc1Helmet1", false, "Helmet 2", "enc1HelmetSplit");

            aslSettings.AddSetting("enc2HelmetSplit", true, "Effect & Cause 2", "helmetSplit");
            aslSettings.AddSetting("enc2Helmet2", true, "Helmet 1", "enc2HelmetSplit");
            aslSettings.AddSetting("enc2Helmet5", true, "Helmet 2", "enc2HelmetSplit");
            aslSettings.AddSetting("enc2Helmet1", true, "Helmet 3", "enc2HelmetSplit");
            aslSettings.AddSetting("enc2Helmet4", true, "Helmet 4", "enc2HelmetSplit");
            aslSettings.AddSetting("enc2Helmet3", true, "Helmet 5", "enc2HelmetSplit");
            aslSettings.AddSetting("enc2Helmet6", true, "Helmet 6", "enc2HelmetSplit");

            aslSettings.AddSetting("b1HelmetSplit", true, "The Beacon 1/3", "helmetSplit");
            aslSettings.AddSetting("b1Helmet8", true, "Helmet 1", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet7", true, "Helmet 2", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet4", true, "Helmet 3", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet2", true, "Helmet 4", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet3", true, "Helmet 5", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet1", true, "Helmet 6", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet9", true, "Helmet 7", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet6", true, "Helmet 8", "b1HelmetSplit");
            aslSettings.AddSetting("b1Helmet5", true, "Helmet 9", "b1HelmetSplit");

            aslSettings.AddSetting("b2HelmetSplit", true, "The Beacon 2", "helmetSplit");
            aslSettings.AddSetting("b2Helmet2", true, "Helmet 1", "b2HelmetSplit");
            aslSettings.AddSetting("b2Helmet1", true, "Helmet 2", "b2HelmetSplit");

            aslSettings.AddSetting("tbfHelmetSplit", true, "Trial by Fire", "helmetSplit");
            aslSettings.AddSetting("tbfHelmet3", true, "Helmet 1", "tbfHelmetSplit");
            aslSettings.AddSetting("tbfHelmet2", true, "Helmet 2", "tbfHelmetSplit");
            aslSettings.AddSetting("tbfHelmet1", true, "Helmet 3", "tbfHelmetSplit");

            aslSettings.AddSetting("arkHelmetSplit", true, "The Ark", "helmetSplit");
            aslSettings.AddSetting("arkHelmet3", true, "Helmet 1", "arkHelmetSplit");
            aslSettings.AddSetting("arkHelmet1", true, "Helmet 2", "arkHelmetSplit");
            aslSettings.AddSetting("arkHelmet2", true, "Helmet 3", "arkHelmetSplit");

            aslSettings.AddSetting("foldHelmetSplit", true, "The Fold Weapon", "helmetSplit");
            aslSettings.AddSetting("foldHelmet3", true, "Helmet 1", "foldHelmetSplit");
            aslSettings.AddSetting("foldHelmet2", true, "Helmet 2", "foldHelmetSplit");
            aslSettings.AddSetting("foldHelmet1", true, "Helmet 3", "foldHelmetSplit");

            Settings.InitASLSettings(aslSettings);

            values["radioSpeaking"] = new MemoryValue("int", new DeepPointer("client.dll", 0x2A98128));
            values["dialogue"] = new MemoryValue("int", new DeepPointer("client.dll", 0x2A9612C));
            values["dialogOption"] = new MemoryValue("int", new DeepPointer("client.dll", 0x27B7210));
            values["inCutscene"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B58));
            values["flag"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B60));

            values["menuText"] = new MemoryValue("string20", new DeepPointer("client.dll", 0x22BC680));
            values["thing"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC26B04));
            values["angle"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x7B666C));
            values["velocity"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2A9EEB8, 0x884));
            values["bnrbutton1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x13B1E914));
            values["bnrbutton2"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9D48));
            values["enc2button1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9C98));
            values["enc2button2"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9E08));
            values["b2button"] = new MemoryValue("int", new DeepPointer("server.dll", 0x1506C00));
            values["arkElevator"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x1398B458));
            values["viper"] = new MemoryValue("int", new DeepPointer("client.dll", 0xC0916C));
            values["embarkCount"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E18D8));
            values["pilotYoureWithMe"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x130D0D78));

            values["lurchMax"] = new MemoryValue("float", new DeepPointer("client.dll", 0x11B0308));
            values["slideStepVelocityReduction"] =
                new MemoryValue("int", new DeepPointer("client.dll", 0x11B0D28));
            values["repelEnable"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x11B287C));
            values["slideBoostCooldown"] =
                new MemoryValue("float", new DeepPointer("client.dll", 0x11B3AD8));
            values["editableVelocityX"] = new MemoryValue("float",
                new DeepPointer("server.dll", 0x00B0EB50, 0xD8, 0x6B8, 0x428));
            values["editableVelocityY"] = new MemoryValue("float",
                new DeepPointer("server.dll", 0x00B0EB50, 0xD8, 0x6B8, 0x42C));
            values["editableVelocityZ"] = new MemoryValue("float",
                new DeepPointer("server.dll", 0x00B0EB50, 0xD8, 0x6B8, 0x430));

            values["currentLevel"] =
                new MemoryValue("string20", new DeepPointer("engine.dll", 0x12A53D55));
            values["inLoadingScreen"] = new MemoryValue("bool", new DeepPointer("client.dll", 0xB38C5C));

            // helmet unlocks for each level
            values["sp_unlocks_level_0"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0A1BC)); // gauntlet
            values["sp_unlocks_level_1"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0A3FC)); // bt
            values["sp_unlocks_level_2"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0C49C)); // bnr
            values["sp_unlocks_level_3"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0C88C)); // ita1
            values["sp_unlocks_level_4"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC09A2C)); // ita2
            values["sp_unlocks_level_5"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC08FFC)); // ita3
            values["sp_unlocks_level_6"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0923C)); // enc1/3
            values["sp_unlocks_level_7"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0A48C)); // enc2
            values["sp_unlocks_level_8"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0911C)); // b1/3
            values["sp_unlocks_level_9"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC09DDC)); // b2
            values["sp_unlocks_level_10"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0A36C)); // tbf
            values["sp_unlocks_level_11"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0C7FC)); // ark
            values["sp_unlocks_level_12"] = new MemoryValue("int", new DeepPointer("server.dll", 0xC0987C)); // fold

            values["x"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2172FF8, 0xDEC));
            values["y"] = new MemoryValue("float", new DeepPointer("client.dll", 0x2173B48, 0x2A0));
            values["z"] = new MemoryValue("float", new DeepPointer("client.dll", 0x216F9C0, 0xF4));
            values["lastLevel"] = new MemoryValue("string20", new DeepPointer("server.dll", 0x1053370));
            values["b3Door"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9D18));
            values["tbfElevator"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7B9B28));
            values["gauntletDialogue"] = new MemoryValue("int",
                new DeepPointer("client.dll", 0x02A9F500, 0x10, 0x50, 0xCF48, 0x20, 0x4C0, 0x568, 0x7E8, 0x900, 0x10,
                    0x4B90));
            values["arkDialogue"] = new MemoryValue("int", new DeepPointer("client.dll", 0x23E7C18));
            values["isB1"] = new MemoryValue("int", new DeepPointer("engine.dll", 0xF8DCC1C));
            values["rodeo"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E0FE4));
            values["btSpeak1"] = new MemoryValue("int",
                new DeepPointer("client.dll", 0x02A9F080, 0xC0, 0x4C0, 0x568, 0x2A8, 0xC0, 0x10, 0x48));
            values["btSpeak2"] = new MemoryValue("int",
                new DeepPointer("client.dll", 0x02A9F080, 0xC0, 0x3B8, 0x180, 0x520, 0xB8, 0x648, 0x10, 0xD8, 0x10,
                    0x4C));
            values["inCutscene"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x111E1B58));
            values["clFrames"] = new MemoryValue("int",
                new DeepPointer("materialsystem_dx11.dll", 0x1A9F4A8, 0x58C));
            values["b3SecureBeaconObjective"] =
                new MemoryValue("int", new DeepPointer("engine.dll", 0x130D0888));
            values["airAcceleration"] =
                new MemoryValue("float", new DeepPointer("engine.dll", 0x13084248, 0x2564));
            values["airSpeed"] = new MemoryValue("float",
                new DeepPointer("engine.dll", 0x13084248, 0xEA8, 0x1008, 0x1038, 0x390, 0x48, 0x18, 0xA30, 0x10,
                    0x2218));
            values["currentHealth"] = new MemoryValue("int",
                new DeepPointer("engine.dll", 0x13084248, 0xA90, 0x18, 0xED8, 0x48, 0xA40, 0x10, 0xCB0, 0x4D4));
            values["yaw"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00E69EA0, 0x1E94));
            values["pitch"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00E69EA0, 0x1E90));
            values["velX"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C2C));
            values["velY"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C30));
            values["velZ"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C34));
            values["viewThunkVertical"] = new MemoryValue("float",
                new DeepPointer("client.dll", 0x00B188C0, 0xD8, 0x1A24));
            values["viewThunkHorizontal"] = new MemoryValue("float",
                new DeepPointer("client.dll", 0x00B188C0, 0xD8, 0x1A28));
            values["recoilVertical"] = new MemoryValue("float",
                new DeepPointer("client.dll", 0x00B188C0, 0xD8, 0x1A3C));
            values["recoilHorizontal"] = new MemoryValue("float",
                new DeepPointer("client.dll", 0x00B188C0, 0xD8, 0x1A40));
            values["holdingM3"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396CC98));

            values["timescale"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x1315A2C8));
            values["sv_cheats"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x12A50EEC));
            values["sp_startpoint"] = new MemoryValue("int", new DeepPointer("server .dll", 0xC0C6DC));
            values["currentTime"] = new MemoryValue("float", new DeepPointer("client.dll", 0xC3DB28));
            values["paused"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7A6620));
            values["tickCount"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x765A24));
            values["onGround"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x11EED78));
            values["frontierDefenseWaveNumber"] = new MemoryValue("int", new DeepPointer("engine.dll", 0x7A7D28));

            values["rightClickTimestamp"] = new MemoryValue("int", new DeepPointer("client.dll", 0x22BC5C0));
            values["enterTimestamp"] = new MemoryValue("int", new DeepPointer("client.dll", 0x22BC510));
            values["leftClickTimestamp"] = new MemoryValue("int", new DeepPointer("client.dll", 0x22BC5BC));
            values["lastSurfaceTouchNormalX"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB18910));
            values["lastSurfaceTouchNormalY"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB18914));
            values["lastSurfaceTouchNormalZ"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB18918));

            state.CurrentTimingMethod = TimingMethod.GameTime;

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Components\\TitanfallInjection.dll"))
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFileAsync(new Uri(INJECTION_DLL_LINK),
                        Directory.GetCurrentDirectory() + "\\Components\\TitanfallInjection.dll");
                }
            }

            ncsAutoLoader = new NCSAutoLoader(this);
            //speedmod = new Speedmod(this);

            //tasTools = new TASTools(this);
            //aimbot = new Aimbot(this);
            splitter = new FzzySplitter(this);

            updateTimer = new Timer {Interval = 15};
            updateTimer.Tick += (sender, args) =>
            {
                try
                {
                    UpdateScript();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            };
            updateTimer.Enabled = true;
        }

        private static bool addToSettingsOnClose;
        private static string settingToAdd;

        public static void AddToSettingsOnClose(string setting)
        {
            addToSettingsOnClose = true;
            settingToAdd = setting;
        }

        private void UpdateScript()
        {
            if (process == null)
            {
                if (addToSettingsOnClose)
                {
                    var settingscfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "Respawn\\Titanfall2\\local\\settings.cfg");
                    if (!settingscfg.Contains(settingToAdd))
                    {
                        File.AppendAllText(settingscfg, settingToAdd);
                    }

                    addToSettingsOnClose = false;
                }

                process = Process.GetProcessesByName("Titanfall2").OrderByDescending(x => x.StartTime)
                    .FirstOrDefault(x => !x.HasExited);

                return;
            }

            if (process.HasExited || process.Modules.Count < 127)
            {
                process = null;
                return;
            }

            if (timer == null) timer = new TimerModel() {CurrentState = state};

            wasLoading = isLoading;
            isLoading = values["clFrames"].Current <= 0 || values["inLoadingScreen"].Current ||
                        values["tickCount"].Current <= 23;

            var settings = Settings.aslsettings.Reader;
            if (settings["cheatsTimerLink"])
            {
                if (state.CurrentPhase == TimerPhase.Running && values["sv_cheats"].Current == 1)
                {
                    RunGameCommand("sv_cheats 0");
                }

                if (state.CurrentPhase != TimerPhase.Running && values["sv_cheats"].Current == 0)
                {
                    RunGameCommand("sv_cheats 1");
                }
            }

            //aimbot.Tick();

            if (Settings.AutoLoadNcs) ncsAutoLoader.Tick();

            splitter.Tick();

            foreach (MemoryValue value in values.Values)
            {
                value.EndTick();
            }
        }

        public string GetTitanfallInstallDirectory()
        {
            return GetTitanfallInstallDirectory(Settings);
        }

        public static string GetTitanfallInstallDirectory(FzzySettings settings)
        {
            var titanfallInstallDirectory = "";

            var originInstall = (string) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Respawn\\Titanfall2",
                "Install Dir", null);
            if (originInstall == null)
            {
                var steamInstall =
                    (string) Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
                if (steamInstall == null)
                {
                    return null;
                }

                var steamTitanfallDefault =
                    Path.Combine(steamInstall.Replace("/", "\\"), "steamapps\\common\\Titanfall2");
                if (Directory.Exists(steamTitanfallDefault))
                {
                    titanfallInstallDirectory = steamTitanfallDefault;
                }
                else
                {
                    var config = Path.Combine(steamInstall, "config\\config.vdf");
                    var lines = File.ReadAllLines(config);
                    foreach (var line in lines)
                    {
                        if (!line.Contains("BaseInstallFolder")) continue;
                        var lastQuote = line.LastIndexOf('"');
                        var secondToLastQuote = line.Substring(0, lastQuote).LastIndexOf('"');
                        var gameDirectory = line.Substring(secondToLastQuote + 1, lastQuote - secondToLastQuote - 1);
                        var externalTitanfallDirectory = Path.Combine(gameDirectory, "steamapps\\common\\Titanfall2");
                        if (Directory.Exists(externalTitanfallDirectory))
                        {
                            titanfallInstallDirectory = externalTitanfallDirectory;
                        }
                    }
                }
            }
            else
            {
                titanfallInstallDirectory = originInstall;
            }

            return titanfallInstallDirectory.Length == 0 ? null : titanfallInstallDirectory;
        }

        public override void Dispose()
        {
            updateTimer.Dispose();
            //tasTools.Stop();
            //speedmod.DisableSpeedmod();
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

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height,
            LayoutMode mode)
        {
        }
    }
}