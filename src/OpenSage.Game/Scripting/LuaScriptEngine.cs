// MOONSHARP LICENSE from https://github.com/xanathar/moonsharp/blob/master/LICENSE
// Licensed under the 3-clause BSD license

//Copyright(c) 2014-2016, Marco Mastropaolo
//All rights reserved.

//Parts of the string library are based on the KopiLua project(https://github.com/NLua/KopiLua)
//Copyright (c) 2012 LoDC

//Visual Studio Code debugger code is based on code from Microsoft vscode-mono-debug project (https://github.com/Microsoft/vscode-mono-debug).
//Copyright (c) Microsoft Corporation - released under MIT license.

//Remote Debugger icons are from the Eclipse project (https://www.eclipse.org/).
//Copyright of The Eclipse Foundation

//The MoonSharp icon is (c) Isaac, 2014-2015

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this
//  list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice,
//  this list of conditions and the following disclaimer in the documentation
//  and/or other materials provided with the distribution.

//* Neither the name of the { organization}
//nor the names of its
//contributors may be used to endorse or promote products derived from
//  this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace OpenSage.Scripting
{
    public sealed class LuaScriptEngine : GameSystem
    {
        public Script MainScript;

        public LuaScriptEngine(Game game) : base(game)
        {
            Script.DefaultOptions.DebugPrint = text =>
            {
                Console.WriteLine(text);
                OpenSage.Diagnostics.LuaScriptConsole._scriptConsoleTextAll =
                string.Concat(OpenSage.Diagnostics.LuaScriptConsole._scriptConsoleTextAll, text, "\n");
            };

            MainScript = new Script();

            FunctionInit();

            try
            {
                MainScript.DoString(Lua401Compatibility.Lua401CompatibilityCode);
                var filePath = System.IO.Path.Combine(Game.ContentManager.FileSystem.RootDirectory, "Data", "Scripts", "scripts.lua");
                //load scripts.lua file from folder or big file
                if (System.IO.File.Exists(filePath))
                {
                    MainScript.DoFile(filePath);
                }
                else
                {
                    //load scripts.lua from big file
                    MainScript.DoString(Game.ContentManager.IniDataContext.GetIniFileContent(System.IO.Path.Combine("Data", "Scripts", "scripts.lua")));
                }
            }
            catch (System.NullReferenceException) //standard case for generals and zero hour since no scripts.lua file
            {
                //Console.Write("no scripts.lua file found");
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.Write("no scripts.lua file found");
            }
            catch (Exception ex)
            {
                //MainScript.DoString("_ALERT()");
                Console.Write(ex);
            }

            LuaEventHandlerInit();
        }

        public void ExecuteUserCode(string externalCode)
        {
            try
            {
                MainScript.DoString(externalCode);
            }
            catch (SyntaxErrorException exeption)
            {
                Console.WriteLine("LUA SYNTAX ERROR: ", exeption.DecoratedMessage);
                throw(exeption);
            }
            catch (ScriptRuntimeException exeption)
            {
                Console.WriteLine($"LUA RUNTIME ERROR: ", exeption.DecoratedMessage);
                throw (exeption);
            }
            catch (Exception exeption)
            {
                Console.WriteLine($"LUA CRITICAL ERROR: ", exeption);
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
            if (!Int32.TryParse(number, out i))
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
            else //if (state.Equals("0") || state.Equals("false"))
            {
                return false;
            }
            //return null;
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
            return String.Concat("ObjID#",ObjectID.ToString("X8"));
        }

        public string Spawn(string objectType)  //quick spawn
        {
            if (objectType.Equals("")) { objectType = "AmericaVehicleDozer"; }
            OpenSage.Logic.Object.GameObject spawnUnit = Game.Scene3D.GameObjects.Add(Game.ContentManager.IniDataContext.Objects.Find(x => x.Name == objectType), Game.Scene3D.LocalPlayer);
            var localPlayerStartPosition = Game.Scene3D.Waypoints[$"Player_{1}_Start"].Position;
            localPlayerStartPosition.Z += Game.Scene3D.Terrain.HeightMap.GetHeight(localPlayerStartPosition.X, localPlayerStartPosition.Y);
            var spawnUnitPosition = localPlayerStartPosition;
            var playerTemplate = Game.ContentManager.IniDataContext.PlayerTemplates.Find(t => t.Side == Game.Scene3D.LocalPlayer.Side);
            var startingBuilding = Game.Scene3D.GameObjects.Add(Game.ContentManager.IniDataContext.Objects.Find(x => x.Name == playerTemplate.StartingBuilding), Game.Scene3D.LocalPlayer);
            spawnUnitPosition += System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, startingBuilding.Transform.Rotation) * startingBuilding.Definition.Geometry.MajorRadius;
            spawnUnit.Transform.Translation = spawnUnitPosition;
            return GetLuaObjectIndex(Game.Scene3D.GameObjects.GetObjectId(spawnUnit));
        }

        public string Spawn2(string objectType, float xPos, float yPos, float zPos, float rotation)
        {
            var player = Game.Scene3D.LocalPlayer;
            OpenSage.Logic.Object.GameObject spawnUnit = Game.Scene3D.GameObjects.Add(Game.ContentManager.IniDataContext.Objects.Find(x => x.Name == objectType), player);
            var spawnPosition = new System.Numerics.Vector3(xPos, yPos, zPos);
            spawnPosition.Z += Game.Scene3D.Terrain.HeightMap.GetHeight(spawnPosition.X, spawnPosition.Y);
            if (zPos > spawnPosition.Z) { spawnPosition.Z = zPos; }
            var rot = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitZ, OpenSage.Mathematics.MathUtility.ToRadians(rotation));
            spawnPosition += System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, rot);
            spawnUnit.Transform.Translation = spawnPosition;
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
            OpenSage.Data.Ini.Parser.IniParser Parser = new OpenSage.Data.Ini.Parser.IniParser(null, null, null, Game.SageGame);
            Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).SetModelConditionFlags(Parser.ParseEnumBitArray<OpenSage.Logic.Object.ModelConditionFlag>(modelCondition));
        }

        public bool ObjectTestModelCondition(string gameObject, string modelCondition)
        {
            OpenSage.Data.Ini.Parser.IniParser Parser = new OpenSage.Data.Ini.Parser.IniParser(null, null, null, Game.SageGame);
            var modelConditionBitArray = Parser.ParseEnumBitArray<OpenSage.Logic.Object.ModelConditionFlag>(modelCondition);
            var modelconditionBitArrayEnum = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).ModelConditionStates;
            foreach (OpenSage.Data.Ini.BitArray<OpenSage.Logic.Object.ModelConditionFlag> i in modelconditionBitArrayEnum)
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
            OpenSage.Data.Ini.Parser.IniParser Parser = new OpenSage.Data.Ini.Parser.IniParser(null, null, null, Game.SageGame);
            var modelConditionBitArray = Parser.ParseEnumBitArray<OpenSage.Logic.Object.ModelConditionFlag>(modelCondition);
            var modelconditionBitArrayEnum = Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).ModelConditionStates;
            foreach (OpenSage.Data.Ini.BitArray<OpenSage.Logic.Object.ModelConditionFlag> i in modelconditionBitArrayEnum)
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
            return (int)Game.Scene3D.GameObjects.GetObjectById(GetLuaObjectID(gameObject)).Health;
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
            Random random = new Random();
            return random.NextDouble();
        }

        public int GetFrame()
        {
            return (int)Game.CurrentFrame;
        }
    }

    public class LuaScriptEngineObject : GameSystem //lua environment instance for each object with lua registration
    {
        public Script MainScript { get; set; }

        public LuaScriptEngineObject(Game game) : base(game)
        {
            Script.DefaultOptions.DebugPrint = text => Console.WriteLine(text);
            MainScript = new Script();
            FunctionInit();
            MainScript.DoString(Lua401Compatibility.Lua401CompatibilityCode);
        }

        public void FunctionInit()
        {
            MainScript.Globals["CurDrawableObjectStatus"] = (Func<string, bool>) CurDrawableObjectStatus;
            MainScript.Globals["CurDrawableModelcondition"] = (Func<string, bool>) CurDrawableModelcondition;
            MainScript.Globals["CurDrawableGetCurrentTargetBearing"] = (Func<float>) CurDrawableGetCurrentTargetBearing;
            MainScript.Globals["CurDrawableGetCurrentTargetHeight"] = (Func<float>) CurDrawableGetCurrentTargetHeight;
            MainScript.Globals["CurDrawableGetCurrentTargetDistance"] = (Func<float>) CurDrawableGetCurrentTargetDistance;
            MainScript.Globals["CurDrawableIsCurrentTargetKindof"] = (Func<string, bool>) CurDrawableIsCurrentTargetKindof;
            MainScript.Globals["CurDrawablePrevAnimation"] = (Func<string>) CurDrawablePrevAnimation;
            MainScript.Globals["CurDrawablePrevAnimationstate"] = (Func<string>) CurDrawablePrevAnimationstate;
            MainScript.Globals["CurDrawablePrevAnimFraction"] = (Func<float>) CurDrawablePrevAnimFraction;
            MainScript.Globals["GetClientRandomNumberReal"] = (Func<int, int, float>) GetClientRandomNumberReal;
            MainScript.Globals["GetFrame"] = (Func<int>) GetFrame;
            MainScript.Globals["CurDrawablePlaySound"] = (Action<string>) CurDrawablePlaySound;
            Action CurDrawableAllowToContinueAction = () => CurDrawableAllowToContinue();
            MainScript.Globals["CurDrawableAllowToContinue"] = CurDrawableAllowToContinueAction;
            MainScript.Globals["CurDrawableSetTransitionAnimstate"] = (Action<string>) CurDrawableSetTransitionAnimstate;
            MainScript.Globals["CurDrawableShowModule"] = (Action<string>) CurDrawableShowModule;
            MainScript.Globals["CurDrawableHideModule"] = (Action<string>) CurDrawableHideModule;
            MainScript.Globals["CurDrawableHideSubObjectPermanently"] = (Action<string>) CurDrawableHideSubObjectPermanently;
            MainScript.Globals["CurDrawableShowSubObjectPermanently"] = (Action<string>) CurDrawableShowSubObjectPermanently;
            MainScript.Globals["CurDrawableShowSubObject"] = (Action<string>) CurDrawableShowSubObject;
            MainScript.Globals["CurDrawableHideSubObject"] = (Action<string>) CurDrawableHideSubObject;
        }

        public void LuaEventHandlerInit()
        {
        }

        public void LuaEventHandler()
        {
        }

            public bool CurDrawableObjectStatus(string objectStatus)
        {
            return true;
        }

        public bool CurDrawableModelcondition(string modelCondition)
        {
            return true;
        }

        public float CurDrawableGetCurrentTargetBearing() //example return 0.10
        {
            return 0;
        }

        public float CurDrawableGetCurrentTargetHeight()
        {
            return 0;
        }

        public float CurDrawableGetCurrentTargetDistance()
        {
            return 0;
        }

        public bool CurDrawableIsCurrentTargetKindof(string kindof)
        {
            return true;
        }

        public string CurDrawablePrevAnimation() //example return "runtrans_bs"
        {
            return "";
        }

        public string CurDrawablePrevAnimationstate() //example return "Moving_Sword"
        {
            return "";
        }

        public float CurDrawablePrevAnimFraction() //example return 0.66
        {
            return 0;
        }

        public float GetClientRandomNumberReal(int low, int high)
        {
            return 0;
        }

        public int GetFrame()
        {
            return 0;
        }

        public void CurDrawablePlaySound(string soundName)
        {

        }

        public void CurDrawableAllowToContinue()
        {

        }

        public void CurDrawableSetTransitionAnimstate(string transitionstateName)
        {

        }

        public void CurDrawableShowModule(string moduleTagName)  //example input "ModuleTag_DrawLight"
        {

        }

        public void CurDrawableHideModule(string moduleTagName)
        {

        }

        public void CurDrawableHideSubObjectPermanently(string subObject)
        {

        }

        public void CurDrawableShowSubObjectPermanently(string subObject)
        {

        }

        public void CurDrawableShowSubObject(string subObject)
        {

        }

        public void CurDrawableHideSubObject(string subObject)
        {

        }
    }

    public class Lua401Compatibility
    {
        public const string Lua401CompatibilityCode = @"
                globals = _G
                function getn(table) return #table end
                closefile = io.close
                flush = io.flush
                openfile = io.open
                read = io.read
                tmpname = os.tmpname
                write = io.write
                abs = math.abs
                acos = math.acos
                asin = math.asin
                atan = math.atan
                atan2 = math.atan2
                ceil = math.ceil
                cos = math.cos
                cosh = math.cosh
                deg = math.deg
                exp = math.exp
                floor = math.floor
                mod = math.fmod
                mod2 = math.modf
                frexp = math.frexp
                ldexp = math.ldexp
                log = math.log
                max = math.max
                min = math.min
                PI = math.pi
                randomseed = math.randomseed
                rad = math.rad
                random = math.random
                sin = math.sin
                sqrt = math.sqrt
                tan = math.tan
                clock = os.clock
                date = os.date
                execute = os.execute
                exit = os.exit
                getenv = os.getenv
                remove = os.remove
                rename = os.rename
                setlocale = os.setlocale
                strbyte = string.byte
                strchar = string.char
                strfind = string.find
                format = string.format
                gsub = string.gsub
                strlen = string.len
                strlower = string.lower
                strrep = string.rep
                strsub = string.sub
                strupper = string.upper
                tinsert = table.insert
                tremove = table.remove
                sort = table.sort
                function log10(number) return math.log(number,10) end
                call = pcall
                function seek(filehandle, whence, offset) return filehandle:seek(whence, offset) end
                rawgettable = rawget
                rawsettable = rawset
                function getglobal(index) return _G[index] end
                function setglobal(index, value) _G[index]=value end
                function foreach(t, f)
                    for i, v in t do
                        local res = f(i, v)
                        if res then return res end
                    end
                end
                function foreachi(t, f)
                    for i=1,#(t) do
                        local res = f(i, t[i])
                        if res then return res end
                    end
                end
                function readfrom(file) _INPUT = io.open(file,'r') return io.read(file) end
                function writeto(file) _OUTPUT = io.open(file,'w+') return io.output(file) end
                function appendto(file) _OUTPUT = io.open(file,'a+') return io.output(file) end
                --debug = debug.debug
                function rawgetglobal(index) rawget(_G, index) end
                function rawsetglobal(index, value) return rawset(_G, index, value) end
                function _ALERT(...) print('error') end
                function _ERRORMESSAGE(...) print('error') end";
                //unsupported (and never used in any SAGE game and it's mods): debug and tag methods, %upvalues
                //DEPRECATED and actually removed from original SAGE: rawgetglobal, rawsetglobal, foreachvar, nextvar
    }
}
