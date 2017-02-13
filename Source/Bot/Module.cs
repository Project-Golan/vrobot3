//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Base module classes.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ProjectGolan.Vrobot3
{
   namespace Modules.EventType
   {
      public delegate void OnMessage(User usr, Channel channel, String msg);
      public delegate void OnSeen(User usr, Channel channel);
   }

   //
   // BotModuleRequiresAudioAttribute
   //
   public class BotModuleRequiresAudioAttribute : Attribute
   {
      public override String ToString() => "Bot Module Requires Audio";
   }

   //
   // BotModuleDisabledAttribute
   //
   public class BotModuleDisabledAttribute : Attribute
   {
      public override String ToString() => "Bot Module Disabled";
   }

   //
   // BotModuleDiscordAttribute
   //
   public class BotModuleDiscordAttribute : Attribute
   {
      public override String ToString() => "Bot Module is Discord only";
   }

   //
   // BotModuleIRCAttribute
   //
   public class BotModuleIRCAttribute : Attribute
   {
      public override String ToString() => "Bot Module is IRC only";
   }

   //
   // BotRole
   //
   // Used for command role-checking.
   //
   public enum BotRole
   {
      User,
      HalfAdmin,
      Admin
   }

   //
   // BotCommandStructure
   //
   // Used for registering commands in a module.
   //
   public class BotCommandStructure
   {
      public BotCommand cmd;
      public String     help;
      public bool       hidden;
      public BotRole    role;
      public Type       mod;
   }

   //
   // IBotModule
   //
   // Base module class. Inherit this for your modules.
   //
   public abstract class IBotModule
   {
      //
      // Events
      //
      public struct Events
      {
         public event Modules.EventType.OnMessage onCmdMessage;
         public event Modules.EventType.OnMessage onMessage;
         public event Modules.EventType.OnSeen    onSeen;

         public void raiseOnCmdMessage(User usr, Channel channel, String msg) =>
            onCmdMessage?.Invoke(usr, channel, msg);

         public void raiseOnMessage(User usr, Channel channel, String msg) =>
            onMessage?.Invoke(usr, channel, msg);

         public void raiseOnSeen(User usr, Channel channel) =>
            onSeen?.Invoke(usr, channel);
      }

      protected void postSetup()
      {
         foreach(var kvp in commands)
            kvp.Value.mod = this.GetType();
      }

      protected IBotModule(Bot bot) {this.bot = bot;}

      public    CommandDict commands = new CommandDict();
      public    Events      events;
      protected Bot         bot;
   }
}

// EOF
