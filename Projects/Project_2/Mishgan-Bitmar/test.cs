using System;
using System.Collections.Generic;
using System.Linq;

namespace MishganBeatmaker
{
    // Интерфейсы
    public interface ICondition { bool IsMet(GameState state); }
    public interface IEffect { void Apply(GameState state); }
    public interface IInteractable { string Id { get; } void Interact(GameState state); }
    public interface ICommand { string Name { get; } void Execute(GameState state, string[] args); }

    // База
    public abstract class ConditionBase : ICondition { public abstract bool IsMet(GameState state); }
    public abstract class EffectBase : IEffect { public abstract void Apply(GameState state); }
    public abstract class CommandBase : ICommand
    {
        public abstract string Name { get; }
        public abstract void Execute(GameState state, string[] args);
    }

    public abstract class GameEventBase
    {
        public ICondition Condition { get; protected set; } = null!;
        public List<IEffect> Effects { get; protected set; } = new List<IEffect>();
        public bool IsOneTime { get; protected set; }
        public bool HasTriggered { get; set; }

        public void TryTrigger(GameState state)
        {
            if (IsOneTime && HasTriggered) return;
            if (Condition.IsMet(state))
            {
                foreach (var effect in Effects) effect.Apply(state);
                HasTriggered = true;
            }
        }
    }

    // Состояние и квесты
    public class Quest
    {
        public string Name { get; }
        public string Description { get; }
        public ICondition CompletionCondition { get; }
        public bool IsCompleted { get; private set; }

        public Quest(string name, string desc, ICondition condition)
        {
            Name = name; Description = desc; CompletionCondition = condition;
        }

        public void CheckUpdate(GameState state)
        {
            if (!IsCompleted && CompletionCondition.IsMet(state))
            {
                IsCompleted = true;
                state.AddLog($"[КВЕСТ ВЫПОЛНЕН]: {Name}");
            }
        }
    }

    public class GameState
    {
        public int Health { get; set; } = 100;
        public List<string> Inventory { get; } = new List<string> { "Телефон" };
        public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>();
        public Location CurrentLocation { get; set; } = null!;
        public List<string> Journal { get; } = new List<string>();
        public List<Quest> Quests { get; } = new List<Quest>();
        public bool IsGameOver { get; set; }
        public int Turns { get; set; } = 0;

        public void AddLog(string message)
        {
            Journal.Add(message);
            Console.WriteLine($"> {message}");
        }

        public string GetHealthStatus()
        {
            if (Health > 70) return "как огурчик";
            if (Health > 30) return "нормалды";
            return "при смерти";
        }
    }

    public class Location
    {
        public string Name { get; }
        public string Description { get; }
        public List<IInteractable> Interactables { get; } = new List<IInteractable>();
        public Dictionary<string, Location> Exits { get; } = new Dictionary<string, Location>();
        public List<GameEventBase> Events { get; } = new List<GameEventBase>();

        public Location(string name, string desc) { Name = name; Description = desc; }
    }

    // Условия
    public class HasItemCondition : ConditionBase
    {
        private string _item;
        public HasItemCondition(string item) => _item = item;
        public override bool IsMet(GameState state) => state.Inventory.Contains(_item);
    }

    public class FlagCondition : ConditionBase
    {
        private string _flag;
        private bool _value;
        public FlagCondition(string flag, bool value = true) { _flag = flag; _value = value; }
        public override bool IsMet(GameState state) => state.Flags.ContainsKey(_flag) && state.Flags[_flag] == _value;
    }

    public class LocationCondition : ConditionBase
    {
        private string _locName;
        public LocationCondition(string locName) => _locName = locName;
        public override bool IsMet(GameState state) => state.CurrentLocation.Name == _locName;
    }

    // И
    public class AndCondition : ConditionBase
    {
        private ICondition _c1, _c2;
        public AndCondition(ICondition c1, ICondition c2) { _c1 = c1; _c2 = c2; }
        public override bool IsMet(GameState state) => _c1.IsMet(state) && _c2.IsMet(state);
    }

    // НЕ
    public class NotCondition : ConditionBase
    {
        private ICondition _c;
        public NotCondition(ICondition c) => _c = c;
        public override bool IsMet(GameState state) => !_c.IsMet(state);
    }

    // Эффекты
    public class DamageEffect : EffectBase
    {
        private int _amount;
        public DamageEffect(int amount) => _amount = amount;
        public override void Apply(GameState state)
        {
            state.Health -= _amount;
            state.AddLog($"Получено {_amount} урона! Состояние: {state.GetHealthStatus()} ({state.Health} HP).");
        }
    }

    public class AddItemEffect : EffectBase
    {
        private string _item;
        public AddItemEffect(string item) => _item = item;
        public override void Apply(GameState state)
        {
            if (!state.Inventory.Contains(_item))
            {
                state.Inventory.Add(_item);
                state.AddLog($"Получен предмет: {_item}");
            }
        }
    }

