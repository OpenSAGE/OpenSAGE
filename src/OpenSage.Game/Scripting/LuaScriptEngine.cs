﻿using System;
using System.IO;
using MoonSharp.Interpreter;
using OpenSage.Data.Ini;
using OpenSage.Scripting.Lua;

namespace OpenSage.Scripting
{
    public sealed class LuaScriptEngine : GameSystem
    {
        public MoonSharp.Interpreter.Script MainScript;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LuaScriptEngine(Game game) : base(game)
        {
            MoonSharp.Interpreter.Script.DefaultOptions.DebugPrint = text =>
            {
                Diagnostics.LuaScriptConsole._scriptConsoleTextAll =
                string.Concat(Diagnostics.LuaScriptConsole._scriptConsoleTextAll, text, "\n");
            };

            MainScript = new MoonSharp.Interpreter.Script();

            FunctionInit();

            try
            {
                LuaCompatibility.Apply(MainScript);

                // Load scripts.lua file from file system or big file
                var filePath = Path.Combine("Data", "Scripts", "scripts.lua");
                var fileEntry = Game.ContentManager.FileSystem.GetFile(filePath);
                if (fileEntry != null)
                {
                    logger.Info($"Executing file {filePath}");
                    using (var fileStream = fileEntry.Open())
                    {
                        MainScript.DoStream(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while loading script file");
            }

            LuaEventHandlerInit();
        }

        public void ExecuteUserCode(string externalCode)
        {
            try
            {
                logger.Info($"Executing user code {externalCode}");
                MainScript.DoString(externalCode);
            }
            catch (SyntaxErrorException exeption)
            {
                throw (exeption);
            }
            catch (ScriptRuntimeException exeption)
            {
                throw (exeption);
            }
            catch (Exception exeption)
            {
                throw (exeption);
            }
        }

        public void FunctionInit()
        {
            MainScript.Globals["ExecuteAction"] = (Func<string, string, string, string, string, string, string, string, bool>) ExecuteAction;
            MainScript.Globals["EvaluateCondition"] = (Func<string, bool>) EvaluateCondition;
            MainScript.Globals["ObjectDoSpecialPower"] = (Action<string, string>) ObjectDoSpecialPower;
            MainScript.Globals["ObjectCreateAndFireTempWeapon"] = (Action<string, string>) ObjectCreateAndFireTempWeapon;
            MainScript.Globals["ObjectGrantUpgrade"] = (Action<string, string>) ObjectGrantUpgrade;
            MainScript.Globals["ObjectRemoveUpgrade"] = (Action<string, string>) ObjectRemoveUpgrade;
            MainScript.Globals["ObjectHasUpgrade"] = (Func<string, string, bool>) ObjectHasUpgrade;
            MainScript.Globals["ObjectSetObjectStatus"] = (Action<string, string>) ObjectSetObjectStatus;
            MainScript.Globals["ObjectTestObjectStatus"] = (Func<string, string, bool>) ObjectTestObjectStatus;
            MainScript.Globals["ObjectClearObjectStatus"] = (Action<string, string>) ObjectClearObjectStatus;
            MainScript.Globals["ObjectSetModelCondition"] = (Action<string, string>) ObjectSetModelCondition;
            MainScript.Globals["ObjectTestModelCondition"] = (Func<string, string, bool>) ObjectTestModelCondition;
            MainScript.Globals["ObjectClearModelCondition"] = (Action<string, string>) ObjectClearModelCondition;
            MainScript.Globals["ObjectPlaySound"] = (Action<string, string>) ObjectPlaySound;
            MainScript.Globals["ObjectSetDelayedDeath"] = (Action<string, string>) ObjectSetDelayedDeath;
            MainScript.Globals["ObjectChangeAllegianceFromNonPlayablePlayer"] = (Action<string, string>) ObjectChangeAllegianceFromNonPlayablePlayer;
            MainScript.Globals["ObjectForbidPlayerCommands"] = (Action<string, string>) ObjectForbidPlayerCommands;
            MainScript.Globals["ObjectSetGeometryActive"] = (Action<string, string, string>) ObjectSetGeometryActive;
            MainScript.Globals["ObjectHideSubObject"] = (Action<string, string, string>) ObjectHideSubObject;
            MainScript.Globals["ObjectHideSubObjectPermanently"] = (Action<string, string, string>) ObjectHideSubObjectPermanently;
            MainScript.Globals["ObjectCountNearbyEnemies"] = (Func<string, string, int>) ObjectCountNearbyEnemies;
            MainScript.Globals["ObjectGetHealthFraction"] = (Func<string, int>) ObjectGetHealthFraction;
            MainScript.Globals["ObjectDescription"] = (Func<string, string>) ObjectDescription;
            MainScript.Globals["ObjectTeamName"] = (Func<string, string>) ObjectTeamName;
            MainScript.Globals["ObjectPlayerSide"] = (Func<string, string>) ObjectPlayerSide;
            MainScript.Globals["ObjectCapturingObjectPlayerSide"] = (Func<string, string>) ObjectCapturingObjectPlayerSide;
            MainScript.Globals["ObjectTemplateName"] = (Func<string, string>) ObjectTemplateName;
            MainScript.Globals["ObjectDispatchEvent"] = (Action<string, string, string>) ObjectDispatchEvent;
            MainScript.Globals["ObjectSpy"] = (Action<string>) ObjectSpy;
            MainScript.Globals["HordeBroadcastEventToMembers"] = (Action<string, string>) HordeBroadcastEventToMembers;
            MainScript.Globals["ObjectBroadcastEventToEnemies"] = (Action<string, string, string>) ObjectBroadcastEventToEnemies;
            MainScript.Globals["ObjectBroadcastEventToAllies"] = (Action<string, string, string>) ObjectBroadcastEventToAllies;
            MainScript.Globals["ObjectBroadcastEventToUnits"] = (Action<string, string, string>) ObjectBroadcastEventToUnits;
            MainScript.Globals["ObjectBroadcastEventToCivilians"] = (Action<string, string, string>) ObjectBroadcastEventToCivilians;
            MainScript.Globals["ObjectSetChanting"] = (Action<string, string>) ObjectSetChanting;
            MainScript.Globals["ObjectSetFearFactor"] = (Action<string, string>) ObjectSetFearFactor;
            MainScript.Globals["ObjectTestCanSufferFear"] = (Func<string, bool>) ObjectTestCanSufferFear;
            MainScript.Globals["ObjectEnterFearstate"] = (Action<string, string, string>) ObjectEnterFearstate;
            MainScript.Globals["ObjectEnterAlertstate"] = (Action<string>) ObjectEnterAlertstate;
            MainScript.Globals["ObjectEnterCowerstate"] = (Action<string, string>) ObjectEnterCowerstate;
            MainScript.Globals["ObjectEnterRampagestate"] = (Action<string>) ObjectEnterRampagestate;
            MainScript.Globals["ObjectEnterRunAwayPanicstate"] = (Action<string, string>) ObjectEnterRunAwayPanicstate;
            MainScript.Globals["ObjectEnterUncontrollableCowerstate"] = (Action<string, string>) ObjectEnterUncontrollableCowerstate;
            MainScript.Globals["GetRandomNumber"] = (Func<double>) GetRandomNumber;
            MainScript.Globals["GetFrame"] = (Func<int>) GetFrame;
            //addititional custom and testing functions
            MainScript.Globals["Spawn"] = (Func<string, string>) Spawn;
            MainScript.Globals["Spawn2"] = (Func<string, float, float, float, float, string>) Spawn2;
        }

        public void LuaEventHandlerInit()
        {

        }

        public void LuaEventHandler()
        {
            //TODO Load "ScriptEvents.xml" and register events

            //EVENTS:
            //OnContainCountChanged
            //OnPowerRestore
            //OnPowerOutage
            //OnClipFull
            //OnClipEmpty
            //OnBuildVariation
            //OnGenericEvent
            //OnSlaughtered
            //OnBuildingComplete
            //OnCreated
            //OnQuenched
            //OnAflame
            //DamageIncoming
            //BeScary
            //OnTeamDestroyed
            //OnTeamExited
            //OnTeamEntered
            //OnUnitExited
            //OnUnitEntered
            //OnArrived
            //OnDestroyed
            //OnHealed
            //OnDamaged

            //ObjectStatusEvent
            //ModelConditionEvent
            //ScriptedEvent
            //InternalEvent
            //GenericEvent (self,data) //example data: "show_rock"
        }

        public int GetInt(string number)
        {
            int i = 0;
            if (!int.TryParse(number, out i))
            {
                i = -1;
            }
            return i;
        }

        public bool Getstate(string state)
        {
            if (state.Equals("1") || state.Equals("true"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddGameObjectRefToGlobalsTable(Table gameObject)
        {
            //_G or globals
            //Add code here
        }

        public int GetLuaObjectID(string gameObject)
        {
            return int.Parse(gameObject.Replace("ObjID#", ""), System.Globalization.NumberStyles.HexNumber);
        }

        public string GetLuaObjectIndex(int ObjectID)
        {
            return String.Concat("ObjID#", ObjectID.ToString("X8"));
        }

        public string Spawn(string objectType)  //quick spawn
        {
            if (objectType.Equals("")) { objectType = "AmericaVehicleDozer"; }
            var spawnUnit = Game.Scene3D.GameObjects.Add(objectType, Game.Scene3D.LocalPlayer);
            var localPlayerStartPosition = Game.Scene3D.Waypoints[$"Player_{1}_Start"].Position;
            localPlayerStartPosition.Z += Game.Scene3D.Terrain.HeightMap.GetHeight(localPlayerStartPosition.X, localPlayerStartPosition.Y);
            var spawnUnitPosition = localPlayerStartPosition;
            var playerTemplate = Game.Scene3D.LocalPlayer.Template;
            var startingBuilding = Game.Scene3D.GameObjects.Add(playerTemplate.StartingBuilding.Value, Game.Scene3D.LocalPlayer);
            spawnUnitPosition += System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, startingBuilding.Rotation) * startingBuilding.Definition.Geometry.MajorRadius;
            spawnUnit.SetTranslation(spawnUnitPosition);
            return GetLuaObjectIndex(Game.Scene3D.GameObjects.GetObjectId(spawnUnit));
        }

        public string Spawn2(string objectType, float xPos, float yPos, float zPos, float rotation)
        {
            var player = Game.Scene3D.LocalPlayer;
            var spawnUnit = Game.Scene3D.GameObjects.Add(objectType, player);
            var spawnPosition = new System.Numerics.Vector3(xPos, yPos, zPos);
            spawnPosition.Z += Game.Scene3D.Terrain.HeightMap.GetHeight(spawnPosition.X, spawnPosition.Y);
            if (zPos > spawnPosition.Z) { spawnPosition.Z = zPos; }
            var rot = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitZ, Mathematics.MathUtility.ToRadians(rotation));
            spawnPosition += System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, rot);
            spawnUnit.SetTranslation(spawnPosition);
            return GetLuaObjectIndex(Game.Scene3D.GameObjects.GetObjectId(spawnUnit));
        }

        public string GetActionNameVariant(string action, int variant)
        {
            if (variant == 1)
            {
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(action.Replace("_", " ").ToLower()).Replace(" ", "");
            }
            else
            {
                return System.Text.RegularExpressions.Regex.Replace(action, "([a-z])([A-Z])", "$1 $2");
            }
        }

        public bool ExecuteAction(string action, string P1, string P2, string P3, string P4, string P5, string P6, string P7)
        {
            //return OpenSage.Scripting.Actions.ActionLookup.Get(action)(action, context);
            return true;
        }

        public bool EvaluateCondition(string ConditionString)
        {
            return true;
        }

        public void ObjectDoSpecialPower(string gameObject, string specialPower)
        {
        }

        public void ObjectCreateAndFireTempWeapon(string gameObject, string weapon)
        {
        }

        public void ObjectGrantUpgrade(string gameObject, string upgrade)
        {
        }

        public void ObjectRemoveUpgrade(string gameObject, string upgrade)
        {
        }

        public bool ObjectHasUpgrade(string gameObject, string upgrade)
        {
            return true;
        }

        public void ObjectSetObjectStatus(string gameObject, string objectStatus)
        {
        }

        public bool ObjectTestObjectStatus(string gameObject, string objectStatus)
        {
            return true;
        }

        public void ObjectClearObjectStatus(string gameObject, string objectStatus)
        {
        }

        public void ObjectSetModelCondition(string gameObject, string modelCondition)
        {
            Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).CopyModelConditionFlags(IniParser.ParseEnumBitArray<Logic.Object.ModelConditionFlag>(modelCondition, IniTokenPosition.None));
        }

        public bool ObjectTestModelCondition(string gameObject, string modelCondition)
        {
            var modelConditionBitArray = IniParser.ParseEnumBitArray<Logic.Object.ModelConditionFlag>(modelCondition, IniTokenPosition.None);
            var modelconditionBitArrayEnum = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).ModelConditionStates;
            foreach (var i in modelconditionBitArrayEnum)
            {
                if (i == modelConditionBitArray)
                {
                    return true;
                }
            }
            return false;
        }

        public void ObjectClearModelCondition(string gameObject, string modelCondition)
        {
            var modelConditionBitArray = IniParser.ParseEnumBitArray<Logic.Object.ModelConditionFlag>(modelCondition, IniTokenPosition.None);
            var modelconditionBitArrayEnum = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).ModelConditionStates;
            foreach (var i in modelconditionBitArrayEnum)
            {
                if (i == modelConditionBitArray)
                {
                    //remove modelCondition from BitArray
                    break;
                }
            }
        }

