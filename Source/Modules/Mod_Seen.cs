//
// Mod_Seen.cs
//
// .seen
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sharkbite.Irc;
using Newtonsoft.Json;
using Tarczynski.NtpDateTime;

namespace ProjectGolan.Vrobot3
{
   //
   // Mod_Seen
   //

   public sealed class Mod_Seen : IBotModule
   {
      //
      // SeenName
      //

      private class SeenName
      {
         public String real, check;
         public DateTime time;
      }

      //
      // SeenDates
      //

      private class SeenDates : List<SeenName> {}

      //
      // Data.

      private SeenDates seendates = new SeenDates();
      private TimeZoneInfo burb;
      private DateTime lastwrite = DateTime.Now;

      //
      // Ctor
      //

      public Mod_Seen(Bot bot_) :
         base(bot_)
      {
         if(File.Exists("/srv/irc/vrobot3/data/seendates." + bot.n_groupname + ".json"))
            seendates = JsonConvert.DeserializeObject<SeenDates>(File.ReadAllText("/srv/irc/vrobot3/data/seendates." +
                                                                                  bot.n_groupname + ".json"));

         commands["seen"] = new BotCommandStructure { cmd = Cmd_Seen,
            help = "Responds with the last time I saw someone. || " +
                   "Syntax: .seen person || " +
                   "Example: .seen vrobot3"
         };
         
         events.OnSeen += Evt_OnSeen;
         events.OnDisconnected += Evt_OnDisconnected;

         burb = TimeZoneInfo.CreateCustomTimeZone("burb", new TimeSpan(10, -30, 0), "burb", "burb");
      }

      //
      // Cmd_Seen
      //

      public void Cmd_Seen(UserInfo usr, String channel, String msg)
      {
         if(msg.Length == 0 || msg.Contains(" "))
            throw new CommandArgumentException("Invalid name.");

         String name = msg.ToLower();
         var seen = from sdata in seendates where sdata.check == name select sdata;
         if(seen.Any())
         {
            var other = seen.First();
            String outp = String.Empty;

            outp += "I last saw ";
            outp += other.real;
            outp += " active ";
            outp += Utils.FuzzyRelativeDate(other.time, DateTime.Now.FromNtp());
            outp += ", at ";
            outp += other.time.ToShortTimeString();
            outp += " CST (";
            outp += TimeZoneInfo.ConvertTime(other.time, TimeZoneInfo.Local, burb).ToShortTimeString();
            outp += " pidgeon time).";

            bot.Reply(usr, channel, outp);
         }
         else
            bot.Reply(usr, channel, "I haven't seen " + msg + " before, sorry.");

         WriteSeenDates();
      }

      //
      // Evt_OnScreen
      //

      public void Evt_OnSeen(UserInfo usr, String channel)
      {
         String name = usr.Nick.ToLower();
         var seen = from sdata in seendates where sdata.check == name select sdata;
         if(seen.Any())
         {
            seen.First().time = DateTime.Now.FromNtp();
            seen.First().real = usr.Nick;
         }
         else
            seendates.Add(new SeenName{ real = usr.Nick, check = usr.Nick.ToLower(), time = DateTime.Now.FromNtp() });

         if(DateTime.Now.Subtract(lastwrite).Minutes >= 30)
            WriteSeenDates();
      }

      //
      // Evt_OnDisconnected
      //

      public void Evt_OnDisconnected()
      {
         WriteSeenDates();
      }

      //
      // WriteSeenDates
      //

      private void WriteSeenDates()
      {
         File.WriteAllText("/srv/irc/vrobot3/data/seendates." + bot.n_groupname + ".json",
                           JsonConvert.SerializeObject(seendates));
      }
   }
}

