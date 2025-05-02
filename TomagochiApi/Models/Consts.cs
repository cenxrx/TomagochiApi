namespace TomagochiApi.Models;

public static class PetActionValues
{
    public const int HappinessOnPlay = 15;
    public const int EnergyOnPlayCost = 10;

    public const int HungerOnFeed = 20;

    public const int HygieneOnClean = 30;

    public const int EnergyOnSleep = 30;

    public const int HappinessOnNoSleep = 20;

    public const double HoursToLoseHappinessFromNoSleep = 24;
    public const double HoursRequiredForSleep = 8;
}