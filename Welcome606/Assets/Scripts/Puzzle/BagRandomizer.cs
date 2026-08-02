using System.Collections.Generic;
using UnityEngine;

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

    // Bag 초기화
    private void InitializeBag()
    {
        colorBag.Clear();

        colorBag.AddRange(allColors);

        ShuffleBag();
    }

    // Bag 셔플
    private void ShuffleBag()
    {
        for (int i = colorBag.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            (colorBag[i], colorBag[randomIndex]) =
                (colorBag[randomIndex], colorBag[i]);
        }
    }

    // Bag 재생성
    private void RefillBag()
    {
        InitializeBag();
    }

    // 다음 색상 반환
    public TileColor GetNextColor()
    {
        if (colorBag.Count == 0)
        {
            RefillBag();
        }

        TileColor nextColor = colorBag[0];

        colorBag.RemoveAt(0);

        return nextColor;
    }
}
