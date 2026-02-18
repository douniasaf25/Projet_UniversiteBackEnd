using System.Globalization;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NoteUseCases.Create;

namespace UniversiteDomain.UseCases.NoteUseCases.Import;

public class ImportNotesCsvUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
    public async Task ExecuteAsync(long idUe, List<ImportNoteRow> rows)
    {
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(rows);

        var errors = new List<string>();
        var toInsert = new List<(long idEtudiant, float valeur)>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int lineNumber = i + 2; // ligne 1 = header CSV

            // Note vide donc on ignore
            if (string.IsNullOrWhiteSpace(row.Note))
                continue;

            try
            {
                //  Vérifier étudiant
                var etudiants = await repositoryFactory.EtudiantRepository()
                    .FindByConditionAsync(e => e.NumEtud == row.NumEtud);

                if (etudiants.Count == 0)
                {
                    errors.Add($"Ligne {lineNumber}: Étudiant introuvable : {row.NumEtud}");
                    continue;
                }

                long idEtudiant = etudiants[0].Id;

                // Conversion note
                if (!float.TryParse(
                        row.Note.Replace(',', '.'),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float valeur))
                {
                    errors.Add($"Ligne {lineNumber}: note invalide : {row.Note}");
                    continue;
                }

                //  Vérification plage de note
                if (valeur < 0 || valeur > 20)
                {
                    errors.Add($"Ligne {lineNumber}: note hors plage (0..20) : {valeur}");
                    continue;
                }

                toInsert.Add((idEtudiant, valeur));
            }
            catch (Exception e)
            {
                errors.Add($"Ligne {lineNumber}: {e.Message}");
            }
        }

        // Si au moins une erreur donc rien n’est inséré
        if (errors.Count > 0)
            throw new Exception("Erreurs CSV:\n" + string.Join("\n", errors));

        //  Insertion uniquement si aucune erreur
        var addNoteUc = new AddNoteUseCase(repositoryFactory);

        foreach (var item in toInsert)
        {
            await addNoteUc.ExecuteAsync(item.idEtudiant, idUe, item.valeur);
        }
    }
}
