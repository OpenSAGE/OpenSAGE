#nullable enable

using System;
using System.Numerics;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras;

/// <summary>
/// Converts input messages into camera movement.
/// </summary>
public sealed class LookAtTranslator(TacticalView tacticalView, CursorManager cursorManager, GameData gameData) : InputMessageHandler
{
    private readonly TacticalView _tacticalView = tacticalView;
    private readonly CursorManager _cursorManager = cursorManager;
    private readonly GameData _gameData = gameData;

    // C++: SCROLL_AMT
    private const float ScrollAmount = 100.0f;

    // TODO: We probably want to make this configurable, even though it's not in the original game
    private const int EdgeScrollSize = 3;

    private string? _prevCursor;

    // TODO(Port): Move this to InGameUI
    public bool IsScrolling { get; private set; }

    public bool IsRotating { get; private set; }

    public bool IsPitching { get; private set; }

    // C++ version is frame-based, these contain milliseconds
    private double _timestamp;
    private double _lastMouseMoveTime;

    public ScrollType ScrollType { get; private set; }


    private Point2D _anchor;
    private Point2D _currentPos;
    private Point2D _originalAnchor;

    private ScrollDirState _scrollDir;

    // C++ version also has _isChangingFOV and _lastPlaneID, but they're only used for debugging

    // TODO: Implement this
    public override HandlingPriority Priority => HandlingPriority.MoveCameraPriority;

    private void SetScrolling(ScrollType scrollType)
    {
        // TODO(Port)
        // if (!TheInGameUI->getInputEnabled())
        //   return;


        _prevCursor = _cursorManager.CurrentCursorName;
        ScrollType = scrollType;
        IsScrolling = scrollType != ScrollType.None;
        // TheInGameUI->setScrolling( TRUE );
        _tacticalView.MouseLock = true;
    }

    private void StopScrolling(in TimeInterval gameTime)
    {
        IsScrolling = false;
        // TheInGameUI->setScrolling( FALSE );
        _tacticalView.MouseLock = false;
        _cursorManager.SetCursor(_prevCursor, gameTime);
        ScrollType = ScrollType.None;
    }

    private Point2D? GetRMBScrollAnchor()
    {
        if (IsScrolling && ScrollType == ScrollType.RightMouseButton)
        {
            return _anchor;
        }
        return null;
    }

    /// <summary>
    /// Returns true if the mouse has moved within the last 1000 milliseconds.
    /// </summary>
    public bool HasMouseMovedRecently(in TimeInterval gameTime)
    {
        if (_lastMouseMoveTime > gameTime.TotalTime.TotalMilliseconds)
        {
            // If mouse moved in the future, reset the timer
            // (I don't think this can actually happen in OpenSAGE, because we recreate this object on map load)
            _lastMouseMoveTime = 0;
        }

        // Original code checks for 30 logic frames
        if (_lastMouseMoveTime + 1000 < gameTime.TotalTime.TotalMilliseconds)
        {
            return false;
        }

        return true;
    }

    // In C++ this is only used by the replay system... for some reason
    public void SetCurrentPos(Point2D pos)
    {
        _currentPos = pos;
    }

