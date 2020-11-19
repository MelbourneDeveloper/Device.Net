namespace Microsoft.Extensions.Logging
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct EventId
#pragma warning restore CA1815 // Override equals and operator equals on value types
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