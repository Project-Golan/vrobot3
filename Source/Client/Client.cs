//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Connection method interface.
//
//-----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace ProjectGolan.Vrobot3
{
   //
   // BotClientInfo
   //
   public class BotClientInfo
   {
      public bool hasAudio;
      public bool hasColors;
      public bool hasNewlines;
      public int  messageSafeMaxLen;
      public bool shortMessages;
   }

   //
   // IBotClient
   //
   public abstract class IBotClient
   {
      protected Bot bot;
      public BotClientInfo info { get; protected set; }

      protected IBotClient(Bot bot)
      {
         this.info = new BotClientInfo();
         this.bot = bot;
      }

      // connect
      public abstract void connect();
      public abstract void disconnect();

      // send
      public abstract void sendAction(Channel channel, String msg);
      public abstract void sendMessage(Channel channel, String msg);
      public virtual void sendMessageRaw(Channel channel, String msg) =>
         sendMessage(channel, msg);

      // channel
      public abstract Channel getChannel(ulong id);
      public virtual void joinChannel(Channel channel) {}
      public virtual void partChannel(Channel channel) {}

      // user
      public abstract bool userPermitted(User usr, BotRole role);

      // audio
      public virtual ChannelAudio getAudioChannel(User usr) =>
         new ChannelAudio();
      public virtual async Task joinAudioChannel(ChannelAudio channel) =>
         await Task.FromResult(0);
      public virtual void partAudioChannel() {}
      public virtual bool isInAudioChannel() => false;
      public virtual async Task playAudioFile(String file) =>
         await Task.FromResult(0);
   }
}

// EOF
