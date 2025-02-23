namespace Core.DAL.Abstractions;

public interface IClientDal
{
    public Task PersistClientsAsync(List<Client> clients);
    public Task<List<Client>> GetClientsAsync(short limit = 100);
}