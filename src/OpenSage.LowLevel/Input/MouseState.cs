namespace LL.Input
{
    public struct MouseState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ScrollWheelValue { get; set; }
        public ButtonState LeftButton { get; set; }
        public ButtonState MiddleButton { get; set; }
        public ButtonState RightButton { get; set; }
        public ButtonState XButton1 { get; set; }
        public ButtonState XButton2 { get; set; }

        public MouseState(
            int x,
            int y,
            int scrollWheel,
            ButtonState leftButton,
            ButtonState middleButton,
            ButtonState rightButton,
            ButtonState xButton1,
            ButtonState xButton2)
        {
            X = x;
            Y = y;
            ScrollWheelValue = scrollWheel;
            LeftButton = leftButton;
            MiddleButton = middleButton;
            RightButton = rightButton;
            XButton1 = xButton1;
            XButton2 = xButton2;
        }
    }
}
