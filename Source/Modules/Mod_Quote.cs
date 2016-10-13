//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Doominati Quote DB interface command.
// .quote
//
//-----------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Quote
   //
   public class Mod_Quote : IBotModule
   {
      private struct QDBInterface
      {
         public int numQuotes;
      }

      static readonly String APIURI = "http://www.greyserv.net/qdb/q/";
      static readonly String InterfaceURI =
         "http://www.greyserv.net/qdb/interface.cgi";
      private Random rnd = Utils.GetRND();

      //
      // Mod_Quote constructor
      //
      public Mod_Quote(Bot bot_) :
         base(bot_)
      {
         commands["quote"] = new BotCommandStructure{
            cmd = cmdQuote,
            help = "Get a quote from the Doominati Quote DB.\n" +
                   "Syntax: .quote [id]\n" +
                   "Example: .quote 536"
         };
      }

      //
      // cmdQuote
      //
      public void cmdQuote(User usr, Channel channel, String msg)
      {
         var inter = JsonConvert.DeserializeObject<QDBInterface>(
            Utils.GetResponseString(InterfaceURI, 64));

         int id;

         if(String.IsNullOrEmpty(msg?.Trim()) || !int.TryParse(msg, out id))
            id = rnd.Next(inter.numQuotes);
         else if(id < 0 || id > inter.numQuotes)
            throw new CommandArgumentException("invalid quote ID");

         var quote = Utils.GetResponseString(APIURI + id.ToString(),
            bot.serverInfo.messageSafeMaxLen);

         if(String.IsNullOrEmpty(quote))
            throw new CommandArgumentException("QDB exploded try again later");

         if(bot.serverInfo.shortMessages)
            quote = Regex.Replace(quote, "\n+", "\n").Trim();

         var lines = quote.Split('\n');

         if(bot.serverInfo.shortMessages &&
            (lines.Length > 5 || quote.Length > 600))
         {
            bot.reply(usr, channel, "Quote is too long.");
            return;
         }

         if(bot.serverInfo.hasNewlines)
            bot.message(channel, quote);
         else
            foreach(var ln_ in lines)
            {
               String ln = ln_.Trim();
               if(ln.Length > 0)
                  bot.message(channel, ln);
            }
      }
   }
}

