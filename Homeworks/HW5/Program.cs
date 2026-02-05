using System;

// Интерфейс получения урона
interface IDamageable
{
    void TakeDamage(int damage);
}

// Абстрактный класс персонажа
abstract class Character : IDamageable
{
    private string name;
    private int health;

    public string Name
    {
        get { return name; }
    }

    public int Health
    {
        get { return health; }
    }

    protected Character(string name, int health)
    {
        this.name = name;
        this.health = health;
    }

    public abstract void Attack();

    public void Move()
    {
        Console.WriteLine($"{name} перемещается.");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Console.WriteLine($"{name} получил {damage} урона. Осталось здоровья: {health}");
    }
}

// Воин
class Warrior : Character
{
    public Warrior(string name, int health)
        : base(name, health)
    {
    }

    public override void Attack()
    {
        Console.WriteLine($"{Name} наносит удар мечом!");
    }
}

// Маг
class Mage : Character
{
    public Mage(string name, int health)
        : base(name, health)
    {
    }

    public override void Attack()
    {
        Console.WriteLine($"{Name} бросает огненный шар!");
    }
}

// Лучник
class Archer : Character
{
    public Archer(string name, int health)
        : base(name, health)
    {
    }

    public override void Attack()
    {
        Console.WriteLine($"{Name} выпускает стрелу из лука!");
    }
}

// Точка входа
class Program
{
    static void Main()
    {
        Character[] characters = new Character[]
        {
            new Warrior("Артур", 100),
            new Mage("Вова", 80),
            new Archer("Робин", 90)
        };

        foreach (Character character in characters)
        {
            character.Attack();
            character.Move();
            character.TakeDamage(10);
            Console.WriteLine();
        }
    }
}
