#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using Org.TechAvalanche.Orm.Specification;
using Org.TechAvalanche.Orm.Fetching;
using System.Reflection;
using Microsoft.CSharp;
using System.IO;
using System.Reflection.Emit;
using Org.TechAvalanche.Orm.Repository.Attributes;
using System.Data.Metadata.Edm;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Repository
{
    #region Class Declaration

    public class EntitiesRepository<TEntity, TSession> 
                                    where TEntity : EntityObject, IFetchable
                                    where TSession : ObjectContext
    {
        #region Private Members

        private readonly TSession _ctx;
        private readonly IFetchParsingService _fetchParser;

        #endregion

        #region Unit of Work Accessor

        public TSession Session
        {
            get { return _ctx; }
        }

        #endregion

        #region Constructors

        public EntitiesRepository(TSession session)
        {
            _fetchParser = null;
            _ctx = session;
        }

        public EntitiesRepository(TSession session, IFetchParsingService fetchParser)
        {
            _fetchParser = fetchParser;
            _ctx = session;
        }

        #endregion

        #region Common Unit of Work Methods

        public void Save()
        {
            _ctx.SaveChanges();
        }

        #endregion

        #region Generic Cross Domain Query Methods

        //TODO: ToList versions of the methods
        //should not be able to do fetching stragies
        //as they run the query when toList is called....
        //code for an eager fetch

        //if a fetching strategy is provided as a parameter
        //check the provided strategy and if it has fetching
        //intention pairs for eager loading named properties
        //of the current entity E and those properties are either
        //a EntityCollection EntityReference then build up
        //a set of includes for those named properties
        //coupled with eager fetching fetch modes.

        //or if the method has a generic parameter
        //look in the location of the executing assembly
        //for a fetching strategy that matches on generic
        //argument and set up includes from that dynamically
        //discovered strategy

        //or if its a lazy load mode of fetching then
        //check if E is IFetchable and add the lazy 
        //Fetching intentions to the IFetchable of type
        //E and our weaving code will check for IFetchable
        //and call Load() on properties that match by
        //name in the fetching intentions 

        #region .All IList<T> Methods

        /// <summary>
        /// Returns all records for the Entity
        /// using the Entities Type Name property.
        /// </summary>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public virtual IList<TEntity> AllToIList()
        {
            var entity_set_name = FindEntitySetName();

            var qry = _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]");
            return qry.ToList();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="where">
        /// A given ISpecification which encapsulates
        /// a where predicate to apply to the .Where
        /// extension method for filtering.
        /// </param>
        /// <returns></returns>
        public IList<TEntity> AllToIList(ISpecification<TEntity> where)
        {
            var entity_set_name = FindEntitySetName();

            return
                _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]")
                .Where(where.EvalPredicate).ToList();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="strategy">
        /// A given IFetchingStrategy that encapsulates
        /// the lazy and eager fetching intention.
        /// </param>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public IList<TEntity> AllToIList(IFetchingStrategy strategy)
        {
            var entity_set_name = FindEntitySetName();

            var qry = _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]");
            //can be matched from the strategy to the ObjectQuery<E>
            //which will project the strategies for lazy loading that
            //will get run in the aspect boundary
            IList<TEntity> entities = new FetchParser().ParseToList(strategy, qry);
            return entities;
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="strategy">
        /// A given IFetchingStrategy that encapsulates
        /// the lazy and eager fetching intention.
        /// </param>
        /// <param name="where">
        /// A given ISpecification which encapsulates
        /// a where predicate to apply to the .Where
        /// extension method for filtering.
        /// </param>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public IList<TEntity> AllToIList(ISpecification<TEntity> where, IFetchingStrategy strategy)
        {
            var entity_set_name = FindEntitySetName();

            var qry = (ObjectQuery<TEntity>)
                _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]")
                .Where(where.EvalPredicate);
            //call parse and have parse attach the .Includes that
            //can be matched from the strategy to the ObjectQuery<E>
            //which will project the strategies for lazy loading that
            //will get run in the aspect boundary
            IList<TEntity> entities = new FetchParser().ParseToList(strategy, qry);
            return entities;
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <typeparam name="S">
        /// A generic argument that specifices
        /// an expectation that it's type will
        /// be defined as an interface that is
        /// used to express a role to locate and
        /// load a IFetchingStrategy of T. Exampe 
        /// would be IMakeCustomerPrefered.
        /// </typeparam>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        [NotYetImplemented("Need to write dynamic assembly loading / DI code")]
        public virtual IList<TEntity> AllToIList<S>()
        {
            //TODO: write code to find and load an IFetchingStrategy<S> 
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <typeparam name="S">
        /// A generic argument that specifices
        /// an expectation that it's type will
        /// be defined as an interface that is
        /// used to express a role to locate and
        /// load a IFetchingStrategy of T. Exampe 
        /// would be IMakeCustomerPrefered.
        /// </typeparam>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        [NotYetImplemented("Need to write dynamic assembly loading / DI code")]
        public IList<TEntity> AllToIList<S>(ISpecification<TEntity> where)
        {
            //TODO: write code to find and load an IFetchingStrategy<S> 
            throw new NotImplementedException();
        }

        #endregion

        #region .All IQueryable<T> Methods

        /// <summary>
        /// Returns all records for the Entity
        /// using the Entities Type Name property.
        /// </summary>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public IQueryable<TEntity> AllToIQueryable()
        {
            var entity_set_name = FindEntitySetName();

            var qry = _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]");
            return qry.AsQueryable();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="where">
        /// A given ISpecification which encapsulates
        /// a where predicate to apply to the .Where
        /// extension method for filtering.
        /// </param>
        /// <returns></returns>
        public IQueryable<TEntity> AllToIQueryable(ISpecification<TEntity> where)
        {
            var entity_set_name = FindEntitySetName();

            return
                _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]")
                .Where(where.EvalPredicate).AsQueryable();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="strategy">
        /// A given IFetchingStrategy that encapsulates
        /// the lazy and eager fetching intention.
        /// </param>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public IQueryable<TEntity> AllToIQueryable(IFetchingStrategy strategy)
        {
            var entity_set_name = FindEntitySetName();

            var qry = _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]");
            //can be matched from the strategy to the ObjectQuery<E>
            //which will project the strategies for lazy loading that
            //will get run in the aspect boundary
            return new FetchParser().ParseToQueryable(strategy, qry);
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <param name="entitySetName">
        /// The given Entity Set name which should
        /// corrrespond to the the ObjectContext
        /// EntitySet name.
        /// </param>
        /// <param name="strategy">
        /// A given IFetchingStrategy that encapsulates
        /// the lazy and eager fetching intention.
        /// </param>
        /// <param name="where">
        /// A given ISpecification which encapsulates
        /// a where predicate to apply to the .Where
        /// extension method for filtering.
        /// </param>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        public IQueryable<TEntity> AllToIQueryable(ISpecification<TEntity> where, IFetchingStrategy strategy)
        {
            var entity_set_name = FindEntitySetName();

            var qry = (ObjectQuery<TEntity>)
                _ctx.CreateQuery<TEntity>("[" + entity_set_name + "]")
                .Where(where.EvalPredicate);
            //call parse and have parse attach the .Includes that
            //can be matched from the strategy to the ObjectQuery<E>
            //which will project the strategies for lazy loading that
            //will get run in the aspect boundary
            return new FetchParser().ParseToQueryable(strategy, qry);
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <typeparam name="S">
        /// A generic argument that specifices
        /// an expectation that it's type will
        /// be defined as an interface that is
        /// used to express a role to locate and
        /// load a IFetchingStrategy of T. Exampe 
        /// would be IMakeCustomerPrefered.
        /// </typeparam>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        [NotYetImplemented("Need to write dynamic assembly loading / DI code")]
        public IQueryable<TEntity> AllToIQueryable<S>()
        {
            //TODO: write code to find and load an IFetchingStrategy<S> 
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all records for the Entity
        /// using a given EntitySet name.
        /// </summary>
        /// <typeparam name="S">
        /// A generic argument that specifices
        /// an expectation that it's type will
        /// be defined as an interface that is
        /// used to express a role to locate and
        /// load a IFetchingStrategy of T. Exampe 
        /// would be IMakeCustomerPrefered.
        /// </typeparam>
        /// <returns>
        /// Returns an IList of TEntity.
        /// </returns>
        [NotYetImplemented("Need to write dynamic assembly loading / DI code")]
        public IQueryable<TEntity> AllToIQueryable<S>(ISpecification<TEntity> where)
        {
            //TODO: write code to find and load an IFetchingStrategy<S> 
            throw new NotImplementedException();
        }

        #endregion

        #region Unique Methods

        public TEntity Unique(string entitySetName, ISpecification<TEntity> where)
        {
            var fetches = from f in typeof(TEntity).GetInterfaces()
                          where f.FullName == typeof(IFetchable).FullName
                          select f;

            var unique = _ctx.CreateQuery<TEntity>("[" + entitySetName + "]").
                Where<TEntity>(where.EvalPredicate).ToList();

            //((IFetchable)unique).FetchingStrategy.Intentions.Add 

            if (unique.Count > 1)
            {
                throw new NotUniqueEntityException();
            }
            return (TEntity)unique.First();
        }

        public TEntity Unique(ISpecification<TEntity> where)
        {
            var unique = _ctx.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]")
                .Where(where.EvalPredicate).ToList();

            if (unique.Count > 1)
            {
                throw new NotUniqueEntityException();
            }

            return (TEntity)unique.First();
        }

        #endregion

        #region First Methods

        public TEntity First(string entitySetName)
        {
            return (TEntity)_ctx.CreateQuery<TEntity>("[" + entitySetName + "]").First();
        }

        public TEntity First()
        {
            return (TEntity)_ctx.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]").First();
        }

        public TEntity First(string entitySetName, ISpecification<TEntity> where)
        {
            return (TEntity)
                _ctx.CreateQuery<TEntity>("[" + entitySetName + "]")
                .Where(where.EvalPredicate).First();
        }

        public TEntity First(ISpecification<TEntity> where)
        {
            return (TEntity)
                _ctx.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]")
                .Where(where.EvalPredicate).First();
        }

        #endregion

        #region Last Methods

        public TEntity Last(string entitySetName)
        {
            return (TEntity)_ctx.CreateQuery<TEntity>("[" + entitySetName + "]").Last();
        }

        public TEntity Last()
        {
            return (TEntity)_ctx.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]").Last();
        }

        public TEntity Last(string entitySetName, ISpecification<TEntity> where)
        {
            return (TEntity)
                _ctx.CreateQuery<TEntity>("[" + entitySetName + "]")
                .Where(where.EvalPredicate).Last();
        }

        public TEntity Last(ISpecification<TEntity> where)
        {
            return (TEntity)
                _ctx.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]")
                .Where(where.EvalPredicate).Last();
        }

        #endregion

        #region Private Worker Methods

        /// <summary>
        /// Selects the EntitySet name for the
        /// current TEntity root for this repository.
        /// </summary>
        /// <returns></returns>
        private string FindEntitySetName()
        {
            var container = _ctx.MetadataWorkspace.
                GetEntityContainer(_ctx.DefaultContainerName, DataSpace.CSpace);
            var entity_set_name = (from meta in container.BaseEntitySets
                                   where meta.BuiltInTypeKind == BuiltInTypeKind.EntitySet &&
                                   meta.ElementType.Name == typeof(TEntity).Name
                                   select meta.Name).First();
            return entity_set_name;
        }

        #endregion

        #endregion
    } 

    #endregion 

    #region Danny Simmons Extension Methods for ObjectContext

    public static class NTierExtensions
    {
        private static void SetAllPropertiesModified(ObjectContext context, object entity)
        {
            var stateEntry = context.ObjectStateManager.GetObjectStateEntry(entity);

            foreach (var propertyName in from fm in stateEntry.CurrentValues.
                                         DataRecordInfo.FieldMetadata
                                         select fm.FieldType.Name)
            {
                stateEntry.SetModifiedProperty(propertyName);
            }
        }

        public static void AttachAsModified(this ObjectContext context, IEntityWithKey entity)
        {
            context.Attach(entity);
            SetAllPropertiesModified(context, entity);
        }

        public static void AttachAsModifiedTo(this ObjectContext context, string entitySetName, object entity)
        {
            context.AttachTo(entitySetName, entity);
            SetAllPropertiesModified(context, entity);
        }
    }

    #endregion

    #region Non Unique Queries Exception Class

    /// <summary>
    /// Specialized Exception for Non Unique Queries
    /// where the Repository is forcing a count
    /// prior to using the .First Extension method.
    /// </summary>
    /// <remarks>
    /// Required due to the lack of implementation
    /// for the .Single() extension method in LINQ
    /// To Entities.
    /// </remarks>
    public class NotUniqueEntityException : Exception
    {
        public NotUniqueEntityException() : 
            base("The entity query produced more than a single result and is not unique") { }
    }

    #endregion
}

#endregion