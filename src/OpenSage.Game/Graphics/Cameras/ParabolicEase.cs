
using System;
using System.Diagnostics;

namespace OpenSage.Graphics.Cameras;

// C++: /GameClient/ParabolicEase.cpp/h
public class ParabolicEase
{
    private float _in;
    private float _out;

    public ParabolicEase(float easeInTime, float easeOutTime)
    {
        SetEaseTimes(easeInTime, easeOutTime);
    }

    public void SetEaseTimes(float easeInTime, float easeOutTime)
    {
        _in = easeInTime;

        if (_in < 0.0f || _in > 1.0f)
        {
            Debug.Assert(false, "Invalid ease in time");
            _in = Math.Clamp(_in, 0.0f, 1.0f);
        }

        _out = 1.0f - easeOutTime;

        if (_out < 0.0f || _out > 1.0f)
        {
            Debug.Assert(false, "Invalid ease out time");
            _out = Math.Clamp(_out, 0.0f, 1.0f);
        }

        if (_in > _out)
        {
            Debug.Assert(false, "Ease-in and ease-out times overlap");
            _in = _out;
        }
    }

    public float Evaluate(float param)
    {
        if (param < 0.0f || param > 1.0f)
        {
            Debug.Assert(false, "Param out of range");
            param = Math.Clamp(param, 0.0f, 1.0f);
        }

        var v0 = 1.0f + _out - _in;
        if (param < _in)
        {
            return param * param / (v0 * _in);
        }
        else if (param <= _out)
        {
            return (_in + 2.0f * (param - _in)) / v0;
        }
        else
        {
            return (_in + 2.0f * (_out - _in) + (2.0f * (param - _out) + _out * _out - param * param) / (1.0f - _out)) / v0;
        }
    }

    public static ParabolicEase CreateLinear()
    {
        return new ParabolicEase(0.0f, 0.0f);
    }
}
