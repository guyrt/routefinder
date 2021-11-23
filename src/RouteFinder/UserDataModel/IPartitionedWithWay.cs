namespace UserDataModel
{
    public interface IPartitionedWithWay : IUserIdPartitionedDataModel
    {
        public string WayId { get; set; }
    }
}
