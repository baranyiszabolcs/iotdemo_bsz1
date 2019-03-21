using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace bszASAEdge
{
    public class DistanceUDf
    {
        // Add your public static function
        public static bool DistanceAlert(Int64 distance)
        {
            return distance>100;
        }
    }
}
