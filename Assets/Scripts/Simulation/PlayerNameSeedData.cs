using System;

public static class PlayerNameSeedData
{
    public const string Russia = "Russia";
    public const string Belarus = "Belarus";
    public const string Canada = "Canada";
    public const string Usa = "USA";
    public const string Finland = "Finland";
    public const string Sweden = "Sweden";
    public const string Slovakia = "Slovakia";
    public const string Czechia = "Czechia";
    public const string Germany = "Germany";
    public const string Switzerland = "Switzerland";

    private static readonly string[] RussianFirstNames =
    {
        "Ivan", "Dmitry", "Alexander", "Sergey", "Mikhail", "Nikita", "Artem", "Maxim",
        "Kirill", "Ilya", "Yegor", "Pavel", "Roman", "Denis", "Vladislav", "Andrei",
        "Aleksey", "Matvey", "Danila", "Nikolay", "Ruslan", "Timur", "Oleg", "Gleb"
    };

    private static readonly string[] RussianLastNames =
    {
        "Sokolov", "Kuznetsov", "Morozov", "Volkov", "Orlov", "Fedorov", "Makarov", "Novikov",
        "Karpov", "Belyaev", "Zaitsev", "Gusev", "Antonov", "Tikhonov", "Borisov", "Egorov",
        "Kharlanov", "Savin", "Vorobyov", "Panarin", "Kiselev", "Larin", "Sorokin", "Danilov"
    };

    private static readonly string[] BelarusianFirstNames =
    {
        "Aleksei", "Viktor", "Daniil", "Yuri", "Nikolai", "Anton", "Matvei", "Arseny",
        "Vlad", "Timofei", "Stepan", "Gleb", "Ruslan", "Semyon", "Vadim", "Mark",
        "Kiryl", "Maksim", "Ihar", "Pavel", "Mikita", "Artyom", "Roman", "Andrei"
    };

    private static readonly string[] BelarusianLastNames =
    {
        "Kovalchuk", "Novitski", "Bondarenko", "Melnikov", "Savitski", "Kravchenko", "Lapko", "Yurchenko",
        "Zhuk", "Sidorov", "Klimovich", "Baranov", "Kozlov", "Pavlovich", "Fedotov", "Mironov",
        "Hrabouski", "Martsinovich", "Kazakevich", "Radkevich", "Yankovski", "Sokol", "Levitski", "Karpuk"
    };

    private static readonly string[] CanadianFirstNames =
    {
        "Connor", "Ryan", "Tyler", "Brayden", "Logan", "Carter", "Dylan", "Mason",
        "Owen", "Cole", "Nathan", "Ethan", "Caleb", "Liam", "Jake", "Wyatt",
        "Matthew", "Adam", "Jordan", "Brett", "Evan", "Nolan", "Spencer", "Hayden"
    };

    private static readonly string[] CanadianLastNames =
    {
        "McLeod", "Campbell", "Bennett", "Harrison", "Fraser", "Walsh", "Morrison", "Gauthier",
        "Lavoie", "Ducharme", "Roy", "Bouchard", "Anderson", "Thompson", "Cameron", "MacDonald",
        "Ouellet", "Tremblay", "Gallagher", "Mercier", "Bergeron", "Fournier", "Leblanc", "Gagnon"
    };

    private static readonly string[] UsaFirstNames =
    {
        "Jack", "Luke", "Cole", "Austin", "Trevor", "Chase", "Blake", "Noah",
        "Hunter", "Grant", "Evan", "Cooper", "Gavin", "Parker", "Brady", "Miles",
        "Logan", "Drew", "Camden", "Sawyer", "Wesley", "Hudson", "Riley", "Zach"
    };

    private static readonly string[] UsaLastNames =
    {
        "Miller", "Johnson", "Parker", "Hayes", "Nelson", "Brooks", "Carter", "Reed",
        "Foster", "Bishop", "Hughes", "Baker", "Collins", "Turner", "Morgan", "Price",
        "Winters", "Bennett", "Sullivan", "Hart", "Coleman", "Griffin", "Pierce", "Wright"
    };

    private static readonly string[] FinnishFirstNames =
    {
        "Mikko", "Antti", "Eetu", "Joonas", "Oskari", "Ville", "Aleksi", "Juho",
        "Sami", "Teemu", "Rasmus", "Lauri", "Kasper", "Matias", "Niko", "Patrik",
        "Aatu", "Jere", "Miro", "Topi", "Kalle", "Julius", "Eemil", "Otto"
    };

    private static readonly string[] FinnishLastNames =
    {
        "Korhonen", "Virtanen", "Laine", "Nieminen", "Salonen", "Hakala", "Lehtinen", "Koskinen",
        "Miettinen", "Heiskanen", "Aaltonen", "Rantala", "Jokinen", "Kapanen", "Vatanen", "Lindholm",
        "Pulkkinen", "Rissanen", "Leinonen", "Hartikainen", "Karjalainen", "Rautiainen", "Saarinen", "Kivela"
    };

