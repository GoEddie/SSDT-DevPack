using System;

namespace SSDTDevPacl.CodeCoverage.Lib
{
    public class CoveredStatement
    {
        public long Offset;
        public long Length;
        public int ObjectId;
        public string Object;
        public string ObjectType;
        public DateTimeOffset TimeStamp;
    }
}