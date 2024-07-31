using System.Collections;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

/// <summary>
/// A class used to draw crude letters, made up by a few lines.
/// Note that there are C#-built-in solutions for this
/// </summary>
public class Letter {

  public class Line(int x1, int y1, int x2, int y2)
    {
      public readonly int x1 = x1, x2 = x2, y1 = y1, y2 = y2;

    public Line Move(int dx, int dy){
      return new Line(x1 + dx, y1 + dy, x2 + dx, y2 + dy);
    }
  }

  public static string WriteTextToFile(string text, Color color, string filename){
    CreateImage(TranslateText(text), color).Save(filename);
    return filename;
  } 

  public static Image CreateImage(ICollection<Line> lines, Color color){
    int max = lines.Count == 0 ? 2 : lines.Max(l => Math.Max(l.x1, l.x2));
    int may = lines.Count == 0 ? 2 : lines.Max(l => Math.Max(l.y1, l.y2));
    int diff = (max - may)/2;
    int margin = max > 800 ? 0 : max/7;
    max = Math.Max(max, may) + 2 * margin;
    Bitmap image = new(max, max);
    DrawText(lines, image, color, margin + Math.Max(0, -diff), margin + Math.Max(0, diff));
    return image;
  }

  public static void DrawText(ICollection<Line> lines, Image image, Color color, int x, int y){
    Graphics g = Graphics.FromImage(image);
    Pen pen = new(color, LINE_WIDTH) {
        EndCap = LineCap.Round,
        StartCap = LineCap.Round
    };
    foreach (Line line in lines){
      g.DrawLine(pen, line.x1+x, line.y1+y, line.x2+x, line.y2+y);
    }
    g.Flush();
  }

  public static ICollection<Line> TranslateText(string text){
    string[] lines = text.Split(' ', '\n', '\t');
    int max = Math.Max(lines.Max(l => l.Length), (int) (Math.Sqrt(text.Length) / CLEAR_WIDTH * CLEAR_HEIGHT * 1.25));
    for (int i = 1; i < lines.Length; i++){
      lines[i-1] += " ";
    }
    for (int i = 0; i < lines.Length-1; i++) {
      for (int j = i+1; j < lines.Length; j++) {
        if(lines[i].Length + lines[j].Length <= max+1){
          lines[i] = lines[i] + lines[j];
          lines[j] = "";
        } else {
          break;
        }
      }
    }
    return TranslateText(lines);
  }

  /**
   * translates the given text into series of 2d-lines.
   * \n-chars in the text are ignored, instead you should split the text into multiple strings
   */
  public static ICollection<Line> TranslateText(string[] lines){
    List<Line> list = [];
    for (int i = 0; i < lines.Length; i++) {
      for (int j = 0; j < lines[i].Length; j++) {
        list.AddRange(GetLinesFor(lines[i][j], j*CLEAR_WIDTH, i*CLEAR_HEIGHT));
      }
    }
    return list;
  }

  public static ICollection<Line> GetLinesFor(char c, int x, int y){
    Line[] original = SelectChar(c);
    Line[] lines = new Line[original.Length];
    for(int i=0; i < lines.Length; ++i) {
      lines[i] = original[i].Move(x, y);
    }
    return lines;
  }

  /**
   * selects a suitable blueprint for the char and translates that blueprint into a list of lines.
   * output should not be mutated
   */
  public static Line[] SelectChar(char c){
    return alphabet[(c < 96? c + 32 : c) % alphabet.Length];
  }

  /**
   * translates a blueprint (element of char or number array) into a set of lines
   */
  protected static ICollection<Line> Construct(String blueprint){
    if(blueprint.Length < 9){
      Console.WriteLine("'" + blueprint+ '\'');
    }
    LinkedList<Line> list = new();
    foreach(String code in pattern){
      for(int i = 0; i< PATTERN_SIZE; ++i){
        if(blueprint[i]!=code[0]){
          continue;
        }
        String targets = code[5..];
        for(int j=0; j < PATTERN_SIZE; ++j){
          if(targets.Contains(blueprint[j])){
            list.AddLast(new Line(toX(i), toY(i), toX(j), toY(j)));
          }
        }
      }
    }
    return list;
  }

  public static Line[][] ConstructStructures(params string[] blueprints){
    Line[][] result = new Line[blueprints.Length][];
    for (int i = 0; i < blueprints.Length; i++) {
      result[i] = [.. Construct(blueprints[i])];
    }
    return result;
  }

  protected static int toX(int index){
    return index % PATTERN_WIDTH * WIDTH_STRETCH;
  }

  /**
   * returns 0 for indices in the first line of a blueprint,
   * bigger numbers for other (lower) lines
   */
  protected static int toY(int index){
    return index / PATTERN_HEIGHT * HEIGHT_STRETCH;
  }

