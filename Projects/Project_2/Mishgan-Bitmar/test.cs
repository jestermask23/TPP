public interface ICondition
{
    bool BabaMet(GameState state);
}
public abstract class ConditionBase : ICondition
{
    public abstract bool BabaMet(GameState state);
}


public class GameState 
{
    public int Health { get; set; } = 100;
    public List<string> Inventory { get; } = new List<string>();
    public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>();
    public List<string> Log { get; } = new List<string>();

    public void AddLog(string message) => Log.Add(message);
    public void ChangeHealth(int amount) => Health += amount;
}

public abstract class GameEventBase 
{
    protected ICondition condition;
    protected List<IEffect> effects;
    public bool IsOneTime { get; protected set; }

    public abstract void Update(GameState state);
}

public class Location
{
    public string Name { get; }
    public string Description { get; private set; }

    public List<IInteractable> Interactables { get; } = new List<IInteractable>();

    public List<GameEventBase> Events { get; } = new List<GameEventBase>();

    public Dictionary<string, Location> Exits { get; } = new Dictionary<string, Location>();

    public Location(string name, string description)
    {
        Name = name;
        Description = description;
    }


    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
    }

}


public abstract class CommandBase : ICommand
{
    public abstract string Name { get; }

    public abstract void Execute(GameState state, string[] args);
}