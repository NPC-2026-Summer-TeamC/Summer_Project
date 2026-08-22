using System.Collections.Generic;

public class PuzzleAction
{
    public List<TileAction> tileActions = new();

    public void AddTileAction(TileAction tileAction)
    {
        tileActions.Add(tileAction);
    }
}
