using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models
{
    /// <summary>
    /// Objects that can be saved on a persistent storage can inherit from this class.
    /// </summary>
    public abstract class Persistable<T> : ViewModelBase
    {
        /// <summary>
        /// The unique id.
        /// </summary>
        public T Id { get; set; }
    }
}
