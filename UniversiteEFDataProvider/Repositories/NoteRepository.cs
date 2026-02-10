using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context)
    : Repository<Note>(context), INoteRepository
{
    public async Task<Note> AddNoteAsync(long idEtudiant, long idUe, float valeur)
    {
        var note = new Note
        {
            IdEtudiant = idEtudiant,
            IdUe = idUe,
            Valeur = valeur
        };

        await Context.Notes.AddAsync(note);
        await Context.SaveChangesAsync();
        return note;
    }
}