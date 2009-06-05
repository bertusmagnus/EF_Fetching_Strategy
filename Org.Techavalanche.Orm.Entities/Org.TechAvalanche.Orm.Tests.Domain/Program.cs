#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthwindModel;
using NUnit;
using Moq;
using System.Data.Objects;
using Org.TechAvalanche.Orm.Specification;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Diagnostics;
using Org.TechAvalanche.Orm.Repository;
using System.Data.Objects.DataClasses;
using Org.TechAvalanche.Orm.Fetching;
using Org.Techavalanche.Orm.EF.Spike.Strategies;
using Org.Techavalanche.Orm.EF.Spike;
using Org.TechAvalanche.Orm.Repository.Attributes; 

#endregion

namespace ConsoleApplication1
{
    [TestFixture()]
    public class Program
    {
        static void Main() { }

        #region IList Code Demonstration and Tests

        /// <summary>
        /// Tests the entity equality matching
        /// via a specification.
        /// </summary>
        [Test()]
        public void TestExpressions()
        {
            Order o1 = new Order();
            o1.OrderID = 1;
            o1.Customer = new Customer();
            o1.Customer.Country = "Australia";

            Specification<Order> firstSpec =
                new Specification<Order>(o => o.OrderID == 1);

            Specification<Order> secondSpec =
                new Specification<Order>(o => o.Customer.Country == "Australia");

            var orSpec = firstSpec | secondSpec;

            Console.WriteLine("The Order o1 matches? {0}", orSpec.Matches(o1));

            Assert.IsTrue(orSpec.Matches(o1));
        }

        /// <summary>
        /// Tests a simple specification scenario
        /// where we wish to find all discontinued
        /// products in orders on hand.
        /// </summary>
        [Test()]
        public void SimpleSpecificationExample()
        {
            Specification<Order> order_spec =
                new Specification<Order>(o => o.Order_Details.All
                    (od => od.Product is DiscontinuedProduct));

            using (var ctx = new NorthwindEntities())
            {
                ObjectQuery<Order> orders =
                    (ObjectQuery<Order>)
                    from o in ctx.Orders.Include("Customer")
                        .Include("Order_Details")
                        .Include("Order_Details.Product")
                    .Where(order_spec.EvalPredicate)
                    select o;
               
                Console.WriteLine(orders.ToTraceString());

                foreach (var order in orders)
                {
                    if (order.Order_Details.Count() > 0)
                    {
                        var sumup =
                            order.Order_Details.Sum
                            (od => od.Quantity * od.UnitPrice);
                        Console.WriteLine("The sum of orders for order ID: " +
                            "{0} is {1} and Country is {2}",
                            order.OrderID.ToString(), sumup.ToString(),
                            order.Customer.Country);
                    }
                }

                //test if all the products retrieved are indeed discontinued
                var discontinued = orders.Count(o => o.Order_Details.Count
                    (od => od.Product as DiscontinuedProduct == null) > 1);

                Assert.AreEqual(discontinued, 0);
            }
        }

        /// <summary>
        /// Tests a simple specification scenario
        /// where we wish to find all discontinued
        /// products in orders on hand and that the
        /// Fetching Framework does NOT intercede
        /// in the reqular EF behaviour.
        /// </summary>
        [Test()]
        public void DepthOfIncludesTest()
        {
            int order_details_count = 0;
            bool customer_loaded = false;
            bool product_loaded = false;

            Specification<Order> order_spec =
                new Specification<Order>(o => o.Order_Details.All
                    (od => od.Product is DiscontinuedProduct));

            using (var ctx = new NorthwindEntities())
            {
                ObjectQuery<Order> orders =
                    (ObjectQuery<Order>)
                    from o in ctx.Orders
                        .Include("Customer")
                        .Include("Order_Details")
                        .Include("Order_Details.Product")
                        .Where(order_spec.EvalPredicate)
                    select o;

                Console.WriteLine(orders.ToTraceString());

                foreach (var order in orders)
                {
                    order_details_count = order.Order_Details.Count();
                    if (order.Customer != null) { customer_loaded = true; }
                    Console.WriteLine("Customer {0} has Order ID : {0}", 
                        order.Customer.CompanyName, order.OrderID);
                    foreach (var order_line in order.Order_Details)
                    {
                        if (order_line.Product != null) { product_loaded = true; }
                        Console.WriteLine("\tLine Item is for {0} X {1}", 
                            order_line.Quantity, order_line.Product.ProductName);
                    }
                    if (order_details_count > 0 && customer_loaded && product_loaded)
                    {
                        break;
                    }
                }
            }
            Assert.IsTrue(order_details_count > 0 && 
                customer_loaded && product_loaded);
        }

