## What is this
I created this project a small while ago to do some simple tasks with the [DisplayPad](https://mountain.gg/keypads/displaypad/).
Originally I created this code just for myself, so it is not very polished nor does it adhere to any coding standards.
I also don't think, that I will ever expand or clean up the code, as this is pretty sufficient for me, but one never knows.
I added the project to GitHub, so other people can use it as a foundation for their projects, if they should decide to build something similar.

### Use this project as a guideline
You can use this project as a more sufficient alternative demo compared to Mountain's own [demo](https://github.com/Mountain-BC/DisplayPad.SDK.Demo).
For that you can refer to the following files:\
__Letter.cs:__ This class only creates bmp-images from text input. This is not relevant for you, like all other classes, except the following two:
__Program.cs:__ The program is basically a state machine. In this file you can see how I registered the three event listeners (you have to add all three, even if you only need one, hence the two empty methods) and delegate the onClick-events to the state machine.\
__Utils.cs:__ This file shows you, how to output a picture to the display pad. The method `UploadImageBySetIconPic` does that. It gets as input:
* a deviceId (to differentiate multiple DisplayPads (but I only have one DisplayPad, so I don't know whether that would actually work))
* a filepath to a `.bmp`-file (The picture you want to show on the pad. The resolution does not matter, it will be scaled accordingly. I did not find a method that accepts the data directly, so you will have to write everything to a file first.)
* The button index (0 for top left, 5 for top right, 11 for bottom right)
* A software/api/only/something-override-boolean which has no effect (according to my testing)
* A profileId, to specify on which profile the button image should be edited. Although it is of type string, the values sould be integers from 1 to n (so `"1"` for example). Those are the same numbers shown by the BaseCamp-GUI in the profiles-tab. However, no profile-parameter in any api-method seems to have an effect. So you can probably ignore this parameter as well.

Feel free to ask me any questions
