using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.LS
{
    /// <summary>
    /// An exception thrown inside a block's Process method.
    /// </summary>
    public class BlockProcessingException : Exception
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public BlockProcessingException()
        {

        }

        /// <summary>
        /// Creates the exception with a message.
        /// </summary>
        /// <param name="message">The message</param>
        public BlockProcessingException(string message) : base(message)
        {

        }

        /// <summary>
        /// Creates the exception with a message and an inner exception.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="inner">The inner exception</param>
        public BlockProcessingException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
