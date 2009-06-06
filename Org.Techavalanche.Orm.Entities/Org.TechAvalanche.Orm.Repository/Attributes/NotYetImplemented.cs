#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Repository.Attributes
{
    #region Class Declaration

    /// <summary>
    /// Meta Attribute.
    /// </summary>
    public class NotYetImplemented : Attribute
    {
        #region Private Members

        private string _reason;

        #endregion

        #region Accessor Mutators

        public string Reason
        {
            get { return _reason; }
        }

        #endregion

        #region Constructors

        private NotYetImplemented() { }

        public NotYetImplemented(string reason)
        {
            _reason = reason;
        }

        #endregion
    }

    #endregion
} 

#endregion