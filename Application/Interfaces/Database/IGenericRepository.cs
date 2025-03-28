using Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Database
{
    /// <summary>
    /// Defines a generic repository interface for performing CRUD operations on entities of type T.
    /// </summary>
    /// <typeparam name="T">The entity type that the repository operates on.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves a paginated list of entities based on the specified filter, page number, and page size.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="filter">An optional filter expression to apply to the query.</param>
        /// <returns>A task representing the asynchronous operation, returning a paginated list of entities.</returns>
        Task<PaginatedList<T>> GetAll(int pageNumber, int pageSize, Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Retrieves a paginated list of entities based on the specified filter, page number, page size, and includeProperties.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="filter">An optional filter expression to apply to the query.</param>
        /// <param name="includeProperties">Optional expressions to specify related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, returning a paginated list of entities.</returns>
        Task<PaginatedList<T>> GetAll(int pageNumber, int pageSize, Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Retrieves a list of entities based on the specified filter.
        /// </summary>
        /// <param name="filter">An optional filter expression to apply to the query.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of entities.</returns>
        Task<List<T>> GetAll(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Retrieves a single entity based on the specified filter.
        /// </summary>
        /// <param name="filter">The filter expression to apply to the query.</param>
        /// <returns>A task representing the asynchronous operation, returning the entity that matches the filter, or null if no match is found.</returns>
        Task<T> Get(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Retrieves an entity by its ID, optionally including related entities.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <param name="includeProperties">Optional include expressions to eagerly load related entities.</param>
        /// <returns>A task representing the asynchronous operation, returning the entity with the specified ID, or null if not found.</returns>
        Task<T> GetById(string id, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Asynchronously retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, returning the entity with the specified ID, or null if not found.</returns>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(T entity);

        /// <summary>
        /// Asynchronously adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(T entity);

        /// <summary>
        /// Deletes an entity from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteById(string id);

        /// <summary>
        /// Deletes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(T entity);

        /// <summary>
        /// Performs a soft delete on an entity by its ID, marking it as deleted without physically removing it from the database.
        /// </summary>
        /// <param name="id">The ID of the entity to soft delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SoftDelete(string id);
    }
}
