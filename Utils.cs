
using System.Text.RegularExpressions;
using DisplayPad.SDK;

public static class Utils {

  /// <summary>
  /// 
  /// </summary>
  /// <param name="index">Button Index</param>
  /// <param name="text">Text to write</param>
  /// <param name="color">Textcolor</param>
  /// <param name="requester">state that called this function</param>
  public static void WriteTextToPad(int index, string text, Color color, ScreenState requester){
    requester.context.helper.UploadImageBySetIconPic(requester.DeviceID, Letter.WriteTextToFile(text, color, requester.context.BasePath + "Text.bmp"), index, true, requester.context.SandboxProfile);
  }

  /// <summary>
  /// Prints a simple selection menu on the screen
  /// </summary>
  /// <param name="other">the state that called this state</param>
  /// <param name="consumer">Lambda that reacts to the user's selection. The input paramter is the button/option-index</param>
  /// <param name="options">text-options to print to the buttons. Should not be more than 12 options</param>
  public class SelectionState(ScreenState other, Action<int> consumer, params string[] options) : ScreenState(other) {
      int LastPress = 3;

      public override void OnActivation(){
        for (int i = 0; i < 12 && Active; i++) {
          WriteTextToPad(i, i < options.Length ? options[i] : "",  Color.White, this);
        }
      }

      public override void OnClick(int i, int p) {
        WriteTextToPad(i, i < options.Length ? options[i] : "", p==0 ? Color.White : Color.Cyan, this);
        if (LastPress - p == 1 && i < options.Length) {
          consumer.Invoke(i);
        }
        LastPress = p;
      }
        public override string ToString() {
            return options.Aggregate("Selection from", (a, b) => a + " | " + b);
        }
    }


  public static async Task EmptyTask() {}

  // returns all elements from the stream that conform to the given regex. the given number is the legth of the postfix. the outputted number is the prefix number. The first string is the Infix. The second string is the whole original name
  public static IEnumerable<Tuple<int, string, string>> FilterParameters(this IEnumerable<string> stream, Tuple<Regex, int> regex) => 
    stream.Where(f => regex.Item1.IsMatch(f)).Select(f => f.Split(SoundScreen.REGEX_PREFIX_DELIMITER, 2)).Select<string[],Tuple<int, string, string>>(f => new(int.Parse(f[0]), f[1][.. ^regex.Item2], f[0] + SoundScreen.REGEX_PREFIX_DELIMITER + f[1])).Where(f => f.Item1 < 12);

  public class StartScreen(Program context, int DeviceID) : ScreenState(context, DeviceID) {
    public override void OnActivation() {/*nothing to draw here, basecamp should draw the start screen*/}

    public override void OnClick(int index, int pressed) {
      if(index == 6 && pressed == 0){
        context.helper.DisplayPadSwitchProfile(context.SandboxProfile.ToString(), DeviceID);
        context.SetState(new SelectionState(this, i => {
          switch (i) {
            case 0: context.helper.DisplayPadSwitchProfile(context.BaseCampProfile.ToString(), DeviceID); context.SetState(this); break;
          }
        }, "Go Back :","..."));
      }
    }
  }

  
  public class StartScreen2(Program context, int DeviceID) : ScreenState(context, DeviceID) {
    public override void OnActivation() {WriteTextToPad(0, "Click Anywhere", Color.Gray, this);}

    public override void OnClick(int index, int pressed) {
      if(pressed == 0){
        ScreenState mainMenu = new SoundScreen(this, context.BasePath + "Sounds");
        context.SetState(mainMenu);
      }
    }
  }

    // a crocodile that works in the water and harms its surrounding flora in an abrasive manner
  public class FileTreeNavyGator(ScreenState parent, string path, Dictionary<int, Tuple<string, Action<int>>> extraOptions) : ScreenState(parent) {

    public override void OnActivation() {
      Color c = Color.BurlyWood;
      string[] options = new string[12];
      Directory.EnumerateDirectories(path);
    }

    public override void OnClick(int index, int pressed){
      throw new NotImplementedException();
    }
  }

    static void OnClick(int KeyMatrix, int iPressed, int DeviceID) {
            if(iPressed == 4){
                Letter.TranslateText("hallo");
                //nhelper.UploadImageBySetIconPic(DeviceID, Letter.WriteToFile("Ist das boßhaft?", "Hallo"+".bmp"), KeyMatrix, true, 0);

                //Console.WriteLine("a ");
            }
            if (iPressed == 0){
                for(int i=0; i<=10; i++){
                    //helper.UploadImageBySetIconPic(DeviceID, Letter.WriteToFile("Flugzeug "+i+" Zug", "Hallo.bmp"), i, true, 0);
                }
                // helper.UploadImageBySetIconPic(DeviceID, Letter.WriteToFile("Key "+KeyMatrix, "Hallo.bmp"), GetIndex(KeyMatrix), true, 0);
            }
            
            // bool r = helper.DisplayPadAPEnable("true", DeviceID);
            Console.WriteLine("Key matrix: " + KeyMatrix + " Key status: " + iPressed + " Profile: " + 8 + " for Device Id: " + DeviceID.ToString());
            
      //       bool switched = context.helper.DisplayPadSwitchProfile(a.ToString(), DeviceID);
      //  Console.WriteLine("Switched to "+ a+ "  " +switched);
      //  Console.WriteLine("c "+context.helper.DisplayPadGetFWInfo(DeviceID).currentlyProfileIndex);
      //    a++;
      //  if(a == 16)
      //    a = -10;
      //  if(a <= 100)
      //    return;
        }
}
