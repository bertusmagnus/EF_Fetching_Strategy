#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using Org.TechAvalanche.Orm.LazyLoading;
using Org.TechAvalanche.Orm.Fetching;
using Org.Techavalanche.Orm.EF.Spike; 

#endregion

#region Namespace Declaration

namespace NorthwindModel
{
    #region Entity Declarations

    /// <summary>
    /// Business Logic behaviours.
    /// </summary>
    public partial class Order
    {
        partial void OnFreightChanging(decimal? value)
        {
            if (value == 100)
            {
                throw new ArgumentException("Freight cannot be greater or equal to 100!");
            }
        }
    }

    /// <summary>
    /// Put a custom attribute here on the partial
    /// where we have access. NOTE: the regex
    /// prevents weaving code into the properties
    /// of the entities that are Reference properties.
    /// For example Order_Detail.ProductReference will
    /// not be effected. This ensures that this framework
    /// will not interfere with Out of the box behaviour
    /// of the Entity Framework.
    /// </summary>
    [LazyLoad("Orders", AttributeTargetMembers = @"regex:(Orders)")]
    public partial class Customer : IFetchable, ICustomerMakePrefered
    {
        private IFetchingStrategy _strategy;

        public void Makeprefered() 
        {
            foreach (var order in this.Orders)
            {
                order.Freight = 0M;
            }
        }

        #region ILazyEntity Members

        public IFetchingStrategy FetchingStrategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }

        #endregion
    }

    /// <summary>
    /// The $ symbol ensures that regex does not let
    /// postsharp weave code into the property accessor
    /// for CustomerReference as a wildcard * for simply
    /// Customer is assumed by default.
    /// </summary>
    [LazyLoad("Customer", AttributeTargetMembers = @"regex:(Customer$)")]
    [LazyLoad("Order_Details", AttributeTargetMembers = @"regex:(Order_Details$)")]
    public partial class Order : IFetchable
    {
        private IFetchingStrategy _strategy;
        public IFetchingStrategy FetchingStrategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }
    }

    [LazyLoad("Product", AttributeTargetMembers = @"regex:(Product$)")]
    [LazyLoad("Order", AttributeTargetMembers = @"regex:(Order$)")]
    public partial class Order_Detail : IFetchable
    {
        private IFetchingStrategy _strategy;
        public IFetchingStrategy FetchingStrategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }
    }

    [LazyLoad("Category", AttributeTargetMembers = @"regex:(Category$)")]
    public partial class Product : IFetchable
    {
        private IFetchingStrategy _strategy;
        public IFetchingStrategy FetchingStrategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }
    }

    #endregion
}

#endregion