
using UnityEngine;

[CreateAssetMenu(fileName = "New Team Banner", menuName = "TeamBanner")]
public class BannerSO : ScriptableObject
{
    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }
}
