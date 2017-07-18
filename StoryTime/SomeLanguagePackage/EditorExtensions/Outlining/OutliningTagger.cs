using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using System.IO;
using SOME.SomeLanguageCore;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageService.EditorExtensions.Outlining
{
    internal sealed class OutliningTagger : ITagger<IOutliningRegionTag>
    {
        string startHide = "{";     //the characters that start the outlining region]
        string endHide = "}";       //the characters that end the outlining region
        string ellipsis = "...";    //the characters that are displayed when the region is collapsed
        ITextBuffer buffer;
        ITextSnapshot snapshot;
        List<Region> regions;

        public OutliningTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();
            this.ReParse();
            this.buffer.Changed += BufferChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = this.snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;
            foreach (var region in currentRegions)
            {
                if (region.StartLine <= endLineNumber &&
                    region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    SnapshotSpan span = new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);

                    string content = span.GetText();
                    StringBuilder sb = new StringBuilder();
                    using (StringReader reader = new StringReader(content))
                    {
                        string line = reader.ReadLine();
                        sb.AppendLine(line);
                        //shift each line to left by "StartOffset"
                        while ((line = reader.ReadLine()) != null)
                        {
                            for (int i = 0; i < region.StartOffset; i++)
                            {
                                if (line.Equals(string.Empty))
                                {
                                    break;
                                }

                                if (line[0] != '\t' && line[0] != ' ')
                                {
                                    break;
                                }
                                line = line.Substring(1);
                            }
                            sb.AppendLine(line);
                        }
                    }

                    //the region starts at the beginning of the "{", and goes until the *end* of the line that contains the "}".
                    yield return new TagSpan<IOutliningRegionTag>(span, new OutliningRegionTag(false, false, ellipsis, sb.ToString()));
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            if (e.After != buffer.CurrentSnapshot)
                return;

            //best chance to refresh the codeDom
            refreshCodeDom();

            this.ReParse();
        }

        //This update services for all other editor extensions
        void refreshCodeDom()
        {
            //refresh the CodeDome types
            string fileName = this.buffer.GetFileName();
            string codeName = Path.GetFileNameWithoutExtension(fileName);
            SomeCompiler.RemoveByNameSpace(codeName);

            string text = this.buffer.CurrentSnapshot.GetText();

            SomeCompiler.ParseTypes(text, codeName);
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            //keep the current (deepest) partial region, which will have
            // references to any parent partial regions.
            PartialRegion currentRegion = null;

            if (this.snapshot.ContentType.DisplayName == SomeContentTypeDefinition.ContentType1)
            {
                AnalyseRegionsSmr(newSnapshot, newRegions, ref currentRegion);
            }
            else
            {
                AnalyseRegionsSms(newSnapshot, newRegions, ref currentRegion);
            }

            //determine the changed span, and send a changed event with the new spans
            List<Span> oldSpans =
                new List<Span>(this.regions.Select(r => AsSnapshotSpan(r, this.snapshot)
                    .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
                    .Span));

            List<Span> newSpans = new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

            NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
            NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

            //the changed regions are regions that appear in one set or the other, but not both.
            NormalizedSpanCollection removed = NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

            int changeStart = int.MaxValue;
            int changeEnd = -1;

            if (removed.Count > 0)
            {
                changeStart = removed[0].Start;
                changeEnd = removed[removed.Count - 1].End;
            }

            if (newSpans.Count > 0)
            {
                changeStart = Math.Min(changeStart, newSpans[0].Start);
                changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
            }

            this.snapshot = newSnapshot;
            this.regions = newRegions;

            if (changeStart <= changeEnd)
            {
                ITextSnapshot snap = this.snapshot;
                if (this.TagsChanged != null)
                    this.TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(this.snapshot, Span.FromBounds(changeStart, changeEnd))));
            }
        }

        //For .smr
        private void AnalyseRegionsSmr(ITextSnapshot newSnapshot, List<Region> newRegions, ref PartialRegion currentRegion)
        {
            int regionStart = -1;
            int regionEnd = -1;

            foreach (var line in newSnapshot.Lines)
            {
                string text = line.GetText();
                if (text.Trim() != string.Empty)
                {
                    if (regionStart == 0)
                    {
                        regionStart = line.LineNumber;
                        int offset = text.Length - text.TrimStart().Length;
                        if (currentRegion != null)
                        {
                            newRegions.Add(new Region()
                            {
                                Level = currentRegion.Level,
                                StartLine = currentRegion.StartLine,
                                StartOffset = currentRegion.StartOffset,
                                EndLine = line.LineNumber
                            });

                            currentRegion = new PartialRegion()
                            {
                                Level = 1,
                                StartLine = line.LineNumber,
                                StartOffset = offset,
                                PartialParent = currentRegion.PartialParent
                            };
                        }
                        //this is a new (sub)region
                        else
                        {
                            currentRegion = new PartialRegion()
                            {
                                Level = 1,
                                StartLine = regionStart,
                                StartOffset = offset,
                                PartialParent = currentRegion
                            };
                        }
                    }
                    else if(regionStart == -1)
                    {
                        SomeTokenList tokenList = new SomeTokenList();

                        //TODOZ: exception handling here
                        SomeCompiler.lex(tokenList, text); 
                        SomeToken token = tokenList.First;
                        if (token.Type == SomeTokenType.Ident)
                        {
                            while (token.Next != null && token.Type == SomeTokenType.Blank)
                            {
                                token = token.Next;
                            }

                            if (token.Type != SomeTokenType.Dot)
                            {
                                regionStart = 0; //start
                            }
                        }
                    }
                }
                //empty line indicates an end of a region
                else if (regionEnd > 0 && text.Trim().Equals(string.Empty))
                {
                    //the regions match
                    if (currentRegion != null)
                    {
                        newRegions.Add(new Region()
                        {
                            Level = 1,
                            StartLine = currentRegion.StartLine,
                            StartOffset = currentRegion.StartOffset,
                            EndLine = regionEnd
                        });

                        currentRegion = currentRegion.PartialParent;
                    }
                    regionStart = -1;
                }

                regionEnd = line.LineNumber;
            }
        }

        //for .sms
        private void AnalyseRegionsSms(ITextSnapshot newSnapshot, List<Region> newRegions, ref PartialRegion currentRegion)
        {
            foreach (var line in newSnapshot.Lines)
            {
                int regionStart = -1;
                string text = line.GetText();

                //lines that contain a "{" denote the start of a new region.
                regionStart = text.IndexOf(startHide, StringComparison.Ordinal);
                int regionEnd = text.IndexOf(endHide, StringComparison.Ordinal);
                if (regionStart != -1 && regionEnd < regionStart) //Eliminate the line with "{" and "}" at same time
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    int newLevel;
                    if (!TryGetLevel(text, regionStart, out newLevel))
                        newLevel = currentLevel + 1;

                    //levels are the same and we have an existing region;
                    //end the current region and start the next
                    if (currentLevel == newLevel && currentRegion != null)
                    {
                        newRegions.Add(new Region()
                        {
                            Level = currentRegion.Level,
                            StartLine = currentRegion.StartLine,
                            StartOffset = currentRegion.StartOffset,
                            EndLine = line.LineNumber
                        });

                        currentRegion = new PartialRegion()
                        {
                            Level = newLevel,
                            StartLine = line.LineNumber,
                            StartOffset = regionStart,
                            PartialParent = currentRegion.PartialParent
                        };
                    }
                    //this is a new (sub)region
                    else
                    {
                        currentRegion = new PartialRegion()
                        {
                            Level = newLevel,
                            StartLine = line.LineNumber,
                            StartOffset = regionStart,
                            PartialParent = currentRegion
                        };
                    }
                }
                //lines that contain "}" denote the end of a region
                regionEnd = text.IndexOf(endHide, StringComparison.Ordinal);
                regionStart = text.IndexOf(startHide, StringComparison.Ordinal);
                if (regionEnd != -1 && regionStart == -1)
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    int closingLevel;
                    if (!TryGetLevel(text, regionEnd, out closingLevel))
                        closingLevel = currentLevel;

                    //the regions match
                    if (currentRegion != null && currentLevel == closingLevel)
                    {
                        newRegions.Add(new Region()
                        {
                            Level = currentLevel,
                            StartLine = currentRegion.StartLine,
                            StartOffset = currentRegion.StartOffset,
                            EndLine = line.LineNumber
                        });

                        currentRegion = currentRegion.PartialParent;
                    }
                }
            }
        }

        static bool TryGetLevel(string text, int startIndex, out int level)
        {
            level = -1;
            if (text.Length > startIndex + 3)
            {
                if (int.TryParse(text.Substring(startIndex + 1), out level))
                    return true;
            }

            return false;
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
            var endLine = (region.StartLine == region.EndLine) ? startLine
                 : snapshot.GetLineFromLineNumber(region.EndLine);
            return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        }

        class PartialRegion
        {
            public int StartLine { get; set; }
            public int StartOffset { get; set; }
            public int Level { get; set; }
            public PartialRegion PartialParent { get; set; }
        }

        class Region : PartialRegion
        {
            public int EndLine { get; set; }
        }
    }


}