        /// <summary>
        /// Tests a more complex specification that
        /// has been created by combining multiple
        /// specifications together through AND and
        /// OR operators. Also that the Fetching Framework 
        /// does NOT intercede in the reqular EF behaviour
        /// and the explicit calls to lazy load are
        /// both required and made.
        /// </summary>
        [Test()]
        public void CombinedOrSpecExample()
        {
            Specification<Order> country_customer_spec =
                new Specification<Order>
                    (o => o.Customer.Country == "Germany" || 
                        o.Customer.Country == "Usa");

            Specification<Order> german_customer_spec =
                new Specification<Order>
                    (o => o.Customer.Country == "Germany");

            Specification<Order> us_customer_spec =
                new Specification<Order>
                    (o => o.Customer.Country == "Usa");

            Specification<Order> orderid_spec =
                new Specification<Order>(o => o.OrderID != 10279);

            Specification<Order> ords_with_discontinued_prods =
                new Specification<Order>(o => o.Order_Details.All
                    (od => od.Product is DiscontinuedProduct));

            var orSpec = ords_with_discontinued_prods & 
                orderid_spec & (german_customer_spec | us_customer_spec);

            using (var ctx = new NorthwindEntities())
            {
                //note the eager loading here
                //and still further down we have
                //to lazy load the same data
                //as eager loading has not worked
                //as expected.
                var discontinued_products_on_order =
                    (ObjectQuery<Order_Detail>)
                    from o in ctx.Orders
                        .Include("Order_Details")
                        .Include("Customer")
                    .Where(orSpec.EvalPredicate)
                    from od in ctx.OrderDetails
                    where od.OrderID == o.OrderID
                    select od;

                Console.WriteLine(discontinued_products_on_order.ToTraceString());

                foreach (var ord_detail in discontinued_products_on_order)
                {
                    if (!ord_detail.OrderReference.IsLoaded)
                    {
                        ord_detail.OrderReference.Load();
                    }
                    if (!ord_detail.ProductReference.IsLoaded)
                    {
                        ord_detail.ProductReference.Load();
                    }
                    if (!ord_detail.Order.CustomerReference.IsLoaded)
                    {
                        ord_detail.Order.CustomerReference.Load();
                    }
                    Console.WriteLine("The OrderID: {0} the product name: {1} the customers location {2}",
                        ord_detail.OrderID, ord_detail.Product.ProductName, ord_detail.Order.Customer.Country);
                }

                Assert.IsTrue(discontinued_products_on_order
                              .Count(p => p.Order.Customer.Country != "Germany" &&
                                          p.Order.Customer.Country != "USA") == 0);
            }
        }

        /// <summary>
        /// Demostates combined specifications
        /// however this time using more than one across
        /// multiple where extension methods in a query
        /// that joins tables. See the TSQL output.
        /// </summary>
        [Test()]
        public void RefineCombinedSpecifications()
        {
            Specification<Customer> german_customer_spec =
                new Specification<Customer>(c => c.Country == "Germany");

            Specification<Customer> us_customer_spec =
                new Specification<Customer>(c => c.Country == "Usa");

            Specification<Product> ords_with_discontinued_prods =
                new Specification<Product>(p => p is DiscontinuedProduct);

            var comb_country_spec = (german_customer_spec | us_customer_spec);

            using (var ctx = new NorthwindEntities())
            {
                var discontinued_products_on_order =
                    (ObjectQuery<Product>)
                    (from p in ctx.Products
                    .Where(ords_with_discontinued_prods.EvalPredicate)
                    from od in ctx.OrderDetails
                    from o in ctx.Orders
                    from c in ctx.Customers
                    .Where(comb_country_spec.EvalPredicate)
                    where od.ProductID == p.ProductID &&
                          o.OrderID == od.OrderID &&
                          c.CustomerID == o.Customer.CustomerID
                    select p).Distinct();

                Console.WriteLine(discontinued_products_on_order.ToTraceString());

                foreach (var product in discontinued_products_on_order)
                {
                    Console.WriteLine("The product ID is {0} with a name of {1}",
                        product.ProductID, product.ProductName);
                }
            }
        }

