using System.Windows;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

namespace IconPatcher
{
    /// <summary>
    /// View for the program.
    /// </summary>
    public partial class MainWindow : Window
    {
        private Controller _controller; // reference to the Controller

        // Constructor for View
        public MainWindow()
        {
            InitializeComponent();
            _controller = new Controller(this); // initialize Controller
        }

        // Add Mod button
        private void BtnAddMod_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); // display file selection dialog
            if ((bool)openFileDialog.ShowDialog()) // if a file was selected
            {
                string fullPath = openFileDialog.FileName;

                if (Path.GetFileNameWithoutExtension(fullPath) == "IconMap") // chck that an IconMap file was selected
                {
                    string guiFolder = Path.GetDirectoryName(fullPath);
                    string modFolder = Path.GetDirectoryName(guiFolder);
                    _controller.AddMod(modFolder); // call Controller's AddMod() function with path to mod
                }
            }
        }

        // Remove Mod button
        private void BtnRemoveMod_Click(object sender, RoutedEventArgs e)
        {
            _controller.RemoveMod(); // call Controller's RemoveMod() function
        }

        // App Part button 
        private void BtnAddPart_Click(object sender, RoutedEventArgs e)
        {
            OverlayUUID.Visibility = Visibility.Visible; // make overlay with textbox visible
            TextBoxUUID.Focus(); // set focus to the textbox for entering a UUID
        }

        // Remove Part button
        private void BtnRemovePart_Click(object sender, RoutedEventArgs e)
        {
            _controller.RemovePart(); // call Controller's RemovePart() function
        }

        // Generate Icons button
        private void BtnGenerateIcons_Click(object sender, RoutedEventArgs e)
        {
            _controller.GenerateIcons(); // call Controller's GenerateIcons() function
        }

        // Mod is selected in Mods ListView
        private void ListViewMods_Select(object sender, MouseButtonEventArgs e)
        {
            _controller.ModSelected(); // call Controller's ModSelected() function
        }

        // UUID is selected in UUID/Part ListView
        private void ListViewParts_Select(object sender, MouseButtonEventArgs e)
        {
            _controller.PartSelected(); // call Controller's PartSelected() function
        }

        // Enter button pressed in UUID overlay
        private void BtnEnterUUID_Click(object sender, RoutedEventArgs e)
        {
            _controller.AddPart(TextBoxUUID.Text); // call Controller's AddPart() function
            closeOverlayUUID();
        }

        // Cancel button pressed in UUID overlay
        private void BtnCancelUUID_Click(object sender, RoutedEventArgs e)
        {
            closeOverlayUUID(); // call function to close overlay
        }

        // Key pressed in UUID textbox
        private void TextBoxUUID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // if Enter key was pressed
            {
                _controller.AddPart(TextBoxUUID.Text); // call Controller's AddPart() function with UUID
                closeOverlayUUID(); // call function to close overlay
            }
            else if (e.Key == Key.Escape) // if Escape key was pressed
            {
                closeOverlayUUID(); // call function to close overlay
            }
        }

        // Closes the UUID part overlay
        private void closeOverlayUUID()
        {
            OverlayUUID.Visibility = Visibility.Collapsed; // collapse overlay visibility
            TextBoxUUID.Text = ""; // clear text box
        }

        // Export Icons Button
        private void BtnExportIcons_Click(object sender, RoutedEventArgs e)
        {
            _controller.ExportIcon(); // call Controller's ExportIcon() function
        }

        // Delete key press in Mod ListView
        private void ListViewMods_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) // if Delete key was pressed
            {
                _controller.RemoveMod(); // call Controller's RemoveMod() function
            }
        }

        // Delete key press in Part ListView
        private void ListViewParts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) // if Delete key was pressed
            {
                _controller.RemovePart(); // call Controller's RemovePart() function
            }
        }
    }
}
