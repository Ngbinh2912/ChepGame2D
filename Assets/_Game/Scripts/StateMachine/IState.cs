using UnityEngine;
using System.Collections.Generic;

public interface IState
{
    void OnEnter(Enemy enemy);

    void OnExecute(Enemy enemy);

    void OnExit(Enemy enemy);
}
