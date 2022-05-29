// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.Tools.AptTool.Base;

var test = new WrappedCharacter<Text>()
{
    Item = new Text()
    {
        
    }
};
var str = JsonConvert.SerializeObject(test);
Console.WriteLine(str);
Console.WriteLine(((Text) JsonConvert.DeserializeObject<WrappedCharacterBase>(str).getItem()));

