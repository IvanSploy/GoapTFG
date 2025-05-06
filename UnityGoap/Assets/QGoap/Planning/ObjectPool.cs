using System;
using System.Collections.Generic;

namespace QGoap.Planning
{
    public class ObjectPool<TObject>
    {
        private readonly Queue<TObject> _pool = new();
        private readonly Func<TObject> _createObject;

        public ObjectPool(Func<TObject> createFunc)
        {
            _createObject = createFunc;
        }

        public TObject Get()
        {
            lock (_pool)
            {
                if (_pool.Count > 0) return _pool.Dequeue();
                return _createObject();
            }
        }

        public void Release(TObject o)
        {
            lock (_pool)
            {
                _pool.Enqueue(o);
            }
        }
    }
}