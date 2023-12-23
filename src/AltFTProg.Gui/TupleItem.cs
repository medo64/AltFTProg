namespace AltFTProgGui;
using System;

internal readonly struct TupleItem<T>(string title, T value) where T : struct {

    public string Title { get; } = title;
    public T Value { get; } = value;

    public override readonly string ToString() {
        return Title;
    }

}