        public void ObjectPlaySound(string gameObject, string sound)
        {
            Game.Audio.PlayAudioEvent(sound);
        }

        public void ObjectSetDelayedDeath(string gameObject, string delay) //unknown params
        {
        }

        public void ObjectChangeAllegianceFromNonPlayablePlayer(string gameObject1, string gameObject2)
        {
            //Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject1)).Owner.Side = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject2)).Owner.Side;
        }

        public void ObjectForbidPlayerCommands(string gameObject, string state)
        {
        }

        public void ObjectSetGeometryActive(string gameObject, string geometryName, string state)
        {
        }

        public void ObjectHideSubObject(string gameObject, string subObject, string state)
        {
            //Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).DrawModules
        }

        public void ObjectHideSubObjectPermanently(string gameObject, string subObject, string state)
        {
        }

        public int ObjectCountNearbyEnemies(string gameObject, string radius)
        {
            return Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Team.Owner.Enemies.Count; //placeholder
        }

        public int ObjectGetHealthFraction(string gameObject)
        {
            return (int)(Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Body?.Health ?? (Mathematics.FixedMath.Fix64) 0);
        }

        public string ObjectDescription(string gameObject)  //EXAMPLE C&C3: "Object 1187 (_jIWv4) [NODAvatar, owned by player 3 (MetaIdea)]"
        {
            int ObjectID = Game.Scene3D.GameObjects.GetObjectId(Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)));
            string ObjectNameRef = "TODO";
            string ObjectTypeName = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Definition.Name;
            string PlayerIndex = "TODO";
            string PlayerName = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Owner.Name;
            return $"Object {ObjectID} ({ObjectNameRef} [{ObjectTypeName}, owend by player {PlayerIndex} ({PlayerName})]";
        }

        public string ObjectTeamName(string gameObject) //EXAMPLE C&C3: "teamPlayer_2"
        {
            return Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Team.Name;
        }

        public string ObjectPlayerSide(string gameObject) //EXAMPLE C&C3: "{0,0}ED46C05A" BFME: "Isengard""
        {
            return Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Owner.Side;
        }

        public string ObjectCapturingObjectPlayerSide(string gameObject) //EXAMPLE C&C3: "{0,0}ED46C05A" BFME: "Isengard""
        {
            return Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Owner.Side;
        }

        public string ObjectTemplateName(string gameObject)  //EXAMPLE C&C3: "{0,0}BC0BF618" (HEX of ObjectTypeName)
        {
            return Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Definition.Name;
        }

        public void ObjectDispatchEvent(string gameObject, string eventName, string radius)
        {
        }

        public void ObjectSpy(string gameObject)
        {
        }

        public void HordeBroadcastEventToMembers(string gameObject, string eventName) //unknown params
        {
        }

        public void ObjectBroadcastEventToEnemies(string gameObject, string eventName, string radius)
        {
        }

        public void ObjectBroadcastEventToAllies(string gameObject, string eventName, string radius)
        {
        }

        public void ObjectBroadcastEventToUnits(string gameObject, string eventName, string radius)
        {
        }

        public void ObjectBroadcastEventToCivilians(string gameObject, string eventName, string radius)
        {
        }

        public void ObjectSetChanting(string gameObject, string state)
        {
        }

        public void ObjectSetFearFactor(string gameObject, string FearFactor) //unknown params
        {
        }

        public bool ObjectTestCanSufferFear(string gameObject)
        {
            return true;
        }

        public void ObjectEnterFearstate(string gameObject1, string gameObject2, string state)
        {
        }

        public void ObjectEnterAlertstate(string gameObject)
        {
        }

        public void ObjectEnterCowerstate(string gameObject1, string gameObject2)
        {
        }

        public void ObjectEnterRampagestate(string gameObject)
        {
        }

        public void ObjectEnterRunAwayPanicstate(string gameObject1, string gameObject2)
        {
        }

        public void ObjectEnterUncontrollableCowerstate(string gameObject1, string gameObject2)
        {
        }

        public double GetRandomNumber()  //attention for multiplayer sync
        {
            var random = new Random();
            return random.NextDouble();
        }

        public int GetFrame()
        {
            return (int) Game.CurrentFrame;
        }
    }
}
