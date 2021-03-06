﻿using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace FzzyTools.UI.Components
{
    class Speedmod
    {

        private FzzyComponent fzzy;

        private long _previousTickCount;

        private bool _allowGauntletLoad = false;
        private bool _allowB3Load = false;

        private static string titanfallInstall;
        private static string cfg
        {
            get
            {
                if (titanfallInstall == null) return null;
                return Path.Combine(titanfallInstall, "r2\\cfg\\autosplitter.cfg");
            }
        }

        public Speedmod(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
            try
            {
                titanfallInstall = fzzy.GetTitanfallInstallDirectory();
            }
            catch (Exception)
            {
            }
        }

        private void Load(string save)
        {
            Log.Info("Load into " + save);
            RunCommand("load " + save + "; set_loading_progress_detente #INTROSCREEN_HINT_PC #INTROSCREEN_HINT_CONSOLE");
        }

        private string lastNonLoadLevel = "";
        private bool levelLoadedFromMenu = false;
        public void Tick()
        {
            if (cfg == null) return;
            if (!fzzy.Settings.SpeedmodEnabled)
            {
                if (IsSpeedmodEnabled())
                {
                    DisableSpeedmod();
                }
            }
            else
            {
                if (!IsSpeedmodEnabled())
                {
                    EnableSpeedmod();
                }
                if (!fzzy.isLoading)
                {
                    fzzy.values["airAcceleration"].Current = 10000f;
                    fzzy.values["airSpeed"].Current = 40f;
                    fzzy.values["lurchMax"].Current = 0f;
                    fzzy.values["slideStepVelocityReduction"].Current = 0f;
                    fzzy.values["slideBoostCooldown"].Current = 0f;
                }

                if (fzzy.values["inLoadingScreen"].Current && !fzzy.values["inLoadingScreen"].Old)
                {
                    levelLoadedFromMenu = lastNonLoadLevel == "";
                }
                if (!fzzy.values["inLoadingScreen"].Current) lastNonLoadLevel = fzzy.values["currentLevel"].Current;

                if (fzzy.values["currentLevel"].Current != fzzy.values["currentLevel"].Old && levelLoadedFromMenu)
                {
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_training")) Load("speedmod1");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_crashsite")) Load("speedmod2");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_sewers1")) Load("speedmod3");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_boomtown_start")) Load("speedmod4");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_boomtown_end")) Load("speedmod5");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_timeshift_spoke02")) Load("speedmod7");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_beacon_spoke0")) Load("speedmod8");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_beacon")) Load("speedmod9");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_tday")) Load("speedmod10");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_skyway_v1")) Load("speedmod11");
                }

                if (fzzy.values["clFrames"].Current <= 0)
                {
                    _allowGauntletLoad = false;
                }
                if (fzzy.values["lastLevel"].Current == "sp_training" && !fzzy.isLoading)
                {
                    if (DistanceSquared(880, 6770, 466) < 1000 * 1000) _allowGauntletLoad = true;

                    float projection = 0.866049f * fzzy.values["x"].Current + 0.499959f * fzzy.values["y"].Current;

                    if ((Math.Abs(projection + 3888.9) < 1 || Math.Abs(projection - 6622) < 1) && _allowGauntletLoad)
                    {
                        Load("speedmod2");
                        _allowGauntletLoad = false;
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_crashsite" && !fzzy.isLoading)
                {
                    if (DistanceSquared(-445, -383, 112) < 25)
                    {
                        Load("speedmod3");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_sewers1" && !fzzy.isLoading)
                {
                    if (DistanceSquared(-9138, -6732, 2605) < 500 * 500 && fzzy.values["inCutscene"].Current == 1)
                    {
                        Load("speedmod4");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_boomtown" && !fzzy.isLoading)
                {
                    float xDistance = fzzy.values["x"].Current - 8167;
                    float yDistance = fzzy.values["y"].Current + 3583;
                    double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                    if (distance < 76)
                    {
                        Load("speedmod5");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_boomtown_end" && !fzzy.isLoading)
                {
                    if (DistanceSquared(8644, 1097, -2621) < 7000 * 7000 && fzzy.values["inCutscene"].Current == 1 && fzzy.values["rodeo"].Current == fzzy.values["rodeo"].Old)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && !fzzy.isLoading)
                {
                    if (Math.Abs(fzzy.values["x"].Current - 1112.845) < 1 && Math.Abs(fzzy.values["y"].Current + 2741) < 100 && Math.Abs(fzzy.values["z"].Current + 859) < 1000)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && !fzzy.isLoading &&
                   fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                   DistanceSquared(-1108, 6017, -10596) < 1000 * 1000)
                {
                    Load("speedmod8");
                }

                if (fzzy.values["lastLevel"].Current == "sp_beacon_spoke0" && !fzzy.isLoading)
                {
                    if (fzzy.values["y"].Current > 3000) _allowB3Load = true;

                    if (_allowB3Load &&
                        fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                        fzzy.values["y"].Current < -500)
                    {
                        Load("speedmod9");
                        _allowB3Load = false;
                    }
                }
                if (fzzy.values["clFrames"].Current <= 0)
                {
                    _allowB3Load = false;
                }

                if (fzzy.values["lastLevel"].Current == "sp_beacon" && !fzzy.isLoading &&
                    fzzy.values["b3Fight"].Current > 0 &&
                    fzzy.values["inCutscene"].Current == 2)
                {
                    Load("speedmod10");
                }

                if (fzzy.values["lastLevel"].Current == "sp_tday" && !fzzy.isLoading &&
                    DistanceSquared(6738, 12395, 2573) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod11");
                }

                if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" && !fzzy.isLoading &&
                    DistanceSquared(9023, 12180, 5693) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod12");
                }
            }

            _previousTickCount = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private float DistanceSquared(float x, float y, float z)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2) + Math.Pow(z - fzzy.values["z"].Current, 2);
            return (float)dis;
        }

        private float DistanceSquared(float x, float y)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2);
            return (float)dis;
        }

        private void Write(DeepPointer pointer, byte[] b)
        {
            pointer.DerefOffsets(FzzyComponent.process, out var ptr);
            FzzyComponent.process.WriteBytes(ptr, b);
        }

        public void EnableSpeedmod()
        {
            if (FzzyComponent.process == null) return;
            if (IsSpeedmodEnabled()) return;
            try
            {
                RemoveWallFriction();
                MakeAlliesInvincible();
            }
            catch (Exception)
            {
            }
        }

        private static string speedmodSavesInstaller = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\profile\\savegames\\installspeedmodsaves.exe");
        public static void InstallSpeedmod(FzzySettings settings)
        {
            if (titanfallInstall == null || !File.Exists(Path.Combine(titanfallInstall, "Titanfall2.exe")))
            {
                string directory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Titanfall2";
                ShowInputDialog("Install Directory", "Titanfall 2 Install Directory not Available!\nPlease enter where you have titanfall 2 installed\n(The folder that contains Titanfall2.exe)", ref directory);
                settings.TitanfallInstallDirectoryOverride = directory;
                return;
            }

            string settingscfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\local\\settings.cfg");
            string settingscontent = File.ReadAllText(settingscfg);
            if (!settingscontent.Contains("\nbind \"F11\" \"exec autosplitter.cfg\""))
            {
                if (FzzyComponent.process != null)
                {
                    FzzyComponent.AddToSettingsOnClose("\nbind \"F11\" \"exec autosplitter.cfg\"");
                    MessageBox.Show("Speedmod Installed!\nRestart your game for it to take effect.");
                } else
                {
                    File.AppendAllText(settingscfg, "\nbind \"F11\" \"exec autosplitter.cfg\"");
                }
            }
            if (FzzySettings.AreSpeedmodSavesInstalled()) return;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileAsync(new Uri(FzzyComponent.SPEEDMOD_SAVES_INSTALLER_LINK), speedmodSavesInstaller);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(SpeedmodSavesDownloadCompleted);
            }
        }
        private static DialogResult ShowInputDialog(string name, string message, ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(350, 120);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = name;

            System.Windows.Forms.Label label = new Label();
            label.Size = new System.Drawing.Size(size.Width - 10, 39);
            label.Location = new System.Drawing.Point(5, 5);
            label.Text = message;
            inputBox.Controls.Add(label);

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 50);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, size.Height - 30);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, size.Height - 30);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
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

        public void DisableSpeedmod()
        {
            if (FzzyComponent.process == null) return;
            if (!IsSpeedmodEnabled()) return;
            try
            {
                fzzy.values["airAcceleration"].Current = 500f;
                fzzy.values["airSpeed"].Current = 60f;
                fzzy.values["lurchMax"].Current = 0.7f;
                fzzy.values["slideStepVelocityReduction"].Current = 10f;
                fzzy.values["slideBoostCooldown"].Current = 2f;
                RestoreWallFriction();
                MakeAlliesKillable();
            }
            catch (Exception)
            {
            }
        }

        private void MakeAlliesInvincible()
        {
            foreach (ProcessModule m in FzzyComponent.process.Modules)
            {
                if (m.ModuleName == "server.dll")
                {
                    var code = new byte[] { 0x83, 0xBB, 0x10, 0x01, 0x00, 0x00, 0x03, 0x74, 0x02, 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3 };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x43373A, code);
                    var jmp = new byte[] { 0x74, 0x1E };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x433725, jmp);
                    break;
                }
            }
        }

        private void RunCommand(string cmd)
        {
            File.WriteAllText(cfg, cmd);
            Log.Info("running command: " + cmd);
            fzzy.board.Send(Keyboard.ScanCodeShort.F11);
        }

        private void MakeAlliesKillable()
        {
            var code = new byte[] { 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC };
            Write(new DeepPointer("server.dll", 0x43373A), code);
            var jmp = new byte[] { 0x74, 0x15 };
            Write(new DeepPointer("server.dll", 0x433725), jmp);
        }

        public bool IsSpeedmodEnabled()
        {
            var speedmodCode = new DeepPointer("server.dll", 0x43373A, new int[] { }).Deref<byte>(FzzyComponent.process);
            return speedmodCode == 0x83;
        }

        private void RemoveWallFriction()
        {
            byte[] code = new byte[] {
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
            };

            Write(new DeepPointer("client.dll", 0x20D6E5), code);
            Write(new DeepPointer("server.dll", 0x185D36), code);
        }

        private void RestoreWallFriction()
        {
            byte[] code = new byte[] {
                0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00, // movss [rcx+8C],xmm0
                0xF3, 0x0F, 0x59, 0x89, 0x90, 0x00, 0x00, 0x00, // mulss xmm1,[rcx+90]
                0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00, // movss [rcx+90],xmm1
            };

            Write(new DeepPointer("client.dll", 0x20D6E5), code);
            Write(new DeepPointer("server.dll", 0x185D36), code);
        }

    }
}