    private static readonly string[] SwedishFirstNames =
    {
        "Erik", "Lucas", "Oscar", "William", "Filip", "Anton", "Axel", "Elias",
        "Viktor", "Linus", "Isak", "Noel", "Hugo", "Albin", "Nils", "Leo",
        "Emil", "Oskar", "Adam", "Jesper", "Rasmus", "Gustav", "Felix", "Arvid"
    };

    private static readonly string[] SwedishLastNames =
    {
        "Andersson", "Johansson", "Karlsson", "Nilsson", "Eriksson", "Larsson", "Olsson", "Persson",
        "Svensson", "Gustafsson", "Lindberg", "Lundqvist", "Ekholm", "Forsberg", "Sandin", "Berglund",
        "Backstrom", "Nylander", "Holmstrom", "Lindholm", "Dahlstrom", "Wallin", "Norberg", "Soderberg"
    };

    private static readonly string[] SlovakFirstNames =
    {
        "Martin", "Tomas", "Lukas", "Marek", "Adam", "Samuel", "Patrik", "Michal",
        "Jakub", "David", "Filip", "Peter"
    };

    private static readonly string[] SlovakLastNames =
    {
        "Novak", "Kovac", "Horvat", "Kollar", "Hudacek", "Tatar", "Nemec", "Cernak",
        "Halas", "Urban", "Kral", "Valach"
    };

    private static readonly string[] CzechFirstNames =
    {
        "Jan", "Petr", "Jakub", "Matej", "Ondrej", "David", "Tomas", "Radek",
        "Vojtech", "Milan", "Roman", "Dominik"
    };

    private static readonly string[] CzechLastNames =
    {
        "Novotny", "Dvorak", "Svoboda", "Prochazka", "Cerny", "Krejci", "Horak", "Vesely",
        "Kral", "Bartos", "Havel", "Simek"
    };

    private static readonly string[] GermanFirstNames =
    {
        "Leon", "Felix", "Jonas", "Lukas", "Maximilian", "Moritz", "Tim", "Nico",
        "Julian", "Florian", "Simon", "Tobias"
    };

    private static readonly string[] GermanLastNames =
    {
        "Muller", "Schmidt", "Weber", "Fischer", "Wagner", "Becker", "Hoffmann", "Schneider",
        "Keller", "Brandt", "Neumann", "Vogel"
    };

    private static readonly string[] SwissFirstNames =
    {
        "Luca", "Noah", "Nico", "Marco", "Dario", "Loris", "Joel", "Sven",
        "Kevin", "Fabian", "Reto", "Simon"
    };

    private static readonly string[] SwissLastNames =
    {
        "Meier", "Muller", "Schmid", "Keller", "Weber", "Baumann", "Frei", "Steiner",
        "Gerber", "Hofer", "Bucher", "Hess"
    };

    public static string PickNationality(Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 40) return Russia;
        if (roll < 60) return Belarus;
        if (roll < 70) return Canada;
        if (roll < 75) return Usa;
        if (roll < 82) return Finland;
        if (roll < 89) return Sweden;
        if (roll < 92) return Slovakia;
        if (roll < 95) return Czechia;
        if (roll < 98) return Germany;
        return Switzerland;
    }

    public static void PickName(string nationality, Random random, out string firstName, out string lastName)
    {
        string[] firstNames;
        string[] lastNames;
        GetNamePools(nationality, out firstNames, out lastNames);
        firstName = firstNames[random.Next(0, firstNames.Length)];
        lastName = lastNames[random.Next(0, lastNames.Length)];
    }

    private static void GetNamePools(string nationality, out string[] firstNames, out string[] lastNames)
    {
        switch (nationality)
        {
            case Belarus:
                firstNames = BelarusianFirstNames;
                lastNames = BelarusianLastNames;
                return;
            case Canada:
                firstNames = CanadianFirstNames;
                lastNames = CanadianLastNames;
                return;
            case Usa:
                firstNames = UsaFirstNames;
                lastNames = UsaLastNames;
                return;
            case Finland:
                firstNames = FinnishFirstNames;
                lastNames = FinnishLastNames;
                return;
            case Sweden:
                firstNames = SwedishFirstNames;
                lastNames = SwedishLastNames;
                return;
            case Slovakia:
                firstNames = SlovakFirstNames;
                lastNames = SlovakLastNames;
                return;
            case Czechia:
                firstNames = CzechFirstNames;
                lastNames = CzechLastNames;
                return;
            case Germany:
                firstNames = GermanFirstNames;
                lastNames = GermanLastNames;
                return;
            case Switzerland:
                firstNames = SwissFirstNames;
                lastNames = SwissLastNames;
                return;
            default:
                firstNames = RussianFirstNames;
                lastNames = RussianLastNames;
                return;
        }
    }
}