    public class RemoveItemEffect : EffectBase
    {
        private string _item;
        public RemoveItemEffect(string item) => _item = item;
        public override void Apply(GameState state)
        {
            if (state.Inventory.Remove(_item)) state.AddLog($"Предмет {_item} потерян/использован.");
        }
    }

    public class SetFlagEffect : EffectBase
    {
        private string _flag;
        private bool _value;
        public SetFlagEffect(string flag, bool value) { _flag = flag; _value = value; }
        public override void Apply(GameState state) => state.Flags[_flag] = _value;
    }

    public class AddExitEffect : EffectBase
    {
        private string _direction;
        private Location _target;
        public AddExitEffect(string dir, Location target) { _direction = dir; _target = target; }
        public override void Apply(GameState state)
        {
            if (!state.CurrentLocation.Exits.ContainsKey(_direction))
            {
                state.CurrentLocation.Exits.Add(_direction, _target);
                state.AddLog($"Открылся новый путь: {_direction}!");
            }
        }
    }

    public class LogEffect : EffectBase
    {
        private string _msg;
        public LogEffect(string msg) => _msg = msg;
        public override void Apply(GameState state) => state.AddLog(_msg);
    }

    // События
    public class TurnEvent : GameEventBase
    {
        public TurnEvent(ICondition condition, IEffect effect, bool isOneTime = false)
        {
            Condition = condition;
            Effects.Add(effect);
            IsOneTime = isOneTime;
        }
    }

    // Объекты

    // Заначка
    public class Chest : IInteractable
    {
        public string Id { get; }
        private List<IEffect> _effects = new List<IEffect>();
        private bool _opened = false;

        public Chest(string id, IEffect effect) { Id = id; _effects.Add(effect); }
        public void Interact(GameState state)
        {
            if (!_opened)
            {
                state.AddLog("Ты осмотрел заначку...");
                foreach (var e in _effects) e.Apply(state);
                _opened = true;
            }
            else state.AddLog("Тут больше ничего нет.");
        }
    }

    // Охранник/дверь
    public class Door : IInteractable
    {
        public string Id { get; }
        private ICondition _unlockCond;
        private List<IEffect> _unlockEffects = new List<IEffect>();

        public Door(string id, ICondition cond, List<IEffect> effects)
        {
            Id = id; _unlockCond = cond; _unlockEffects = effects;
        }

        public void Interact(GameState state)
        {
            if (_unlockCond.IsMet(state))
            {
                state.AddLog("Охранник кивнул и пропустил тебя.");
                foreach (var e in _unlockEffects) e.Apply(state);
            }
            else state.AddLog("Охранник: 'Сюда только с деньгами, пацан. Вали.'");
        }
    }

    // NPC
    public class NPC : IInteractable
    {
        public string Id { get; }
        public NPC(string id = "mellstroy")
        {
            Id = id;
        }

        public void Interact(GameState state)
        {
            if (state.Health < 50)
            {
                state.AddLog("Меллстрой: 'Бро, ты выглядишь при смерти. На, держи аптечку.'");
                state.Health += 30;
                state.AddLog($"Твое здоровье восстановлено. Сейчас ты {state.GetHealthStatus()}.");
            }
            else
            {
                state.AddLog("Меллстрой: 'Нормалды выглядишь! Держи минус для бита, это вдохновение!'");
                new AddItemEffect("Вдохновение").Apply(state);
            }
        }
    }

    // Пульт (финал)
    public class MixerTerminal : IInteractable
    {
        public string Id => "mixer";
        public void Interact(GameState state)
        {
            var hasInspiration = new HasItemCondition("Вдохновение").IsMet(state);
            
            if (hasInspiration)
            {
                state.AddLog("Ты загрузил сэмплы в микшер и написал ЛЮТЫЙ ХИТ!");
                state.Flags["win"] = true;
                state.IsGameOver = true;
            }
            else
            {
                state.AddLog("Ты сел за пульт, но в голове пусто. Нужно найти Вдохновение у кента.");
            }
        }
    }

    // Ловушка
    public class Trap : IInteractable
    {
        public string Id => "pc";
        private bool _triggered = false;
        public void Interact(GameState state)
        {
            if (!_triggered)
            {
                state.AddLog("Ты попытался скачать плагин, но поймал вирус! Проект стерт, у тебя инфаркт.");
                new DamageEffect(30).Apply(state);
                _triggered = true;
            }
            else state.AddLog("Этот комп уже мертв.");
        }
    }

    // Команды
    public class GoCommand : CommandBase
    {
        public override string Name => "go";
        public override void Execute(GameState state, string[] args)
        {
            if (args.Length == 0) { state.AddLog("Куда идти? Укажи направление."); return; }
            if (state.CurrentLocation.Exits.TryGetValue(args[0], out var next))
            {
                state.CurrentLocation = next;
                state.AddLog($"--- {next.Name} ---");
                state.AddLog(next.Description);
            }
            else state.AddLog("Туда не пройти.");
        }
    }

