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
   // IBotClient
   //
   public abstract class IBotClient
   {
      protected Bot bot;
      public ServerInfo info;

      public IBotClient(Bot bot) { this.bot = bot; }

      // Connect
      public abstract void connect();
      public abstract void disconnect();

      // Send
      public abstract void sendAction(Channel channel, String msg);
      public abstract void sendMessage(Channel channel, String msg);

      // Channel
      public abstract Channel getChannel(ulong id);
      public virtual void joinChannel(Channel channel) {}
      public virtual void partChannel(Channel channel) {}

      // Audio
      public virtual ChannelAudio getAudioChannel(User user) =>
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