        /// <summary>
        /// Tests a specialized Repository that contains
        /// non generic methods and demonstrates EF lazy loading
        /// </summary>
        [Test()]
        public void TestEfRepository()
        {
            CustomerRepository cust_repos = new CustomerRepository();

            var custs = cust_repos.GetCustomerByCountry("Germany");

            foreach (var cust in custs)
            {
                //enlist lazy loading (of sorts)
                if (cust.Orders.IsLoaded == false)
                {
                    cust.Orders.Load();
                }
                Console.WriteLine("Customer ID: {0} from {1} and " +
                    "Contacts is {2} and Number of Orders are: {3}",
                    new object[] { cust.CustomerID, cust.Country, 
                        cust.ContactName, cust.Orders.Count() });
            }

            int custs_not_german = custs.Count(c => c.Country != "Germany");
            Assert.AreEqual(custs_not_german, 0);
        }

        /// <summary>
        /// Demonstrates the use of a generic
        /// repository without using inheritance.
        /// </summary>
        [Test()]
        public void TestUniqueWithSave()
        {
            //new up the repository
            EntitiesRepository<Customer, NorthwindEntities> ctx =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            //new up a specification
            Specification<Customer> cust_spec =
                new Specification<Customer>(c => c.Country == "Germany" && c.CustomerID == "KOENE");

            var unique_german_cust = ctx.Unique("Customers", cust_spec);

            var rand_num = new Random(DateTime.Now.Year).Next();

            unique_german_cust.Orders.Add(new Order() { ShipName = "Nikovshi" + rand_num.ToString()  });

            ctx.Save();

            Assert.IsNotNull(unique_german_cust);

            Console.WriteLine("The Unique customer ID is : {0}", unique_german_cust.CustomerID);

        }

        /// <summary>
        /// Demonstrates the AllToIList method with
        /// a fetching strategy for customers to 
        /// make prefered.
        /// </summary>
        [Test()]
        public void TestFetchingStrateyAllIlist()
        {
            //new up the repository
            EntitiesRepository<Customer, NorthwindEntities> ctx =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            //new up a specification
            Specification<Customer> cust_spec =
                new Specification<Customer>(c => c.Country == "Germany" && c.CustomerID == "KOENE");

            MakePreferedFetchingStrategy stratey = new MakePreferedFetchingStrategy();

            var list = ctx.AllToIList(stratey);
            
            Assert.IsTrue(list.ToList().Count > 0);
        }

        /// <summary>
        /// Demonstrates failing the Unique
        /// query and also if one looks at the
        /// body of the .Unique method in the
        /// repository then you will notice that
        /// the .First() method has been used
        /// with a count prior to test for multiple
        /// results. This is due tot he EF NOT supporting
        /// the .Single() extension method.
        /// </summary>
        [Test()]
        [ExpectedException(typeof(NotUniqueEntityException))]
        public void FailTestForUnique()
        {
            //new up the repository
            EntitiesRepository<Customer, NorthwindEntities> ctx =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            //new up a specification
            Specification<Customer> cust_spec =
                new Specification<Customer>(c => c.Country == "Germany");

            var unique_german_cust = ctx.Unique("Customers", cust_spec);

            Assert.IsNotNull(unique_german_cust);

            Console.WriteLine("The Unique customer ID is : {0}", unique_german_cust.CustomerID);
        }

