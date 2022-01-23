using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Containers
{

    public interface IPool<T>
    {
        T Reserve();
        void Release(T item);
        void Clear();
    }
}