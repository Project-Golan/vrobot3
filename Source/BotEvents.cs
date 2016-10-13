//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Bot events.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectGolan.Vrobot3
{
   //
   // Bot
   //
   public partial class Bot
   {
      //
      // onSeen
      //
      public void onSeen(User usr, Channel channel)
      {
         foreach(var mod in modules)
            if(checkModPermissions(channel, mod.GetType()))
               mod.events.raiseOnSeen(usr, channel);
      }

      //
      // onMessage
      //
      // Attempt to run commands.
      //
      public void onMessage(User usr, Channel channel, String msg)
      {
         var validCmdPreceders = ".%".ToCharArray();
         String rest = null;

         if(msg.Length >= 1 && validCmdPreceders.Contains(msg[0]))
         {
            Predicate<BotCommandStructure> pred;

            if(msg[0] == '%')
               pred = fn =>  fn.flags.HasFlag(BotCommandFlags.AdminOnly);
            else
               pred = fn => !fn.flags.HasFlag(BotCommandFlags.AdminOnly);

            var dict = from fn in cmdfuncs where pred(fn.Value.Item2) select fn;

            // Get the command name.
            String[] splt = msg.Substring(1).Split(" ".ToCharArray(), 2);
            String cmdname = splt[0].ToLower();

            // Handle commands ending with "^".
            // These take the last message as input.
            if(cmdname.EndsWith("^"))
            {
               cmdname = cmdname.Substring(0, cmdname.Length - 1);
               lastLine.TryGetValue(channel.id, out rest);
            }

            var tcmd =
               from kvp in dict where kvp.Key == cmdname select kvp.Value;
            if(tcmd.Any())
            {
               var tcmdr = tcmd.First();

               // Check permissions.
               if(usr.hostname != info.adminId &&
                  (tcmdr.Item2.flags.HasFlag(BotCommandFlags.AdminOnly) ||
                  !checkModPermissions(channel, tcmdr.Item1.GetType())))
                  goto RaiseMessage;

               // If we have input, grab that too.
               if(rest == null && splt.Length > 1)
                  rest = splt[1];
               else if(rest == null)
                  rest = String.Empty;

               // Go over each module and raise a command message event.
               foreach(var mod in modules)
                  if(checkModPermissions(channel, mod.GetType()))
                     mod.events.raiseOnCmdMessage(usr, channel, msg);

               runCommand(usr, channel, tcmdr.Item2.cmd, rest);
               return;
            }
         }

      RaiseMessage:
         // Go over each module and raise a message event.
         foreach(var mod in modules)
            if(checkModPermissions(channel, mod.GetType()))
               mod.events.raiseOnMessage(usr, channel, msg);

         lastLine[channel.id] = msg;
      }
   }
}

// EOF
