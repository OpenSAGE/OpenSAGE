#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenSage.Logic.Object;

public interface IGameObjectCollection
{
    IEnumerable<GameObject> Objects { get; }
    public GameObject CreateObject(ObjectDefinition objectDefinition, Player? player);
    public GameObject? GetObjectById(uint objectId);
    public bool TryGetObjectByName(string name, [NotNullWhen(true)] out GameObject? gameObject);
    public void AddNameLookup(GameObject gameObject);
    public void DestroyObject(GameObject gameObject);
    public void DeleteDestroyed();
}
