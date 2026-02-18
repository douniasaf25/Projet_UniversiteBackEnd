namespace UniversiteDomain.Dtos;

/// DTO utilisé pour lire le fichier CSV importé par la scolarité.
/// Cette classe représente une ligne du fichier uploadé.
/// Elle sert uniquement à la phase d'import (lecture du CSV).
public class ImportNoteRow
{
    // Informations sur l'UE (issues du fichier CSV)
    public string NumeroUe { get; set; } = "";
    public string IntituleUe { get; set; } = "";
    
    // Informations sur l'étudiant
    public string NumEtud { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    
    // Note saisie dans le fichier (peut être vide)
    public string Note { get; set; } = ""; 
}