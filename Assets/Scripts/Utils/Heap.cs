using System;
using System.Collections.Generic;

sealed class Heap<T>
{
    private readonly IComparer<T> comparer;
    private readonly List<T> list = new List<T> { default };

    public Heap() : this(default(IComparer<T>)) { }

    public Heap(IComparer<T> comparer) {
        this.comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(Comparison<T> comparison) : this(Comparer<T>.Create(comparison)) { }

    public int Count => list.Count - 1;

    public void Push(T element) {
        list.Add(element);
        SiftUp(list.Count - 1);
    }

    public T Pop() {
        T result = list[1];
        list[1] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        SiftDown(1);
        return result;
    }

    private static int Parent(int i) => i / 2;
    private static int Left(int i) => i * 2;
    private static int Right(int i) => i * 2 + 1;

    private void SiftUp(int i) {
        while (i > 1) {
            int parent = Parent(i);
            if (comparer.Compare(list[i], list[parent]) > 0) return;
            (list[parent], list[i]) = (list[i], list[parent]);
            i = parent;
        }
    }

    private void SiftDown(int i) {
        for (int left = Left(i); left < list.Count; left = Left(i)) {
            int smallest = comparer.Compare(list[left], list[i]) <= 0 ? left : i;
            int right = Right(i);
            if (right < list.Count && comparer.Compare(list[right], list[smallest]) <= 0) smallest = right;
            if (smallest == i) return;
            (list[i], list[smallest]) = (list[smallest], list[i]);
            i = smallest;
        }
    }

}