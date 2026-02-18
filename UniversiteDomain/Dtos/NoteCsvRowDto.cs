namespace UniversiteDomain.Dtos;

/// DTO utilisé pour générer le fichier CSV modèle.
/// Cette classe représente une ligne du fichier téléchargé par la scolarité.
/// Elle est utilisée uniquement pour l'export.
public class NoteCsvRowDto
{ 
    // Informations sur l'UE
    public string NumeroUe { get; set; } = "";
    public string IntituleUe { get; set; } = "";
    
    // Informations sur l'étudiant

    public string NumEtud { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";

    // Note existante en base (ou vide si aucune note)
    public string? Note { get; set; }
}