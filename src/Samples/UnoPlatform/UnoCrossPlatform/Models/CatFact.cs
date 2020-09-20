using System;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming Styles

namespace UnoCrossPlatform.Models
{
    public class CatFact
    {
        public bool used { get; set; }
        public string? source { get; set; }
        public string? type { get; set; }
        public bool deleted { get; set; }
        public string? _id { get; set; }
        public int __v { get; set; }
        public string? text { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public Status? status { get; set; }
        public string? user { get; set; }
    }
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1707 // Identifiers should not contain underscores
