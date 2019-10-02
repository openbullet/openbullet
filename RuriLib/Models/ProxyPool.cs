using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RuriLib.Models
{
    /// <summary>
    /// Class that manages a pool of proxies.
    /// </summary>
    public class ProxyPool
    {
        private static Random rng = new Random();

        /// <summary>The full list of proxies in the pool.</summary>
        public List<CProxy> Proxies { get; } = new List<CProxy>();

        /// <summary>The list of Alive proxies (AVAILABLE or BUSY).</summary>
        public List<CProxy> Alive { get { return Proxies.Where(p => p.Status == Status.AVAILABLE || p.Status == Status.BUSY).ToList(); } }

        /// <summary>The list of Available proxies.</summary>
        public List<CProxy> Available { get { return Proxies.Where(p => p.Status == Status.AVAILABLE).ToList(); } }

        /// <summary>The list of Banned proxies.</summary>
        public List<CProxy> Banned { get { return Proxies.Where(p => p.Status == Status.BANNED).ToList(); } }

        /// <summary>The list of Bad proxies.</summary>
        public List<CProxy> Bad { get { return Proxies.Where(p => p.Status == Status.BAD).ToList(); } }

        /// <summary>
        /// <para>Whether the proxy list is locked or not.</para>
        /// <para>It prevents multiple bots from choosing the same proxy simultaneously, regardless of the fact that concurrent usage may be disabled.</para>
        /// </summary>
        private bool Locked { get; set; } = false;

        /// <summary>
        /// Initializes the proxy pool given a collection of string to be parsed as proxies and their type.
        /// </summary>
        /// <param name="proxies">The collection of strings to parse the proxies from</param>
        /// <param name="type">The type of the proxies</param>
        /// <param name="shuffle">Whether to shuffle the proxy list</param>
        public ProxyPool(IEnumerable<string> proxies, Extreme.Net.ProxyType type, bool shuffle)
        {
            Proxies = proxies.Select(p => new CProxy(p, type)).ToList();
            if (shuffle) Shuffle(Proxies);
        }

        /// <summary>
        /// <para>Initializes the proxy pool given a collection of CProxy objects.</para>
        /// <para>The proxies will be first cloned and then stored in the list.</para>
        /// <para>They will also be unbanned to make sure there are no leftovers from previous checks.</para>
        /// </summary>
        /// <param name="proxies">The list of CProxy objects to be cloned and added to the list</param>
        /// <param name="shuffle">Whether to shuffle the proxy list</param>
        public ProxyPool(List<CProxy> proxies, bool shuffle = false)
        {
            // We clone the list, since we don't want to target to the same objects that are in the Proxy Manager
            // If we don't do this, a ban on a runner instance would cause a ban on all the other instances as well!
            Proxies = IOManager.CloneProxies(proxies);

            // Now that the list is cloned, we make sure that no proxy is already banned
            UnbanAll();

            // Shuffle it if necessary
            if (shuffle) Shuffle(Proxies);
        }

        /// <summary>
        /// Clears the Clearance and Cfduid cookies set by Cloudflare.
        /// </summary>
        public void ClearCF()
        {
            foreach (var proxy in Proxies)
            {
                proxy.Clearance = "";
                proxy.Cfduid = "";
            }
        }

        /// <summary>
        /// Sets all the BANNED and BAD proxies status to AVAILABLE and resets their Uses. It also clears the Cloudflare cookies.
        /// </summary>
        public void UnbanAll()
        {
            foreach (var proxy in Proxies)
            {
                if (proxy.Status == Status.BANNED || proxy.Status == Status.BAD)
                {
                    proxy.Status = Status.AVAILABLE;
                    proxy.Hooked = 0;
                    proxy.Uses = 0;
                }
            }
            ClearCF();
        }

        /// <summary>
        /// Tries to return the first available proxy from the list.
        /// </summary>
        /// <param name="evenBusy">Whether to include proxies that are being used by other bots</param>
        /// <param name="maxUses">The maximum uses of a proxy, after which it will be banned (0 for infinite)</param>
        /// <param name="neverBan">Whether a proxy can ever be banned</param>
        /// <returns>The first available proxy respecting the conditions or null</returns>
        public CProxy GetProxy(bool evenBusy, int maxUses, bool neverBan = false)
        {
            while (Locked) Thread.Sleep(10);

            CProxy proxy;
            Locked = true;
            if (evenBusy)
            {
                if (maxUses == 0) proxy = Alive.FirstOrDefault();
                else proxy = Alive.Where(p => p.Uses < maxUses).FirstOrDefault();
            }
            else
            {
                if (maxUses == 0) proxy = Available.FirstOrDefault();
                else proxy = Available.Where(p => p.Uses < maxUses).FirstOrDefault();
            }

            if (proxy != null)
            {
                if (maxUses > 0 && proxy.Uses > maxUses && !neverBan)
                {
                    proxy.Status = Status.BANNED;
                    Locked = false;
                    return null;
                }
                else
                {
                    proxy.Status = Status.BUSY;
                    proxy.Hooked++;
                }
            }
            Locked = false;
            return proxy;
        }

        /// <summary>
        /// Removes all duplicate proxies from the pool.
        /// </summary>
        public void RemoveDuplicates()
        {
            var dupeList = Proxies
               .GroupBy(p => p.Proxy)
               .Where(grp => grp.Count() > 1)
               .Select(grp => grp.First());

            foreach (var p in dupeList)
            {
                Proxies.Remove(p);
            }
        }

        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
