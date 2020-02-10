# ScrapMechanicCustomIcons
 Custom Icon Patch Tool for Scrap Mechanic
 Code by MJMBuilds

I created this tool to patch the generated icon maps with custom icons for specified items.

This is useful if you want to use some custom icons, but don't want to manually re-edit the icon map using an image editor every time you add something new to your mod.

![](screenshot01.png)

To use this tool, you would first generate your icon map using the official mod tool, then run this tool to patch the custom icons. 

The settings file stores selected Mods as file paths to the mod along with a list of UUIDs for the items which get custom icons. The settings file must remain in the same folder as the program executable.

The custom icons are to be put into a folder named "Custom" added inside the "Gui" folder of the mod.

The icons are to be png files named as the UUID. 
(Ex: ...\Mods\YourModName\Gui\Custom\123e4567-e89b-12d3-a456-426655440000.png )

When generating the patched icon map, this program opens the original IconMap.png image and IconMap.xml files, then steps through the list of UUIDS parsing the xml for the coordinates of that icon on the map.
A transparent "hole" is cut in the image, then the custom icon is added.
After all the custom icons have been patched in, the image is either saved over the original IconMap.png, or as a new file IconMap_Custom.png, depending on the option selected.

