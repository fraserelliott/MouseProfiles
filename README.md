# MouseProfiles

## What is this?
This is a small program Windows I created. I have an optical mouse and a trackball mouse and I find the trackball mouse wants a significantly faster speed than the optical mouse, so I made this to easily swap between them. This uses a binding to the Windows API to update the cursor speed in the same place you can update it in system systems. It stores the values for each profile in appdata and will load it upon opening and it reads the current cursor speed to determine if 1 of the 2 profiles is selected.

## How do I use this?

The inactive button is the currently active profile. There are 2 ways to set the cursor speed:
1. Move the slider for the currently active profile. This will update the cursor speed as you move it.
2. Click on the button for a profile to activate it.

This first version was made exactly to how I needed it to work. I may look to add more profiles in the future, which is why I store the settings in xml files to easily expand upon this in the future.

![MouseProfiles](https://github.com/user-attachments/assets/7faa6697-7b83-444b-98d4-0cad35ffc26d)

## Download

https://github.com/fraserelliott/MouseProfiles/releases/tag/1.0
