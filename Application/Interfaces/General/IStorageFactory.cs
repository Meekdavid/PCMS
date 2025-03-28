using Persistence.Enums;

namespace Application.Interfaces.General
{
    public interface IStorageFactory
    {
        IStorage CreateStorage(StorageType storageType);
    }
}
