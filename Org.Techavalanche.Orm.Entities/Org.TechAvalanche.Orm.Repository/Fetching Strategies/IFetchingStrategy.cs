#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Fetching
{
    #region IFetchingStrategy Interface Declarations

    public interface IFetchingStrategy
    {
        IList<FetchingIntention> Intentions { get; }
    }

    public interface IFetchingStrategy<TRole> : IFetchingStrategy { } 

    #endregion
}

#endregion