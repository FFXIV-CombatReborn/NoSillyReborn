namespace NoSillyReborn
{
    public sealed class ActionIntercept(uint originalId, uint replacementId, string description)
    {
        public uint OriginalActionId { get; } = originalId;

        public uint ReplacementActionId { get; } = replacementId;

        public string Description { get; } = description;
    }
}
