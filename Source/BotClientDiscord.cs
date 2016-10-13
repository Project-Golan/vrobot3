//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------
//
// Discord client.
//
//-----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord.Audio;

namespace ProjectGolan.Vrobot3
{
   //
   // BotClientDiscord
   //
   public class BotClientDiscord : IBotClient
   {
      //
      // AudioBuffer
      //
      class AudioBuffer
      {
//       private static const int MaxSize = 20 * 1024 * 1024;
         private readonly String name;
         private ulong num     = 0;
         private ulong next    = 1;
         private ulong bufSize = 0;
         public bool completed { get; private set; } = false;

         public AudioBuffer()
         {
            name = System.IO.Path.GetRandomFileName() + ".";
         }
      }

      private Discord.DiscordClient client = new Discord.DiscordClient();
      private Discord.Audio.IAudioClient audioClient;
      private Discord.Server server;

      //
      // BotClientDiscord constructor
      //
      public BotClientDiscord(Bot bot) :
         base(bot)
      {
         info.hasAudio          = true;
         info.hasColors         = false;
         info.hasNewlines       = true;
         info.messageSafeMaxLen = 1777;
         info.shortMessages     = false;

         client.MessageReceived += (sender, evt) =>
         {
            if(!evt.Message.IsAuthor && !evt.User.IsBot &&
               (bot.info.channels == null ||
               bot.info.channels.Contains("#" + evt.Channel.Name)) &&
               evt.Server.Id.ToString() == bot.info.serverAddr)
            {
               var usr     = new User{};
               var channel = new Channel{};

               usr.hostname = evt.User.Id.ToString();
               usr.name     = evt.User.Nickname ?? evt.User.Name;

               channel.id   = evt.Channel.Id;
               channel.name = "#" + evt.Channel.Name;

               bot.onMessage(usr, channel, evt.Message.Text);
            }
         };

         client.UsingAudio(x => x.Mode = AudioMode.Outgoing);
      }

      //
      // getUser
      //
      private Discord.User getUser(User usr)
      {
         if(server == null)
            server = client.GetServer(ulong.Parse(bot.info.serverAddr));
         return server.GetUser(ulong.Parse(usr.hostname));
      }

      //
      // connect
      //
      public override void connect()
      {
         Console.WriteLine("{0}: Creating connection.", bot.info.serverName);
         client.ExecuteAndWait(async () =>
         {
            await client.Connect(bot.info.serverPass, Discord.TokenType.Bot);
            client.SetGame("vrobot 3.1 series");
         });
      }

      //
      // disconnect
      //
      public override void disconnect()
      {
         if(client != null)
         {
            partAudioChannel();
            client.Disconnect();
            client = null;
         }
      }

      //
      // sendAction
      //
      public override void sendAction(Channel channel, String msg) =>
         client.GetChannel(channel.id)?.SendMessage(
            "_" + Discord.Format.Escape(msg) + "_");

      //
      // sendMessage
      //
      public override void sendMessage(Channel channel, String msg) =>
         client.GetChannel(channel.id)?.SendMessage(Discord.Format.Escape(msg));

      //
      // getChannel
      //
      public override Channel getChannel(ulong id)
      {
         var dchannel = client.GetChannel(id);
         var channel  = new Channel{};
         channel.id   = dchannel.Id;
         channel.name = "#" + dchannel.Name;
         return channel;
      }

      //
      // getAudioChannel
      //
      public override ChannelAudio getAudioChannel(User usr)
      {
         var dchannel = getUser(usr).VoiceChannel;
         if(dchannel == null) return null;
         var channel  = new ChannelAudio{};
         channel.id   = dchannel.Id;
         channel.name = dchannel.Name;
         return channel;
      }

      //
      // joinAudioChannel
      //
      public override async Task joinAudioChannel(ChannelAudio channel)
      {
         if(channel == null)
            return;

         var dchannel = client.GetChannel(channel.id);
         if(!isInAudioChannel())
            audioClient = await dchannel.JoinAudio();
         else
            await audioClient.Join(dchannel);
      }

      //
      // partAudioChannel
      //
      public override void partAudioChannel()
      {
         if(isInAudioChannel())
         {
            audioClient.Clear();
            audioClient.Wait();
            audioClient.Disconnect();
         }
         audioClient = null;
      }

      //
      // isInAudioChannel
      //
      public override bool isInAudioChannel() =>
         audioClient?.State == Discord.ConnectionState.Connected;

      //
      // playAudioFile
      //
      public override async Task playAudioFile(String file)
      {
         if(!isInAudioChannel()) return;

         var proc = Process.Start(new ProcessStartInfo{
            FileName = "ffmpeg",
            Arguments = $"-i {file} -f s16le -ar 48000 -ac 2 " +
                         "-loglevel quiet pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true
         });

         var buf = new byte[3840 * 32];
         var ostream = audioClient.OutputStream;
         var istream = proc.StandardOutput.BaseStream;

         int count;
         try
         {
            while(!proc.HasExited &&
               (count = await istream.ReadAsync(buf, 0, buf.Length)) != 0)
            {
               Thread.Sleep(8);
               await ostream.WriteAsync(buf, 0, count);
            }
         }
         catch(OperationCanceledException)
         {
            Console.WriteLine("{0}: Canceled audio stream.",
               bot.info.serverName);
         }
         finally
         {
            istream.Dispose();
            ostream.Dispose();
         }
      }
   }
}

// EOF
