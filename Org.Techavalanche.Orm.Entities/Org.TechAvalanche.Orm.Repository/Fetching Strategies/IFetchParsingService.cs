#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Linq.Expressions;
using Org.TechAvalanche.Orm.Specification;
using Org.TechAvalanche.Orm.Repository.Attributes;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Fetching
{
    public interface IFetchParsingService
    {
        IList<TEntity> ParseToList<TEntity>(IFetchingStrategy strategy,
            ObjectQuery<TEntity> query) where TEntity : IFetchable;

        IQueryable<TEntity> ParseToQueryable<TEntity>(IFetchingStrategy strategy,
            ObjectQuery<TEntity> query) where TEntity : IFetchable;
    }

    public class FetchWorker
    {
        #region Constructor

        public FetchWorker() { }

        #endregion

        #region Worker Methods

        internal ObjectQuery<TEntity> InjectEagerIncludes<TEntity>(ObjectQuery<TEntity> query,
            List<FetchingIntention> eager_loads) where TEntity : IFetchable
        {
            //inject the .Includes
            foreach (var intent in eager_loads)
            {
                query = query.Include(intent.FetchAssociate);
            }
            return query;
        }

        internal List<FetchingIntention> GetLazyLoadIntentions(IFetchingStrategy strategy,
            List<PropertyInfo> all_props_that_can_load)
        {
            var lazy_loads = from l in strategy.Intentions
                             //join f in all_props_that_can_load
                             //on l.FetchAssociate equals f.Name.Replace("Reference", "")
                             where l.FetchMode == FetchMode.Lazy
                             select new FetchingIntention
                                  (l.FetchAssociate.Replace("Reference", ""), l.FetchMode);
            return lazy_loads.ToList();
        }

        internal List<FetchingIntention> GetEagerLoadIntentions(IFetchingStrategy strategy,
            List<PropertyInfo> all_props_that_can_load)
        {
            var eager_loads = from e in strategy.Intentions
                              //join f in all_props_that_can_load
                              //on e.FetchAssociate equals f.Name.Replace("Reference", "")
                              where e.FetchMode == FetchMode.Eager
                              select new FetchingIntention
                                  (e.FetchAssociate.Replace("Reference", ""), e.FetchMode);
            return eager_loads.ToList();
        }

        internal List<PropertyInfo> GetAllLoadableProperties<TEntity>()
            where TEntity : IFetchable
        {
            var all_props_that_can_load = from a in typeof(TEntity).GetProperties()
                                          where a.PropertyType.FullName.Contains("EntityCollection") ||
                                                a.PropertyType.FullName.Contains("EntityReference")
                                          select a;
            
            return all_props_that_can_load.ToList();
        }

        #endregion
    }

    public class FetchParser : FetchWorker, IFetchParsingService
    {
        #region IFetchParsingService Members

        public IList<TEntity> ParseToList<TEntity>(IFetchingStrategy strategy, ObjectQuery<TEntity> query)
            where TEntity : IFetchable
        {
            IList<TEntity> entities = null;

            List<FetchingIntention> eager_loads;
            List<FetchingIntention> lazy_loads;

            GetPropertiesForLoadingAndTheirIntentions<TEntity>(strategy, out eager_loads, out lazy_loads);

            query = InjectEagerIncludes<TEntity>(query, eager_loads);

            if (lazy_loads.Count() > 0)
            {
                //project the strategies for lazy loading that
                //will get run in the aspect boundary
                entities = (from e in query.ToList()
                            .Select(e => { e.FetchingStrategy = strategy; return e; })
                            select e).ToList();
            }
            else
            {
                entities = query.ToList();
            }

            return entities;
        }

        public IQueryable<TEntity> ParseToQueryable<TEntity>(IFetchingStrategy strategy, ObjectQuery<TEntity> query)
            where TEntity : IFetchable
        {
            IQueryable<TEntity> entities = null;

            List<FetchingIntention> eager_loads;
            List<FetchingIntention> lazy_loads;

            GetPropertiesForLoadingAndTheirIntentions<TEntity>(strategy, out eager_loads, out lazy_loads);

            query = InjectEagerIncludes<TEntity>(query, eager_loads);

            if (lazy_loads.Count() > 0)
            {
                //project the strategies for lazy loading so
                //the aspect boundary will have access to the strategy
                entities = (from e in query.ToList()
                            .Select(e => { e.FetchingStrategy = strategy; return e; })
                            select e).AsQueryable();
            }
            else
            {
                entities = query.AsQueryable();
            }

            return entities;
        }


        #endregion

        #region Private Worker Methods

        private void GetPropertiesForLoadingAndTheirIntentions<TEntity>(IFetchingStrategy strategy, 
            out List<FetchingIntention> eager_loads, 
            out List<FetchingIntention> lazy_loads) where TEntity : IFetchable
        {
            var all_props_that_can_load =
                GetAllLoadableProperties<TEntity>();

            eager_loads =
                GetEagerLoadIntentions(strategy, all_props_that_can_load);

            lazy_loads =
                GetLazyLoadIntentions(strategy, all_props_that_can_load);
        }

        #endregion
    }
}

#endregion