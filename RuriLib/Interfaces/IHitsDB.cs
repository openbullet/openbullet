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
        /// Deletes all hits from the database.
        /// </summary>
        void Purge();

        /// <summary>
        /// The collection of hits.
        /// </summary>
        IEnumerable<Hit> Hits { get; }
    }
}
