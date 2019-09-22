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
            Message = "Edge dashes.",
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

        public IEnumerable<Issue> GenerateIssue(Beatmap aBeatmap, CatchHitObject currentObject, CatchHitObject nextObject, double distanceToHDash, bool isFirstNode, bool isSecondNode)
        {
            string objectTimestamp;
            string nodeTimestamp;
            if (isFirstNode) objectTimestamp = Timestamp.Get(currentObject.time);
            else objectTimestamp = Timestamp.Get(currentObject);
            if (isSecondNode) nodeTimestamp = Timestamp.Get(nextObject.time);
            else nodeTimestamp = Timestamp.Get(nextObject);

            if (distanceToHDash < 10)
            {
                yield return new Issue(
                    GetTemplate("Problem"),
                    aBeatmap,
                    objectTimestamp,
                    distanceToHDash,
                    nodeTimestamp
                ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);

                yield return new Issue(
                    GetTemplate("Warning"),
                    aBeatmap,
                    objectTimestamp,
                    distanceToHDash,
                    nodeTimestamp
                ).ForDifficulties(Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
            }
            else if (distanceToHDash < 20)
            {
                yield return new Issue(
                    GetTemplate("Minor"),
                    aBeatmap,
                    objectTimestamp,
                    distanceToHDash,
                    nodeTimestamp
                );
            }
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var FruitsObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = FruitsObjectManager.GenerateCatchObjects(aBeatmap);
            FruitsObjectManager.initialiseHypers(catchObjects, aBeatmap);

            for (var i = 0; i < catchObjects.Count; i++)
            {
                CatchHitObject currentObject = catchObjects[i];
                CatchHitObject nextObject;

                var distanceToHDash = Math.Ceiling(currentObject.PixelsToHyperDash);
                if (distanceToHDash == 0) continue;

                // If object is not a slider, just pick next object
                if (currentObject.Extras == null) nextObject = catchObjects[i + 1];
                // else pick first node (either slider tick or tail)
                else nextObject = currentObject.Extras[0];

                foreach (Issue aIssue in GenerateIssue(aBeatmap, currentObject, nextObject, distanceToHDash, false, false))
                {
                    yield return aIssue;
                }

                if (currentObject.Extras == null) continue;
                for (var j = 0; j < currentObject.Extras.Count; j++)
                {
                    CatchHitObject currentNode = currentObject.Extras[j];

                    distanceToHDash = Math.Ceiling(currentNode.PixelsToHyperDash);
                    if (distanceToHDash == 0) continue;
                    CatchHitObject nextNode;
                    bool isNode;

                    if (j == currentObject.Extras.Count - 1)
                    {
                        // If current node is a tail, then select next object
                        nextNode = catchObjects[i + 1];
                        isNode = false;
                    }
                    else
                    {
                        // else select next node.
                        nextNode = currentObject.Extras[j + 1];
                        isNode = true;
                    }
                    foreach (Issue aIssue in GenerateIssue(aBeatmap, currentNode, nextNode, distanceToHDash, true, isNode))
                    {
                        yield return aIssue;
                    }
                }
            }
        }
    }
}
