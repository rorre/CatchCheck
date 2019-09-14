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
using CatchCheck.helper;
using MapsetVerifierFramework.objects.attributes;

namespace CatchCheck
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
                { "Problem Time",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Hyperdash duration ({1}ms, expected {2}ms).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") },
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
            var gen = new Generator();
            
            List<CatchHitObject> catchObjects = gen.GenerateCatchObjects(aBeatmap);
            gen.initialiseHypers(catchObjects, aBeatmap);

            for (var i = 0; i < catchObjects.Count; i++) {
                CatchHitObject currentObject = catchObjects[i];
            
                // Skip object that doesn't use HDash
                if (!currentObject.HyperDash) { continue; }

                // No HDash on Cup and Platter
                yield return new Issue(
                    GetTemplate("Problem"),
                    aBeatmap,
                    Timestamp.Get(currentObject),
                    Timestamp.Get(currentObject.HyperDashTarget)
                ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                
                double delta = currentObject.HyperDashTarget.time - currentObject.time;

                if (delta < 62) {
                    // 62ms length minimum for Rain
                    yield return new Issue(
                        GetTemplate("Problem Time"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        delta,
                        62
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                } else if (delta < 125) {
                    // 125ms length minimum for Platter
                    yield return new Issue(
                        GetTemplate("Problem Time"),
                        aBeatmap,
                        Timestamp.Get(currentObject),
                        delta,
                        125
                    ).ForDifficulties(Beatmap.Difficulty.Hard);
                }

                var count = 1;
                var nextObject = currentObject.HyperDashTarget;
                
                while (nextObject.HyperDash) {
                    CatchHitObject curNextObject = nextObject;
                    count++;
                    nextObject = curNextObject.HyperDashTarget;
                    curNextObject.HyperDashTarget = null;
                }
                
                if (count >= 4) {
                    // 4 and more consecutive HDashes isn't allowed in Rain
                    yield return new Issue(
                        GetTemplate("Problem Dashes"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                }

                if (count >= 2) {
                    // 2 and more consecutive HDashes isn't allowed in Platter
                    yield return new Issue(
                        GetTemplate("Problem Dashes"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Hard);
                }
            }
        }
    }
    
    [Check]
    public class CheckSpinnerGap : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {   
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Catch
            },

            Category = "Compose",
            Message = "Spinner gap between objects.",
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
                { "Problem Previous",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Spinner gap between previous object is too short ({1}, expected {2}).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") },
                { "Problem Next",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Spinner gap between next object is too short ({1}, expected {2}).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            HitObject hitObject = default(HitObject);
            Beatmap.Difficulty mapDifficulty = aBeatmap.GetDifficulty(true);
            float requirePreviousTime = 0.0f;
            float requireNextTime = 0.0f;

            switch (mapDifficulty)
            {
                case Beatmap.Difficulty.Easy:
                        requirePreviousTime = requireNextTime = 250;
                        break;
                case Beatmap.Difficulty.Normal:
                        requirePreviousTime = requireNextTime = 250;
                        break;
                case Beatmap.Difficulty.Hard:
                        requirePreviousTime = 125;
                        requireNextTime = 250;
                        break;
                case Beatmap.Difficulty.Insane:
                        requirePreviousTime = requireNextTime = 125;
                        break;
                default:
                        requirePreviousTime = 62;
                        requireNextTime = 125;
                        break;
            }

            for (var i = 0; i < aBeatmap.hitObjects.Count; i++) {
                var nextTime = 0.0;
                var previousTime = 0.0;
                hitObject = aBeatmap.hitObjects[i];

                if (hitObject is Spinner) {
                    if (i-1 >= 0) {
                        var previousObject = aBeatmap.hitObjects[i-1];
                        previousTime = hitObject.time - previousObject.time;
                    }

                    if (i+1 < aBeatmap.hitObjects.Count) {
                        var nextObject = aBeatmap.hitObjects[i+1];
                        nextTime = nextObject.time - hitObject.GetEndTime();    
                    }

                    if (previousTime < requirePreviousTime && previousTime != 0.0) {
                        yield return new Issue(GetTemplate("Problem Previous"), aBeatmap, Timestamp.Get(hitObject),
                        $"{(int)previousTime}", $"{(int)requirePreviousTime}");
                    }

                    if (nextTime < requireNextTime && nextTime != 0.0) {
                        yield return new Issue(GetTemplate("Problem Next"), aBeatmap, Timestamp.Get(hitObject),
                        $"{(int)nextTime}", $"{(int)requireNextTime}");
                    }
                }
            }
        }
    }

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
                        "{0} Edge dash ({1})",
                        "timestamp - ", "distance")
                    .WithCause(
                        "soon") },
				{ "Problem",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Edge dash ({1})",
                        "timestamp - ", "distance")
                    .WithCause(
                        "soon") },
                { "Problem",
                    new IssueTemplate(Issue.Level.Minor,
                        "{0} Edge dash ({1})",
                        "timestamp - ", "distance")
                    .WithCause(
                        "soon") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var gen = new Generator();
            
            List<CatchHitObject> catchObjects = gen.GenerateCatchObjects(aBeatmap);
            gen.initialiseHypers(catchObjects, aBeatmap);

            for (var i = 0; i < catchObjects.Count; i++) {
                CatchHitObject currentObject = catchObjects[i];

                var distanceToHDash = currentObject.DistanceToHyperDash;
                if (distanceToHDash == 0) { continue; }
                if (distanceToHDash < 10) {
                    yield return new Issue(
                        GetTemplate("Problem"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        distanceToHDash
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);

                    yield return new Issue(
                        GetTemplate("Warning"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        distanceToHDash
                    ).ForDifficulties(Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                } else if (distanceToHDash < 25) {
                    yield return new Issue(
                        GetTemplate("Minor"),
                        aBeatmap,
                        Timestamp.Get(currentObject.time),
                        distanceToHDash
                    );
                }
            }
        }
    }
}