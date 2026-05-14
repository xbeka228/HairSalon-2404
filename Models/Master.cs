namespace HairSalon.Models;

public class Master
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }

    public override string ToString() => Name;
}