    public override InputMessageResult HandleMessage(InputMessage message, in TimeInterval gameTime)
    {
        var disp = InputMessageResult.NotHandled;

        switch (message.MessageType)
        {
            case InputMessageType.KeyDown:
            case InputMessageType.KeyUp:
                {
                    var key = message.Value.Key;
                    var state = message.MessageType == InputMessageType.KeyDown;

                    // TODO(Port):
                    // if (TheShell && TheShell->isShellActive())
                    // break;

                    switch (key)
                    {
                        case Veldrid.Key.Up:
                            _scrollDir.Up = state;
                            break;
                        case Veldrid.Key.Down:
                            _scrollDir.Down = state;
                            break;
                        case Veldrid.Key.Left:
                            _scrollDir.Left = state;
                            break;
                        case Veldrid.Key.Right:
                            _scrollDir.Right = state;
                            break;
                        // New feature: Escape disables mouse lock, so let's also stop scrolling
                        case Veldrid.Key.Escape:
                            if (IsScrolling)
                            {
                                StopScrolling(gameTime);
                            }
                            return disp;
                    }

                    // TODO(Port): Also check TheInGameUI->isSelecting()
                    if (IsScrolling && ScrollType != ScrollType.Key)
                    {
                        break;
                    }

                    // See if we need to start / stop scrolling
                    var numDirs = 0;
                    if (_scrollDir.Up) numDirs++;
                    if (_scrollDir.Down) numDirs++;
                    if (_scrollDir.Left) numDirs++;
                    if (_scrollDir.Right) numDirs++;

                    if (numDirs > 0 && !IsScrolling)
                    {
                        SetScrolling(ScrollType.Key);
                    }
                    else if (numDirs == 0 && IsScrolling)
                    {
                        StopScrolling(gameTime);
                    }
                }
                break;
            case InputMessageType.MouseRightButtonDown:
                {
                    _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;
                    _anchor = message.Value.MousePosition;

                    // TODO(Port): Also check TheInGameUI->isSelecting()
                    if (!IsScrolling)
                    {
                        SetScrolling(ScrollType.RightMouseButton);
                    }
                    break;
                }
            case InputMessageType.MouseRightButtonUp:
                {
                    _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;

                    if (ScrollType == ScrollType.RightMouseButton)
                    {
                        StopScrolling(gameTime);
                    }
                    break;
                }
            case InputMessageType.MouseMiddleButtonDown:
                {
                    _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;
                    IsRotating = true;
                    _anchor = message.Value.MousePosition;
                    _originalAnchor = _anchor;
                    _currentPos = _anchor;
                    _timestamp = gameTime.TotalTime.TotalMilliseconds;
                    break;
                }
            case InputMessageType.MouseMiddleButtonUp:
                {
                    _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;

                    // If the button was held for 5 of fewer logic frames, and it didn't move more than 5 pixels, treat it as a click
                    const uint ClickDurationFrames = 5;
                    const double ClickDurationMs = ClickDurationFrames * 1000.0 / 30.0;
                    const uint PixelOffset = 5;

                    IsRotating = false;
                    var dx = _currentPos.X - _originalAnchor.X;
                    if (dx < 0)
                    {
                        dx = -dx;
                    }
                    var dy = _currentPos.Y - _originalAnchor.Y;
                    var didMove = dx > PixelOffset || dy > PixelOffset;

                    if (!didMove && gameTime.TotalTime.TotalMilliseconds - _timestamp < ClickDurationMs)
                    {
                        _tacticalView.SetAngleAndPitchToDefault();
                        _tacticalView.SetZoomToDefault();
                    }

                    break;
                }
            case InputMessageType.MouseMove:
                {
                    var newMousePos = message.Value.MousePosition;
                    if (newMousePos != _currentPos)
                    {
                        _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;
                    }
                    _currentPos = newMousePos;

                    var screenSize = _tacticalView.Camera.ScreenSize;

                    // TODO(Port): Check TheInGameUI->getInputEnabled() and stop scrolling if it's false

                    // TODO(Port): Check if we're actually running in windowed mode
                    // (we pretty much always are, but that disables edge scrolling so let's just ignore it for now)
                    var isWindowed = false;

                    if (!isWindowed)
                    {
                        if (IsScrolling)
                        {
                            // TODO: Replace these manual comparisons with something nicer like rectangle.Contains
                            if (
                                ScrollType == ScrollType.ScreenEdge &&
                                (
                                    _currentPos.X >= EdgeScrollSize &&
                                    _currentPos.Y >= EdgeScrollSize &&
                                    _currentPos.X <= screenSize.X - EdgeScrollSize &&
                                    _currentPos.Y <= screenSize.Y - EdgeScrollSize
                                )
                            )
                            {
                                StopScrolling(gameTime);
                            }
                        }
                        else if (
                            _currentPos.X < EdgeScrollSize ||
                            _currentPos.Y < EdgeScrollSize ||
                            _currentPos.X > screenSize.X - EdgeScrollSize ||
                            _currentPos.Y > screenSize.Y - EdgeScrollSize
                        )
                        {
                            SetScrolling(ScrollType.ScreenEdge);
                        }
                    }

                    if (IsRotating)
                    {
                        var factor = 0.01f;
                        var angle = factor * (_currentPos.X - _anchor.X);
                        _tacticalView.Angle += angle;
                        _anchor = _currentPos;
                    }

                    if (IsPitching)
                    {
                        var factor = 0.01f;
                        var angle = factor * (_currentPos.Y - _anchor.Y);
                        _tacticalView.Pitch += angle;
                        _anchor = _currentPos;
                    }

                    break;
                }

            case InputMessageType.MouseWheel:
                {
                    _lastMouseMoveTime = gameTime.TotalTime.TotalMilliseconds;
                    var spin = message.Value.ScrollWheel;

                    if (spin > 0)
                    {
                        while (spin > 0)
                        {
                            _tacticalView.ZoomIn();
                            spin--;
                        }
                    }
                    else if (spin < 0)
                    {
                        while (spin < 0)
                        {
                            _tacticalView.ZoomOut();
                            spin++;
                        }
                    }
                    break;
                }
                // TODO(Port): View save / restore
        }

        return disp;
    }

