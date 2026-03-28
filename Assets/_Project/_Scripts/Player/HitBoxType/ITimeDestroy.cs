using UnityEngine;

public interface ITimeDestroy
{
    float _lifeTime { get; set; }

    void StartLifeTime();
}