public class DonkeyKong : Character
{
  public string? Species { get; set; }

  public override string Display()
  {
    return $"Id: {Id}\nSpecies: {Species}\nName: {Name}\nDescription: {Description}\n";
  }
}
