namespace SSDTDevPack.Clippy.Operations
{
    abstract class ReWriterOperation : ClippyOperationBuilder
    {
        protected void PerformAction(ClippyOperation operation, GlyphDefinition glyph)
        {
            if (operation is ClippyReplacementOperations)
            {
                (operation as ClippyReplacementOperations).DoOperation(glyph);
                return;
            }

            if (operation is ClippyReplacementOperation)
            {
                (operation as ClippyReplacementOperation).DoOperation(glyph);
                return;
            }
        }
    }
}