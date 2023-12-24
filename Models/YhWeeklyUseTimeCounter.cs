// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhWeeklyUseTimeCounter : RealmObject
    {
        [MapTo("years")]
        public long Years { get; set; }

        [MapTo("months")]
        public long Months { get; set; }

        [MapTo("count")]
        public long Count { get; set; }
    }
}
