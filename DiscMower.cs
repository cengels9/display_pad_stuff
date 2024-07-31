using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DisplayPad.SDK;
using System.IO;

/// <summary>
/// DiscordMover
/// A discord bot, that quickly moves users from channel to channel with the press of a button
/// </summary>
public class DiscMower : ScreenState {
  readonly ScreenState predecessor; readonly DiscordSocketClient client; readonly ulong g0; readonly ulong c1; readonly ulong c2;
  /// <summary>
  /// 
  /// </summary>
  /// <param name="predecessor">The state that created or invoked this state. Some kind of menu</param>
  /// <param name="client">The connection to the discord API</param>
  /// <param name="guild">The server-id on which the bot agitates</param>
  /// <param name="channel1">Channel A</param>
  /// <param name="channel2">Channel B</param>
  public DiscMower(ScreenState predecessor, DiscordSocketClient client, ulong guild, ulong channel1, ulong channel2) : base (predecessor){
    this.g0 = guild; this.client = client; this.predecessor = predecessor; this.c1 = channel1; this.c2 = channel2;

    client.UserVoiceStateUpdated += (user, oldState, newState) => {
      RegisterUserToChannel(user, newState.VoiceChannel);
      return Utils.EmptyTask();
    };
  }

  /// <summary>
  /// Tracks which user is currently registered to which button
  /// </summary>
  readonly Dictionary<ulong, int> indices = [];
  /// <summary>
  /// Tracks which button is currently registered to which user.
  /// Null -> there is no user for this button currently
  /// Redundand with the Dict above
  /// </summary>
  readonly ulong?[] users = new ulong?[12];
  /// <summary>
  /// Tracks in which channel each relevant user is. 
  /// Null -> there is no user or the user is not connected to any voice channel
  /// </summary>
  readonly ulong?[] channels = new ulong?[12];

  /// <summary>
  /// The file stores prefereed positions for (discord-)users. If one user was registered to button 4 last time, and button 4 ist still free this time, the user will bbe registered to that button again
  /// </summary>
  readonly private string PREFERENCE_FILE = "UserIndices.txt";
  /// <summary>
  /// Stores an user-to-button mapping history. 
  /// Key: user-id
  /// Value: button indices to which this user has been mapped to in the past
  /// </summary>
  private static Dictionary<ulong, List<int>>? _preferences;
  Dictionary<ulong, List<int>> Preferences{
    get => _preferences ??= File.ReadAllLines(predecessor.context.BasePath + PREFERENCE_FILE).Select<string, KeyValuePair<ulong, List<int>>>(line => {
      var numbers = line.Split(' ');
      return new(Convert.ToUInt64(numbers[0]), numbers.Skip(1).Select(s => Convert.ToInt32(s)).ToList());
    }).ToDictionary();
  }

  private void SavePreferences(){
    File.WriteAllLines(predecessor.context.BasePath+PREFERENCE_FILE, Preferences.AsEnumerable().Select(e => e.Value.Select(n => n.ToString()).Aggregate(e.Key.ToString(), (a, b) => a + " " + b)));
  }

  public override void OnActivation() {
    // in the top left corner print the "return to menu" option
    Utils.WriteTextToPad(0, ": Go Quack", System.Drawing.Color.BurlyWood, this);
    // in the remaining scrrens print placeholders
    for (int i = 1; i<12 && Active; ++i){
      Utils.WriteTextToPad(i, "_", System.Drawing.Color.DarkBlue, this);
    }
    // scan for connected users. Since there are only 11 remaining buttons, prefer those in channel1, then cannel2 then other channels
    foreach(var channel in client.GetGuild(g0).VoiceChannels.OrderBy(c => c.Id == c1 ? 0 : c.Id == c2 ? 1 : 2)){
      foreach(var user in channel.ConnectedUsers){
        RegisterUserToChannel(user, channel);
      }
    }
  }

  int presses = 1;

