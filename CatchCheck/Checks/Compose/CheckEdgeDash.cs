using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetParser.objects.timinglines;
using MapsetParser.statics;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using MapsetVerifierFramework.objects.attributes;

namespace CatchCheck.Check.Compose
{
    [Check]
    public class CheckEdgeDash : BeatmapCheck
    {
        
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {   
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Catch
            },

            Category = "Compose",
            Message = "Edge Dashes.",
            Author = "-Keitaro",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    soon."
                },
                {
                    "Reasoning",
                    @"
                    tm."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Warning",
                    new IssueTemplate(Issue.Level.Warning,
                        "{0} Edge dash ({1}px to hyperdash to {2})",
                        "timestamp - ", "distance", "timestamp - ")
                    .WithCause(
                        "soon") },
				{ "Problem",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Edge dash ({1}px to hyperdash to {2})",
                        "timestamp - ", "distance", "timestamp - ")
                    .WithCause(
                        "soon") },
                { "Minor",
                    new IssueTemplate(Issue.Level.Minor,
                        "{0} Edge dash ({1}px to hyperdash to {2})",
                        "timestamp - ", "distance", "timestamp - ")
                    .WithCause(
                        "soon") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var FruitsObjectManager = new ObjectManager();
            
            List<CatchHitObject> catchObjects = FruitsObjectManager.GenerateCatchObjects(aBeatmap);
            FruitsObjectManager.initialiseHypers(catchObjects, aBeatmap);

            for (var i = 0; i < catchObjects.Count; i++) {
                CatchHitObject currentObject = catchObjects[i];

                var distanceToHDash = Math.Round(currentObject.PixelsToHyperDash);
                if (distanceToHDash == 0) { continue; }
                if (distanceToHDash < 25) {
                    yield return new Issue(
                        GetTemplate("Problem"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        distanceToHDash,
                        Timestamp.Get(catchObjects[i+1])
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);

                    yield return new Issue(
                        GetTemplate("Warning"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        distanceToHDash,
                        Timestamp.Get(catchObjects[i+1])
                    ).ForDifficulties(Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                } else if (distanceToHDash < 50) {
                    yield return new Issue(
                        GetTemplate("Minor"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        distanceToHDash,
                        Timestamp.Get(catchObjects[i+1])
                    );
                }
            }
        }
    }
}
