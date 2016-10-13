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
         Random rnd = new Random(unchecked((int)(RNDHash & 0x7fffffff)));
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

         if(max == 0)
            split = msg.Split(splitseq);
         else
            split = msg.Split(splitseq, max);

         if(min >= 0 && split.Length < min)
            throw new CommandArgumentException(help);

         return split;
      }

      //
      // SetRange
      //
      public static Double SetRange(Double x, Double min, Double max)
         => ((max - min) * x) + min;

      //
      // FuzzyRelativeDate
      //
      public static String FuzzyRelativeDate(DateTime then, DateTime now)
      {
         TimeSpan span = now.Subtract(then);

         if(span.Seconds == 0)
            return "now";

         String denom = span.Days > 0 ? "day" :
                        span.Hours > 0 ? "hour" :
                        span.Minutes > 0 ? "minute" :
                        "second";

         int number;
         switch(denom)
         {
         default:       number = 0;            break;
         case "second": number = span.Seconds; break;
         case "minute": number = span.Minutes; break;
         case "hour":   number = span.Hours;   break;
         case "day":    number = span.Days;    break;
         }

         return String.Format("{0} {1}{2} ago", number, denom,
            number != 1 ? "s" : String.Empty);
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
            byte[] bufp = new byte[maxsize];
            int read;

            using(var stream = resp.GetResponseStream())
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
