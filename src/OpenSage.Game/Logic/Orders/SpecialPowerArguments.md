- the leading integer for these special power arguments seems to correspond to SpecialPowerType
- the second integer argument seems to be some sort of bitflags similar to commandbuttonoption
- last object id could be command center source?

## Integer argument flags
enemy vs neutral needs verification - no special powers in generals only have one of them in isolation
```
1         0000 0000 0000 0001  NEED_TARGET_ENEMY_OBJECT
2         0000 0000 0000 0010  NEED_TARGET_NEUTRAL_OBJECT
4         0000 0000 0000 0100  NEED_TARGET_ALLY_OBJECT
8         0000 0000 0000 1000  ?????
16        0000 0000 0001 0000  ?????
32        0000 0000 0010 0000  NEED_TARGET_POS
64        0000 0000 0100 0000  NEED_UPGRADE
128       0000 0000 1000 0000  NEED_SPECIAL_POWER_SCIENCE
256       0000 0001 0000 0000  OK_FOR_MULTI_SELECT
512       0000 0010 0000 0000  CONTEXTMODE_COMMAND
1024      0000 0100 0000 0000  CHECK_LIKE
2048      0000 1000 0000 0000  ?????
4096      0001 0000 0000 0000  ?????
8192      0010 0000 0000 0000  OPTION_ONE
16384     0100 0000 0000 0000  OPTION_TWO
32768     1000 0000 0000 0000  OPTION_THREE
```

## `SpecialPower`
```
burton detonate demo charge      SpecialPower(Integer:25, Integer:256, ObjectId:0)   // 0000 0001 0000 0000
detention center intelligence    SpecialPower(Integer:34, Integer:0, ObjectId:0)     // 0000 0000 0000 0000
strategy center bombardment      SpecialPower(Integer:41, Integer:9216, ObjectId:0)  // 0010 0100 0000 0000
strategy center hold the line    SpecialPower(Integer:41, Integer:17408, ObjectId:0) // 0100 0100 0000 0000
strategy center search & destroy SpecialPower(Integer:41, Integer:33792, ObjectId:0) // 1000 0100 0000 0000
```

## `SpecialPowerAtLocation`
unclear what first object id is
```
fuel air bomb             SpecialPowerAtLocation(Integer:2, Position:<1217.1951, 1874.8988, 18.75>, ObjectId:0, Integer:672, ObjectId:657)     // 0000 0010 1010 0000
paradrop                  SpecialPowerAtLocation(Integer:3, Position:<1128.1514, 705.649, 18.74997>, ObjectId:0, Integer:672, ObjectId:657)    // 0000 0010 1010 0000
cluster mines             SpecialPowerAtLocation(Integer:5, Position:<1346.773, 2096.985, 18.75>, ObjectId:1243, Integer:672, ObjectId:657)    // 0000 0010 1010 0000
emp bomb                  SpecialPowerAtLocation(Integer:6, Position:<1603.1487, 1894.5223, 18.75>, ObjectId:3661, Integer:672, ObjectId:657)  // 0000 0010 1010 0000
a10                       SpecialPowerAtLocation(Integer:8, Position:<1795.539, 1420.2986, 110>, ObjectId:0, Integer:672, ObjectId:657)        // 0000 0010 1010 0000
nuclear missile           SpecialPowerAtLocation(Integer:10, Position:<668.3643, 241.7229, 10>, ObjectId:5, Integer:672, ObjectId:0)           // 0000 0010 1010 0000
scud storm                SpecialPowerAtLocation(Integer:12, Position:<1432.6141, 1948.8225, 18.75>, ObjectId:0, Integer:544, ObjectId:0)      // 0000 0010 0010 0000
artillery barrage         SpecialPowerAtLocation(Integer:13, Position:<1490.2301, 2075.6533, 18.75>, ObjectId:2794, Integer:672, ObjectId:657) // 0000 0010 1010 0000
spy satellite             SpecialPowerAtLocation(Integer:15, Position:<304.26398, 409.33313, 10>, ObjectId:0, Integer:544, ObjectId:0)         // 0000 0010 0010 0000
spy drone                 SpecialPowerAtLocation(Integer:16, Position:<825.8123, 606.12665, 10>, ObjectId:0, Integer:928, ObjectId:4)          // 0000 0011 1010 0000
radar van scan            SpecialPowerAtLocation(Integer:17, Position:<1508.3314, 1920.3726, 18.75>, ObjectId:0, Integer:864, ObjectId:0)      // 0000 0011 0110 0000
rebel ambush              SpecialPowerAtLocation(Integer:20, Position:<1734.9331, 1322.1748, 110>, ObjectId:0, Integer:672, ObjectId:657)      // 0000 0010 1010 0000
repair                    SpecialPowerAtLocation(Integer:35, Position:<1105.9589, 728.7699, 18.75>, ObjectId:2816, Integer:672, ObjectId:657)  // 0000 0010 1010 0000
particle cannon           SpecialPowerAtLocation(Integer:37, Position:<442.46735, 318.8226, 10>, ObjectId:0, Integer:672, ObjectId:0)          // 0000 0010 1010 0000
ambulance cleanup area    SpecialPowerAtLocation(Integer:42, Position:<443.1347, 219.59857, 10>, ObjectId:0, Integer:288, ObjectId:0)          // 0000 0001 0010 0000
```

## `SpecialPowerAtObject`
first object id seems to be target
```
cash hack                 SpecialPowerAtObject(Integer:14, ObjectId:674, Integer:643, ObjectId:657)  // 0000 0010 1000 0011
missile laser lock        SpecialPowerAtObject(Integer:23, ObjectId:700, Integer:771, ObjectId:0)    // 0000 0011 0000 0011
tank hunter tnt           SpecialPowerAtObject(Integer:24, ObjectId:6, Integer:259, ObjectId:0)      // 0000 0001 0000 0011
burton remote demo charge SpecialPowerAtObject(Integer:25, ObjectId:2, Integer:771, ObjectId:0)      // 0000 0011 0000 0011
burton timed demo charge  SpecialPowerAtObject(Integer:26, ObjectId:3, Integer:259, ObjectId:0)      // 0000 0001 0000 0011
hacker disable building   SpecialPowerAtObject(Integer:27, ObjectId:8, Integer:259, ObjectId:0)      // 0000 0001 0000 0011
lotus capture building    SpecialPowerAtObject(Integer:28, ObjectId:8, Integer:259, ObjectId:0)      // 0000 0001 0000 0011
usa ranger capture        SpecialPowerAtObject(Integer:29, ObjectId:8, Integer:323, ObjectId:0)      // 0000 0001 0100 0011
china red guard capture   SpecialPowerAtObject(Integer:30, ObjectId:8, Integer:323, ObjectId:0)      // 0000 0001 0100 0011
gla rebel capture         SpecialPowerAtObject(Integer:31, ObjectId:8, Integer:323, ObjectId:0)      // 0000 0001 0100 0011
lotus disable vehicle     SpecialPowerAtObject(Integer:32, ObjectId:6, Integer:259, ObjectId:0)      // 0000 0001 0000 0011
lotus cash hack           SpecialPowerAtObject(Integer:33, ObjectId:674, Integer:259, ObjectId:0)    // 0000 0001 0000 0011
bomb truck disguise       SpecialPowerAtObject(Integer:36, ObjectId:1176, Integer:263, ObjectId:0)   // 0000 0001 0000 0111
```
