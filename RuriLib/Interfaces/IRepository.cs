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
    /// <typeparam name="TId">The type of id</typeparam>
    public interface IRepository<TEntity, in TId> where TEntity : class
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
        TEntity Get(TId id);

        /// <summary>
        /// Adds an entity to the storage.
        /// </summary>
        /// <param name="entity">The entity that needs to be added</param>
        void Add(TEntity entity);

        /// <summary>
        /// Adds multiple entities to the storage.
        /// </summary>
        /// <param name="entities">The entities that need to be added</param>
        void Add(IEnumerable<TEntity> entities);

        /// <summary>
        /// Removes an entity from the storage.
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Removes multiple entities from the storage.
        /// </summary>
        /// <param name="entities">The entities that need to be removed</param>
        void Remove(IEnumerable<TEntity> entities);

        /// <summary>
        /// Clears the storage.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Updates an entity in the storage.
        /// </summary>
        /// <param name="entity">The entity that needs to be updated</param>
        void Update(TEntity entity);

        /// <summary>
        /// Updates multiple entities in the storage.
        /// </summary>
        /// <param name="entities">The entities that need to be updated</param>
        void Update(IEnumerable<TEntity> entities);
    }
}
