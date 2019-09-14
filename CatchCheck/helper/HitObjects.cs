using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;

namespace CatchCheck.helper
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

    public class Generator
    {
        public const float CATCHER_SIZE = 106.75f;
        public const float BASE_WIDTH = 512;
        public const double BASE_SPEED = 1.0 / 512;
        public List<CatchHitObject> GenerateCatchObjects(Beatmap aBeatmap)
        {
            List<HitObject> mapObjects = aBeatmap.hitObjects;
            List<CatchHitObject> objects = new List<CatchHitObject>();
            foreach (Slider mapObject in mapObjects.OfType<Slider>())
            {
                List<CatchHitObject> objectExtras = new List<CatchHitObject>();
                string[] origCode = mapObject.code.Split(',');
                foreach (double ticktimes in mapObject.sliderTickTimes)
                {
                    string[] objectCode = String.Copy(mapObject.code).Split(',');
                    objectCode[0] = Math.Round(mapObject.GetPathPosition(ticktimes).X).ToString();
                    objectCode[2] = ticktimes.ToString();
                    string line = String.Join(",", objectCode);
                    CatchHitObject node = new CatchHitObject(String.Copy(line).Split(','), aBeatmap);
                    objectExtras.Add(node);
                }
                foreach (double ticktimes in GetEdgeTimes(mapObject))
                {
                    string[] objectCode = String.Copy(mapObject.code).Split(',');
                    objectCode[0] = Math.Round(mapObject.GetPathPosition(ticktimes).X).ToString();
                    objectCode[2] = ticktimes.ToString();
                    string line = String.Join(",", objectCode);
                    CatchHitObject node = new CatchHitObject(String.Copy(line).Split(','), aBeatmap);
                    objectExtras.Add(node);
                }

                CatchHitObject sliderObject = new CatchHitObject(origCode, aBeatmap);
                sliderObject.Extras = objectExtras;
                objects.Add(sliderObject);
            }

            foreach (HitObject mapObject in mapObjects)
            {
                if (mapObject is Slider)
                {
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
                if (currentObject.GetObjectType() == "Spinner") { continue; }
                objectWithDroplets.Add(currentObject);
                if (currentObject.Extras == null) { continue; }
                foreach (var sliderNode in currentObject.Extras) {
                    objectWithDroplets.Add(sliderNode);
                }
            }
            objectWithDroplets.Sort((h1, h2) => h1.time.CompareTo(h2.time));
            
            double adjuDiff = (aBeatmap.difficultySettings.circleSize - 5.0) / 5.0;
            float catcherWidth = (float)(64 * (1.0 - 0.7 * adjuDiff)) / 128f;
            float num2 = 305f * catcherWidth * 0.7f;
            double halfCatcherWidth = num2 / 2;
            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

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

        public static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}