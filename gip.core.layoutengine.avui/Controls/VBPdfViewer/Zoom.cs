using System;

namespace gip.core.layoutengine.avui;

public readonly struct PdfZoom : IComparable, IComparable<PdfZoom>, IEquatable<PdfZoom>
{
    private readonly double _value;
    private readonly string _name;

    public static PdfZoom Automatic => new PdfZoom(0, "Automatic");

    private PdfZoom(double value, string name)
    {
        _value = value;
        _name = name;
    }

    public int CompareTo(PdfZoom other)
    {
        return _value.CompareTo(other._value);
    }

    public override string ToString()
    {
        return _name;
    }

    public int CompareTo(object obj)
    {
        if (obj is PdfZoom z)
        {
            return CompareTo(z);
        }
        return -1;
    }


    public static implicit operator PdfZoom(double d) => new(d, $"{d*100}%");
    public static implicit operator double(PdfZoom z) => z._value;

    public bool Equals(PdfZoom other)
    {
        return _value.Equals(other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is PdfZoom other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}