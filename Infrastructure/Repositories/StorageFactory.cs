using Application.Interfaces.General;
using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// A factory class for creating storage instances based on the specified storage type.
    /// </summary>
    public class StorageFactory : IStorageFactory
    {
        /// <summary>
        /// The service provider used to resolve storage dependencies.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve storage dependencies.</param>
        public StorageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a storage instance based on the specified storage type.
        /// </summary>
        /// <param name="storageType">The type of storage to create.</param>
        /// <returns>An instance of the <see cref="IStorage"/> interface, representing the created storage.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid storage type is selected.</exception>
        public IStorage CreateStorage(StorageType storageType)
        {
            return storageType switch
            {
                StorageType.Local => _serviceProvider.GetRequiredService<ILocalStorage>(),
                StorageType.Firebase => _serviceProvider.GetRequiredService<IFirebaseStorage>(),
                _ => throw new ArgumentException("Invalid storage type selected.")
            };
        }
    }

}
