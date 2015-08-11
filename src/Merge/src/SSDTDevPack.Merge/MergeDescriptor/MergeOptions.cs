namespace SSDTDevPack.Merge.MergeDescriptor
{
    public class MergeOptions
    {
        public MergeOptions(bool hasUpdate, bool hasInsert, bool hasDelete)
        {
            HasUpdate = hasUpdate;
            HasInsert = hasInsert;
            HasDelete = hasDelete;
        }

        public bool HasUpdate { get; set; }
        public bool HasInsert { get; set; }
        public bool HasDelete { get; set; }
    }
}