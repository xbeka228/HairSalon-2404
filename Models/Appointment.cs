using System;

namespace HairSalon.Models;

public class Appointment
{
    public int Id { get; set; }
    public Client Client { get; set; } = null!;
    public Master Master { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public DateTime DateTime { get; set; }
    public string Status { get; set; } = "Запланирована";

    public string Summary =>
        $"{DateTime:dd.MM.yyyy HH:mm} | {Client.Name} | {Master.Name} | {Service.Name} | {Status}";
}
