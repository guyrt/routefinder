﻿namespace UserDataModel
{
    public interface IPartitionedDataModel
    {
        /// <summary>
        /// Our CosmosDB containers use UserId for partitioning.
        /// </summary>
        public Guid UserId { get; set;}

        public string Type { get; }
    }
}