  public override void OnClick(int i, int pressed) {
    presses += pressed + pressed - 1;
    if(pressed == 1){
      // only react on key-up
      return;
    }
    if (i == 0){
      // return to start screen
      context.SetState(predecessor);
      SavePreferences();
      presses = 1;
      return;
    }
    if(users[i] == null){
      // there is no user associated with this button
      return;
    }
    if(presses == 1){
      // switch users channels
      MoveMember(i, channels[i] == c1 ? c2 : c1);
      return;
    }
    var otherChannel = channels[i];
    for (int j = 1; j < 12; ++j) {
      // 2 presses: move every existing user if it is in a different channel
      // 3 presses: move every existing user unconditionally
      if(users[j] != null && (pressed != 2 || (channels[j] != null && channels[j] != otherChannel))){
        MoveMember(j, pressed == 2 ? otherChannel??c1:c1);
      }
    } // I have an impression, that this last loop might not work, but without multiple connected people I lack test data
  }

  // changes a guild member's voice state. 
  // it also draws a new intermediate button image. 
  // on successful movement, a corresponding voice-state-change-observer should update the image again. 
  // i: index of a button that corresponds to an existing user
  // c: target channel
  async void MoveMember(int i, ulong c){
    var user = client.GetGuild(g0).GetUser(users[i]??0xFA1L);
    Utils.WriteTextToPad(i, user.GlobalName + " moving", System.Drawing.Color.Gray , this); 
    var channel = client.GetGuild(g0).GetVoiceChannel(c);
    
    await client.GetGuild(g0).MoveAsync(user, channel);
  }

  /// <summary>
  /// Gets called when a new discord connection info is available.
  /// Registers the respective data to the local maps and arrays and also updates the display pad screen
  /// </summary>
  /// <param name="user">the discord user</param>
  /// <param name="channel">the voice channel to which te user is connected now, or null, if the user is not connected</param>
  private void RegisterUserToChannel(SocketUser user, SocketVoiceChannel? channel){
    if(!Active){
      return;
    }
    // user is not known yet (and user did not disconnect - in that case we don't care)
    if(!indices.TryGetValue(user.Id, out int index) && channel != null){
      Preferences.TryGetValue(user.Id, out var preferedIndices);
      Preferences[user.Id] = preferedIndices ??= [];
      // j = target index
      int j = -1;
      // assign the user one of the preferred button indices
      foreach(var i in preferedIndices){
        if(channels[i] == null){
          j = i;
          break;
        }
      }
      // assign the user to one of the free button indices
      for(int i = 1; i < 12 && j == -1; ++i){
        if(channels[i] == null){
          j = i;
          preferedIndices.Add(j);
        }
      }
      // every space is full, this user will be ignored
      if(j == -1){
        return;
      }
      channels[j] = channel.Id;
      users[j] = user.Id;
      indices[user.Id] = index = j;
    }
    // the main part of the method
    var newChannel = channel?.Id;
    Utils.WriteTextToPad(index, user.GlobalName + " " + (channel?.Name??"Disconnected"), 
    newChannel == null ? System.Drawing.Color.DarkViolet
    : newChannel == c1 ? System.Drawing.Color.Green
    : newChannel == c2 ? System.Drawing.Color.Red 
    : System.Drawing.Color.Yellow, this);
    channels[index] = newChannel;
    Console.WriteLine("user "+user.GlobalName+" moved to "+newChannel??"DC");
  }

  /* --  --  -- */
  /* -- Init -- */
  /* --  --  -- */

  /// <summary>
  /// Stores singletons of this class, one for each display pad
  /// </summary>
  public static readonly Dictionary<int,DiscMower> mover = [];

