using System.Collections;
using System.Collections.Generic;

namespace OpenSage.Terrain.Roads
{
    /// <summary>
    /// This class is responsible for maintaining the rendering order of the different road templates.
    /// The default order is the inverse asset loading order, but it can be changed by end caps:
    ///
    /// When an end cap of road template A joins a road of a different template B, A is moved
    /// to the back of the list (so that it is rendered last and thus certainly above B).
    /// We also store the information that A has to come after B, in case B is later moved to the
    /// back by another end cap. In that case A is moved with it so that the order B -> A is kept.
    /// We don't move A's successors though, which is probably a bug in the original engine.
    /// On the other hand, there could easily be cycles, and it doesn't have to be perfect,
    /// we just need a deterministic order.
    /// </summary>
    internal sealed class RoadTemplateList : IEnumerable<RoadTemplate>
    {
        private LinkedList<RoadTemplate> _orderedTemplates;
        private IDictionary<RoadTemplate, List<RoadTemplate>> _subsequentTemplates;

        public RoadTemplateList(IEnumerable<RoadTemplate> roadTemplates)
        {
            _orderedTemplates = new LinkedList<RoadTemplate>();
            _subsequentTemplates = new Dictionary<RoadTemplate, List<RoadTemplate>>();

            // by prepending the templates, we get the default (inverse asset loading) order
            foreach (var template in roadTemplates)
            {
                _orderedTemplates.AddFirst(template);
                _subsequentTemplates[template] = new List<RoadTemplate>();
            }
        }

        /// <summary>
        /// Modify the Z-order of the road templates so that "above" is rendered after "below".
        /// </summary>
        public void HandleRoadJoin(RoadTemplate above, RoadTemplate below)
        {
            _subsequentTemplates[below].Add(above);
            _subsequentTemplates[below].AddRange(_subsequentTemplates[above]);

            _orderedTemplates.Remove(above);
            _orderedTemplates.AddLast(above);

            foreach (var subsequent in _subsequentTemplates[above])
            {
                _orderedTemplates.Remove(subsequent);
                _orderedTemplates.AddLast(subsequent);
            }
        }        

        public IEnumerator<RoadTemplate> GetEnumerator() => _orderedTemplates.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }    
}
