using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Interfaces
{
    public interface IBehaviour
    {
        // методы расположены в порядке выполнения

        /// <summary>
        /// Выполняется после всех действий в конструкторе.
        /// </summary>
        void Start();
        /// <summary>
        /// Выполняется непосредственно перед методом Update.
        /// </summary>
        void PreUpdate();
        /// <summary>
        /// Выполняется каждый кадр после метода MapController.PreUpdate, но перед удалением объекта.
        /// </summary>
        void Update();
        /// <summary>
        /// Выполняется непосредственно после метода Update.
        /// </summary>
        void PostUpdate();
    }
}