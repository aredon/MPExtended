#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    public partial class MPTVSeries : Database, ITVShowLibrary
    {
        public IEnumerable<WebSearchResult> Search(string text)
        {
            IEnumerable<WebSearchResult> results;
            SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");

            // simple name matching
            string showSql = "SELECT ID, Pretty_Name FROM online_series WHERE Pretty_Name LIKE @search";
            IEnumerable<WebSearchResult> shows = ReadList<WebSearchResult>(showSql, delegate(SQLiteDataReader reader)
            {
                string title = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVShow,
                    Id = reader.ReadIntAsString(0),
                    Title = title,
                    Score = 40 + (int)Math.Round((decimal)text.Length / title.Length * 40)
                };
            }, param);
            results = shows;

            string episodeSql = "SELECT EpisodeID, EpisodeName FROM online_episodes WHERE EpisodeName LIKE @search";
            IEnumerable<WebSearchResult> episodes = ReadList<WebSearchResult>(episodeSql, delegate(SQLiteDataReader reader)
            {
                string title = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVEpisode,
                    Id = reader.ReadIntAsString(0),
                    Title = title,
                    Score = 40 + (int)Math.Round((decimal)text.Length / title.Length * 40)
                };
            }, param);
            results = results.Concat(episodes);

            // actor search in shows
            string actorShowSql = "SELECT ID, Pretty_Name, Actors FROM online_series WHERE Actors LIKE @search";
            IEnumerable<WebSearchResult> actorShows = ReadList<WebSearchResult>(actorShowSql, delegate(SQLiteDataReader reader)
            {
                var valid = reader.ReadPipeList(2).Where(x => x.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0); // contains is case sensitive
                if(valid.Count() == 0)
                    return null;
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVShow,
                    Id = reader.ReadIntAsString(0),
                    Title = reader.ReadString(1),
                    Score = valid.Max(x => 40 + (int)Math.Round((decimal)text.Length / x.Length * 30))
                };
            }, param);
            results = results.Concat(actorShows);

            // actor search in episodes (guest stars)
            string actorSql = "SELECT EpisodeID, EpisodeName, GuestStars FROM online_episodes WHERE GuestStars LIKE @search";
            IEnumerable<WebSearchResult> actors = ReadList<WebSearchResult>(actorSql, delegate(SQLiteDataReader reader)
            {
                var valid = reader.ReadPipeList(2).Where(x => x.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0); // contains is case sensitive
                if(valid.Count() == 0)
                    return null;
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVEpisode,
                    Id = reader.ReadIntAsString(0),
                    Title = reader.ReadString(1),
                    Score = valid.Max(x => 20 + (int)Math.Round((decimal)text.Length / x.Length * 40))
                };
            }, param);
            results = results.Concat(actors);

            // fancy episode search: <showname> <season>x<episode>
            Regex episodeRegex = new Regex(@"^(.*) ([0-9]{1,3})x([0-9]{1,3})$");
            Match episodeResult = episodeRegex.Match(text);
            if (episodeResult.Success)
            {
                string sql =
                    "SELECT e.EpisodeID, e.EpisodeName " +
                    "FROM online_episodes e " +
                    "LEFT JOIN online_series s ON e.SeriesID = s.ID " +
                    "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                    "WHERE e.Hidden = 0 AND s.Pretty_Name = @show AND e.SeasonIndex = @season AND e.EpisodeIndex = @episode " +
                    "GROUP BY e.EpisodeID, e.EpisodeName ";
                results = ReadList<WebSearchResult>(sql, delegate(SQLiteDataReader reader)
                {
                    string title = reader.ReadString(1);
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.TVEpisode,
                        Id = reader.ReadString(0),
                        Title = reader.ReadString(1),
                        Score = 100
                    };
                }, new SQLiteParameter("@show", episodeResult.Groups[1].Value.Trim()),
                   new SQLiteParameter("@season", episodeResult.Groups[2].Value), 
                   new SQLiteParameter("@episode", episodeResult.Groups[3].Value))
                     .Concat(results);
            }

            // fancy season search: <showname> s<season>
            Regex seasonRegex = new Regex(@"^(.*) s([0-9]{1,3})$");
            Match seasonResult = seasonRegex.Match(text);
            if (seasonResult.Success)
            {
                string sql =
                    "SELECT DISTINCT e.ID, s.Pretty_Name, e.SeasonIndex " +
                    "FROM season e " +
                    "LEFT JOIN online_series s ON e.SeriesID = s.ID " +
                    "WHERE s.Pretty_Name = @show AND e.SeasonIndex = @season " +
                    "GROUP BY e.ID, s.Pretty_Name ";
                results = ReadList<WebSearchResult>(sql, delegate(SQLiteDataReader reader)
                {
                    string title = reader.ReadString(1);
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.TVSeason,
                        Id = reader.ReadString(0),
                        Title = reader.ReadString(1) + " (season " + reader.ReadInt32(2) + ")",
                        Score = 100
                    };
                }, new SQLiteParameter("@show", seasonResult.Groups[1].Value.Trim()),
                   new SQLiteParameter("@season", seasonResult.Groups[2].Value))
                     .Concat(results);
            }
            
            // return
            return results;
        }
    }
}