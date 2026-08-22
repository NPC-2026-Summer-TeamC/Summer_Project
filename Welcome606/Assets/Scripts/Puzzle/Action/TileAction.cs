public class TileAction
{
    public TileData tile;

    public RuntimeState previousState;
    public RuntimeState currentState;

    public TileAction(
        TileData tile,
        RuntimeState previousState,
        RuntimeState currentState)
    {
        this.tile = tile;
        this.previousState = previousState;
        this.currentState = currentState;
    }
}
