namespace Core.Models.Entities;

public struct Assets
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Assets(int id, string name) => (Id, Name) = (id, name);
}