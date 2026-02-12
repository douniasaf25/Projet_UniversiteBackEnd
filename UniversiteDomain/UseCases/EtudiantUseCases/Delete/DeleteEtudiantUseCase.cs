using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(factory);

        // supprime l'étudiant
        await factory.EtudiantRepository().DeleteAsync(idEtudiant);
        await factory.SaveChangesAsync();
    }

    public bool IsAuthorized(string role)
    {
        return Roles.Responsable.Equals(role) || Roles.Scolarite.Equals(role);
    }
}