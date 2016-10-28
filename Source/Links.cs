//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Link (URI) utilities.
//
//-----------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace ProjectGolan.Vrobot3
{
   //
   // Utils
   //
   public static partial class Utils
   {
      //
      // URI
      //
      public struct URI
      {
         public String method, host, path, query, tag, uri;

         //
         // ToString
         //
         public override String ToString() => uri;

         //
         // Finder
         //
         public static Regex Finder = new Regex(
            @"((?<method>[^:/?# ]+)?:)" +
            @"(//(?<host>[^/?# ]*))" +
            @"(?<path>[^?# ]*)" +
            @"(?<query>\?([^# ]*))?" +
            @"(?<tag>#(.*))?");

         //
         // FromMatch
         //
         public static URI FromMatch(Match match)
         {
            return new URI{
               method = match.Groups["method"]?.Value ?? String.Empty,
               host   = match.Groups["host"]?.Value,
               path   = match.Groups["path"]?.Value,
               query  = match.Groups["query"]?.Value  ?? String.Empty,
               tag    = match.Groups["tag"]?.Value    ?? String.Empty,
               uri    = match.Value
            };
         }

         //
         // Match
         //
         public static URI Match(String str) => FromMatch(Finder.Match(str));

         //
         // Matches
         //
         public static URI[] Matches(String str)
         {
            var matchbox = Finder.Matches(str);
            if(matchbox.Count == 0) return null;
            var matches = new URI[matchbox.Count];
            for(var i = 0; i < matchbox.Count; i++)
               matches[i] = FromMatch(matchbox[i]);
            return matches;
         }
      }
   }
}

// EOF
