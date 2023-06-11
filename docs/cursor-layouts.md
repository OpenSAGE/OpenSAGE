This document describe cursor layouts defined by the field **Directions** in the object **MouseCursor**. Currently that field only used by the scroll cursor. If that value is greater than 0 then the cursor name appended by some direction index. All possible cursor directions shown below.

↖&nbsp;&nbsp;&nbsp;↑&nbsp;&nbsp;&nbsp;↗

←&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;→

↙&nbsp;&nbsp;&nbsp;↓&nbsp;&nbsp;&nbsp;↘

### Layout 1
Original game just hide cursor when Directions == 1. We show cursor with direction index 0.

### Layout 2
1&nbsp;&nbsp;&nbsp;0&nbsp;&nbsp;&nbsp;0

1&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

1&nbsp;&nbsp;&nbsp;0&nbsp;&nbsp;&nbsp;0

### Layout 3
2&nbsp;&nbsp;&nbsp;2&nbsp;&nbsp;&nbsp;0

2&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

1&nbsp;&nbsp;&nbsp;1&nbsp;&nbsp;&nbsp;0

### Layout 4
3&nbsp;&nbsp;&nbsp;3&nbsp;&nbsp;&nbsp;3

2&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

1&nbsp;&nbsp;&nbsp;1&nbsp;&nbsp;&nbsp;1

### Layout 5
3&nbsp;&nbsp;&nbsp;4&nbsp;&nbsp;&nbsp;4

3&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

2&nbsp;&nbsp;&nbsp;1&nbsp;&nbsp;&nbsp;1

### Layout 6
4&nbsp;&nbsp;&nbsp;5&nbsp;&nbsp;&nbsp;5

3&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

2&nbsp;&nbsp;&nbsp;1&nbsp;&nbsp;&nbsp;1

### Layout 7
4&nbsp;&nbsp;&nbsp;5&nbsp;&nbsp;&nbsp;6

4&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

3&nbsp;&nbsp;&nbsp;2&nbsp;&nbsp;&nbsp;1

### Layout 8
5&nbsp;&nbsp;&nbsp;6&nbsp;&nbsp;&nbsp;7

4&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0

3&nbsp;&nbsp;&nbsp;2&nbsp;&nbsp;&nbsp;1

### Other layouts
Directions number greater than 8 seems like a bugged in the original game. We just cap directions number at 8.