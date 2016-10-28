//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Fun stuff.
// .carmack, .revenant, .wan, .nyan, .:^)
//
//-----------------------------------------------------------------------------

using System;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Fun
   //
   public class Mod_Fun : IBotModule
   {
      //
      // ShitpostingDevice
      //
      private class ShitpostingDevice
      {
         private String word, final;
         private Random rnd = Utils.GetRND();
         private int min, max;
         private Bot bot;

         //
         // ShitpostingDevice constructor
         //
         public ShitpostingDevice(String word, String final, int min, int max,
            Bot bot)
         {
            this.word = word;
            this.final = final;
            this.min = min;
            this.max = max;
            this.bot = bot;
         }

         //
         // run
         //
         public void run(User usr, Channel channel, String msg)
         {
            var n = rnd.Next(min, max);
            var outp = String.Empty;

            if(bot.clientInfo.hasColors && rnd.Next(0, 8) == 1)
               for(int i = 0; i < 6; i++)
               {
                  String[] colors = { "04", "07", "08", "09", "12", "06" };
                  outp += "\x03";
                  outp += colors[i];
                  outp += word;
                  outp += word;
               }
            else
               for(var i = 0; i < n; i++)
                  outp += word;

            bot.reply(usr, channel, outp + final);
         }
      }

      //
      // Mod_Fun constructor
      //
      public Mod_Fun(Bot bot) :
         base(bot)
      {
         commands["carmack"] = new BotCommandStructure{
            cmd = new ShitpostingDevice("MM", "", 3, 20, bot).run,
            flags = BotCommandFlags.Hidden
         };
         commands["revenant"] = new BotCommandStructure{
            cmd = new ShitpostingDevice("AA", "", 3, 20, bot).run,
            flags = BotCommandFlags.Hidden
         };
         commands["wan"] = new BotCommandStructure{
            cmd = new ShitpostingDevice("wan ", "- !", 2, 12, bot).run,
            flags = BotCommandFlags.Hidden
         };
         commands["nyan"] = new BotCommandStructure{
            cmd = new ShitpostingDevice("nyan ", "!~", 2, 12, bot).run,
            flags = BotCommandFlags.Hidden
         };
         commands[":^)"] = new BotCommandStructure{
            cmd = (usr, channel, msg) => bot.message(channel, ":^)"),
            flags = BotCommandFlags.Hidden
         };

         events.onMessage += onMessage;
      }

      //
      // onMessage
      //
      public void onMessage(User usr, Channel channel, String msg)
      {
         if(msg.Contains("OLD MEN"))
            bot.message(channel, "WARNING! WARNING!");
      }
   }
}

// EOF
