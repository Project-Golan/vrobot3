//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Useful utilities.
//
//-----------------------------------------------------------------------------

using System;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace ProjectGolan.Vrobot3
{
   //
   // Utils
   //
   public static partial class Utils
   {
      private static long RNDHash = 0x7f083dfd7f083dfd;

      //
      // List.AddItem
      //
      public static T AddItem<T>(this List<T> list, T item)
      {
         list.Add(item);
         return item;
      }

      //
      // GetRND
      //
      public static Random GetRND()
      {
         RNDHash *= DateTime.UtcNow.ToFileTime();
         var rnd = new Random(unchecked((int)(RNDHash & 0x7fffffff)));
         RNDHash ^= 0x7f8f8f8f8f8f8f8f;
         RNDHash >>= 4;
         RNDHash += 0x7f0000007f000000;
         return rnd;
      }

      //
      // GetArguments
      //
      public static String[] GetArguments(String msg, String help, int min,
         int max = 0, char splitchr = ',')
      {
         char[] splitseq = { splitchr };
         String[] split;

         if(min == 1 && msg == String.Empty)
            throw new CommandArgumentException(help);

         split = max == 0 ? msg.Split(splitseq) : msg.Split(splitseq, max);

         if(min >= 0 && split.Length < min)
            throw new CommandArgumentException(help);

         return split;
      }

      //
      // SetRange
      //
      public static double SetRange(double x, double min, double max)
         => ((max - min) * x) + min;

      //
      // FuzzyRelativeDate
      //
      public static String FuzzyRelativeDate(DateTime then, DateTime now)
      {
         var span = now.Subtract(then);

         if(span.Seconds == 0)
            return "now";

         String denom;
         int number;

              if(span.Days    > 0) {denom = "day";    number = span.Days;}
         else if(span.Hours   > 0) {denom = "hour";   number = span.Hours;}
         else if(span.Minutes > 0) {denom = "minute"; number = span.Minutes;}
         else                      {denom = "second"; number = span.Seconds;}

         return $"{number} {denom}{(number != 1 ? "s" : String.Empty)} ago";
      }

      //
      // FuzzyRelativeDate
      //
      public static String FuzzyRelativeDate(DateTime then)
         => FuzzyRelativeDate(then, DateTime.Now);

      //
      // GetResponseString
      //
      public static String GetResponseString(WebResponse resp, int maxsize)
      {
         try
         {
            var bufp = new byte[maxsize];
            var read = 0;

            using(var stream = resp.GetResponseStream())
               if(stream != null)
                  read = stream.Read(bufp, 0, maxsize);

            return Encoding.Default.GetString(bufp, 0, read);
         }
         catch(Exception exc)
         {
            Console.WriteLine("URL request error: {0}",
               exc.Message ?? "[unknown]");
            return null;
         }
      }

      //
      // GetResponseString
      //
      public static String GetResponseString(String uri, int maxsize,
         String referer = null)
      {
         try
         {
            var req = WebRequest.Create(uri);

            if(referer != null)
            {
               var req_ = req as HttpWebRequest;
               req_.Referer = referer;
            }

            using(var resp = req.GetResponse())
               return GetResponseString(resp, maxsize);
         }
         catch(Exception exc)
         {
            Console.WriteLine("URL request error: {0}",
               exc.Message ?? "[unknown]");
            return null;
         }
      }

      //
      // TryParse
      //
      public static void TryParse(String str, String err, out double outp)
      {
         if(!double.TryParse(str, out outp))
            throw new CommandArgumentException(err);
      }

      //
      // TryParse
      //
      public static void TryParse(String str, String err, out int outp)
      {
         if(!int.TryParse(str, out outp))
            throw new CommandArgumentException(err);
      }
   }
}

// EOF
