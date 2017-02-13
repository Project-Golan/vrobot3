//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Utility commands.
// .rand, .help, .decide, .eightball, .mystery
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Utils
   //
   public class Mod_Utils : IBotModule
   {
      private readonly Random rnd = Utils.GetRND();

      //
      // Mod_Utils constructor
      //
      public Mod_Utils(Bot bot) :
         base(bot)
      {
         commands["rand"] = new BotCommandStructure{
            cmd = cmdRand,
            help = "Random number device.\n" +
                   "Syntax: .rand maximum [minimum]\n" +
                   "Example: .rand 100"
         };

         commands["help"] = new BotCommandStructure{
            cmd = cmdHelp,
            help = "Shows help or a list of commands.\n" +
                   "Syntax: .help [topic]\n" +
                   "Example: .help\n" +
                   "Example: .help eightball"
         };

         commands["decide"] = new BotCommandStructure{
            cmd = cmdDecide,
            help = "Decides between 2 or more choices.\n" +
                   "Syntax: .decide x, y[, ...]\n" +
                   "Example: .decide apples, oranges, bananas"
         };

         commands["eightball"] = new BotCommandStructure{
            cmd = cmdEightball,
            help = "Peer into the magic 8-ball.\n" +
                   "Example: .eightball If I take the mask off, will you die?"
         };

         commands["mystery"] = new BotCommandStructure{
            cmd = cmdMystery,
            help = @"Does nothing. \o/"
         };

         postSetup();
      }

      //
      // cmdMystery
      //
      public void cmdMystery(User usr, Channel channel, String msg) {}

      //
      // cmdRand
      //
      public void cmdRand(User usr, Channel channel, String msg)
      {
         var args = Utils.GetArguments(msg, commands["rand"].help, 1, 2, ' ');
         double max = 0.0, min = 0.0;

         Utils.TryParse(args[0].Trim(), "Invalid maximum.", out max);

         if(args.Length == 2)
            Utils.TryParse(args[1].Trim(), "Invalid minimum.", out min);

         bot.reply(usr, channel,
            Utils.SetRange(rnd.NextDouble(), min, max).ToString());
      }

      //
      // cmdHelp
      //
      public void cmdHelp(User usr, Channel channel, String msg)
      {
         msg = msg.Trim();
         if(msg == String.Empty || msg == "admin")
            helpList(channel, msg == "admin");
         else
            helpCommand(channel, msg);
      }

      //
      // cmdDecide
      //
      public void cmdDecide(User usr, Channel channel, String msg)
      {
         var args = Utils.GetArguments(msg, commands["decide"].help, 2);
         bot.reply(usr, channel, args[rnd.Next(args.Length)].Trim());
      }

      //
      // helpList
      //
      private void helpList(Channel channel, bool admin)
      {
         var outp = String.Empty;
         var en =
            from kvp in bot.cmdfuncs
               let fhidden = kvp.Value.Item2.hidden
               let fadmin  = kvp.Value.Item2.role != BotRole.User
               where
                  bot.checkModPermissions(channel, kvp.Value.Item2.mod) &&
                  ((admin && fadmin) || (!admin && !fadmin)) &&
                  !fhidden
               orderby kvp.Key
               select kvp.Key;

         outp += "Available commands: ";

         foreach(var key in en)
         {
            outp += key;
            if(key != en.Last())
               outp += ", ";
         }

         bot.message(channel, outp);
      }

      //
      // helpCommand
      //
      private void helpCommand(Channel channel, String cmdname)
      {
         if(bot.cmdfuncs.ContainsKey(cmdname))
         {
            var str = bot.cmdfuncs[cmdname].Item2.help;
            if(!bot.clientInfo.hasNewlines) str = str.Replace("\n", " || ");
            bot.message(channel, str ?? "No help available for this command.");
         }
         else
            bot.message(channel, "Invalid command, for a list do \".help\".");
      }

      //
      // cmdEightball
      //
      public void cmdEightball(User usr, Channel channel, String msg)
      {
         String[] answers = {
            "Yes.",
            "No.",
            "Try again later.",
            "Reply hazy.",
            "Perhaps...",
            "Maybe not...",
            "Definitely.",
            "Never.",
            "system error [0xfded] try again later",
            "Can you repeat the question?",
            "Most certainly.",
            "Nope.",
            "Without a doubt.",
            "Not at all.",
            "Better not tell you now.",
            "Concentrate and ask again.",
            "It is decidedly so.",
            "My reply is \"no\".",
            "You may rely on it.",
            "Don't count on it.",
            "The answer is uncertain.",
            "Sorry, I wasn't paying attention. What'd you say?",
            "As I see it, yes.",
            "My sources say \"no\".",
            "Most likely.",
            "Very doubtful.",
            "I don't know. Ask your mom.",
            "The demon runes are quaking again. One moment.",
            "Outlook good.",
            "Outlook is not so good.",
            "Indeed.",
            "Absolutely not.",
            "Yeah, we'll go with that.",
            "That works.",
            "Of course. What are you, stupid?",
            "No. Hell no.",
            "Signs say yes.",
            "Aren't you a little too old to be believing that?",
            "Looks good to me.",
            "Sure, why not?",
            "It is certain.",
            "pls no",
            "Yes, please.",
            "Nah.",
            "Go for it!",
            "Negative.",
            "Obviously, dumbass.",
            "I doubt it.",
            "Eeeh...it's likely?",
            "Forget about it.",
            "Chances aren't good.",
            "Ahahahahahahaha no.",
            "Maybe? I think.",
            "No. Die.",
            "Huh? Oh, sure.",
            "Yeah, right...",
            "How about no.",
            "Doesn't look good to me.",
            "Probably.",
            "Obviously not, dumbass."
         };

         bot.reply(usr, channel, answers[rnd.Next(0, answers.Length)]);
      }
   }
}

// EOF
