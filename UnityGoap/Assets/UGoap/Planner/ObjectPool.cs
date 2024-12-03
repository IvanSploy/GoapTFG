using System;
using System.Collections.Generic;

namespace UGoap.Planner
{
    public class ObjectPool<TObject>
    {
        public Queue<TObject> _pool = new();

        private Func<TObject> _createObject;

        public ObjectPool(Func<TObject> createFunc)
        {
            _createObject = createFunc;
        }

        public TObject Get()
        {
            if (_pool.Count > 0) return _pool.Dequeue();
            return _createObject();
        }

        public void Release(TObject o)
        {
            _pool.Enqueue(o);
        }
    }
}