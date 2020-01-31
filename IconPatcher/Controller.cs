using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IconPatcher
{
    /// <summary>
    /// Controller for the program.
    /// </summary>
    class Controller
    {
        private Model _model;     // reference to the Model
        private MainWindow _view; // reference to the View

        // Constructor for Controller
        public Controller(MainWindow mainWindow)
        {
            _model = LoadUserSettings(); // set reference to Model returned from attempt to load previous state
            _view = mainWindow; // set reference to View
            _view.DataContext = _model; // set data context for View
            _view.ListViewMods.ItemsSource = _model.Mods; // set source of Mods ListView
            UpdateButtonsEnabled(); // set enable status of buttons
        }

        // Attempt to load previous settings/state of program, else return a new Model
        private Model LoadUserSettings()
        {
            try // try loading from settings json file
            {
                using (StreamReader sr = new StreamReader("IconPatcherSettings.json"))
                {
                    string jsonString = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<Model>(jsonString);
                }
            }
            catch (Exception)
            {
                // OK to silently fail here. UserSettings.json may not exist yet
            }

            return new Model();
        }

        // Save settings/state of program 
        private void SaveUserSettings()
        {
            try // try saving to settings json file
            {
                using (StreamWriter sw = new StreamWriter("IconPatcherSettings.json"))
                {
                    string jsonString = JsonConvert.SerializeObject(_model, Formatting.Indented);
                    sw.Write(jsonString);
                    FooterMessage("Saved");
                }
            }
            catch (Exception)
            {
                FooterMessage("Could not SAVE UserSettings.json", true);
            }
        }

        // Add a new mod
        public void AddMod(string filePath)
        {
            if (filePath != null)
            {
                _model.Mods.Add(new Mod(filePath));
                _view.ListViewMods.SelectedIndex = _view.ListViewMods.Items.Count - 1;
                ModSelected();
                SaveUserSettings();
                UpdateButtonsEnabled();
            }
        }

        // Remove a mod
        public void RemoveMod()
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                _model.Mods.RemoveAt(selectedMod);
                _view.ListViewParts.ItemsSource = null;
                SaveUserSettings();
                UpdateButtonsEnabled();
            }
        }

        // Add a UUID for a part
        public void AddPart(string uuid)
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                _model.Mods[selectedMod].PartUUIDs.Add(uuid);
                SaveUserSettings();
                UpdateButtonsEnabled();
            }
        }

        // Remove a UUID for a part
        public void RemovePart()
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                int selectedPart = _view.ListViewParts.SelectedIndex;
                if (selectedPart != -1)
                {
                    _model.Mods[selectedMod].PartUUIDs.RemoveAt(selectedPart);
                    SaveUserSettings();
                    UpdateButtonsEnabled();
                }
            }
        }

        // Update Parts ListView when a Mod is selected
        public void ModSelected()
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                _view.ListViewParts.ItemsSource = _model.Mods[selectedMod].PartUUIDs;
                UpdateButtonsEnabled();
            }
        }

        // Update button enabled status when a UUID Part is selected
        public void PartSelected()
        {
            UpdateButtonsEnabled();
        }

        // Set status message in the footer
        private async void FooterMessage(string message, bool isError = false)
        {
            _view.FooterText.Text = message;
            if (isError) _view.FooterPanel.Background = new SolidColorBrush(Colors.Tomato);
            else _view.FooterPanel.Background = new SolidColorBrush(Colors.Chartreuse);

            // delay for how long to display message before resetting footer text
            await Task.Delay(1000);
            _view.FooterText.Text = "";
            _view.FooterPanel.Background = new SolidColorBrush(Colors.LightGray);
        }

        // Update the enabled status of buttons
        private void UpdateButtonsEnabled()
        {
            // Check if "Generate IconMap" and "Export Icons" Buttons should be enabled
            if (_view.ListViewParts.Items.Count > 0)
            {
                _view.BtnGenerateIcons.IsEnabled = true;
                _view.BtnExportIcons.IsEnabled = true;
            }
            else
            {
                _view.BtnGenerateIcons.IsEnabled = false;
                _view.BtnExportIcons.IsEnabled = false;
            }

            // Check if "Remove Mod" and "Add Part" Buttons should be enabled
            if (_view.ListViewMods.SelectedItem != null)
            {
                _view.BtnRemoveMod.IsEnabled = true;
                _view.BtnAddPart.IsEnabled = true;
            }
            else
            {
                _view.BtnRemoveMod.IsEnabled = false;
                _view.BtnAddPart.IsEnabled = false;
            }

            // Check if "Remove Part" Button should be enabled
            if (_view.ListViewParts.SelectedItem != null)
            {
                _view.BtnRemovePart.IsEnabled = true;
            }
            else _view.BtnRemovePart.IsEnabled = false;
        }

        // Generate the new IconMap image with custom icons
        public void GenerateIcons()
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                string guiPath = _model.Mods[selectedMod].ModFilePath + "\\Gui\\"; // file path to mod GUI folder
                string iconMapPathOrig = guiPath + "IconMap.png"; // file path to original IconMap.png
                string iconMapPathCust = guiPath + "IconMap_Custom.png"; // file path to custom IconMap.png
                string iconXMLPath = guiPath + "IconMap.xml"; // file path to IconMap.xml
                string errors = ""; // error message listing reasons any icons failed
                int successCount = 0; // number of successful custom icons added
                bool saveNewOverride = false; // flag if saving over origianl file fails and new file will be saved instead

                try
                {
                    if (!File.Exists(iconXMLPath))
                    {
                        throw new Exception(); // if IconMap.xml wasn't found, throw exteption
                    }
                    Bitmap customIcon = null; // for custom icons
                    Bitmap iconMap = new Bitmap(iconMapPathOrig); // load the original iconMap image

                    using (Graphics gr = Graphics.FromImage(iconMap))
                    {
                        foreach (var partUUID in _model.Mods[selectedMod].PartUUIDs) // for each specified UUID in the Mod
                        {
                            try
                            {
                                // load the custom icon
                                customIcon = new Bitmap($"{guiPath}Custom\\{partUUID}.png");
                                int x = 0; // custom icon X location
                                int y = 0; // custom icon Y location 
                                bool xmlMatchFound = false;
                                var lines = File.ReadLines(iconXMLPath); // open the xml file
                                foreach (var line in lines)
                                {
                                    // if previous line matched, parse the coordinates from this line
                                    if (xmlMatchFound) 
                                    {
                                        /* Line will look like this example: <Frame point="74 74"/>
                                         * Following code gets index of the first quote and length to the second quote 
                                         * then splits a substring by the space and casts the numbers to X and Y ints */
                                        int substringStart = line.IndexOf('"') + 1;
                                        int substringLen = line.IndexOf('"', substringStart) - substringStart;
                                        string[] strCoords = line.Substring(substringStart, substringLen).Split(" ");
                                        x = int.Parse(strCoords[0]);
                                        y = int.Parse(strCoords[1]);
                                        break;
                                    }
                                    // if a match found in this line, set flag so next line will be parsed
                                    if (line.Contains(partUUID))
                                    {
                                        xmlMatchFound = true; 
                                    }
                                }

                                if (xmlMatchFound) // check if UUID match location was found in xml
                                {
                                    gr.SetClip(new Rectangle(x, y, 74, 74)); // mask off area where old icon was
                                    gr.Clear(System.Drawing.Color.Transparent); // clear the masked area
                                    gr.DrawImage(customIcon, x, y, customIcon.Width, customIcon.Height); // draw the new icon
                                    successCount++; // increment count of successful custom icons
                                }
                                else // display error message that UUID wasn't in the xml file
                                {
                                    errors += $"\n\nERROR: Could not find UUID {partUUID} in IconMap.xml";
                                }
                            }
                            catch (Exception) // catches exception if Bitmap for customIcon fails
                            {
                                errors += $"\n\nERROR: Could not find icon {partUUID}.png";
                            }
                            finally
                            {
                                if (customIcon != null) customIcon.Dispose(); // dispose customIcon if it was used
                            }
                        }
                    }

                    iconMap.Save(guiPath + "IconMap_Custom.png", ImageFormat.Png); // save new image as a copy
                    iconMap.Dispose(); // dispose Bitmap for iconMap

                    if ((bool)_view.RadioBtnReplace.IsChecked) // if image should be saved over original file...
                    {
                        try
                        {
                            File.Copy(iconMapPathCust, iconMapPathOrig, true); // copy the "copy" over the original
                            try
                            {
                                File.Delete(iconMapPathCust); // remove the "copy" (keeping the now altered original)
                            }
                            catch (Exception) // catches if removing the "copy" fails
                            {
                                // OK to silently fail here, leaving the "copy" doesn't hurt anything
                            }
                        }
                        catch (Exception e) // catches failure to replace original
                        {
                            saveNewOverride = true; // if saving over original failed, flag for error message
                        }
                    }

                    string completionMessage = $"{successCount} Custom Icons Sucessfully Added.{errors}"; // build completion message
                    if (saveNewOverride == true) // if original file failed to overwrite, add message that new file was made instead
                    {
                        completionMessage = "NOTE: Could not overwrite original IconMap.png image.\nA new copy has been saved instead.\n\n" + completionMessage;
                    }
                    MessageBox.Show(completionMessage, "Complete", MessageBoxButton.OK, MessageBoxImage.None); // display completion message
                }
                catch (Exception) // catches any exeptions thrown working with IconMap.xml or IconMap.png
                {
                    string errorMessage = "Could not generate IconMap.\nIconMap files may be missing or mod path may be incorrect.";
                    MessageBox.Show(errorMessage, "Failed", MessageBoxButton.OK, MessageBoxImage.None);
                }
            }
        }

        // Exports individual icons extracted from the IconMap.png
        public void ExportIcon()
        {
            int selectedMod = _view.ListViewMods.SelectedIndex;
            if (selectedMod != -1)
            {
                string guiPath = _model.Mods[selectedMod].ModFilePath + "\\Gui\\"; // file path to mod GUI folder
                string iconMapPathOrig = guiPath + "IconMap.png"; // file path to original IconMap.png
                string iconXMLPath = guiPath + "IconMap.xml"; // file path to IconMap.xml
                string exportPath = guiPath + "Exported"; // file path to export folder
                int successCount = 0; // number of icons successfuly exported
                int failCount = 0; // number of icons failed to exported

                if (!Directory.Exists(exportPath)) // create export directory if it doesn't exist
                {
                    Directory.CreateDirectory(exportPath); 
                }

                try
                {
                    if (!File.Exists(iconXMLPath))
                    {
                        throw new Exception(); // if IconMap.xml wasn't found, throw exteption
                    }

                    Bitmap iconMap = new Bitmap(iconMapPathOrig); // load the original iconMap image
                    Bitmap singleIcon = null; // for individual icons
                    string uuid = "";
                    int x = 0;
                    int y = 0;

                    var lines = File.ReadLines(iconXMLPath); // open the xml file
                    foreach (var line in lines) // parse xml file for 
                    {
                        if (uuid != "" && uuid != "Empty") // if previous line set a uuid
                        {
                            // parse line for coordinates
                            int substringStart = line.IndexOf('"') + 1;
                            int substringLen = line.IndexOf('"', substringStart) - substringStart;
                            string[] strCoords = line.Substring(substringStart, substringLen).Split(" ");
                            x = int.Parse(strCoords[0]);
                            y = int.Parse(strCoords[1]);

                            singleIcon = iconMap.Clone(new Rectangle(x, y, 74, 74), iconMap.PixelFormat); // crop icon from IconMap

                            try
                            {
                                singleIcon.Save($"{exportPath}\\{uuid}.png", ImageFormat.Png); // save individual icon
                                successCount++;
                            }
                            catch (Exception)
                            {
                                failCount++;
                            }

                            singleIcon.Dispose();
                            uuid = ""; // clear uuid
                        }
                        if (line.Contains("<I")) // lines containing the uuid start with "<I"
                        {
                            // parse line for uuid
                            int substringStart = line.IndexOf('"') + 1;
                            int substringLen = line.IndexOf('"', substringStart) - substringStart;
                            uuid = line.Substring(substringStart, substringLen);
                        }
                    }
                    
                    iconMap.Dispose(); // dispose iconMap
                    if (singleIcon != null) singleIcon.Dispose(); // dispose singleIcon

                    string completionMessage = $"{successCount} Icons Sucessfully Exported."; // build completion message
                    if (failCount > 0)
                    {
                        completionMessage += $"\n\nERROR: {failCount} Icons Failed to Export!";
                    }
                    MessageBox.Show(completionMessage, "Complete", MessageBoxButton.OK, MessageBoxImage.None); // display completion message
                }
                catch (Exception) // catches any exeptions thrown working with IconMap.xml or IconMap.png
                {
                    string errorMessage = "Could not export icons.\nIconMap files may be missing or mod path may be incorrect.";
                    MessageBox.Show(errorMessage, "Failed", MessageBoxButton.OK, MessageBoxImage.None);
                }
            }
        }
    }
}
