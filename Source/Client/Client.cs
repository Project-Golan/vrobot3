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

namespace ProjectGolan.Vrobot3.Client
{
   public class ClientInfo
   {
      public bool hasAudio;
      public bool hasColors;
      public bool hasNewlines;
      public int  messageSafeMaxLen;
      public bool shortMessages;
   }

   public abstract class IChatClient
   {
      protected Bot bot;
      public ClientInfo info { get; protected set; }

      protected IChatClient(Bot bot)
      {
         this.info = new ClientInfo();
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
   }
}

// EOF
