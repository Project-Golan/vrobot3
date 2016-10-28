//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Program entry point.
//
//-----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ProjectGolan.Vrobot3
{
   //
   // Program
   //
   public class Program
   {
      //
      // ProgramInfo
      //
      public struct ProgramInfo
      {
         public String googleKey;
      }

      //
      // JsonConfig
      //
      private struct JsonConfig
      {
         public ProgramInfo info;
         public BotInfo[]   servers;
      }

      //
      // Main
      //
      [STAThread]
      public static void Main(String[] args)
      {
         Instance = new Program();
         Instance.main(args);
      }

      //
      // main
      //
      public void main(String[] args)
      {
         try
         {
            var configFile = File.ReadAllText(dataDir + "/config.json");
            var config = JsonConvert.DeserializeObject<JsonConfig>(configFile);

            info = config.info;

            foreach(var server in config.servers)
               threads.AddItem(new Thread(bots.AddItem(new Bot(server)).connect)).Start();
         }
         catch(Exception exc)
         {
            File.WriteAllText(dataDir + "/excdump.txt", exc.ToString());
            Console.WriteLine("Error: {0}", exc.Message);
         }
      }

      //
      // end
      //
      public void end()
      {
         foreach(var bot in bots)
            try { bot.disconnect(); }
            catch(Exception exc)
            {
               File.WriteAllText(dataDir + "/disconnectexcdump.txt",
                  exc.ToString());
            }

         bots.Clear();
         threads.Clear();
      }

      private readonly List<Bot>    bots    = new List<Bot>();
      private readonly List<Thread> threads = new List<Thread>();
      public  String                dataDir = "../data";
      public  ProgramInfo           info;

      public static Program Instance;
   }
}

// EOF
