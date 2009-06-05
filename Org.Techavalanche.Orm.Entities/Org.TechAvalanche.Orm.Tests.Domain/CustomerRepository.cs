#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthwindModel;
using Org.TechAvalanche.Orm.Repository;
using System.Data.Objects; 
using Org.TechAvalanche.Orm;
using Org.TechAvalanche.Orm.Specification;

#endregion

#region Namespace Declaration

namespace Org.Techavalanche.Orm.EF.Spike
{
    /// <summary>
    /// A trivial specialised repository with
    /// a custom method not present in the
    /// generic repository. Typical DDD repositories
    /// follow this specific way of working.
    /// </summary>
    public class CustomerRepository :
        EntitiesRepository<Customer, NorthwindEntities>
    {
        #region Private Members

        static readonly Func<NorthwindEntities, string, IQueryable<Customer>> a_compiledQuery2 =
            CompiledQuery.Compile<NorthwindEntities, string, IQueryable<Customer>>(
            (ctx, country) => from cust in ctx.Customers
                              where cust.Country == country
                              select cust);

        static readonly Func<NorthwindEntities, IQueryable<Customer>> b_compiledQuery2 =
            CompiledQuery.Compile<NorthwindEntities, IQueryable<Customer>>(
            (ctx) => from cust in ctx.Customers
                     .Where(c => c.Country == "Germany")
                     select cust); 

        #endregion

        #region Public Methods - Queries

        public IQueryable<Customer> GetCustomerByCountry(string country)
        {
            var custs = from c in Session.Customers
                        where c.Country == country
                        select c;

            return custs;
        }

        public IQueryable<Customer> GetCompiledCustomerByCountry()
        {
            var custs = b_compiledQuery2.Invoke(Session);
            return custs;
        }

        public IQueryable<Customer> GetCompiledCustomerByCountry(string country)
        {
            var custs = a_compiledQuery2.Invoke(Session, country);
            return custs;
        } 

        #endregion

        #region Constructors


        /// <summary>
        /// Uses the base constructor
        /// to inject the Unit of work
        /// 'session'. (should new up the
        /// session outside of here if
        /// it's being used accross 
        /// repositories. Also change to
        /// add the context to the constructor
        /// parameters as well)
        /// </summary>
        public CustomerRepository() :
            base(new NorthwindEntities()) { }

        /// <summary>
        /// Uses the base constructor
        /// to inject the Unit of work
        /// 'session'. (should new up the
        /// session outside of here if
        /// it's being used accross 
        /// repositories. Also change to
        /// add the context to the constructor
        /// parameters as well)
        /// </summary>
        public CustomerRepository(NorthwindEntities session) :
            base(session) { }

        #endregion
    }
}

#endregion