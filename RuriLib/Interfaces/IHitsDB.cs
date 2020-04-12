using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib.Models;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// An interface for a database of hits.
    /// </summary>
    public interface IHitsDB
    {
        /// <summary>
        /// Deletes duplicate hits.
        /// </summary>
        void DeleteDuplicates();

        /// <summary>
        /// Removes all hits from the database.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// The collection of hits.
        /// </summary>
        IEnumerable<Hit> Hits { get; }

        /// <summary>
        /// Removes a hit from the database.
        /// </summary>
        /// <param name="hit">The hit to remove</param>
        void Remove(Hit hit);

        /// <summary>
        /// Removes multiple hits from the database.
        /// </summary>
        /// <param name="hits">The hits to remove</param>
        void Remove(IEnumerable<Hit> hits);

        /// <summary>
        /// Updates a hit in the database.
        /// </summary>
        /// <param name="hit">The hit to update</param>
        void Update(Hit hit);

        /// <summary>
        /// Adds a new hit to the database.
        /// </summary>
        /// <param name="hit">The hit to add</param>
        void Add(Hit hit);
    }
}
