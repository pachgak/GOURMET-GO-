using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ExMono : MonoBehaviour
{
    [SubclassSelector]
    [SerializeReference] private List<A> myListofA;
    [SerializeReference, SubclassSelector] public List<SkillActionTest> actions = new List<SkillActionTest>();

    [SerializeReference, SubclassSelector] public List<IHitBox> hitBoxs = new List<IHitBox>();
    [SerializeReference, SubclassSelector] public List<ITakeDamage> takeDamages = new List<ITakeDamage>();

    [System.Serializable]
    public abstract class A
    {
        [SerializeField] public int intA;
    }


    [System.Serializable]
    public class B : A
    {
        [SerializeField] public float floatB;
    }
    [System.Serializable]
    public class C : A
    {
        [SerializeField] public string stringC;
    }


    //[ContextMenu("Add A")]
    //public void AddA()
    //{
    //    Debug.Log("ContextMenu B");

    //    //myListofA.Add(new A { intA = 10 });
    //}

    [ContextMenu("Add B")]
    public void AddB()
    {
        Debug.Log("ContextMenu B");

        myListofA.Add(new B { intA = 10 , floatB = 20f });
    }

    [ContextMenu("Add C")]
    public void AddC()
    {
        Debug.Log("ContextMenu C");

        myListofA.Add(new C { intA = 10 , stringC = ($"String_C") });
    }

}