  public static float LINE_WIDTH = 6.829f;
  public static int LETTER_WIDTH = 36;
  public static int LETTER_HEIGHT = 54;
  public static int CLEAR_WIDTH = LETTER_WIDTH + 12;
  public static int CLEAR_HEIGHT = LETTER_HEIGHT + 14;

  protected static int PATTERN_WIDTH = 3;
  protected static int PATTERN_HEIGHT = 3;
  protected static int PATTERN_SIZE = PATTERN_HEIGHT * PATTERN_WIDTH;

  protected static int WIDTH_STRETCH = LETTER_WIDTH / (PATTERN_WIDTH-1);
  protected static int HEIGHT_STRETCH = LETTER_HEIGHT / (PATTERN_HEIGHT-1);

  /**
   * speciefies which symbols of a blueprint represent an start point of the line, and how the endpoints are marked
   */
  protected static readonly String[] pattern = [
      "X -> xyz",
      "Y -> yzZ",
      "Z -> xz",
      "O -> o",
      "G -> XO",
      "P -> poz",
      "F -> fp",
      "A -> Xx"
  ];

  protected static readonly Line[][] alphabet = ConstructStructures (
      "   " + 
      "   " +
      "   ", // space

      "  y" + // !
      "y  " +
      " Y ",

      " xo" + // "
      "XO " +
      "   ",

      "fox" + // #
      "y F" +
      "OAY",

      " OZ" + // $
      "x Y" +
      "yo ",

      "Zzo" + // %
      "Y A" +
      "OXx",

      "xOf"+ // &
      "o o"+
      "FOX",

      " Y " + // '
      " y " +
      "   ",

      "  X"+ // (
      " x "+
      "  X",

      "x  "+ // )
      " X "+
      "x  ",

      " x "+ // *
      "oXO"+
      "x x",

      " x "+ // +
      "o O"+
      " X ",

      "   "+ // ,
      " x "+
      "X  ",

      "   "+ // -
      "F f"+
      "   ",

      "   "+ // .
      "Xx "+
      "xX ",

      "  f"+ // /
      "   "+
      "F  ",

      "x X"+ // 0
      "   "+
      "X x",

      " X "+ // 1
      "x  "+
      "Oxo",

      " Zz"+
      "x P"+
      "o O",

      "x Z"+
      "pzP"+
      "O o",

      " xO"+
      "X x"+
      "  o",

      "Z x"+
      "zP "+
      "O o",

      " GO"+
      "X x"+
      "x Z",

      "x X"+
      " Oo"+
      " x ",

      "x X"+
      "O o"+
      "X x",

      "x X"+ // 9
      "FXz"+
      " pP",

      " ZX" + // :
      "xZG" +
      "oO ",

      " X " + // ;
      "   " +
      " x ",

      " ox" + // <
      "OX " +
      " ox",

      "F f" + // =
      "O o" +
      "X x",

      "xo " + // >
      " XO" +
      "xo ",

      " X " + // ?
      "xOG" +
      " o ",

       "X  "+  // @`
       " x "+
       "   ",

       " X "+ // A
       "O o"+
       "x x",

       /* "y  "+ */ "Z x"+
       /* "Z Y"+ */ "GX "+
       /* "X x", */ "Y y",

       " Xx"+
       "G  "+
       " Oo",

       "Zx "+
       "  X"+
       "Yy ",

       "o O"+ // E
       "Pp "+
       "z Z",

       "o O"+
       "Pp "+
       "z  ",

       " Xx"+
       "GpP"+
       " Oo",

       "F O"+ // H
       "Z z"+
       "f o",

       "FZf"+
       "   "+
       "Ozo",

       "Z z"+
       "O  "+
       " oP",

       "O x"+ // K
       "X  "+
       "o x",

       "x  "+
       "   "+
       "X x",

       "Z P"+
       " z "+
       "x o",

       "X Y"+ // N
       "   "+
       "x z",

       " xX"+ // o
       "Z  "+
       "Y y",

       "X G"+
       "o O"+
       "x  ",

       "y Y"+
       " OZ"+
       "Xxo",

       "X G"+ // R
       "ofO"+
       "x F",

       "O o"+
       " GX"+
       "Z x",

       "XOx"+ // T
       "   "+
       " o ",

       "o X"+
       "   "+
       "O G",

       "x x"+
       "   "+
       " X ",

       "x o"+ // W
       " z "+
       "Z P",

       "x O"+
       "   "+
       "o X",

       "x x"+
       " X "+
       " x ",

      "x X"+ // Z
      "o O"+
      "z Y",

      " Xx"+ // [{;
      " G "+
      " Oo",

      "Y  "+ // \|
      "   "+
      "  y",

      "xX "+ // ]}
      " G "+
      "oO ",

      " x "+ // ^~
      "X X"+
      "   ",

      "   "+ // _ DEL
      "   "+
      "X x"
  );
}

