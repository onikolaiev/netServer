using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terra.Greedy
{
    internal enum PointState : byte
    {
        Unused = 0,
        Used = 1,
        Ignored = 2,
        Unknown = 3
    }
}
