// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhHeadphonesUsageDay : RealmObject
    {
        [PrimaryKey]
        [Required]
        [MapTo("pk")]
        public string Pk { get; set; }

        [MapTo("year")]
        public long Year { get; set; }

        [MapTo("month")]
        public long Month { get; set; }

        [MapTo("day")]
        public long Day { get; set; }

        [MapTo("weekNumber")]
        public long WeekNumber { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }
    }
}
