#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Diagnostics;
using Org.TechAvalanche.Orm.Fetching;
using PostSharp.Laos;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.LazyLoading
{
    #region Lazying Loading Attribute for Entity Framework

    [Serializable]
    public class LazyLoadAttribute : OnMethodBoundaryAspect
    {
        #region Private Members

        private readonly string name_of_loaded;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="load">
        /// Name given to the attribute instance.
        /// </param>
        public LazyLoadAttribute(string load)
        {
            this.name_of_loaded = load;
        }

        #endregion

        #region Accessor

        public string LazyObjectName { get { return name_of_loaded; } }

        #endregion

        #region Public Post Sharp Overrides

        public override void OnEntry(MethodExecutionEventArgs args) { }

        public override void OnExit(MethodExecutionEventArgs args)
        {
            //if the instance with the attribute is not an EntityObject
            //then exit without doing anything. (prevents user error in
            //marking up an object with the attribute).
            if (args.Instance.GetType().IsAssignableFrom(typeof(EntityObject)))
                return;

            var fetchable = args.Instance as IFetchable;

            CacheAndInjectFetchingStrategy(fetchable);

            //its the root object in the query
            if ((fetchable != null) && (fetchable.FetchingStrategy != null))
            {
                DetermineStrategyAndLoad(args, fetchable);
            }
        }

        #endregion

        #region Static Private Worker Methods

        private static void DetermineStrategyAndLoad(MethodExecutionEventArgs args, IFetchable fetchable)
        {
            //does the aspects instance of IFetchable have a strategy
            //with Intentions declared for the current properties method
            //name that is executing where the intention is to lazy load
            var fetch_has_load = from f in fetchable.FetchingStrategy.Intentions
                                 where (f.FetchAssociate.Replace("Reference", "") ==
                                       args.Method.Name.Replace("get_", "").Replace("()", "") ||
                                       f.FetchAssociate ==
                                       args.Method.Name.Replace("get_", "")
                                        .Replace("()", "").Replace("Reference", "")) &&
                                       f.FetchMode == FetchMode.Lazy
                                 select f;

            if (fetch_has_load.Count() > 0)
            {
                LazyLoad(args, fetchable);
            }
        }

        private static void LazyLoad(MethodExecutionEventArgs args, IFetchable fetchable)
        {
            //the name of the method (accessor) being
            //called (intercepted here) with the string
            //Reference appended to it.
            string method_name_with_for_reference =
                args.Method.Name.Replace("get_", "").Replace("()", "") + "Reference";

            var reference_property = fetchable.GetType().
                GetProperty(method_name_with_for_reference);

            //Is the return value of the accessor being called
            //a ReferenceEntity or Collection? We are calling
            //the property name without "Reference" to avoid
            //any use of .IsLoaded and instead we intercept it
            //here. Hence as we are intercepting the PropertyName
            //and not PropertyNameReference - then the return
            //value in EF will be null.
            //NOTE: may be subsequently not null if lazy loaded
            //by a previous interception for a ReferenceEntity
            if (args.ReturnValue == null)
            {
                //is their a property that ends with the name
                //of the method being intercepted with the
                //string Reference appended to it?
                if (reference_property != null)
                {
                    //get the Reference version of the entity ref
                    var entity_reference = args.Instance.GetType().GetProperty(method_name_with_for_reference).
                        GetValue(args.Instance, null) as System.Data.Objects.DataClasses.EntityReference;

                    LoadEntityReferenceAndSetReturnValue(args, method_name_with_for_reference, entity_reference);
                }
            }
            else if (args.ReturnValue as RelatedEnd != null)
            {
                ((IRelatedEnd)args.ReturnValue).Load();
            }
        }

        private static void LoadEntityReferenceAndSetReturnValue(MethodExecutionEventArgs args,
            string method_name_with_for_reference, EntityReference entity_reference)
        {
            //is it an entity reference?
            if (entity_reference != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("Lazy Loadiing Entities of type {0}.{1} .............",
                    args.Method.DeclaringType.Name,
                    args.Method.Name.Replace("get_", "").Replace("()", "")));

                //load the Entity Reference
                entity_reference.Load();

                //Get the Entity Reference value
                var lazy_loaded_entity = args.Instance.GetType().GetProperty(method_name_with_for_reference).
                    GetValue(args.Instance, null).GetType().GetProperty("Value").
                    GetValue(args.Instance.GetType().GetProperty(method_name_with_for_reference).
                    GetValue(args.Instance, null), null);

                //set the Entity References value to the Return Value of the Accessor
                //captured here on Exit. If return value is not set then the code
                //calling the accessor will get a null reference exception
                args.ReturnValue = lazy_loaded_entity;
            }
        }

        private static void CacheAndInjectFetchingStrategy(IFetchable fetchable)
        {
            //Only root level will get their Fetching Strategies
            //injected via projection and we cant know ahead
            //of time what the object graph looks like
            //in a Repository<TEntity, TSession>.
            //We might be injecting redundantly because the
            //current entity for which we are dealing with
            //may not have a lazy fetching intention.
            //Cache the roots fetching strategy for the
            //properties that are also entities and cannot 
            //have their strategies projected into them
            if ((fetchable != null) && (fetchable.FetchingStrategy != null))
            {
                FetchingStrategyCache.LazyCachedStrategy = fetchable.FetchingStrategy;
            }
            else
            //it might be a subordinate of the root
            //so load it's fetching strategy from the cached roots copy
            if ((fetchable != null) && (fetchable.FetchingStrategy == null))
            {
                fetchable.FetchingStrategy = FetchingStrategyCache.LazyCachedStrategy;
            }
        }

        #endregion
    }

    #endregion
}

#endregion