    // This matches "case GameMessage::MSG_FRAME_TICK:" in the C++ version
    public void Update(in TimeInterval gameTime)
    {
        var offset = Vector2.Zero;

        // TODO(Port): TheInGameUI->isScrolling()
        var theInGameUiIsScrolling = IsScrolling;

        if (IsScrolling && !theInGameUiIsScrolling)
        {
            // TODO(Port): TheInGameUI->setScrollAmount(offset);
            StopScrolling(gameTime);
        }
        else if (IsScrolling)
        {
            var speedFactor = new Vector2(
                _gameData.HorizontalScrollSpeedFactor,
                _gameData.VerticalScrollSpeedFactor
            );

            // TODO: Integrate this with settings
            // There is an INI field for this (_gameData.KeyboardScrollSpeedFactor)
            // However, Generals seems to always overrides it with a value from player options INI file
            // We'll use the default speed for now
            var keyboardScrollSpeedFactor = 0.6f;

            switch (ScrollType)
            {
                case ScrollType.RightMouseButton:
                    {
                        // TODO(Port): TheInGameUI->shouldMoveRMBScrollAnchor()
                        var shouldMoveRMBScrollAnchor = true;

                        if (shouldMoveRMBScrollAnchor)
                        {
                            var max = _tacticalView.Camera.ScreenSize / 2.0f;
                            _anchor = new Point2D(
                                (int)Math.Clamp(_anchor.X, max.X, _tacticalView.Camera.ScreenSize.X - max.X),
                                (int)Math.Clamp(_anchor.Y, max.Y, _tacticalView.Camera.ScreenSize.Y - max.Y)
                            );
                        }

                        offset = (_currentPos.ToVector2() - _anchor.ToVector2()) * speedFactor;
                        var vec = Vector2.Normalize(offset);

                        // It's interesting that right mouse scrolling also uses the keyboard speed factor
                        offset += speedFactor * vec * MathF.Pow(keyboardScrollSpeedFactor, 2.0f);
                        break;
                    }
                case ScrollType.Key:
                    {
                        if (_scrollDir.Up)
                        {
                            offset.Y -= 1.0f;
                        }
                        if (_scrollDir.Down)
                        {
                            offset.Y += 1.0f;
                        }
                        if (_scrollDir.Left)
                        {
                            offset.X -= 1.0f;
                        }
                        if (_scrollDir.Right)
                        {
                            offset.X += 1.0f;
                        }

                        offset *= speedFactor * ScrollAmount * keyboardScrollSpeedFactor;
                        break;
                    }

                case ScrollType.ScreenEdge:
                    {
                        var screenSize = _tacticalView.Camera.ScreenSize;

                        if (_currentPos.Y < EdgeScrollSize)
                        {
                            offset.Y = -1.0f;
                        }
                        else if (_currentPos.Y > screenSize.Y - EdgeScrollSize)
                        {
                            offset.Y = 1.0f;
                        }
                        if (_currentPos.X < EdgeScrollSize)
                        {
                            offset.X = -1.0f;
                        }
                        else if (_currentPos.X > screenSize.X - EdgeScrollSize)
                        {
                            offset.X = 1.0f;
                        }

                        offset *= speedFactor * ScrollAmount * keyboardScrollSpeedFactor;
                        break;
                    }
                default:
                    break;
            }

            // TODO(Port): TheInGameUI->setScrollAmount(offset);
            _tacticalView.ScrollBy(offset, gameTime);
        }
        else
        {
            // TODO(Port): TheInGameUI->setScrollAmount(offset); <-- with zero offset
        }

        // TODO(Port): Replay stuff
    }

    private struct ScrollDirState
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
    }
}

public enum ScrollType
{
    None,
    RightMouseButton,
    Key,
    ScreenEdge
}
