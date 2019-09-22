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
    public class CheckConsecutiveHDash : BeatmapCheck
    {

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Catch
            },

            Category = "Compose",
            Message = "Consecutive hyperdashes.",
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
                { "Problem Dashes",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} {1} Consecutive hyperdashes.",
                        "timestamp - ", "amount")
                    .WithCause(
                        "soon") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var FruitsObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = FruitsObjectManager.GenerateCatchObjects(aBeatmap);
            FruitsObjectManager.initialiseHypers(catchObjects, aBeatmap);
            var count = 0;
            for (var i = 0; i < catchObjects.Count; i++)
            {
                CatchHitObject currentObject = catchObjects[i];

                if (currentObject.HyperDash)
                {
                    count++;
                    continue;
                }

                if (count >= 4)
                {
                    // 4 and more consecutive HDashes isn't allowed in Rain
                    yield return new Issue(
                        GetTemplate("Problem Dashes"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                }

                if (count >= 2)
                {
                    // 2 and more consecutive HDashes isn't allowed in Platter
                    yield return new Issue(
                        GetTemplate("Problem Dashes"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Hard);
                }
                count = 0;
            }
        }
    }
}