//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Logging.
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

delegate void HaxFn(String name, String vname);

namespace ProjectGolan.Vrobot3.Modules
{
   [BotModuleDiscord]
   public class Mod_DiscordLogger : IBotModule
   {
      public Mod_DiscordLogger(Bot bot) : base(bot)
      {
         var client = bot.getClient() as Client.ClientDiscord;

         client.client.ChannelCreated +=
         catchFunc<Discord.ChannelEventArgs>(
            async (_, e) =>
            await logMsg(e.Server, "Channel created", getChannel(e.Channel)));

         client.client.ChannelDestroyed +=
         catchFunc<Discord.ChannelEventArgs>(
            async (_, e) =>
            await logMsg(e.Server, "Channel destroyed", getChannel(e.Channel)));

         client.client.ChannelUpdated +=
         catchFunc<Discord.ChannelUpdatedEventArgs>(async (_, e) =>
         {
            var delta = getChannelDelta(e.Before, e.After);
            if(delta != String.Empty)
               await logMsg(e.Server, "Channel updated", delta);
         });

         client.client.MessageDeleted +=
         catchFunc<Discord.MessageEventArgs>(async (_, e) =>
         {
            if(!e.Message.User.IsBot)
               await verMsg(e.Server, "Message deleted", getMessage(e.Message));
         });

         client.client.MessageUpdated +=
         catchFunc<Discord.MessageUpdatedEventArgs>(async (_, e) =>
         {
            var delta = getMessageDelta(e.Before, e.After);
            if(delta != String.Empty)
               await verMsg(e.Server, "Message edited", delta);
         });

         client.client.UserBanned +=
         catchFunc<Discord.UserEventArgs>(async (_, e) =>
            await logMsg(e.Server, "User banned", getUser(e.User)));

         client.client.UserUnbanned +=
         catchFunc<Discord.UserEventArgs>(async (_, e) =>
            await logMsg(e.Server, "User unbanned", getUser(e.User)));

         client.client.UserJoined +=
         catchFunc<Discord.UserEventArgs>(async (_, e) =>
            await logMsg(e.Server, "User joined", getUser(e.User)));

         client.client.UserLeft +=
         catchFunc<Discord.UserEventArgs>(async (_, e) =>
            await logMsg(e.Server, "User removed", getUser(e.User)));

         client.client.UserUpdated +=
         catchFunc<Discord.UserUpdatedEventArgs>(async (_, e) =>
         {
            var delta = getUserDelta(e.Before, e.After);
            if(delta != String.Empty)
               await verMsg(e.Server, "User updated", delta);
         });
      }

      private EventHandler<T> catchFunc<T>(EventHandler<T> fn)
      {
         return (object sender, T evt) =>
         {
            try
               {fn(sender, evt);}
            catch(System.Exception)
               {}
         };
      }

      private String yesno(bool arg) => arg ? "Yes" : "No";

      private String getTime(DateTime time)
      {
         var offs = TimeZoneInfo.Local.GetUtcOffset(time);
         return $"{time.ToShortDateString()} {time.ToLongTimeString()} " +
                $"UTC{offs.Hours.ToString("+#;-#;0")}:{offs.Minutes}";
      }

      private String discordEscape(String str) =>
         Discord.Format.Escape(str ?? String.Empty);

      private String getChannel(Discord.Channel channel) =>
         $"`Name         :` {discordEscape(channel.Name)}\n" +
         $"`Topic        :` {discordEscape(channel.Topic)}\n" +
         $"`Id           :` `{channel.Id}`\n" +
         $"`Position     :` `{channel.Position}`\n" +
         $"`Type         :` {channel.Type.ToString()}";

      private String getUser(Discord.User user) =>
         $"`Username     :` {discordEscape(user.Name)}\n" +
         $"`Nickname     :` {discordEscape(user.Nickname)}\n" +
         $"`Id           :` `{user.Id}`\n" +
         $"`Is Bot       :` {yesno(user.IsBot)}\n" +
         $"`Joined At    :` {getTime(user.JoinedAt)}";

      private String getMessage(Discord.Message msg) =>
         $"`Time Sent    :` {getTime(msg.Timestamp)}\n" +
         $"`Channel      :` {discordEscape(msg.Channel.Name)}\n" +
         $"`Id           :` `{msg.Id}`\n" +
         $"`Content      :` {msg.Text}\n" +
         $"\nUser info:\n{getUser(msg.User)}";

      private String getMessageDelta(Discord.Message bfr, Discord.Message aft)
      {
         if(bfr.Text == aft.Text) return String.Empty;
         return $"`Time Sent    :` {getTime(aft.Timestamp)}\n" +
                $"`Channel      :` {discordEscape(aft.Channel.Name)}\n" +
                $"`Id           :` `{aft.Id}`\n" +
                $"`Old Content  :` {bfr.Text}\n" +
                $"`New Content  :` {aft.Text}\n" +
                $"\nUser info:\n{getUser(aft.User)}";
      }

