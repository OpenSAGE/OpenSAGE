Assume the following terminology:

- `player`: the player attempting to change the position properties
- `AI`: any entrant in the lobby which is not a human
- `competitor`: any entrant in the lobby, human or `AI`
- `slot`: the map position button in question

- If `slot` is empty
  - If `player` is the only competitor in the lobby _or_ if `player` is not the host
    - `player`'s index should be **moved** from the previous slot (if applicable) to the newly-selected `slot`
- If `player` is the host
  - `slot` should become occupied with the numerically-next slot belonging to either `player` or `AI` 
  - If one cannot be found after reaching the max player index, **clear** the slot

#### Assuming `player` is not the host

- If the slot is occupied by `player`
  - **clear** the slot

If none of these apply, do **nothing**

