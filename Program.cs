using DisplayPad.SDK;
using DisplayPad.SDK.Helper;

/// <summary>
/// A state machine that delegates button-presses to substates.
/// Can do multiple things, but mainly it's a soundboard.
/// </summary>
/// <param name="helper">Mountains API</param>
/// <param name="basePath">A file path for local files</param>
/// <param name="channelAndVolume">Output channel names for the sounboard. And respective volume multipliers</param>
public class Program(DisplayPadHelper helper, String basePath, Tuple<String, float>[] channelAndVolume)
{
    /**
     * when button 0 gets pressed, key matrix is 8. For example.
     */
    private static int[] matrixFromPad = [
        8, 17, 26, 35, 44, 53,
        62, 71, 80, 89, 98, 125 
    ];

    /**
     * Why does the pad output these weird numbers?
     * matrixToPad[KeyMatrix & 15] is equivalent to Math.Max(KeyMatrix/9, 11)
     */
    private static readonly int[] matrixToPad = [8, 1, 10, 3, -12, 5, -14, 7, 0, 9, 2, -11, 4, 1_1, 6, -15];

    // usage: 
    // <BasePath:String> <Channel1:String> <VolumeMultiplier1:float> <Channel2:String> <Volume2:float> ... <ChannelN:String> <VolumeN:float>
    private static void Main(string[] args) {
        args = ["C:\\Users\\engel\\Desktop\\UHU", "Default", "1.0", "Voicemeeter Input", "1.5"];
        List<Tuple<string, float>> channels = [];
        for(int i = 2; i < args.Length; i+=2){
            if(float.TryParse(args[i], out float f)){
                channels.Add(new(args[i-1], f));
            } else {
                Console.WriteLine("Volume "+args[i]+" for channel "+args[i-1]+" is not a float");
            }
        }
        if(channels.Count == 0){
            channels.Add(new("Default", 1.0f));
        }
        if(channels.Count == 1){
            channels.Add(new("VoiceMeeter", 1.5f));
        }
        Program context = new(new DisplayPadHelper(), args.Select(s => s+"//").FirstOrDefault(""), [.. channels]);

        //Event will fire when any key is pressed on the device
        DisplayPadHelper.DisplayPadKeyCallBack += OnClick;
        DisplayPadHelper.DisplayPadPlugCallBack += IgnorePlug;
        DisplayPadHelper.DisplayPadProgressCallBack += IgnoreProgress;

        // delegates a button-click to the state machine
        // KeyMatrix: Mountains-Button-Identifyer. See matrixFromPad. 
        // iPressed: 1 for press, 0 for release
        // DeviceID: Probably necessary when there are multiple display-pads connected to the pc
        void OnClick(int KeyMatrix, int iPressed, int DeviceID) {
            try {
            if(!context.DisplayStates.TryGetValue(DeviceID, out ScreenState? handler))
            {
                handler = new Utils.StartScreen2(context, DeviceID).SetActive(true);
                context.DisplayStates[DeviceID] = handler;
            }
            handler.OnClick(matrixToPad[KeyMatrix & 15], iPressed);
            } catch (Exception e){
                Console.WriteLine(e);
            }
        }

        Console.WriteLine("Click anywhere on the display-pad to start.");
    }

    private static void IgnoreProgress(int Percentage){}

    private static void IgnorePlug(int Status, int DeviceID){}

    private readonly Dictionary<int, ScreenState> DisplayStates = [];

    // the API-Interface
    public readonly DisplayPadHelper helper = helper;
    public Tuple<String, float>[] ChannelAndVolume {
        get => channelAndVolume;
    }
    public string BasePath {
        get => basePath;
    }
    public readonly int SandboxProfile = 0;
    public readonly int BaseCampProfile = 1;

    public void SetState(ScreenState handler){
        DisplayStates[handler.DeviceID].SetActive(false);
        DisplayStates[handler.DeviceID] = handler;
        handler.SetActive(true);
    }
}

/**
 * This class is a state for the automaton that handles the display pad with the respective device id
 * The "context" parameter can be used to change the state of the automaton or to access the displaypadhelper
 */
public abstract class ScreenState (Program Context, int DeviceID){

    public ScreenState(ScreenState pattern):
        this(pattern.context, pattern.DeviceID){}

    readonly public Program context = Context;

    /**
     * the device, this handler is responsible for
     */
    readonly public int DeviceID = DeviceID;

    /**
     * changes the active status, that determines whether the handler is allowed to perform actions non concurrently with the on-click-method
     */
    protected bool Active = false;

    /**
     * handles a button press of the assigned display pad. One can assume, that Active=true, when this method is called.
     * index: which button was pressed. Goes from 0 to 11
     * pressed: 0 -> button was just released, 1 -> button was just pressed
     */
    public abstract void OnClick(int index, int pressed);

    /**
     * Gets called when this object gains control over the screen.
     * One can safely assume, that Active=true, when this is called.
     * In this phase, the handler should draw images on the front space.
     */
    public abstract void OnActivation();

    // my most cursed method
    internal ScreenState SetActive(bool active) {
        if (Active = active)
        OnActivation();
        return this;
    }
}
