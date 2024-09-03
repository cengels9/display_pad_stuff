## What is this
I created this project a small while ago to do some simple tasks with the [DisplayPad](https://mountain.gg/keypads/displaypad/).
Originally I created this code just for myself, so it is not very polished nor does it adhere to any coding standards.
I also don't think, that I will ever expand or clean up the code, as this is pretty sufficient for me, but one never knows.
I added the project to GitHub, so other people can use it as a foundation for their projects, if they should decide to build something similar.

### Use this project as a guideline
You can use this project as a more sufficient alternative demo compared to Mountain's own [demo](https://github.com/Mountain-BC/DisplayPad.SDK.Demo).
For that you can refer to the following files:\
* __Letter.cs:__ This class only creates bmp-images from text input. This is not relevant for you, like all other classes, except the following two:
* __Program.cs:__ The program is basically a state machine. In this file you can see how I registered the three event listeners (you have to add all three, even if you only need one, hence the two empty methods) and delegate the onClick-events to the state machine.\
* __Utils.cs:__ This file shows you, how to output a picture to the display pad. The method `UploadImageBySetIconPic` does that. It gets as input:
  - a deviceId (to differentiate multiple DisplayPads (but I only have one DisplayPad, so I don't know whether that would actually work))
  - a filepath to a `.bmp`-file (The picture you want to show on the pad. The resolution does not matter, it will be scaled accordingly. I did not find a method that accepts the data directly, so you will have to write everything to a file first.)
  - The button index (0 for top left, 5 for top right, 11 for bottom right)
  - A software/api/only/something-override-boolean which has no effect (according to my testing)
  - A profileId, to specify on which profile the button image should be edited. Although it is of type string, the values sould be integers from 1 to n (so `"1"` for example). Those are the same numbers shown by the BaseCamp-GUI in the profiles-tab. However, no profile-parameter in any api-method seems to have an effect. So you can probably ignore this parameter as well.

Feel free to ask me any questions

### Use this projekt like I do
I created the program to bind funtionality to the displaypad.
This functionality revolves around two things: Moving people along voice channels using a discord bot and playing audio clips.
I understand that this are very distinct aund niche things, so I doubt that anyone would want do the same with their displaypad.
But in case anyone does, I will describe how to do that in the following.

First of all, you can of course do only one of those two things with this software and safely ignore the rest.
###### Layout
Configuring the button layout is done via a directory, which, in this case is [this](https://github.com/cengels9/display_pad_stuff/tree/main/Sounds).
That is the base directory for the layout.
It's parent directory should be the first program argument.
To that directory you can add files and subdirectories.
Each of that entity corresponds to one button on the display pad (after you pressed the first button, all options of the base directory will be drawn to the displaypad).
They have to conform to the RegExes specified [here](https://github.com/cengels9/display_pad_stuff/blob/main/SoundScreen.cs), which means they will follow this format: `index`_`name`.`file ending`.\
`index` ranges from 0 to 11 and specifies which button on the diplaypad is referred by the file.\
`name` is the string that will be written to the displaypad.\
`file ending` specifies what should be done, when the button is pressed. There are several options for this:
* `_sounds` (only for directories) Clicking the corresponding button will draw all options of the subdirectory to the display pad.
* `.back` return to the previous state (usually the parent-directory) (the file's content does not matter)
* `.sync` scan for changed files and directories in the current directory or any subdirectory and update the button images accordingly (the file's content does not matter).
* `.wav` or `.mp3` play the sound of the file to the specified outputs see more below
* `.dNew` transfers you to a discord-move-bot-control-panel:
  1. Select a token from a token file if there is no such file, the program will create such a file and promt you to enter your token (Where do you get a token? - Go [here](https://discord.com/developers/applications), create an application, and a bot for your application, copy its token and add the bot to your server). You can select from multiple tokens by creating multiple token files.
  2. Select on which discord server you want to agitate (only necessary if the bot is connected to multiple servers)
  3. Select two voice channels on that server
  4. At next, the displaypad will draw all server members that are currently connected to the voice chat. One person per button (so not more than 12 people). The text color changes depending on the channel the person is connected to. Clicking a person-button moves the person to a different channel (one of those specified in step 3). Clicking on a person while already holding an other button will move every person to that voice-channel. Clicking on a person while already holding two other buttons will move every person to the first specified voice channel.
* `.dOld` Does the same as `.dNew`, it just skips the steps 1 - 3, if you already performed them at least once.

##### how do I specify sound outputs?
Clicking a sound-playback-button will play a sound.
The second, third, fourth and fivth program arguments specify to which output the sound will be played.
The second and fourth argument are substrings contained in the output's name.
The third and fivth argument are volume multiplicators for the corresponding output.
I specified two outputs for myself: My default audio-output (for monitoring) and [Voicemeeter](https://vb-audio.com/Voicemeeter/). 
Voicemeeter allows you to map an audio output to an audio input, which means, that you can use the displaypad as a soundboard (if you use the voicemeeter as microphone input).

For this chapter also:\
Feel free to ask me any questions


