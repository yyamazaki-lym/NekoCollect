using UnityEngine;

namespace NekoCollect.Data
{
    /// <summary>
    /// スキルの定義データ
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "NekoCollect/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string skillName;
        [TextArea] public string description;

        [Header("効果")]
        public SkillEffectType effectType;
        /// <summary>効果値（%またはフラット値）</summary>
        public float effectValue;

        [Header("習得条件")]
        /// <summary>習得に必要なレベル</summary>
        public int requiredLevel;
    }
}
