using System;
using System.Collections.Generic;

namespace MyOTKE.BufferManagement;

public interface IListBufferItem<TVertex> : IDisposable
{
    void Set(IList<TVertex> vertices);
}
