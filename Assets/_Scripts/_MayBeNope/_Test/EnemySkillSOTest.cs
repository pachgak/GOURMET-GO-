using System.Collections.Generic;
using UnityEngine;
using static ExMono;

[CreateAssetMenu(menuName = "_Tes/Skills/New Modern Skill")]
public class EnemySkillSOTest : ScriptableObject
{
    [SerializeReference,SubclassSelector] public List<SkillActionTest> actions = new List<SkillActionTest>();
} 