using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.Csv;

public class ExportNotesCsvTemplateUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
    public async Task<List<NoteCsvRowDto>> ExecuteAsync(long idUe)
    {
        // Vérifier que l’UE existe
        var ue = (await repositoryFactory.UeRepository()
                .FindByConditionAsync(u => u.Id == idUe))
            .FirstOrDefault();

        if (ue == null)
            throw new Exception($"UE {idUe} introuvable");

        // Récupérer les étudiants qui suivent cette UE
        var etudiants = await repositoryFactory.EtudiantRepository()
            .FindEtudiantsByUeAsync(idUe);

        var rows = new List<NoteCsvRowDto>();

        foreach (var e in etudiants)
        {
            var note = await repositoryFactory.NoteRepository()
                .FindAsync(e.Id, idUe);

            rows.Add(new NoteCsvRowDto
            {
                NumeroUe = ue.NumeroUe,
                IntituleUe = ue.Intitule,
                NumEtud = e.NumEtud,
                Nom = e.Nom,
                Prenom = e.Prenom,
                Note = note == null ? "" : note.Valeur.ToString()
            });
        }

        return rows;
    }
}