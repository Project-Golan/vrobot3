//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Private functions for Bot.
//
//-----------------------------------------------------------------------------

using System;
using System.IO;

namespace ProjectGolan.Vrobot3
{
   //
   // Bot
   //
   public partial class Bot
   {
      //
      // runCommand
      //
      private void runCommand(User usr, Channel channel, BotCommand cmd,
         String rest)
      {
         try
         {
            cmd(usr, channel, rest ?? String.Empty);
         }
         catch(CommandArgumentException exc)
         {
            reply(usr, channel, exc.Message);
         }
         catch(Exception exc)
         {
            reply(usr, channel, "fug it borked");
            Console.WriteLine("{0}: Unhandled exception in command: {1}",
               info.serverName, exc.Message);
            File.WriteAllText(Program.Instance.dataDir + "/cmdexcdump.txt",
               exc.ToString());
         }
      }

      //
      // moduleIsValid
      //
      private bool moduleIsValid(Type type)
      {
         if(!typeof(IBotModule).IsAssignableFrom(type) ||
            !type.IsClass || type.IsAbstract)
            return false;

         foreach(var attribute in type.GetCustomAttributes(false))
         {
            if(attribute is BotModuleDisabledAttribute)
               return false;

            if(attribute is BotModuleIRCAttribute &&
               !(client is BotClientIRC))
               return false;

            if(attribute is BotModuleDiscordAttribute &&
               !(client is BotClientDiscord))
               return false;

            if(attribute is BotModuleRequiresAudioAttribute &&
               !clientInfo.hasAudio)
               return false;
         }

         return true;
      }
   }
}

// EOF
