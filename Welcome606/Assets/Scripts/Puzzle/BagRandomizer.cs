using System.Collections.Generic;

public class BagRandomizer
{
    // Bag 생성에 사용할 전체 색상 목록
    private readonly List<TileColor> allColors = new()
    {
        TileColor.DarkMutedRed,
        TileColor.EmeraldMeadow,
        TileColor.ModerateOrange,
        TileColor.BlueGray,
        TileColor.BrownTerracotta,
        TileColor.MutedPurple,
        TileColor.DustyRose,
        TileColor.OliveBrown,

        TileColor.SoftYellow,
        TileColor.DeepTeal,
        TileColor.Lavender,
        TileColor.CoralPink,
        TileColor.SageGreen,
        TileColor.MintGreen,
        TileColor.PlumPurple,
        TileColor.WarmBeige
    };

    // 현재 사용할 색상 Bag
    private readonly List<TileColor> colorBag = new();

    public BagRandomizer()
    {
        InitializeBag();
    }

    // Bag 생성 및 초기화
    private void InitializeBag()
    {
        colorBag.Clear();

        colorBag.AddRange(allColors);

        // TODO : Bag 셔플
    }
}
