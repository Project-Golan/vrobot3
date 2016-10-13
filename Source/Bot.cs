//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Main bot class.
//
//-----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using CommandFuncDict =
   System.Collections.Generic.Dictionary<
      System.String,
      System.Tuple<
         ProjectGolan.Vrobot3.IBotModule,
         ProjectGolan.Vrobot3.BotCommandStructure>>;

namespace ProjectGolan.Vrobot3
{
   //
   // BotCommand
   //
   // Delegate type for bot commands.
   //
   public delegate void BotCommand(User usr, Channel channel, String msg);

   //
   // CommandDict
   //
   // Dictionary of bot commands.
   //
   public class CommandDict : Dictionary<String, BotCommandStructure> {}

   //
   // Bot
   //
   public partial class Bot
   {
      public List<IBotModule> modules  = new List<IBotModule>();
      public CommandFuncDict  cmdfuncs = new CommandFuncDict();
      public readonly BotInfo info;

      private Dictionary<ulong, String> lastLine = new Dictionary<ulong, String>();
      private IBotClient client;

      public bool isInAudioChannel => false;
      public ServerInfo serverInfo => client.info;

      //
      // Bot constructor
      //
      public Bot(BotInfo info)
      {
         this.info = info;

         switch(info.serverType)
         {
         case ServerType.IRC:     client = new BotClientIRC(this);     break;
         case ServerType.Discord: client = new BotClientDiscord(this); break;
         }

         var modClasses =
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
                 from type in assembly.GetTypes()
                    where moduleIsValid(type)
                    select type;

         foreach(var mod in modClasses)
            modules.Add(Activator.CreateInstance(mod, this) as IBotModule);

         foreach(var mod in modules)
            foreach(var kvp in mod.commands)
               cmdfuncs.Add(kvp.Key, Tuple.Create(mod, kvp.Value));
      }

      //
      // connect
      //
      public void connect() => client.connect();

      //
      // disconnect
      //
      public void disconnect()
      {
         cmdfuncs.Clear();
         modules.Clear();
         client.disconnect();
      }

      //
      // reply
      //
      public void reply(User usr, Channel channel, String msg) =>
         message(channel, usr.name + ": " + msg);
      public void reply(User usr, ulong id, String msg) =>
         message(id, usr.name + ": " + msg);

      //
      // message
      //
      public void message(Channel channel, String msg) =>
         client.sendMessage(channel, msg);
      public void message(ulong id, String msg) =>
         client.sendMessage(client.getChannel(id), msg);

      //
      // action
      //
      public void action(Channel channel, String msg) =>
         client.sendAction(channel, msg);
      public void action(ulong id, String msg) =>
         client.sendAction(client.getChannel(id), msg);

      //
      // joinAudioChannel
      //
      public async Task<bool> joinAudioChannel(User user)
      {
         var channel = client.getAudioChannel(user);
         if(channel != null)
         {
            await client.joinAudioChannel(channel);
            return true;
         }
         else
            return false;
      }

      //
      // partAudioChannel
      //
      public void partAudioChannel() => client.partAudioChannel();

      //
      // playAudioFile
      //
      public Task playAudioFile(String file) => client.playAudioFile(file);

      //
      // checkModPermissions
      //
      public bool checkModPermissions(Channel channel, Type mod)
      {
         String[] enables;

         if(info.enables.ContainsKey(channel.name))
            enables = info.enables[channel.name];
         else if(info.enables.ContainsKey("*"))
            enables = info.enables["*"];
         else
            return true;

         foreach(var modname in enables)
         {
            Type type;

            if(modname == "*")
               return true;
            else if(modname[0] == '@')
               type = Type.GetType("ProjectGolan.Vrobot3.Modules." + modname.Substring(1));
            else
               type = Type.GetType(modname);

            if(type == mod)
               return true;
         }

         return false;
      }

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
            if(exc.Message != null)
               reply(usr, channel, exc.Message);
            else
               Console.WriteLine("{0}: Unknown CommandArgumentException",
                  info.serverName);
         }
         catch(Exception exc)
         {
            reply(usr, channel, "fug it borked");
            Console.WriteLine("{0}: Unhandled exception in command: {1}",
               info.serverName, exc?.Message ?? "unknown error");
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
            if((attribute is BotModuleIRCAttribute &&
               info.serverType != ServerType.IRC) ||
               (attribute is BotModuleDiscordAttribute &&
               info.serverType != ServerType.Discord) ||
               (attribute is BotModuleRequiresAudioAttribute &&
               !serverInfo.hasAudio) ||
               attribute is BotModuleDisabledAttribute)
               return false;
         }

         return true;
      }
   }
}

// EOF
