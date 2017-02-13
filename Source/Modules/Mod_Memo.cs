//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Memoing capabilities.
// .memo
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Sharkbite.Irc;
using Newtonsoft.Json;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Memo
   //
   public class Mod_Memo : IBotModule
   {
      //
      // MemoFlags
      //
      [Flags]
      enum MemoFlags
      {
         OnSeen = 1 << 0
      }

      //
      // MemoInfo
      //
      private struct MemoInfo
      {
         public String content;
         public String sender;
         public DateTime time;
         public MemoFlags flags;
      };

      // MemoDict
      private class MemoDict : Dictionary<String, List<MemoInfo>> {}

      MemoDict memos = new MemoDict();

      //
      // Mod_Memo constructor
      //
      public Mod_Memo(Bot bot_) :
         base(bot_)
      {
         if(File.Exists("/srv/irc/vrobot3/data/memos." + bot.n_groupname + ".json"))
            memos = JsonConvert.DeserializeObject<MemoDict>(File.ReadAllText("/srv/irc/vrobot3/data/memos." +
                                                                             bot.n_groupname + ".json"));
         
         commands["memo"] = new BotCommandStructure{ cmd = Cmd_Memo,
            help = "Sends a message to someone later. Activates when they say something. || " +
                   "Syntax: .memo person message || " +
                   "Example: .memo SomeDude wow u suck at videogames"
         };
         
         commands["memoseen"] = new BotCommandStructure{ cmd = Cmd_MemoSeen,
            help = "Sends a message to someone later. Activates when they do anything. || " +
                   "Syntax: .memoseen person message || " +
                   "Example: .memoseen SomeDude wow u suck at videogames"
         };

         commands["memocount"] = new BotCommandStructure{ cmd = Cmd_MemoCount, flags = BotCommandFlags.AdminOnly,
            help = "Gets the amount of memos for this session. || " +
                   "Syntax: @memocount"
         };

         events.OnMessage += Evt_OnMessage;
         events.OnDisconnected += Evt_OnDisconnected;
         events.OnSeen += Evt_OnSeen;

         postSetup();
      }

      //
      // Cmd_MemoCount
      //
      public void Cmd_MemoCount(UserInfo usr, String channel, String msg)
      {
         bot.Reply(usr, channel, memos.Count.ToString());
      }

      //
      // Cmd_Memo
      //
      public void Cmd_Memo(UserInfo usr, String channel, String msg)
      {
         String[] args = Utils.GetArguments(msg, commands["memo"].help, 2, 2, ' ');

         args[0] = args[0].Replace(",", "");

         AddMemo(args[0], new MemoInfo {
            content = args[1],
            sender = usr.Nick,
            time = DateTime.Now
         });

         bot.Reply(usr, channel, String.Format("Message for {0} will be sent next time they say something.", args[0]));
      }

      //
      // Cmd_MemoSeen
      //
      public void Cmd_MemoSeen(UserInfo usr, String channel, String msg)
      {
         String[] args = Utils.GetArguments(msg, commands["memoseen"].help, 2, 2, ' ');

         args[0] = args[0].Replace(",", "");

         AddMemo(args[0], new MemoInfo {
            content = args[1],
            sender = usr.Nick,
            time = DateTime.Now,
            flags = MemoFlags.OnSeen
         });

         bot.Reply(usr, channel, String.Format("Message for {0} will be sent next time I see them.", args[0]));
      }

      //
      // AddMemo
      //
      private void AddMemo(String name, MemoInfo memo)
      {
         name = name.ToLower();

         if(!memos.ContainsKey(name))
            memos[name] = new List<MemoInfo>();

         memos[name].Add(memo);

         WriteMemos();
      }

      //
      // OutputMemos
      //
      private void OutputMemos(String channel, String realnick, bool onseen)
      {
         String nick = realnick.ToLower();

         if(!memos.ContainsKey(nick))
            return;

         var arr = memos[nick];
         for(int i = arr.Count - 1; i >= 0; i--)
         {
            MemoInfo memo = arr[i];

            if(!memo.flags.HasFlag(MemoFlags.OnSeen) && onseen)
               continue;
            
            String outp = String.Empty;

            outp += String.Format("[Memo from {0}, {1}]", memo.sender, Utils.FuzzyRelativeDate(memo.time));

            // Wrap if it's probably going to be too long.
            if(memo.content.Length > 350)
            {
               bot.Message(channel, outp + ":");
               outp = String.Empty;
            }

            outp += String.Format(" {0}: {1}", realnick, memo.content);

            bot.Message(channel, outp);

            arr.RemoveAt(i);
         }

         if(arr.Count == 0)
            memos.Remove(nick);

         WriteMemos();
      }

      //
      // Evt_OnMessage
      //
      public void Evt_OnMessage(UserInfo usr, String channel, String msg, bool iscmd)
      {
         OutputMemos(channel, usr.Nick, false);
      }

      //
      // Evt_OnSeen
      //
      public void Evt_OnSeen(UserInfo usr, String channel)
      {
         OutputMemos(channel, usr.Nick, true);
      }

      //
      // Evt_OnDisconnected
      //
      public void Evt_OnDisconnected()
      {
         WriteMemos();
      }

      //
      // WriteMemos
      //
      private void WriteMemos()
      {
         File.WriteAllText("/srv/irc/vrobot3/data/memos." + bot.n_groupname + ".json",
                           JsonConvert.SerializeObject(memos));
      }
   }
}

