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
    public class CheckHyperDash : BeatmapCheck
    {

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Catch
            },

            Category = "Compose",
            Message = "Hyperdashes.",
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
                { "Problem",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Hyperdash to {1}",
                        "timestamp - ", "timestamp - ")
                    .WithCause(
                        "soon") },
                { "Problem Node",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Hyperdash on {1}.",
                        "timestamp - ", "timestamp - ")
                    .WithCause(
                        "soon") },
                { "Problem Time",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Hyperdash duration ({1}ms, expected {2}ms).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var FruitsObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = FruitsObjectManager.GenerateCatchObjects(aBeatmap);
            FruitsObjectManager.initialiseHypers(catchObjects, aBeatmap);

            for (var i = 0; i < catchObjects.Count; i++)
            {
                CatchHitObject currentObject = catchObjects[i];

                // Skip object that doesn't use HDash
                if (!currentObject.HyperDash) continue;

                // No HDash on Cup and Platter
                yield return new Issue(
                    GetTemplate("Problem"),
                    aBeatmap,
                    Timestamp.Get(currentObject),
                    Timestamp.Get(currentObject.HyperDashTarget)
                ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);

                double delta = currentObject.HyperDashTarget.time - currentObject.time;

                if (delta < 62)
                {
                    // 62ms length minimum for Rain
                    yield return new Issue(
                        GetTemplate("Problem Time"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        delta,
                        62
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                }

                if (delta < 125)
                {
                    // 125ms length minimum for Platter
                    yield return new Issue(
                        GetTemplate("Problem Time"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        delta,
                        125
                    ).ForDifficulties(Beatmap.Difficulty.Hard);
                }

                if (currentObject.Extras == null) continue;
                for (var j = 0; j < currentObject.Extras.Count; j++)
                {
                    CatchHitObject currentNode = currentObject.Extras[j];
                    if (!currentNode.HyperDash) continue;
                    yield return new Issue(
                        GetTemplate("Problem Node"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        Timestamp.Get(currentNode.time)
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                }
            }
        }
    }
}