﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Forms;

namespace TimeHACK.Engine
{
    public static class SaveSystem
    {
        public static Save CurrentSave { get; set; }
        public static FileSystemFolderInfo filesystemflinfo { get; set; }
        public static bool DevMode = false;
        public static Form troubleshooter;

        public static Theme currentTheme { get; set; }

        public static string GameDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeHACK");
            }
        }

        public static string DataDirectory
        {
            get
            {
                return Path.Combine(GameDirectory, "Data");
            }
        }

        public static string AllProfilesDirectory
        {
            get
            {
                return Path.Combine(GameDirectory, "Profiles");
            }
        }

        public static string ProfileName = "";
        public static string ProfileFile = "main.save";

        public static string ProfileDirectory
        {
            get
            {
                return Path.Combine(GameDirectory, Path.Combine("Profiles", ProfileName));
            }
        }

        public static string ProfileFileSystemDirectory
        {
            get
            {
                return Path.Combine(ProfileDirectory, "folders");
            }
        }

        public static string ProfileMyComputerDirectory
        {
            get
            {
                return Path.Combine(ProfileFileSystemDirectory, "CDrive");
            }
        }

        public static string ProfileSettingsDirectory
        {
            get
            {
                    return Path.Combine(ProfileMyComputerDirectory, "Settings");
            }
        }

        public static string ProfileDocumentsDirectory
        {
            get
            {
                return Path.Combine(ProfileMyComputerDirectory, "Doc");                
            }
        }

        public static string ProfileProgramsDirectory
        {
            get
            {
                return Path.Combine(ProfileMyComputerDirectory, "Prog");
            }
        }

        public static string ProfileWindowsDirectory
        {
            get
            {
                return Path.Combine(ProfileMyComputerDirectory, "Win");
            }
        }

        public static bool LoadSave()
        {
            try
            {
                // ON A FINAL RELEASE USE THE "FINAL RELEASE THINGS"
                #region Final Release Things
                //Read base64 string from file
                //string b64 = File.ReadAllText(Path.Combine(ProfileDirectory, ProfileFile));
                //Get Unicode byte array
                //byte[] bytes = Convert.FromBase64String(b64);
                //Decode the Unicode
                //string json = Encoding.UTF8.GetString(bytes);
                //Deserialize save object.
                #endregion
                // USE THE THINGS IN THE "DEVELOPER THINGS" FOR A DEVELOPMENT RELEASE
                #region Developer Things
                string json = File.ReadAllText(Path.Combine(ProfileDirectory, ProfileFile));
                #endregion
                CurrentSave = JsonConvert.DeserializeObject<Save>(json);
                
            } catch
            {
                MessageBox.Show("WARNING! It looks like this save is corrupt!");
                MessageBox.Show("We will now open the Save troubleshooter");

                troubleshooter.ShowDialog();
            }
            return true;
        }

        public static void NewGame()
        {        
            var save = new Save();
            save.ExperiencedStories = new List<string>();
            if (DevMode == true)
            {
                if (ProfileName == "98")
                {
                    save.CurrentOS = "98";
                    save.ThemeName = "default98";
                    currentTheme = new Default98Theme();
                }
                else
                {
                    save.CurrentOS = "95";
                    save.ThemeName = "default95";
                    currentTheme = new Default95Theme();
                }
            }
            else
            {
                save.CurrentOS = "95";
                save.ThemeName = "default95";
                currentTheme = new Default95Theme();
            }
            CurrentSave = save;
                      
            CheckFiles();
            SaveGame();
        }

        public static void CheckFiles()
        {
            if (!Directory.Exists(GameDirectory))
                Directory.CreateDirectory(GameDirectory);

            if (!Directory.Exists(AllProfilesDirectory))
                Directory.CreateDirectory(AllProfilesDirectory);

            if (!Directory.Exists(ProfileDirectory))
                Directory.CreateDirectory(ProfileDirectory);

            if (!Directory.Exists(ProfileFileSystemDirectory))
                Directory.CreateDirectory(ProfileFileSystemDirectory);

            SaveDirectoryInfo(ProfileFileSystemDirectory, false, "My Computer", false);            
            SaveDirectoryInfo(ProfileMyComputerDirectory, false, "Win95 (C:)", true);
            if (CurrentSave.CurrentOS == "95" || CurrentSave.CurrentOS == "98") SaveDirectoryInfo(ProfileDocumentsDirectory, false, "My Documents", true);
            if (CurrentSave.CurrentOS == "2000" || CurrentSave.CurrentOS == "ME") SaveDirectoryInfo(ProfileSettingsDirectory, false, "Documents and Settings", true);                    
            SaveDirectoryInfo(ProfileProgramsDirectory, true, "Program Files", true);
            SaveDirectoryInfo(Path.Combine(ProfileProgramsDirectory, "Accessories"), false, "Accessories", true);
            SaveDirectoryInfo(Path.Combine(ProfileProgramsDirectory, "Internet Explorer"), true, "Internet Explorer", true);
            SaveDirectoryInfo(Path.Combine(ProfileProgramsDirectory, "The Microsoft Network"), true, "The Microsoft Network", true);
            SaveDirectoryInfo(ProfileWindowsDirectory, true, "Windows", true);

            CreateWindowsFile(Path.Combine(ProfileProgramsDirectory, "Accessories", "wordpad.exe"), "wordpad");
            CreateWindowsFile(Path.Combine(ProfileProgramsDirectory, "Internet Explorer", "ie20.exe"), "ie");
            CreateWindowsFile(Path.Combine(ProfileProgramsDirectory, "Internet Explorer", "lnfinst.exe"), "iebrokeninstaller");
            CreateWindowsFile(Path.Combine(ProfileProgramsDirectory, "The Microsoft Network", "msnver.txt"), "5900");
            
            CreateWindowsDirectory();
        }

        public static void CreateWindowsDirectory()
        {
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "System"), true, "System", true);
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "Config"), true, "Config", true);
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "Cursors"), true, "Cursors", true);
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "Fonts"), true, "Fonts", true);
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "Help"), true, "Help", true);
            SaveDirectoryInfo(Path.Combine(ProfileWindowsDirectory, "Temp"), true, "Temp", true);

            CreateWindowsFile(Path.Combine(ProfileWindowsDirectory, "calc.exe"), "calc");
            CreateWindowsFile(Path.Combine(ProfileWindowsDirectory, "explorer.exe"), "explorer");
        }

        public static void CreateWindowsFile(string filepath, string contents)
        {
            File.WriteAllText(filepath, contents);
        }

        public static void UpgradeFileSystem(string oldOS, string newOS)
        {
            switch (oldOS)
            {
                case "95":
                    if (newOS == "98" || newOS == "2000" || newOS == "ME")
                    {
                        // We are upgrading from the old WinClassic file System to the new WinClassic filesystem!
                        // All the above OSes share basically the same file layout!
                        // (Excluding Documents And Settings) which is 2000 and ME only

                        // Rename the C Drive to Win98

                        SaveDirectoryInfo(ProfileMyComputerDirectory, false, "Win98 (C:)", true);

                        // Add Address Book into existance!

                        SaveDirectoryInfo(Path.Combine(ProfileProgramsDirectory, "Outlook Express"), false, "Outlook Express", true);
                        CreateWindowsFile(Path.Combine(ProfileProgramsDirectory, "Outlook Express", "WAB.exe"), "addressbook");

                        // There is no "The Microsoft Network" folder!

                        if (Directory.Exists(Path.Combine(ProfileProgramsDirectory, "The Microsoft Network"))) Directory.Delete(Path.Combine(ProfileProgramsDirectory, "The Microsoft Network"));
                    }
                    break;
            }
        }

        public static void SaveDirectoryInfo(string directory, bool isProtected, string label, bool allowback)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            FileSystemFolderInfo info = new FileSystemFolderInfo();

            info.Isprotected = isProtected;
            info.label = label;
            info.allowback = allowback;

            string toWrite = JsonConvert.SerializeObject(info, Formatting.Indented);

            File.WriteAllText(Path.Combine(directory, "_data.info"), toWrite);
        }


        public static void SaveGame()
        {
            //Serialize the save to JSON.
            string json = JsonConvert.SerializeObject(CurrentSave, Formatting.Indented);

            // ADD THE TWO LINES OF CODE BELOW ON A FINAL RELEASE
            //Get JSON bytes (Unicode format).
            //var bytes = Encoding.UTF8.GetBytes(json);
            //Encode the array into Base64.
            //string b64 = Convert.ToBase64String(bytes);
            //Write to disk.

            // CHANGE THE "JSON" TO "B64" ON A FINAL RELEASE!
            File.WriteAllText(Path.Combine(ProfileDirectory, ProfileFile), json);
        }

        public static byte[] GetAchievements()
        {
            byte[] byt = new byte[] { 0, 0 };
            if (DevMode) File.WriteAllBytes(Path.Combine(DataDirectory, "achieved.thack"), byt);

            if (File.Exists(Path.Combine(DataDirectory, "achieved.thack"))) byt = File.ReadAllBytes(Path.Combine(DataDirectory, "achieved.thack"));
            else File.WriteAllBytes(Path.Combine(DataDirectory, "achieved.thack"), byt);

            return byt;
        }

        public static void SetTheme()
        {
            switch (CurrentSave.ThemeName)
            {
                case "default95":
                    currentTheme = new Default95Theme();
                    break;
                case "default98":
                    currentTheme = new Default98Theme();
                    break;
                case "dangeranimals":
                    currentTheme = new DangerousCreaturesTheme();
                    break;
                case "insidepc":
                    currentTheme = new InsideComputerTheme();
                    break;
            }
        }
    }

    public class Save
    {
        public string Username { get; set; }

        public string CurrentOS { get; set; }
        // public Dictionary<string, bool> InstalledPrograms { get; set; } InstallProgram is no longer needed... we have that data in the FileSystem
        public List<string> ExperiencedStories { get; set; }
        public bool FTime95 { get; set; }
        public string ThemeName { get; set; }
    }

    public class FileSystemFolderInfo
    {
        public bool Isprotected { get; set; }
        public string label { get; set; }
        public bool allowback { get; set; }
    }
}
