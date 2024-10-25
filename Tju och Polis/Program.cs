using System;
using System.Collections.Generic;
using System.Threading;

public class Person
{
    public int X { get; set; }
    public int Y { get; set; }
    public int XDirection { get; set; }
    public int YDirection { get; set; }
    public string Symbol { get; protected set; }
    public List<string> Inventory { get; set; }

    protected static Random random = new Random();

    public Person()
    {
        X = random.Next(1, 101);  // X-koordinater för staden (1-100) - inuti ramarna
        Y = random.Next(1, 26);   // Y-koordinater för staden (1-25) - inuti ramarna
        SetRandomDirection();
        Inventory = new List<string>();
    }

    // Slumpmässig rörelseriktning
    public void SetRandomDirection()
    {
        int direction = random.Next(0, 6);
        switch (direction)
        {
            case 0: XDirection = 1; YDirection = 0; break;  // Höger
            case 1: XDirection = -1; YDirection = 0; break; // Vänster
            case 2: XDirection = 0; YDirection = 1; break;  // Ner
            case 3: XDirection = 0; YDirection = -1; break; // Upp
            case 4: XDirection = 1; YDirection = 1; break;  // Snett höger ner
            case 5: XDirection = -1; YDirection = -1; break;// Snett vänster upp
        }
    }

    // Flytta personen
    public virtual void Move()
    {
        X += XDirection;
        Y += YDirection;

        // Om personen lämnar staden, kommer tillbaka på andra sidan
        if (X < 1) X = 100;
        if (X > 100) X = 1;
        if (Y < 1) Y = 25;
        if (Y > 25) Y = 1;
    }

    public bool Meets(Person other)
    {
        return this.X == other.X && this.Y == other.Y;
    }
}

public class Citizen : Person
{
    public Citizen()
    {
        Symbol = "M";
        Inventory.Add("Nycklar");
        Inventory.Add("Mobiltelefon");
        Inventory.Add("Pengar");
        Inventory.Add("Klocka");
    }
}

public class Thief : Person
{
    public Thief()
    {
        Symbol = "T";
    }

    public void Steal(Citizen citizen)
    {
        if (citizen.Inventory.Count > 0)
        {
            string stolenItem = citizen.Inventory[random.Next(citizen.Inventory.Count)];
            citizen.Inventory.Remove(stolenItem);
            this.Inventory.Add(stolenItem);
            Console.SetCursorPosition(0, 28);
            Console.WriteLine("Tjuv rånar en medborgaren och tar " + stolenItem);
        }
    }
}

public class Police : Person
{
    public Police()
    {
        Symbol = "P";
    }

    public void Arrest(Thief thief)
    {
        this.Inventory.AddRange(thief.Inventory);
        thief.Inventory.Clear();
        Console.SetCursorPosition(0, 28);
        Console.WriteLine("Polisen griper tjuven och tar allt stöldgods.");
    }
}

public class City
{
    private List<Person> people = new List<Person>();
    private static Random random = new Random();
    private int robbedCitizens = 0;   // Variabel för att hålla reda på rånade medborgare
    private int arrestedThieves = 0;  // Variabel för att hålla reda på gripna tjuvar

    public City(int citizenCount, int thiefCount, int policeCount)
    {
        for (int i = 0; i < citizenCount; i++) people.Add(new Citizen());
        for (int i = 0; i < thiefCount; i++) people.Add(new Thief());
        for (int i = 0; i < policeCount; i++) people.Add(new Police());
    }

    // Ritar ramar runt spelplanen och statistiken
    private void DrawBorders()
    {
        Console.Clear();

        // Spelplanens ram
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("+" + new string('-', 102) + "+");
        for (int y = 1; y <= 25; y++)
        {
            Console.SetCursorPosition(0, y);
            Console.Write("|");
            Console.SetCursorPosition(102, y);
            Console.Write("|");
        }
        Console.SetCursorPosition(0, 26);
        Console.WriteLine("+" + new string('-', 102) + "+");

        // Statistikens ram
        Console.SetCursorPosition(0, 27);
        Console.WriteLine("+" + new string('-', 102) + "+");
    }

