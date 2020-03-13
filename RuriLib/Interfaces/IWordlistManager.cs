using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib.Models;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a class that manages a collection of wordlists.
    /// </summary>
    public interface IWordlistManager
    {
        /// <summary>
        /// Adds a wordlist to the collection.
        /// </summary>
        /// <param name="wordlist">The wordlist to add</param>
        void Add(Wordlist wordlist);

        /// <summary>
        /// The collection of available wordlists.
        /// </summary>
        IEnumerable<Wordlist> Wordlists { get; }

        /// <summary>
        /// Updates a wordlist.
        /// </summary>
        /// <param name="wordlist">The updated wordlist</param>
        void Update(Wordlist wordlist);

        /// <summary>
        /// Removes a given wordlist from the collection.
        /// </summary>
        /// <param name="wordlist">The wordlist to remove</param>
        void Remove(Wordlist wordlist);

        /// <summary>
        /// Deletes wordlists that reference a missing file from the collection.
        /// </summary>
        void DeleteNotFound();

        /// <summary>
        /// Removes all wordlists from the collection.
        /// </summary>
        void RemoveAll();
    }
}
