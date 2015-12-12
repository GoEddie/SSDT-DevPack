using NUnit.Framework;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Common.UnitTests
{
    [TestFixture]
    public class DeleteChinkerReWriter
    {
        [Test]
        public void Rewrites_Insert_Into_Batched_Insert()
        {
            var script = @" delete from tablea;";
	   
            var rewriter = new ChunkDeletesRewriter(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetDeleteStatements(script));
            Assert.AreEqual(1, replacements.Count);
            
        }
    }
}