    // Visa staden i konsollen
    public void DrawCity()
    {
        // Rita ramarna först
        DrawBorders();

        // Skapa en spelplan med storlek 100x25 (inuti ramarna)
        string[,] cityMap = new string[100, 25];

        // Fyll enbart de positioner där det finns personer
        foreach (var person in people)
        {
            cityMap[person.X - 1, person.Y - 1] = person.Symbol;
        }

        // Rita kartan med färger
        for (int y = 0; y < 25; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                if (!string.IsNullOrEmpty(cityMap[x, y]))
                {
                    Console.SetCursorPosition(x + 1, y + 1);  // Placera markören på rätt plats inom ramarna

                    // Ställ in färg baserat på personens typ
                    switch (cityMap[x, y])
                    {
                        case "M":
                            Console.ForegroundColor = ConsoleColor.Green; // Medborgare
                            break;
                        case "T":
                            Console.ForegroundColor = ConsoleColor.Red;   // Tjuv
                            break;
                        case "P":
                            Console.ForegroundColor = ConsoleColor.Blue;  // Polis
                            break;
                    }

                    Console.Write(cityMap[x, y]);
                }
            }
        }

        // Återställ färg
        Console.ResetColor();

        // Uppdatera statistik under spelplanen
        Console.SetCursorPosition(0, 28);
        Console.WriteLine("+" + new string('-', 100) + "+");
        Console.SetCursorPosition(1, 29);
        Console.WriteLine($"| Antal medborgare som har blivit rånade: {robbedCitizens} |".PadRight(97) + "|");
        Console.SetCursorPosition(1, 30);
        Console.WriteLine($"| Antal tjuvar som har blivit gripna: {arrestedThieves} |".PadRight(97) + "|");
        Console.SetCursorPosition(0, 31);
        Console.WriteLine("+" + new string('-', 100) + "+");
    }

    // Simulera staden i angivna antal steg
    public void Simulate(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            DrawCity();

            foreach (var person in people) person.Move();

            // Kolla interaktioner
            for (int j = 0; j < people.Count; j++)
            {
                for (int k = j + 1; k < people.Count; k++)
                {
                    if (people[j].Meets(people[k]))
                    {
                        HandleInteraction(people[j], people[k]);
                    }
                }
            }

            Thread.Sleep(500); // Paus för att visa simulationens rörelser
        }

        // Efter att simulationen är klar, fortsätt visa statistik
        DrawCity();  // Rita sista versionen av staden och statistik
    }

    // Hantera interaktioner mellan personer
    private void HandleInteraction(Person p1, Person p2)
    {
        if (p1 is Police && p2 is Thief)
        {
            ((Police)p1).Arrest((Thief)p2);
            arrestedThieves++;
            Thread.Sleep(2000);  // Pausa i 2 sekunder
        }
        else if (p1 is Thief && p2 is Police)
        {
            ((Police)p2).Arrest((Thief)p1);
            arrestedThieves++;
            Thread.Sleep(2000);  // Pausa i 2 sekunder
        }
        else if (p1 is Thief && p2 is Citizen)
        {
            ((Thief)p1).Steal((Citizen)p2);
            robbedCitizens++;
            Thread.Sleep(2000);  // Pausa i 2 sekunder
        }
        else if (p1 is Citizen && p2 is Thief)
        {
            ((Thief)p2).Steal((Citizen)p1);
            robbedCitizens++;
            Thread.Sleep(2000);  // Pausa i 2 sekunder
        }
    }
}

public class Program
{
    public static void Main()
    {
        City city = new City(30, 20, 10);  // 30 medborgare, 20 tjuvar och 10 poliser
        city.Simulate(100);  // Simulera 100 steg
    }
}
