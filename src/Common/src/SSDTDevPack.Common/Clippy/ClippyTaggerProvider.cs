using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SSDTDevPack.Clippy
{
    /// <summary>
    ///     Export a <see cref="ITaggerProvider" />
    /// </summary>
    [Export(typeof (ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof (ClippyTag))]
    internal class ClippyTaggerProvider : ITaggerProvider
    {
        [Import] internal IClassifierAggregatorService AggregatorFactory;

        /// <summary>
        ///     Creates an instance of our custom ClippyTagger for a given buffer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The buffer we are creating the tagger for.</param>
        /// <returns>An instance of our custom ClippyTagger.</returns>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            return new ClippyTagger(AggregatorFactory.GetClassifier(buffer)) as ITagger<T>;
        }
    }
}