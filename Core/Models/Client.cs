namespace Core;

public struct Client
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Client(string name) => Name = name;
    public Client(int id, string name) => (Id, Name) = (id, name);
}