        /// <summary>
        /// Demonstrates that the Fetching is only
        /// enlisted in an opt in fashion. The inner
        /// loop will not run as no Loading instruction
        /// has been specified either in the EF or by
        /// a fetching strategy.
        /// </summary>
        [Test()]
        public void TestInterceptLazyLoad()
        {
            bool lazy_loaded = false;

            CustomerRepository cust_repos = new CustomerRepository();

            var custs = cust_repos.GetCustomerByCountry("Germany");

            foreach (var cust in custs)
            {
                //test the lazy loading does not kick
                //as we have not specified to use it
                //by virtue of supplying a fetching strategy
                foreach (var order in cust.Orders)
                {
                    lazy_loaded = true;
                    Console.WriteLine("Customer ID {0} has Order ID {1}",
                        cust.CustomerID, order.OrderID);
                }
            }

            int custs_not_german = custs.Count(c => c.Country != "Germany");

            Assert.AreEqual(custs_not_german, 0);
            Assert.IsFalse(lazy_loaded);
        }

        /// <summary>
        /// Basic demonstration of the fetching strategy
        /// and Specification being used in combination.
        /// </summary>
        [Test()]
        public void TestWithFetchingStrategyParser()
        {
            CustomerRepository repos = new CustomerRepository();
            Specification<Customer> italian_spec =
                new Specification<Customer>(c => c.Country == "Italy");
            var italian_custs = repos.AllToIList(new MakePreferedFetchingStrategy());
            
            foreach (var cust in italian_custs)
            {
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("Order Number : {0}", order.OrderID);
                }
            }
        }

        /// <summary>
        /// Demonstrate a fetching stratey that uses 
        /// eager fetching exclusively. Should eager
        /// load customers and order lines for orders.
        /// </summary>
        [Test()]
        public void FetchingEagerOnly()
        {
            EntitiesRepository<Order, NorthwindEntities> repos = 
                new EntitiesRepository<Order,NorthwindEntities>(new NorthwindEntities());

            Specification<Order> italian_spec =
                new Specification<Order>(o => o.Customer.Country == "Italy");

            var italian_orders = repos.AllToIList(new EagerOnlyOrdersFetchingStrategy());

            foreach (var order in italian_orders)
            {
                Console.WriteLine("The Customer Name is {0}", order.Customer.CompanyName);
                foreach (var orderLine in order.Order_Details)
                {
                    Console.WriteLine("\tThe value ordered for Product ID {0} is {1}",
                        orderLine.ProductID, orderLine.UnitPrice * orderLine.Quantity);
                }
            }
        }

        /// <summary>
        /// Demonstrate lazy fetching for an entity reference
        /// being combined with eager fetching for an entity
        /// collection for instances of Orders. Customers
        /// will be loaded lazy and Order Lines will be eager.
        /// </summary>
        [Test()]
        public void Fetching_Lazy_EntityReference_Eager_EntityCollection()
        {
            EntitiesRepository<Order, NorthwindEntities> repos =
                new EntitiesRepository<Order, NorthwindEntities>(new NorthwindEntities());

            Specification<Order> italian_spec =
                new Specification<Order>(o => o.Customer.Country == "Italy");

            var italian_orders = repos.AllToIList(new LazyEntityReferenceEagerEntityCollection());

            foreach (var order in italian_orders)
            {
                Console.WriteLine("The Customer Name is {0}", order.Customer.CompanyName);
                foreach (var orderLine in order.Order_Details)
                {
                    Console.WriteLine("\tThe value ordered for Product ID {0} is {1}",
                        orderLine.ProductID, orderLine.UnitPrice * orderLine.Quantity);
                }
            }
        }

