namespace UniversiteDomain.Entities;

public class Note
{
    public long Id { get; set; }
    public float Valeur { get; set; }

    // ManyToOne : une note appartient à un étudiant
    public Etudiant? Etudiant { get; set; }

    // ManyToOne : une note appartient à une UE
    public Ue? Ue { get; set; }

    public override string ToString()
    {
        return $"Note {Valeur}/20 pour {Etudiant?.Nom} {Etudiant?.Prenom} dans {Ue?.Intitule}";
    }
}