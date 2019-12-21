namespace RuriLib.Models
{
    /// <summary>
    /// The result of a proxy check.
    /// </summary>
    public struct ProxyResult
    {
        /// <summary>
        /// The proxy that was tested.
        /// </summary>
        public CProxy proxy;

        /// <summary>
        /// Whether the proxy works.
        /// </summary>
        public bool working;

        /// <summary>
        /// The ping in milliseconds.
        /// </summary>
        public int ping;

        /// <summary>
        /// The approximate location of the proxy server (if it was tested).
        /// </summary>
        public string country;

        /// <summary>
        /// Initializes a proxy check result.
        /// </summary>
        /// <param name="proxy">The proxy that was tested</param>
        /// <param name="working">Whether the proxy works</param>
        /// <param name="ping">The ping in milliseconds</param>
        /// <param name="country">The approximate location of the proxy server (if it was tested)</param>
        public ProxyResult(CProxy proxy, bool working, int ping, string country = "Unknown")
        {
            this.proxy = proxy;
            this.working = working;
            this.ping = ping;
            this.country = country;
        }
    }
}
