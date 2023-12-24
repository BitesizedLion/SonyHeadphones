// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhActivityTimeInfo : RealmObject
    {
        [Required]
        [MapTo("actionKind")]
        public string ActionKind { get; set; }

        [MapTo("accumulatedTime")]
        public long AccumulatedTime { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }
    }
}
