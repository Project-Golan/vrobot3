//
// Mod_Links.cs
//
// Link title capabilities.
//

using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using Sharkbite.Irc;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProjectGolan.Vrobot3
{
   //
   // Mod_Links
   //

   public sealed class Mod_Links : IBotModule
   {
      //
      // URI
      //

      private struct URI
      {
         public String method, host, path, query, tag, uri;
      }

      //
      // Delegates.

      private delegate void URIHandler(URI uri, String referer, ref String result);

      //
      // Ctor
      //

      public Mod_Links(Bot bot_) :
         base(bot_)
      {
         events.OnMessage += Evt_OnMessage;
      }

      //
      // Evt_OnMessage
      //

      public void Evt_OnMessage(UserInfo usr, String channel, String msg, bool iscmd)
      {
         //
         // Do this asynchronously, we don't want link parsing to block operation.

         new Thread(() => {
            try
            {
               if(!iscmd)
                  TryParseURIs(channel, msg);
            }
            catch(Exception exc)
            {
               Console.WriteLine("{0}: URL thread error: {1}", bot.n_groupname, exc.Message);
            }
         }).Start();
      }

      //
      // GetURITitle
      //

      private Match GetURITitle(URI uri, String referer, int kb = 16)
      {
         String rstr = Utils.GetResponseString(uri.uri, 1024 * kb, referer);

         if(rstr == null)
            return null;

         return new Regex(@"\<title\>(?<realtitle>.+?)\</title\>").Match(rstr);
      }

      //
      // URI_Default
      //

      private void URI_Default(URI uri, String referer, ref String result)
      {
         var req = WebRequest.Create(uri.uri) as HttpWebRequest;
         req.Referer = referer;
         req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.9) Gecko/20100101 Firefox/31.9";

         using(var response = req.GetResponse() as HttpWebResponse)
         {
            var html = new HtmlDocument();
            html.LoadHtml(Utils.GetResponseString(response, 16*1024));
            var x_title = from item in html.DocumentNode.Descendants()
                          where (item?.Name ?? String.Empty) == "title" ||
                                ((item?.Name ?? String.Empty) == "meta" &&
                                (item?.Attributes["id"]?.Value ?? String.Empty).EndsWith("title"))
                          select item;

            if(x_title.Any())
               result = WebUtility.HtmlDecode(x_title.First().InnerText.Trim(new char[]{ ' ', '\t', '\n' }));
         }
      }

      //
      // URI_Youtube
      //
      // Special fucking flower.
      //

      private void URI_Youtube(URI uri, String referer, ref String result)
      {
         var req = WebRequest.Create(uri.uri) as HttpWebRequest;
         req.Referer = referer;

         using(var response = req.GetResponse() as HttpWebResponse)
         {
            var html = new HtmlDocument();
            html.Load(response.GetResponseStream());
            var x_title = from item in html.DocumentNode.Descendants()
                          where (item?.Attributes["id"]?.Value ?? String.Empty) == "eow-title"
                          select item;

            if(x_title.Any())
               result = WebUtility.HtmlDecode(x_title.First().InnerText.Trim(new char[]{ ' ', '\t', '\n' })) +
                        " - YouTube";
         }
      }


      //
      // URI_Gelooru
      //

      private void URI_Gelbooru(URI uri, String referer, ref String result)
      {
         var match = GetURITitle(uri, referer, 8); // Should be OK to just get the first 8kb here.
         if(match?.Success == true)
         {
            String title = WebUtility.HtmlDecode(match.Groups["realtitle"].Value);
            if(title.Contains("Image View"))
               result = "Image View - Gelbooru";
            else
               result = title;
         }
      }

      //
      // URI_Hitbox
      //

      private void URI_Hitbox(URI uri, String referer, ref String result)
      {
         String name = WebUtility.HtmlEncode(uri.path.TrimStart(new char[]{'/'}));

         var req = WebRequest.Create("https://api.hitbox.tv/media/live/" + name + "?fast") as HttpWebRequest;
         req.Referer = referer;

         using(var response = req.GetResponse() as HttpWebResponse)
         {
            var json = JObject.Parse(Utils.GetResponseString(response, 64 * 1024));
            var node = json["livestream"][0];
            String displayname = (String)node["media_display_name"];
            String status = (String)node["media_status"];
            bool live = Int32.Parse((String)node["media_is_live"] ?? "0") == 1;

            result = displayname;
            if(live)
               result += " (live)";
            if(!String.IsNullOrEmpty(status))
               result += ": " + status;
            result += " - hitbox";
         }
      }

      //
      // TryParseURIs
      //
      // This function is really complicated because of exploits. Fuck exploits.
      //

      private void TryParseURIs(String channel, String msg)
      {
         try
         {
            Regex r_finduris = new Regex(
               @"((?<method>[^:/?# ]+):)" +
               @"(//(?<host>[^/?# ]*))" +
               @"(?<path>[^?# ]*)" +
               @"(?<query>\?([^# ]*))?" +
               @"(?<tag>#(.*))?"
            );

            var matchbox = r_finduris.Matches(msg);

            if(matchbox.Count != 0)
            {
               String outp = String.Empty;

               for(int i = 0; i < matchbox.Count; i++)
               {
                  var match = matchbox[i];

                  URI uri = new URI{
                     method = match.Groups["method"].Value,
                     host = match.Groups["host"].Value,
                     path = match.Groups["path"].Value,
                     query = match.Groups["query"]?.Value ?? String.Empty,
                     tag = match.Groups["tag"]?.Value ?? String.Empty,
                     uri = match.Value
                  };

                  //
                  // Will the real URI please stand up?

                  if(uri.method == "http" || uri.method == "https")
                  {
                     var req = WebRequest.Create(uri.uri) as HttpWebRequest;
                     using(var resp = req.GetResponse())
                        if(resp.ResponseUri.Host != uri.host)
                        {
                           uri.method = resp.ResponseUri.Scheme;
                           uri.host = resp.ResponseUri.Host;
                           uri.path = resp.ResponseUri.AbsolutePath;
                           uri.query = resp.ResponseUri.Query;
                           uri.tag = resp.ResponseUri.Fragment;
                           uri.uri = resp.ResponseUri.OriginalString;
                        }
                  }

                  if(uri.path.Length == 0)
                     uri.path = "/";

                  //
                  // Make sure the method is OK.
                  // Previously:
                  // [22:19] <marrub> file:///srv/www/marrub/oldmen.html
                  // [22:19] <vrobot3> [ OLD MEN OLD MEN OLD MEN OLD MEN OLD MEN OLD MEN OLD MEN OLD ... ]

                  String[] validmethods = { "ftp", "ftps", "http", "https" };
                  if(!validmethods.Contains(uri.method))
                     continue;

                  //
                  // Try and get a decent title from the URL.

                  URIHandler handler = URI_Default;
                  String result = String.Empty;
                  String referer = null;

                  if(uri.method == "http" || uri.method == "https")
                  {
                     referer = uri.method + "://" + uri.host;

                     Dictionary<String, URIHandler> handlers = new Dictionary<String, URIHandler>(){
                        { "youtube.com",  URI_Youtube  },
                        { "youtu.be",     URI_Youtube  },
                        { "gelbooru.com", URI_Gelbooru },
                        { "hitbox.tv",    URI_Hitbox   },
                     };

                     String hostst = Regex.Replace(uri.host, @"^www\.", String.Empty, RegexOptions.Multiline);
                     if(handlers.ContainsKey(hostst))
                        handler = handlers[hostst];
                  }

                  //
                  // Handle grabbing the title. Just get on with it if we throw an exception.

                  try
                   { handler(uri, referer, ref result); }
                  catch(Exception exc)
                  {
                     Console.WriteLine("URL handle exception: {0}", exc.Message);
                     continue;
                  }

                  //
                  // Sanitize.

                  result.Trim();

                  for(int j = result.Length - 1; j >= 0; j--)
                  {
                     Char ch = result[j];
                     if((Char.IsWhiteSpace(ch) && ch != ' ') || Char.IsControl(ch) || Char.IsSurrogate(ch))
                        result = result.Remove(j, 1);
                  }

                  //
                  // If the result is 0-length, just get rid of it.

                  if(result.Trim().Length == 0)
                     continue;

                  //
                  // Throw the result into the output buffer.

                  outp += result;

                  //
                  // If the output is too long, we need to shorten it and break.

                  if(outp.Length > 400 - 3)
                  {
                     outp = outp.Substring(0, 400 - 3);
                     outp += "···";
                     break;
                  }

                  //
                  // Add separators.

                  if(i != matchbox.Count - 1)
                     outp += " | ";
               }

               if(outp.Length > 0)
                  bot.Message(channel, "[ " + outp + " ]");
            }
         }
         catch(Exception exc)
         {
            Console.WriteLine("{0}: URL parse error: {1}", bot.n_groupname, exc.Message ?? "[unknown]");
         }
      }
   }
}

