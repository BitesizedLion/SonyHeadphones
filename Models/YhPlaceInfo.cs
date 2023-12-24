// Please note : [Backlink] properties and default values are not represented
// in the schema and thus will not be part of the generated models

using System;
using System.Collections.Generic;
using Realms;

namespace SonyHeadphones.Models
{
    public class YhPlaceInfo : RealmObject
    {
        [PrimaryKey]
        [Required]
        [MapTo("placeId")]
        public string PlaceId { get; set; }

        [Required]
        [MapTo("placeName")]
        public string PlaceName { get; set; }

        [Required]
        [MapTo("placeDisplayType")]
        public string PlaceDisplayType { get; set; }
    }
}
