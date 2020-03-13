namespace RuriLib.Models
{
    /// <summary>
    /// Wrapper for a check result.
    /// </summary>
    /// <typeparam name="T">The type of result</typeparam>
    public struct CheckResult<T>
    {
        /// <summary>
        /// Whether the check was completed successfully.
        /// </summary>
        public bool success;

        /// <summary>
        /// The result of the check
        /// </summary>
        public T result;

        /// <summary>
        /// The error message, if any.
        /// </summary>
        public string error;

        /// <summary>
        /// Constructs a check result.
        /// </summary>
        /// <param name="success">Whether the check was completed successfully</param>
        /// <param name="result">The result of the check</param>
        /// <param name="error">The error message, if any</param>
        public CheckResult(bool success, T result, string error = "")
        {
            this.success = success;
            this.result = result;
            this.error = error;
        }
    }
}
