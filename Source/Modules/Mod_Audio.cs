//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Audio-based commands.
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Audio
   //
   [BotModuleRequiresAudio, BotModuleDisabled]
   public class Mod_Audio : IBotModule
   {
      //
      // QueueItem
      //
      class QueueItem
      {
         private Utils.URI uri;

         public QueueItem(Utils.URI uri)
         {
            this.uri = uri;
         }

         public override String ToString() => String.Empty;
      }

      //
      // Queue
      //
      class Queue
      {
         private TimeSpan        curTime;
         private List<QueueItem> items;
         private int             pos;

         public Queue()
         {
            this.curTime = new TimeSpan();
            this.items   = new List<QueueItem>();
            this.pos     = 0;
         }

         public bool addItem(Utils.URI uri)
         {
            var item = new QueueItem(uri);
            items.Add(item);
            return true;
         }
      }

      private readonly String[] validMethods =
         { "http", "https", "ftp", "ftps" };
      private Queue queue = new Queue();

      //
      // Mod_Audio constructor
      //
      public Mod_Audio(Bot bot_) :
         base(bot_)
      {
         commands["queue"] = new BotCommandStructure{
            cmd = cmdQueue,
            help = "Add an item (or items) to the audio queue.\n" +
                   "Syntax: .queue uri...\n" +
                   "Example: .queue https://www.youtube.com/watch?v=13pL0TiOiHM"
         };

         commands["play"] = new BotCommandStructure{
            cmd = cmdPlay,
            help = "Set the currently playing item in the queue. " +
                   "If a URL is given, queues and plays that.\n" +
                   "Syntax: .play [number|uri]\n" +
                   "Example: .play 5\n" +
                   "Example: .play https://www.youtube.com/watch?v=13pL0TiOiHM"
         };

         commands["lsqueue"] = new BotCommandStructure{
            cmd = cmdListQueue,
            help = "Lists queue items.\n" +
                   "Syntax: .lsqueue"
         };

         commands["summon"] = new BotCommandStructure{
            cmd = cmdSummon,
            help = "Makes the bot join your audio channel.\n" +
                   "Syntax: .summon"
         };

         commands["vanquish"] = new BotCommandStructure{
            cmd = cmdVanquish,
            help = "Makes the bot leave their audio channel.\n" +
                   "Syntax: %vanquish",
            role = BotRole.HalfAdmin
         };
      }

      //
      // summon
      //
      private async Task<bool> summon(User usr, Channel channel)
      {
         if(bot.isInAudioChannel)
            return true;

         if(!await bot.joinAudioChannel(usr))
         {
            bot.reply(usr, channel,
               "Couldn't find audio channel. " +
               "If you are already in an audio channel, please reconnect to " +
               "it and try again.");
            return false;
         }

         return true;
      }

      //
      // cmdQueue
      //
      public void cmdQueue(User usr, Channel channel, String msg)
      {
         var uris = Utils.URI.Matches(msg);

         if(uris == null)
            throw new CommandArgumentException("no valid URIs");

         int loadPass = 0;
         foreach(var uri_ in uris)
         {
            var uri = uri_;
            if(uri.method == String.Empty)
               uri.method = "http";

            if(validMethods.Contains(uri.method) &&
               queue.addItem(uri))
               loadPass++;
         }

         bot.reply(usr, channel,
            $"Added {loadPass} item{loadPass != 1 ? "s" : ""} to the queue.");
      }

      //
      // cmdPlay
      //
      public void cmdPlay(User usr, Channel channel, String msg)
      {
      }

      //
      // cmdListQueue
      //
      public void cmdListQueue(User usr, Channel channel, String msg)
      {
      }

      //
      // cmdSummon
      //
      public void cmdSummon(User usr, Channel channel, String msg) =>
         summon(usr, channel);

      //
      // cmdVanquish
      //
      public void cmdVanquish(User usr, Channel channel, String msg) =>
         bot.partAudioChannel();
   }
}

