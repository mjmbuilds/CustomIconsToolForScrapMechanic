using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace IconPatcher
{
    /// <summary>
    /// Represents a Mod. Contains a file path to the mod folder 
    /// and a list of UUIDs for parts to have custom icons.
    /// </summary>
    class Mod
    {
        public string ModFilePath { get; set; }     // File path to a mod
        public ObservableCollection<string> PartUUIDs { get; set; } // List of part UUIDs

        [JsonIgnore]
        public string ModName // gets the name of the Mod from the directory
        {
            get
            {
                return new DirectoryInfo(ModFilePath).Name;
            }
        }

        public Mod(string modFilePath)
        {
            ModFilePath = modFilePath;
            PartUUIDs = new ObservableCollection<string>();
        }
    }
}
