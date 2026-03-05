using NLog;
using System.Reflection;
using System.Text.Json;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

// deserialize mario json from file into List<Mario>
string marioFileName = "mario.json";
List<Mario> marios = [];
// check if file exists
if (File.Exists(marioFileName))
{
  marios = JsonSerializer.Deserialize<List<Mario>>(File.ReadAllText(marioFileName))!;
  logger.Info($"File deserialized {marioFileName}");
}

do
{
  // display choices to user
  Console.WriteLine("1) Display Mario Characters");
  Console.WriteLine("2) Add Mario Character");
  Console.WriteLine("3) Remove Mario Character");
  Console.WriteLine("Enter to quit");

  // input selection
  string? choice = Console.ReadLine();
  logger.Info("User choice: {Choice}", choice);

  if (choice == "1")
  {
    // Display Mario Characters
     foreach(var c in marios)
    {
      Console.WriteLine(c.Display());
    }
  }
  else if (choice == "2")
  {
    // Add Mario Character
     // Generate unique Id
    Mario mario = new()
    {
      Id = marios.Count == 0 ? 1 : marios.Max(c => c.Id) + 1
    };
    InputCharacter(mario);
     // Add Character
    marios.Add(mario);
    File.WriteAllText(marioFileName, JsonSerializer.Serialize(marios));
    logger.Info($"Character added: {mario.Name}");
  }
  else if (choice == "3")
  {
    // Remove Mario Character
       Console.WriteLine("Enter the Id of the character to remove:");
    if (UInt32.TryParse(Console.ReadLine(), out UInt32 Id))
    {
      Mario? character = marios.FirstOrDefault(c => c.Id == Id);
      if (character == null)
      {
        logger.Error($"Character Id {Id} not found");
      } else {
         marios.Remove(character);
        // serialize list<marioCharacter> into json file
        File.WriteAllText(marioFileName, JsonSerializer.Serialize(marios));
        logger.Info($"Character Id {Id} removed");
      }
    } else {
      logger.Error("Invalid Id");
    }
  } else if (string.IsNullOrEmpty(choice)) {
    break;
  } else {
    logger.Info("Invalid choice");
  }
} while (true);

logger.Info("Program ended");

static void InputCharacter(Character character)
{
  Type type = character.GetType();
  PropertyInfo[] properties = type.GetProperties();
  var props = properties.Where(p => p.Name != "Id");
  foreach (PropertyInfo prop in props)
  {
    if (prop.PropertyType == typeof(string))
    {
      Console.WriteLine($"Enter {prop.Name}:");
      prop.SetValue(character, Console.ReadLine());
    } else if (prop.PropertyType == typeof(List<string>)) {
      List<string> list = [];
      do {
        Console.WriteLine($"Enter {prop.Name} or (enter) to quit:");
        string response = Console.ReadLine()!;
        if (string.IsNullOrEmpty(response)){
          break;
        }
        list.Add(response);
      } while (true);
      prop.SetValue(character, list);
    }
  }
}