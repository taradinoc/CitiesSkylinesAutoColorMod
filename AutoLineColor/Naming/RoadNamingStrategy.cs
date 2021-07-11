using System;
using System.Linq;

namespace AutoLineColor.Naming
{
    // TODO: Require names to be unique.

    // TODO: More interesting naming patterns.

    /* TODO: Handle non-road networks.
     * 
     * Tram and monorail lines can use roads, so they might work in some cases already,
     * but if they only use dedicated tracks, they'll get generic names. (And if they use
     * a road for a tiny bit of their length, they'll get named after the road, which might
     * be surprising.)
     * 
     * Metro lines tend to follow roads, but they don't actually use the roads. We could
     * try to find road segments that line up with the metro tunnel, or use the roads the
     * stations are on.
     */

    internal class RoadNamingStrategy : NamingStrategyBase
    {
        protected override string GetGenericLineName(in TransportLine transportLine)
        {
            var analysis = AnalyzeLine(transportLine);
            var lineNum = transportLine.m_lineNumber;

            if (analysis.DistanceOnSegments == null)
                return null;

            //var logger = Console.Instance;

            //logger.Message($"Line traverses {analysis.DistanceOnSegments.Count} segment names...");

            IChunk districtBasedChunk = null, roadBasedChunk = null;

            if (analysis.Districts.Count > 0)
            {
                districtBasedChunk = new DecayingListChunk(
                    DecayMode.RespectEndpoints,
                    analysis.Districts.Select((d, i) => new DistrictNameChunk(
                            d,
                            i == analysis.Districts.Count - 1
                                ? AbbreviationMode.AbbreviateSuffix
                                : AbbreviationMode.StripSuffix))
                        .Cast<IChunk>().ToArray());
            }

            if (analysis.DistanceOnSegments.Count > 0)
            {
                //foreach (var pair in analysis.DistanceOnSegments)
                //{
                //    logger.Message($"... '{pair.Key}' (for {pair.Value} units)");
                //}

                // sort all segment names by descending distance traveled, and add up the cumulative distance
                var sorted = analysis.DistanceOnSegments
                    .OrderByDescending(p => p.Value)
                    .Scan(0f, (cd, p) => cd + p.Value,
                        (p, cd) => new { name = p.Key, distance = p.Value, cumulativeDistance = cd })
                    .ToArray();

                // find the minimum set of roads that make up more than half the total distance
                var totalDistance = analysis.DistanceOnSegments.Sum(p => p.Value);

                //logger.Message("totalDistance=" + totalDistance);
                //logger.Message("with cumulative:");
                //foreach (var r in sorted)
                //{
                //    logger.Message($"'{r.name}', d={r.distance}, cd={r.cumulativeDistance}");
                //}

                var majoritySize = 1 + Array.FindIndex(sorted, r => r.cumulativeDistance > totalDistance / 2);
                var majority = sorted.Take(majoritySize);

                roadBasedChunk = new DecayingListChunk(
                    DecayMode.RespectPriority,
                    majority.Select((r, i) => new RoadNameChunk(
                            r.name,
                            i == majoritySize - 1
                                ? AbbreviationMode.AbbreviateSuffix
                                : AbbreviationMode.StripSuffix))
                        .Cast<IChunk>().ToArray());
            }

            var lineNumChunk = new StaticChunk("#" + lineNum + " ");

            if (districtBasedChunk != null)
            {
                if (roadBasedChunk != null)
                {
                    return new ConcatChunk(
                            lineNumChunk,
                            districtBasedChunk,
                            StaticChunk.Via,
                            roadBasedChunk)
                        .VaryAndShortenToFit();
                }

                return new ConcatChunk(
                        lineNumChunk,
                        districtBasedChunk)
                    .VaryAndShortenToFit();
            }

            if (roadBasedChunk != null)
            {
                return new ConcatChunk(
                        lineNumChunk,
                        roadBasedChunk,
                        OptionalCosmeticChunk.Line)
                    .VaryAndShortenToFit();
            }

            return null;
        }
    }
}
