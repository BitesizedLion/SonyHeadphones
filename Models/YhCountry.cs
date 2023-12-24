// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhCountry : RealmObject
    {
        [Required]
        [MapTo("countryCode")]
        public string CountryCode { get; set; }

        [MapTo("arrival")]
        public long Arrival { get; set; }

        [MapTo("deviceInformation")]
        public YhDevice DeviceInformation { get; set; }
    }
}
