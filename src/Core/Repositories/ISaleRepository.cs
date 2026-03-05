using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;

namespace UTM_Market.Core.Repositories;

/// <summary>
/// Defines the repository contract for the Sale aggregate root.
/// This interface is the boundary for all persistence operations related to sales.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Retrieves all sales from the data store using an asynchronous stream.
    /// This method is memory-efficient for processing large datasets.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IAsyncEnumerable{Sale}"/> that streams sales from the data store.</returns>
    IAsyncEnumerable<Sale> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single sale by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. 
    /// The task result contains the <see cref="Sale"/> if found; otherwise, null.</returns>
    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds sales that match the specified filter criteria, returning them as an async stream.
    /// </summary>
    /// <param name="filter">The criteria to apply to the search.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IAsyncEnumerable{Sale}"/> that streams matching sales.</returns>
    IAsyncEnumerable<Sale> FindAsync(SaleFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new sale to the data store.
    /// </summary>
    /// <param name="sale">The <see cref="Sale"/> aggregate root to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation.
    /// The task result is the persisted <see cref="Sale"/>, which may include a database-generated ID.</returns>
    Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sale in the data store.
    /// </summary>
    /// <param name="sale">The <see cref="Sale"/> aggregate root with updated data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous update operation.</returns>
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
}
