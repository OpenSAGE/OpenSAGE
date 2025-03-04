﻿using Veldrid;

namespace OpenSage.Gui.Apt;

public abstract class TexturedItem : DisplayItem
{
    public Texture Texture { get => _texture; set => DisposeAndAssign(ref _texture, value); }
    private Texture _texture;
}