      private String getPermDelta(Discord.ServerPermissions bfr,
         Discord.ServerPermissions aft)
      {
         var outp = String.Empty;

         if(bfr.RawValue != aft.RawValue)
         {
            outp += $"`Old Value    :` `{bfr.RawValue}`\n";
            outp += $"`New Value    :` `{aft.RawValue}`\n";

            HaxFn fn = (String name, String vname) => {
               var a = (bool)typeof(Discord.ServerPermissions)
                  .GetProperty(vname).GetValue(bfr);
               var b = (bool)typeof(Discord.ServerPermissions)
                  .GetProperty(vname).GetValue(aft);
               if(a != b)
                  outp += $"`{name}:` {yesno(b)}\n";
            };

            fn("Create Invite", "CreateInstantInvite");
            fn("Ban Users    ", "BanMembers");
            fn("Kick Users   ", "KickMembers");
            fn("Edit Channels", "ManageChannels");
            fn("Edit Server  ", "ManageServer");
            fn("Join Channels", "ReadMessages");
            fn("Send Messages", "SendMessages");
            fn("Send TTS     ", "SendTTSMessages");
            fn("Edit Messages", "ManageMessages");
            fn("Embed Links  ", "EmbedLinks");
            fn("Attach Files ", "AttachFiles");
            fn("Ping Everyone", "MentionEveryone");
            fn("Join Voice   ", "Connect");
            fn("Speak        ", "Speak");
            fn("Mute Users   ", "MuteMembers");
            fn("Deafen Users ", "DeafenMembers");
            fn("Move Users   ", "MoveMembers");
            fn("Use VA       ", "UseVoiceActivation");
            fn("Change Nick  ", "ChangeNickname");
            fn("Edit Nicks   ", "ManageNicknames");
            fn("Edit Roles   ", "ManageRoles");
         }
         return outp;
      }

      private String getChannelDelta(Discord.Channel bfr, Discord.Channel aft)
      {
         var outp1 = String.Empty;
         var outp2 = String.Empty;
         if(bfr.Name != aft.Name)
         {
            outp1 += "Name changed.\n";
            if(!String.IsNullOrEmpty(bfr.Name))
               outp2 += $"`Old Name     :` {discordEscape(bfr.Name)}\n";
            if(!String.IsNullOrEmpty(aft.Name))
               outp2 += $"`New Name     :` {discordEscape(aft.Name)}\n";
         }
         if(bfr.Id != aft.Id)
         {
            outp1 += "Id changed.\n";
            outp2 += $"`Old Id       :` `{bfr.Id}`\n";
            outp2 += $"`New Id       :` `{aft.Id}`\n";
         }
         if(bfr.Topic != aft.Topic)
         {
            outp1 += "Topic changed.\n";
            if(!String.IsNullOrEmpty(bfr.Topic))
               outp2 += $"`Old Topic    :` {discordEscape(bfr.Topic)}\n";
            if(!String.IsNullOrEmpty(aft.Topic))
               outp2 += $"`New Topic    :` {discordEscape(aft.Topic)}\n";
         }
         if(bfr.Position != aft.Position)
         {
            outp1 += "Position changed.\n";
            outp2 += $"`Old Position :` `{bfr.Position}`\n";
            outp2 += $"`New Position :` `{aft.Position}`\n";
         }
         if(outp1 != String.Empty)
            outp2 += $"\nChannel info:\n{getChannel(aft)}\n";
         return outp1 + outp2.Trim();
      }

      private String getUserDelta(Discord.User bfr, Discord.User aft)
      {
         var outp1 = String.Empty;
         var outp2 = String.Empty;
         var perm = getPermDelta(bfr.ServerPermissions, aft.ServerPermissions);
         if(bfr.Name != aft.Name)
         {
            outp1 += "Username changed.\n";
            if(!String.IsNullOrEmpty(bfr.Name))
               outp2 += $"`Old Username :` {discordEscape(bfr.Name)}\n";
            if(!String.IsNullOrEmpty(aft.Name))
               outp2 += $"`New Username :` {discordEscape(aft.Name)}\n";
         }
         if(bfr.Nickname != aft.Nickname)
         {
            outp1 += "Nickname changed.\n";
            if(!String.IsNullOrEmpty(bfr.Nickname))
               outp2 += $"`Old Nickname :` {discordEscape(bfr.Nickname)}\n";
            if(!String.IsNullOrEmpty(aft.Nickname))
               outp2 += $"`New Nickname :` {discordEscape(aft.Nickname)}\n";
         }
         if(bfr.AvatarId != aft.AvatarId)
         {
            outp1 += "Avatar changed.\n";
            outp2 += $"`Old Avatar Id:` `{bfr.AvatarId}`\n";
            outp2 += $"`New Avatar Id:` `{aft.AvatarId}`\n";
            outp2 += $"`Old AvatarUrl:` `{bfr.AvatarUrl}`\n";
            outp2 += $"`New AvatarUrl:` `{aft.AvatarUrl}`\n";
         }
         if(bfr.Discriminator != aft.Discriminator)
         {
            outp1 += "Unique Id changed.\n";
            outp2 += $"`Old Unique Id:` `{bfr.Discriminator}`\n";
            outp2 += $"`New Unique Id:` `{aft.Discriminator}`\n";
         }
         if(bfr.Id != aft.Id)
         {
            outp1 += "Id changed.\n";
            outp2 += $"`Old Id       :` `{bfr.Id}`\n";
            outp2 += $"`New Id       :` `{aft.Id}`\n";
         }
         if(!String.IsNullOrEmpty(perm))
         {
            outp1 += "Permissions changed.\n";
            outp2 += perm;
         }
         if(outp1 != String.Empty)
            outp2 += $"\nUser info:\n{getUser(aft)}\n";
         return outp1 + outp2.Trim();
      }

      private async Task sndMsg(Discord.Server server, String name,
         String type, String msg)
      {
         if(server.Id.ToString() != bot.info.serverAddr) return;
         var log = server.FindChannels(name).FirstOrDefault();
         if(log == null) return;
         var time = getTime(DateTime.Now);
         if(msg.Length > 1800)
            await log.SendMessage($"{type} at {time}\n[Omitted, too long]");
         else
            await log.SendMessage($"{type} at {time}\n{msg}");
      }

      private async Task logMsg(Discord.Server server, String type, String msg)
      => await sndMsg(server, "log", type, msg);

      private async Task verMsg(Discord.Server server, String type, String msg)
      => await sndMsg(server, "log-v", type, msg);
   }
}

