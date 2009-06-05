#region Imported Libraries

using System.Linq.Expressions;
using d = System.Diagnostics;
using System.Linq;
using System;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Fetching
{
    #region Fetch Mode Enumeration

    public enum FetchMode
    {
        Eager = 0,
        Lazy = 1
    }

    #endregion

    #region FetchingIntention Class Declaration

    public class FetchingIntention
    {
        #region Private Members

        [ThreadStatic()]
        private static FetchingIntention _factory_intent;

        private readonly FetchMode _fetchMode;
        private readonly string _fetchAssociate;

        #endregion

        #region Accessors

        public FetchMode FetchMode
        {
            get { return _fetchMode; }
        }

        public string FetchAssociate
        {
            get { return _fetchAssociate; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fetch">
        /// Function that describes returns
        /// the type of the entity name 
        /// to fetch.
        /// </param>
        /// <param name="mode">
        /// The mode of fetching.
        /// </param>
        public FetchingIntention(string fetch, FetchMode mode)
        {
            _fetchAssociate = fetch;
            _fetchMode = mode;
        }

        private FetchingIntention() { }

        #endregion

        #region Factory Method

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fetch">
        /// Function that describes returns
        /// the type of the entity name 
        /// to fetch.
        /// </param>
        /// <param name="mode">
        /// The mode of fetching.
        /// </param>
        /// <typeparam name="TEntity">
        /// Returns a Fetching Intention.
        /// </typeparam>
        public static FetchingIntention CreateInstance<TRootEntity, TFetchEntity>(
            Expression<Func<TRootEntity, TFetchEntity>> fetch, FetchMode mode) 
        {
            if (fetch.Parameters.Count > 1)
                throw new ArgumentException("FetchingIntentions support only " +
                    "one parameter in a dynamic expression!");

            int dot = fetch.Body.ToString().IndexOf(".") + 1;
            string includes = fetch.Body.ToString().Remove(0, dot);

            _factory_intent =
                new FetchingIntention(includes, mode);

            return _factory_intent;
        }

        #endregion
    }

    public static class FetchingIntentionExtensions
    {
        #region Extension Methods

        public static FetchingIntention And(this FetchingIntention original, FetchingIntention addTo)
        {
            FetchingIntention compound_intention;
            FetchMode mode;

            if (original.FetchMode != addTo.FetchMode)
                throw new ArgumentException
                    ("FetchingIntention objects must " +
                    "share the same type of fetch mode " +
                    "to be joined together!");

            compound_intention = 
                new FetchingIntention(original.FetchAssociate + "." + 
                    addTo.FetchAssociate, original.FetchMode);

            return compound_intention;
        }

        #endregion
    }

    #endregion
}

#endregion