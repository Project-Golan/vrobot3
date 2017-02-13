//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Idgames search module.
// .idgames
//
//-----------------------------------------------------------------------------

using System;
using System.Net;
using System.Web;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Idgames
   //
   public class Mod_Idgames : IBotModule
   {
      private const String APIURI =
         "http://doomworld.com/idgames/api/api.php";

      private Random rnd = Utils.GetRND();

      //
      // Mod_Idgames constructor
      //
      public Mod_Idgames(Bot bot) :
         base(bot)
      {
         commands["idgames"] = new BotCommandStructure{
            cmd = cmdIdgames,
            help = "Gets an entry from the idgames archive.\n" +
                   "Syntax: .idgames [name or ID[, type[, position]]]\n" +
                   "Example: .idgames scythe, filename, 4\n" +
                   "Example: .idgames"
         };

         postSetup();
      }

      //
      // cmdIdgames
      //
      public void cmdIdgames(User usr, Channel channel, String msg)
      {
         var args = Utils.GetArguments(msg, commands["idgames"].help, 0, 3);

         switch(args.Length)
         {
         default:
         case 1:
            int id;
            if(args[0].Trim().Length == 0)
               idgamesRandom(usr, channel);
            else if(Int32.TryParse(args[0], out id))
               idgamesID(usr, channel, id);
            else
               idgames(usr, channel, args[0]);
            break;
         case 2: idgames(usr, channel, args[0], args[1]); break;
         case 3:
            idgames(usr, channel, args[0], args[1], args[2].Trim());
            break;
         }
      }

      //
      // idgamesRandom
      //
      private void idgamesRandom(User usr, Channel channel)
      {
         var req = WebRequest.Create("http://doomworld.com/idgames/?random")
            as HttpWebRequest;
         if(req == null) throw new CommandArgumentException("fug it borked");

         req.Referer = "http://doomworld.com/idgames/";
         bot.message(channel,
            Discord.Format.Escape(req.GetResponse().ResponseUri.ToString()));
      }

      //
      // idgamesID
      //
      private void idgamesID(User usr, Channel channel, int id)
      {
         var req = WebRequest.Create(APIURI + "?action=get&id=" + id)
            as HttpWebRequest;
         if(req == null) throw new CommandArgumentException("fug it borked");

         using(var response = req.GetResponse())
         {
            var xml = XDocument.Load(response.GetResponseStream());

            var x_title =
               from item in xml.Descendants("title") select item.Value;
            var x_uri = from item in xml.Descendants("url") select item.Value;

            if(!x_title.Any())
            {
               bot.message(channel, "Nothing found.");
               return;
            }

            bot.message(channel,
               Discord.Format.Escape(x_title.First() + ": " + x_uri.First()));
         }
      }

      //
      // idgames
      //
      private void idgames(User usr, Channel channel, String inquiry,
         String type = "title", String pos = "1")
      {
         var ipos = 0;

         if(pos.ToLower() != "random")
         {
            Utils.TryParse(pos, "Invalid position.", out ipos);

            if(ipos < 1)
               throw new CommandArgumentException("Invalid position.");

            ipos = ipos - 1;
         }

         inquiry = HttpUtility.UrlEncode(inquiry.Trim());
         type = HttpUtility.UrlEncode(type.Trim().ToLower());

         if(type == "name") type = "title"; // >_>'

         String[] validtypes = {
            "filename", "title", "author", "email",
            "description", "credits", "editors", "textfile"
         };

         if(!validtypes.Contains(type))
            throw new CommandArgumentException("Invalid inquiry type.");

         var uri = APIURI + "?action=search&sort=rating&query=" + inquiry +
                   "&type=" + type;
         var req = WebRequest.Create(uri);
         Console.WriteLine("idgames query: {0}", uri);

         using(var response = req.GetResponse())
         {
            var xml = XDocument.Load(response.GetResponseStream());

            var x_titles =
               from item in xml.Descendants("title") select item.Value;
            var x_uris = from item in xml.Descendants("url") select item.Value;

            if(!x_titles.Any())
            {
               bot.message(channel, "Nothing found.");
               return;
            }

            if(pos == "random")          ipos = rnd.Next(0, x_titles.Count());
            if(ipos >= x_titles.Count()) ipos = x_titles.Count() - 1;

            var title = x_titles.ElementAtOrDefault(ipos) ?? "invalid title";
            if(title.Trim().Length > 0) title = "[ " + title + " ] ";

            bot.message(channel,
               Discord.Format.Escape(String.Format("({0} of {1}{4} {2}{3}",
                  ipos + 1, x_titles.Count(), title,
                  x_uris.ElementAtOrDefault(ipos),
                  x_titles.Count() >= 100 ? "+)" : ")")));
         }
      }
   }
}

