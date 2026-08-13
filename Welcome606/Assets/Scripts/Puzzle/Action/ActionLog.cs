using System.Collections.Generic;

public class ActionLog
{
    private readonly List<PuzzleAction> actions = new();

    private int currentIndex = -1;

    // 새로운 Action 기록
    public void Record(PuzzleAction action)
    {
        // 현재 위치보다 뒤에 있는 Redo 기록 제거
        if (currentIndex < actions.Count - 1)
        {
            actions.RemoveRange(
                currentIndex + 1,
                actions.Count - currentIndex - 1);
        }

        actions.Add(action);

        currentIndex = actions.Count - 1;
    }

    // Undo 가능한지 확인
    public bool CanUndo()
    {
        return currentIndex >= 0;
    }

    // Redo 가능한지 확인
    public bool CanRedo()
    {
        return currentIndex < actions.Count - 1;
    }

    // Undo할 Action 반환
    public PuzzleAction Undo()
    {
        if (!CanUndo())
        {
            return null;
        }

        PuzzleAction action = actions[currentIndex];

        currentIndex--;

        return action;
    }

    // Redo할 Action 반환
    public PuzzleAction Redo()
    {
        if (!CanRedo())
        {
            return null;
        }

        currentIndex++;

        return actions[currentIndex];
    }
}
