#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MovingPictures
{
    public partial class MPMovingPictures : Database, IMovieLibrary
    {
        public IEnumerable<WebSearchResult> Search(string text)
        {
            IEnumerable<WebSearchResult> results;
            SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");

            // simple title search
            string showSql = "SELECT id, title FROM movie_info WHERE title LIKE @search";
            results = ReadList<WebSearchResult>(showSql, delegate(SQLiteDataReader reader)
            {
                string title = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.Movie,
                    Id = reader.ReadIntAsString(0),
                    Title = title,
                    Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40)
                };
            }, param);

            // actor search in shows
            string actorSql = "SELECT id, title, actors FROM movie_info WHERE actors LIKE @search";
            IEnumerable<WebSearchResult> actors = ReadList<WebSearchResult>(actorSql, delegate(SQLiteDataReader reader)
            {
                var valid = reader.ReadPipeList(2).Where(x => x.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0); // contains is case sensitive
                if (valid.Count() == 0)
                    return null;
                return new WebSearchResult()
                {
                    Type = WebMediaType.Movie,
                    Id = reader.ReadIntAsString(0),
                    Title = reader.ReadString(1),
                    Score = valid.Max(x => 40 + (int)Math.Round((decimal)text.Length / x.Length * 30))
                };
            }, param);
            results = results.Concat(actors);

            return results;
        }
    }
}
