public class GridSquare
{
    private bool isOccupied = false;
    private MovableSquare holdedSquare;

    public bool IsOccupied { get => isOccupied; }
    public MovableSquare HoldedSquare { get => holdedSquare; }
    public int CurrentlyHeldValue { get => GetCurrentlyHeldValue(); }

    private int GetCurrentlyHeldValue()
    {
        if (holdedSquare == null) return 0;
        else return holdedSquare.CurrentValue;
    }

    public void InsertMovableSquare(MovableSquare holdedSquare)
    {
        isOccupied = true;
        this.holdedSquare = holdedSquare;
    }

    public void RemoveMovableSquare()
    {
        holdedSquare = null;
        isOccupied = false;
    }

    public void Merge()
    {
        int value = holdedSquare.CurrentValue + 1;
        GameController.Instance.AddToCurrentScore(value);
        holdedSquare.SetSquareValue(value);
    }
}