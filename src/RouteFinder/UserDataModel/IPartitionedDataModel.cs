namespace UserDataModel
{
    public interface IPartitionedDataModel
    {
        public string Id { get; }

        /// <summary>
        /// Our CosmosDB containers use UserId for partitioning.
        /// </summary>
        public Guid UserId { get; set;}

        public string Type { get; }
    }
}
