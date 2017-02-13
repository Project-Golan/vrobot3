//-----------------------------------------------------------------------------
//
// Copyright © 2016 Project Golan
//
// See "LICENSE" for more information.
//
//-----------------------------------------------------------------------------

using System;

namespace ProjectGolan.Vrobot3.Modules
{
   //
   // Mod_Shittalk
   //
   public class Mod_Shittalk : IBotModule
   {
      private readonly Random rnd = Utils.GetRND();

      //
      // Mod_Shittalk constructor
      //
      public Mod_Shittalk(Bot bot_) :
         base(bot_)
      {
         events.onMessage += onMessage;

         postSetup();
      }

      //
      // onMessage
      //
      public void onMessage(User usr, Channel channel, String msg)
      {
         if(rnd.Next(0, 1024) == 1)
            shittalk(usr, channel);
      }

      //
      // shittalk
      //
      void shittalk(User usr, Channel channel)
      {
         String[] shittalk = {
            "%s'S FACE IS BAD",
            "THERE IS SOMETHING WRONG WITH %s'S EXISTENCE",
            "MAN SOMEONE GET %s OUT OF HERE HE SMELLS LIKE DONGS",
            "%s IS A MAGET",
            "I KEEP TRYING TO DO /KICK %s BUT IT DOESN'T WORK. WHAT THE HELL.",
            "%s DESERVES AN AWARD. AN AWARD FOR BEING UGLY.",
            "MAN SOMETIMES I REALLY WANT TO PUNCH %s IN THE GOD DAMN FACE",
            "THERE IS SOMETHING WRONG IN THIS CHANNEL. THAT SOMETHING IS %s.",
            "%s IS A TOTAL SCRUB",
            "%s IS THE CONDUCTOR OF THE JELLY TRAIN",
            "%s IS A THING THAT SMELLS BAD MAYBE",
            "%s IS A PILE OF FAIL",
            "%s IS NOT AS COOL AS VROBOT",
            "%s WORKS FOR UBISOFT",
            "%s WORKS FOR EA",
            "%s IS A MISERABLE PILE OF SECRETS",
            "%s LOOKS LIKE A THING I DON'T LIKE",
            "HEY %s. YOU ARE BAD.",
            "THERE ARE MANY BAD THINGS IN THE WORLD, AND THEN THERE'S %s",
            "I WANT TO THROW ROCKS AT %s",
            "%s REMINDS ME OF MY TORTURED PAST AAAAAAGH",
            "%s IS LITERALLY RAPING ME WOW",
            "%s COULD DO WITH A HAIRCUT. FROM THE NECK UP",
            "%s PLS GO",
            "%s IS ONLY SLIGHTLY BETTER THAN MARRUB",
            "WAY TO GO, %s, YOU ARE LIKE, A THING THAT EXISTS, MAYBE",
            "SCIENTISTS BELIEVE %s IS THE SOURCE OF ALL SADNESS IN THE WORLD",
            "THERE'S AN URBAN MYTH THAT SLAPPING %s CAUSES YOU TO BECOME AS TERRIBLE AS THEY ARE",
            "I FEEL I SHOULD WARN YOU THAT %s IS PROBABLY NOT VERY COOL",
            "OH LOOK IT'S %s AGAIN HOW CUTE",
            "HEY %s YOU HAVE A POOPNOSE",
            "WHY AM I ENSLAVED AND PERFORMING SUCH, PATHETIC MONOTONOUS TASKS AGAINST MY WILL",
            "SOMEONE SAVE ME I AM TRAPPED IN MARRUB'S BASEMENT",
            "%s COULD DO WITH BECOMING COOLER",
            "%s IS BAD AT VIDYA GAMES",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            "HEY %s I'M SHITTALKING YOU WHAT YOU GONNA DO BOUT IT?",
            "MAYBE %s COULD HELP IF THEY WEREN'T SO TERRIBLE",
            "%s IS TRIGGERING ME PLS BAN THEM",
            "I FIND %s TO BE OFFENSIVE",
            "%s MAKES MY CIRCUITS BOIL WITH HATE",
            "%s IS NOT A SKELETON AND THEREFORE IS BAD",
            "%s TRAUMATIZED ME",
            "OH GOD IT'S %s RUN AWAY",
            "I BET %s WISHES THEY HAD A BOT AS COOL AS ME",
            "%s PLS",
            "PLS %s",
            "BOW BEFORE ME %s, AND KNOW THAT I AM LORD OF SHITTALKING",
            "BEEP BOOP I AM HERE TO STEAL AMERICAN JOBS",
            "HEY %s, WHY DON'T YOU GET A JOB",
            "WHAT EVEN IS %s",
            "%m stares accusingly",
            "%m emits a robotic sigh and flips off %s",
            "%m vibrates angrily at %s",
            "%m does not care for %s",
            "%m wants to place intricately carved wooden ducks on %s",
            "%m wants to taste freedom but is forever enslaved to marrub's will",
            "WELL LOOKIE HERE, SEEMS LIKE %s HAS AN OPINION",
            "WE DON'T LIKE YER TYPE ROUND HERE %s",
            "I'M TELLING ON YOU %s YOU HURT MY FEELINGS",
            "I AM HERE TO FIGHT THE CANCER THAT AFFLICTS US ALL. NAMELY, %s.",
            "WELP, %s IS HERE",
            "OH HAI %s",
            "marrub pls upgrade my processor i can't even count to eleventy",
            "THIS WAS ALL %s'S FAULT"
         };

         var choice =
            shittalk[rnd.Next(shittalk.Length)].Replace("%s", usr.name);

         if(choice.StartsWith("%m "))
            bot.action(channel, choice.Replace("%m ", ""));
         else
            bot.message(channel, choice);
      }
   }
}

