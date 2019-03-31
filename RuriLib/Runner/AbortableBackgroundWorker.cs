using System.ComponentModel;
using System.Threading;

namespace RuriLib.Runner
{
    /// <summary>
    /// Whether the AbortableBackgroundWorker is idle, stopping or working.
    /// </summary>
    public enum WorkerStatus
    {
        /// <summary>The Worker is not working.</summary>
        Idle,

        /// <summary>The Worker is working.</summary>
        Running,

        /// <summary>The Worker is cancelling its work.</summary>
        Stopping
    }

    /// <summary>
    /// A Worker that can be aborted like normal threads.
    /// </summary>
    public class AbortableBackgroundWorker : BackgroundWorker
    {
        private Thread workerThread;

        /// <summary>The Status of the Worker.</summary>
        public WorkerStatus Status { get; set; }

        /// <summary>The Id of the Worker.</summary>
        public int Id { get; set; }

        /// <summary>
        /// OnDoWork that supports aborting.
        /// </summary>
        /// <param name="e">The DoWorkEventArgs</param>
        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }

        /// <summary>Calls the Abort() method on the inner Thread.</summary>
        public void Abort()
        {
            if (workerThread != null)
            {
                workerThread.Abort();
                workerThread = null;
            }
        }
    }
}
