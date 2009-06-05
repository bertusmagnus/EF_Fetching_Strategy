#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Fetching
{
    #region Interface Declaration

    /// <summary>
    /// Interface used in creating Entities
    /// that are identifiable as opting in
    /// to the Fetching Framework.
    /// </summary>
    public interface IFetchable
    {
        IFetchingStrategy FetchingStrategy { get; set; }
    } 

    #endregion
}

#endregion