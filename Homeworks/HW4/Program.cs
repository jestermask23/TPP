using System;


class Student
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Group { get; set; }

    public Student(string name, int age, string group)
    {
        Name = name;
        Age = age;
        Group = group;
    }

    public void Study()
    {
        Console.WriteLine($"Студент по имени {Name}, которому {Age} лет, учится в группе {Group}.");
    }
}


class Master : Student
{
    public Master(string name, int age, string group) : base(name, age, group) { }

    public void DefendThesis()
    {
        Console.WriteLine($"{Name} готовится к защите диплома и проводит научное исследование.");
    }
}


class Bachelor : Student
{
    public Bachelor(string name, int age, string group) : base(name, age, group) { }

    public void PassExams()
    {
        Console.WriteLine($"{Name} сдаёт итоговые экзамены по выбранным дисциплинам.");
    }
}

class Program
{
    static void Main()
    {
        Student student = new Student("Иван", 19, "ИТ-101");
        student.Study();

        Master master = new Master("Анна", 24, "МТ-202");
        master.Study();
        master.DefendThesis();

        Bachelor bachelor = new Bachelor("Пётр", 21, "БК-303");
        bachelor.Study();
        bachelor.PassExams();
    }
}