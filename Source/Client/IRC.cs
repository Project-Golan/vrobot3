//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// IRC client.
//
//-----------------------------------------------------------------------------

using System;

namespace ProjectGolan.Vrobot3
{
   //
   // BotClientIRC
   //
   public class BotClientIRC : IBotClient
   {
      //
      // BotClientIRC constructor
      //
      public BotClientIRC(Bot bot) :
         base(bot)
      {
         this.info.hasAudio          = false;
         this.info.hasColors         = true;
         this.info.hasNewlines       = false;
         this.info.messageSafeMaxLen = 601;
         this.info.shortMessages     = true;
      }

      public override void connect() {}
      public override void disconnect() {}
      public override Channel getChannel(ulong id) => new Channel{};
      public override void joinChannel(Channel channel) {}
      public override void partChannel(Channel channel) {}
      public override void sendAction(Channel channel, String msg) {}
      public override void sendMessage(Channel channel, String msg) {}
      public override bool userPermitted(User usr, BotRole role) => true;
   }
}

// EOF
