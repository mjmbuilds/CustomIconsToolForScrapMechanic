using System.Collections.ObjectModel;

namespace IconPatcher
{
    /// <summary>
    /// Model of the program.
    /// </summary>
    class Model
    {
        public ObservableCollection<Mod> Mods; // List of Mods

        public Model()
        {
            Mods = new ObservableCollection<Mod>();
        }
    }
}
