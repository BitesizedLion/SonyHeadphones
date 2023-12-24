// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhAppLaunchCounter : RealmObject
    {
        [MapTo("year")]
        public long Year { get; set; }

        [MapTo("dayOfYear")]
        public long DayOfYear { get; set; }
    }
}
