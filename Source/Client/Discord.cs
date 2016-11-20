//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Discord client.
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProjectGolan.Vrobot3.Client
{
   public class ClientDiscord : IChatClient
   {
      public Discord.DiscordClient client;
      public Discord.Server        server;

      public ClientDiscord(Bot bot) : base(bot)
      {
         this.client = new Discord.DiscordClient();

         this.info.hasAudio          = true;
         this.info.hasColors         = false;
         this.info.hasNewlines       = true;
         this.info.messageSafeMaxLen = 1777;
         this.info.shortMessages     = false;

         this.client.MessageReceived += (sender, evt) =>
         {
            if(!evt.Message.IsAuthor && !evt.User.IsBot &&
               (bot.info.channels == null ||
               bot.info.channels.Contains("#" + evt.Channel.Name)) &&
               evt.Server.Id.ToString() == bot.info.serverAddr)
            {
               var usr     = new User{};
               var channel = new Channel{};

               usr.hostname = evt.User.Id.ToString();
               usr.name     = evt.User.Nickname ?? evt.User.Name;

               channel.id   = evt.Channel.Id;
               channel.name = "#" + evt.Channel.Name;

               bot.onMessage(usr, channel, evt.Message.Text);
            }
         };
      }

      public Discord.User getUser(User usr)
      {
         if(server == null)
            server = client.GetServer(ulong.Parse(bot.info.serverAddr));
         return server.GetUser(ulong.Parse(usr.hostname));
      }

      public bool checkRole(User usr, String[] strings)
      {
         var duser = getUser(usr);

         foreach(var str in strings)
         {
            if(str[0] == '#')
            {
               var sel =
                  from role in duser.Roles
                  let strn = str.Substring(1)
                  where role.Name == strn
                  select role;

               if(sel.Any())
                  return true;
            }
            else if(usr.hostname == str)
               return true;
         }

         return false;
      }

      public override void connect()
      {
         Console.WriteLine("{0}: Creating connection.", bot.info.serverName);
         client.ExecuteAndWait(async () =>
         {
            await client.Connect(bot.info.serverPass, Discord.TokenType.Bot);
            client.SetGame("vrobot 3.1 series");
         });
      }

      public override void disconnect()
      {
         if(client != null)
         {
            client.Disconnect();
            client = null;
         }
      }

      public override void sendAction(Channel channel, String msg) =>
         client.GetChannel(channel.id)?.SendMessage(
            "_" + Discord.Format.Escape(msg ?? String.Empty) + "_");

      public override void sendMessage(Channel channel, String msg) =>
         client.GetChannel(channel.id)?.SendMessage(Discord.Format.Escape(msg ?? String.Empty));

      public override void sendMessageRaw(Channel channel, String msg) =>
         sendMessage(channel,
            "```" + Discord.Format.Escape(msg ?? String.Empty) + "```");

      public override Channel getChannel(ulong id)
      {
         var dchannel = client.GetChannel(id);
         var channel  = new Channel{};
         channel.id   = dchannel.Id;
         channel.name = "#" + dchannel.Name;
         return channel;
      }

      public override bool userPermitted(User usr, BotRole role) =>
         role == BotRole.User ||
         (role == BotRole.HalfAdmin && checkRole(usr, bot.info.roles.halfadmin)) ||
         (role == BotRole.Admin && checkRole(usr, bot.info.roles.admin));
   }
}

// EOF
