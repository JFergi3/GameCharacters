using NLog;
using System.Reflection;
using System.Text.Json;

string path = Directory.GetCurrentDirectory() + "//nlog.config";
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

string marioFileName = "mario.json";
string donkeyKongFileName = "dk.json";
string streetFighterFileName = "sf2.json";

List<Mario> marios = LoadCharacters<Mario>(marioFileName, logger);
List<DonkeyKong> donkeyKongs = LoadCharacters<DonkeyKong>(donkeyKongFileName, logger);
List<StreetFighter> streetFighters = LoadCharacters<StreetFighter>(streetFighterFileName, logger);

while (true)
{
  Console.WriteLine("Choose a game:");
  Console.WriteLine("1) Mario");
  Console.WriteLine("2) Donkey Kong");
  Console.WriteLine("3) Street Fighter II");
  Console.WriteLine("Enter to quit");

  string? gameChoice = Console.ReadLine();
  logger.Info("Game choice: {Choice}", gameChoice);

  if (string.IsNullOrEmpty(gameChoice))
  {
    break;
  }

  Console.WriteLine("Choose an action:");
  Console.WriteLine("1) Display Characters");
  Console.WriteLine("2) Add Character");
  Console.WriteLine("3) Remove Character");
  Console.WriteLine("4) Back");

  string? actionChoice = Console.ReadLine();
  logger.Info("Action choice: {Choice}", actionChoice);

  switch (gameChoice)
  {
    case "1":
      HandleAction(actionChoice, marios, marioFileName, logger);
      break;
    case "2":
      HandleAction(actionChoice, donkeyKongs, donkeyKongFileName, logger);
      break;
    case "3":
      HandleAction(actionChoice, streetFighters, streetFighterFileName, logger);
      break;
    default:
      logger.Info("Invalid game choice");
      break;
  }
}

logger.Info("Program ended");

static List<T> LoadCharacters<T>(string fileName, Logger logger) where T : Character
{
  if (!File.Exists(fileName))
  {
    return [];
  }

  List<T>? characters = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(fileName));
  logger.Info("File deserialized {FileName}", fileName);
  return characters ?? [];
}

static void DisplayCharacters<T>(List<T> characters) where T : Character
{
  foreach (T character in characters)
  {
    Console.WriteLine(character.Display());
  }
}

static void HandleAction<T>(string? actionChoice, List<T> characters, string fileName, Logger logger)
  where T : Character, new()
{
  switch (actionChoice)
  {
    case "1":
      DisplayCharacters(characters);
      break;
    case "2":
      AddCharacter(characters, fileName, logger);
      break;
    case "3":
      RemoveCharacter(characters, fileName, logger);
      break;
    case "4":
      break;
    default:
      logger.Info("Invalid action choice");
      break;
  }
}

static void AddCharacter<T>(List<T> characters, string fileName, Logger logger) where T : Character, new()
{
  T character = new()
  {
    Id = characters.Count == 0 ? 1 : characters.Max(c => c.Id) + 1
  };

  InputCharacter(character);
  characters.Add(character);
  File.WriteAllText(fileName, JsonSerializer.Serialize(characters));
  logger.Info("Character added: {Name}", character.Name);
}

static void RemoveCharacter<T>(List<T> characters, string fileName, Logger logger) where T : Character
{
  Console.WriteLine("Enter the Id of the character to remove:");

  if (!UInt64.TryParse(Console.ReadLine(), out UInt64 id))
  {
    logger.Error("Invalid Id");
    return;
  }

  T? character = characters.FirstOrDefault(c => c.Id == id);
  if (character == null)
  {
    logger.Error("Character Id {Id} not found", id);
    return;
  }

  characters.Remove(character);
  File.WriteAllText(fileName, JsonSerializer.Serialize(characters));
  logger.Info("Character Id {Id} removed", id);
}

static void InputCharacter(Character character)
{
  Type type = character.GetType();
  PropertyInfo[] properties = type.GetProperties();
  IEnumerable<PropertyInfo> props = properties.Where(p => p.Name != "Id");

  foreach (PropertyInfo prop in props)
  {
    if (prop.PropertyType == typeof(string))
    {
      Console.WriteLine($"Enter {prop.Name}:");
      prop.SetValue(character, Console.ReadLine());
    }
    else if (prop.PropertyType == typeof(List<string>))
    {
      List<string> list = [];
      do
      {
        Console.WriteLine($"Enter {prop.Name} or (enter) to quit:");
        string response = Console.ReadLine()!;
        if (string.IsNullOrEmpty(response))
        {
          break;
        }

        list.Add(response);
      } while (true);

      prop.SetValue(character, list);
    }
  }
}
