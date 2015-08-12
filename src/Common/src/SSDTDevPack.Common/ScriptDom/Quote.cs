namespace SSDTDevPack.Merge.MergeDescriptor
{
    public static class Quote
    {
        public static string Name(string source)
        {
            if (source.Length < 1)
                return source;

            if (source[0] == '[')
                return source;

            return string.Format("[{0}]", source);
        }
    }
}