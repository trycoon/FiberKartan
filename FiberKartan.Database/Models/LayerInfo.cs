﻿using System;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "layerInfo")]
    public class LayerInfo
    {
        [DataMember(Name = "id")]
        public String Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}