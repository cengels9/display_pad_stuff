using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Devices;
using Soundboard.Lib.Services;

/// <summary>
/// Prints a small soundbord onto the display pad.
/// There are multiple options for the buttons:
/// - invoke a discord bot
/// - play a sound
/// - go to an other directory
/// - sync all (after changing the directory structure)
/// To configure the buttons, create a file structure with files and directories whose names match the corresponding regexes.
/// For the most part, only the filename is relevant, the file content is only relevant for sound files
/// </summary>
public class SoundScreen : ScreenState {

  public static readonly char REGEX_PREFIX_DELIMITER = '_'; // <-  when changing this, also change regexes below -v
  static readonly Tuple<Regex, int> SOUND_SUBDIRECTORY_MATCHER = new(new("^\\d+_.*?_sounds$"), 7);
  static readonly Tuple<Regex, int> DISCORD_SETUP_MATCHER = new(new("^\\d+_.*?\\.d(Old|New)$"), 5);
  static readonly Tuple<Regex, int> GO_BACK_MATCHER = new(new("^\\d+_.*?\\.back$"), 5);
  static readonly Tuple<Regex, int> MUSIC_MATCHER = new(new("^\\d+_.*?\\.[MmWw][PpAa][3Vv]$"), 4);
  static readonly Tuple<Regex, int> UPDATE_MATCHER = new(new("^\\d+_.*?\\.sync$"), 5);
  static readonly Regex VOLUME_MATCHER = new(".*?volumes.txt$");

  // a regex for selecting files and the length of the postfix inluding the delimiter 


  // interface for the bass library
  readonly AudioManagerService audio;
  // lambdas for the 12 buttons. Default initialzation with empty text
  readonly ButtonHandler[] buttons = new int[12].Select(i => new ButtonHandler("", Color.Black, () => {})).ToArray();
  // volume multipliers for each sound. default is 1
  readonly float[] volume = Enumerable.Repeat(1f, 12).ToArray();

  public SoundScreen(ScreenState predecessor, string directory) : base(predecessor){
    audio = new AudioManagerService(directory, context.ChannelAndVolume[0].Item1, context.ChannelAndVolume[1].Item1);
    //foreach(var t in GetFileNames(directory, true).Where(f => f.StartsWith(SOUND_SUBDIRECTORY_MATCHER)).Select<string, Tuple<string, string[]>>(f => new(f, f.Substring(SOUND_SUBDIRECTORY_MATCHER.Length).Split('_'))).Select<Tuple<string, string[]>, Tuple<string, int, string>>(f => new(f.Item1, int.Parse(f.Item2[0]), f.Item2[1]))) {
    foreach(var t in GetFileNames(directory, true).FilterParameters(SOUND_SUBDIRECTORY_MATCHER)){
      SoundScreen successor = new(this, directory + "//" + t.Item3);
      buttons[t.Item1] = new ButtonHandler(t.Item2, Color.BlueViolet, () => context.SetState(successor));
    }
    foreach(var t in GetFileNames(directory).FilterParameters(DISCORD_SETUP_MATCHER)){
      buttons[t.Item1] = new ButtonHandler(t.Item2, Color.Coral, () => DiscMower.SetupAndSelectToken(this, t.Item3.EndsWith('d')));
    }
    foreach(var t in GetFileNames(directory).FilterParameters(GO_BACK_MATCHER)){
      buttons[t.Item1] = new ButtonHandler(t.Item2.Replace('#', ':'), Color.Beige, () => context.SetState(predecessor));
    }
    foreach(var t in GetFileNames(directory).FilterParameters(UPDATE_MATCHER)){
      // reinitialize all subdirectories
      buttons[t.Item1] = new ButtonHandler(t.Item2, Color.Red, () => {
        var successor = new SoundScreen(predecessor, directory);
        context.SetState(successor);
        DiscMower.mover.Remove(DeviceID);
      });
    }
    foreach(var t in GetFileNames(directory).FilterParameters(MUSIC_MATCHER)){
      buttons[t.Item1] = new ButtonHandler(t.Item2, Color.LimeGreen, () => {
        audio.PlayAudio(t.Item3[..^MUSIC_MATCHER.Item2], volume[t.Item1] * context.ChannelAndVolume[1].Item2, volume[t.Item1] * context.ChannelAndVolume[0].Item2);
      });
    }
    foreach(var t in GetFileNames(directory).Where(f => VOLUME_MATCHER.IsMatch(f)).SelectMany(f => File.ReadAllLines(directory + "//" + f)).Select(l => l.Split(' ', 3)).Where(l => l.Length > 1)){
      if(int.TryParse(t[0], out int index) && index >= 0 && index < 12 && float.TryParse(t[1], out float volume)){
        this.volume[index] = volume;
      }
    }
  }

  /// <summary>
  /// lists the names of entries in this directory
  /// </summary>
  /// <param name="directory">absolute path to the directory</param>
  /// <param name="isDir">if true, scans for directories only, and only for files elseways</param>
  /// <returns>names, without any separator, but with file ending</returns>
  public IEnumerable<string> GetFileNames(string directory, bool isDir = false) {
    Func<string, IEnumerable<string>> func = isDir ? Directory.EnumerateDirectories : Directory.EnumerateFiles;
    return func.Invoke(directory).Select(f => f.Split('/', '\\').Last());
  }

  public override void OnActivation() {
    audio.CanAudioOverlap = true;
    for (int i = 0; i < buttons.Length && Active; i++) {
      Utils.WriteTextToPad(i, buttons[i].Text, buttons[i].GetValue(0), this);
    }
  }

  public override void OnClick(int i, int pressed) {
    Utils.WriteTextToPad(i, buttons[i].Text, buttons[i].GetValue(pressed), this);
    buttons[i].PerformClick(pressed);
  }

  public override string ToString() => buttons.Select(b => b.Text).Aggregate((a, b) => a +", "+ b);
    
  internal class ButtonHandler(string text, Color color, Action consumer){
    public string Text {
      get => text;
    }
    
    public Color GetValue(int pressed) => pressed == 1 ? Color.DimGray : color;

    public void PerformClick(int pressed){
      if(pressed == 0){
        consumer.Invoke();
      }
    }
  }
}