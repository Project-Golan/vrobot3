//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Admin commands module.
// %kill, %msg, %action
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Admin
   //
   public class Mod_Admin : IBotModule
   {
      //
      // Mod_Admin constructor
      //
      public Mod_Admin(Bot bot_) :
         base(bot_)
      {
         commands["kill"] = new BotCommandStructure{
            cmd = cmdKill,
            flags = BotCommandFlags.AdminOnly,
            help = "Kills all bot instances.\n" +
                   "Syntax: %kill"
         };

         commands["msg"] = new BotCommandStructure{
            cmd = cmdMsg,
            flags = BotCommandFlags.AdminOnly,
            help = "Sends a message.\n" +
                   "Syntax: %msg channel, msg\n" +
                   "Example: %msg #general, ur all dumb"
         };

         commands["action"] = new BotCommandStructure{
            cmd = cmdAction,
            flags = BotCommandFlags.AdminOnly,
            help = "Sends an action.\n" +
                   "Syntax: %action channel, msg\n" +
                   "Example: %action #general, explodes violently"
         };
      }

      //
      // cmdKill
      //
      public void cmdKill(User usr, Channel channel, String msg)
      {
         Console.WriteLine("{0}: Killing all instances.", bot.info.serverName);
         Program.Instance.end();
      }

      //
      // cmdMsg
      //
      public void cmdMsg(User usr, Channel channel, String msg)
      {
         String[] args = Utils.GetArguments(msg, commands["msg"].help, 2, 2);
         bot.message(ulong.Parse(args[0]), args[1].Trim());
      }

      //
      // cmdAction
      //
      public void cmdAction(User usr, Channel channel, String msg)
      {
         String[] args = Utils.GetArguments(msg, commands["action"].help, 2, 2);
         bot.action(ulong.Parse(args[0]), args[1].Trim());
      }
   }
}

