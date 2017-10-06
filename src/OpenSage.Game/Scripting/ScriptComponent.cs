namespace OpenSage.Scripting
{
    public class ScriptComponent : EntityComponent
    {
        public MapScriptGroup[] ScriptGroups { get; set; }
        public MapScript[] Scripts { get; set; }

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
