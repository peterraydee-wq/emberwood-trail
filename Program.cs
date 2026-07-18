using System;
using System.Collections.Generic;
using System.Threading;

// Entry point — top-level statement kicks off the game loop.
Game.Run();

// ============================================================
//  EMBERWOOD TRAIL — a fox's journey home, Oregon-Trail style
// ============================================================

class Zone
{
    public string Name;
    public string Art;
    public string Intro;
    public int Length;      // how many "Press Onward" actions clear this zone
    public int Cold;        // 0-3, how harshly it drains Warmth
    public string DangerBias; // which random event is more likely here

    public Zone(string name, string art, string intro, int length, int cold, string dangerBias)
    {
        Name = name;
        Art = art;
        Intro = intro;
        Length = length;
        Cold = cold;
        DangerBias = dangerBias;
    }
}

static class Game
{
    static Random rng = new Random();

    // --- player state ---
    static int day = 1;
    static int maxDays = 24;
    static double health = 100;
    static double hunger = 100;
    static double energy = 100;
    static double warmth = 100;
    static int zoneIndex = 0;
    static int progress = 0;
    static bool scouted = false;

    static List<Zone> zones = new List<Zone>();

    public static void Run()
    {
        try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* not all terminals allow this */ }
        BuildZones();
        SafeClear();
        TitleScreen();

        bool gameOver = false;
        string ending = "";

        ZoneTransition(firstEntry: true);

        while (!gameOver)
        {
            SafeClear();
            DrawHeader();
            Console.WriteLine();
            PrintSlow(zones[zoneIndex].Art, 0);
            Console.WriteLine();
            DrawBars();
            Console.WriteLine();
            Console.WriteLine("  Day " + day + " of " + maxDays + " — before the first snow falls.");
            Console.WriteLine();
            Console.WriteLine("  What do you do?");
            Console.WriteLine("   [1] Press onward along the trail");
            Console.WriteLine("   [2] Forage for food");
            Console.WriteLine("   [3] Rest a while");
            Console.WriteLine("   [4] Scout ahead / test the wind");
            Console.WriteLine("   [5] Check your condition");
            Console.WriteLine();
            Console.Write("  > ");

            ConsoleKeyInfo key = ReadValidKey(new[] { '1', '2', '3', '4', '5' });
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1': DoTravel(); break;
                case '2': DoForage(); break;
                case '3': DoRest(); break;
                case '4': DoScout(); break;
                case '5': ShowStatus(); continue; // free action, no day cost
            }

            ApplyPassiveDecay();
            ClampAll();

            string endCheck = CheckEndConditions();
            if (endCheck != "")
            {
                gameOver = true;
                ending = endCheck;
                break;
            }

