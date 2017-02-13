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

namespace ProjectGolan.Vrobot3
{
   // Delegate type for bot commands.
   public delegate void BotCommand(User usr, Channel channel, String msg);

   // Dictionary of bot commands.
   public class CommandDict : Dictionary<String, BotCommandStructure> {}
   public class CommandFuncDict :
      Dictionary<String, Tuple<IBotModule, BotCommandStructure>> {}

   public partial class Bot
   {
      private readonly Dictionary<ulong, String> lastLine;
      private readonly Client.IChatClient        client;

      public List<IBotModule> modules  {get; private set;}
      public CommandFuncDict  cmdfuncs {get; private set;}
      public readonly BotInfo info;

      public Client.ClientInfo  clientInfo  => client.info;
      public Client.IChatClient getClient() => client;

      public Bot(BotInfo info)
      {
         this.info     = info;
         this.lastLine = new Dictionary<ulong, String>();
         this.modules  = new List<IBotModule>();
         this.cmdfuncs = new CommandFuncDict();

         switch(info.serverType)
         {
         case "IRC":     this.client = new Client.ClientIRC(this);     break;
         case "Discord": this.client = new Client.ClientDiscord(this); break;
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

      public void disconnect()
      {
         cmdfuncs.Clear();
         modules.Clear();
         client.disconnect();
      }

      private bool checkMod(Type mod, String modName)
      {
         const String modBase = "ProjectGolan.Vrobot3.Modules.";
         var modFull = modBase + modName.Substring(1);
         Type type;

              if(modName    == "*") return true;
         else if(modName[0] == '@') type = Type.GetType(modFull);
         else                       type = Type.GetType(modName);

         return type == mod;
      }

      private bool procModPermissions(Type mod, String[] e, String[] d = null)
      {
         if(e == null)
            return false;

         bool ret = false;
         foreach(var modName in e)
            if(checkMod(mod, modName)) {ret = true; break;}

         if(d != null)
            foreach(var modName in d)
               if(checkMod(mod, modName)) return false;

         return ret;
      }

      public bool checkModPermissions(Channel channel, Type mod)
      {
         String[] e = null;
         String[] d = null;

         if( info.enables.ContainsKey("*")) e =  info.enables["*"];
         if(info.disables.ContainsKey("*")) d = info.disables["*"];

         bool ret = procModPermissions(mod, e, d);

         String name = channel.name;
         e =  info.enables.ContainsKey(name) ?  info.enables[name] : null;
         d = info.disables.ContainsKey(name) ? info.disables[name] : null;

         return ret || procModPermissions(mod, e, d);
      }
   }
}

// EOF
