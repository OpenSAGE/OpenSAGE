using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    // type OrderGeneratorResult = Success of Order list * exitMode: bool | Failure of Error
    public abstract class OrderGeneratorResult
    {
        private OrderGeneratorResult() { }

        public sealed class Success : OrderGeneratorResult
        {
            public IReadOnlyList<Order> Orders;

            // TODO: Should this be on the base class?
            /// <summary>
            /// Indicates if the we should return to the default OrderGenerator after this result.
            /// </summary>
            public readonly bool Exit;

            public Success(IEnumerable<Order> orders, bool exit)
            {
                Orders = orders.ToList();
                Exit = exit;
            }
        }

        public sealed class InapplicableResult : OrderGeneratorResult
        {
        }

        public sealed class FailureResult : OrderGeneratorResult
        {
            // TODO: Handle localisation / use an enum instead?
            public readonly string Error;

            public FailureResult(string error)
            {
                Error = error;
            }
        }

        /// <summary>
        /// Issues the supplied orders and continues using the current OrderGenerator.
        /// </summary>
        public static OrderGeneratorResult SuccessAndContinue(IEnumerable<Order> orders) => new Success(orders, false);

        /// <summary>
        /// Issues the supplied orders and returns to default OrderGenerator.
        /// </summary>
        public static OrderGeneratorResult SuccessAndExit(IEnumerable<Order> orders) => new Success(orders, true);

        public static OrderGeneratorResult Inapplicable() => new InapplicableResult();

        /// <summary>
        /// Notifies about an error while trying to issue the order (e.g not enough space for a building).
        /// Doesn't change the OrderGenerator.
        /// </summary>
        public static OrderGeneratorResult Failure(string error) => new FailureResult(error);
    }

    public interface IOrderGenerator
    {
        bool CanDrag { get; }

        // TODO: Should we use some other way of rendering, via Scene3D?
        void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime);

        OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers);

        void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition);

        void UpdateDrag(Vector3 position);

        string GetCursor(KeyModifiers keyModifiers);
    }
}