  // reads the token from a file
  // if there are multiple files, this thing will prompt a user's reaction
  public static void SetupAndSelectToken(ScreenState source, bool reuse){
    /* Restore previous discord handler
    */
    if(reuse && mover.TryGetValue(source.DeviceID, out DiscMower? value)) {
      source.context.SetState(value);
      return;
    }
    /* Create new discord handler
    */
    String path = source.context.BasePath + "token/"; // path to the token directory
    if(!Directory.Exists(path)){
      Directory.CreateDirectory(path);
    }
    string file = path + "/Bot1.txt";
    switch(Directory.EnumerateFiles(path).Count()){
      case 0: 
        File.Create(file).Close();
        File.AppendAllText(file, "Replace this text by your token"); 
        source.context.SetState(new Utils.SelectionState(source, i => {if(i==4) LoginAndSelectGuild(source, Directory.EnumerateFiles(path).FirstOrDefault(file)); else if(i==5) source.context.SetState(source);}, "Please go to", file, "and insert your token", "", "Ok, done", "Cancel", "=", "=", "=", "=", "=", "="));
        break;
      case 1:
        LoginAndSelectGuild(source, Directory.EnumerateFiles(path).FirstOrDefault(""));
        break;
      default:
        string[] files = Directory.EnumerateFiles(path).ToArray();
        string[] names = new string[files.Length];
        for(int i = 0; i < files.Length; ++i){
          names[i] = files[i].Split('.')[0].Split('/').Last();
        }
        source.context.SetState(new Utils.SelectionState(source, i => LoginAndSelectGuild(source, files[i]), names));
        break;
    }
  }

  // initializes the discord client and requests a list of all guild available to the bot
  // creates a selection dialogue to choose one guild
  private async static void LoginAndSelectGuild(ScreenState source, string tokenFile){
    // draw blank screen while waiting for discord setup
    source.context.SetState(new Utils.SelectionState(source, i => {}));

    DiscordSocketClient client = new DiscordSocketClient();

    await client.LoginAsync(TokenType.Bot, File.ReadAllText(tokenFile));
    await client.StartAsync();
    var guilds = await client.Rest.GetGuildsAsync();
    if(guilds.Count == 0){
      source.context.SetState(new Utils.SelectionState(source, i => {if(i == 7) source.context.SetState(source);}, "Your", "bot", "isn't", "registered", "to any", "guild yet",  "", "ok"));
      return;
    }
    List<Tuple<string, ulong>> nameAndIDs = [];
    foreach (var guild in guilds) {
      nameAndIDs.Add(new(guild.Name, guild.Id));
    }
    source.context.SetState(new Utils.SelectionState(source, i => {
      StartAndSelectChannels(source, client, nameAndIDs[i].Item2);
    }, nameAndIDs.Select(pair => pair.Item1).ToArray()));
  }

  // Allows one to select two voicechannels
  // finally starts the regular discord state
  private static void StartAndSelectChannels(ScreenState source, DiscordSocketClient client, ulong guildID){
    var nameAndIDs = client.GetGuild(guildID).VoiceChannels.Select<SocketVoiceChannel, Tuple<string, ulong>>(c => new(c.Name, c.Id)).ToArray();
    if(nameAndIDs.Length < 2){
      source.context.SetState(new Utils.SelectionState(source, i => {if(i == 7) source.context.SetState(source);}, "There", "are not", "enough", "channels", "on this", "guild/ server", "", "ok"));
      return;
    }
    int[] selected = [-1];
    source.context.SetState(new Utils.SelectionState(source, i => {
      if (selected[0] == -1){
        selected[0]= i;
        Utils.WriteTextToPad(i, nameAndIDs[i].Item1, System.Drawing.Color.DarkGreen, source);
      } else if (selected[0] == i) {
        // redrawing of the image is handeled by the selection state
        selected[0] = -1;
      } else {
        mover[source.DeviceID] = new DiscMower(source, client, guildID, nameAndIDs[selected[0]].Item2, nameAndIDs[i].Item2);
        source.context.SetState(mover[source.DeviceID]);
      }
    }, nameAndIDs.Select(p => p.Item1).ToArray()));
  }
}
