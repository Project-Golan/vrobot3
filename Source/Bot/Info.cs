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
   // RoleInfo
   //
   public struct RoleInfo
   {
      public String[] admin;
      public String[] halfadmin;
   }

   //
   // BotInfo
   //
   public struct BotInfo
   {
      public Dictionary<String, String[]> enables;
      public Dictionary<String, String[]> disables;
      public String   serverType;
      public String   serverName;
      public String   serverPass;
      public String   serverAddr;
      public String[] channels;
      public RoleInfo roles;
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
