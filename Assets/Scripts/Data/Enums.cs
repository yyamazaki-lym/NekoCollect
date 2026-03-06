namespace NekoCollect.Data
{
    /// <summary>
    /// 猫のレアリティ
    /// </summary>
    public enum Rarity
    {
        N,
        R,
        SR,
        SSR
    }

    /// <summary>
    /// スキルの効果タイプ
    /// </summary>
    public enum SkillEffectType
    {
        /// <summary>放置コイン倍率アップ（%）</summary>
        IdleCoinBonus,
        /// <summary>クリックコイン増加（固定値）</summary>
        ClickCoinBonus,
        /// <summary>経験値獲得量アップ（%）</summary>
        ExpBonus
    }
}
