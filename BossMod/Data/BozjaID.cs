﻿using Lumina.Excel.Sheets;

namespace BossMod;

// elements of the bozja holster
public enum BozjaHolsterID : uint
{
    None,
    LostParalyze,
    LostBanish,
    LostManawall,
    LostDispel,
    LostStealth,
    LostSpellforge,
    LostSteelsting,
    LostSwift,
    LostProtect1,
    LostShell1,
    LostReflect,
    LostStoneskin1,
    LostBravery,
    LostFocus,
    LostFontOfMagic,
    LostFontOfSkill,
    LostFontOfPower,
    LostSlash,
    LostDeath,
    BannerNobleEnds,
    BannerHonoredSacrifice,
    BannerTirelessConviction,
    BannerFirmResolve,
    BannerSolemnClarity,
    BannerHonedAcuity,
    LostCure1,
    LostCure2,
    LostCure3,
    LostCure4,
    LostArise,
    LostIncense,
    LostFairTrade,
    Mimic,
    DynamisDice,
    ResistancePhoenix,
    ResistanceReraiser,
    ResistancePotionKit,
    ResistanceEtherKit,
    ResistanceMedikit,
    ResistancePotion,
    EssenceAetherweaver,
    EssenceMartialist,
    EssenceSavior,
    EssenceVeteran,
    EssencePlatebearer,
    EssenceGuardian,
    EssenceOrdained,
    EssenceSkirmisher,
    EssenceWatcher,
    EssenceProfane,
    EssenceIrregular,
    EssenceBreathtaker,
    EssenceBloodsucker,
    EssenceBeast,
    EssenceTemplar,
    DeepEssenceAetherweaver,
    DeepEssenceMartialist,
    DeepEssenceSavior,
    DeepEssenceVeteran,
    DeepEssencePlatebearer,
    DeepEssenceGuardian,
    DeepEssenceOrdained,
    DeepEssenceSkirmisher,
    DeepEssenceWatcher,
    DeepEssenceProfane,
    DeepEssenceIrregular,
    DeepEssenceBreathtaker,
    DeepEssenceBloodsucker,
    DeepEssenceBeast,
    DeepEssenceTemplar,
    LostPerception,
    LostSacrifice,
    PureEssenceGambler,
    PureEssenceElder,
    PureEssenceDuelist,
    PureEssenceFiendhunter,
    PureEssenceIndomitable,
    PureEssenceDivine,
    LostFlareStar,
    LostRendArmor,
    LostSeraphStrike,
    LostAethershield,
    LostDervish,
    Lodestone,
    LostStoneskin2,
    LostBurst,
    LostRampage,
    LightCurtain,
    LostReraise,
    LostChainspell,
    LostAssassination,
    LostProtect2,
    LostShell2,
    LostBubble,
    LostImpetus,
    LostExcellence,
    LostFullCure,
    LostBloodRage,
    ResistanceElixir,

    Count
}

// holster -> action id mapping
public static class BozjaActionID
{
    public static readonly ActionID SlotFromHolsterAction = new(ActionType.Spell, 21023u);

    private static readonly ActionID[] _normalActions = new ActionID[(int)BozjaHolsterID.Count];
    private static readonly ActionID[] _holsterActions = new ActionID[(int)BozjaHolsterID.Count];

    public static ActionID GetNormal(BozjaHolsterID id) => _normalActions[(int)id];
    public static ActionID GetHolster(BozjaHolsterID id) => _holsterActions[(int)id];

    static BozjaActionID()
    {
        var sheet = Service.LuminaSheet<MYCTemporaryItem>()!;
        var len = _normalActions.Length;
        for (var i = 0; i < len; ++i)
        {
            var row = sheet.GetRow((uint)i);
            _normalActions[i] = new(ActionType.Spell, row.Action.RowId);
            _holsterActions[i] = row.Type == 2 ? _normalActions[i] : SlotFromHolsterAction;
        }
    }
}
