namespace Core.Models.Entities;

public struct Clients
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Clients(string name) => Name = name;
    public Clients(int id, string name) => (Id, Name) = (id, name);
}