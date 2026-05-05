// Path: MediQueue.Infrastructure/Persistence/IDataSeeder.cs
using System.Threading.Tasks;

namespace MediQueue.Infrastructure.Persistence;

public interface IDataSeeder
{
    Task SeedAsync();
}
