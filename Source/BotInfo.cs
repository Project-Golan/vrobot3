//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Bot informational classes.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ProjectGolan.Vrobot3
{
   //
   // ServerType
   //
   public enum ServerType
   {
      IRC,
      Discord
   }

   //
   // ServerInfo
   //
   public struct ServerInfo
   {
      public bool hasAudio;
      public bool hasColors;
      public bool hasNewlines;
      public int  messageSafeMaxLen;
      public bool shortMessages;
   }

   //
   // BotInfo
   //
   public struct BotInfo
   {
      public Dictionary<String, String[]> enables;
      public ServerType serverType;
      public String     serverName;
      public String     serverPass;
      public String     serverAddr;
      public String     adminId;
      public String[]   channels;
   }

   //
   // User
   //
   public struct User
   {
      public String hostname; // A consistent identifier for the user.
      public String name;     // Nickname for replying and etc.
   }

   //
   // Channel
   //
   public struct Channel
   {
      public ulong  id;
      public String name;
   }

   //
   // ChannelAudio
   //
   public class ChannelAudio
   {
      public ulong  id;
      public String name;
   }
}

// EOF
