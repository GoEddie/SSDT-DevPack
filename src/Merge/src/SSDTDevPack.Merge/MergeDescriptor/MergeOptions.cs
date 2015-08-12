namespace SSDTDevPack.Merge.MergeDescriptor
{
    public class MergeOptions
    {


        public MergeOptions(bool hasUpdate, bool hasInsert, bool hasDelete, bool writeIdentityColumns)
        {
            HasUpdate = hasUpdate;
            HasInsert = hasInsert;
            HasDelete = hasDelete;
            WriteIdentityColumns = writeIdentityColumns;
        }

        public bool HasUpdate { get; set; }
        public bool HasInsert { get; set; }
        public bool HasDelete { get; set; }
        public bool WriteIdentityColumns { get; set; }
    }
}