            day++;
            Pause();
        }

        ShowEnding(ending);
    }

    // ---------------- setup ----------------

    static void BuildZones()
    {
        zones.Add(new Zone(
            "Meadow's Edge",
            @"
        .       .        .
            .        ,-\/\_/\-.        .
       .          ( o.o  )          .
        .          > ^ <   .
              _.--''''''--._
",
            "The storm that swept you from your mother's side has passed.\n" +
            "  Ahead, the meadow grass is tall and gold. Somewhere beyond the\n" +
            "  tree line — home. You just have to find the way back.",
            2, 0, "calm"));

        zones.Add(new Zone(
            "Whispering Pines",
            @"
          /\      /\      /\
         /  \    /  \    /  \
        /____\  /____\  /____\
              |  o.o  |
              |  ^  <-- you
",
            "The pines close in overhead, and the light turns green and dim.\n" +
            "  Every snapped twig sounds like a warning.",
            3, 1, "predator"));

        zones.Add(new Zone(
            "The Riverbend",
            @"
      ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        ~~~   ,-\/\_/\-.    ~~~
      ~~~    ( o.o  )      ~~~
        ~~~~~~ > ^ < ~~~~~~~~~~
",
            "The river runs high and cold. The scent trail is faint here,\n" +
            "  half-washed away by the current.",
            3, 1, "storm"));

        zones.Add(new Zone(
            "Old Mill Ruins",
            @"
             ___
            /___\      /\_/\
           |  |  |    ( o.o )
           |__|__|     > ^ <
           #  #  #  ___||___
",
            "Broken beams and rusted metal. Something about this place\n" +
            "  smells of iron and old smoke. Best tread carefully.",
            2, 1, "trap"));

        zones.Add(new Zone(
            "Owl's Hollow",
            @"
              ,   ,
             (\,,/)
            ( o  o )
             )(''')(
            ^^^     ^^^      /\_/\
                            ( o.o )
                             > ^ <
",
            "Something with wide golden eyes watches from the high\n" +
            "  branches. It hasn't decided yet whether you're worth the effort.",
            3, 2, "predator"));

        zones.Add(new Zone(
            "Frostfern Hollow",
            @"
        *  .  *   .  *  .   *  .
      .   ,-\/\_/\-.    *   .   *
         ( o.o  )   .  *  .
      *   > ^ < .    *   .   *
        .   *  .   *   .  *
",
            "The first frost has already found this place. Your breath\n" +
            "  fogs in the air, and the cold sits deep in your paws.",
            3, 3, "storm"));

        zones.Add(new Zone(
            "The Denning Grounds",
            @"
              home is close now.
                    /\_/\
                   ( o.o )
                    > ^ <
",
            "The air changes. Familiar. Something in your chest aches\n" +
            "  and lifts at the same time. This is it — the last stretch.",
            1, 3, "calm"));
    }

    // ---------------- actions ----------------

    static void DoTravel()
    {
        Console.WriteLine("  You push onward through " + zones[zoneIndex].Name + "...");
        Thread.Sleep(400);

        hunger -= 12;
        energy -= 18;
        warmth -= zones[zoneIndex].Cold * 3 + 2;

        RollEvent();

        progress++;

        if (progress >= zones[zoneIndex].Length)
        {
            if (zoneIndex == zones.Count - 1)
            {
                // reaching the end is handled by CheckEndConditions
            }
            else
            {
                zoneIndex++;
                progress = 0;
                ZoneTransition(firstEntry: false);
            }
        }

        scouted = false;
    }

    static void DoForage()
    {
        Console.WriteLine("  You nose through the underbrush, hunting for something to eat...");
        Thread.Sleep(400);

        energy -= 8;
        warmth -= zones[zoneIndex].Cold;

        int roll = rng.Next(100);
        if (roll < 10)
        {
            Console.WriteLine("  A thorn bush catches your paw. That stings.");
            health -= 5;
        }
        else if (roll < 25)
        {
            Console.WriteLine("  Nothing but dirt and old leaves today.");
        }
        else
        {
            int gain = rng.Next(15, 31);
            hunger += gain;
            Console.WriteLine("  You find a few beetles and half a berry patch. Not bad.");
        }
    }

    static void DoRest()
    {
        Console.WriteLine("  You curl up, nose tucked under your tail, and rest...");
        Thread.Sleep(400);

        int gain = rng.Next(20, 36);
        energy += gain;
        hunger -= 6;
        warmth -= Math.Max(0, zones[zoneIndex].Cold - 1);

        if (energy < 40)
        {
            health += rng.Next(0, 6);
        }
    }

    static void DoScout()
    {
        Console.WriteLine("  You raise your nose to the wind and listen...");
        Thread.Sleep(400);

        energy -= 5;
        hunger -= 4;
        scouted = true;

        Console.WriteLine("  You get a better sense of what lies ahead in " + zones[zoneIndex].Name + ".");
        if (rng.Next(100) < 30)
        {
            Console.WriteLine("  You spot a safer path through the undergrowth. That should help.");
        }
    }

    // ---------------- random events on travel ----------------

    static void RollEvent()
    {
        string bias = zones[zoneIndex].DangerBias;
        int roll = rng.Next(100);

        // scouting recently softens bad outcomes
        int dangerReduction = scouted ? 15 : 0;

        if (roll < 40)
        {
            Console.WriteLine("  The trail is quiet today. Just wind through the branches.");
            return;
        }

        int eventRoll = rng.Next(100) + dangerReduction;

        if (bias == "predator" && eventRoll < 55)
        {
            if (scouted && rng.Next(100) < 50)
            {
                Console.WriteLine("  You hear wings overhead but you already knew to keep to the shadows.");
                return;
            }
            int dmg = rng.Next(10, 21);
            health -= dmg;
            Console.WriteLine("  Something swoops low out of the trees! You bolt, heart hammering.");
        }
        else if (bias == "storm" && eventRoll < 55)
        {
            int loss = rng.Next(15, 26);
            warmth -= loss;
            Console.WriteLine("  Cold rain sweeps through, soaking your fur through to the skin.");
        }
        else if (bias == "trap" && eventRoll < 45)
        {
            if (scouted)
            {
                Console.WriteLine("  You spot the rusted snare before it's too late and skirt around it.");
            }
            else
            {
                int dmg = rng.Next(15, 31);
                health -= dmg;
                Console.WriteLine("  Your leg catches on something cold and metal. You wrench free, hurt.");
            }
        }
        else if (roll > 85)
        {
            int gain = rng.Next(10, 21);
            warmth += gain;
            Console.WriteLine("  You duck into a hollow log, dry and out of the wind for a moment.");
        }
        else
        {
            Console.WriteLine("  A long, uneventful stretch of trail. Small mercies.");
        }
    }

    // ---------------- decay / end conditions ----------------

    static void ApplyPassiveDecay()
    {
        if (hunger <= 0) { health -= 8; Console.WriteLine("  Your stomach is empty. This is starting to hurt."); }
        if (warmth <= 0) { health -= 8; Console.WriteLine("  The cold has crept all the way into your bones."); }
        if (energy <= 0) { health -= 5; Console.WriteLine("  You are running on nothing but instinct."); }
    }

    static void ClampAll()
    {
        health = Math.Clamp(health, 0, 100);
        hunger = Math.Clamp(hunger, 0, 100);
        energy = Math.Clamp(energy, 0, 100);
        warmth = Math.Clamp(warmth, 0, 100);
    }

    static string CheckEndConditions()
    {
        if (health <= 0) return "death";
        if (zoneIndex == zones.Count - 1 && progress >= zones[zoneIndex].Length) return "win";
        if (day >= maxDays && !(zoneIndex == zones.Count - 1 && progress >= zones[zoneIndex].Length)) return "snow";
        return "";
    }

    // ---------------- display ----------------

    static void TitleScreen()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        PrintSlow(@"
   ______           __                                     __
  / ____/___ ___  / /_  ___  ______      ______  ____  ____/ /
 / __/ / __ `__ \/ __ \/ _ \/ ___/ | /| / / __ \/ __ \/ __  /
/ /___/ / / / / / /_/ /  __/ /   | |/ |/ / /_/ / /_/ / /_/ /
/_____/_/ /_/ /_/_.___/\___/_/    |__/|__/\____/\____/\__,_/

               T R A I L
", 4);
        Console.ResetColor();
        Console.WriteLine();
        PrintSlow("  You are a young fox, torn from your family in last night's storm.", 12);
        PrintSlow("  Somewhere through the woods is home. Winter is close behind you.", 12);
        Console.WriteLine();
        Console.WriteLine("  Press any key to begin...");
        Console.ReadKey(true);
    }

    static void ZoneTransition(bool firstEntry)
    {
        SafeClear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("  == " + zones[zoneIndex].Name + " ==");
        Console.ResetColor();
        Console.WriteLine();
        PrintSlow(zones[zoneIndex].Intro, 10);
        Console.WriteLine();
        Console.WriteLine("  Press any key to continue...");
        Console.ReadKey(true);
    }

    static void DrawHeader()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("  ------------------------------------------------------------");
        Console.WriteLine("   " + zones[zoneIndex].Name.ToUpper() + "   (" + progress + "/" + zones[zoneIndex].Length + " through this stretch)");
        Console.WriteLine("  ------------------------------------------------------------");
        Console.ResetColor();
    }

    static void DrawBars()
    {
        PrintBar("Health", health, ConsoleColor.Red);
        PrintBar("Hunger", hunger, ConsoleColor.Yellow);
        PrintBar("Energy", energy, ConsoleColor.Cyan);
        PrintBar("Warmth", warmth, ConsoleColor.Magenta);
    }

    static void PrintBar(string label, double value, ConsoleColor color)
    {
        int filled = (int)Math.Round(value / 5.0); // out of 20 blocks
        filled = Math.Clamp(filled, 0, 20);
        string bar = new string('█', filled) + new string('░', 20 - filled);

        Console.Write("  " + label.PadRight(7) + " [");
        Console.ForegroundColor = color;
        Console.Write(bar);
        Console.ResetColor();
        Console.WriteLine("] " + (int)value + "/100");
    }

    static void ShowStatus()
    {
        SafeClear();
        DrawHeader();
        Console.WriteLine();
        DrawBars();
        Console.WriteLine();
        Console.WriteLine("  Day " + day + " of " + maxDays + ".");
        Console.WriteLine();
        Console.WriteLine("  Press any key to return...");
        Console.ReadKey(true);
    }

    static void ShowEnding(string ending)
    {
        SafeClear();
        if (ending == "win")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            PrintSlow(@"
            /\_/\        /\_/\
           ( o.o )      ( -.- )
            > ^ <        > ^ <     welcome home.
              \\        //
               \\      //
                \\____//
", 6);
            Console.ResetColor();
            Console.WriteLine();
            PrintSlow("  Through the last stand of trees, a familiar shape rises to meet you.\n" +
                      "  Your mother's scent. Your littermates, tumbling over each other.\n" +
                      "  You made it home before the snow.", 15);
            Console.WriteLine();
            Console.WriteLine("  *** YOU FOUND YOUR WAY HOME ***");
        }
        else if (ending == "death")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            PrintSlow(@"
                 /\_/\
                ( x.x )
                 > . <
", 6);
            Console.ResetColor();
            Console.WriteLine();
            PrintSlow("  The woods go quiet around you. You don't get up again.", 15);
            Console.WriteLine();
            Console.WriteLine("  *** THE TRAIL ENDS HERE ***");
        }
        else // snow
        {
            Console.ForegroundColor = ConsoleColor.White;
            PrintSlow(@"
        *  .  *  .  *  .  *  .  *
      .    /\_/\           .   *
    *     ( o.o )    .   *    .
      .    > ^ <   *    .   *
        *  .   *  .  *   .  *
", 6);
            Console.ResetColor();
            Console.WriteLine();
            PrintSlow("  The first snow falls, soft and endless, and the trail\n" +
                      "  disappears beneath it. You'll have to wait out the winter\n" +
                      "  wherever you've ended up, and try again when it thaws.", 15);
            Console.WriteLine();
            Console.WriteLine("  *** WINTER CAUGHT YOU ***");
        }

        Console.WriteLine();
        Console.WriteLine("  Days survived: " + day);
        Console.WriteLine("  Reached: " + zones[zoneIndex].Name);
        Console.WriteLine();
        Console.WriteLine("  Press any key to exit...");
        Console.ReadKey(true);
    }

    // ---------------- small helpers ----------------

    static void PrintSlow(string text, int delayMs)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            if (delayMs > 0) Thread.Sleep(delayMs);
        }
        Console.WriteLine();
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("  Press any key to continue...");
        Console.ReadKey(true);
    }

    static ConsoleKeyInfo ReadValidKey(char[] validChars)
    {
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            foreach (char c in validChars)
            {
                if (key.KeyChar == c) return key;
            }
        }
    }

    static void SafeClear()
    {
        try { Console.Clear(); } catch { /* redirected output, ignore */ }
    }
}