        /// <summary>
        /// Demonstrates the combination of eager fetching an
        /// EntityReference and Lazy Loading and Entity collection
        /// all specified by a FetchingStrategy.
        /// </summary>
        [Test()]
        public void Fetching_Eager_EntityReference_Lazy_EntityCollection()
        {
            EntitiesRepository<Order, NorthwindEntities> repos =
                new EntitiesRepository<Order, NorthwindEntities>(new NorthwindEntities());

            var orders = repos.AllToIList(new EagerEntityReferenceLazyEntityCollection());

            foreach (var order in orders)
            {
                //customer is eager
                Console.WriteLine("The Customer Name is {0}", order.Customer.CompanyName);
                foreach (var orderLine in order.Order_Details)
                {
                    //order details is lazy
                    Console.WriteLine("\tThe value ordered for Product ID {0} is {1}",
                        orderLine.ProductID, orderLine.UnitPrice * orderLine.Quantity);
                }
            }
        }

        /// <summary>
        /// Demonstrate eager fetching deeper than 1
        /// level deep in the roots graph.
        /// </summary>
        [Test()]
        public void Multi_Level_Eager_Only_Hierarchy_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            var custs = repos.AllToIList(new MultiLevelEagerOnlyStrategy());

            foreach (var cust in custs)
            {
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }
        }

        /// <summary>
        /// Demonstrate lazy loading deeper than 1
        /// level deep in the roots graph and over
        /// both Entity collection and references.
        /// </summary>
        [Test()]
        public void Multi_Level_Lazy_Only_Hierarchy_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            var custs = repos.AllToIList(new MultiLevelLazyOnlyStrategy());

            foreach (var cust in custs)
            {
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }
        }

        /// <summary>
        /// Demonstrate lazy loading deeper than 1
        /// level deep in the roots graph and over
        /// both Entity collection and references.
        /// </summary>
        [Test()]
        public void Multi_Level_Mixed_Hierarchy_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            var custs = repos.AllToIList(new MultiLevelMixedStrategy());

            foreach (var cust in custs)
            {
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }
        }

        /// <summary>
        /// Demonstrate lazy loading deeper than 1
        /// level deep in the roots graph and over
        /// both Entity collection and references
        /// and with persistance over the entities
        /// regardless of their place in the relationships.
        /// </summary>
        [Test()]
        public void Multi_Level_Mixed_Hierarchy_With_Persisting_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            Specification<Customer> german_cust_spec =
                new Specification<Customer>(c => c.CustomerID == "ALFKI");

            var german_custs = repos.AllToIList(german_cust_spec, new
                                MultiLevelMixedStrategy());
            
            foreach (var cust in german_custs)
            {
                cust.Country = "Germany";
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    order.ShipName = "Battleship Potemkin";
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        orderline.Discount = 0.15f;
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }

            repos.Save();
        }

        #endregion

        #region IQueryable Code Demonstration and Tests

        /// <summary>
        /// Demonstrate lazy loading deeper than 1
        /// level deep in the roots graph and over
        /// both Entity collection and references
        /// and with persistance over the entities
        /// regardless of their place in the relationships.
        /// </summary>
        /// <remarks>
        /// NOTE:NOTE:NOTE:
        /// The Fetching strategy will also be injected
        /// using LINQ defered execution. The FetchingParser
        /// will execute the ObjectQuery with eager fetching
        /// as expected due to the .ToList() call on the 
        /// ObjectQuery, however the .Select() extension method
        /// will have it's execution defered until the
        /// root in the Repository for the query is being
        /// requested explcitly in code.
        /// <see cref="FetchParser.ParseToQueryable"/>
        /// </remarks>
        [Test()]
        public void Test_IQueryable()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());
            var query = repos.AllToIQueryable(new MultiLevelMixedStrategy());

            foreach (var cust in query)
            {
                Console.WriteLine(">> Customer ordering is {0}", cust.CompanyName);
                foreach (var ord in cust.Orders)
                {
                    Console.WriteLine("\t-- The customer ID is {0} and the number of lines is {1}", 
                        ord.Customer.CustomerID, ord.Order_Details.Count());
                    foreach (var line in ord.Order_Details)
                    {
                        Console.WriteLine("\t\t++ Product ordered is {0}", 
                            line.Product.ProductName);
                    }
                }
            }
        }

        #endregion

        #region FetchingIntention Tests

        [Test()]
        public void Name_Of_Intention_Test()
        {
            var orders_lazy = 
                FetchingIntention.CreateInstance
                <Customer, EntityCollection<Order>>
                (c => c.Orders, FetchMode.Lazy);
            var order_details_lazy = 
                FetchingIntention.CreateInstance
                <Order, EntityCollection<Order_Detail>>
                (o => o.Order_Details, FetchMode.Lazy);

            var full_intent = 
                orders_lazy.And(order_details_lazy);

            Assert.AreEqual(full_intent.FetchAssociate, 
                "Orders.Order_Details");
        }

        [Test()]
        public void Role_Intention_Compound_Test()
        {
            RoleStrategy rs = new RoleStrategy();

            var ord_details_product_intent = 
                    from i in rs.Intentions
                    where i.FetchAssociate == "Orders.Order_Details"
                    select i;

            Assert.AreEqual(rs.Intentions.Count, 3);
            Assert.AreEqual(ord_details_product_intent.Count(), 1);
        }

        [Test()]
        public void Lambda_Built_Strategy_With_Hierarchies_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            var custs = repos.AllToIList(new RoleStrategy());

            foreach (var cust in custs)
            {
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }
        }

        [Test(), ExpectedException(typeof(NotImplementedException))]
        public void Example_Role_Specified_Strategy_Test()
        {
            EntitiesRepository<Customer, NorthwindEntities> repos =
                new EntitiesRepository<Customer, NorthwindEntities>(new NorthwindEntities());

            var custs = repos.AllToIList<ICustomerMakePrefered>();

            foreach (var cust in custs)
            {
                Console.WriteLine("The Customer Name is {0}", cust.CompanyName);
                foreach (var order in cust.Orders)
                {
                    Console.WriteLine("\tThe Order ID is : {0}", order.OrderID);
                    foreach (var orderline in order.Order_Details)
                    {
                        Console.WriteLine("\t\tThe value ordered for Product ID {0} is {1}",
                            orderline.Product.ProductID, orderline.UnitPrice * orderline.Quantity);
                    }
                }
            }
        }

        #endregion

        #region Compiled Query Tests

        [Test()]
        public void compiled_cust_by_country()
        {
            CustomerRepository cust_repo =
                new CustomerRepository();
            var german_custs = cust_repo.GetCompiledCustomerByCountry("Germany");

            foreach (var cust in german_custs)
            {
                Console.WriteLine("The customer is from {0}", cust.Country);
            }
        }

        [Test()]
        public void compiled_cust_by_country_expression()
        {
            CustomerRepository cust_repo =
                new CustomerRepository();
            var german_custs = cust_repo.GetCompiledCustomerByCountry();

            foreach (var cust in german_custs)
            {
                Console.WriteLine("The customer is from {0}", cust.Country);
            }
        }
        #endregion

        #region Mocks

        [Test()]
        public void mock_customers_only_where_in_germany()
        {
            NorthwindEntities fake_entity_model = null;
            var mock_repo = 
                new Mock<EntitiesRepository<Customer, NorthwindEntities>>
                    (fake_entity_model);

            var germany_cust_spec = 
                new Specification<Customer>(c => c.Country == "Germany");

            var orders = new EntityCollection<Order>();
            orders.Add(new Order{Freight = 10M});
            orders.Add(new Order{Freight = 23M});

            mock_repo.Expect
                (repo => repo.AllToIList<ICustomerMakePrefered>())
                .Returns(new List<Customer>()
                {
                    new Customer()
                    {
                        CustomerID = "ALFKI", 
                        Country = "Germany", 
                        Orders = orders
                    },
                    new Customer()
                    {
                        CustomerID = "SIMSE", Country = "Australia"
                    }
                });

            var results = mock_repo.Object.AllToIList<ICustomerMakePrefered>();

            var cust = results.Where(germany_cust_spec.EvalFunc).First();

            cust.Makeprefered();

            Assert.That(cust.Orders.Sum(o => o.Freight) == 0);
            Assert.AreEqual(results.Count, 2);
            Assert.AreEqual(results.First().Country, "Germany");
            Assert.AreEqual(mock_repo.Object.AllToIList<ICustomerMakePrefered>().Count, 2);
        }
        #endregion
    }
}
