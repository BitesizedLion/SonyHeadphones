// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhCounter : RealmObject
    {
        [MapTo("countType")]
        public long CountType { get; set; }

        [MapTo("count")]
        public long Count { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }

        [MapTo("lastCountTime")]
        public long LastCountTime { get; set; }
    }
}
