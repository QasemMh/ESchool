using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Constants
{
    public class SubjectComparer : IEqualityComparer<SubjectDetails>
    {
        public bool Equals(SubjectDetails x, SubjectDetails y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] SubjectDetails obj)
        {
            return (int)obj.Id;

        }
    }
}
