using Core.Models.Entities;

namespace Core.DAL.Abstractions;

public interface IClientDal
{
    public Task PersistClientsAsync(List<Clients> clients);
    public Task<List<Clients>> GetClientsAsync(short limit = 100);
}