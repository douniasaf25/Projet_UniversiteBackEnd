using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.UeExceptions;
using UniversiteDomain.Util;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class AddNoteUseCase(IRepositoryFactory repositoryFactory)
{
    // AJOUT DE LA MÉTHODE IsAuthorized
    public bool IsAuthorized(string role)
    {
        // Seuls Responsable et Scolarité peuvent créer des notes
        return role.Equals(Roles.Responsable) || role.Equals(Roles.Scolarite);
    }
    public async Task<Note> ExecuteAsync(long idEtudiant, long idUe, float valeur)
    {
        await CheckBusinessRules(idEtudiant, idUe, valeur);
        return await repositoryFactory.NoteRepository().AddNoteAsync(idEtudiant, idUe, valeur);
    }

    private async Task CheckBusinessRules(long idEtudiant, long idUe, float valeur)
    {
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);

        // Vérifier les repositories
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        var ueRepo = repositoryFactory.UeRepository();
        var noteRepo = repositoryFactory.NoteRepository();

        ArgumentNullException.ThrowIfNull(etudiantRepo);
        ArgumentNullException.ThrowIfNull(ueRepo);
        ArgumentNullException.ThrowIfNull(noteRepo);

        //  Vérifier que l'étudiant existe + charger son parcours + ses UEs
        var etudiant = await etudiantRepo.FindEtudiantCompletAsync(idEtudiant);
        if (etudiant == null)
            throw new EtudiantNotFoundException($"Étudiant {idEtudiant} introuvable");

        //  Vérifier que l'UE existe
        var ue = await ueRepo.FindByConditionAsync(u => u.Id.Equals(idUe));
        if (ue is { Count: 0 })
            throw new UeNotFoundException($"UE {idUe} introuvable");

        // Vérifier que la note est entre 0 et 20
        if (valeur < 0 || valeur > 20)
            throw new InvalidNoteValueException($"La note {valeur} n’est pas valide (doit être comprise entre 0 et 20)");

        // Vérifier que l'étudiant suit bien cette UE dans son parcours
        var parcoursEtudiant = etudiant.ParcoursSuivi;
        if (parcoursEtudiant?.UesEnseignees?.Any(u => u.Id == idUe) != true)
            throw new InvalidNoteParcoursException(
                $"L’étudiant {idEtudiant} n’est pas inscrit dans le parcours contenant l’UE {idUe}"
            );
        // Vérifier qu’il n’a pas déjà une note dans cette UE
        var noteExistante = await noteRepo.FindByConditionAsync(n =>
            n.IdEtudiant == idEtudiant && n.IdUe == idUe);
        if (noteExistante is { Count: > 0 })
            throw new DuplicateNoteException($"L’étudiant {idEtudiant} a déjà une note dans l’UE {idUe}");
    }
}
