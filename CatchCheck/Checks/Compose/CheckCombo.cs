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
    public class CheckCombo : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Catch
            },

            Category = "Compose",
            Message = "Combo count.",
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
                        "{0} Combo should not exceed {1}",
                        "timestamp - ", "maximum combo"
                    )
                    .WithCause(
                        "soon") }
            };
        }

        public enum Type
        {
            Circle = 1,
            Slider = 2,
            NewCombo = 4,
            Spinner = 8,
            ComboSkip1 = 16,
            ComboSkip2 = 32,
            ComboSkip3 = 64,
            ManiaHoldNote = 128
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            int count = 1;
            CatchHitObject startObject;
            CatchHitObject currentObject;
            var FruitsObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = FruitsObjectManager.GenerateCatchObjects(aBeatmap);
            startObject = catchObjects[0];
            for (var i = 1; i < catchObjects.Count; i++)
            {
                currentObject = catchObjects[i];

                // Parse hitobject types as we can't check flags
                string[] objectCodeArgs = currentObject.code.Split(',');
                Type objectTypes = (Type)int.Parse(objectCodeArgs[3]);

                if (objectTypes.HasFlag(Type.NewCombo) || i == catchObjects.Count - 1)
                {
                    if (count > 8)
                    {
                        yield return new Issue(
                            GetTemplate("Warning"),
                            aBeatmap,
                            Timestamp.Get(startObject),
                            8
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy
                        );
                    }
                    
                    if (count > 10)
                    {
                        yield return new Issue(
                            GetTemplate("Warning"),
                            aBeatmap,
                            Timestamp.Get(startObject),
                            10
                        ).ForDifficulties(
                            Beatmap.Difficulty.Normal
                        );
                    }
                    
                    if (count > 12)
                    {
                        yield return new Issue(
                            GetTemplate("Warning"),
                            aBeatmap,
                            Timestamp.Get(startObject),
                            12
                        ).ForDifficulties(
                            Beatmap.Difficulty.Hard
                        );
                    }
                    
                    if (count > 16)
                    {
                        yield return new Issue(
                            GetTemplate("Warning"),
                            aBeatmap,
                            Timestamp.Get(startObject),
                            8
                        ).ForDifficulties(
                            Beatmap.Difficulty.Insane,
                            Beatmap.Difficulty.Expert,
                            Beatmap.Difficulty.Ultra
                        );
                    }
                    startObject = currentObject;
                    count = 1;
                }
                else
                {
                    count++;
                    if (currentObject.Extras == null) continue;
                    count += currentObject.Extras.Count;
                }
            }
        }
    }
}
