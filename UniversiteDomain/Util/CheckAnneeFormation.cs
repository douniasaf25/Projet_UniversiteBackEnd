namespace UniversiteDomain.Util;

public class CheckAnneeFormation
{
    /// <summary>
    /// Vérifie si l'année de formation est valide (ex: 2024/2025)
    /// </summary>
    public static bool IsValidAnnee(int anneeFormation)
    {
        // Exemple simple : l’année doit être entre 2000 et 2100
        return anneeFormation >= 1 && anneeFormation <= 2100;
    }

    /// <summary>
    /// Variante si tu veux que ce soit une chaîne "2024/2025"
    /// </summary>
    public static bool IsValidAnnee(string anneeFormation)
    {
        // Exemple : format "2024/2025"
        return System.Text.RegularExpressions.Regex.IsMatch(anneeFormation, @"^\d{4}/\d{4}$");
    }
}
