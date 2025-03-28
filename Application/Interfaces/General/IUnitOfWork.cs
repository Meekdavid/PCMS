using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    /// <summary>
    /// Defines an interface for a Unit of Work, providing transactional and data persistence management.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Saves all changes made in the current context to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the number of state entries written to the underlying database.</returns>
        Task<int> SaveChanges();

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task BeginTransaction();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Commit();

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Rollback();

        /// <summary>
        /// Detaches all tracked entities from the context.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DetachAllEntries();

        /// <summary>
        /// Asynchronously begins a new transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Asynchronously commits the current transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CommitAsync();

        /// <summary>
        /// Asynchronously rolls back the current transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RollbackAsync();

        /// <summary>
        /// Asynchronously saves all changes made in the current context to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the number of state entries written to the underlying database.</returns>
        Task SaveChangesAsync();
    }
}
