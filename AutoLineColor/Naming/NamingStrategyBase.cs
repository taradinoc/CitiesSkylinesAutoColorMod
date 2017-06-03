﻿using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoLineColor.Naming
{
    struct LineAnalysis
    {
        public int StopCount;
        public List<string> Districts;
        public bool HasNonDistrictStop;

        /// <summary>
        /// May be null if pathfinding wasn't successful at analysis time.
        /// </summary>
        public Dictionary<string, float> DistanceOnSegments;
    }

    abstract class NamingStrategyBase : INamingStrategy
    {
        public string GetName(TransportLine transportLine)
        {
            try
            {
                string result = null;

                switch (transportLine.Info.m_transportType)
                {
                    case TransportInfo.TransportType.Bus:
                        result = GetBusLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Metro:
                        result = GetMetroLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Train:
                        result = GetTrainLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Ship:
                        result = GetShipLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Tram:
                        result = GetTramLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Monorail:
                        result = GetMonorailLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.CableCar:
                        result = GetCableCarLineName(transportLine);
                        break;

                    case TransportInfo.TransportType.Airplane:
                        result = GetBlimpLineName(transportLine);
                        break;
                }

                return result ?? GetGenericLineName(transportLine);
            }
            catch (Exception e)
            {
                Console.Instance.Error(e.ToString());
                return null;
            }
        }

        protected virtual string GetGenericLineName(TransportLine transportLine)
        {
            return null;
        }

        protected virtual string GetBusLineName(TransportLine transportLine)
        {
            return null;
        }

        protected virtual string GetTrainLineName(TransportLine transportLine)
        {
            return null;
        }

        protected virtual string GetMetroLineName(TransportLine transportLine)
        {
            return null;
        }

        protected virtual string GetCableCarLineName(TransportLine transportLine)
        {
            return null;
        }

        protected virtual string GetTramLineName(TransportLine transportLine)
        {
            return GetBusLineName(transportLine);
        }

        protected virtual string GetShipLineName(TransportLine transportLine)
        {
            return GetTrainLineName(transportLine);
        }

        protected virtual string GetMonorailLineName(TransportLine transportLine)
        {
            return GetTrainLineName(transportLine);
        }

        protected virtual string GetBlimpLineName(TransportLine transportLine)
        {
            return GetShipLineName(transportLine);
        }

        protected static LineAnalysis AnalyzeLine(TransportLine transportLine)
        {
            var theNetManager = Singleton<NetManager>.instance;
            var theDistrictManager = Singleton<DistrictManager>.instance;
            var stop = transportLine.m_stops;
            var firstStop = stop;
            var collectSegments = true;

            var result = new LineAnalysis
            {
                Districts = new List<string>(),
                HasNonDistrictStop = false,
                StopCount = 0,
                DistanceOnSegments = new Dictionary<string, float>(),
            };

            var segments = new List<ushort>();

            do
            {
                var stopInfo = theNetManager.m_nodes.m_buffer[stop];

                // record the name of the district containing the stop
                var district = theDistrictManager.GetDistrict(stopInfo.m_position);

                if (district == 0)
                {
                    result.HasNonDistrictStop = true;
                }
                else
                {
                    var districtName = theDistrictManager.GetDistrictName(district).Trim();
                    if (!result.Districts.Contains(districtName))
                    {
                        result.Districts.Add(districtName);
                    }
                }

                var nextStop = TransportLine.GetNextStop(stop);

                if (collectSegments)
                {
                    // record the segment names and distances from this stop to the next
                    segments.Clear();
                    if (GetSegmentsBetweenStops(stop, nextStop, segments))
                    {
                        foreach (var segment in segments)
                        {
                            var name = theNetManager.GetSegmentName(segment).Trim();

                            if (string.IsNullOrEmpty(name))
                                continue;

                            var length = theNetManager.m_segments.m_buffer[segment].m_averageLength;

                            float curLength;
                            if (result.DistanceOnSegments.TryGetValue(name, out curLength))
                            {
                                result.DistanceOnSegments[name] = curLength + length;
                            }
                            else
                            {
                                result.DistanceOnSegments[name] = length;
                            }
                        }
                    }
                    else
                    {
                        collectSegments = false;
                    }
                }

                stop = nextStop;
                result.StopCount++;
            } while (result.StopCount < Constants.MaxLineAnalysisStops && stop != firstStop);

            if (!collectSegments)
                result.DistanceOnSegments = null;

            return result;
        }

        private static bool GetSegmentsBetweenStops(ushort stop1, ushort stop2, List<ushort> segments)
        {
            var theTransportManager = Singleton<TransportManager>.instance;
            var theNetManager = Singleton<NetManager>.instance;

            theTransportManager.UpdateLinesNow();

            while (stop1 != stop2)
            {
                // these are the transport-network segments that make up the logical line
                var transportSegment = TransportLine.GetNextSegment(stop1);

                if (transportSegment == 0)
                    return false;

                var path = theNetManager.m_segments.m_buffer[transportSegment].m_path;

                if (path == 0)
                    return false;

                // these are the road/track segments that make up the physical line
                segments.AddRange(GetPathPositions(path).Select(pp => pp.m_segment));

                stop1 = TransportLine.GetNextStop(stop1);
            }

            return true;
        }

        private static IEnumerable<PathUnit.Position> GetPathPositions(uint pathUnit)
        {
            var thePathManager = Singleton<PathManager>.instance;

            var unit = thePathManager.m_pathUnits.m_buffer[pathUnit];
            PathUnit.Position position;

            if (!unit.GetPosition(0, out position))
                yield break;

            int posIndex = 0;
            bool invalid;

            do
            {
                yield return position;
            } while (PathUnit.GetNextPosition(ref pathUnit, ref posIndex, out position, out invalid));
        }

        protected static List<string> GetExistingNames()
        {
            var names = new List<string>();
            var theTransportManager = Singleton<TransportManager>.instance;
            var theInstanceManager = Singleton<InstanceManager>.instance;
            var lines = theTransportManager.m_lines.m_buffer;
            for (ushort lineIndex = 0; lineIndex < lines.Length - 1; lineIndex++)
            {
                if (lines[lineIndex].HasCustomName())
                {
                    string name = theTransportManager.GetLineName(lineIndex);
                    if (!String.IsNullOrEmpty(name))
                    {
                        names.Add(name);
                    }
                }
            }
            return names;
        }

        protected static string GetInitials(string words)
        {
            string initials = words[0].ToString();
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i] == ' ')
                {
                    initials += words[i + 1];
                }
            }
            return initials;
        }

        protected static string FirstWord(string words)
        {
            var pos = words.IndexOf(' ');
            return pos >= 0 ? words.Substring(0, pos) : words;
        }

        protected static string LastWord(string words)
        {
            var pos = words.LastIndexOf(' ');
            return pos >= 0 ? words.Substring(pos + 1) : words;
        }

        protected static string AllButLastWord(string words)
        {
            var pos = words.LastIndexOf(' ');
            return pos >= 0 ? words.Substring(0, pos) : words;
        }

        protected static string StripDistrictSuffix(string name)
        {
            return AllButLastWord(name);
        }

        protected static string StripRoadSuffix(string name)
        {
            return AllButLastWord(name);
        }

        protected static string AbbreviateDistrictSuffix(string name)
        {
            // TODO: make localizable

            if (name.IndexOf(' ') < 0)
                return name;

            var suffix = LastWord(name);

            switch (suffix)
            {
                case "District":
                    suffix = "Dst";
                    break;

                case "Heights":
                    suffix = "Hts";
                    break;

                case "Hills":
                    suffix = "Hls";
                    break;

                case "Park":
                    suffix = "Pk";
                    break;

                case "Square":
                    suffix = "Sq";
                    break;

                default:
                    suffix = AutoShortenWord(suffix).Substring(0, 3);
                    break;
            }

            return AllButLastWord(name) + " " + suffix;
        }

        // TODO: make localizable
        /* Loosely based on the US Postal Service street suffix abbreviations:
         * https://pe.usps.com/text/pub28/28apc_002.htm */
        private static readonly Dictionary<string, string> RoadSuffixAbbreviations =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Alley", "Aly" },
            { "Avenue", "Ave" },
            { "Boulevard", "Blvd" },
            { "Bridge", "Br" },
            { "Circle", "Cir" },
            { "Court", "Ct" },
            { "Crossing", "Xing" },
            { "Drive", "Dr" },
            { "Expressway", "Expy" },
            { "Freeway", "Fwy" },
            { "Highway", "Hwy" },
            { "Junction", "Jct" },
            { "Lane", "Ln" },
            { "Parkway", "Pkwy" },
            { "Road", "Rd" },
            { "Station", "Stn" },
            { "Street", "St" },
            { "Track", "Trk" },
            { "Tunnel", "Tunl" },
            { "Way", "Wy" },
        };

        protected static string AbbreviateRoadSuffix(string name)
        {
            if (name.IndexOf(' ') < 0)
                return name;

            var suffix = LastWord(name);

            string abbrev;
            if (!RoadSuffixAbbreviations.TryGetValue(suffix, out abbrev))
                abbrev = AutoShortenWord(suffix).Substring(0, 3);

            return AllButLastWord(name) + " " + abbrev;
        }

        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'a', 'e', 'i', 'o', 'u',
            'A', 'E', 'I', 'O', 'U',
        };

        /// <summary>
        /// Tries to shorten a word by removing unneeded letters.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A new string with length less than or equal to the length of <paramref name="word"/>.</returns>
        /// <remarks>
        /// <para>The first character is always preserved, even if it's a vowel.</para>
        /// <para>It was hard to resist the temptation to call this <c>Abrvt</c>.</para>
        /// </remarks>
        protected static string AutoShortenWord(string word)
        {
            var sb = new StringBuilder(word);

            for (int i = sb.Length - 1; i > 0; i--)
            {
                if (Vowels.Contains(sb[i]) || sb[i] == sb[i - 1] ||
                    (sb[i] == 'c' && i + 1 < sb.Length && sb[i + 1] == 'k'))
                {
                    sb.Remove(i, 1);
                }
            }

            return sb.ToString();
        }
    }
}
