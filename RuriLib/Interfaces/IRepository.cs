using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Generic interface repository for performing CRUD operations on a persistent storage.
    /// </summary>
    /// <typeparam name="TEntity">The returned entity type</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Retrieves all entities from the storage.
        /// </summary>
        /// <returns>All the stored entities</returns>
        IEnumerable<TEntity> Get();

        /// <summary>
        /// Retrieves an entity basing on its id.
        /// </summary>
        /// <param name="id">The unique entity id</param>
        /// <returns>The found entity.</returns>
        TEntity Get(Guid id);

        /// <summary>
        /// Adds an entity to the storage.
        /// </summary>
        /// <param name="entity">The entity that needs to be added</param>
        void Add(TEntity entity);

        /// <summary>
        /// Removes an entity from the storage.
        /// </summary>
        /// <param name="entity">The entity that needs to be removed</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Updates an entity in the storage.
        /// </summary>
        /// <param name="entity">The entity that needs to be updated</param>
        void Update(TEntity entity);

        /// <summary>
        /// Clears the storage.
        /// </summary>
        void RemoveAll();
    }
}
