using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.TechAvalanche.Orm.Fetching
{
    public static class FetchingStrategyCache
    {
        [ThreadStatic()]
        private static IFetchingStrategy _lazyCacheStrategy;

        public static IFetchingStrategy LazyCachedStrategy
        {
            get { return _lazyCacheStrategy; }
            set { _lazyCacheStrategy = value; }
        }
    }
}
