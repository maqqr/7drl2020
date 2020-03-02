using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator.GameViews
{
    public interface IGameView
    {
        void Initialize(GameManager gameManager);
        void Destroy();
        void OpenView();
        void CloseView();
        bool UpdateView();
    }
}
