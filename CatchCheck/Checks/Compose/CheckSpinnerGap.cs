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
                        "{0} Spinner gap between previous object is too short ({1}ms, expected {2}ms).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") },
                { "Problem Next",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Spinner gap between next object is too short ({1}ms, expected {2}ms).",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") },
                { "Minor Previous",
                    new IssueTemplate(Issue.Level.Minor,
                        "{0} Spinner gap between previous object is too short ({1}ms, expected {2}ms) -- but it's a spinner.",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") },
                { "Minor Next",
                    new IssueTemplate(Issue.Level.Minor,
                        "{0} Spinner gap between next object is too short ({1}ms, expected {2}ms) -- but it's a spinner.",
                        "timestamp - ", "duration", "duration")
                    .WithCause(
                        "soon") }
            };
        }

        private string GetKind(bool aBool)
        {
            return aBool ? "Minor" : "Problem";
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            HitObject hitObject = default(HitObject);
            float requirePreviousTime = 0.0f;
            float requireNextTime = 0.0f;

            for (var i = 0; i < aBeatmap.hitObjects.Count; i++)
            {
                var nextTime = 0.0;
                var previousTime = 0.0;
                hitObject = aBeatmap.hitObjects[i];

                if (hitObject is Spinner)
                {
                    bool isPreviousSpinner = false;
                    bool isNextSpinner = false;

                    if (i - 1 >= 0)
                    {
                        var previousObject = aBeatmap.hitObjects[i - 1];
                        previousTime = hitObject.time - previousObject.time;
                        isPreviousSpinner = previousObject is Spinner;
                    }

                    if (i + 1 < aBeatmap.hitObjects.Count)
                    {
                        var nextObject = aBeatmap.hitObjects[i + 1];
                        nextTime = nextObject.time - hitObject.GetEndTime();
                        isNextSpinner = nextObject is Spinner;
                    }

                    if (previousTime < 62 && previousTime != 0.0)
                    {
                        if (isPreviousSpinner)
                            yield return new Issue(
                                GetTemplate(GetKind(isPreviousSpinner) + " Previous"),
                                aBeatmap,
                                Timestamp.Get(hitObject),
                                $"{(int)previousTime}",
                                $"{(int)requirePreviousTime}"
                            ).ForDifficulties(
                                Beatmap.Difficulty.Expert,
                                Beatmap.Difficulty.Ultra
                            );
                    }

                    if (previousTime < 125 && previousTime != 0.0)
                    {
                        yield return new Issue(
                            GetTemplate(GetKind(isPreviousSpinner) + " Previous"),
                            aBeatmap,
                            Timestamp.Get(hitObject),
                            $"{(int)previousTime}",
                            $"{(int)requirePreviousTime}"
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy,
                            Beatmap.Difficulty.Normal,
                            Beatmap.Difficulty.Hard,
                            Beatmap.Difficulty.Insane
                        );
                    }

                    if (nextTime < 125 && nextTime != 0.0)
                    {
                        yield return new Issue(
                            GetTemplate(GetKind(isNextSpinner) + " Next"),
                            aBeatmap,
                            Timestamp.Get(hitObject),
                            $"{(int)nextTime}",
                            $"{(int)requireNextTime}"
                        ).ForDifficulties(
                            Beatmap.Difficulty.Insane,
                            Beatmap.Difficulty.Expert,
                            Beatmap.Difficulty.Ultra
                        );
                    }

                    if (nextTime < 250 && nextTime != 0.0)
                    {
                        yield return new Issue(
                            GetTemplate(GetKind(isNextSpinner) + " Next"),
                            aBeatmap,
                            Timestamp.Get(hitObject),
                            $"{(int)nextTime}",
                            $"{(int)requireNextTime}"
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy,
                            Beatmap.Difficulty.Normal,
                            Beatmap.Difficulty.Hard
                        );
                    }
                }
            }
        }
    }
}