using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;

namespace CatchCheck
{
    public class CatchHitObject : HitObject
    {

        public CatchHitObject(string[] anArgs, Beatmap aBeatmap) : base (anArgs, aBeatmap)
        {
            x = Position.X;
        }

        public float x;
        public float DistanceToHyperDash { get; set; }
        public bool HyperDash => HyperDashTarget != null;
        public CatchHitObject HyperDashTarget;
        public List<CatchHitObject> Extras { get; set; }
  
    }

    public class ObjectManager
    {
        public List<CatchHitObject> GenerateCatchObjects(Beatmap aBeatmap)
        {
            List<HitObject> mapObjects = aBeatmap.hitObjects;
            List<CatchHitObject> objects = new List<CatchHitObject>();

            foreach (Slider mapObject in mapObjects.OfType<Slider>())
            {
                List<CatchHitObject> objectExtras = new List<CatchHitObject>();
                string[] objectCode = String.Copy(mapObject.code).Split(',');

                // Slider ticks
                foreach (double ticktimes in mapObject.sliderTickTimes)
                {
                    objectCode[0] = Math.Round(mapObject.GetPathPosition(ticktimes).X).ToString();
                    objectCode[2] = ticktimes.ToString();
                    string line = String.Join(",", objectCode);
                    CatchHitObject node = new CatchHitObject(String.Copy(line).Split(','), aBeatmap);
                    objectExtras.Add(node);
                }

                foreach (double ticktimes in GetEdgeTimes(mapObject))
                {
                    // Slider repeats and tail
                    objectCode[0] = Math.Round(mapObject.GetPathPosition(ticktimes).X).ToString();
                    objectCode[2] = ticktimes.ToString();
                    string line = String.Join(",", objectCode);
                    CatchHitObject node = new CatchHitObject(String.Copy(line).Split(','), aBeatmap);
                    objectExtras.Add(node);
                }

                CatchHitObject sliderObject = new CatchHitObject(mapObject.code.Split(','), aBeatmap);
                sliderObject.Extras = objectExtras;
                objects.Add(sliderObject);
            }

            foreach (HitObject mapObject in mapObjects)
            {
                if (mapObject is Slider)
                {
                    // Skip slider object because we have added it before
                    continue;
                }
                CatchHitObject hitObject = new CatchHitObject(mapObject.code.Split(','), aBeatmap);
                objects.Add(hitObject);
            }

            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));
            return objects;
        }

        private IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (int i = 0; i < sObject.edgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        public void initialiseHypers(List<CatchHitObject> mapObjects, Beatmap aBeatmap)
        {
            List<CatchHitObject> objectWithDroplets = new List<CatchHitObject>();

            foreach (var currentObject in mapObjects)
            {
                // Skip spinner because it's just random bananas and no actual Hit Object
                if (currentObject.GetObjectType() == "Spinner") { continue; }
                objectWithDroplets.Add(currentObject);

                // If object isnt Slider, just skip it
                if (currentObject.Extras == null) { continue; }

                foreach (var sliderNode in currentObject.Extras) {
                    objectWithDroplets.Add(sliderNode);
                }
            }
            objectWithDroplets.Sort((h1, h2) => h1.time.CompareTo(h2.time));
            
            // Taken from Modding Assistant as osu-lazer seems broken
            // https://github.com/rorre/decompiled-MA/blob/master/Modding%20assistant/osu/DiffCalc/BeatmapDifficultyCalculatorFruits.cs
            double adjustDiff = (aBeatmap.difficultySettings.circleSize - 5.0) / 5.0;
            float catcherWidth = (float)(64 * (1.0 - 0.7 * adjustDiff)) / 128f;
            float num2 = 305f * catcherWidth * 0.7f;
            double halfCatcherWidth = num2 / 2;
            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Catch/Beatmaps/CatchBeatmapProcessor.cs#L190
            // With modifications taken from Modding Assistant
            for (int i = 0; i < objectWithDroplets.Count - 1; i++)
            {
                CatchHitObject currentObject = objectWithDroplets[i];
                CatchHitObject nextObject = objectWithDroplets[i + 1];

                int thisDirection = nextObject.x > currentObject.x ? 1 : -1;
                double timeToNext = nextObject.time - currentObject.time - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = Math.Abs(nextObject.x - currentObject.x) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }

        }

        // https://github.com/ppy/osuTK/blob/master/src/osuTK/Math/MathHelper.cs#L303
        public static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}