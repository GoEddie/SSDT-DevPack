namespace SSDTDevPack.Merge.MergeDescriptor
{
    public class MergeOptions
    {


        public MergeOptions(bool hasUpdate, bool hasInsert, bool hasDelete, bool hasSearchKeys)
        {
            HasUpdate = hasUpdate;
            HasInsert = hasInsert;
            HasDelete = hasDelete;
            
            HasSearchKeys = hasSearchKeys;
        }

        public bool HasUpdate { get; set; }
        public bool HasInsert { get; set; }
        public bool HasDelete { get; set; }

        public bool HasSearchKeys { get; set; }
    }
}