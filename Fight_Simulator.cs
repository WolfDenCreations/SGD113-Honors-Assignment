namespace ConsoleApp2
{
    //I used VS Copilot for this and it coded it perfectly and has even given a choice to choose either random fighters or to choose your own.
    //It is pretty crazy how easily is created a workable code with no need to fix it anywhere.
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Simple Fight Simulator";
            var rng = new Random();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Simple Fight Simulator");
                Console.WriteLine("======================");
                Console.WriteLine("1) Choose fighters");
                Console.WriteLine("2) Random fighters");
                Console.WriteLine("3) Exit");
                Console.Write("Select an option: ");
                var choice = Console.ReadLine();

                if (choice == "3")
                    break;

                Fighter fighterA;
                Fighter fighterB;

                if (choice == "1")
                {
                    fighterA = PickFighter("Fighter A", rng);
                    fighterB = PickFighter("Fighter B", rng);
                }
                else
                {
                    fighterA = Fighter.Random("Fighter A", rng);
                    fighterB = Fighter.Random("Fighter B", rng);
                    Console.WriteLine();
                    Console.WriteLine("Random fighters generated:");
                    Console.WriteLine(fighterA);
                    Console.WriteLine(fighterB);
                }

                Console.WriteLine();
                Console.WriteLine("Press Enter to start the fight...");
                Console.ReadLine();

                var winner = FightSimulator.Run(fighterA, fighterB, rng);

                Console.WriteLine();
                Console.WriteLine($"Winner: {winner.Name} (HP: {winner.HP}/{winner.MaxHP})");
                Console.WriteLine();
                Console.Write("Fight again? (y/N): ");
                var again = Console.ReadLine();
                if (!string.Equals(again, "y", StringComparison.OrdinalIgnoreCase))
                    break;
            }

            Console.WriteLine("Goodbye.");
        }

        private static Fighter PickFighter(string prompt, Random rng)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"{prompt} - choose a class:");
                Console.WriteLine("1) Warrior (High HP, moderate attack/defense)");
                Console.WriteLine("2) Archer (Moderate HP, high speed/attack)");
                Console.WriteLine("3) Mage (Low HP, high attack, low defense)");
                Console.WriteLine("4) Random");
                Console.Write("Selection: ");
                var sel = Console.ReadLine();

                switch (sel)
                {
                    case "1":
                        return new Fighter("Warrior", 120, 18, 8, 6);
                    case "2":
                        return new Fighter("Archer", 90, 20, 5, 10);
                    case "3":
                        return new Fighter("Mage", 75, 26, 3, 8);
                    case "4":
                        return Fighter.Random(prompt, rng);
                    default:
                        Console.WriteLine("Invalid selection. Try again.");
                        continue;
                }
            }
        }
    }

    internal class Fighter
    {
        private static int s_instanceCounter = 1;

        public string Name { get; }
        public int MaxHP { get; }
        public int HP { get; private set; }
        public int Attack { get; }
        public int Defense { get; }
        public int Speed { get; }

        public Fighter(string baseName, int maxHP, int attack, int defense, int speed)
        {
            Name = $"{baseName} #{s_instanceCounter++}";
            MaxHP = maxHP;
            HP = maxHP;
            Attack = attack;
            Defense = defense;
            Speed = speed;
        }

        public static Fighter Random(string baseName, Random rng)
        {
            // Random archetype blended from ranges
            int hp = rng.Next(70, 131);       // 70-130
            int atk = rng.Next(10, 31);       // 10-30
            int def = rng.Next(3, 16);        // 3-15
            int spd = rng.Next(4, 13);        // 4-12
            return new Fighter(baseName, hp, atk, def, spd);
        }

        public bool IsAlive => HP > 0;

        public int PerformAttack(Fighter target, Random rng)
        {
            // Damage: attack +/- 10% random, minus a portion of target defense
            double variance = 0.9 + rng.NextDouble() * 0.2; // 0.9 - 1.1
            int rawDamage = (int)Math.Round(Attack * variance);
            int mitigated = Math.Max(0, rawDamage - (int)Math.Round(target.Defense * 0.5));
            // Ensure at least 1 damage when attacker has any attack
            int damage = Math.Max(1, mitigated);
            target.ReceiveDamage(damage);
            return damage;
        }

        public void ReceiveDamage(int amount)
        {
            HP = Math.Max(0, HP - amount);
        }

        public void Reset()
        {
            HP = MaxHP;
        }

        public override string ToString()
        {
            return $"{Name} (HP: {HP}/{MaxHP}, ATK: {Attack}, DEF: {Defense}, SPD: {Speed})";
        }
    }

    internal static class FightSimulator
    {
        public static Fighter Run(Fighter a, Fighter b, Random rng)
        {
            // Reset HPs in case fighters are reused
            a.Reset();
            b.Reset();

            // Decide initiative: higher speed goes first; tie-breaker is RNG
            Fighter first = a, second = b;
            if (b.Speed > a.Speed || (b.Speed == a.Speed && rng.Next(2) == 0))
            {
                first = b;
                second = a;
            }

            Console.WriteLine("Fight start!");
            Console.WriteLine(first);
            Console.WriteLine(second);
            Console.WriteLine();

            int round = 1;
            while (a.IsAlive && b.IsAlive)
            {
                Console.WriteLine($"-- Round {round} --");

                // First attacks
                var dmg1 = first.PerformAttack(second, rng);
                Console.WriteLine($"{first.Name} attacks {second.Name} for {dmg1} damage. ({second.HP}/{second.MaxHP})");
                if (!second.IsAlive)
                {
                    Console.WriteLine($"{second.Name} has fallen!");
                    break;
                }

                // Second attacks
                var dmg2 = second.PerformAttack(first, rng);
                Console.WriteLine($"{second.Name} attacks {first.Name} for {dmg2} damage. ({first.HP}/{first.MaxHP})");
                if (!first.IsAlive)
                {
                    Console.WriteLine($"{first.Name} has fallen!");
                    break;
                }

                round++;
                Console.WriteLine();
            }

            return a.IsAlive ? a : b;
        }
    }
}
