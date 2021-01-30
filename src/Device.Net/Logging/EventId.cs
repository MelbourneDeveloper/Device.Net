#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microsoft.Extensions.Logging
{
    public readonly struct EventId
    {
        public EventId(int id, string name = null)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1815 // Override equals and operator equals on value types
