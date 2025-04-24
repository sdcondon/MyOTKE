using System;
using System.Collections.Generic;

namespace MyOTKE.BufferManagement;

public interface IListBufferItem<TVertex> : IDisposable
    where TVertex : struct
{
    ListBuffer<TVertex> Parent { get; }

    void Set(IList<TVertex> vertices);
}
