﻿using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
{
    public class MapScriptCollection
    {
        private readonly Dictionary<string, MapScript> _scriptsByName;

        public MapScriptGroup[] ScriptGroups { get; }
        public MapScript[] Scripts { get; }

        public MapScriptCollection(ScriptList scriptList)
        {
            var scriptGroups = MapScriptGroup.Create(scriptList.ScriptGroups);
            var scripts = MapScript.Create(scriptList.Scripts);

            ScriptGroups = scriptGroups;
            Scripts = scripts;

            _scriptsByName = new Dictionary<string, MapScript>();

            foreach (var script in scripts)
            {
                _scriptsByName[script.Name] = script;
            }

            foreach (var scriptGroup in scriptGroups)
            {
                foreach (var script in scriptGroup.Scripts)
                {
                    _scriptsByName[script.Name] = script;
                }
            }
        }

        public MapScript FindScript(string name)
        {
            _scriptsByName.TryGetValue(name, out var result);
            return result;
        }

        public void Execute(ScriptExecutionContext context)
        {
            foreach (var scriptGroup in ScriptGroups)
            {
                scriptGroup.Execute(context);
            }

            foreach (var script in Scripts)
            {
                script.Execute(context);
            }
        }
    }
}