    public class InteractCommand : CommandBase
    {
        public override string Name => "interact";
        public override void Execute(GameState state, string[] args)
        {
            if (args.Length == 0) { state.AddLog("С чем взаимодействовать?"); return; }
            var obj = state.CurrentLocation.Interactables.Find(i => i.Id == args[0]);
            if (obj != null) obj.Interact(state);
            else state.AddLog("Здесь такого нет.");
        }
    }

    public class LookCommand : CommandBase
    {
        public override string Name => "look";
        public override void Execute(GameState state, string[] args)
        {
            state.AddLog(state.CurrentLocation.Description);
            var items = string.Join(", ", state.CurrentLocation.Interactables.Select(i => i.Id));
            if (!string.IsNullOrEmpty(items)) state.AddLog("Тут есть: " + items);
            state.AddLog("Выходы: " + string.Join(", ", state.CurrentLocation.Exits.Keys));
        }
    }

    public class InvCommand : CommandBase
    {
        public override string Name => "inv";
        public override void Execute(GameState state, string[] args) => 
            state.AddLog("Инвентарь: " + (state.Inventory.Count > 0 ? string.Join(", ", state.Inventory) : "пусто"));
    }

    public class StatusCommand : CommandBase
    {
        public override string Name => "status";
        public override void Execute(GameState state, string[] args)
        {
            state.AddLog($"Здоровье: {state.Health} ({state.GetHealthStatus()}).");
            state.AddLog("Квесты:");
            foreach (var q in state.Quests) 
                state.AddLog($"- {q.Name}: {(q.IsCompleted ? "[Выполнено]" : "[В процессе]")}");
        }
    }

    public class HelpCommand : CommandBase
    {
        public override string Name => "help";
        public override void Execute(GameState state, string[] args) =>
            state.AddLog("Команды: help, look, go [направление], interact [объект], inv, status.");
    }

    // Старт игры
    class Program
    {
        static void Main(string[] args)
        {
            // Локации
            var state = new GameState();
            var home = new Location("Дом", "Твоя хата. Безопасное место. Выход: block.");
            var block = new Location("Блок", "Опасный район. Здесь можно найти проблемы или деньги. Выходы: home, friend.");
            var friendHome = new Location("Дом кента", "Хата Меллстроя. Тут можно почилить. Выход: block.");
            var studio = new Location("Студос", "Место, где делаются хиты. Доступ к пульту открыт!");

            home.Exits["block"] = block;
            block.Exits["home"] = home;
            block.Exits["friend"] = friendHome;
            friendHome.Exits["block"] = block;
            studio.Exits["block"] = block;

            // Объекты
            block.Interactables.Add(new Chest("pacany", new AddItemEffect("Пачка денег")));
            block.Interactables.Add(new Door("guard", 
                new HasItemCondition("Пачка денег"), 
                new List<IEffect> { 
                    new RemoveItemEffect("Пачка денег"), 
                    new AddExitEffect("studio", studio) 
                }));
            
            friendHome.Interactables.Add(new NPC("mellstroy"));

            studio.Interactables.Add(new Trap());
            studio.Interactables.Add(new MixerTerminal());

            // На блоке больно без уверенности
            var blockDamageEvent = new TurnEvent(
                new AndCondition(new LocationCondition("Блок"), new NotCondition(new FlagCondition("уверенность"))),
                new DamageEffect(10),
                false // Срабатывает каждый ход!
            );
            block.Events.Add(blockDamageEvent);

            // Квесты
            state.Quests.Add(new Quest("Стартовый капитал", "Добудь деньги на Блоке и вернись домой.", 
                new AndCondition(new HasItemCondition("Пачка денег"), new LocationCondition("Дом"))));
            
            state.Quests.Add(new Quest("Финальный дроп", "Запиши бит в Студии.", 
                new FlagCondition("win")));

            // Команды
            var commands = new List<ICommand> { new GoCommand(), new InteractCommand(), new LookCommand(), new InvCommand(), new StatusCommand(), new HelpCommand() };
            state.CurrentLocation = home;

            Console.WriteLine("=== МИШГАН-БИТМАРЬ ===");
            commands.Find(c => c.Name == "look")?.Execute(state, new string[0]);

            while (!state.IsGameOver && state.Health > 0)
            {
                state.Turns++;
                Console.Write("\n> Введи команду: ");
                var inputLine = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(inputLine)) continue;

                var input = inputLine.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var cmd = commands.Find(c => c.Name == input[0]);
                if (cmd != null)
                {
                    cmd.Execute(state, input.Skip(1).ToArray());
                }
                else
                {
                    Console.WriteLine("> Неизвестная команда. Напиши help.");
                }

                // События локации
                foreach (var ev in state.CurrentLocation.Events)
                {
                    ev.TryTrigger(state);
                }

                // Квесты
                foreach (var q in state.Quests)
                {
                    q.CheckUpdate(state);
                }
            }

            if (state.Health <= 0) Console.WriteLine("\n[ИГРА ОКОНЧЕНА] Мишган остался в низах...");
            else if (state.Flags.ContainsKey("win")) Console.WriteLine("\n[ПОБЕДА!] Мишган добился успеха!");
            Console.ReadLine();
        }
    }
}