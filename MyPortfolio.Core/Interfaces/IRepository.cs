using System.Linq.Expressions;

namespace MyPortfolio.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Add a new entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Check if any entity matches the predicate
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
