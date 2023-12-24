// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhUseTime : RealmObject
    {
        [MapTo("startTime")]
        public long StartTime { get; set; }

        [MapTo("endTime")]
        public long EndTime { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }

        [MapTo("isTemporaryRecord")]
        public bool IsTemporaryRecord { get; set; }
    }
}
