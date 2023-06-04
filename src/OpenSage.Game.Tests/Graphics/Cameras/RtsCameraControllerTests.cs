using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using Xunit;

namespace OpenSage.Tests.Graphics.Cameras
{
    public class RtsCameraControllerTests
    {
        const float DefaultHeight = 10;
        const float DefaultPitch = (float)Math.PI / 4;
        const int MapSize = 1000;
        const int WindowSize = 100;

        class MockHeightMap : IHeightMap
        {
            public int MaxXCoordinate => MapSize;

            public int MaxYCoordinate => MapSize;

            public float GetHeight(float x, float y)
            {
                return 0;
            }
        }

        class MockCamera : ICamera
        {
            public Vector3 CameraPosition { get; private set; }
            public Vector3 CameraTarget { get; private set; }
            public Vector3 Up { get; private set; }

            public float FieldOfView
            {
                get => (float)Math.PI / 2;
                set => throw new NotImplementedException();
            }
            public float FarPlaneDistance
            {
                get => 100;
                set => throw new NotImplementedException();
            }

            public Vector3 Position => throw new NotImplementedException();

            public void SetLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up)
            {
                CameraPosition = cameraPosition;
                CameraTarget = cameraTarget;
                Up = up;
            }
        }

        class MockPanel : IPanel
        {
            public Rectangle Frame => new Rectangle(0, 0, WindowSize, WindowSize);
        }

        MockCamera Camera { get; }
        MockHeightMap HeightMap { get; }
        MockPanel  Panel { get; }

        ICameraController Controller { get; }
        Vector3 _lastPosition;
        Vector3 _lastTarget;

        void AssertUp()
        {
            Assert.True(Vector3.Dot(Camera.Up, new Vector3(0, 0, 1)) > 0);
        }

        Vector3 CameraTarget(float pitch)
        {
            Vector3 pos = Camera.CameraPosition;
            return pos - new Vector3(0, 0, (float)Math.Tan(pitch) * pos.Z);
        }

        public RtsCameraControllerTests()
        {
            Camera = new MockCamera();
            HeightMap = new MockHeightMap();
            Panel = new MockPanel();
            Controller = new RtsCameraController(
                DefaultHeight,
                DefaultPitch,
                0,
                Camera,
                HeightMap,
                Panel);
            Controller.TerrainPosition = new Vector3(MapSize / 2, MapSize / 2, 0);
            Controller.UpdateCamera(Camera, TimeInterval.Zero);
            AssertUp();
            _lastPosition = Camera.CameraPosition;
            _lastTarget = Camera.CameraTarget;
        }

        static CameraInputState CreateInputState()
        {
            var inputState = new CameraInputState();
            inputState.PressedKeys = new List<Veldrid.Key>();
            inputState.LastX = WindowSize / 2;
            inputState.LastY = WindowSize / 2;
            return inputState;
        }

        public static IEnumerable<object[]> PanningData()
        {
            foreach (var dir in Enum.GetValues(typeof(CameraPanDirection)))
            {
                yield return new object[] { dir };
            }
        }

        static (int, int) DirectionTuple(CameraPanDirection panDirection)
        {
            return panDirection switch
            {
                CameraPanDirection.None => (0, 0),
                CameraPanDirection.Right => (1, 0),
                CameraPanDirection.Down => (0, 1),
                CameraPanDirection.RightDown => (1, 1),
                CameraPanDirection.LeftDown => (-1, 1),
                CameraPanDirection.Left => (-1, 0),
                CameraPanDirection.LeftUp => (-1, -1),
                CameraPanDirection.Up => (0, -1),
                CameraPanDirection.RightUp => (1, -1),
                _ => throw new NotImplementedException()
            };
        }

        static Vector3 ClaculatePanDelta(int x, int y, float speed)
        {
            return new Vector3(x * speed, y * speed, 0);
        }

        void DoPanningTest(
            CameraInputState inputState, int x, int y, 
            CameraPanDirection panDirection,
            float speed = RtsCameraController.PanSpeed)
        {
            Vector3 delta = ClaculatePanDelta(x, y, speed);
            Controller.UpdateInput(inputState, TimeInterval.Zero);
            Controller.UpdateCamera(Camera, TimeInterval.Zero);
            AssertUp();
            Assert.Equal(_lastPosition + delta, Camera.CameraPosition);
            Assert.Equal(_lastTarget + delta, Camera.CameraTarget);
            Assert.Equal(Controller.PanDirection, panDirection);
        }

        // Assume lookup direction is (1, 0, 0)
        [Theory]
        [MemberData(nameof(PanningData))]
        public void TestHoverPanning(CameraPanDirection panDirection)
        {
            CameraInputState inputState = CreateInputState();
            (int x, int y) = DirectionTuple(panDirection);
            inputState.LastX = (1 + x) * WindowSize / 2;
            inputState.LastY = (1 + y) * WindowSize / 2;
            DoPanningTest(inputState, -y, -x, panDirection);
        }

        // Assume lookup direction is (1, 0, 0)
        [Theory]
        [MemberData(nameof(PanningData))]
        public void TestKeyboardPanning(CameraPanDirection panDirection)
        {
            CameraInputState inputState = CreateInputState();
            (int x, int y) = DirectionTuple(panDirection);
            if (x > 0) inputState.PressedKeys.Add(Veldrid.Key.Right);
            else if (x < 0) inputState.PressedKeys.Add(Veldrid.Key.Left);
            if (y > 0) inputState.PressedKeys.Add(Veldrid.Key.Down);
            else if (y < 0) inputState.PressedKeys.Add(Veldrid.Key.Up);
            DoPanningTest(inputState, -y, -x, panDirection);
        }

        // Assume lookup direction is (1, 0, 0)
        [Theory]
        [MemberData(nameof(PanningData))]
        public void TestRightMousePanning(CameraPanDirection panDirection)
        {
            CameraInputState inputState = CreateInputState();
            (int x, int y) = DirectionTuple(panDirection);
            float speed = RtsCameraController.MousePanSpeed;
            int movement = (int)Math.Ceiling(RtsCameraController.PanDirectionThreshold / speed);
            x *= movement;
            y *= movement;
            inputState.RightMouseDown = true;
            inputState.DeltaX = x;
            inputState.DeltaY = y;
            if (panDirection == CameraPanDirection.None)
            {
                panDirection = CameraPanDirection.Right;
            }
            DoPanningTest(inputState, -y, -x, panDirection, speed);
        }

    }
}