// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhBadge : RealmObject
    {
        [Required]
        [MapTo("badgeType")]
        public string BadgeType { get; set; }

        [MapTo("level")]
        public long Level { get; set; }

        [MapTo("status")]
        public long Status { get; set; }

        [MapTo("obtainedTime")]
        public long ObtainedTime { get; set; }

        [MapTo("device")]
        public YhDevice Device { get; set; }
    }
}
