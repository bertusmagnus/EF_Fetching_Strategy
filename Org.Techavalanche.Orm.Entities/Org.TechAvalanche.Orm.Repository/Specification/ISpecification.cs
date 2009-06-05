#region Imported Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Specification
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> EvalPredicate { get; }
        Func<T, bool> EvalFunc { get; }
        bool Matches(T entity);
    }
}

#endregion