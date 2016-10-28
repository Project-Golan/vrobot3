//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Data for the Bot class.
//
//-----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
      private readonly Dictionary<ulong, String> lastLine;
      private readonly IBotClient                client;

      public List<IBotModule> modules  { get; private set; }
      public CommandFuncDict  cmdfuncs { get; private set; }
      public readonly BotInfo info;

      public bool          isInAudioChannel => client.isInAudioChannel();
      public BotClientInfo clientInfo       => client.info;

      //
      // Bot constructor
      //
      public Bot(BotInfo info)
      {
         this.info     = info;
         this.lastLine = new Dictionary<ulong, String>();
         this.modules  = new List<IBotModule>();
         this.cmdfuncs = new CommandFuncDict();

         switch(info.serverType)
         {
         case "IRC":     this.client = new BotClientIRC(this);     break;
         case "Discord": this.client = new BotClientDiscord(this); break;
         default: throw new BotConfigurationException("Invalid server type.");
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
      // client
      //
      public void connect() => client.connect();

      public void action(Channel channel, String msg) =>
         client.sendAction(channel, msg);
      public void action(ulong id, String msg) =>
         client.sendAction(client.getChannel(id), msg);

      public void message(Channel channel, String msg) =>
         client.sendMessage(channel, msg);
      public void message(ulong id, String msg) =>
         client.sendMessage(client.getChannel(id), msg);

      public void messageRaw(Channel channel, String msg) =>
         client.sendMessageRaw(channel, msg);
      public void messageRaw(ulong id, String msg) =>
         client.sendMessageRaw(client.getChannel(id), msg);

      public void reply(User usr, Channel channel, String msg) =>
         message(channel, usr.name + ": " + msg);
      public void reply(User usr, ulong id, String msg) =>
         message(id, usr.name + ": " + msg);

      public void partAudioChannel() => client.partAudioChannel();
      public Task playAudioFile(String file) => client.playAudioFile(file);

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
   }
}

// EOF
