// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhDevice : RealmObject
    {
        [PrimaryKey]
        [Required]
        [MapTo("deviceId")]
        public string DeviceId { get; set; }

        [Required]
        [MapTo("deviceName")]
        public string DeviceName { get; set; }

        [MapTo("modelColor")]
        public long ModelColor { get; set; }
    }
}
