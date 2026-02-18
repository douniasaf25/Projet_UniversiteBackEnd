using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class EtudiantRepository(UniversiteDbContext context) : Repository<Etudiant>(context), IEtudiantRepository
{
    public async Task AffecterParcoursAsync(long idEtudiant, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        e.ParcoursSuivi = p;
        await Context.SaveChangesAsync();
    }
    
    public async Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours)
    {
        await AffecterParcoursAsync(etudiant.Id, parcours.Id); 
    }
    public async Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant)
    {
        return await Context.Etudiants
            .Include(e => e.ParcoursSuivi)
            .ThenInclude(p => p.UesEnseignees)
            .Include(e => e.NotesObtenues)
            .ThenInclude(n => n.Ue)
            .FirstOrDefaultAsync(e => e.Id == idEtudiant);
    }
    
    public async Task<List<Etudiant>> FindEtudiantsByUeAsync(long idUe)
    {
        return await Context.Etudiants!
            .Where(e => e.ParcoursSuivi != null
                        && e.ParcoursSuivi.UesEnseignees!.Any(u => u.Id == idUe))
            .ToListAsync();
    }

}