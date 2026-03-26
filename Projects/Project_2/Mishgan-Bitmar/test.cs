using System;
using System.Collections.Generic;

namespace MishganBeatmaker
{

    public interface ICondition 
    { 
        bool IsMet(GameState state);
    }

    public interface IEffect 
    { 
        void Apply(GameState state);
    }

    public interface IInteractable 
    { 
        string Id { get; } 
        void Interact(GameState state);
    }

    public interface ICommand 
    { 
        string Name { get; }
        void Execute(GameState state, string[] args); 
    }

    public abstract class CommandBase : ICommand 
    {
        public abstract string Name { get; }
        public abstract void Execute(GameState state, string[] args);
    }

    public abstract class ConditionBase : ICondition 
    {
        public abstract bool IsMet(GameState state);
    }

    public abstract class EffectBase : IEffect 
    {
        public abstract void Apply(GameState state);
    }

    public class GameState 
    {
        public int Health { get; set; } = 100;
        public List<string> Inventory { get; } = new List<string>();
        public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>();
        public Location CurrentLocation { get; set; }
        public List<string> Journal { get; } = new List<string>();

        public void AddLog(string message) => Journal.Add(message);
    }


    public class Location 
    {
        public string Name { get; }
        public string Description { get; }
        public List<IInteractable> Interactables { get; } = new List<IInteractable>();
        public Dictionary<string, Location> Exits { get; } = new Dictionary<string, Location>();

        public Location(string name, string desc) { Name = name; Description = desc; }
    }

    public class HasItemCondition : ConditionBase 
    {
        private string _itemName;
        public HasItemCondition(string item) => _itemName = item;
        public override bool IsMet(GameState state) => state.Inventory.Contains(_itemName);
    }

    public class HealthCondition : ConditionBase 
    {
        private int _threshold;
        public HealthCondition(int threshold) => _threshold = threshold;
        public override bool IsMet(GameState state) => state.Health <= _threshold;
    }

    public class DamageEffect : EffectBase 
    {
        private int _amount;
        public DamageEffect(int amount) => _amount = amount;
        public override void Apply(GameState state) 
        {
            state.Health -= _amount;
            state.AddLog($"Мишган получил {_amount} урона. Состояние: {state.Health}%");
        }
    }

    public class AddItemEffect : EffectBase 
    {
        private string _item;
        public AddItemEffect(string item) => _item = item;
        public override void Apply(GameState state) 
        {
            state.Inventory.Add(_item);
            state.AddLog($"В инвентарь добавлено: {_item}");
        }
    }

    public class Trap : IInteractable
    {
        public string Id => "plugin";
        public void Interact(GameState state) 
        {
            state.AddLog("Ты открыл архив с плагином, но это был вирус!");
            new DamageEffect(20).Apply(state);
        }
    }

    public class NPC : IInteractable
    {
        public string Id { get; }
        private string _dialogue;
        public NPC(string id, string dialogue) { Id = id; _dialogue = dialogue; }
        public void Interact(GameState state) => state.AddLog($"{Id}: {_dialogue}");
    }

    public class LookCommand : CommandBase 
    {
        public override string Name => "look";
        public override void Execute(GameState state, string[] args) 
        {
            state.AddLog($"--- {state.CurrentLocation.Name} ---");
            state.AddLog(state.CurrentLocation.Description);
        }
    }

    public class InventoryCommand : CommandBase 
    {
        public override string Name => "inv";
        public override void Execute(GameState state, string[] args) 
        {
            state.AddLog("В карманах: " + string.Join(", ", state.Inventory));
        }
    }
}


public abstract class CommandBase : ICommand
{
    public abstract string Name { get; }

    public abstract void Execute(GameState state, string[] args);
}