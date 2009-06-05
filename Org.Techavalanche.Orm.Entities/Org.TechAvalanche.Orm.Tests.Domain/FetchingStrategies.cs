#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.TechAvalanche.Orm.Fetching;
using NorthwindModel;
using System.Data.Objects.DataClasses; 

#endregion

#region Namespace Declaration

namespace Org.Techavalanche.Orm.EF.Spike.Strategies
{
    #region Fetching Strategies

    public class MakePreferedFetchingStrategy :
        IFetchingStrategy<ICustomerMakePrefered>
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public MakePreferedFetchingStrategy()
        {
            this.Intentions.Add(new FetchingIntention("Orders", FetchMode.Lazy));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class EagerOnlyOrdersFetchingStrategy :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public EagerOnlyOrdersFetchingStrategy()
        {
            this.Intentions.Add(new FetchingIntention("Customer", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Order_Details", FetchMode.Eager));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class LazyEntityReferenceEagerEntityCollection :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public LazyEntityReferenceEagerEntityCollection()
        {
            this.Intentions.Add(new FetchingIntention("Customer", FetchMode.Lazy));
            this.Intentions.Add(new FetchingIntention("Order_Details", FetchMode.Eager));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class EagerEntityReferenceLazyEntityCollection :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public EagerEntityReferenceLazyEntityCollection()
        {
            this.Intentions.Add(new FetchingIntention("Customer", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Order_Details", FetchMode.Lazy));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class MultiLevelEagerOnlyStrategy :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public MultiLevelEagerOnlyStrategy()
        {
            this.Intentions.Add(new FetchingIntention("Orders", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Orders.Order_Details", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Orders.Order_Details.Product", FetchMode.Eager));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class MultiLevelLazyOnlyStrategy :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public MultiLevelLazyOnlyStrategy()
        {
            this.Intentions.Add(new FetchingIntention("Orders", FetchMode.Lazy));
            this.Intentions.Add(new FetchingIntention("Order_Details", FetchMode.Lazy));
            this.Intentions.Add(new FetchingIntention("Product", FetchMode.Lazy));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class MultiLevelMixedStrategy :
        IFetchingStrategy
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public MultiLevelMixedStrategy()
        {
            this.Intentions.Add(new FetchingIntention("Orders", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Orders.Order_Details", FetchMode.Eager));
            this.Intentions.Add(new FetchingIntention("Product", FetchMode.Lazy));
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    public class RoleStrategy :
        IFetchingStrategy<ICustomerMakePrefered>
    {
        private readonly IList<FetchingIntention> _intentions =
            new List<FetchingIntention>();

        public RoleStrategy()
        {
            var order_intention = 
                FetchingIntention.CreateInstance
                <Customer, EntityCollection<Order>>
                (c => c.Orders, FetchMode.Eager);

            var order_details_intention = order_intention.And(
                FetchingIntention.CreateInstance
                <Order, EntityCollection<Order_Detail>>
                (o => o.Order_Details, FetchMode.Eager));

            var product_intention = FetchingIntention.CreateInstance
                <Order_Detail, Product>
                (od => od.Product, FetchMode.Lazy);

            this.Intentions.Add(order_intention);
            this.Intentions.Add(order_details_intention);
            this.Intentions.Add(product_intention);
        }

        #region IFetchingStrategy Members

        public IList<FetchingIntention> Intentions
        {
            get { return _intentions; }
        }

        #endregion
    }

    #endregion
}

#endregion