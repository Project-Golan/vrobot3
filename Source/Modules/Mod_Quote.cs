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

      const String APIURI = "http://www.greyserv.net/qdb/q/";
      const String InterfaceURI = "http://www.greyserv.net/qdb/interface.cgi";
      private Random rnd = Utils.GetRND();

      //
      // Mod_Quote constructor
      //
      public Mod_Quote(Bot bot_) :
         base(bot_)
      {
         commands["quote"] = new BotCommandStructure{
            cmd  = cmdQuote,
            help = "Get a quote from the Doominati Quote DB.\n" +
                   "Syntax: .quote [id]\n" +
                   "Example: .quote 536"
         };

         postSetup();
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
            bot.clientInfo.messageSafeMaxLen);

         if(String.IsNullOrEmpty(quote))
            throw new CommandArgumentException("QDB exploded try again later");

         if(bot.clientInfo.shortMessages)
            quote = Regex.Replace(quote, "\n+", "\n").Trim();

         var lines = quote.Split('\n');

         if(bot.clientInfo.shortMessages &&
            (lines.Length > 5 || quote.Length > 600))
         {
            bot.reply(usr, channel, "Quote is too long.");
            return;
         }

         if(bot.clientInfo.hasNewlines)
            bot.message(channel, quote);
         else
            foreach(var ln_ in lines)
            {
               var ln = ln_.Trim();
               if(ln.Length > 0)
                  bot.message(channel, ln);
            }
      }
   }
}

