using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;

namespace ModManager
{
    public partial class MainWindow : Window
    {
        void RefreshMods()
        {
            lstMods.Items.Clear();
            if (Directory.Exists(GameModsPath))
            {
                foreach (var dir in Directory.GetDirectories(GameModsPath))
                    lstMods.Items.Add(System.IO.Path.GetFileName(dir));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            RefreshMods();
            btnInstallPC.Click += (s, e) => InstallMod(false);
            btnInstallQuest.Click += (s, e) => InstallMod(true);
        }

        string SteamModsPath = @"C:\Program Files (x86)\Steam\steamapps\common\Ancient Dungeon VR\Ancient_Dungeon_Data\StreamingAssets\Mods";
        string OculusModsPath = @"C:\Program Files\Oculus\Software\Software\erthu-ancient-dungeon-vr\Ancient_Dungeon_Data\StreamingAssets\Mods";

        string GameModsPath
        {
            get
            {
                var steamRoot = Path.GetDirectoryName(SteamModsPath);
                if (Directory.Exists(steamRoot))
                {
                    Directory.CreateDirectory(SteamModsPath);
                    return SteamModsPath;
                }

                var oculusRoot = Path.GetDirectoryName(OculusModsPath);
                if (Directory.Exists(oculusRoot))
                {
                    Directory.CreateDirectory(OculusModsPath);
                    return OculusModsPath;
                }

                return null;
            }
        }


        void InstallMod(bool toQuest)
        {
            var dlg = new OpenFileDialog { Filter = "Zip Files|*.zip" };
            if (dlg.ShowDialog() != true) return;
            string zipPath = dlg.FileName;
            string modFolder = null;

            using (var zip = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName.EndsWith(".modinfo"))
                    {
                        modFolder = entry.FullName.Split('/')[0];
                        break;
                    }
                }
            }

            if (modFolder == null)
            {
                MessageBox.Show("No .modinfo found");
                return;
            }

            if (toQuest)
            {
                Process.Start("adb", $"push \"{zipPath}\" \"/sdcard/Download/{Path.GetFileName(zipPath)}\"");
                MessageBox.Show("Pushed zip to Quest Download folder. (Future: auto extract)");
            }
            else
            {
                if (GameModsPath == null)
                {
                    MessageBox.Show("Could not find Ancient Dungeon install folder (Steam or Oculus).");
                    return;
                }

                ZipFile.ExtractToDirectory(zipPath, GameModsPath, true);
                MessageBox.Show("Installed to " + GameModsPath);
                RefreshMods();
            }
        }
    }
}
