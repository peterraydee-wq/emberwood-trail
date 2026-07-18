# emberwood trail

a fox cub, torn from its family in a storm, trying to find its way home
through the woods before the first snow falls. an oregon-trail-style
survival game told in ascii — manage your health, hunger, energy, and
warmth across seven zones, each with its own dangers and its own sound.

there are two versions of this game in here, built from the same design:

- a **terminal version** written in c#, playable in any console
- a **browser version** with a generative ambient soundtrack built in tone.js

---

## what's the point

you have a set number of days before winter arrives. every day you choose
one action. the woods don't care whether you make it — they just keep
getting colder.

- reach **the denning grounds** before day 24 and you find your way home
- let health hit zero and the trail ends there
- run out of days first and the first snow buries the path behind you

there's no fighting, no combat system, nothing violent — just the slow
math of resources against distance against time, the same tension that
made the original oregon trail work.

---

## the zones

| zone               | feel                          | main danger |
|--------------------|-------------------------------|-------------|
| meadow's edge      | calm, open, golden grass       | none — a gentle start |
| whispering pines    | dim, close, quiet              | predators |
| the riverbend       | cold water, faint scent trail  | storms |
| old mill ruins      | rusted, metallic, unsettled    | traps |
| owl's hollow        | watched, tense, still          | predators |
| frostfern hollow    | first frost, sparse and sharp  | storms |
| the denning grounds | almost there                   | the clock itself |

each zone takes a few days of travel to clear, and the trail only moves
forward — there's no going back for a better run at it.

---

## your stats

- **health** — if this hits zero, the trip is over
- **hunger** — ignore it too long and it starts costing you health
- **energy** — running on empty makes everything harder
- **warmth** — the deeper into the woods, the faster it drains

## your actions, each day

- **press onward** — advances you through the current zone. costs the
  most, and can trigger random events: predators, storms, traps, or a bit
  of luck.
- **forage** — restores hunger, small chance of a minor injury.
- **rest** — restores energy, and a little health if you were running low.
- **scout** — costs little, and softens whatever danger comes next.
- **check your condition** — free. costs no time at all.

balancing travel against rest and forage is the whole game. sprint
straight through and you'll run yourself into the ground. linger too long
and winter catches you first.

---

## the browser version

built as a single self-contained `.html` file — no install, no build step,
just open it. same game, same zones, same everforest-tinted terminal look,
but each zone now has its own generative ambient music, built live with
[tone.js](https://tonejs.github.io/):

- **meadow's edge** — a bright open drone, a wandering pentatonic pluck,
  the odd high "bird" blip
- **whispering pines** — a detuned minor drone under pink-noise wind,
  sparse and uneasy plucked notes
- **the riverbend** — filtered white noise sweeping like moving water,
  drifting bell tones
- **old mill ruins** — a dissonant, detuned drone, a low brown-noise hum,
  and rare metallic clanks out of nowhere
- **owl's hollow** — barely-there sub-bass, long silences, an occasional
  pitch-bent blip like something watching from the dark
- **frostfern hollow** — thin, cold noise and slow sparkling high bells
- **the denning grounds** — a full warm chord, quicker plucks and bells —
  the arrival, resolving

none of it is pre-recorded. every zone schedules its own randomized notes
and intervals through tone.js's transport, so it never plays back quite
the same way twice. moving between zones fades the old patch out and
brings the new one in.

### running it

open `emberwood-trail.html` in any modern browser. that's it.

browsers block audio until you interact with the page, so there's a
"begin, with sound" button on the title screen — that first click is what
starts the audio engine. there's a volume slider and a mute button in the
top right if you'd rather play it quiet.

controls: click the numbered options, or use keys `1`–`5`.

---

## the terminal version

a plain c# console app. no external dependencies, no nuget packages —
just the .net sdk.

### running it

you'll need the .net sdk (8.0 or newer) installed:

```
sudo pacman -S dotnet-sdk      # arch
winget install Microsoft.DotNet.SDK.8   # windows
```

then, from the project folder:

```
dotnet run
```

or build a standalone binary:

```
dotnet build -c release
./bin/release/net8.0/foxtrail
```

> note: the `.csproj` targets whatever sdk version you actually have
> installed. if `dotnet run` complains about a missing target framework
> (`nu1100`), check `dotnet --list-sdks` and update the
> `<targetframework>` in `foxtrail.csproj` to match.

controls: type the number of your chosen action and press enter.

---

## why ascii

no sprites, no engine, no gpu — just text and a good sense of pacing. the
whole point was to see how much atmosphere a handful of characters and a
resource meter could carry, in a language that made sense on a real
terminal and in a browser tab alike.
