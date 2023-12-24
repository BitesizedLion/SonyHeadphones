// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhPlaceTimeInfo : RealmObject
    {
        [MapTo("entryTime")]
        public long EntryTime { get; set; }

        [MapTo("exitTime")]
        public long ExitTime { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }

        [MapTo("placeInformation")]
        public YhPlaceInfo PlaceInformation { get; set; }

        [MapTo("isTemporaryRecord")]
        public bool IsTemporaryRecord { get; set; }
    }
}
