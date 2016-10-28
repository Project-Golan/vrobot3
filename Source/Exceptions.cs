//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Exception types.
//
//-----------------------------------------------------------------------------

using System;

namespace ProjectGolan.Vrobot3
{
   //
   // CommandArgumentException
   //
   public class CommandArgumentException : Exception
   {
      public CommandArgumentException() {}
      public CommandArgumentException(String message) : base(message) {}
      public CommandArgumentException(String message, Exception inner) :
         base(message, inner)
      {
      }
   }

   //
   // BotConfigurationException
   //
   public class BotConfigurationException : Exception
   {
      public BotConfigurationException() {}
      public BotConfigurationException(String message) : base(message) {}
      public BotConfigurationException(String message, Exception inner) :
         base(message, inner)
      {
      }
   }
}

// EOF
