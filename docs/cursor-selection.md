

## Overview

Cursor selection in Zero Hour is as follows, with the following terminology:

- `object`: The object currently selected by the player
- `structure`: The object currently selected by the player, inferred to be a structure
- `unit`: The object currently selected by the player, inferred to be a unit
- `target`: The target object the player is hovering over
- `terrain`: The (lack of a) target object the player is hovering over, inferred to be terrain

This is written under the assumption that no special ability is selected, which would always take precedence over this tree.

#### If `object` is not owned by the player...

**Always** show the **Select** cursor

## No modifier keys

#### `object` is player-owned

- If `object` is a `structure`
  - If `target` is _any game object_
    - If `structure` has a weapon (Strategy Center + Bombardment), and `target` is enemy-owned
      - If `structure` can attack target
        - Show the **AttackObj** cursor
      - Show the **GenericInvalid** cursor
    - Show the **Select** cursor
  - If `structure` has a rally point ability
    - Show the **SetRallyPoint** cursor
  - Show the **GenericInvalid** cursor

#### `object` is a player-owned `unit`

- If `target` is terrain (i.e. null)
  - If `terrain` is hidden under the fog of war _or_ if `terrain` is passable (not marked impassable, not water)
    - Show the **Move** cursor (note - no exception for disabled battlebuses)
  - In Generals: Show the **GenericInvalid** cursor
  - In Zero Hour:
    - If any `unit` has cliff locomotor
      - Show the **Move** cursor (there is a caveat here where GenericInvalid is always shown at the _edge_ of impassable terrain, but I'd speculate that's a bug) 
    - Show the **GenericInvalid** cursor

#### `target` is a game object

- If `target` is an enemy _and_ `unit` is not a dozer
  - If `unit` is `Black Lotus` and `target` is vehicle or supply stash
    - Show the **Hack** cursor
  - If `unit` is capable of attacking `target` _or_ any of `unit`'s garrisons (e.g. ranger in humvee) are capable of attacking target
    - Show the **AttackObj** cursor
  - Show the **GenericInvalid** cursor

#### `target` is not enemy

- If `target` is neutral _and_ `target` is capturable _and_ `unit` has capture special power
  - Show the **CaptureBuilding** cursor
- If `target` supports garrisons _and_ `target` is garrisonable by `unit`
  - If `target` is not full (e.g. barracks/airfield)
    - Show the **EnterFriendly** cursor 
- If `target` is non-enemy structure 
  - If `unit` is `dozer` 
    - If `target` is incomplete player structure _and_ `target` is not currently being built by another dozer
      - Show the **ResumeConstruction** cursor
    - If `target` needs healing
      - Show the **GetRepaired** cursor
  - If `unit` is damaged _and_ `target` can heal `unit`
    - If `unit` is infantry _and_ `target` is player-owned _and_ not full barracks
      - Show the **EnterFriendly** cursor
    - If `unit` is vehicle _and_ `target` is friendly
      - Show the **EnterFriendly** cursor
    - If `unit` is aircraft _and_ `unit` is airborne
      - If `unit` is plane _and_ `target` has open hangars
        - Show the **EnterFriendly** cursor
      - If `unit` is helicopter
        - Show the **EnterFriendly** cursor
    - If `target` is not full (e.g. barracks) _or_ heal doesn't require garrison (e.g. airfield?)
      - Show the **EnterFriendly** cursor
    - If `unit` is `supplyCarrier`
      - if `unit` has supplies _and_ `target` accepts supplies
        - Show the **EnterFriendly** cursor
      - If `target`  contains supplies _and_ `unit` is not full of supplies
        - Show the **EnterFriendly** cursor

- Show the **Select** cursor

## Modifier keys

This section assumes `object` is player-owned. If not, the standard cursor selection rules apply (basically, just show the **Select** cursor)

### Alt

- Show the **Waypoint** cursor (yes, all the time, even if it's a building)

### Ctrl

- If `target` is self
  - Show the **Select** cursor
- If `target` is in `unit` group
  - Process rules for all other units in `unit` group
- If `target` is airborne
  - _From testing, the real rules here are complex and obtuse, with many exceptions with units like stealth bombers, comanches, or ambulances selected. It seems best to just ignore them._
  - If any of `object` (or garrisons) can attack airborne targets
    - Show the **AttackObj** cursor
  - Show the **GenericInvalid** cursor
- Show the **AttackObj** cursor

#### `Target` is (on) the ground

- If `object` (or garrisons) can attack ground targets _and_  [`object` is _not_ a structure _or_ `target` is in range] (or potentially, just doesn't have a locomotor? As per above, does not apply to disabled battlebus)
  - Show the **AttackObj** cursor
- Show the **GenericInvalid** cursor